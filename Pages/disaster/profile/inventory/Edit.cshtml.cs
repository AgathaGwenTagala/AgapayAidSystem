using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.inventory
{
    public class EditModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public EditModel(IConfiguration configuration) => _configuration = configuration;		
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

            string inventoryID = Request.Query["inventoryID"];	

			if (string.IsNullOrEmpty(inventoryID))
			{
				errorMessage = "Invalid inventoryID.";
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					// Fetch inventory item data related to the selected center log
					string inventorySql = "SELECT * FROM inventory WHERE inventoryID = @inventoryID;";
					using (MySqlCommand inventoryCommand = new MySqlCommand(inventorySql, connection))
					{
						inventoryCommand.Parameters.AddWithValue("@inventoryID", inventoryID);
						using (MySqlDataReader inventoryReader = inventoryCommand.ExecuteReader())
						{
							if (inventoryReader.Read())
							{
								inventoryInfo.inventoryID = inventoryReader.GetString(0);
								inventoryInfo.centerLogID = inventoryReader.GetString(1);
								inventoryInfo.itemName = inventoryReader.GetString(2);
								inventoryInfo.itemType = inventoryReader.GetString(3);
								inventoryInfo.qty = inventoryReader.GetInt32(4).ToString();
								inventoryInfo.unitMeasure = inventoryReader.GetString(5);
								inventoryInfo.remarks = inventoryReader.IsDBNull(6) ? null : inventoryReader.GetString(6);
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
			string inventoryID = Request.Form["inventoryID"];
			inventoryInfo.itemName = Request.Form["itemName"];
			inventoryInfo.itemType = Request.Form["itemType"];
			inventoryInfo.qty = Request.Form["qty"];
			inventoryInfo.unitMeasure = Request.Form["unitMeasure"];
			inventoryInfo.remarks = Request.Form["remarks"];

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
					string sql = "UPDATE inventory " +
								 "SET itemName = @itemName, itemName = @itemName, qty = @qty, " +
								 "unitMeasure = @unitMeasure, remarks = @remarks " +
								 "WHERE inventoryID = @inventoryID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@inventoryID", inventoryID);
						command.Parameters.AddWithValue("@itemName", inventoryInfo.itemName);
						command.Parameters.AddWithValue("@itemType", inventoryInfo.itemType);
						command.Parameters.AddWithValue("@qty", inventoryInfo.qty);
						command.Parameters.AddWithValue("@unitMeasure", inventoryInfo.unitMeasure);
						command.Parameters.AddWithValue("@remarks", inventoryInfo.remarks);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Inventory item edited successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/inventory/edit?inventoryID=" + inventoryID + "&errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Disaster page after successful add
				Response.Redirect("/disaster/profile/inventory/index?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
			}
		}
	}
}
