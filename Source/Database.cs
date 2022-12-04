using System.Data.Common;
using System.Data.SQLite;

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
                return String.Format("data source={0}", this.databasePath);
            }
        }

        public DbConnection GetDbConnection()
        {
            return new SQLiteConnection(sqliteDBConnectionString);
        }

        public bool isDatabaseInitialized()
        {
            return File.Exists(this.databasePath);
        }
    }
}
