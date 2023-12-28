using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.account
{
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public SettingsModel(IConfiguration configuration) => _configuration = configuration;
        public ProfileInfo profileInfo = new ProfileInfo();
        public string successMessage = "";
        public string errorMessage = "";
        public string? UserId { get; set; }
        public string? UserType { get; set; }

        public void OnGet()
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                Response.Redirect("/login/index");
                return;
            }

			try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (UserType == "Admin")
                    {
                        string userSql = "SELECT * FROM user_profile_view " +
                                         "WHERE userID = @userID ";
                        using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
                        {
                            userCommand.Parameters.AddWithValue("@userID", UserId);
                            using (MySqlDataReader userReader = userCommand.ExecuteReader())
                            {
                                if (userReader.Read())
                                {
                                    profileInfo.userID = userReader.GetString(0);
                                    profileInfo.username = userReader.GetString(1);
                                    profileInfo.password = userReader.GetString(2);
                                    profileInfo.userType = userReader.GetString(3);
                                    profileInfo.createdAt = userReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                    profileInfo.adminID = userReader.GetString(5);
                                    profileInfo.adminName = userReader.GetString(6);
                                }
                            }
                        }
                    }

                    if (UserType == "LGU Staff")
					{
						string userSql = "SELECT * FROM lgu_profile_view " +
										 "WHERE userID = @userID ";
						using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
						{
							userCommand.Parameters.AddWithValue("@userID", UserId);
							using (MySqlDataReader userReader = userCommand.ExecuteReader())
							{
								if (userReader.Read())
								{
									profileInfo.userID = userReader.GetString(0);
									profileInfo.username = userReader.GetString(1);
									profileInfo.password = userReader.GetString(2);
									profileInfo.userType = userReader.GetString(3);
									profileInfo.createdAt = userReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
									profileInfo.lguStaffID = userReader.GetString(5);
									profileInfo.firstName = userReader.GetString(6);
									profileInfo.middleName = userReader.IsDBNull(7) ? "-" : userReader.GetString(7);
									profileInfo.lastName = userReader.GetString(8);
									profileInfo.extName = userReader.IsDBNull(9) ? "-" : userReader.GetString(9);
									profileInfo.sex = userReader.GetString(10);
									profileInfo.birthdate = userReader.GetDateTime(11).ToString("MMMM d, yyyy");
									profileInfo.mobileNum = userReader.GetString(12);
									profileInfo.emailAddress = userReader.GetString(13);
								}
							}
						}
					}

                    if (UserType == "EC Staff")
                    {
                        string userSql = "SELECT * FROM ec_profile_view " +
                                         "WHERE userID = @userID ";
                        using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
                        {
                            userCommand.Parameters.AddWithValue("@userID", UserId);
                            using (MySqlDataReader userReader = userCommand.ExecuteReader())
                            {
                                if (userReader.Read())
                                {
                                    profileInfo.userID = userReader.GetString(0);
                                    profileInfo.username = userReader.GetString(1);
                                    profileInfo.password = userReader.GetString(2);
                                    profileInfo.userType = userReader.GetString(3);
                                    profileInfo.createdAt = userReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                    profileInfo.ecStaffID = userReader.GetString(5);
                                    profileInfo.firstName = userReader.GetString(6);
                                    profileInfo.middleName = userReader.IsDBNull(7) ? "-" : userReader.GetString(7);
                                    profileInfo.lastName = userReader.GetString(8);
                                    profileInfo.extName = userReader.IsDBNull(9) ? "-" : userReader.GetString(9);
                                    profileInfo.sex = userReader.GetString(10);
                                    profileInfo.birthdate = userReader.GetDateTime(11).ToString("MMMM d, yyyy");
                                    profileInfo.mobileNum = userReader.GetString(12);
                                    profileInfo.emailAddress = userReader.GetString(13);
                                    profileInfo.availabilityStatus = userReader.GetString(14);
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public void OnPostContactInformation()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
            }

            // Retrieve information from the form
            profileInfo.emailAddress = Request.Form["emailAddress"];
            profileInfo.mobileNum = Request.Form["mobileNum"];
            profileInfo.availabilityStatus = Request.Form["availabilityStatus"];

            // Retrieve details of user in session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (UserType == "EC Staff")
                    {
                        // Insert updated data into the 'ec_staff' table
                        string sql = "UPDATE ec_staff " +
                                 "SET emailAddress = @emailAddress, mobileNum = @mobileNum, " +
                                 "availabilityStatus = @availabilityStatus " +
                                 "WHERE userID = @userID";

                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@emailAddress", profileInfo.emailAddress);
                            command.Parameters.AddWithValue("@mobileNum", profileInfo.mobileNum);
                            command.Parameters.AddWithValue("@availabilityStatus", profileInfo.availabilityStatus);
                            command.Parameters.AddWithValue("@userID", UserId);
                            command.ExecuteNonQuery();
                        }
                    }

                    if (UserType == "LGU Staff")
                    {
                        // Insert updated data into the 'lgu_staff' table
                        string sql = "UPDATE lgu_staff " +
                                     "SET emailAddress = @emailAddress, mobileNum = @mobileNum " +
                                     "WHERE userID = @userID";
                        
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@emailAddress", profileInfo.emailAddress);
                            command.Parameters.AddWithValue("@mobileNum", profileInfo.mobileNum);
                            command.Parameters.AddWithValue("@userID", UserId);
                            command.ExecuteNonQuery();
                        }
                    }

                }
                successMessage = "Information updated successfully!";
            }
            catch (Exception ex)
            {
                errorMessage = "Error updating contact information: " + ex.Message;
            }

            // Redirect back to the Settings page with success or error message
            Response.Redirect("/account/settings?successMessage=" + successMessage + "&errorMessage=" + errorMessage);
        }

        public void OnPostChangePassword()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
            }

            // Retrieve details of user in session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            // Retrieve passwords from the form
            string currentPassword = Request.Form["currentPassword"];
            string newPassword = Request.Form["newPassword"];
            string confirmPassword = Request.Form["confirmPassword"];

            if (newPassword != confirmPassword)
            {
                errorMessage = "Passwords do not match.";
                return;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the current password matches the one in the database
                    string checkPasswordSql = "SELECT password FROM user WHERE userID = @userID";
                    using (MySqlCommand checkPasswordCommand = new MySqlCommand(checkPasswordSql, connection))
                    {
                        checkPasswordCommand.Parameters.AddWithValue("@userID", UserId);
                        string storedPassword = checkPasswordCommand.ExecuteScalar()?.ToString();

                        // Compare the stored password with the entered current password
                        if (storedPassword != currentPassword)
                        {
                            errorMessage = "Current password is incorrect. Please enter the correct current password.";
                            return;
                        }
                    }

                    // Insert updated data into the 'user' table
                    string sql = "UPDATE user " +
                                 "SET password = @password " +
                                 "WHERE userID = @userID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@password", confirmPassword);
                        command.Parameters.AddWithValue("@userID", UserId);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "Password changed successfully!";
            }
            catch (Exception ex)
            {
                errorMessage = "Error changing password: " + ex.Message;
            }

            // Redirect back to the Settings page with success or error message
            Response.Redirect("/account/settings?successMessage=" + successMessage + "&errorMessage=" + errorMessage);
        }
    }
}
