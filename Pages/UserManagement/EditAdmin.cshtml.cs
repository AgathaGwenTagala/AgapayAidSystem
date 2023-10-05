using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class EditAdminModel : PageModel
    {
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string adminID { get; set; } = "";
        public string adminName { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            userID = Request.Query["userID"];

            // Fetch info of selected user from the database
            try
            {
                string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM admin where userID = @userID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                adminID = reader.GetString(0);
                                userID = reader.GetString(1);
                                adminName = reader.GetString(2);
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

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                return;
            }

            // Retrieve the userID and adminName from the form
            userID = Request.Form["userID"];
            adminName = Request.Form["adminName"];

            try
            {
                string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert updated data into the 'admin' table
                    string sql = "UPDATE admin " +
                                 "SET adminName = @adminName " +
                                 "WHERE userID = @userID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@adminName", adminName);
                        command.Parameters.AddWithValue("@userID", userID);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "Admin user updated successfully";
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/UserManagement/Index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }
    }
}
