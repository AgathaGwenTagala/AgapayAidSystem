using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.account
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public List<AssignmentInfo> listAssignment = new List<AssignmentInfo>();
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
                        string sql = "SELECT * FROM assignment_history_view " +
                                     "WHERE userID = @userID " +
                                     "ORDER BY assignmentDate";
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@userID", UserId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    AssignmentInfo assignmentInfo = new AssignmentInfo();
                                    assignmentInfo.assignmentID = reader.GetString(0);
                                    assignmentInfo.centerLogID = reader.GetString(1);
                                    assignmentInfo.ecStaffID = reader.GetString(2);
                                    assignmentInfo.role = reader.GetString(3);
                                    assignmentInfo.assignmentDate = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                    assignmentInfo.completionDate = reader.IsDBNull(5) ? "-" : reader.GetDateTime(5).ToString("yyyy-MM-dd  hh:mm tt").ToUpper();
                                    assignmentInfo.status = reader.GetString(6);
                                    assignmentInfo.userID = reader.GetString(7);
                                    assignmentInfo.disasterName = reader.GetString(8);
                                    assignmentInfo.centerName = reader.GetString(9);
                                    listAssignment.Add(assignmentInfo);
                                }
                            }
                        }

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

                    if (UserType == "Admin")
                    {
                        string userSql = "SELECT * FROM admin_profile_view " +
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
                }
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
    }

    public class AssignmentInfo
    {
        public string assignmentID { get; set; }
        public string centerLogID { get; set; }
        public string ecStaffID { get; set; }
        public string role { get; set; }
        public string assignmentDate { get; set; }
        public string completionDate { get; set; }
        public string status { get; set; }
        public string userID { get; set; }
        public string disasterName { get; set; }
        public string centerName { get; set; }
    }

    public class ProfileInfo
    {
        public string userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
        public string createdAt { get; set; }
        public string adminID { get; set; }
        public string lguStaffID { get; set; }
        public string ecStaffID { get; set; }
        public string adminName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string extName { get; set; }
        public string fullName { get; set; }
        public string sex { get; set; }
        public string birthdate { get; set; }
        public string mobileNum { get; set; }
        public string emailAddress { get; set; }
        public string availabilityStatus { get; set; }
    }
}