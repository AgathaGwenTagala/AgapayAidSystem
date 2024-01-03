using AgapayAidSystem.Pages.disaster.profile.staffassignment;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.entrylog
{
    public class CheckInModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public CheckInModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public StaffAssignmentInfo assignmentInfo { get; set; } = new StaffAssignmentInfo();
		public List<LatestEntryInfo> listLatestEntry = new List<LatestEntryInfo>();
		public string errorMessage = "";
		public string successMessage = "";
        public string? UserId { get; set; }
        public string? UserType { get; set; }

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

            if (UserType == "LGU Staff")
            {
                Response.Redirect("/accessdenied");
                return;
            }

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

					// Fetch assigned staff user information
					string assignedSql = "SELECT esa.* " +
										 "FROM ec_staff_assignment esa " +
										 "JOIN ec_staff ec ON esa.ecStaffID = ec.ecStaffID " +
										 "WHERE ec.userID = @userID AND esa.status = 'Assigned';";
					using (MySqlCommand assignedCommand = new MySqlCommand(assignedSql, connection))
					{
						assignedCommand.Parameters.AddWithValue("@userID", UserId);
						using (MySqlDataReader assignedReader = assignedCommand.ExecuteReader())
						{
							if (assignedReader.Read())
							{
								assignmentInfo.assignmentID = assignedReader.GetString(0);
								assignmentInfo.centerLogID = assignedReader.GetString(1);
								assignmentInfo.ecStaffID = assignedReader.GetString(2);
								assignmentInfo.role = assignedReader.GetString(3);
								assignmentInfo.assignmentDate = assignedReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.completionDate = assignedReader.IsDBNull(5) ? null : assignedReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.status = assignedReader.GetString(6);

								if (assignmentInfo.status != "Assigned")
								{
									Response.Redirect("/accessdenied");
									return;
								}

							}
						}
					}

					// Fetch family members who either have no records in the entry_log table
					// or have the latest entryStatus as 'Check-out'
					string sql = "SELECT * FROM member_latest_entry_log ORDER BY fullName";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								LatestEntryInfo latestEntryInfo = new LatestEntryInfo();
								latestEntryInfo.memberID = reader.GetString(0);
								latestEntryInfo.fullName = reader.GetString(1);
								latestEntryInfo.sex = reader.GetString(2);
								latestEntryInfo.birthdate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
								latestEntryInfo.barangayName = reader.GetString(4);
								latestEntryInfo.entryStatus = reader.IsDBNull(5) ? null : reader.GetString(5);
								latestEntryInfo.age = reader.GetInt64(6).ToString();
                                latestEntryInfo.serialNum = reader.GetString(7);
                                listLatestEntry.Add(latestEntryInfo);
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

        public void OnPost(string[] selectedEvacuees, string[] remarks)
        {
            bool errorOccurred = false;

            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                errorOccurred = true;
            }

            if (selectedEvacuees == null || selectedEvacuees.Length == 0)
            {
                errorMessage = "Please select at least one evacuee to check-in.";
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
                    for (int i = 0; i < selectedEvacuees.Length; i++)
                    {
                        string memberID = selectedEvacuees[i];
                        string selectedRemarks = remarks[i]; // Retrieve the remarks for the current evacuee

                        string insertSql = "INSERT INTO entry_log (memberID, centerLogID, remarks) " +
                                           "VALUES (@memberID, @centerLogID, @remarks)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@memberID", memberID);
                            insertCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                            insertCommand.Parameters.AddWithValue("@remarks", selectedRemarks);

                            int rowsInserted = insertCommand.ExecuteNonQuery();
                            if (rowsInserted == 1)
                            {
                                successMessage = "Evacuee checked-in.";
                            }
                            else
                            {
                                errorMessage = "Failed to check-in one or more evacuees.";
                                errorOccurred = true; // Set the error flag
                                break;
                            }
                        }

                        // Retrieve the last inserted entryLogID
                        string? lastInsertID;
                        string lastInsertSql = "SELECT MAX(entryLogID) FROM entry_log WHERE memberID = @memberID AND centerLogID = @centerLogID AND remarks = @remarks";
                        using (MySqlCommand lastInsertCommand = new MySqlCommand(lastInsertSql, connection))
                        {
                            lastInsertCommand.Parameters.AddWithValue("@memberID", memberID);
                            lastInsertCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                            lastInsertCommand.Parameters.AddWithValue("@remarks", selectedRemarks);
                            lastInsertID = lastInsertCommand.ExecuteScalar()?.ToString();
                        }

                        // Update userID in table_log
                        UserId = HttpContext.Session.GetString("UserId");
                        string updateUserIdSql = "UPDATE table_log SET userID = @userID WHERE tableID = @tableID AND logType = 'Add'";
                        using (MySqlCommand updateUserIdCommand = new MySqlCommand(updateUserIdSql, connection))
                        {
                            updateUserIdCommand.Parameters.AddWithValue("@userID", UserId);
                            updateUserIdCommand.Parameters.AddWithValue("@tableID", lastInsertID);
                            updateUserIdCommand.ExecuteNonQuery();
                        }
                    }

                    // If all selected centers were successfully allocated
                    successMessage = "Selected evacuees checked-in successfully!";
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
                Response.Redirect("/disaster/profile/entrylog/checkin?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Entry Log page after successful check-in
                Response.Redirect("/disaster/profile/entrylog/index?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
            }
        }
    }

	public class LatestEntryInfo
	{
		public string? memberID { get; set; }
		public string? fullName { get; set; }
		public string? sex { get; set; }
		public string? birthdate { get; set; }
		public string? barangayName { get; set; }
		public string? entryStatus { get; set; }
		public string? age { get; set; }
		public string? serialNum { get; set; }
	}
}
