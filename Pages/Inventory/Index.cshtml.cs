using AgapayAidSystem.Pages.disaster.profile.inventory;
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

        public void OnGet()
        {
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
                                inventoryInfo.remarks = reader.IsDBNull(6) ? null : reader.GetString(6);
								inventoryInfo.disasterName = reader.GetString(7);
								inventoryInfo.centerName = reader.GetString(8);
								inventoryInfo.remainingQty = reader.GetInt32(9).ToString();
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
        public string inventoryID { get; set; }
        public string disasterName { get; set; }
        public string centerLogID { get; set; }
        public string centerName { get; set; }
        public string itemName { get; set; }
        public string itemType { get; set; }
        public string qty { get; set; }
        public string unitMeasure { get; set; }
        public string remarks { get; set; }
        public string remainingQty { get; set; }
    }
}
