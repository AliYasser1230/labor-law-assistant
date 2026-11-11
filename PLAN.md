# Labor Law Assistant: Build Plan

An AI assistant that answers labor law questions (Arabic + English) from official law PDFs, using RAG, and shows the exact article and page as the source.

**MVP scope (today):** one country (🇸🇦 Saudi Labor Law, official English version) and one law PDF.

## Steps

| # | Step | Status |
|---|---|---|
| 0 | Setup: DBeaver ✅, SQLite ✅ (built into macOS), user-secrets ✅, first git commit ✅ · user still doing: Anthropic + Voyage AI API keys, Saudi Labor Law PDF, publish repo to GitHub | 🔄 |
| 1 | Project + `_Layout` (header, menu Ask · Documents · History, disclaimer footer, Arabic right-to-left) | ✅ |
| 2 | Database: EF Core + SQLite (one file, `laborlaw.db`, opened in DBeaver), tables `LawDocument`, `LawArticle`, `QuestionLog`, first migration | ✅ |
| 3 | Upload page: `DocumentsController`, `UploadDocumentViewModel` (model binding + validation: PDF only, < 20 MB) | ✅ |
| 4 | 🟦 RAG prep: read the PDF page by page (PdfPig), cut into articles, save with page numbers | ⬜ |
| 5 | 🟦 RAG prep: embeddings for every article (`IEmbeddingService` → Voyage AI), saved in SQLite | ⬜ |
| 6 | 🟦 R = Retrieval: `ISearchService` compares the question's embedding with every article's embedding (cosine similarity in C#), top 5, "not found" if nothing close | ⬜ |
| 7 | 🟦 A + G: `AskController` + `IAnswerService` → Claude (Anthropic C# SDK, citations on); `Answer.cshtml` shows answer + source **(MVP done 🎉)** | ⬜ |
| 8 | History page: `QuestionLog` (question, answer, time, cost) | ⬜ |
| 9 | README (problem, flowchart, stack, how to run, screenshots) + push to GitHub | ⬜ |

**Later (not today):** Egypt + UAE laws, evaluation set (30 questions), xUnit tests, Docker, deploy to GCP Cloud Run, LinkedIn demo video, move to a vector database (e.g. PostgreSQL + pgvector) only if the data grows to many thousands of articles.

## Flow (where RAG fits)

**Part A, prepare the laws:** upload PDF → read pages → cut into articles (country, law, article #, page) → embeddings → SQLite (`laborlaw.db`)

**Part B, answer a question:**
1. User asks on `Ask.cshtml` (+ chooses country)
2. `AskController`: model binding → `AskQuestionViewModel`; model validation (`ModelState.IsValid`)
3. 🟦 **R:** embed the question → `ISearchService` finds the 5 closest articles (same country)
4. Nothing close → "I couldn't find this in the law"
5. 🟦 **A:** build the Claude request: question + 5 articles + rules ("answer ONLY from these, cite article + page, reply in the user's language")
6. 🟦 **G:** Claude writes the answer with citations
7. Save `QuestionLog` → show `Answer.cshtml` (answer + 📄 source + ⚠️ "information, not legal advice")

## Tech
ASP.NET Core MVC (.NET 10), Razor views + layout view, model binding, model validation, dependency injection, EF Core + SQLite (viewed in DBeaver), cosine similarity search in C#, PdfPig, Voyage AI embeddings, Claude API (Anthropic C# SDK, model `claude-opus-5-5`, citations).
