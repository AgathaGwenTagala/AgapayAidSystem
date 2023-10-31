using AgapayAidSystem.Pages.Family.Profile.Add;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Profile.Edit
{
    public class MemberModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public MemberModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public AddFamilyMemberInfo addfamilymemberInfo { get; set; } = new AddFamilyMemberInfo();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve evacuation center details from the form
			addfamilymemberInfo.memberID = Request.Form["memberID"];
			addfamilymemberInfo.relationship = Request.Form["relationship"];
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

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert updated data into the 'evacuation_center' table
					string sql = "UPDATE family_member " +
								 "SET relationship = @relationship, firstName = @firstName, " +
								 "middleName = @middleName, lastName = @lastName, extName = @extName, sex = @sex, " +
								 "birthdate = @birthdate, civilStatus = @civilStatus, religion = @religion, " +
								 "education = @education, occupation = @occupation, ethnicity = @ethnicity " +
								 "healthCondition = @healthCondition, remarks = @remarks " +
								 "WHERE memberID = @memberID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@relationship", addfamilymemberInfo.relationship);
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
						command.Parameters.AddWithValue("@memberID", addfamilymemberInfo.memberID);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Family member updated successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/family/profile/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}
}
