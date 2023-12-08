using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile.reliefgoodspack
{
    public class ViewModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public ViewModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public List<PackInclusionInfo> listPackInclusion { get; set; } = new List<PackInclusionInfo>();
		public string errorMessage = "";
		public string successMessage = "";
        public string packID = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

        public void OnGet()
        {
			
			string centerLogID = Request.Query["centerLogID"];
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                Response.Redirect("/login/index");
                return;
            }
            
			packID = Request.Query["packID"];
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

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

                    // Fetch pack inclusion related to the selected relief goods pack
                    string sql = "select * from pack_inclusion_view WHERE packID = @packID;";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@packID", packID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								PackInclusionInfo inclusion = new PackInclusionInfo();
								inclusion.packInclusionID = reader.GetString(0);
								inclusion.inventoryID = reader.GetString(1);
								inclusion.packID = reader.GetString(2);
								inclusion.qty = reader.GetInt32(3).ToString();
								inclusion.totalQty = reader.GetInt32(4).ToString();
								inclusion.itemName = reader.GetString(5);
								inclusion.itemType = reader.GetString(6);
								inclusion.unitMeasure = reader.GetString(7);
								inclusion.disasterName = reader.GetString(8);
								inclusion.centerName = reader.GetString(9);
								listPackInclusion.Add(inclusion);
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

		public int GetRemainingInventoryCount()
		{
			string centerLogID = Request.Query["centerLogID"];
			string sql = "SELECT count(*) FROM inventory_item_view " +
						 "WHERE centerLogID = @centerLogID AND remainingQty > 0;";
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						return Convert.ToInt32(command.ExecuteScalar());
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return 0;
			}
		}

		public int GetRemainingPackCount()
		{
			string centerLogID = Request.Query["centerLogID"];
			string sql = "SELECT count(*) AS remainingPacks FROM pack " +
						 "WHERE centerLogID = @centerLogID AND status = 'Packed';";
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						return Convert.ToInt32(command.ExecuteScalar());
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return 0;
			}
		}

		public int GetRemainingAssessmentCount()
		{
			string centerLogID = Request.Query["centerLogID"];
			string sql = "SELECT count(*) AS remainingAssessment FROM distinct_family_head_view " +
						 "WHERE centerLogID = @centerLogID;";
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						return Convert.ToInt32(command.ExecuteScalar());
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return 0;
			}
		}
	}

	public class PackInclusionInfo
	{
		public string packInclusionID { get; set; }
		public string inventoryID { get; set; }
		public string packID { get; set; }
		public string qty { get; set; }
		public string totalQty { get; set; }
		public string itemName { get; set; }
		public string itemType { get; set; }
		public string unitMeasure { get; set; }
		public string disasterName { get; set; }
		public string centerName { get; set; }
	}
}
