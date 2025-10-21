using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using API_FrontEnd.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using Polly.Extensions.Http;
using Polly.RateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddMemoryCache();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == (HttpStatusCode)429) // To handle TooManyRequests
    .WaitAndRetryAsync(3, attempt =>
    {
        var baseDelay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 200));
        return baseDelay + jitter;
    });

var breakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 8, durationOfBreak: TimeSpan.FromSeconds(15));

builder.Services.AddHttpClient<HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("API_FrontEnd", "1.0"));
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(breakerPolicy);

builder.Services.AddSingleton<BestStoriesService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "per-ip-fixed", options =>
    {
        options.PermitLimit = 30;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 50; // bursty callers get queued briefly
    });
});
builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseRateLimiter();
app.UseResponseCaching();

app.MapGet("/best-stories", () => Results.Redirect("/best-stories/10")); // top 10 by defaul from the endpoint /best-stories
app.MapGet("/", () => Results.Redirect("/best-stories/10")); // top 10 by defaul from the base endpoint 

app.MapGet("/best-stories/{n:int}", async (
    int n,
    BestStoriesService service,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    if (n <= 0) return Results.BadRequest("n must be > 0");
    if (n > 500) // keep it sane; HN list is usually ~500
        return Results.BadRequest("n must be <= 500");

    var results = await service.GetTopBestStoriesAsync(n, ct);
    // Cache our response for a short time to help our *clients* (not HN)
    httpContext.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromSeconds(15)
        };
    return Results.Ok(results);
})
.RequireRateLimiting("per-ip-fixed");

app.Run();
