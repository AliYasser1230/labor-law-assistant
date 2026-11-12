# Running and deploying Labor Law Assistant

The app runs in two modes. You choose the mode with **settings** only; the code is the same.

| | Local mode (default) | Cloud mode (Google Cloud) |
|---|---|---|
| Database | SQLite file (`laborlaw.db`) | PostgreSQL on Cloud SQL |
| Uploaded PDFs | `App_Data/LawPdfs` folder | Google Cloud Storage bucket |
| API keys | .NET user-secrets | Google Secret Manager |
| Runs on | your computer (`dotnet run`) | Cloud Run (Docker container) |

Everyone uses **their own** API keys, Google Cloud project, database and bucket. Nothing in this repository belongs to a real account.

## 1. Settings

Every setting can be set in `appsettings.json`, with user-secrets, or with an **environment variable**. In environment variables, the `:` becomes `__` (two underscores).

| Setting | Environment variable | Default | What it does |
|---|---|---|---|
| `Database:Provider` | `Database__Provider` | `Sqlite` | `Sqlite` or `PostgreSQL` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Data Source=laborlaw.db` | The SQLite file |
| `ConnectionStrings:PostgreSQL` | `ConnectionStrings__PostgreSQL` | (empty) | The PostgreSQL connection string |
| `Database:MigrateOnStartup` | `Database__MigrateOnStartup` | `true` | Creates or updates the tables when the app starts |
| `Storage:Provider` | `Storage__Provider` | `Local` | `Local` or `GoogleCloudStorage` |
| `Storage:BucketName` | `Storage__BucketName` | (empty) | The Cloud Storage bucket for the PDFs |
| `Anthropic:ApiKey` | `Anthropic__ApiKey` | (none) | Your Anthropic key (answers) |
| `VoyageAI:ApiKey` | `VoyageAI__ApiKey` | (none) | Your Voyage AI key (search) |
| `Anthropic:Model` | `Anthropic__Model` | `claude-opus-5-5` | The Claude model |
| `VoyageAI:Model` | `VoyageAI__Model` | `voyage-4` | The embedding model |
| `Search:MinScore` | `Search__MinScore` | `0.4` | Below this score the app says "I couldn't find this in the law" |

Example PostgreSQL connection strings:

- On your computer: `Host=localhost;Database=laborlaw;Username=postgres;Password=your-password`
- On Cloud Run with Cloud SQL: `Host=/cloudsql/PROJECT_ID:REGION:INSTANCE;Database=laborlaw;Username=laborlaw_app;Password=your-password`

For Google Cloud Storage the app uses **Application Default Credentials**: on Cloud Run this is the service account of the service; on your computer run `gcloud auth application-default login`.

## 2. Run with Docker (on your computer)

You need [Docker](https://www.docker.com/). From the repository folder:

```bash
docker build -t labor-law-assistant .
docker run -p 8080:8080 \
  -e Anthropic__ApiKey="your-anthropic-key" \
  -e VoyageAI__ApiKey="your-voyage-key" \
  -v labor-law-data:/app/data \
  -v labor-law-pdfs:/app/App_Data \
  labor-law-assistant
```

Open **http://localhost:8080**. This uses local mode (SQLite and a folder), saved in two Docker volumes so the data stays when the container is removed. The health check is at `/health`.

## 3. Deploy to Google Cloud Run

### What you need

- A Google Cloud account and a project with **billing enabled**
- The [Google Cloud CLI](https://cloud.google.com/sdk/docs/install), logged in: `gcloud auth login`
- Your own Anthropic and Voyage AI API keys (see the README)

### Steps

1. Copy the settings template and fill in **your own** values:

   ```bash
   cp deploy/cloud-run.env.example deploy/cloud-run.env
   ```

   | Value | What to put |
   |---|---|
   | `PROJECT_ID` | Your Google Cloud project ID |
   | `REGION` | A region close to your users, e.g. `europe-west1` or `me-central1` |
   | `SERVICE_NAME`, `REPOSITORY` | Names for the Cloud Run service and the image repository (the defaults are fine) |
   | `BUCKET_NAME` | A bucket name that is unique in the whole world, e.g. `yourproject-law-pdfs` |
   | `SQL_INSTANCE`, `SQL_DATABASE`, `SQL_USER` | Names for the database (the defaults are fine) |
   | `SQL_TIER` | The database size; `db-f1-micro` is the smallest |
   | `SQL_PASSWORD` | A long random password for the database user |
   | `ANTHROPIC_API_KEY`, `VOYAGE_API_KEY` | Your own API keys |

   `deploy/cloud-run.env` is in `.gitignore`, so your values are never committed. The script stops if a value still looks like the example.

2. Run the script:

   ```bash
   ./deploy/deploy-cloud-run.sh
   ```

3. Open the address printed at the end (`https://...run.app`).

