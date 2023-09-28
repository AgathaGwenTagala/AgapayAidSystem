using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.EvacuationCenter
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
					String sql = "SELECT * FROM evacuation_center";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								EvacuationInfo evacuationInfo = new EvacuationInfo();

								evacuationInfo.centerID = "" + reader.GetString(0);
								evacuationInfo.centerName = "" + reader.GetString(1);
								evacuationInfo.barangayID = "" + reader.GetString(4);
								evacuationInfo.maxCapacity = "" + reader.GetString(7);
								evacuationInfo.status = "" + reader.GetString(8);
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
		public string centerID { get; set; }
		public string centerName { get; set; }
		public string barangayID { get; set; }
		public string maxCapacity { get; set; }
		public string status { get; set; }
	}


}
