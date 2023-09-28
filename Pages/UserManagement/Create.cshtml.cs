using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement
{
	public class CreateModel : PageModel
    {
		public UserInfo userInfo { get; set; } = new UserInfo();
		public string userID { get; set; } = "";
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

			// Retrieve the selected userType from the form
			userInfo.userType = Request.Form["userType"];

			// Validate userType
			if (string.IsNullOrEmpty(userInfo.userType))
			{
				errorMessage = "Please select a user type.";
				return;
			}

			else if (userInfo.userType != "Admin" && userInfo.userType != "EC Staff" && userInfo.userType != "LGU Staff")
			{
				errorMessage = "Invalid user type. Choose from Admin, EC Staff, LGU Staff.";
				return;
			}

            // Initialize a variable to store the last inserted userID
            string lastInsertedUserID = "";

            // Save the new user into the database
            try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

                    // Insert the userType into the 'user' table
                    String sql = "INSERT INTO user (userType) VALUES (@userType);";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@userType", userInfo.userType);
						command.ExecuteNonQuery();
					}

                    // Fetch the last inserted userID based on userType
                    string selectSql = "SELECT userID FROM user WHERE userType = @userType ORDER BY createdAt DESC LIMIT 1;";
                    using (MySqlCommand selectCommand = new MySqlCommand(selectSql, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@userType", userInfo.userType);
                        lastInsertedUserID = selectCommand.ExecuteScalar()?.ToString();
                    }
                }

                
            }
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}


            // Redirect to respective pages based on userType
            // Check if lastInsertedUserID has a valid value
            if (!string.IsNullOrEmpty(lastInsertedUserID))
            {
                if (userInfo.userType == "Admin")
                {
                    Response.Redirect($"/UserManagement/CreateAdmin?userID={WebUtility.UrlEncode(lastInsertedUserID)}");
                }
                else if (userInfo.userType == "LGU Staff")
                {
                    Response.Redirect($"/UserManagement/CreateLGUStaff?userID={WebUtility.UrlEncode(lastInsertedUserID)}");
                }
                else if (userInfo.userType == "EC Staff")
                {
                    Response.Redirect($"/UserManagement/CreateECStaff?userID={WebUtility.UrlEncode(lastInsertedUserID)}");
                }
            }
            else
            {
                // Handle the case where lastInsertedUserID is missing or invalid
                errorMessage = "UserID is missing or invalid.";
            }            
        }
    }
}