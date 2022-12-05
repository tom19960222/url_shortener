using Dapper;
using SqlKata.Compilers;
using SqlKata.Execution;
using url_shortener.Models;

namespace url_shortener.Repositories
{
    public class URLMappingRepository
    {
        private DatabaseManager db;
        private Compiler compiler = new SqliteCompiler();

        public URLMappingRepository(DatabaseManager db)
        {
            this.db = db;
        }

        public async Task<URLMapping> Create (URLMappingInput input)
        {
            if (string.IsNullOrWhiteSpace(input.code)) throw new Exception("Code cannot be empty!");
            if (string.IsNullOrWhiteSpace(input.url)) throw new Exception("URL cannot be empty!");

            using var conn = db.GetDbConnection();
            var queryFactory = new QueryFactory(conn, compiler);
            var query = queryFactory.Query("URLMapping");
            await query.InsertAsync(new { code = input.code, url = input.url, created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            return new URLMapping(input.url, input.code, DateTime.Now);
        }

        public async Task<List<URLMapping>> List (bool getAccessLog = false)
        {
            using var conn = db.GetDbConnection();
            var queryFactory = new QueryFactory(conn, compiler);
            var query = queryFactory.Query("URLMapping");
            query.Select("URLMapping.code AS code")
                .Select("url")
                .Select("created_at")
                .OrderByDesc("URLMapping.created_at");

            if (getAccessLog)
            {
                query
                    .SelectRaw("COUNT(URLAccessLog.code) AS access_count")
                    .LeftJoin("URLAccessLog", "URLMapping.code", "URLAccessLog.code")
                    .GroupBy("URLMapping.code");
            }
            Console.WriteLine(compiler.Compile(query).Sql);
            List<URLMapping> result = (await query.GetAsync())
                .Select(u => new URLMapping(
                    url: (string)u.url,
                    code: (string)u.code, 
                    created_at: (DateTime)u.created_at, 
                    access_count: (int?)u.access_count
                ))
                .ToList();

            return result;
        }

        public async Task<URLMapping?> Get(string code, bool getAccessLog = false)
        {
            using var conn = db.GetDbConnection();
            var queryFactory = new QueryFactory(conn, compiler);
            var query = queryFactory.Query("URLMapping");
            query.Select("URLMapping.code AS code")
                .Select("url")
                .Select("created_at")
                .Where("URLMapping.code", "=", code)
                .OrderByDesc("URLMapping.created_at");

            if (getAccessLog)
            {
                query
                    .Select("COUNT(URLAccessLog.code) as access_count")
                    .LeftJoin("URLAccessLog", "URLMapping.code", "URLAccessLog.code")
                    .GroupBy("URLMapping.code");
            }

            List<URLMapping> result = (await query.GetAsync())
                .Select(u => new URLMapping(
                    url: (string)u.url,
                    code: (string)u.code,
                    created_at: (DateTime)u.created_at,
                    access_count: (int?)u.access_count
                ))
                .ToList();

            return result.Count > 0 ? result[0] : null;
        }
             
        public async Task initDb()
        {
            if (this.db.isDatabaseInitialized()) return;
            using var conn = db.GetDbConnection();
            await conn.ExecuteAsync(@"
                CREATE TABLE URLMapping (
                    code VARCHAR(255),
                    url TEXT,
                    created_at DATETIME,
                    CONSTRAINT URLMapping_PK PRIMARY KEY (code)
                );
            ");
            await conn.ExecuteAsync(@"
                CREATE TABLE URLAccessLog (
                    code VARCHAR(255),
                    accessed_at DATETIME,
                    FOREIGN KEY(code) REFERENCES URLMapping(code)
                );
            ");
        }
    }
}
