using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Users
{
    public class CreateModel : PageModel
    {
        public UserInfo userInfo = new UserInfo();
        public String errorMessage = "";
        public String successMessage = "";
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            userInfo.username = Request.Form["username"];
            userInfo.userType = Request.Form["userType"];

            if (userInfo.username.Length == 0 || userInfo.userType.Length == 0)
            {
                errorMessage = "All fields are required.";
                return;
            }

            //save the new user into the database
            try
            {
                string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=alsk1207;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO user " +
                                 "(username, userType) VALUES " +
                                 "(@username, @userType);";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", userInfo.username);
						command.Parameters.AddWithValue("@userType", userInfo.userType);

                        command.ExecuteNonQuery();
					}
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            userInfo.username = "";
            userInfo.userType = "";
            successMessage = "New user added successfully.";

            Response.Redirect("/Users/UsersIndex");
        }
    }
}
