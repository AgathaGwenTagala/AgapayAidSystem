using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.ECStaffDatabase
{
    public class IndexModel : PageModel
    {
		public List<ECStaffDatabaseInfo> listECStaffDatabase = new List<ECStaffDatabaseInfo>();

		public string SortBy { get; set; } //Disaster Name
		public string SortOrder { get; set; } // Ascending, Descending
		public DateTime Birthdate { get; set; }
		public void OnGet(string sortBy, string sortOrder)
        {
            try
            {
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
					connection.Open();
					string sql = "SELECT * FROM ec_staff";

					// Apply sorting based on user's selection
					if (!string.IsNullOrEmpty(sortBy))
					{
						sql += $" ORDER BY {sortBy} {(sortOrder == "desc" ? "DESC" : "ASC")}";
						SortBy = sortBy;
						SortOrder = sortOrder;
					}
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ECStaffDatabaseInfo ecstaffdatabaseInfo = new ECStaffDatabaseInfo();

								ecstaffdatabaseInfo.ecStaffID = reader.GetString(0);
								ecstaffdatabaseInfo.firstName = reader.GetString(3);
								ecstaffdatabaseInfo.middleName = reader.GetString(4);
								ecstaffdatabaseInfo.lastName = reader.GetString(5);
								ecstaffdatabaseInfo.sex = reader.GetString(6);
								ecstaffdatabaseInfo.mobileNum = reader.GetString(8);
								ecstaffdatabaseInfo.birthdate = reader.GetDateTime(7).ToString("yyyy-MM-dd");
								ecstaffdatabaseInfo.availabilityStatus = reader.GetString(10);
								listECStaffDatabase.Add(ecstaffdatabaseInfo);
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
	public class ECStaffDatabaseInfo
	{
		public string ecStaffID { get; set; }
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string sex { get; set; }
		public string mobileNum { get; set; }
		public string birthdate { get; set; }
		public string availabilityStatus { get; set; }
		public string FullName => $"{firstName} {middleName} {lastName}";
	}
}
