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
								evacuationInfo.centerType = "" + reader.GetString(2);
								evacuationInfo.streetAddress = "" + reader.GetString(3);
								evacuationInfo.barangayID = "" + reader.GetString(4);
								evacuationInfo.mobileNum = "" + reader.GetString(5);
								evacuationInfo.telephoneNum = "" + reader.GetString(6);
								evacuationInfo.maxCapacity = reader.GetInt32(7);
								evacuationInfo.status = "" + reader.GetString(8);
								evacuationInfo.toilet = reader.GetInt32(9);
								evacuationInfo.bathingCubicle = reader.GetInt32(10);
								evacuationInfo.communityKitchen = reader.GetInt32(11);
								evacuationInfo.washingArea = reader.GetInt32(12);
								evacuationInfo.womenChildSpace = reader.GetInt32(13);
								evacuationInfo.multipurposeArea = reader.GetInt32(14);
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
		public string centerType { get; set; }
		public string streetAddress { get; set; }
		public string barangayID { get; set; }
		public string mobileNum { get; set; }
		public string telephoneNum { get; set; }
		public int maxCapacity { get; set; }
		public string status { get; set; }
		public int toilet { get; set; }
		public int bathingCubicle { get; set; }
		public int communityKitchen { get; set; }
		public int washingArea { get; set; }
		public int womenChildSpace { get; set; }
		public int multipurposeArea { get; set; }
	}
}
