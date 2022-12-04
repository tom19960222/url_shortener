using Dapper;
using DapperQueryBuilder;
using url_shortener.Models;

namespace url_shortener.Repositories
{
    public class URLMappingRepository
    {
        private DatabaseManager db;

        public URLMappingRepository(DatabaseManager db)
        {
            this.db = db;
        }

        public async Task<URLMapping> Create (URLMappingInput input)
        {
            if (string.IsNullOrWhiteSpace(input.code)) throw new Exception("Code cannot be empty!");
            if (string.IsNullOrWhiteSpace(input.url)) throw new Exception("URL cannot be empty!");

            using var conn = db.GetDbConnection();
            await conn.ExecuteAsync(
                @"INSERT INTO URLMapping (code, url, created_at) VALUES (@code, @url, @created_at);",
                new { code = input.code, url = input.url, created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            );

            return new URLMapping(input.url, input.code, DateTime.Now);
        }

        public async Task<List<URLMapping>> List (bool getAccessLog = false)
        {
            using var conn = db.GetDbConnection();
            var sql = new FluentQueryBuilder(conn);
            sql
                .Select($"URLMapping.code AS code")
                .Select($"url")
                .Select($"created_at")
                .From($"URLMapping")
                .OrderBy($"URLMapping.created_at DESC"); 

            if (getAccessLog)
            {
                sql
                    .Select($"COUNT(URLAccessLog.code) as access_count")
                    .From($"LEFT JOIN URLAccessLog ON URLMapping.code = URLAccessLog.code")
                    .Where($"1 = 1")
                    .GroupBy($"URLMapping.code");
            }

            Console.WriteLine($"Sql: {sql.Sql}");
            List<URLMapping> result = (await sql.QueryAsync())
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
            var sql = new FluentQueryBuilder(conn);
            sql
                .Select($"URLMapping.code AS code")
                .Select($"url")
                .Select($"created_at")
                .From($"URLMapping")
                .Where($"URLMapping.code = {code}");

            if(getAccessLog)
            {
                sql
                    .Select($"COUNT(URLAccessLog.code) as access_count")
                    .From($"INNER JOIN URLAccessLog ON URLMapping.code = URLAccessLog.code")
                    .Where($"1 = 1")
                    .GroupBy($"URLMapping.code");
            }

            List<URLMapping> result = (await sql.QueryAsync())
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
