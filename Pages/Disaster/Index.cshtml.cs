using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster
{
	public class IndexModel : PageModel
	{
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public List<DisasterInfo> listDisaster = new List<DisasterInfo>();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			try
			{
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM disaster ORDER BY dateOccured";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								DisasterInfo disasterInfo = new DisasterInfo();
								disasterInfo.disasterID = reader.GetString(0);
								disasterInfo.disasterName = reader.GetString(1);
								disasterInfo.disasterType = reader.GetString(2);
								disasterInfo.description = reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
								listDisaster.Add(disasterInfo);
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

	public class DisasterInfo
	{
		public string disasterID { get; set; }
		public string centerID { get; set; }
		public string centerLogID { get; set; }
		public string disasterName { get; set; }
		public string disasterType { get; set; }
		public string description { get; set; }
		public string dateOccured { get; set; }
	}

}