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

        static readonly string _connectionString;

        static QueryHelper()
        {
            _connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-Endevrian-5E06C235-D29D-4E9D-8A06-5EE32B599278;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public static string SelectQuery(string query, string field)
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
                    
                }
                finally
                {

                    reader.Close();
                    connection.Dispose();

                }

            }

            return result;
        }

        public static void UpdateQuery(string query)
        {

            using SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            connection.Dispose();

            return;
        }
    }
}
