using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.UserManagement.LGUStaff
{
    public class EditModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public EditModel(IConfiguration configuration) => _configuration = configuration;
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string lguStaffID { get; set; } = "";
        public string firstName { get; set; } = "";
        public string middleName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string extName { get; set; } = "";
        public string sex { get; set; } = "";
        public string birthdate { get; set; } = "";
        public string mobileNum { get; set; } = "";
        public string emailAddress { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            userID = Request.Query["userID"];

            // Fetch info of selected user from the database
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM lgu_staff where userID = @userID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lguStaffID = reader.GetString(0);
                                userID = reader.GetString(1);
                                firstName = reader.GetString(2);
                                middleName = reader.IsDBNull(3) ? null : reader.GetString(3);
                                lastName = reader.GetString(4);
                                extName = reader.IsDBNull(5) ? null : reader.GetString(5);
                                sex = reader.GetString(6);
                                birthdate = reader.GetDateTime("birthdate").ToString("yyyy-MM-dd");
                                mobileNum = reader.GetString(8);
                                emailAddress = reader.GetString(9);
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
            firstName = Request.Form["firstName"];
            middleName = Request.Form["middleName"];
            lastName = Request.Form["lastName"];
            extName = Request.Form["extName"];
            sex = Request.Form["sex"];
            birthdate = Request.Form["birthdate"];
            mobileNum = Request.Form["mobileNum"];
            emailAddress = Request.Form["emailAddress"];

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert updated data into the 'lgu_staff' table
                    string sql = "UPDATE lgu_staff " +
                                 "SET firstName = @firstName, middleName = @middleName, lastName = @lastName, extName = @extName, " +
                                 "sex = @sex, birthdate = @birthdate, mobileNum = @mobileNum, emailAddress = @emailAddress " +
                                 "WHERE userID = @userID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstName", firstName);
                        command.Parameters.AddWithValue("@middleName", middleName);
                        command.Parameters.AddWithValue("@lastName", lastName);
                        command.Parameters.AddWithValue("@extName", extName);
                        command.Parameters.AddWithValue("@sex", sex);
                        command.Parameters.AddWithValue("@birthdate", birthdate);
                        command.Parameters.AddWithValue("@mobileNum", mobileNum);
                        command.Parameters.AddWithValue("@emailAddress", emailAddress);
                        command.Parameters.AddWithValue("@userID", userID);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "LGU staff user updated successfully!";
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/usermanagement/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }
    }
}
