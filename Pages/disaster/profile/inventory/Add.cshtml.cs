using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.inventory
{
    public class AddModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public AddModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public InventoryInfo inventoryInfo { get; set; } = new InventoryInfo();
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

            string centerLogID = Request.Query["centerLogID"];

			if (string.IsNullOrEmpty(centerLogID))
			{
				errorMessage = "Invalid centerLogID.";
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName " +
									"FROM evacuation_center_log AS log " +
									"INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
									"INNER JOIN disaster AS d ON log.disasterID = d.disasterID " +
									"WHERE log.centerLogID = @centerLogID";
					using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
					{
						logCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader logReader = logCommand.ExecuteReader())
						{
							if (logReader.Read())
							{
								logInfo.centerLogID = logReader.GetString(0);
								logInfo.disasterID = logReader.GetString(1);
								logInfo.disasterName = logReader.GetString(2);
								logInfo.centerName = logReader.GetString(3);
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
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			// Retrieve item details from the form
			string centerLogID = Request.Form["centerLogID"];
			inventoryInfo.itemName = Request.Form["itemName"];
			inventoryInfo.itemType = Request.Form["itemType"];
			inventoryInfo.qty = Request.Form["qty"];
			inventoryInfo.unitMeasure = Request.Form["unitMeasure"];
			inventoryInfo.earliestExpiryDate = Request.Form["earliestExpiryDate"];
			inventoryInfo.remarks = Request.Form["remarks"];
			
			if (string.IsNullOrEmpty(inventoryInfo.earliestExpiryDate))
			{
				inventoryInfo.earliestExpiryDate = null;
			}

			if (string.IsNullOrEmpty(inventoryInfo.remarks))
			{
				inventoryInfo.remarks = null;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'inventory' table
					string sql = "INSERT INTO inventory " +
                                 "(centerLogID, itemName, itemType, qty, unitMeasure, earliestExpiryDate, remarks) " +
                                 "VALUES (@centerLogID, @itemName, @itemType, @qty, @unitMeasure, @earliestExpiryDate, @remarks)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						command.Parameters.AddWithValue("@itemName", inventoryInfo.itemName);
						command.Parameters.AddWithValue("@itemType", inventoryInfo.itemType);
						command.Parameters.AddWithValue("@qty", inventoryInfo.qty);
						command.Parameters.AddWithValue("@unitMeasure", inventoryInfo.unitMeasure);
						command.Parameters.AddWithValue("@earliestExpiryDate", inventoryInfo.earliestExpiryDate);
						command.Parameters.AddWithValue("@remarks", inventoryInfo.remarks);
						command.ExecuteNonQuery();
					}

                    // Retrieve the last inserted inventoryID
                    string? lastInsertID;
                    string lastInsertSql = "SELECT MAX(inventoryID) FROM inventory WHERE itemName = @itemName AND qty = @qty";
                    using (MySqlCommand lastInsertCommand = new MySqlCommand(lastInsertSql, connection))
                    {
                        lastInsertCommand.Parameters.AddWithValue("@itemName", inventoryInfo.itemName);
                        lastInsertCommand.Parameters.AddWithValue("@qty", inventoryInfo.qty);
                        lastInsertID = lastInsertCommand.ExecuteScalar()?.ToString();
                    }

                    // Update userID in table_log
                    UserId = HttpContext.Session.GetString("UserId");
                    string updateUserIdSql = "UPDATE table_log SET userID = @userID WHERE tableID = @tableID AND logType = 'Add'";
                    using (MySqlCommand updateUserIdCommand = new MySqlCommand(updateUserIdSql, connection))
                    {
                        updateUserIdCommand.Parameters.AddWithValue("@userID", UserId);
                        updateUserIdCommand.Parameters.AddWithValue("@tableID", lastInsertID);
                        updateUserIdCommand.ExecuteNonQuery();
                    }
                }

                successMessage = "Inventory item added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/inventory/add?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Disaster page after successful add
				Response.Redirect("/disaster/profile/inventory/index?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
			}
		}
	}
}
