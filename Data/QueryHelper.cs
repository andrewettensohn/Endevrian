using Endevrian.Controllers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Data
{
    public class QueryHelper
    {
        
        private readonly SystemLogController _logger;
        private readonly string _connectionString;

        public QueryHelper(IConfiguration config, SystemLogController systemLogController)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = systemLogController;
            
        }

        public string SelectQuery(string query, string field)
        {

            string result = "No Results";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result = (reader[field].ToString());
                    }
                }
                catch(Exception exc)
                {
                    _logger.AddSystemLog($"Failed to read query: {exc}");
                }
                finally
                {

                    reader.Close();
                    connection.Dispose();

                }

            }

            return result;
        }

        public void UpdateQuery(string query)
        {

            //using SqlConnection connection = new SqlConnection(_connectionString);
            //SqlCommand command = new SqlCommand(query, connection);
            //connection.Open();
            //connection.Dispose();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }

            return;
        }
    }
}
