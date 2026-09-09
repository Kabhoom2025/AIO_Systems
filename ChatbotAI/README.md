# Chatbot AI

A production-ready, ChatGPT-style AI chatbot: Angular 19 (standalone components) frontend,
.NET 9 Web API backend, PostgreSQL via EF Core, JWT auth, SignalR streaming, and a
provider-agnostic AI integration (OpenAI, Azure OpenAI, or xAI's Grok — swappable via config).
Voice input/output uses the browser's Web Speech API, with a dedicated full-duplex
"Voice Conversation" mode.

> **Angular version note:** the spec asked for Angular 20+. This environment's Node.js
> (22.12.0) is below the minimum the Angular 20 CLI enforces at runtime (22.22.3+), so the
> app is built on Angular 19.2 instead — the same Standalone Components APIs, control flow
> syntax (`@if`/`@for`), and signals used throughout are identical between the two versions.
> Bump Node and the `@angular/*`/`@angular/cli` versions in `client/package.json` to `^20` to
> move to Angular 20 with no further changes required.

---

## 1. Architecture

```
ChatbotAI/
 ├── Chatbot.Domain            # Entities, enums — no dependencies
 ├── Chatbot.Application       # Use-case services, DTOs, validators, IAiChatService abstraction
 ├── Chatbot.Infrastructure     # EF Core + Postgres, JWT/BCrypt, AI provider implementations
 ├── Chatbot.API                # Controllers, SignalR hub, middleware, composition root
 ├── Chatbot.Tests              # xUnit unit tests (Application + Infrastructure)
 ├── client/                    # Angular 19 standalone-components SPA
 ├── docker-compose.yml
 └── README.md (this file)
```

Clean Architecture dependency direction: `API → Infrastructure → Application → Domain`
(`API` and `Infrastructure` both depend on `Application`; `Application` never depends on
`Infrastructure` or `API`). `IAiChatService` is defined in `Application` and implemented in
`Infrastructure`, so the AI provider is swappable without touching business logic.

### Request/response flow (text chat)

```
Angular ChatComponent → SignalR ChatHub.SendMessage
  → ChatOrchestrationService.PrepareTurnAsync (create/load conversation, save user message,
     build bounded history)
  → IAiChatService.StreamResponseAsync (OpenAI/Grok/Azure OpenAI)
  → ChatHub streams "MessageChunk" events to the caller as tokens arrive
  → ChatOrchestrationService.CompleteAssistantMessageAsync (persist the final assistant message)
  → ChatHub sends "MessageCompleted"
```

A non-streaming `POST /api/chat/message` endpoint exists too, useful for testing via Swagger
or any client that doesn't use SignalR.

### Voice architecture — browser MVP, with a clean upgrade path

`VoiceRecognitionService` and `TextToSpeechService` wrap the browser's Web Speech API
(`SpeechRecognition` / `SpeechSynthesis`) behind small, provider-agnostic interfaces
(`startListening/stopListening`, `speak/pause/resume/stop`). Nothing else in the app talks to
`webkitSpeechRecognition` or `speechSynthesis` directly — `ChatComponent` and
`VoiceConversationComponent` only depend on those two services. That means the whole app can
move to a low-latency realtime speech API (e.g. a WebSocket/WebRTC-based streaming STT/TTS
service) later by swapping the *implementation* of those two services — the components, the
SignalR contract, and the backend do not change.

---

## 2. Prerequisites

- .NET SDK 9.0 (the repo was verified with the .NET 10 SDK, which can also build `net9.0`
  target frameworks)
- Node.js 22.x and npm
- PostgreSQL 17 (via Docker, or a local install)
- Docker + Docker Compose (optional, for containerized runs)
- An API key from your chosen AI provider (OpenAI, Azure OpenAI, or xAI/Grok)

---

## 3. Configure the AI provider

AI configuration lives under the `AI` section (`Chatbot.API/appsettings.json` /
environment variables / user-secrets). **Never put a real API key in `appsettings*.json` —
use user-secrets locally and environment variables/a secret manager in production.**

`IAiChatService` (`Chatbot.Application/Common/Interfaces/IAiChatService.cs`) is the
provider-agnostic abstraction. `Chatbot.Infrastructure/Ai` implements it three ways, all
sharing one base class (`OpenAiCompatibleChatServiceBase`) because OpenAI, Grok, and Azure
OpenAI all speak the same chat-completions wire format:

| `AI:Provider` | Class | Notes |
|---|---|---|
| `OpenAI` (default) | `OpenAiChatService` | `https://api.openai.com/v1` unless `AI:BaseUrl` overrides it |
| `Grok` | `GrokChatService` | xAI's OpenAI-compatible endpoint, `https://api.x.ai/v1` — keys look like `xai-...` |
| `Groq` | `GroqChatService` | Groq's (the fast-inference company) OpenAI-compatible endpoint, `https://api.groq.com/openai/v1` — keys look like `gsk_...`. **Not the same provider as "Grok"/xAI above** despite the similar name. |
| `AzureOpenAI` | `AzureOpenAiChatService` | requires `AI:AzureEndpoint` + `AI:AzureDeploymentName` |

Setting `AI:BaseUrl` on the `OpenAI` provider also makes any other OpenAI-compatible API
(Together AI, Groq, a local Ollama/vLLM server exposing `/v1/chat/completions`, etc.) work
with zero code changes.

### Local dev (user-secrets — recommended)

```bash
cd Chatbot.API
dotnet user-secrets init
dotnet user-secrets set "AI:Provider" "OpenAI"
dotnet user-secrets set "AI:ApiKey" "sk-..."
dotnet user-secrets set "AI:Model" "gpt-4o-mini"
dotnet user-secrets set "Jwt:Secret" "a-random-32+char-string-for-local-dev-only"
```

To use Grok instead:

```bash
dotnet user-secrets set "AI:Provider" "Grok"
dotnet user-secrets set "AI:ApiKey" "xai-..."
dotnet user-secrets set "AI:Model" "grok-2-latest"
```

To use Azure OpenAI instead:

```bash
dotnet user-secrets set "AI:Provider" "AzureOpenAI"
dotnet user-secrets set "AI:ApiKey" "<azure-key>"
dotnet user-secrets set "AI:AzureEndpoint" "https://<your-resource>.openai.azure.com"
dotnet user-secrets set "AI:AzureDeploymentName" "<your-deployment-name>"
```

### Environment variables (Docker / production)

ASP.NET Core maps `__` to nested config, so `AI:ApiKey` becomes `AI__ApiKey`:

```
Jwt__Secret=...
AI__Provider=OpenAI
AI__ApiKey=sk-...
AI__Model=gpt-4o-mini
```

These are exactly the variables `docker-compose.yml` reads from your shell/`.env` file for
`chatbot-api` (`JWT_SECRET`, `AI_PROVIDER`, `AI_API_KEY`, `AI_MODEL`, and optionally
`AI_BASE_URL` / `AI_AZURE_ENDPOINT` / `AI_AZURE_DEPLOYMENT_NAME`).

### Per-user "bring your own key" override (Settings page)

Beyond the server-wide default above, each signed-in user can set their **own** AI provider
and API key from **Settings → AI provider** — e.g. a user can enter their own Grok/xAI key
there even if the server default is OpenAI, and their conversations will use it instead.

- The key is encrypted at rest (ASP.NET Core Data Protection) in the `UserSettings` table's
  `AiApiKeyEncrypted` column and is **never** sent back to the client after saving — the API
  only ever returns `hasCustomAiApiKey: true/false`, never the key itself.
- Leaving the field blank on a later save leaves the stored key untouched; the trash-can
  icon (or clearing the field to empty and saving) removes it and reverts that user to the
  server default.
- Implementation: `IAiChatServiceFactory.Create(providerOverride, apiKeyOverride)`
  (`Chatbot.Infrastructure/Ai/AiChatServiceFactory.cs`) builds the right concrete provider
  class per call — this is the same `IAiChatService` abstraction from section 3, just
  instantiated with different `AiOptions` per request instead of once at startup.
  `ChatOrchestrationService.ResolveAiServiceAsync` looks up the calling user's settings and
  calls this factory before every chat turn (both the SignalR streaming path and the
  non-streaming `POST /api/chat/message` path).

---

## 4. Database setup & migrations

### Start Postgres (Docker)

```bash
cd ChatbotAI
docker compose up -d postgres
```

This starts a dedicated Postgres 17 container on host port **5433** (so it doesn't collide
with another Postgres on 5432), database `ChatbotAIDB`, user `postgres`, password `1234` —
already wired into `Chatbot.API/appsettings.Development.json`.

### Apply migrations

```bash
cd Chatbot.API
dotnet tool install --global dotnet-ef   # first time only
dotnet ef database update --project ../Chatbot.Infrastructure --startup-project .
```

### Creating a new migration after changing entities

```bash
dotnet ef migrations add <MigrationName> --project ../Chatbot.Infrastructure --startup-project . --output-dir Persistence/Migrations
dotnet ef database update --project ../Chatbot.Infrastructure --startup-project .
```

The API also calls `Database.MigrateAsync()` on startup, so a fresh environment self-initializes
as long as the connection string points at a reachable, empty Postgres instance.

---

## 5. Running the backend

```bash
cd Chatbot.API
dotnet run
```

- Swagger UI: http://localhost:5080/swagger (Development only)
- API base URL: http://localhost:5080/api
- SignalR hub: http://localhost:5080/hubs/chat

## 6. Running the frontend

```bash
cd client
npm install
npm start   # ng serve, http://localhost:4210
```

`src/environments/environment.ts` points at `http://localhost:5080` for local dev;
`environment.prod.ts` uses relative `/api` and `/hubs/chat` (for the Dockerized build, where
nginx reverse-proxies those paths to the API container).

---

## 7. Docker

```bash
cd ChatbotAI
cp .env.example .env   # if you create one — see below — or export the vars in your shell
docker compose up -d postgres        # Postgres only — fastest inner loop (dotnet run + npm start)
# or, full containerized stack:
JWT_SECRET=... AI_API_KEY=... docker compose up --build
```

Services: `postgres` (5433→5432), `chatbot-api` (5010→8080), `chatbot-client` (4210→80, nginx
serving the built Angular app and reverse-proxying `/api` and `/hubs` to `chatbot-api`).
`chatbot-api`'s Dockerfile builds all four backend projects; `chatbot-client`'s Dockerfile is a
two-stage Node build → nginx serve.

Required env vars for the full-stack compose run: `JWT_SECRET`, `AI_API_KEY` (compose refuses
to start without them). Optional: `AI_PROVIDER` (default `OpenAI`), `AI_MODEL`, `AI_BASE_URL`,
`AI_AZURE_ENDPOINT`, `AI_AZURE_DEPLOYMENT_NAME`.

---

## 8. API documentation

Swagger/OpenAPI is enabled in Development at `/swagger`, with JWT bearer auth wired into the
"Authorize" button (paste just the access token, no `Bearer ` prefix needed — Swashbuckle adds
it). Every controller endpoint is documented there with request/response schemas.

### Key endpoints

```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/auth/me

GET    /api/conversations
GET    /api/conversations/{id}
GET    /api/conversations/{id}/messages
POST   /api/conversations
PUT    /api/conversations/{id}
DELETE /api/conversations/{id}
DELETE /api/messages/{id}

POST   /api/chat/message        # non-streaming
GET    /api/settings
PUT    /api/settings
```

`/hubs/chat` (SignalR): client invokes `SendMessage(conversationId, message)`,
`RegenerateMessage(assistantMessageId)`, `StopGeneration()`; server pushes `MessageStarted`,
`MessageChunk`, `MessageCompleted`, `TypingStarted`, `TypingStopped`, `Error`.

---

## 9. Testing

### Backend unit tests

```bash
cd Chatbot.API   # any project in the solution works
dotnet test ../Chatbot.Tests/Chatbot.Tests.csproj
```

Covers `AuthService` (registration conflicts, login failure paths), FluentValidation rules,
`ConversationService`, the JWT token service, the BCrypt password hasher, and the OpenAI-wire
client (`GetResponseAsync`/`StreamResponseAsync` against a fake `HttpMessageHandler`, including
SSE chunk parsing and provider error mapping) — 23 tests, all passing.

### How to test text chat manually

1. `docker compose up -d postgres`, apply migrations, configure an AI provider (section 3).
2. Run the API (`dotnet run` in `Chatbot.API`) and the client (`npm start` in `client`).
3. Open http://localhost:4210, register an account, and send a message — the response streams
   in token-by-token via the SignalR hub.
4. Without a configured AI provider, sending a message still exercises the full pipeline
   (conversation creation, message persistence, SignalR round-trip) and surfaces a friendly
   "Unable to connect to the AI service" snackbar instead of a raw error — this was verified
   end-to-end with a headless-browser smoke test during development.

### How to test voice chat manually

Voice recognition and text-to-speech require a real Chromium/Edge/Safari browser (not
supported in a headless test runner) — see browser requirements below.

1. In the chat view, click the microphone icon, allow microphone access, and speak — the
   recognized text fills the input and sends automatically.
2. Click the speaker icon on any assistant message to hear it read aloud, or turn on
   "Automatically read AI responses aloud" in Settings.
3. Click "Start voice conversation" on the welcome screen for the full-screen voice mode:
   it listens, sends, waits for the AI, speaks the reply, and starts listening again
   automatically — tap the mic to interrupt speech and start talking immediately, or use
   Pause/Mute/End Conversation.

### How to test folder upload (attach files as chat context)

The folder icon in the chat input (`FolderUploadService`,
`client/src/app/core/services/folder-upload.service.ts`) lets you attach a local folder's
text files as one-shot context for your next message — nothing is uploaded to the server
until you actually send.

1. Click the folder icon next to the message box and pick a folder (Chrome/Edge: the native
   folder picker; requires `<input webkitdirectory>` support — Firefox does not support
   picking a folder this way, only individual files).
2. The app reads every file client-side, keeping only text-like ones (source code, markdown,
   config, etc. — see `TEXT_EXTENSIONS`/`TEXT_FILENAMES`), skipping `node_modules`/`.git`/
   `bin`/`obj`/etc., anything over 200 KB, and anything that decodes with a NUL byte (a
   binary masquerading as text). A snackbar reports how many files were attached and what
   was skipped.
3. An "N file(s) attached" bar appears above the input. Type a question (e.g. "what does
   notes.md say?") and send — the attached files are prepended to that one message as a
   markdown context block (capped at ~60,000 characters total so it stays within a
   reasonable AI context budget), then the attachment is cleared. Click "Clear" to discard
   attachments without sending.
4. Verified end-to-end with a headless-browser test: selecting a real folder with markdown,
   Python, TypeScript, and a fake PNG correctly attached the three text files, skipped the
   PNG, embedded their content ahead of the question in the outgoing SignalR message, and
   cleared the attachment bar once sent.

---

## 10. Browser requirements for voice features

- **Speech recognition** (`SpeechRecognition`/`webkitSpeechRecognition`): Chrome, Edge, and
  Safari (16.4+) support it; Firefox does not. The app detects this and shows "Voice
  recognition is not supported in this browser" instead of crashing — text chat still works
  fully.
- **Text-to-speech** (`speechSynthesis`): supported in all modern browsers, though the set of
  available voices differs by OS/browser (populate the "AI voice" list in Settings and use
  "Preview voice" to check what's available).
- Microphone access requires a secure context (`https://`, or `http://localhost`) — this is a
  browser platform requirement, not an app limitation.

---

## 11. Security notes

- Passwords hashed with BCrypt (work factor 12); JWT access tokens (default 60 min) + opaque,
  DB-tracked refresh tokens (default 7 days, single-use — rotated and revoked on each refresh).
- All conversation/message/settings endpoints scope every query to the authenticated user's
  `UserId` — there is no endpoint that can return another user's data by guessing an id.
- `GlobalExceptionMiddleware` maps every exception to a generic `{ success, message, errorCode }`
  body; stack traces, connection strings, and API keys are never sent to the client, only
  logged server-side (and never logged themselves — see `Program.cs`/`ChatHub.cs` for what gets
  logged: connection ids and generic messages, not tokens or message content).
- Rate limiting (60 req/min per user or IP) via `Microsoft.AspNetCore.RateLimiting`.
- CORS is locked to `Cors:AllowedOrigins` (no wildcard), with credentials allowed only for that
  origin — required for the SignalR cookie-less bearer-token flow.

---

## 12. Production deployment checklist

1. Set `Jwt__Secret`, `AI__Provider`, `AI__ApiKey` (and any Azure-specific vars) via your
   platform's secret manager / environment — never in a committed file.
2. Point `ConnectionStrings__DefaultConnection` at a managed Postgres instance.
3. Run `dotnet ef database update` (or let the API's startup `MigrateAsync()` do it) against
   that instance before first traffic.
4. Build and push the two Docker images (`Chatbot.API/Dockerfile`, `client/Dockerfile`), or
   `docker compose build` and push from this repo.
5. Put both behind HTTPS (a reverse proxy/load balancer terminating TLS) — SignalR needs
   WebSocket upgrades to pass through untouched.
6. Set `Cors:AllowedOrigins` to your real frontend origin(s).

---

## 13. Embedding on another website (e.g. a college site)

The chatbot can be embedded as a floating chat bubble on any third-party site — the same
technique Intercom/Drift use — via one `<script>` tag, with **no changes needed to the college
site's own code or CSS**, and **no CORS configuration needed either**.

### How it works

- `/widget` (`client/src/app/features/widget/widget.component.ts`) is a compact, self-contained
  build of the chat experience — inline login/register, one ongoing conversation, mic input,
  no sidebar — meant to run inside an `<iframe>` rather than as the full app.
- `public/embed/widget-loader.js` is a tiny, dependency-free vanilla-JS script. Pasted into any
  page, it draws a floating button; clicking it lazily creates an `<iframe src=".../widget">`.
  Because the iframe's `src` is *your* chatbot's own domain, every request it makes (login,
  chat, SignalR) is same-origin to your API — the college's site is never involved in that
  traffic at all, so `Cors:AllowedOrigins` doesn't need the college's domain added.

### Integration steps for the college's webmaster

Paste this once, anywhere on the page (typically just before `</body>`), on every page that
should show the chat bubble:

```html
<script
  src="https://YOUR-CHATBOT-DOMAIN/embed/widget-loader.js"
  data-app-url="https://YOUR-CHATBOT-DOMAIN"
  data-position="bottom-right"
  data-accent-color="#673ab7"
></script>
```

- `data-app-url` (required) — where you deployed this app (see section 12).
- `data-position` (optional) — `bottom-right` (default) or `bottom-left`.
- `data-accent-color` (optional) — hex color for the bubble button.

That's the entire integration — most CMSs (WordPress, Drupal, a custom site) have a "custom
HTML/footer scripts" block for exactly this.

### Requirements for this to work

- **HTTPS in production.** Both the college site and your deployed chatbot domain must be
  served over HTTPS — the microphone (`getUserMedia`) is blocked entirely in a cross-origin
  iframe over plain HTTP. The loader script sets `<iframe allow="microphone">` for you, but the
  browser still requires a secure context underneath it.
- The chatbot must actually be deployed somewhere public (not `localhost`) — see section 12.
- Students authenticate with the same email/password accounts as the main app (register once
  via the widget's own inline form, or via the main app — same backend, same users).

### Verified

Tested end-to-end with a mock third-party site on a different origin/port than the chatbot
app: floating bubble renders, opening it loads the iframe, inline login succeeds, a real
message round-trips to the AI provider and back, and the in-widget close button correctly
collapses the panel via `postMessage` back to the host page.
