using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class CreateAdminModel : PageModel
    {
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string adminName { get; set; } = "";
		public string userID { get; set; } = "";
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet(string lastInsertedUserID)
		{
            // Retrieve and decode the userID from the query string
            string encodedUserID = HttpContext.Request.Query["userID"];
            userID = WebUtility.UrlDecode(encodedUserID);

            // Ensure that userID is not empty
            if (string.IsNullOrEmpty(lastInsertedUserID))
			{
				errorMessage = "UserID is missing.";
				return;
			}

			// Retrieve the userID passed from the Create page
			// userInfo.userID = lastInsertedUserID;

            
        }

		public void OnPost()
		{
            if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve the userID from the form
			string userID = Request.Form["userID"];
            string adminName = Request.Form["adminName"];

            // Check if userID is null or empty
            if (string.IsNullOrEmpty(userID))
			{
				errorMessage = "UserID is missing.";
				return;
			}

            try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'admin' table
					string sql = "INSERT INTO admin (userID, adminName) VALUES (@userID, @adminName);";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@userID", userID);
						command.Parameters.AddWithValue("@adminName", adminName);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Admin user created successfully!";
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/UserManagement/Index");
		}
	}
}