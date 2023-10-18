using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Add
{
    public class MemberModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public MemberModel(IConfiguration configuration) => _configuration = configuration;
		public AddFamilyMemberInfo addfamilymemberInfo { get; set; } = new AddFamilyMemberInfo();
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve disaster details from the form
			addfamilymemberInfo.firstName = Request.Form["firstName"];
			addfamilymemberInfo.middleName = Request.Form["middleName"];
			addfamilymemberInfo.lastName = Request.Form["lastName"];
			addfamilymemberInfo.extName = Request.Form["extName"];
			addfamilymemberInfo.sex = Request.Form["sex"];
			addfamilymemberInfo.birthdate = Request.Form["birthdate"];
			addfamilymemberInfo.civilStatus = Request.Form["civilStatus"];
			addfamilymemberInfo.religion = Request.Form["religion"];
			addfamilymemberInfo.education = Request.Form["education"];
			addfamilymemberInfo.occupation = Request.Form["occupation"];
			addfamilymemberInfo.ethnicity = Request.Form["ethnicity"];
			addfamilymemberInfo.healthCondition = Request.Form["healthCondition"];
			addfamilymemberInfo.remarks = Request.Form["remarks"];
			addfamilymemberInfo.relationship = Request.Form["relationship"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'disaster' table
					string sql = "INSERT INTO family_member" +
								 "(firstName, middleName, lastName, extName, sex, birthdate, civilStatus, religion, education, occupation, ethnicity, healthCondition, remarks, relationship) " +
								 "VALUES (@firstName, @middleName, @lastName, @extName, @sex, @birthdate, @civilStatus, @religion, @education, @occupation, @ethnicity, @healthCondition, @remarks, @relationship)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@firstName", addfamilymemberInfo.firstName);
						command.Parameters.AddWithValue("@middleName", addfamilymemberInfo.middleName);
						command.Parameters.AddWithValue("@lastName", addfamilymemberInfo.lastName);
						command.Parameters.AddWithValue("@extName", addfamilymemberInfo.extName);
						command.Parameters.AddWithValue("@sex", addfamilymemberInfo.sex);
						command.Parameters.AddWithValue("@birthdate", addfamilymemberInfo.birthdate);
						command.Parameters.AddWithValue("@civilStatus", addfamilymemberInfo.civilStatus);
						command.Parameters.AddWithValue("@religion", addfamilymemberInfo.religion);
						command.Parameters.AddWithValue("@education", addfamilymemberInfo.education);
						command.Parameters.AddWithValue("@occupation", addfamilymemberInfo.occupation);
						command.Parameters.AddWithValue("@ethnicity", addfamilymemberInfo.ethnicity);
						command.Parameters.AddWithValue("@healthCondition", addfamilymemberInfo.healthCondition);
						command.Parameters.AddWithValue("@remarks", addfamilymemberInfo.remarks);
						command.Parameters.AddWithValue("@relationship", addfamilymemberInfo.relationship);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Family member added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/family/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}

	public class AddFamilyMemberInfo
	{
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string extName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string civilStatus { get; set; }
		public string religion { get; set; }
		public string education { get; set; }
		public string occupation { get; set; }
		public string ethnicity { get; set; }
		public string healthCondition { get; set; }
		public string remarks { get; set; }
		public string memberID { get; set; }
		public string familyID { get; set; }
		public string serialNum { get; set; }
		public string relationship { get; set; }
	}
}
