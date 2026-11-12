#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="${1:-$SCRIPT_DIR/cloud-run.env}"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Settings file not found: $ENV_FILE"
  echo "Copy deploy/cloud-run.env.example to deploy/cloud-run.env and fill in your own values."
  exit 1
fi

set -a
source "$ENV_FILE"
set +a

for name in PROJECT_ID REGION SERVICE_NAME REPOSITORY BUCKET_NAME SQL_INSTANCE SQL_TIER SQL_DATABASE SQL_USER SQL_PASSWORD ANTHROPIC_API_KEY VOYAGE_API_KEY; do
  if [[ -z "${!name:-}" || "${!name}" == your-* || "${!name}" == choose-* ]]; then
    echo "Please set $name in $ENV_FILE"
    exit 1
  fi
done

IMAGE="$REGION-docker.pkg.dev/$PROJECT_ID/$REPOSITORY/$SERVICE_NAME:latest"
SERVICE_ACCOUNT="$SERVICE_NAME-run@$PROJECT_ID.iam.gserviceaccount.com"
CONNECTION_NAME="$PROJECT_ID:$REGION:$SQL_INSTANCE"

step() {
  echo
  echo "==> $*"
}

save_secret() {
  if gcloud secrets describe "$1" >/dev/null 2>&1; then
    printf '%s' "$2" | gcloud secrets versions add "$1" --data-file=- >/dev/null
  else
    printf '%s' "$2" | gcloud secrets create "$1" --replication-policy automatic --data-file=- >/dev/null
  fi
  echo "    saved secret: $1"
}

step "Using project $PROJECT_ID in region $REGION"
gcloud config set project "$PROJECT_ID" >/dev/null

step "Enabling the Google Cloud APIs (Cloud Run, Cloud SQL, Secret Manager, Artifact Registry, Cloud Build, Cloud Storage)"
gcloud services enable run.googleapis.com sqladmin.googleapis.com secretmanager.googleapis.com \
  artifactregistry.googleapis.com cloudbuild.googleapis.com storage.googleapis.com iam.googleapis.com

step "Artifact Registry repository for the Docker image"
gcloud artifacts repositories describe "$REPOSITORY" --location "$REGION" >/dev/null 2>&1 \
  || gcloud artifacts repositories create "$REPOSITORY" --repository-format docker --location "$REGION"

step "Cloud Storage bucket for the uploaded PDFs: gs://$BUCKET_NAME"
gcloud storage buckets describe "gs://$BUCKET_NAME" >/dev/null 2>&1 \
  || gcloud storage buckets create "gs://$BUCKET_NAME" --location "$REGION" --uniform-bucket-level-access

step "Cloud SQL PostgreSQL: instance, database and user (the first time this takes 5-15 minutes)"
gcloud sql instances describe "$SQL_INSTANCE" >/dev/null 2>&1 \
  || gcloud sql instances create "$SQL_INSTANCE" --database-version POSTGRES_16 --edition enterprise \
       --tier "$SQL_TIER" --region "$REGION"
gcloud sql databases describe "$SQL_DATABASE" --instance "$SQL_INSTANCE" >/dev/null 2>&1 \
  || gcloud sql databases create "$SQL_DATABASE" --instance "$SQL_INSTANCE"
gcloud sql users create "$SQL_USER" --instance "$SQL_INSTANCE" --password "$SQL_PASSWORD" >/dev/null 2>&1 \
  || gcloud sql users set-password "$SQL_USER" --instance "$SQL_INSTANCE" --password "$SQL_PASSWORD"

step "Secret Manager: API keys and the database connection string"
save_secret anthropic-api-key "$ANTHROPIC_API_KEY"
save_secret voyage-api-key "$VOYAGE_API_KEY"
save_secret database-connection-string "Host=/cloudsql/$CONNECTION_NAME;Database=$SQL_DATABASE;Username=$SQL_USER;Password=$SQL_PASSWORD"

step "Service account for the app, with only the permissions it needs"
gcloud iam service-accounts describe "$SERVICE_ACCOUNT" >/dev/null 2>&1 \
  || gcloud iam service-accounts create "$SERVICE_NAME-run" --display-name "Labor Law Assistant on Cloud Run"
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member "serviceAccount:$SERVICE_ACCOUNT" --role roles/cloudsql.client --condition None >/dev/null
for secret in anthropic-api-key voyage-api-key database-connection-string; do
  gcloud secrets add-iam-policy-binding "$secret" \
    --member "serviceAccount:$SERVICE_ACCOUNT" --role roles/secretmanager.secretAccessor >/dev/null
done
gcloud storage buckets add-iam-policy-binding "gs://$BUCKET_NAME" \
  --member "serviceAccount:$SERVICE_ACCOUNT" --role roles/storage.objectAdmin >/dev/null

step "Building the Docker image with Cloud Build: $IMAGE"
gcloud builds submit "$ROOT_DIR" --tag "$IMAGE"

step "Deploying to Cloud Run"
gcloud run deploy "$SERVICE_NAME" \
  --image "$IMAGE" \
  --region "$REGION" \
  --service-account "$SERVICE_ACCOUNT" \
  --add-cloudsql-instances "$CONNECTION_NAME" \
  --set-env-vars "Database__Provider=PostgreSQL,Storage__Provider=GoogleCloudStorage,Storage__BucketName=$BUCKET_NAME" \
  --set-secrets "Anthropic__ApiKey=anthropic-api-key:latest,VoyageAI__ApiKey=voyage-api-key:latest,ConnectionStrings__PostgreSQL=database-connection-string:latest" \
  --memory 1Gi \
  --timeout 600 \
  --allow-unauthenticated

step "Done. Your app is live at:"
gcloud run services describe "$SERVICE_NAME" --region "$REGION" --format "value(status.url)"
