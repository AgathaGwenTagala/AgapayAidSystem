using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.family.profile.distribution
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public List<RecordInfo> listRecord = new List<RecordInfo>();
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

            string familyID = Request.Query["familyID"];
			
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected family from the database
					string familySql = "SELECT familyID, serialNum FROM family " +
									   "WHERE familyID = @familyID";
					using (MySqlCommand familyCommand = new MySqlCommand(familySql, connection))
					{
						familyCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyReader = familyCommand.ExecuteReader())
						{
							if (familyReader.Read())
							{
								familyInfo.familyID = familyReader.GetString(0);
								familyInfo.serialNum = familyReader.GetString(1);
							}
						}
					}

					// Fetch distribution record of selected family
					string sql = "SELECT * FROM family_distribution_record_view " +
								 "WHERE familyID = @familyID ORDER BY receiveDate;";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								RecordInfo recordInfo = new RecordInfo
								{
									distributionID = reader.GetString(0),
									packID = reader.GetString(1),
									assignmentID = reader.IsDBNull(2) ? null : reader.GetString(2),
									entryLogID = reader.GetString(3),
									qty = reader.GetInt32(4).ToString(),
									receiveDate = reader.GetDateTime(5).ToString("yyyy-MM-dd"),
									disasterName = reader.GetString(6),
									centerName = reader.GetString(7),
									familyID = reader.GetString(8),
									fullName = reader.GetString(9),
									staffFullName = reader.GetString(10)
								};
								listRecord.Add(recordInfo);
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

	public class RecordInfo
	{
		public string? distributionID { get; set; }
		public string? packID { get; set; }
		public string? assignmentID { get; set; }
		public string? entryLogID { get; set; }
		public string? qty { get; set; }
		public string? receiveDate { get; set; }
		public string? disasterName { get; set; }
		public string? centerName { get; set; }
		public string? familyID { get; set; }
		public string? fullName { get; set; }
		public string? staffFullName { get; set; }
	}
}
