# Labor Law Assistant

ASP.NET Core MVC (.NET 10) portfolio project: a RAG assistant that answers labor law questions (Arabic + English) from official law PDFs and cites the article + page. The full plan and step status live in `PLAN.md`; keep the status column up to date.

## Structure
- `LaborLawAssistant.slnx`: solution
- `src/LaborLawAssistant/`: the MVC app (Controllers, Models, Views, wwwroot)

## How to work with the user
- Use simple, clear English. The user's English is basic; explain new terms plainly.
- **After finishing any step or task, always show the full step list from `PLAN.md` with progress** (✅ done, 🔄 in progress, ⬜ not started) and say what's next.
- The user must be able to explain every line in interviews: briefly explain what each new file does and why.
- Required technologies (the user asked for these specifically): MVC, Razor views + layout view, model binding, model validation, dependency injection.

## Decisions
- MVP: Saudi Labor Law only (official English version); Egypt + UAE later.
- Database: SQLite, one file `laborlaw.db` (user decided: no Postgres). The user inspects it with DBeaver. The `.db` file is git-ignored.
- Vector search: embeddings are stored in SQLite; `ISearchService` computes cosine similarity in C# (about 250 articles per law, so this is fast). No pgvector.
- API keys live in user-secrets (`dotnet user-secrets`, already initialized): `Anthropic:ApiKey`, `VoyageAI:ApiKey`.
- Embeddings: Voyage AI (Claude has no embeddings API), called through a typed `HttpClient`.
- LLM: Claude API through the official Anthropic C# SDK, model `claude-opus-5-5`, citations enabled.
- PDF reading: PdfPig.
- Every answer shows the source (law, article, page) and the footer says "This is information, not legal advice."
- Never commit API keys; use user-secrets / environment variables.
