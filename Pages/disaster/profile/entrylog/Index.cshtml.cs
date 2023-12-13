using AgapayAidSystem.Pages.disaster.profile.staffassignment;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.entrylog
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();
		public StaffAssignmentInfo assignmentInfo { get; set; } = new StaffAssignmentInfo();
		public List<EntryLogInfo> listEntryLog { get; set; } = new List<EntryLogInfo>();
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

            string centerLogID = Request.Query["centerLogID"];
            
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
							}
						}
					}

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

					// Fetch info of selected center log from the database
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName, log.status " +
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
								logInfo.status = logReader.GetString(4);
							}
                        }
                    }

					// Fetch entry log data related to the selected center log
					string entrySql = "SELECT * FROM entry_log_view " +
									  "WHERE centerLogID = @centerLogID ";
					using (MySqlCommand entryCommand = new MySqlCommand(entrySql, connection))
					{
						entryCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader entryReader = entryCommand.ExecuteReader())
						{
							while (entryReader.Read())
							{
								EntryLogInfo entryInfo = new EntryLogInfo();
								entryInfo.entryLogID = entryReader.GetString(0);
								entryInfo.memberID = entryReader.GetString(1);
								entryInfo.centerLogID = entryReader.GetString(2);
								entryInfo.checkInDate = entryReader.GetDateTime(3).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								entryInfo.checkOutDate = entryReader.IsDBNull(4) ? null : entryReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								entryInfo.entryStatus = entryReader.GetString(5);
								entryInfo.remarks = entryReader.IsDBNull(6) ? null : entryReader.GetString(6);
								entryInfo.fullName = entryReader.GetString(7);
								entryInfo.serialNum = entryReader.GetString(8);
								entryInfo.familyID = entryReader.GetString(9);
								listEntryLog.Add(entryInfo);
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

		public void OnPost(string[] selectedEvacuees)
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
			}

			if (selectedEvacuees == null || selectedEvacuees.Length == 0)
			{
				errorMessage = "Please select at least one evacuee to check-out.";
			}

			string centerLogID = Request.Form["centerLogID"];

			if (string.IsNullOrEmpty(centerLogID))
			{
				errorMessage = "Missing centerLogID";
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					foreach (string entryLogID in selectedEvacuees)
					{
						string insertSql = "UPDATE entry_log " +
										   "SET checkOutDate = CURRENT_TIMESTAMP(), entryStatus = 'Check-out' " +
										   "WHERE entryLogID = @entryLogID";

						using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
						{
							insertCommand.Parameters.AddWithValue("@entryLogID", entryLogID);

							int rowsInserted = insertCommand.ExecuteNonQuery();
							if (rowsInserted == 1)
							{
								successMessage = "Evacuee checked-out.";
							}
							else
							{
								errorMessage = "Failed to check-out one or more evacuees.";
							}
						}
					}

					// If all selected evacuees were successfully allocated
					successMessage = "Selected evacuees checked-out successfully!";
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			// Redirect to the Entry Log page after check-out or encountering an error
			Response.Redirect("/disaster/profile/entrylog/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}

	public class EntryLogInfo
	{
		public string entryLogID { get; set; }
		public string memberID { get; set; }
		public string centerLogID { get; set; }
		public string checkInDate { get; set; }
		public string checkOutDate { get; set; }
		public string entryStatus { get; set; }
		public string remarks { get; set; }
		public string fullName { get; set; }
		public string serialNum { get; set; }
		public string familyID { get; set; }
	}
}
