using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public List<EvacuationCenterLogInfo> listCenterLog { get; set; } = new List<EvacuationCenterLogInfo>();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string disasterID = Request.Query["disasterID"];

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch info of selected disaster from the database
                    string sql = "SELECT * FROM disaster where disasterID = @disasterID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                disasterInfo.disasterID = reader.GetString(0);
                                disasterInfo.disasterName = reader.GetString(1);
                                disasterInfo.disasterType = reader.GetString(2);
                                disasterInfo.description = reader.GetString(3);
                                disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("MMMM dd, yyyy");
                            }
                        }
                    }

                    // Fetch evacuation center log data related to the selected disaster
                    string logSql = "SELECT log.*, ec.centerName, ec.maxCapacity, b.barangayName " +
                                                    "FROM evacuation_center_log AS log " +
                                                    "INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
                                                    "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
                                                    "WHERE disasterID = @disasterID";
                    using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
                    {
                        logCommand.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader logReader = logCommand.ExecuteReader())
                        {
                            while (logReader.Read())
                            {
                                EvacuationCenterLogInfo logInfo = new EvacuationCenterLogInfo();
                                logInfo.centerLogID = logReader.GetString(0);
                                logInfo.disasterID = logReader.GetString(1);
                                logInfo.centerID = logReader.GetString(2);
                                logInfo.occupancy = logReader.GetInt32(3).ToString();
                                logInfo.openingDateTime = logReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                logInfo.closingDateTime = logReader.IsDBNull(5) ? null : logReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                logInfo.centerName = logReader.GetString(6);
                                logInfo.maxCapacity = logReader.GetInt32(7).ToString();
                                logInfo.barangayName = logReader.GetString(8);
                                listCenterLog.Add(logInfo);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
    }

    public class EvacuationCenterLogInfo
    {
        public string centerLogID { get; set; }
        public string disasterID { get; set; }
        public string centerID { get; set; }
        public string occupancy { get; set; }
        public string openingDateTime { get; set; }
        public string closingDateTime { get; set; }
        public string centerName { get; set; }
        public string maxCapacity { get; set; }
        public string barangayName { get; set; }
    }
}
