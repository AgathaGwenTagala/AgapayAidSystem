using AgapayAidSystem.Pages.ECStaffDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family
{
    public class IndexModel : PageModel
    {
		public List<FamilyInfo> listFamily = new List<FamilyInfo>();
		public List<string> UniqueBarangays { get; set; }
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
					string sql = "SELECT fam.*, b.barangayName " +
								 "FROM family AS fam " +
								 "INNER JOIN barangay AS b ON fam.barangayID = b.barangayID";

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
								familyInfo.beneficiary = reader.GetString(6);
								familyInfo.serialNum = reader.GetString(7);
								familyInfo.barangayName = reader.GetString(8);
								familyInfo.mobileNum = reader.GetString(3);
								familyInfo.telephoneNum = reader.GetString(4);
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

			UniqueBarangays = GetUniqueBarangaysFromDatabase();
		}

		private List<string> GetUniqueBarangaysFromDatabase()
		{
			List<string> uniqueBarangays = new List<string>();

			try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					string sql = "SELECT DISTINCT b.barangayName " +
								 "FROM family AS fam " +
								 "INNER JOIN barangay AS b ON fam.barangayID = b.barangayID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								string barangayName = reader.GetString(0);
								uniqueBarangays.Add(barangayName);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.ToString());
			}

			return uniqueBarangays;
		}

	}
	public class FamilyInfo
	{
		public string familyID { get; set; }
		public string streetAddress { get; set; }
		public string barangayID { get; set; }
		public string livingInGida { get; set; }
		public string serialNum { get; set; }
		public string barangayName { get; set; }
		public string beneficiary { get; set; }
		public string mobileNum { get; set; }
		public string telephoneNum { get; set; }


	}
}
