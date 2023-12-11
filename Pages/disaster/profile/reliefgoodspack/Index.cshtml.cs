using AgapayAidSystem.Pages.disaster.profile.staffassignment;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.reliefgoodspack
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();
		public StaffAssignmentInfo assignmentInfo { get; set; } = new StaffAssignmentInfo();
		public List<ReliefPackInfo> listReliefPack { get; set; } = new List<ReliefPackInfo>();
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

					// Fetch relief pack data related to the selected center log
					string reliefSql = "SELECT *, (packQty - distributedQty) AS remainingQty " +
									   "FROM pack " +									  
									   "WHERE centerLogID = @centerLogID " +
									   "ORDER BY createdAt;";
					using (MySqlCommand reliefCommand = new MySqlCommand(reliefSql, connection))
					{
						reliefCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader reliefReader = reliefCommand.ExecuteReader())
						{
							while (reliefReader.Read())
							{
								ReliefPackInfo reliefPackInfo = new ReliefPackInfo();
								reliefPackInfo.packID = reliefReader.GetString(0);
								reliefPackInfo.centerLogID = reliefReader.GetString(1);
								reliefPackInfo.packQty = reliefReader.GetInt32(2).ToString();
								reliefPackInfo.distributedQty = reliefReader.GetInt32(3).ToString();
								reliefPackInfo.status = reliefReader.GetString(4);
								reliefPackInfo.createdAt = reliefReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								reliefPackInfo.distributedAt = reliefReader.IsDBNull(6) ? null : reliefReader.GetDateTime(6).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								reliefPackInfo.remainingQty = reliefReader.GetInt32(7).ToString();
								listReliefPack.Add(reliefPackInfo);
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

	public class ReliefPackInfo
	{
		public string packID { get; set; }
		public string centerLogID { get; set; }
		public string packQty { get; set; }
		public string distributedQty { get; set; }
		public string remainingQty { get; set; }
		public string status { get; set; }
		public string createdAt { get; set; }
		public string distributedAt { get; set; }
	}

	public class EcLogNotification
	{
        public int remainingInventoryCount { get; set; }
        public int remainingPackCount { get; set; }
        public int remainingAssessmentCount { get; set; }
    }
}
