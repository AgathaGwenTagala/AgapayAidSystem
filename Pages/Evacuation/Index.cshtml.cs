using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Evacuation
{
    public class IndexModel : PageModel
    {
		public List<EvacuationInfo> listEvacuation = new List<EvacuationInfo>();
		public void OnGet()
        {
            try
			{
				String connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					String sql = "SELECT * FROM evacuation_center_log";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								EvacuationInfo evacuationInfo = new EvacuationInfo();

								evacuationInfo.evacuationID = "" + reader.GetString(0);
								evacuationInfo.evacuationName = "" + reader.GetString(1);
								evacuationInfo.barangay = "" + reader.GetString(2);
								evacuationInfo.capacity = "" + reader.GetString(3);
								evacuationInfo.status = "" + reader.GetString(4);
								listEvacuation.Add(evacuationInfo);
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

	public class EvacuationInfo
	{
		public string evacuationID { get; set; }
		public string evacuationName { get; set; }
		public string barangay { get; set; }
		public string capacity { get; set; }
		public string status { get; set; }
	}
}
