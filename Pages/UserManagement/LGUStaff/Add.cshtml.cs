using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement.LGUStaff
{
    public class AddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddModel(IConfiguration configuration) => _configuration = configuration;
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

        public void OnGet(string action)
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
            if (action == "cancel" && !string.IsNullOrEmpty(userID))
            {
                // Delete the record associated with userIDToDelete
                DeleteUserRecord(userID);
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

            // Retrieve the userID and other fields from the form
            string userID = Request.Form["userID"];
            string firstName = Request.Form["firstName"];
            string? middleName = Request.Form["middleName"];
            string lastName = Request.Form["lastName"];
            string? extName = Request.Form["extName"];
            string sex = Request.Form["sex"];
            string birthdate = Request.Form["birthdate"];
            string mobileNum = Request.Form["mobileNum"];
            string emailAddress = Request.Form["emailAddress"];

            // Check if userID is null or empty
            if (string.IsNullOrEmpty(userID))
            {
                errorMessage = "UserID is missing.";
                errorOccurred = true;
            }

			if (string.IsNullOrEmpty(middleName))
			{
				middleName = null;
			}

			if (string.IsNullOrEmpty(extName))
			{
				extName = null;
			}

            // Validate mobile number (should be exactly 11 characters)
            if (mobileNum.Length != 11)
            {
                errorMessage = "Mobile number should be exactly 11 characters long.";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert data into the 'lgu_staff' table
                    string sql = "INSERT INTO lgu_staff" +
                                 "(userID, firstName, middleName, lastName, extName, " +
                                 "sex, birthdate, mobileNum, emailAddress)" +
                                 "VALUES (@userID, @firstName, @middleName, @lastName, @extName," +
                                 "@sex, @birthdate, @mobileNum, @emailAddress)";

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

                successMessage = "LGU Staff user created successfully!";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/usermanagement/lgustaff/add?userID=" + userID + "&errorMessage=" + errorMessage);
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
                        command.ExecuteNonQuery();
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
                Response.Redirect("/usermanagement/lgustaff/add?userID=" + userID + "&errorMessage=" + errorMessage);
            }
            else
            {
                Response.Redirect("/usermanagement/index");
            }
        }
    }
}
