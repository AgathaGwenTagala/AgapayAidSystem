using AgapayAidSystem.Pages.Family.Profile;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using AgapayAidSystem.Pages.EvacuationCenter;

namespace AgapayAidSystem.Pages.Family.Profile
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public List<FamilyMemberInfo> listFamilyMember = new List<FamilyMemberInfo>();
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public string barangayName { get; set; }
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
					string familySql = "SELECT f.*, b.barangayName FROM family f " +
										"INNER JOIN barangay b ON f.barangayID = b.barangayID " +
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
								familyInfo.telephoneNum = familyReader.GetString(4);
								familyInfo.livingInGida = familyReader.GetString(5);
								familyInfo.beneficiary = familyReader.GetString(6);
								familyInfo.serialNum = familyReader.GetString(7);
								barangayName = familyReader.GetString(8);
							}
						}
					}

					// Fetch family member information
					string familyMemberSql = "SELECT * FROM family_member WHERE familyID = @familyID";
					using (MySqlCommand familyMemberCommand = new MySqlCommand(familyMemberSql, connection))
					{
						familyMemberCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyMemberReader = familyMemberCommand.ExecuteReader())
						{
							while (familyMemberReader.Read())
							{
								FamilyMemberInfo memberInfo = new FamilyMemberInfo
								{
									relationship = familyMemberReader.GetString(8),
									firstName = familyMemberReader.GetString(2),
									sex = familyMemberReader.GetString(6),
									birthdate = familyMemberReader.GetString(7),
									civilStatus = familyMemberReader.GetString(9),
									education = familyMemberReader.GetString(10),
									occupation = familyMemberReader.GetString(11),
									religion = familyMemberReader.GetString(12),
									ethnicity = familyMemberReader.GetString(13),
									healthCondition = familyMemberReader.GetString(14),
									memberID = familyMemberReader.GetString(16)
								};
								listFamilyMember.Add(memberInfo);
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

	}

	public class FamilyMemberInfo
	{
		public string relationship { get; set; }
		public string firstName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string civilStatus { get; set; }
		public string education { get; set; }
		public string occupation { get; set; }
		public string religion { get; set; }
		public string ethnicity { get; set; }
		public string healthCondition { get; set; }
		public string memberID { get; set; }
	}
}
