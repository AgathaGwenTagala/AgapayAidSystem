using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster
{
	public class IndexModel : PageModel
	{
		public List<DisasterInfo> listDisaster = new List<DisasterInfo>();

		public void OnGet()
		{
			try
			{
				String connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					String sql = "SELECT * FROM disaster";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								DisasterInfo disasterInfo = new DisasterInfo();

								disasterInfo.disasterID = "" + reader.GetString(0);
								disasterInfo.disasterName = "" + reader.GetString(1);
								disasterInfo.disasterType = "" + reader.GetString(2);
								disasterInfo.description = "" + reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
								listDisaster.Add(disasterInfo);
							}
						}
					}
				}
			}

			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.ToString());
			}
		}
	}

	public class DisasterInfo
	{
		public string disasterID { get; set; }
		public string disasterName { get; set; }
		public string disasterType { get; set; }
		public string description { get; set; }
		public string dateOccured { get; set; }
	}

}