using AgapayAidSystem.Pages.Disaster;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgapayAidSystem.Pages
{
    public class IndexModel : PageModel
    {
		private readonly ILogger<IndexModel> _logger;
		private readonly IConfiguration _configuration;
		public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public DashboardInfo dashboardInfo { get; set; } = new DashboardInfo();
		public List<EvacuationCenterLogInfo> listCenterLog { get; set; } = new List<EvacuationCenterLogInfo>();
		public List<NotificationInfo> listNotification = new List<NotificationInfo>();
		public string errorMessage = "";
		public string successMessage = "";
        public string? UserId { get; set; }
        public string? UserType { get; set; }

        public void OnGet()
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					string notifSql = "SELECT esn.* " +
									  "FROM ec_staff_notification esn " +
									  "JOIN ec_staff_assignment esa ON esn.assignmentID = esa.assignmentID " +
									  "JOIN ec_staff ec ON ec.ecStaffID = esa.ecStaffID " +
									  "JOIN user u ON ec.userID = u.userID " +
									  "WHERE u.userID = @userID " +
									  "ORDER BY sentAt DESC";
					using (MySqlCommand notifCommand = new MySqlCommand(notifSql, connection))
					{
						notifCommand.Parameters.AddWithValue("@userID", UserId);
						using (MySqlDataReader notifReader = notifCommand.ExecuteReader())
						{
							while (notifReader.Read())
							{
								NotificationInfo notificationInfo = new NotificationInfo();
								notificationInfo.notificationID = notifReader.GetString(0);
								notificationInfo.assignmentID = notifReader.GetString(1);
								notificationInfo.centerLogID = notifReader.GetString(2);
								notificationInfo.message = notifReader.GetString(3);
								notificationInfo.sentAt = notifReader.GetDateTime(4).ToString("MMMM dd, yyyy");
								notificationInfo.readAt = notifReader.IsDBNull(5) ? null : notifReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt");
								notificationInfo.isRead = notifReader.GetString(6);
								listNotification.Add(notificationInfo);
							}
						}
					}

					// Fetch info of selected disaster from the database
					string sql = "SELECT * FROM disaster ORDER BY dateOccured DESC LIMIT 1;";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								disasterInfo.disasterID = reader.GetString(0);
								disasterInfo.disasterName = reader.GetString(1);
								disasterInfo.disasterType = reader.GetString(2);
								disasterInfo.description = reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("MMMM d, yyyy");
							}
						}
					}
					string disasterID = disasterInfo.disasterID;

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

                    // Fetch dashboard data related to the selected disaster
                    string dashboardSql = "CALL get_disaster_summary(@disasterID)";
                    using (MySqlCommand dashboardCommand = new MySqlCommand(dashboardSql, connection))
                    {
                        dashboardCommand.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader dashboardReader = dashboardCommand.ExecuteReader())
                        {
                            if (dashboardReader.Read())
                            {
                                dashboardInfo.totalDistinctEvacuees = dashboardReader.GetInt32(0).ToString();
                                dashboardInfo.totalCheckIn = dashboardReader.GetInt32(1).ToString();
                                dashboardInfo.totalCheckOut = dashboardReader.GetInt32(2).ToString();
                                dashboardInfo.totalCenter = dashboardReader.GetInt32(3).ToString();
                                dashboardInfo.totalOpenCenter = dashboardReader.GetInt32(4).ToString();
                                dashboardInfo.totalClosedCenter = dashboardReader.GetInt32(5).ToString();
                                dashboardInfo.totalDistinctBarangays = dashboardReader.GetInt32(6).ToString();
                                dashboardInfo.totalDistinctFamilies = dashboardReader.GetInt32(7).ToString();
                                dashboardInfo.totalDistinctFemale = dashboardReader.GetInt32(8).ToString();
                                dashboardInfo.totalDistinctMale = dashboardReader.GetInt32(9).ToString();
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

            string notificationID = Request.Form["notificationID"];
            string centerLogID = "";
            string isRead = "";

            if (string.IsNullOrEmpty(notificationID))
            {
                errorMessage = "Missing notificationID";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string notifSql = "SELECT centerLogID, isRead " +
                                      "FROM ec_staff_notification " +
                                      "WHERE notificationID = @notificationID";
                    using (MySqlCommand notifCommand = new MySqlCommand(notifSql, connection))
                    {
                        notifCommand.Parameters.AddWithValue("@notificationID", notificationID);
                        using (MySqlDataReader notifReader = notifCommand.ExecuteReader())
                        {
                            if (notifReader.Read())
                            {
                                centerLogID = notifReader.GetString(0);   
                                isRead = notifReader.GetString(1);   
                            }
                        }
                    }

                    if (isRead == "Sent")
                    {
                        // Update isRead in the 'ec_staff_notification' table
                        string sql = "UPDATE ec_staff_notification " +
                                     "SET isRead = 'Read', readAt = CURRENT_TIMESTAMP() " +
                                     "WHERE notificationID = @notificationID";

                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@notificationID", notificationID);
                            command.ExecuteNonQuery();
                        }
                    }   
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
                Response.Redirect("/index?errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Information Board page after successful update
                Response.Redirect("/disaster/profile/informationboard/index?centerLogID=" + centerLogID);
            }
        }
    }

    public class DashboardInfo
    {
        public string totalDistinctEvacuees { get; set; }
        public string totalCheckIn { get; set; }
        public string totalCheckOut { get; set; }
        public string totalCenter { get; set; }
        public string totalOpenCenter { get; set; }
        public string totalClosedCenter { get; set; }
        public string totalDistinctBarangays { get; set; }
        public string totalDistinctFamilies { get; set; }
        public string totalDistinctFemale { get; set; }
        public string totalDistinctMale { get; set; }
    }

	public class NotificationInfo
	{
		public string? notificationID { get; set; }
		public string? assignmentID { get; set; }
		public string? userID { get; set; }
		public string? centerLogID { get; set; }
		public string? message { get; set; }
		public string? sentAt { get; set; }
		public string? readAt { get; set; }
		public string? isRead { get; set; }
	}
}