using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages
{
    public class NotificationModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public NotificationModel(IConfiguration configuration) => _configuration = configuration;
        public List<NotificationInfo> listNotification = new List<NotificationInfo>();
        public NotificationInfo notificationInfo = new NotificationInfo();
        public string errorMessage = "";
        public string successMessage = "";
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

            string ecStaffID = "";

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string staffSql = "SELECT ecStaffID FROM ec_staff WHERE userID = @userID;";
                    using (MySqlCommand staffCommand = new MySqlCommand(staffSql, connection))
                    {
                        staffCommand.Parameters.AddWithValue("@userID", UserId);
                        using (MySqlDataReader staffReader = staffCommand.ExecuteReader())
                        {
                            if (staffReader.Read())
                            {
                                ecStaffID = staffReader.GetString(0);
                            }
                        }
                    }

                    string sql = "SELECT esn.*, d.disasterName, ec.centerName " +
                                 "FROM ec_staff_notification esn " +
                                 "JOIN evacuation_center_log log ON esn.centerLogID = log.centerLogID " +
                                 "JOIN disaster d ON log.disasterID = d.disasterID " +
                                 "JOIN evacuation_center ec ON log.centerID = ec.centerID " +
                                 "WHERE esn.ecStaffID = @ecStaffID " +
                                 "ORDER BY sentAt DESC";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ecStaffID", ecStaffID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                NotificationInfo notificationInfo = new NotificationInfo();
                                notificationInfo.notificationID = reader.GetString(0);
                                notificationInfo.ecStaffID = reader.GetString(1);
                                notificationInfo.centerLogID = reader.GetString(2);
                                notificationInfo.sentAt = reader.GetDateTime(3).ToString("yyyy-MM-dd hh:mm tt");
                                notificationInfo.readAt = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt");
                                notificationInfo.isRead = reader.GetString(5);
                                notificationInfo.disasterName = reader.GetString(6);
                                notificationInfo.centerName = reader.GetString(7);
                                listNotification.Add(notificationInfo);
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

    public class NotificationInfo
    {
        public string? notificationID { get; set; }
        public string? ecStaffID { get; set; }
        public string? userID { get; set; }
        public string? centerLogID { get; set; }        
        public string? sentAt { get; set; }
        public string? readAt { get; set; }
        public string? isRead { get; set; }
        public string? disasterName { get; set; }
        public string? centerName { get; set; }
    }
}
