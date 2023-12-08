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
        public string UserId { get; set; }
        public string UserType { get; set; }

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
    }
}
