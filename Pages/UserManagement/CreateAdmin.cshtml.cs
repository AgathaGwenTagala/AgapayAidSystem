using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class CreateAdminModel : PageModel
    {
        //public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string adminName { get; set; } = "";
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet(string lastInsertedUserID, string action, string userIDToDelete)
		{
            // Retrieve and decode the userID from the query string
            string encodedUserID = HttpContext.Request.Query["userID"];
            userID = WebUtility.UrlDecode(encodedUserID);

            // Call the deletion method
            if (action == "cancel" && !string.IsNullOrEmpty(userIDToDelete))
            {
                // Delete the record associated with userIDToDelete
                DeleteAdminRecord(userIDToDelete);
                return;
            }
        }

		public void OnPost()
		{
            if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve the userID from the form
			string userID = Request.Form["userID"];
            string adminName = Request.Form["adminName"];

            // Check if userID is null or empty
            if (string.IsNullOrEmpty(userID))
			{
				errorMessage = "UserID is missing.";
				return;
			}

            try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'admin' table
					string sql = "INSERT INTO admin (userID, adminName) VALUES (@userID, @adminName);";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@userID", userID);
						command.Parameters.AddWithValue("@adminName", adminName);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Admin user created successfully!";
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/UserManagement/Index");
		}

        private void DeleteAdminRecord(string userIDToDelete)
        {
            try
            {
                string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Implement code to delete the record with the given userID
                    String sql = "DELETE FROM user WHERE userID = @userID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // User deleted successfully
                            successMessage = "User deleted successfully.";
                        }
                        else
                        {
                            // No user found with the provided userID
                            errorMessage = "User not found with the provided userID.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            // Redirect to the appropriate URL after deletion
            Response.Redirect("/UserManagement/Index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }

    }
}