Run the script again after you change the code: it builds a new image and deploys it, and your data stays.

### What the script creates

| Google Cloud service | Used for |
|---|---|
| Artifact Registry | Stores the Docker image |
| Cloud Build | Builds the image from the `Dockerfile` (no Docker needed on your computer) |
| Cloud Storage | The bucket for the uploaded PDFs |
| Cloud SQL (PostgreSQL 16) | The database; the app creates the tables when it starts |
| Secret Manager | The two API keys and the database connection string |
| A service account | Gives the app only what it needs: connect to Cloud SQL, read the three secrets, read and write the bucket |
| Cloud Run | Runs the app, public on the internet |

On Cloud Run the app gets its settings like this: `Database__Provider=PostgreSQL`, `Storage__Provider=GoogleCloudStorage`, `Storage__BucketName=...` as environment variables, and the keys and the connection string from Secret Manager (also as environment variables). The app code does not change.

### Costs

- **Cloud SQL** is the main cost: it runs all the time, even with no visitors. The smallest tier costs roughly 10 US dollars per month; check Google's pricing for your region.
- Cloud Run, Cloud Storage, Artifact Registry and Secret Manager cost very little for a small app, and some have a free tier.
- Every answer also uses your Anthropic credit (about 3–5 US cents) and your Voyage AI tokens.
- Anyone with the link can upload laws and ask questions. If you share the link widely, watch your API usage.

### Delete everything (to stop the costs)

Use the same values as in `deploy/cloud-run.env`:

```bash
gcloud run services delete SERVICE_NAME --region REGION
gcloud sql instances delete SQL_INSTANCE
gcloud storage rm --recursive gs://BUCKET_NAME
gcloud secrets delete anthropic-api-key
gcloud secrets delete voyage-api-key
gcloud secrets delete database-connection-string
gcloud artifacts repositories delete REPOSITORY --location REGION
gcloud iam service-accounts delete SERVICE_NAME-run@PROJECT_ID.iam.gserviceaccount.com
```

### Troubleshooting

- See the app logs: `gcloud run services logs read SERVICE_NAME --region REGION`
- If a step fails with a permission error, the message names the missing role; give that role to the account it names and run the script again.
- If Cloud SQL creation fails because of the tier, choose a different `SQL_TIER` (see `gcloud sql tiers list`).

## 4. What has been tested

| Part | Tested |
|---|---|
| Local mode (SQLite, folder) | Yes |
| Production build (`Release`, `Production` environment, new empty database created at startup) | Yes |
| PostgreSQL 16 (temporary server, connected through a Unix socket like Cloud SQL): tables, upload, embeddings, search, answer, history | Yes |
| Security keys saved in the database (forms keep working after a restart) | Yes |
| Google Cloud Storage | No (needs a Google Cloud account) |
| Docker image build | No (Docker was not available on the development computer) |
| The deploy script against a real Google Cloud project | No (only its syntax and its settings check) |

## 5. For developers: database migrations

There are two sets of migrations, because SQLite and PostgreSQL need different SQL: `Migrations/` for SQLite (`AppDbContext`) and `Migrations/PostgreSql/` for PostgreSQL (`PostgresAppDbContext`). When you change a model, add a migration to **both** (inside `src/LaborLawAssistant`, with the `dotnet-ef` tool):

```bash
dotnet ef migrations add YourChange --context AppDbContext

Database__Provider=PostgreSQL \
ConnectionStrings__PostgreSQL="Host=localhost;Database=laborlaw;Username=postgres;Password=postgres" \
dotnet ef migrations add YourChange --context PostgresAppDbContext --output-dir Migrations/PostgreSql
```

The second command does not need a running PostgreSQL server; the connection string is only used to choose the PostgreSQL provider.
