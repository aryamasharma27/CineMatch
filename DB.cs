// ============================================================
//  DB.cs  —  Database connection helper
//  This class holds one static method that gives us a fresh,
//  open MySqlConnection every time we need to talk to MySQL.
//
//  BACKEND EXPLANATION:
//  ADO.NET (ActiveX Data Objects .NET) is the .NET technology
//  for talking to databases.  The key objects are:
//    MySqlConnection  — the pipe between our app and MySQL
//    MySqlCommand     — a SQL query we want to run
//    MySqlDataReader  — reads results row by row (connected)
//    MySqlDataAdapter — fills a DataTable in one shot (disconnected)
// ============================================================
using MySql.Data.MySqlClient;

namespace CineMatch
{
    public static class DB
    {
        // ── Connection string ─────────────────────────────────
        // Change "root" and "yourpassword" to match your MySQL setup.
        private const string ConnStr =
            "Server=localhost;" +
            "Database=cinematch;" +
            "Uid=root;" +
            "Pwd=aryamasql@27;" +      // <-- CHANGE THIS
            "CharSet=utf8mb4;";

        // Returns an open connection.
        // We call .Open() here so every caller gets a ready connection.
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();          // Opens the TCP connection to MySQL
            return conn;
        }
    }
}