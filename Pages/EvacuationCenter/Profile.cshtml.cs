using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AgapayAidSystem.Pages.EvacuationCenter
{
    public class EvacuationCenterProfileModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public EvacuationCenterProfileModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationInfo evacuationInfo = new EvacuationInfo();
		public List<DisasterInfo> listDisaster = new List<DisasterInfo>();
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

            string centerID = Request.Query["centerID"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch evacuation center data 
					string sql = "SELECT ec.*, b.barangayName " +
									"FROM evacuation_center AS ec " +
									"INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
									"WHERE centerID = @centerID ORDER BY centerName";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerID", centerID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								evacuationInfo.centerID = reader.GetString(0);
								evacuationInfo.centerName = reader.GetString(1);
								evacuationInfo.centerType = reader.GetString(2);
								evacuationInfo.streetAddress = reader.GetString(3);
								evacuationInfo.barangayID = reader.GetString(4);
								evacuationInfo.mobileNum = reader.IsDBNull(5) ? null : reader.GetString(5);
								evacuationInfo.telephoneNum = reader.IsDBNull(6) ? null : reader.GetString(6);
								evacuationInfo.maxCapacity = reader.GetInt32(7).ToString();
								evacuationInfo.status = reader.GetString(8);
								evacuationInfo.toilet = reader.GetInt32(9).ToString();
								evacuationInfo.bathingCubicle = reader.GetInt32(10).ToString();
								evacuationInfo.communityKitchen = reader.GetInt32(11).ToString();
								evacuationInfo.washingArea = reader.GetInt32(12).ToString();
								evacuationInfo.womenChildSpace = reader.GetInt32(13).ToString();
								evacuationInfo.multipurposeArea = reader.GetInt32(14).ToString();
								evacuationInfo.barangayName = reader.GetString(15);
							}
						}
					}

					// Fetch disaster log data

					string disaster_sql = "SELECT log.centerLogID, ec.centerID, d.disasterName, d.disasterType, d.description, d.dateOccured " +
										  "FROM evacuation_center_log log " +
										  "JOIN disaster d ON log.disasterID = d.disasterID " +
										  "JOIN evacuation_center ec ON log.centerID = ec.centerID " +
										  "WHERE ec.centerID = @centerID ORDER BY d.dateOccured";
					using (MySqlCommand disaster_command = new MySqlCommand(disaster_sql, connection))
					{
						disaster_command.Parameters.AddWithValue("@centerID", centerID);
						using (MySqlDataReader disaster_reader = disaster_command.ExecuteReader())
						{
							while (disaster_reader.Read())
							{
								DisasterInfo disasterInfo = new DisasterInfo();
								disasterInfo.centerLogID = disaster_reader.GetString(0);
								disasterInfo.centerID = disaster_reader.GetString(1);
								disasterInfo.disasterName = disaster_reader.GetString(2);
								disasterInfo.disasterType = disaster_reader.GetString(3);
								disasterInfo.description = disaster_reader.GetString(4);
								disasterInfo.dateOccured = disaster_reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
								listDisaster.Add(disasterInfo);
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
