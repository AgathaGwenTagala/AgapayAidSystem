using AgapayAidSystem.Pages.Disaster.Profile;
using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.informationboard
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public InformationBoard infoBoard { get; set; } = new InformationBoard();
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();
        public List<string> staffFullName = new List<string>();
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

            string disasterID = Request.Query["disasterID"];
			string centerLogID = Request.Query["centerLogID"];

            try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

                    // Fetch ec log notification count
                    string notifSql = "CALL get_eclog_notification(@centerLogID)";
                    using (MySqlCommand notifCommand = new MySqlCommand(notifSql, connection))
                    {
                        notifCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader notifReader = notifCommand.ExecuteReader())
                        {
                            if (notifReader.Read())
                            {
                                ecLogNotif.remainingInventoryCount = notifReader.GetInt32(0);
                                ecLogNotif.remainingPackCount = notifReader.GetInt32(1);
                                ecLogNotif.remainingAssessmentCount = notifReader.GetInt32(2);
                            }
                        }
                    }

                    // Fetch info of selected disaster from the database
                    string sql = "SELECT disasterID, disasterName FROM disaster where disasterID = @disasterID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@disasterID", disasterID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								disasterInfo.disasterID = reader.GetString(0);
								disasterInfo.disasterName = reader.GetString(1);
							}
						}
					}

					// Fetch info of selected center log from the database
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName, " +
                                    "ec.toilet, ec.bathingCubicle, ec.communityKitchen, ec.washingArea, " +
                                    "ec.womenChildSpace, ec.multipurposeArea, " +
                                    "CONCAT(ec.streetAddress, ', Brgy. ', b.barangayName, ', ', " +
                                    "b.municipalityCity, ', ', b.province) AS fullAddress, " +
                                    "ec.centerType, ec.mobileNum, ec.telephoneNum, " +
                                    "log.openingDateTime, log.closingDateTime, log.status " +
									"FROM evacuation_center_log AS log " +
									"INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
									"INNER JOIN disaster AS d ON log.disasterID = d.disasterID " +
                                    "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
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
                                logInfo.toilet = logReader.GetInt32(4).ToString();
                                logInfo.bathingCubicle = logReader.GetInt32(5).ToString();
                                logInfo.communityKitchen = logReader.GetInt32(6).ToString();
                                logInfo.washingArea = logReader.GetInt32(7).ToString();
                                logInfo.womenChildSpace = logReader.GetInt32(8).ToString();
                                logInfo.multipurposeArea = logReader.GetInt32(9).ToString();
                                logInfo.fullAddress = logReader.GetString(10);
                                logInfo.centerType = logReader.GetString(11);
                                logInfo.mobileNum = logReader.IsDBNull(12) ? null : logReader.GetString(12);
                                logInfo.telephoneNum = logReader.IsDBNull(13) ? null : logReader.GetString(13);
                                logInfo.openingDateTime = logReader.GetDateTime(14).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                logInfo.closingDateTime = logReader.IsDBNull(15) ? null : logReader.GetDateTime(15).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                logInfo.status = logReader.GetString(16);
                            }
                        }
					}

					// Fetch information board data related to the selected center log
					string infoSql = "CALL get_information_board(@centerLogID)";
					using (MySqlCommand infoCommand = new MySqlCommand(infoSql, connection))
					{
						infoCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader infoReader = infoCommand.ExecuteReader())
						{
							if (infoReader.Read())
							{
								infoBoard.assignedStaffCount = infoReader.GetInt32(0);
								infoBoard.assignedManager = infoReader.IsDBNull(1) ? null : infoReader.GetString(1);
								infoBoard.assignedAsstManager = infoReader.IsDBNull(2) ? null : infoReader.GetString(2);
								infoBoard.totalDistinctFamilies = infoReader.GetInt32(3);
								infoBoard.totalDistinctIndividuals = infoReader.GetInt32(4);
								infoBoard.totalDistinctMale = infoReader.GetInt32(5);
								infoBoard.totalDistinctFemale = infoReader.GetInt32(6);
								infoBoard.totalDistinctPregnantWoman = infoReader.GetInt32(7);
								infoBoard.totalDistinctChildren = infoReader.GetInt32(8);
								infoBoard.totalDistinctElderly = infoReader.GetInt32(9);
								infoBoard.totalDistinctPwd = infoReader.GetInt32(10);
								infoBoard.totalDistinctIP = infoReader.GetInt32(11);
							}
						}
					}

                    // Fetch list of assigned staff related to the selected center log
                    string staffSql = "SELECT staffFullName " +
                                      "FROM assigned_staff_view " +
                                      "WHERE role = 'Staff' AND centerLogID = @centerLogID;";
                    using (MySqlCommand staffCommand = new MySqlCommand(staffSql, connection))
                    {
                        staffCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader staffReader = staffCommand.ExecuteReader())
                        {
                            while (staffReader.Read())
                            {
                                staffFullName.Add(staffReader.GetString(0));
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

		public void OnPost()
		{
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			string centerLogID = Request.Form["centerLogID"];
			string disasterID = Request.Form["disasterID"];
			string centerName = Request.Form["centerName"];

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

					// Update status of evacuation center log
					string sql = "UPDATE evacuation_center_log " +
								 "SET closingDateTime = CURRENT_TIMESTAMP(), status = 'Closed' " +
								 "WHERE centerLogID = @centerLogID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						command.ExecuteNonQuery();
					}
                    
                    UpdateUserIDInTableLog1(connection, centerLogID, "evacuation_center_log", 2, ref errorOccurred);

                    // Retrieve the centerID related to the centerLogID
                    string? centerID;
                    string centerIDSql = "SELECT centerID FROM evacuation_center_log " +
                                         "WHERE centerLogID = @centerLogID";
                    using (MySqlCommand centerIDCommand = new MySqlCommand(centerIDSql, connection))
                    {
                        centerIDCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        centerID = centerIDCommand.ExecuteScalar()?.ToString();
                    }

                    // Set status of evacuation_center to 'Inactive'
                    string sql1 = "UPDATE evacuation_center SET status = 'Inactive' WHERE centerID = @centerID";
                    using (MySqlCommand command1 = new MySqlCommand(sql1, connection))
                    {
                        command1.Parameters.AddWithValue("@centerID", centerID);
                        command1.ExecuteNonQuery();
                    }

                    UpdateUserIDInTableLog1(connection, centerID, "evacuation_center", 1, ref errorOccurred);

                    // Set status of entry_log with status 'Check-in' to 'Check-out'
                    string sql2 = "UPDATE entry_log " +
								  "SET entryStatus = 'Check-out', checkOutDate = CURRENT_TIMESTAMP() " +
								  "WHERE centerLogID = @centerLogID AND entryStatus = 'Check-in'";
                    using (MySqlCommand command2 = new MySqlCommand(sql2, connection))
                    {
                        command2.Parameters.AddWithValue("@centerLogID", centerLogID);
                        int rowsAffected = command2.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            UpdateUserIDInTableLog2(connection, "entry_log", rowsAffected, ref errorOccurred);
                            UpdateUserIDInTableLog2(connection, "evacuation_center_log", rowsAffected, ref errorOccurred);
                        }
                    }

                    // Set status of ec_staff_assignment with status 'Assigned' to 'Completed'
                    string sql3 = "UPDATE ec_staff_assignment " +
                                  "SET status = 'Completed', completionDate = CURRENT_TIMESTAMP() " +
                                  "WHERE centerLogID = @centerLogID AND status = 'Assigned'";
                    using (MySqlCommand command3 = new MySqlCommand(sql3, connection))
                    {
                        command3.Parameters.AddWithValue("@centerLogID", centerLogID);
                        int rowsAffected = command3.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            UpdateUserIDInTableLog2(connection, "ec_staff_assignment", rowsAffected, ref errorOccurred);
                            UpdateUserIDInTableLog2(connection, "ec_staff", rowsAffected, ref errorOccurred);
                        }
                    }
                }

				successMessage = centerName + " closed successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/informationboard/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Disaster page after successful close
				Response.Redirect("/disaster/profile/index?disasterID=" + disasterID + "&successMessage=" + successMessage);
			}
		}

        private void UpdateUserIDInTableLog1(MySqlConnection connection, string tableID, string tableName, int limit, ref bool errorOccurred)
        {
            try
            {
                UserId = HttpContext.Session.GetString("UserId");
                if (tableName == "evacuation_center")
                {
                    string updateUserIdSql = "UPDATE table_log " +
                                         "SET userID = @userID, description = 'status: Active -> Inactive'" +
                                         "WHERE tableID = @tableID AND logType = 'Update'" +
                                         "ORDER BY loggedAt DESC " +
                                         "LIMIT @limit;";
                    using (MySqlCommand command = new MySqlCommand(updateUserIdSql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", UserId);
                        command.Parameters.AddWithValue("@tableID", tableID);
                        command.Parameters.AddWithValue("@limit", limit);
                        command.ExecuteNonQuery();
                    }
                }
                if (tableName == "evacuation_center_log")
                {
                    string updateUserIdSql = "UPDATE table_log " +
                                         "SET userID = @userID " +
                                         "WHERE tableID = @tableID AND logType = 'Update'" +
                                         "ORDER BY loggedAt DESC " +
                                         "LIMIT @limit;";
                    using (MySqlCommand command = new MySqlCommand(updateUserIdSql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", UserId);
                        command.Parameters.AddWithValue("@tableID", tableID);
                        command.Parameters.AddWithValue("@limit", limit);
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                errorOccurred = true;
                throw new Exception("Error in UpdateUserIDInTableLog1: ", ex);
            }
        }

        private void UpdateUserIDInTableLog2(MySqlConnection connection, string tableName, int limit, ref bool errorOccurred)
        {
            try
            {
                UserId = HttpContext.Session.GetString("UserId");
                string updateUserIdSql = "UPDATE table_log " +
                                         "SET userID = @userID " +
                                         "WHERE tableName = @tableName AND logType = 'Update' " +
                                         "ORDER BY loggedAt DESC " +
                                         "LIMIT @limit";
                using (MySqlCommand command = new MySqlCommand(updateUserIdSql, connection))
                {
                    command.Parameters.AddWithValue("@userID", UserId);
                    command.Parameters.AddWithValue("@tableName", tableName);
                    command.Parameters.AddWithValue("@limit", limit);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorOccurred = true;
                throw new Exception("Error in UpdateUserIDInTableLog2: ", ex);
            }
        }
    }

    public class InformationBoard
    {
		public int assignedStaffCount { get; set; }
		public string? assignedManager { get; set; }
		public string? assignedAsstManager { get; set; }
		public int totalDistinctFamilies { get; set; }
		public int totalDistinctIndividuals { get; set; }
		public int totalDistinctMale { get; set; }
		public int totalDistinctFemale { get; set; }
		public int totalDistinctPregnantWoman { get; set; }
		public int totalDistinctChildren { get; set; }
		public int totalDistinctElderly { get; set; }
		public int totalDistinctPwd { get; set; }
		public int totalDistinctIP { get; set; }
	}
}
