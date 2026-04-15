
using MySql.Data.MySqlClient;

namespace CineMatch
{
    public static class DB
    {
        //Connection string
        private const string ConnStr =
            "Server=localhost;" +
            "Database=cinematch;" +
            "Uid=root;" +
            "Pwd=aryamasql@27;" +    
            "CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();          
            return conn;
        }
    }
}