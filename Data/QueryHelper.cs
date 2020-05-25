using Endevrian.Controllers;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Endevrian.Models.TagModels;
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
        private ApplicationDbContext _context;

        public QueryHelper(IConfiguration config, SystemLogController systemLogController, ApplicationDbContext context)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = systemLogController;
            _context = context;
            
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

            //string query = $"SELECT * FROM Maps WHERE MapName LIKE '%{userSearchQuery}%' AND UserId = '{userId}' AND CampaignID = {selectedCampaignID}";
            string query = $@"
                SELECT *
                FROM Maps AS m
                JOIN TagRelations AS r ON r.MapID = m.MapID
                WHERE m.MapName LIKE '%{userSearchQuery}%'
                AND m.UserId = '{userId}'
                AND CampaignID = {selectedCampaignID}";

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

                    map.ActiveTags = _context.TagRelations.Where(x => x.MapID == map.MapID).ToList();
                    map.InactiveTags = GetInactiveTagsForMap(map);

                    foundMaps.Add(map);
                }
            }

            return foundMaps;

        }

        public List<Tag> GetInactiveTagsForMap(Map map)
        {
            List<Tag> InactiveTags = new List<Tag>();
            List<Tag> allTags = _context.Tags.Where(x => x.UserId == map.UserId).ToList();
            foreach (Tag tag in allTags)
            {
                List<TagRelation> matchingTags = map.ActiveTags.Where(x => x.TagID == tag.TagID).ToList();

                if (!matchingTags.Any())
                {
                    InactiveTags.Add(tag);
                }
            }

            return InactiveTags;
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
