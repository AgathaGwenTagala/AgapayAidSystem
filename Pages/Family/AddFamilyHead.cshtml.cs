using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family
{
    public class AddFamilyHeadModel : PageModel
    {
		public AddFamilyHeadInfo addfamilyheadInfo { get; set; } = new AddFamilyHeadInfo();
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
			addfamilyheadInfo.firstName = Request.Form["firstName"];
			addfamilyheadInfo.middleName = Request.Form["middleName"];
			addfamilyheadInfo.lastName = Request.Form["lastName"];
			addfamilyheadInfo.extName = Request.Form["extName"];
			addfamilyheadInfo.sex = Request.Form["sex"];
			addfamilyheadInfo.birthdate = Request.Form["birthdate"];
			addfamilyheadInfo.civilStatus = Request.Form["civilStatus"];
			addfamilyheadInfo.religion = Request.Form["religion"];
			addfamilyheadInfo.education = Request.Form["education"];
			addfamilyheadInfo.occupation = Request.Form["occupation"];
			addfamilyheadInfo.ethnicity = Request.Form["ethnicity"];
			addfamilyheadInfo.healthCondition = Request.Form["healthCondition"];
			addfamilyheadInfo.remarks = Request.Form["remarks"];
			addfamilyheadInfo.relationship = Request.Form["relationship"];

			try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'disaster' table
					string sql = "INSERT INTO family_member" +
								 "(firstName, middleName, lastName, extName, sex, birthdate, civilStatus, religion, education, occupation, ethnicity, healthCondition, remarks, relationship) " +
								 "VALUES (@firstName, @middleName, @lastName, @extName, @sex, @birthdate, @civilStatus, @religion, @education, @occupation, @ethnicity, @healthCondition, @remarks, @relationship)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@firstName", addfamilyheadInfo.firstName);
						command.Parameters.AddWithValue("@middleName", addfamilyheadInfo.middleName);
						command.Parameters.AddWithValue("@lastName", addfamilyheadInfo.lastName);
						command.Parameters.AddWithValue("@extName", addfamilyheadInfo.extName);
						command.Parameters.AddWithValue("@sex", addfamilyheadInfo.sex);
						command.Parameters.AddWithValue("@birthdate", addfamilyheadInfo.birthdate);
						command.Parameters.AddWithValue("@civilStatus", addfamilyheadInfo.civilStatus);
						command.Parameters.AddWithValue("@religion", addfamilyheadInfo.religion);
						command.Parameters.AddWithValue("@education", addfamilyheadInfo.education);
						command.Parameters.AddWithValue("@occupation", addfamilyheadInfo.occupation);
						command.Parameters.AddWithValue("@ethnicity", addfamilyheadInfo.ethnicity);
						command.Parameters.AddWithValue("@healthCondition", addfamilyheadInfo.healthCondition);
						command.Parameters.AddWithValue("@remarks", addfamilyheadInfo.remarks);
						command.Parameters.AddWithValue("@relationship", addfamilyheadInfo.relationship);
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

			Response.Redirect("/AddFamily/AddFamilyHead");
		}
	}

	public class AddFamilyHeadInfo
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
