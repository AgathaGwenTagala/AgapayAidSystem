using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.staffassignment
{
    public class AddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public List<AvailableStaffInfo> listAvailable = new List<AvailableStaffInfo>();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string centerLogID = Request.Query["centerLogID"];

            if (string.IsNullOrEmpty(centerLogID))
            {
                errorMessage = "Invalid centerLogID.";
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch info of available EC staff
                    string sql = "SELECT * FROM available_ec_staff";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AvailableStaffInfo availableInfo = new AvailableStaffInfo();
                                availableInfo.ecStaffID = reader.GetString(0);
                                availableInfo.fullName = reader.GetString(1);
                                availableInfo.sex = reader.GetString(2);
                                availableInfo.birthdate = reader.GetDateTime(3).ToString("MMMM d, yyyy");
                                availableInfo.mobileNum = reader.GetString(4);
                                availableInfo.emailAddress = reader.GetString(5);
                                listAvailable.Add(availableInfo);
                            }
                        }
                    }

                    // Fetch info of selected center log from the database
                    string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName " +
                                    "FROM evacuation_center_log AS log " +
                                    "INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
                                    "INNER JOIN disaster AS d ON log.disasterID = d.disasterID " +
                                    "WHERE log.centerLogID = @centerLogID";
                    using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
                    {
                        logCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader logReader = logCommand.ExecuteReader())
                        {
                            if (logReader.Read())
                            {
                                logInfo.centerLogID = logReader.GetString(0);
                                logInfo.disasterID = logReader.GetString(1);
                                logInfo.disasterName = logReader.GetString(2);
                                logInfo.centerName = logReader.GetString(3);
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

        public void OnPost(string[] selectedStaff, string[] role)
        {
            bool errorOccurred = false;

            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                errorOccurred = true;
            }

            if (selectedStaff == null || selectedStaff.Length == 0)
            {
                errorMessage = "Please select at least one evacuee to check-out.";
                errorOccurred = true;
            }
            
            string centerLogID = Request.Form["centerLogID"];

            if (string.IsNullOrEmpty(centerLogID))
            {
                errorMessage = "Missing centerLogID";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    for (int i = 0; i < selectedStaff.Length; i++)
                    {
                        string ecStaffID = selectedStaff[i];
                        string selectedRole = role[i]; // Retrieve the role for the current staff member

                        string insertSql = "INSERT INTO ec_staff_assignment (ecStaffID, centerLogID, role) " +
                                           "VALUES (@memberID, @centerLogID, @role)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@memberID", ecStaffID);
                            insertCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                            insertCommand.Parameters.AddWithValue("@role", selectedRole); // Use the selected role

                            int rowsInserted = insertCommand.ExecuteNonQuery();
                            if (rowsInserted == 1)
                            {
                                successMessage = "Staff assigned.";
                            }
                            else
                            {
                                errorMessage = "Failed to assign one or more staff members.";
                                errorOccurred = true;
                                break;
                            }
                        }
                    }

                    // If all selected staff members were successfully assigned
                    successMessage = "Selected staff members assigned successfully!";
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
                Response.Redirect("/disaster/profile/staffassignment/add?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Staff Assignment page after successful assignment
                Response.Redirect("/disaster/profile/staffassignment/index?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
            }
        }

    }

    public class AvailableStaffInfo
    {
        public string ecStaffID { get; set; }
        public string fullName { get; set; }
        public string sex { get; set; }
        public string birthdate { get; set; }
        public string mobileNum { get; set; }
        public string emailAddress { get; set; }
    }
}
