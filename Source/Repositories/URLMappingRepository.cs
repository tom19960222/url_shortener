using Dapper;
using SqlKata.Execution;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
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

        public async Task<URLMapping> Create (URLMappingInput input, DbConnection conn, [Optional] IDbTransaction? transaction)
        {
            if (string.IsNullOrWhiteSpace(input.code)) throw new Exception("Code cannot be empty!");
            if (string.IsNullOrWhiteSpace(input.url)) throw new Exception("URL cannot be empty!");

            var queryFactory = db.GetDbQueryFactory(conn);
            var query = queryFactory.Query("URLMapping");
            await query.InsertAsync(new { code = input.code, url = input.url, created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, transaction);

            return new URLMapping(input.url, input.code, DateTime.Now);
        }

        public async Task<List<URLMapping>> List (DbConnection conn, [Optional] IDbTransaction? transaction, bool getAccessLog = false)
        {
            var queryFactory = db.GetDbQueryFactory(conn);
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

            List<URLMapping> result = (await query.GetAsync(transaction))
                .Select(u => new URLMapping(
                    url: (string)u.url,
                    code: (string)u.code, 
                    created_at: (DateTime)u.created_at, 
                    access_count: (int?)u.access_count
                ))
                .ToList();

            return result;
        }

        public async Task<URLMapping?> Get(string code, DbConnection conn, [Optional] IDbTransaction? transaction, bool getAccessLog = false)
        {
            var queryFactory = db.GetDbQueryFactory(conn);
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

            List<URLMapping> result = (await query.GetAsync(transaction))
                .Select(u => new URLMapping(
                    url: (string)u.url,
                    code: (string)u.code,
                    created_at: (DateTime)u.created_at,
                    access_count: (int?)u.access_count
                ))
                .ToList();

            return result.Count > 0 ? result[0] : null;
        }
             
        public async Task initDb(DbConnection conn)
        {
            if (this.db.isDatabaseInitialized()) return;
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
