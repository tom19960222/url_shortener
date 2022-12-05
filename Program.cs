using System.Text.Json;
using url_shortener;
using url_shortener.Models;
using url_shortener.Repositories;

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

using (var conn = await databaseManager.GetDbConnection())
{
    await urlMappingRepository.initDb(conn);
}


app.MapPost("/", async (URLMappingInput body) =>
{
    using var conn = await databaseManager.GetDbConnection();
    using (var transaction = await conn.BeginTransactionAsync())
    {
        try
        {
            var result = await urlMappingRepository.Create(body, conn: conn, transaction: transaction);
            await transaction.CommitAsync();

            return Results.Created($"/{result.code}", result);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e.ToString());
            return Results.Problem(e.ToString());
        }
    }
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
    using var conn = await databaseManager.GetDbConnection();
    var result = await urlMappingRepository.List(conn, getAccessLog: true);
    return JsonSerializer.Serialize(new { data = result });
}).WithName("List");

app.MapGet("/{code}", async (string code) =>
{
    using var conn = await databaseManager.GetDbConnection();
    var matched = await urlMappingRepository.Get(code, conn);
    if (matched is null) return Results.NotFound(string.Format("Code {0} has no matched URL.", code));

    await urlAccessLogRepository.Create(new URLAccessLog(code: code), conn);

    return Results.Redirect(matched.url);
}).WithName("RedirectWithCode");

app.Run();

