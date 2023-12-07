using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public List<UserInfo> listUsers = new List<UserInfo>();
        public string successMessage = "";
        public string errorMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

        public void OnGet()
        {
            // Check if UserId is set in the session
            UserId = HttpContext.Session.GetString("UserId");
            UserType = HttpContext.Session.GetString("UserType");

            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(UserType))
            {
                // Redirect to the login page or handle unauthorized access
                Response.Redirect("/login/index");
                return;
            }

            if (UserType != "Admin")
            {
                // Redirect to the login page or handle unauthorized access
                Response.Redirect("/accessdenied");
                return;
            }

            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM user ORDER BY createdAt";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserInfo userInfo = new UserInfo();
                                userInfo.userID = reader.GetString(0);
                                userInfo.username = reader.GetString(1);
                                userInfo.password = reader.GetString(2);
                                userInfo.userType = reader.GetString(3);
                                userInfo.createdAt = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm:ss tt").Replace("am", "AM").Replace("pm", "PM");
                                listUsers.Add(userInfo);
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
	}

    public class UserInfo
    {
        public string userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
        public string createdAt { get; set; }
    }
}