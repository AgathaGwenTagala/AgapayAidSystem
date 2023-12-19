using AgapayAidSystem.Pages.account;
using AgapayAidSystem.Pages.UserManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;


namespace AgapayAidSystem.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public UserInfo userInfo = new UserInfo();
        public ProfileInfo profileInfo = new ProfileInfo();
        public string successMessage = "";
        public string errorMessage = "";

		public void OnGet()
        {
            // Check if there is already a user in session
            if (HttpContext.Session.GetString("UserId") != null)
            {
                Response.Redirect("/account/index");
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

            string username = Request.Form["username"];
            string password = Request.Form["password"];

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch user information
                    string sql = "SELECT * FROM user WHERE username = @username and password = @password;";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                
                                userInfo.userID = reader.GetString(0);
                                userInfo.username = reader.GetString(1);
                                userInfo.password = reader.GetString(2);
                                userInfo.userType = reader.GetString(3);
                                // userInfo.createdAt = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();

                                // Convert createdAt to UTC+8
                                DateTime createdAtUtc = reader.GetDateTime(4);
                                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"); // Use the time zone for UTC+8
                                userInfo.createdAt = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzInfo).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();

                                // Set user information in the session
                                HttpContext.Session.SetString("UserId", userInfo.userID);
                                HttpContext.Session.SetString("UserType", userInfo.userType);

                                // Log the successful login
                                LogLoginSuccess(userInfo.userID, userInfo.userType, userInfo.username);
                            }
							else
					        {
								// No user found with the provided credentials
								errorMessage = "Invalid username or password.";
								errorOccurred = true;
							}
						}
                    }

                    // Fetch user information
                    if (userInfo.userType == "Admin")
                    {
                        string userSql = "SELECT adminName as fullName FROM user_profile_view " +
                                         "WHERE userID = @userID ";
                        using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
                        {
                            userCommand.Parameters.AddWithValue("@userID", userInfo.userID);
                            using (MySqlDataReader userReader = userCommand.ExecuteReader())
                            {
                                if (userReader.Read())
                                {
                                    profileInfo.fullName = userReader.GetString(0);
                                }
                            }
                        }
                    }

                    if (userInfo.userType == "LGU Staff")
                    {
                        string userSql = "SELECT firstName AS fullName " +
                                         "FROM lgu_profile_view " +
                                         "WHERE userID = @userID ";
                        using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
                        {
                            userCommand.Parameters.AddWithValue("@userID", userInfo.userID);
                            using (MySqlDataReader userReader = userCommand.ExecuteReader())
                            {
                                if (userReader.Read())
                                {
                                    profileInfo.fullName = userReader.GetString(0);
                                }
                            }
                        }
                    }

                    if (userInfo.userType == "EC Staff")
                    {
                        string userSql = "SELECT firstName AS fullName " +
                                         "FROM ec_profile_view " +
                                         "WHERE userID = @userID ";
                        using (MySqlCommand userCommand = new MySqlCommand(userSql, connection))
                        {
                            userCommand.Parameters.AddWithValue("@userID", userInfo.userID);
                            using (MySqlDataReader userReader = userCommand.ExecuteReader())
                            {
                                if (userReader.Read())
                                {
                                    profileInfo.fullName = userReader.GetString(0);
                                }
                            }
                        }
                    }

                    // Console.WriteLine($"Retrieved username [{username}] with type [{userInfo.userType}]");
                    successMessage = $"Hello, {profileInfo.fullName}!";
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
                Response.Redirect("/login/index?errorMessage=" + errorMessage);
            }
            else
            {
				Response.Redirect("/index?successMessage=" + successMessage);
            }
        }

        private void LogLoginSuccess(string userID, string userType, string username)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert log entry into table_log
                    string logSql = "INSERT INTO table_log (userID, tableName, tableID, logType, description, loggedAt) " +
                                    "VALUES (@userID, 'user', @userID, 'Login', CONCAT(@userType, ' ', @username, ' logged in'), current_timestamp())";

                    using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
                    {
                        logCommand.Parameters.AddWithValue("@userID", userID);
                        logCommand.Parameters.AddWithValue("@userType", userType);
                        logCommand.Parameters.AddWithValue("@username", username);
                        // logCommand.ExecuteNonQuery();

                        int rowsAffected = logCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Log entry inserted successfully
                            Console.WriteLine("Login log entry inserted successfully");
                        }
                        else
                        {
                            // Failed to insert log entry
                            Console.WriteLine("Failed to insert login log entry");
                        }
                    }
                }
            }   
            catch (Exception ex)
            {
                // Handle the exception or log an error message if logging fails
                Console.WriteLine($"Error inserting login log entry: {ex.Message}");
            }
        }
    }

    public class TableLogInfo
    {
        public string? logID { get; set; }
        public string? userID { get; set; }
        public string? tableName { get; set; }
        public string? tableID { get; set; }
        public string? logType { get; set; }
        public string? description { get; set; }
        public string? loggedAt { get; set; }
	}
}
