using AgapayAidSystem.Pages.EvacuationCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class AllocateECModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AllocateECModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public List<EvacuationInfo> listEvacuation = new List<EvacuationInfo>();
        public List<string> SelectedEvacuationCenters { get; set; } = new List<string>();
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

            if (UserType == "EC Staff")
            {
                Response.Redirect("/accessdenied");
                return;
            }

            string disasterID = Request.Query["disasterID"];

            if (string.IsNullOrEmpty(disasterID))
            {
                errorMessage = "Invalid disaster ID.";
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch inactive evacuation centers
                    string sql = "SELECT ec.*, b.barangayName " +
                                 "FROM evacuation_center AS ec " +
                                 "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
                                 "WHERE status = 'Inactive'";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EvacuationInfo evacuationInfo = new EvacuationInfo();
                                evacuationInfo.centerID = reader.GetString(0);
                                evacuationInfo.centerName = reader.GetString(1);
                                evacuationInfo.centerType = reader.GetString(2);
                                evacuationInfo.streetAddress = reader.GetString(3);
                                evacuationInfo.barangayID = reader.GetString(4);
                                evacuationInfo.mobileNum = reader.IsDBNull(5) ? null : reader.GetString(5);
                                evacuationInfo.telephoneNum = reader.IsDBNull(6) ? null : reader.GetString(6);
                                evacuationInfo.maxCapacity = reader.GetInt32(7).ToString();
                                evacuationInfo.status = reader.GetString(8);
                                evacuationInfo.toilet = reader.GetInt32(9).ToString();
                                evacuationInfo.bathingCubicle = reader.GetInt32(10).ToString();
                                evacuationInfo.communityKitchen = reader.GetInt32(11).ToString();
                                evacuationInfo.washingArea = reader.GetInt32(12).ToString();
                                evacuationInfo.womenChildSpace = reader.GetInt32(13).ToString();
                                evacuationInfo.multipurposeArea = reader.GetInt32(14).ToString();
                                evacuationInfo.barangayName = reader.GetString(15);
                                listEvacuation.Add(evacuationInfo);
                            }
                        }
                    }

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
                }
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public void OnPost(string[] selectedEvacuationCenters)
        {
            bool errorOccurred = false;

            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                errorOccurred = true;
            }

            if (selectedEvacuationCenters == null || selectedEvacuationCenters.Length == 0)
            {
                errorMessage = "Please select at least one evacuation center to allocate.";
                errorOccurred = true;
            }

            string disasterID = Request.Form["disasterID"];

            if (string.IsNullOrEmpty(disasterID))
            {
                errorMessage = "Invalid disaster ID.";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (string centerID in selectedEvacuationCenters)
                    {
                        string insertSql = "INSERT INTO evacuation_center_log (disasterID, centerID) " +
                                           "VALUES (@disasterID, @centerID)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@disasterID", disasterID);
                            insertCommand.Parameters.AddWithValue("@centerID", centerID);

                            int rowsInserted = insertCommand.ExecuteNonQuery();
                            if (rowsInserted == 1)
                            {
                                successMessage = "Evacuation center allocated.";
                            }
                            else
                            {
                                errorMessage = "Failed to allocate one or more evacuation centers.";
                                errorOccurred = true; // Set the error flag
                                break;
                            }
                        }
                    }

                    // If all selected centers were successfully allocated
                    successMessage = "Evacuation centers allocated successfully!";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/disaster/profile/index?disasterID=" + disasterID + "&errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Entry Log page after successful allocation
                Response.Redirect("/disaster/profile/index?disasterID=" + disasterID + "&successMessage=" + successMessage);
            }
        }
    }
}
