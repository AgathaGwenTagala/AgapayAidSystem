using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class ReportModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ReportModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string disasterID = Request.Query["disasterID"];

            if (string.IsNullOrEmpty(disasterID))
            {
                errorMessage = "Invalid disaster ID.";
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Fetch info of selected disaster from the database
                    string disasterSql = "SELECT * FROM disaster where disasterID = @disasterID";
                    using (MySqlCommand disasterCommand = new MySqlCommand(disasterSql, connection))
                    {
                        disasterCommand.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader disasterReader = disasterCommand.ExecuteReader())
                        {
                            if (disasterReader.Read())
                            {
                                disasterInfo.disasterID = disasterReader.GetString(0);
                                disasterInfo.disasterName = disasterReader.GetString(1);
                                disasterInfo.disasterType = disasterReader.GetString(2);
                                disasterInfo.description = disasterReader.GetString(3);
                                disasterInfo.dateOccured = disasterReader.GetDateTime("dateOccured").ToString("MMMM dd, yyyy");
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
}
