using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.reliefgoodspack
{
    public class RecordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public RecordModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();
		public List<DistributionInfo> listDistribution { get; set; } = new List<DistributionInfo>();
        public string errorMessage = "";
        public string successMessage = "";
        public string packID = "";
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

            string centerLogID = Request.Query["centerLogID"];
            packID = Request.Query["packID"];
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

                    // Fetch entry log data related to the selected center log
                    string distributionSql = "SELECT * FROM distribution_record_view " +
                                             "WHERE packID = @packID ORDER BY receiveDate;";
                    using (MySqlCommand distributionCommand = new MySqlCommand(distributionSql, connection))
                    {
                        distributionCommand.Parameters.AddWithValue("@packID", packID);
                        using (MySqlDataReader distributionReader = distributionCommand.ExecuteReader())
                        {
                            while (distributionReader.Read())
                            {
                                DistributionInfo distributionInfo = new DistributionInfo();
                                distributionInfo.distributionID = distributionReader.GetString(0);
                                distributionInfo.packID = distributionReader.GetString(1);
                                distributionInfo.assignmentID = distributionReader.IsDBNull(2) ? null : distributionReader.GetString(2);
                                distributionInfo.entryLogID = distributionReader.GetString(3);
                                distributionInfo.qty = distributionReader.GetInt32(4).ToString();
                                distributionInfo.receiveDate = distributionReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                distributionInfo.staffFullName = distributionReader.IsDBNull(6) ? null : distributionReader.GetString(6);
                                distributionInfo.memberFullName = distributionReader.GetString(7);
                                distributionInfo.serialNum = distributionReader.GetString(8);
                                listDistribution.Add(distributionInfo);
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

    public class DistributionInfo
    {
        public string distributionID { get; set; }
        public string packID { get; set; }
        public string assignmentID { get; set; }
        public string entryLogID { get; set; }
        public string qty { get; set; }
        public string receiveDate { get; set; }
        public string staffFullName { get; set; }
        public string memberFullName { get; set; }
        public string serialNum { get; set; }
    }
}
