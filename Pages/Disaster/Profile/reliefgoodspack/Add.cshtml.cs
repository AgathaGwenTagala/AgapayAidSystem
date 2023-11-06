using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile.reliefgoodspack
{
    public class AddModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public List<AvailableInventoryInfo> listAvInventory = new List<AvailableInventoryInfo>();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
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

                    // Fetch info of available inventory items of selected center log
                    string sql = "SELECT * FROM available_inventory_item_view WHERE centerLogID = @centerLogID;";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AvailableInventoryInfo avInventoryInfo = new AvailableInventoryInfo();
                                avInventoryInfo.inventoryID = reader.GetString(0);
                                avInventoryInfo.centerLogID = reader.GetString(1);
                                avInventoryInfo.itemName = reader.GetString(2);
                                avInventoryInfo.itemType = reader.GetString(3);
                                avInventoryInfo.qty = reader.GetInt32(4).ToString();
                                avInventoryInfo.unitMeasure = reader.GetString(5);
                                avInventoryInfo.createdAt = reader.GetDateTime(6).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                avInventoryInfo.remarks = reader.IsDBNull(7) ? null : reader.GetString(5);
                                avInventoryInfo.packedQty = reader.GetInt32(8).ToString();
                                avInventoryInfo.remainingQty = reader.GetInt32(9).ToString();
                                listAvInventory.Add(avInventoryInfo);
                            }
                        }
                    }

                    // Fetch info of selected center log from the database
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

        public void OnPost(string[] selectedInclusion, string[] qty)
        {
            bool errorOccurred = false;

            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                errorOccurred = true;
            }

            if (selectedInclusion == null || selectedInclusion.Length == 0)
            {
                errorMessage = "Please select at least one evacuee to check-out.";
                errorOccurred = true;
            }

            string centerLogID = Request.Form["centerLogID"];
            string packQty = Request.Form["packQty"];

            if (string.IsNullOrEmpty(centerLogID))
            {
                errorMessage = "Missing centerLogID";
                errorOccurred = true;
            }
            
            if (string.IsNullOrEmpty(packQty))
            {
                errorMessage = "Missing packQty";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Step 1: Insert into the 'pack' table
                    string insertPackSql = "INSERT INTO pack (centerLogID, packQty) VALUES (@centerLogID, @packQty)";
                    using (MySqlCommand insertPackCommand = new MySqlCommand(insertPackSql, connection))
                    {
                        insertPackCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        insertPackCommand.Parameters.AddWithValue("@packQty", packQty);
                        insertPackCommand.ExecuteNonQuery();
                        // Console.WriteLine("Successfully inserted into 'pack' table.");
                    }

                    // Step 2: Retrieve the last inserted packID from the 'pack' table
                    string newPackID;
                    string selectMaxPackIDSql = "SELECT MAX(packID) FROM pack WHERE packQty = @packQty";
                    using (MySqlCommand selectMaxPackID = new MySqlCommand(selectMaxPackIDSql, connection))
                    {
                        selectMaxPackID.Parameters.AddWithValue("@packQty", packQty);
                        newPackID = selectMaxPackID.ExecuteScalar()?.ToString();
                    }
                    // Console.WriteLine($"Retrieved new packID: {newPackID} with packQty {packQty}");

                    // Step 3: Loop through inclusions and insert into the 'pack_inclusion' table
                    for (int i = 0; i < selectedInclusion.Length; i++)
                    {
                        string inventoryID = selectedInclusion[i];
                        string qtyInput = qty[i]; // Retrieve the qty for the current inclusion

                        string insertSql = "INSERT INTO pack_inclusion (inventoryID, packID, qty) " +
                                           "VALUES (@inventoryID, @packID, @qty)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@inventoryID", inventoryID);
                            insertCommand.Parameters.AddWithValue("@packID", newPackID);
                            insertCommand.Parameters.AddWithValue("@qty", qtyInput);
                            int rowsInserted = insertCommand.ExecuteNonQuery();

                            if (rowsInserted == 1)
                            {
                                Console.WriteLine($"Successfully inserted into 'pack_inclusion' table for inventoryID: {inventoryID}");
                            }
                            else
                            {
                                errorMessage = "Failed to add one or more pack inclusions.";
                                errorOccurred = true;
                                break;
                            }
                        }
                    }

                    // If pack and all selected pack inclusions were successfully added
                    successMessage = "Relief goods pack added successfully!";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/disaster/profile/reliefgoodspack/add?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Relief Goods Pack page after successful add
                Response.Redirect("/disaster/profile/reliefgoodspack/index?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
            }
        }
    }

    public class AvailableInventoryInfo
    {
        public string inventoryID { get; set; }
        public string centerLogID { get; set; }
        public string itemName { get; set; }
        public string itemType { get; set; }
        public string qty { get; set; }
        public string unitMeasure { get; set; }
        public string createdAt { get; set; }
        public string remarks { get; set; }
        public string packedQty { get; set; }
        public string remainingQty { get; set; }
    }
}
