using System.Text.Json;
using url_shortener.Models;
using url_shortener.Repositories;

namespace url_shortener.Routes
{
    public class URLMappingRoute
    {
        private DatabaseManager databaseManager;
        private URLMappingRepository urlMappingRepository;
        private URLAccessLogRepository urlAccessLogRepository;

        public URLMappingRoute(DatabaseManager databaseManager, URLMappingRepository urlMappingRepository, URLAccessLogRepository urlAccessLogRepository) {
            this.databaseManager = databaseManager;
            this.urlMappingRepository = urlMappingRepository;
            this.urlAccessLogRepository = urlAccessLogRepository;
        }    

        public RouteGroupBuilder Mount(RouteGroupBuilder group)
        {
            group.MapGet("/list", List).WithName(nameof(List));
            group.MapPost("/", Create).WithName(nameof(Create));
            group.MapGet("/{code}", RedirectWithCode).WithName(nameof(RedirectWithCode));
            group.MapGet("/", IndexPage).WithName(nameof(IndexPage));
            group.MapGet("/admin/list", AdminListPage).WithName(nameof(AdminListPage));
            return group;
        }

        public async Task<string> List()
        {
            using var conn = await databaseManager.GetDbConnection();
            var result = await urlMappingRepository.List(conn, getAccessLog: true);
            return JsonSerializer.Serialize(new { data = result });
        }

        public async Task<IResult> Create(URLMappingInput body)
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
        }

        public async Task<IResult> RedirectWithCode(string code)
        {
            using var conn = await databaseManager.GetDbConnection();
            var matched = await urlMappingRepository.Get(code, conn);
            if (matched is null) return Results.NotFound(string.Format("Code {0} has no matched URL.", code));

            await urlAccessLogRepository.Create(new URLAccessLog(code: code), conn);

            return Results.Redirect(matched.url);
        }

        public async Task<IResult> IndexPage()
        {
            return Results.File(Path.Combine(Environment.CurrentDirectory, "Pages", "index.html"), "text/html");
        }

        public async Task<IResult> AdminListPage()
        {
            return Results.File(Path.Combine(Environment.CurrentDirectory, "Pages", "list.html"), "text/html");
        }


    }
}
