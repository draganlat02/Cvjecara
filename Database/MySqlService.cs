using MySql.Data.MySqlClient;

namespace PrviProjekat.Database
{
    public static class MySqlDb
    {
        private static string connString = "Server=localhost;Database=cvjecara;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(connString);
            conn.Open();
            return conn;
        }
    }
}
