using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement.ECStaff
{
    public class AddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddModel(IConfiguration configuration) => _configuration = configuration;
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string firstName { get; set; } = "";
        public string middleName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string extName { get; set; } = "";
        public string sex { get; set; } = "";
        public string birthdate { get; set; } = "";
        public string mobileNum { get; set; } = "";
        public string emailAddress { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet(string action, string userIDToDelete)
        {
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
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                return;
            }

            // Retrieve the userID and other fields from the form
            string userID = Request.Form["userID"];
            string firstName = Request.Form["firstName"];
            string middleName = Request.Form["middleName"];
            string lastName = Request.Form["lastName"];
            string extName = Request.Form["extName"];
            string sex = Request.Form["sex"];
            string birthdate = Request.Form["birthdate"];
            string mobileNum = Request.Form["mobileNum"];
            string emailAddress = Request.Form["emailAddress"];

            // Check if userID is null or empty
            if (string.IsNullOrEmpty(userID))
            {
                errorMessage = "UserID is missing";
                return;
            }

            // Validate mobile number (should be exactly 11 characters)
            if (mobileNum.Length != 11)
            {
                errorMessage = "Mobile number should be exactly 11 characters long";
                return;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert data into the 'ec_staff' table
                    string sql = "INSERT INTO ec_staff" +
                                 "(userID, firstName, middleName, lastName, extName, " +
                                 "sex, birthdate, mobileNum, emailAddress )" +
                                 "VALUES (@userID, @firstName, @middleName, @lastName, @extName ," +
                                 "@sex, @birthdate, @mobileNum, @emailAddress )";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        command.Parameters.AddWithValue("@firstName", firstName);
                        command.Parameters.AddWithValue("@middleName", middleName);
                        command.Parameters.AddWithValue("@lastName", lastName);
                        command.Parameters.AddWithValue("@extName", extName);
                        command.Parameters.AddWithValue("@sex", sex);
                        command.Parameters.AddWithValue("@birthdate", birthdate);
                        command.Parameters.AddWithValue("@mobileNum", mobileNum);
                        command.Parameters.AddWithValue("@emailAddress", emailAddress);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "EC Staff user created successfully!";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/usermanagement/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }

        private void DeleteUserRecord(string userIDToDelete)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
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
                            successMessage = "User addition aborted";
                        }
                        else
                        {
                            // No user found with the provided userID
                            errorMessage = "User not found with the provided userID";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            // Redirect to the appropriate URL after deletion
            Response.Redirect("/usermanagement/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }
    }
}
