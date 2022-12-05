using System.Data.Common;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace url_shortener
{
    public class DatabaseManager
    {
        private string databasePath;

        public DatabaseManager()
        {
            this.databasePath = Config.DataPath;
        }

        private string sqliteDBConnectionString
        {
            get
            {
                return string.Format("data source={0};Cache=Shared", this.databasePath);
            }
        }

        public async Task<DbConnection> GetDbConnection()
        {
            var conn = new SQLiteConnection(sqliteDBConnectionString);
            await conn.OpenAsync();
            return conn;
        }

        public Compiler GetDbQueryCompiler()
        {
            return new SqliteCompiler();
        }

        public QueryFactory GetDbQueryFactory(DbConnection conn, [Optional] Compiler compiler)
        {
            return new QueryFactory(conn, compiler ?? GetDbQueryCompiler());
        }

        public bool isDatabaseInitialized()
        {
            return File.Exists(this.databasePath);
        }
    }
}
