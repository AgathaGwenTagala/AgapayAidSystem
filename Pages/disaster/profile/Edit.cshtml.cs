using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AgapayAidSystem.Pages.Disaster;

namespace AgapayAidSystem.Pages.disaster.profile
{
	public class EditModel : PageModel
	{
		private readonly IConfiguration _configuration;
		public EditModel(IConfiguration configuration) => _configuration = configuration;
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public List<string> DisasterTypes { get; set; }
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

            if (UserType == "EC Staff")
            {
                Response.Redirect("/accessdenied");
                return;
            }

            string disasterID = Request.Query["disasterID"];

			// Fetch the list of disaster types from your database
			DisasterTypes = GetDisasterTypesFromDatabase();

			// Fetch info of selected disaster from the database
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM disaster where disasterID = @disasterID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@disasterID", disasterID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								disasterInfo.disasterID = "" + reader.GetString(0);
								disasterInfo.disasterName = "" + reader.GetString(1);
								disasterInfo.disasterType = "" + reader.GetString(2);
								disasterInfo.description = "" + reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
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

		private List<string> GetDisasterTypesFromDatabase()
		{
			var disasterTypes = new List<string>();

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Query to fetch available disaster types from an ENUM column
					string sql = "SELECT COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'disaster' AND COLUMN_NAME = 'disasterType';";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								string columnType = reader.GetString(0);
								// Extract the ENUM values from the COLUMN_TYPE
								disasterTypes.AddRange(columnType.Replace("enum(", "").Replace(")", "").Replace("'", "").Split(','));
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			return disasterTypes;
		}

		public void OnPost()
		{
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			// Retrieve disaster details from the form
			disasterInfo.disasterID = Request.Form["disasterID"];
			disasterInfo.disasterName = Request.Form["disasterName"];
			disasterInfo.disasterType = Request.Form["disasterType"];
			disasterInfo.description = Request.Form["description"];
			disasterInfo.dateOccured = Request.Form["dateOccured"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert updated data into the 'disaster' table
					string sql = "UPDATE disaster " +
								 "SET disasterName = @disasterName, disasterType = @disasterType, " +
								 "description = @description, dateOccured = @dateOccured " +
								 "WHERE disasterID = @disasterID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@disasterName", disasterInfo.disasterName);
						command.Parameters.AddWithValue("@disasterType", disasterInfo.disasterType);
						command.Parameters.AddWithValue("@description", disasterInfo.description);
						command.Parameters.AddWithValue("@dateOccured", disasterInfo.dateOccured);
						command.Parameters.AddWithValue("@disasterID", disasterInfo.disasterID);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Disaster updated successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/index?disasterID=" + disasterInfo.disasterID + "&errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Entry Log page after successful allocation
				Response.Redirect("/disaster/profile/index?disasterID=" + disasterInfo.disasterID + "&successMessage=" + successMessage);
			}
		}
	}
}
