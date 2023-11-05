using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public List<FamilyInfo> listFamily = new List<FamilyInfo>();
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
					string sql = "SELECT * FROM family_list_view;";
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
								familyInfo.mobileNum = reader.GetString(3);
								familyInfo.telephoneNum = reader.IsDBNull(4) ? null : reader.GetString(4);
								familyInfo.livingInGida = reader.GetString(5);
								familyInfo.beneficiary = reader.GetString(6);
								familyInfo.serialNum = reader.GetString(7);
								familyInfo.barangayName = reader.GetString(8);
								familyInfo.municipalityCity = reader.GetString(9);
								familyInfo.familySize = reader.GetInt64(10).ToString();
								listFamily.Add(familyInfo);
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

	public class FamilyInfo
	{
		public string familyID { get; set; }
		public string streetAddress { get; set; }
		public string barangayID { get; set; }
		public string mobileNum { get; set; }
		public string telephoneNum { get; set; }
		public string livingInGida { get; set; }
		public string beneficiary { get; set; }
		public string serialNum { get; set; }
		public string barangayName { get; set; }
		public string municipalityCity { get; set; }
		public string familySize { get; set; }
	}
}
