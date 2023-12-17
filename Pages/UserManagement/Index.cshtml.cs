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

            if (UserType != "Admin")
            {
                Response.Redirect("/accessdenied");
                return;
            }

            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT u.*, log.lastLogin " +
                                 "FROM user u " +
                                 "LEFT JOIN (SELECT userID, MAX(loggedAt) AS lastLogin " +
                                 "FROM table_log " +
                                 "WHERE logType = 'Login' " +
                                 "GROUP BY userID, logType) log ON u.userID = log.userID " +
                                 "ORDER BY u.createdAt;";
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
                                //userInfo.createdAt = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();
                                //userInfo.lastLogin = reader.IsDBNull(5) ? "-" : reader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();
                                
                                // Convert createdAt to UTC+8
                                DateTime createdAtUtc = reader.GetDateTime(4);
                                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"); // Use the time zone for UTC+8
                                userInfo.createdAt = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzInfo).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();

                                // Convert lastLogin to UTC+8
                                DateTime? lastLoginUtc = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                                userInfo.lastLogin = lastLoginUtc.HasValue
                                    ? TimeZoneInfo.ConvertTimeFromUtc(lastLoginUtc.Value, tzInfo).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper()
                                    : "-";
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
        public string? userID { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? userType { get; set; }
        public string? createdAt { get; set; }
        public string? lastLogin { get; set; }
    }
}