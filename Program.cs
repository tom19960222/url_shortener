using System.Text.Json;
using url_shortener;
using url_shortener.Models;
using url_shortener.Repositories;
using url_shortener.Routes;

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

var apiGroup = app.MapGroup("/");
(new URLMappingRoute(databaseManager, urlMappingRepository, urlAccessLogRepository)).Mount(apiGroup);

app.Run();

