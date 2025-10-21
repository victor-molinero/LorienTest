# Best Stories API (API_FrontEnd) — README


## Projects
- API_FrontEnd — Minimal RESTful API exposing /best-stories/{n}
- BestStoriesClient — Console app that queries the API and prints results

## Quick start

- Download or clone the repository.
- Open solution in your IDE (e.g., Visual Studio, VS Code).
- Build the solution to restore dependencies.

- To run the API:
  - Navigate to API_FrontEnd folder
  - Run `dotnet run`
- To run the console client (ensure that API_FrontEnd is running):
  - Navigate to BestStoriesClient folder
  - Run `dotnet run`

## Endpoints
- GET / — Redirects to /best-stories/10 by default
- GET /best-stories — Redirects to /best-stories/10
- GET /best-stories/{n:int} — Returns top n best stories (n > 0 and <= 500)

Example:
curl https://localhost:7298/best-stories/10

Response: JSON array of BestStoryDto:
- title, uri, postedBy, time (ISO), score, commentCount

## Implementation highlights
- HttpClient configured for Hacker News API with a 10s timeout and a user-agent.
- Polly policies:
  - Retry: 3 attempts with exponential backoff + jitter; treats transient errors and HTTP 429 as retriable.
  - Circuit breaker: trips after 8 handled failures for 15s.
- Caching:
  - Best story IDs cached for 30s.
  - Individual story objects cached for 5 minutes (in-memory).
  - API responses include short client-facing cache headers (15s).
- Concurrency and throttling:
  - Semaphore limits parallel fetches to 8 concurrent story requests.
  - ASP.NET Core __Rate Limiting__ configured with a fixed-window limiter named "per-ip-fixed" (30 permits / 10s, queue up to 50).
  - Response caching enabled via __ResponseCaching__.
- DTO mapping: Story -> BestStoryDto with ISO timestamp formatting.

