using AgapayAidSystem.Pages.ECStaffDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family
{
    public class IndexModel : PageModel
    {
		public List<FamilyInfo> listFamily = new List<FamilyInfo>();
		public string SortBy { get; set; } //Disaster Name
		public string SortOrder { get; set; } // Ascending, Descending
		public void OnGet(string sortBy, string sortOrder)
        {
			try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM family";

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
								FamilyInfo familyInfo = new FamilyInfo();

								familyInfo.familyID = reader.GetString(0);
								familyInfo.streetAddress = reader.GetString(1);
								familyInfo.barangayID = reader.GetString(2);
								familyInfo.livingInGida = reader.GetString(5);
								familyInfo.serialNum = reader.GetString(7);
								listFamily.Add(familyInfo);
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
	public class FamilyInfo
	{
		public string familyID { get; set; }
		public string streetAddress { get; set; }
		public string barangayID { get; set; }
		public string livingInGida { get; set; }
		public string serialNum { get; set; }

	}
}
