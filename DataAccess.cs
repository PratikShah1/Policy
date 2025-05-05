using System.Data.OleDb;

namespace PD_Access
{
    public class DataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void GetData()
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                // Perform database operations here
            }
        }
    }
}
