using Endevrian.Controllers;
using Endevrian.Models;
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }

            return;
        }

        public Campaign ActiveCampaignQuery(string userId)
        {
            Campaign activeCampaign = new Campaign();

            string query = $"SELECT * FROM Campaigns WHERE IsSelectedCampaign = 1 AND UserId = '{userId}'";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            activeCampaign = new Campaign()
                            {
                                CampaignID = int.Parse(reader["CampaignID"].ToString()),
                                UserId = reader["UserId"].ToString(),
                                IsSelectedCampaign = bool.Parse(reader["IsSelectedCampaign"].ToString()),
                                CampaignName = reader["CampaignName"].ToString(),
                                CampaignDescription = reader["CampaignDescription"].ToString(),
                                CampaignCreateDate = DateTime.Parse(reader["CampaignCreateDate"].ToString())
                            };
                        }
                    }
                }
                catch (Exception exc)
                {
                    _logger.AddSystemLog($"Failed to read query: {exc}");
                }
            }
            return activeCampaign;
        }
    }
}
