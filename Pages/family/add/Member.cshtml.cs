using AgapayAidSystem.Pages.Family;
using AgapayAidSystem.Pages.Family.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.family.add
{
    public class MemberModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public MemberModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public MemberInfo memberInfo { get; set; } = new MemberInfo();
		public string errorMessage = "";
		public string successMessage = "";
		
		[BindProperty(SupportsGet = true)]
		public string familyID { get; set; }

		public void OnGet()
        {
			familyID = Request.Query["familyID"];

			if (string.IsNullOrEmpty(familyID))
			{
				errorMessage = "Invalid familyID.";
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
			memberInfo.firstName = Request.Form["firstName"];
			memberInfo.middleName = Request.Form["middleName"];
			memberInfo.lastName = Request.Form["lastName"];
			memberInfo.extName = Request.Form["extName"];
			memberInfo.sex = Request.Form["sex"];
			memberInfo.birthdate = Request.Form["birthdate"];
			memberInfo.relationship = "Family member";
			memberInfo.civilStatus = Request.Form["civilStatus"];
			memberInfo.education = Request.Form["education"];
			memberInfo.occupation = Request.Form["occupation"];
			memberInfo.religion = Request.Form["religion"];
			memberInfo.isIndigenousPerson = Request.Form["isIndigenousPerson"];
			memberInfo.healthCondition = Request.Form["healthCondition"];
			memberInfo.remarks = Request.Form["remarks"];

			// Others
			string redirectType = Request.Form["redirectType"];

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

					Console.WriteLine($"Retrieved familyID: {familyID}");

					// Insert family head details into the 'family_member' table
					string memberSql = "INSERT INTO family_member " +
									   "(familyID, firstName, middleName, lastName, extName, sex, birthdate, relationship, " +
									   "civilStatus, education, occupation, religion, isIndigenousPerson, healthCondition, remarks) " +
									   "VALUES (@familyID, @firstName, @middleName, @lastName, @extName, @sex, @birthdate, @relationship, " +
									   "@civilStatus, @education, @occupation, @religion, @isIndigenousPerson, @healthCondition, @remarks)";
					using (MySqlCommand memberCommand = new MySqlCommand(memberSql, connection))
					{
						memberCommand.Parameters.AddWithValue("@familyID", familyID);
						memberCommand.Parameters.AddWithValue("@firstName", memberInfo.firstName);
						memberCommand.Parameters.AddWithValue("@middleName", memberInfo.middleName);
						memberCommand.Parameters.AddWithValue("@lastName", memberInfo.lastName);
						memberCommand.Parameters.AddWithValue("@extName", memberInfo.extName);
						memberCommand.Parameters.AddWithValue("@sex", memberInfo.sex);
						memberCommand.Parameters.AddWithValue("@birthdate", memberInfo.birthdate);
						memberCommand.Parameters.AddWithValue("@relationship", memberInfo.relationship);
						memberCommand.Parameters.AddWithValue("@civilStatus", memberInfo.civilStatus);
						memberCommand.Parameters.AddWithValue("@education", memberInfo.education);
						memberCommand.Parameters.AddWithValue("@occupation", memberInfo.occupation);
						memberCommand.Parameters.AddWithValue("@religion", memberInfo.religion);
						memberCommand.Parameters.AddWithValue("@isIndigenousPerson", memberInfo.isIndigenousPerson);
						memberCommand.Parameters.AddWithValue("@healthCondition", memberInfo.healthCondition);
						memberCommand.Parameters.AddWithValue("@remarks", memberInfo.remarks);
						memberCommand.ExecuteNonQuery();
						Console.WriteLine($"Successfully inserted family member for familyID {familyID}");
					}
				}
				successMessage = "Family added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/family/add/member?familyID=" + familyID + "&errorMessage=" + errorMessage);
			}
			else
			{
				if (redirectType == "add")
				{
					Response.Redirect("/family/add/member?familyID=" + familyID);
				}
				else
				{
					Response.Redirect("/family/profile/index?familyID=" +familyID + "&successMessage=" + successMessage);
				}
			}
		}
    }
}
