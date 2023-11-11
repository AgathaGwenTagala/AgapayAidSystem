using AgapayAidSystem.Pages.Family.Profile.Add;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Profile.Edit
{
    public class FamilyModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public FamilyModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
        public List<BarangayInfo> Barangays { get; set; }
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string familyID = Request.Query["familyID"];

            // Fetch the list of barangays from database
            Barangays = GetBarangaysFromDatabase();

            // Fetch info of selected evacuation center from the database
            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM family where familyID = @familyID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@familyID", familyID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                familyInfo.familyID = reader.GetString(0);
                                familyInfo.streetAddress = reader.GetString(1);
                                familyInfo.barangayID = reader.GetString(2);
                                familyInfo.livingInGida = reader.GetString(5);
                                familyInfo.serialNum = reader.GetString(7);
                                familyInfo.beneficiary = reader.GetString(6);
                                familyInfo.mobileNum = reader.IsDBNull(3) ? null : reader.GetString(3);
                                familyInfo.telephoneNum = reader.IsDBNull(4) ? null : reader.GetString(4);
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

        private List<BarangayInfo> GetBarangaysFromDatabase()
        {
            var barangayData = new List<BarangayInfo>();

            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT barangayID, barangayName FROM barangay ORDER BY barangayName";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                barangayData.Add(new BarangayInfo
                                {
                                    barangayID = reader.GetString(0),
                                    barangayName = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return barangayData;
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                return;
            }

            // Retrieve evacuation center details from the form
            familyInfo.familyID = Request.Form["familyID"];
            familyInfo.streetAddress = Request.Form["streetAddress"];
            familyInfo.barangayID = Request.Form["barangayID"];
            familyInfo.livingInGida = Request.Form["livingInGida"];
            familyInfo.serialNum = Request.Form["serialNum"];
            familyInfo.beneficiary = Request.Form["beneficiary"];
            familyInfo.mobileNum = Request.Form["mobileNum"];
            familyInfo.telephoneNum = Request.Form["telephoneNum"];

            // Check if mobileNum is empty and set it to null
            if (string.IsNullOrWhiteSpace(familyInfo.mobileNum))
            {
                familyInfo.mobileNum = null;
            }

            // Check if telephoneNum is empty and set it to null
            if (string.IsNullOrWhiteSpace(familyInfo.telephoneNum))
            {
                familyInfo.telephoneNum = null;
            }

            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert updated data into the 'evacuation_center' table
                    string sql = "UPDATE family " +
                                 "SET streetAddress = @streetAddress, barangayID = @barangayID, " +
                                 "livingInGida = @livingInGida, serialNum = @serialNum, beneficiary = @beneficiary, mobileNum = @mobileNum, telephoneNum = @telephoneNum" +
                                 "WHERE familyID = @familyID";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@familyID", familyInfo.familyID);
                        command.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
                        command.Parameters.AddWithValue("@barangayID", familyInfo.barangayID);
                        command.Parameters.AddWithValue("@livingInGida", familyInfo.livingInGida);
                        command.Parameters.AddWithValue("@serialNum", familyInfo.serialNum);
                        command.Parameters.AddWithValue("@beneficiary", familyInfo.beneficiary);
                        command.Parameters.AddWithValue("@mobileNum", familyInfo.mobileNum);
                        command.Parameters.AddWithValue("@telephoneNum", familyInfo.telephoneNum);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "Family updated successfully!";
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/family/profile/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }
    }
}
