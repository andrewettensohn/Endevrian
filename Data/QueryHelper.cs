using Endevrian.Controllers;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
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

        public List<Map> UserQueryMapGallery(string userId, string userSearchQuery)
        {
            List<Map> foundMaps = new List<Map>();

            int selectedCampaignID = ActiveCampaignQuery(userId).CampaignID;

            string query = $"SELECT * FROM Maps WHERE MapName LIKE '%{userSearchQuery}%' AND UserId = '{userId}' AND CampaignID = {selectedCampaignID}";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                DataTable MapResultDataValues = new DataTable();
                MapResultDataValues.Load(reader);

                foreach(DataRow row in MapResultDataValues.AsEnumerable())
                {
                    Map map = new Map
                    {
                        CampaignID = row.Field<int>("CampaignID"),
                        FileName = row.Field<string>("FileName"),
                        FilePath = row.Field<string>("FilePath"),
                        UserId = row.Field<string>("UserId"),
                        MapID = row.Field<int>("MapID"),
                        MapName = row.Field<string>("MapName"),
                        PreviewFileName = row.Field<string>("PreviewFileName"),
                        PreviewFilePath = row.Field<string>("PreviewFilePath"),
                    };

                    foundMaps.Add(map);
                }
            }

            return foundMaps;

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
