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
                    string sql = "select i.*, d.disasterName, ec.centerName from inventory i " +
                                 "join evacuation_center_log log ON i.centerLogID = log.centerLogID " +
                                 "join disaster d ON log.disasterID = d.disasterID " +
                                 "join evacuation_center ec ON log.centerID = ec.centerID;";
                    
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                InventoryInfo inventoryInfo = new InventoryInfo();

                                inventoryInfo.inventoryID= reader.GetString(0);
                                inventoryInfo.disasterName = reader.GetString(1);
                                inventoryInfo.centerLogID = reader.GetString(2);
                                inventoryInfo.centerName = reader.GetString(3);
                                inventoryInfo.itemName = reader.GetString(4);
                                inventoryInfo.itemType = reader.GetString(5);
                                inventoryInfo.qty = reader.GetString(6);
                                inventoryInfo.unitMeasure = reader.GetString(7);
                                inventoryInfo.remarks = reader.IsDBNull(8) ? null : reader.GetString(8);

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
    }
}
