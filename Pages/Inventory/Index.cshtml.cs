using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public List<InventoryInfo> listInventory = new List<InventoryInfo>();
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

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM all_inventory_view;";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                InventoryInfo inventoryInfo = new InventoryInfo();
                                inventoryInfo.inventoryID= reader.GetString(0);
                                inventoryInfo.centerLogID= reader.GetString(1);
                                inventoryInfo.itemName = reader.GetString(2);
                                inventoryInfo.itemType = reader.GetString(3);
                                inventoryInfo.qty = reader.GetString(4);
                                inventoryInfo.unitMeasure = reader.GetString(5);
								inventoryInfo.earliestExpiryDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6).ToString("yyyy-MM-dd");
								inventoryInfo.remarks = reader.IsDBNull(7) ? "-" : reader.GetString(7);
								inventoryInfo.disasterName = reader.GetString(8);
								inventoryInfo.centerName = reader.GetString(9);
								inventoryInfo.remainingQty = reader.GetInt32(10).ToString();
								listInventory.Add(inventoryInfo);
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

    public class InventoryInfo
    {
        public string? inventoryID { get; set; }
        public string? disasterName { get; set; }
        public string? centerLogID { get; set; }
        public string? centerName { get; set; }
        public string? itemName { get; set; }
        public string? itemType { get; set; }
        public string? qty { get; set; }
        public string? unitMeasure { get; set; }
        public string? earliestExpiryDate { get; set; }
        public string? remarks { get; set; }
        public string? remainingQty { get; set; }
    }
}
