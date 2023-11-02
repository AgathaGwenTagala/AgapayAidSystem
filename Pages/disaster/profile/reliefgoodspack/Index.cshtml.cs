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
		public List<ReliefPackInfo> listReliefPack { get; set; } = new List<ReliefPackInfo>();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			string centerLogID = Request.Query["centerLogID"];
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

					// Fetch relief pack data related to the selected center log
					string reliefSql = "SELECT *, (packQty - distributedQty) AS remainingQty " +
									   "FROM pack " +									  
									   "WHERE centerLogID = @centerLogID;";
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
		public string packInclusionID { get; set; }
		public string batchInclusionID { get; set; }
		public string qty { get; set; }
	}
}
