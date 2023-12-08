using AgapayAidSystem.Pages.Family;
using AgapayAidSystem.Pages.Family.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.family.edit
{
    public class MemberModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public MemberModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public MemberInfo memberInfo { get; set; } = new MemberInfo();
		public string errorMessage = "";
		public string successMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

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
			string memberID = Request.Query["memberID"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected family from the database
					string familySql = "SELECT * FROM family WHERE familyID = @familyID";
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
							}
						}
					}

					// Fetch family member information
					string familyMemberSql = "SELECT * FROM family_member_view WHERE memberID = @memberID";
					using (MySqlCommand familyMemberCommand = new MySqlCommand(familyMemberSql, connection))
					{
						familyMemberCommand.Parameters.AddWithValue("@memberID", memberID);
						using (MySqlDataReader familyMemberReader = familyMemberCommand.ExecuteReader())
						{
							if (familyMemberReader.Read())
							{
								memberInfo.familyID = familyMemberReader.GetString(0);
								memberInfo.memberID = familyMemberReader.GetString(1);
								memberInfo.firstName = familyMemberReader.GetString(2);
								memberInfo.middleName = familyMemberReader.IsDBNull(3) ? null : familyMemberReader.GetString(3);
								memberInfo.lastName = familyMemberReader.GetString(4);
								memberInfo.extName = familyMemberReader.IsDBNull(5) ? null : familyMemberReader.GetString(5);
								memberInfo.fullName = familyMemberReader.GetString(6);
								memberInfo.sex = familyMemberReader.GetString(7);
								memberInfo.birthdate = familyMemberReader.GetDateTime(8).ToString("yyyy-MM-dd");
								memberInfo.age = familyMemberReader.GetInt64(9).ToString();
								memberInfo.relationship = familyMemberReader.GetString(10);
								memberInfo.civilStatus = familyMemberReader.GetString(11);
								memberInfo.education = familyMemberReader.GetString(12);
								memberInfo.occupation = familyMemberReader.GetString(13);
								memberInfo.religion = familyMemberReader.GetString(14);
								memberInfo.isIndigenousPerson = familyMemberReader.GetString(15);
								memberInfo.healthCondition = familyMemberReader.GetString(16);
								memberInfo.remarks = familyMemberReader.IsDBNull(17) ? null : familyMemberReader.GetString(17);
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

		public void OnPost()
		{
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			// Retrieve family member details from the form
			string familyID = Request.Form["familyID"];
			memberInfo.memberID = Request.Form["memberID"];
			memberInfo.firstName = Request.Form["firstName"];
			memberInfo.middleName = Request.Form["middleName"];
			memberInfo.lastName = Request.Form["lastName"];
			memberInfo.extName = Request.Form["extName"];
			memberInfo.sex = Request.Form["sex"];
			memberInfo.birthdate = Request.Form["birthdate"];
			memberInfo.civilStatus = Request.Form["civilStatus"];
			memberInfo.education = Request.Form["education"];
			memberInfo.occupation = Request.Form["occupation"];
			memberInfo.religion = Request.Form["religion"];
			memberInfo.isIndigenousPerson = Request.Form["isIndigenousPerson"];
			memberInfo.healthCondition = Request.Form["healthCondition"];
			memberInfo.remarks = Request.Form["remarks"];

			if (string.IsNullOrEmpty(memberInfo.middleName))
			{
				memberInfo.middleName = null;
			}

			if (string.IsNullOrEmpty(memberInfo.extName))
			{
				memberInfo.extName = null;
			}

			if (string.IsNullOrEmpty(memberInfo.healthCondition))
			{
				memberInfo.healthCondition = "No existing health condition";
			}

			if (string.IsNullOrEmpty(memberInfo.remarks))
			{
				memberInfo.remarks = null;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Update details of the selected family member
					string memberSql = "UPDATE family_member " +
									   "SET firstName = @firstName, middleName = @middleName, lastName = @lastName, extName = @extName, " +
									   "sex = @sex, birthdate = @birthdate, civilStatus = @civilStatus, " +
									   "education = @education, occupation = @occupation, religion = @religion, " +
									   "isIndigenousPerson = @isIndigenousPerson, healthCondition = @healthCondition, remarks = @remarks " +
									   "WHERE memberID = @memberID";
					using (MySqlCommand memberCommand = new MySqlCommand(memberSql, connection))
					{
						memberCommand.Parameters.AddWithValue("@firstName", memberInfo.firstName);
						memberCommand.Parameters.AddWithValue("@middleName", memberInfo.middleName);
						memberCommand.Parameters.AddWithValue("@lastName", memberInfo.lastName);
						memberCommand.Parameters.AddWithValue("@extName", memberInfo.extName);
						memberCommand.Parameters.AddWithValue("@sex", memberInfo.sex);
						memberCommand.Parameters.AddWithValue("@birthdate", memberInfo.birthdate);
						memberCommand.Parameters.AddWithValue("@civilStatus", memberInfo.civilStatus);
						memberCommand.Parameters.AddWithValue("@education", memberInfo.education);
						memberCommand.Parameters.AddWithValue("@occupation", memberInfo.occupation);
						memberCommand.Parameters.AddWithValue("@religion", memberInfo.religion);
						memberCommand.Parameters.AddWithValue("@isIndigenousPerson", memberInfo.isIndigenousPerson);
						memberCommand.Parameters.AddWithValue("@healthCondition", memberInfo.healthCondition);
						memberCommand.Parameters.AddWithValue("@remarks", memberInfo.remarks);
						memberCommand.Parameters.AddWithValue("@memberID", memberInfo.memberID);
						memberCommand.ExecuteNonQuery();
					}
				}
				successMessage = "Family member information edited successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/family/edit/memberID?=" + memberInfo.memberID + "&familyID=" + familyID + "&errorMessage=" + errorMessage);
			}
			else
			{
				Response.Redirect("/family/profile/index?familyID=" + familyID + "&successMessage=" + successMessage);
			}
		}
	}
}
