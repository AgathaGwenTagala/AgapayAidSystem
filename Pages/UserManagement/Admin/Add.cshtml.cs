using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement.Admin
{
    public class AddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddModel(IConfiguration configuration) => _configuration = configuration;
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string adminName { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

        public void OnGet(string action, string userIDToDelete)
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                Response.Redirect("/login/index");
                return;
            }

            if (UserType != "Admin")
            {
                Response.Redirect("/accessdenied");
                return;
            }

            // Retrieve and decode the userID from the query string
            string encodedUserID = HttpContext.Request.Query["userID"];
            userID = WebUtility.UrlDecode(encodedUserID);

            // Call the deletion method
            if (action == "cancel" && !string.IsNullOrEmpty(userIDToDelete))
            {
                // Delete the record associated with userIDToDelete
                DeleteUserRecord(userIDToDelete);
                return;
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

            // Retrieve the userID and adminName from the form
            string userID = Request.Form["userID"];
            string adminName = Request.Form["adminName"];

            // Check if userID is null or empty
            if (string.IsNullOrEmpty(userID))
            {
                errorMessage = "UserID is missing.";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
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
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/usermanagement/admin/add?userID=" + userID + "&errorMessage=" + errorMessage);
            }
            else
            {
                Response.Redirect("/usermanagement/index?successMessage=" + successMessage);
            }
        }

        private void DeleteUserRecord(string userIDToDelete)
        {
            bool errorOccurred = false;

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Implement code to delete the record with the given userID
                    string sql = "DELETE FROM user WHERE userID = @userID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userIDToDelete);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // User deleted successfully
                            // successMessage = "User addition aborted";
                        }
                        else
                        {
                            // No user found with the provided userID
                            errorMessage = "User not found with the provided userID";
                            errorOccurred = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/usermanagement/admin/add?userID=" + userID + "&errorMessage=" + errorMessage);
            }
            else
            {
                Response.Redirect("/usermanagement/index");
            }
        }
    }
}
