using System.Text.Json;
using url_shortener.Models;

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

var allUrlMapping = await URLMapping.loadURLMappingFromDisk();

app.MapPost("/", async (URLMapping inputData) =>
{
    var data = new URLMapping(inputData.url);
    allUrlMapping.Add(data);

    await URLMapping.saveURLMappingToDisk(allUrlMapping);
    return JsonSerializer.Serialize(data);
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

app.MapGet("/list", () =>
{
    return JsonSerializer.Serialize(new { data = allUrlMapping });
}).WithName("List");

app.MapGet("/{code}", (string code) =>
{
    var matched = allUrlMapping.Where(m => m.code == code).ToList();
    if (matched.Count == 0) return Results.NotFound(string.Format("Code {0} has no matched URL.", code));

    return Results.Redirect(matched[0].url);
}).WithName("RedirectWithCode");

app.Run();

