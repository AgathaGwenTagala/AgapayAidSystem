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
        public string successMessage = "";
        public string errorMessage = "";

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
                                userInfo.createdAt = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm:ss tt").Replace("am", "AM").Replace("pm", "PM");

                                // Set user information in the session
                                HttpContext.Session.SetString("UserId", userInfo.userID);
                                HttpContext.Session.SetString("UserType", userInfo.userType);
                            }
                        }
                        
                        Console.WriteLine($"Retrieved username [{username}] with type [{userInfo.userType}]");
                    }
                }

                successMessage = $"Hello, {userInfo.username}!";
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

    }
}
