using AgapayAidSystem.Pages.disaster.profile.staffassignment;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.inventory
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();
        public StaffAssignmentInfo assignmentInfo { get; set; } = new StaffAssignmentInfo();
        public List<InventoryInfo> listInventory { get; set; } = new List<InventoryInfo>();
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

					// Fetch inventory data related to the selected center log
					string inventorySql = "SELECT * FROM inventory_item_view " +
										  "WHERE centerLogID = @centerLogID;";
					using (MySqlCommand inventoryCommand = new MySqlCommand(inventorySql, connection))
					{
						inventoryCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader inventoryReader = inventoryCommand.ExecuteReader())
						{
							while (inventoryReader.Read())
							{
								InventoryInfo inventoryInfo = new InventoryInfo();
								inventoryInfo.inventoryID = inventoryReader.GetString(0);
								inventoryInfo.centerLogID = inventoryReader.GetString(1);
								inventoryInfo.itemName = inventoryReader.GetString(2);
								inventoryInfo.itemType = inventoryReader.GetString(3);
								inventoryInfo.qty = inventoryReader.GetInt32(4).ToString();
								inventoryInfo.unitMeasure = inventoryReader.GetString(5);
								inventoryInfo.remarks = inventoryReader.IsDBNull(6) ? "-" : inventoryReader.GetString(6);
								inventoryInfo.packedQty = inventoryReader.GetInt32(7).ToString();
								inventoryInfo.remainingQty = inventoryReader.GetInt32(8).ToString();
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

	public class InventoryInfo
	{
		public string inventoryID { get; set; }
		public string centerLogID { get; set; }
		public string itemName { get; set; }
		public string itemType { get; set; }
		public string qty { get; set; }
		public string unitMeasure { get; set; }
		public string remarks { get; set; }
		public string packedQty { get; set; }
		public string remainingQty { get; set; }
	}
}
