using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class IndexModel : PageModel
    {
        public List<UserInfo> listUsers = new List<UserInfo>();

        public void OnGet()
        {
            try
            {
                String connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM user";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader()) 
                        {
                            while (reader.Read())
                            {
                                UserInfo userInfo = new UserInfo();
                                userInfo.userID = "" + reader.GetString(0);
                                userInfo.username = "" + reader.GetString(1);
                                userInfo.password = "" + reader.GetString(2);
                                userInfo.userType = "" + reader.GetString(3);
                                
                                // Convert the userPhoto BLOB data to a byte[]
                                if (!reader.IsDBNull(4))
                                {
                                    userInfo.userPhoto = (byte[])reader.GetValue(4);
                                }
                                
                                userInfo.createdAt = reader.GetDateTime(5).ToString();
                                listUsers.Add(userInfo);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }
    }

    public class UserInfo
    {
        public string userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
        public byte[] userPhoto { get; set; }
        public string createdAt { get; set; }
    }
}