using Dapper;
using Microsoft.AspNetCore.Components.Forms;
using url_shortener.Models;

namespace url_shortener.Repositories
{

    public class URLAccessLogRepository
    {
        private DatabaseManager db;

        public URLAccessLogRepository(DatabaseManager db)
        {
            this.db = db;
        }

        public async Task<URLAccessLog> Create(URLAccessLog input)
        {
            if (string.IsNullOrWhiteSpace(input.code)) throw new Exception("Code cannot be empty!");
            DateTime accessAt = (input.accessed_at ?? DateTime.Now);

            using var conn = db.GetDbConnection();
            await conn.ExecuteAsync(
                @"INSERT INTO URLAccessLog (code, accessed_at) VALUES (@code, @accessed_at);",
                new { code = input.code, accessed_at = accessAt.ToString("yyyy-MM-dd HH:mm:ss") }
            );

            return new URLAccessLog(input.code, accessAt);
        }
    }
}
