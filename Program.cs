using System.Text.Json;
using url_shortener;
using url_shortener.Models;
using url_shortener.Repositories;
using url_shortener.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var databaseManager = new DatabaseManager();
var urlMappingRepository = new URLMappingRepository(databaseManager);
var urlAccessLogRepository = new URLAccessLogRepository(databaseManager);
await urlMappingRepository.initDb();

app.MapPost("/", async (URLMappingInput body) =>
{
    var result = await urlMappingRepository.Create(body);

    return result;
})
.WithName("Create");

app.MapGet("/", (context) =>
{
    context.Response.Headers.Add("Content-Type", "text/html");
    return context.Response.SendFileAsync(Path.Combine("Pages", "index.html"));
})
.WithName("IndexPage");

app.MapGet("/admin/list", (context) =>
{
    context.Response.Headers.Add("Content-Type", "text/html");
    return context.Response.SendFileAsync(Path.Combine("Pages", "list.html"));
})
.WithName("AdminListPage");

app.MapGet("/list", async () =>
{
    var result = await urlMappingRepository.List(getAccessLog: true);
    return JsonSerializer.Serialize(new { data = result });
}).WithName("List");

app.MapGet("/{code}", async (string code) =>
{
    var matched = await urlMappingRepository.Get(code);
    if (matched is null) return Results.NotFound(string.Format("Code {0} has no matched URL.", code));

    await urlAccessLogRepository.Create(new URLAccessLog(code: code));

    return Results.Redirect(matched.url);
}).WithName("RedirectWithCode");

app.Run();

