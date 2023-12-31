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
        public string? UserId { get; set; }
        public string? UserType { get; set; }

        public void OnGet()
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                Response.Redirect("/login/index");
                return;
            }

            try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM family_list_view ORDER BY familyHead;";
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
								familyInfo.serialNum = reader.GetString(5);
								familyInfo.barangayName = reader.GetString(6);
								familyInfo.municipalityCity = reader.GetString(7);
								familyInfo.familyHead = reader.GetString(8);
								familyInfo.familySize = reader.GetInt64(9).ToString();
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
		public string? familyID { get; set; }
		public string? streetAddress { get; set; }
		public string? barangayID { get; set; }
		public string? mobileNum { get; set; }
		public string? telephoneNum { get; set; }
		public string? serialNum { get; set; }
		public string? barangayName { get; set; }
		public string? fullAddress { get; set; }
		public string? municipalityCity { get; set; }
		public string? familyHead { get; set; }
		public string? familySize { get; set; }
	}
}
