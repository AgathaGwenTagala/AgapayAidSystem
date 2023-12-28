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
		public List<MemberInfo> listMember = new List<MemberInfo>();
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public List<BarangayInfo> Barangays { get; set; }
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

            Barangays = GetBarangaysFromDatabase();
			
			string familyID = Request.Query["familyID"];
			
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected family from the database
					string familySql = "SELECT f.*, " +
									   "CONCAT(f.streetAddress, ', Brgy. ', b.barangayName, ', ', " +
									   "b.municipalityCity, ', ', b.province) AS fullAddress " + 
									   "FROM family f " +
									   "JOIN barangay b ON f.barangayID = b.barangayID " +
									   "WHERE f.familyID = @familyID";
					using (MySqlCommand familyCommand = new MySqlCommand(familySql, connection))
					{
						familyCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyReader = familyCommand.ExecuteReader())
						{
							if (familyReader.Read())
							{
								familyInfo.familyID = familyReader.GetString(0);
								familyInfo.streetAddress = familyReader.GetString(1);
								familyInfo.barangayID = familyReader.GetString(2);
								familyInfo.mobileNum = familyReader.GetString(3);
								familyInfo.telephoneNum = familyReader.IsDBNull(4) ? null : familyReader.GetString(4);
								familyInfo.serialNum = familyReader.GetString(5);
								familyInfo.fullAddress = familyReader.GetString(6);
							}
						}
					}

					// Fetch family member information
					string familyMemberSql = "SELECT * FROM family_member_view WHERE familyID = @familyID";
					using (MySqlCommand familyMemberCommand = new MySqlCommand(familyMemberSql, connection))
					{
						familyMemberCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyMemberReader = familyMemberCommand.ExecuteReader())
						{
							while (familyMemberReader.Read())
							{
								MemberInfo memberInfo = new MemberInfo
								{
									familyID = familyMemberReader.GetString(0),
									memberID = familyMemberReader.GetString(1),
									firstName = familyMemberReader.GetString(2),
									middleName = familyMemberReader.IsDBNull(3) ? null : familyMemberReader.GetString(3),
									lastName = familyMemberReader.GetString(4),
									extName = familyMemberReader.IsDBNull(5) ? null : familyMemberReader.GetString(5),
									fullName = familyMemberReader.GetString(6),
									sex = familyMemberReader.GetString(7),
									birthdate = familyMemberReader.GetDateTime(8).ToString("MMM. d, yyyy"),
									age = familyMemberReader.GetInt64(9).ToString(),
									relationship = familyMemberReader.GetString(10),
									civilStatus = familyMemberReader.GetString(11),
									education = familyMemberReader.GetString(12),
									occupation = familyMemberReader.GetString(13),
									religion = familyMemberReader.GetString(14),
									isIndigenousPerson = familyMemberReader.GetString(15),
									healthCondition = familyMemberReader.GetString(16),
									remarks = familyMemberReader.IsDBNull(17) ? null : familyMemberReader.GetString(17)
								};
								listMember.Add(memberInfo);
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

		private List<BarangayInfo> GetBarangaysFromDatabase()
		{
			var barangayData = new List<BarangayInfo>();

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					string sql = "SELECT barangayID, barangayName FROM barangay ORDER BY barangayName";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								barangayData.Add(new BarangayInfo
								{
									barangayID = reader.GetString(0),
									barangayName = reader.GetString(1)
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			return barangayData;
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}

	public class MemberInfo
	{
		public string memberID { get; set; }
		public string familyID { get; set; }
		public string fullName { get; set; }
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string extName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string age { get; set; }
		public string relationship { get; set; }
		public string civilStatus { get; set; }
		public string education { get; set; }
		public string occupation { get; set; }
		public string religion { get; set; }
		public string isIndigenousPerson { get; set; }
		public string healthCondition { get; set; }
		public string remarks { get; set; }
	}
}
