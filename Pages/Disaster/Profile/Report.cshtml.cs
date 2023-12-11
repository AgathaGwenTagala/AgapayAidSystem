using AgapayAidSystem.Pages.disaster.profile.reliefgoodspack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class ReportModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ReportModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public List<EvacuationCenterLogInfo> listCenterLog { get; set; } = new List<EvacuationCenterLogInfo>();
        
        public string errorMessage = "";
        public string successMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

        public void OnGet()
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                Response.Redirect("/login/index");
                return;
            }

            string disasterID = Request.Query["disasterID"];

            if (string.IsNullOrEmpty(disasterID))
            {
                errorMessage = "Invalid disaster ID.";
            }

            try
            {
                string d_connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(d_connectionString))
                {
                    connection.Open();
                    // Fetch info of selected disaster from the database
                    string disasterSql = "SELECT * FROM disaster where disasterID = @disasterID";
                    using (MySqlCommand disasterCommand = new MySqlCommand(disasterSql, connection))
                    {
                        disasterCommand.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader disasterReader = disasterCommand.ExecuteReader())
                        {
                            if (disasterReader.Read())
                            {
                                disasterInfo.disasterID = disasterReader.GetString(0);
                                disasterInfo.disasterName = disasterReader.GetString(1);
                                disasterInfo.disasterType = disasterReader.GetString(2);
                                disasterInfo.description = disasterReader.GetString(3);
                                disasterInfo.dateOccured = disasterReader.GetDateTime("dateOccured").ToString("MMMM dd, yyyy");
                            }
                        }
                    }

                    // Fetch evacuation center log data related to the selected disaster
                    string logSql = "SELECT log.*, ec.centerName, ec.maxCapacity, b.barangayName, " +
                                    "(SELECT COUNT(assignmentID) FROM ec_staff_assignment " +
                                    "WHERE centerLogID = log.centerLogID AND status = 'Assigned') AS totalStaff " +
                                    "FROM evacuation_center_log AS log " +
                                    "INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
                                    "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
                                    "WHERE log.disasterID = @disasterID ORDER BY openingDateTime";
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
                                logInfo.status = logReader.GetString(6);
                                logInfo.centerName = logReader.GetString(7);
                                logInfo.maxCapacity = logReader.GetInt32(8).ToString();
                                logInfo.barangayName = logReader.GetString(9);
                                logInfo.totalStaff = logReader.GetInt32(10).ToString();
                                listCenterLog.Add(logInfo);
                            }
                        }
                    }

                    string connectionString = _configuration.GetConnectionString("DefaultConnection");
                    using (MySqlConnection inv_connection = new MySqlConnection(connectionString))
                    {
                        inv_connection.Open();
                        string sql = "SELECT * FROM all_inventory_view;";
                        using (MySqlCommand command = new MySqlCommand(sql, inv_connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    InventoryInfo inventoryInfo = new InventoryInfo();
                                    inventoryInfo.inventoryID = reader.GetString(0);
                                    inventoryInfo.centerLogID = reader.GetString(1);
                                    inventoryInfo.itemName = reader.GetString(2);
                                    inventoryInfo.itemType = reader.GetString(3);
                                    inventoryInfo.qty = reader.GetString(4);
                                    inventoryInfo.unitMeasure = reader.GetString(5);
                                    inventoryInfo.remarks = reader.IsDBNull(6) ? null : reader.GetString(6);
                                    inventoryInfo.disasterName = reader.GetString(7);
                                    inventoryInfo.centerName = reader.GetString(8);
                                    inventoryInfo.remainingQty = reader.GetInt32(9).ToString();
                                    listInventory.Add(inventoryInfo);
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
}
