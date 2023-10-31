using AgapayAidSystem.Pages.Family.Profile;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Profile
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public string errorMessage = "";
		public string successMessage = "";
		public void OnGet()
        {
			string familyID = Request.Query["familyID"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected disaster from the database
					string sql = "SELECT * FROM family where familyID = @familyID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								familyInfo.familyID = reader.GetString(0);
								familyInfo.streetAddress = reader.GetString(1);
								familyInfo.barangayID = reader.GetString(2);
								familyInfo.mobileNum = reader.GetString(3);
								familyInfo.telephoneNum = reader.GetString(4);
								familyInfo.livingInGida = reader.GetString(5);
								familyInfo.beneficiary = reader.GetString(6);
								familyInfo.serialNum = reader.GetString(7);
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
	}
}
