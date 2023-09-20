using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Users
{
    public class IndexModel : PageModel
    {
        public List<UserInfo> listUsers = new List<UserInfo>();
        public void OnGet()
        {
            try
            {
                string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=alsk1207;";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM user";
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
								userInfo.createdAt = reader.GetDateTime(4).ToString();

								/** Convert the userPhoto BLOB data to a byte[]
                                if (!reader.IsDBNull(4))
                                {
                                    userInfo.userPhoto = (byte[])reader.GetValue(4);
<<<<<<<< HEAD:Pages/Users/UsersIndex.cshtml.cs
                                }**/
========
                                }
                                userInfo.createdAt = reader.GetDateTime(5).ToString();
>>>>>>>> ca9cba49272f2106b726ad178d5f27d5244a152f:Pages/Users/Users.cshtml.cs

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
<<<<<<<< HEAD:Pages/Users/UsersIndex.cshtml.cs
========
        public byte[] userPhoto { get; set; }
>>>>>>>> ca9cba49272f2106b726ad178d5f27d5244a152f:Pages/Users/Users.cshtml.cs
        public string createdAt { get; set; }
    }
}
