using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;


namespace AgapayAidSystem.Pages.UserManagement
{
	public enum UserType
	{
		Admin,
		EC_Staff,
		LGU_Staff
	}
	public class CreateModel : PageModel
    {
		public UserInfo userInfo = new UserInfo();
		public String errorMessage = "";
		public String successMessage = "";

		
		
		public void OnGet()
		{
		}

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Get the selected userType directly from the form
			userInfo.userType = Request.Form["userType"];

			// Validate userType
			if (userInfo.userType != "Admin" && userInfo.userType != "EC Staff" && userInfo.userType != "LGU Staff")
			{
				errorMessage = "Invalid user type. Choose from Admin, EC Staff, LGU Staff.";
				return;
			}


			// Save the new user into the database
			try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					String sql = "INSERT INTO user " +
								 "(userType) VALUES " +
								 "(@userType);";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@userType", userInfo.userType);
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			userInfo.userType = "";
			successMessage = "New user added successfully.";

			Response.Redirect("/UserManagement/Index");
		}
	}
}