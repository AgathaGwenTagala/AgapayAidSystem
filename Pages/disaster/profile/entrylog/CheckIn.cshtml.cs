using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.entrylog
{
    public class CheckInModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public CheckInModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public List<LatestEntryInfo> listLatestEntry = new List<LatestEntryInfo>();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			string centerLogID = Request.Query["centerLogID"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch family members who either have no records in the entry_log table
					// or have the latest entryStatus as 'Check-out'
					string sql = "SELECT * FROM member_latest_entry_log";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								LatestEntryInfo latestEntryInfo = new LatestEntryInfo();
								latestEntryInfo.memberID = reader.GetString(0);
								latestEntryInfo.fullName = reader.GetString(1);
								latestEntryInfo.sex = reader.GetString(2);
								latestEntryInfo.birthdate = reader.GetDateTime(3).ToString("MMMM d, yyyy");
								latestEntryInfo.barangayName = reader.GetString(4);
								latestEntryInfo.entryStatus = reader.IsDBNull(5) ? null : reader.GetString(5);
								listLatestEntry.Add(latestEntryInfo);
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

        public void OnPost(string[] selectedEvacuees)
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
            }

            if (selectedEvacuees == null || selectedEvacuees.Length == 0)
            {
                errorMessage = "Please select at least one evacuee to check-out.";
            }

            string centerLogID = Request.Form["centerLogID"];

            if (string.IsNullOrEmpty(centerLogID))
            {
                errorMessage = "Missing centerLogID";
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (string memberID in selectedEvacuees)
                    {
                        string insertSql = "INSERT INTO entry_log (memberID, centerLogID) " +
                                           "VALUES (@memberID, @centerLogID)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@memberID", memberID);
                            insertCommand.Parameters.AddWithValue("@centerLogID", centerLogID);

                            int rowsInserted = insertCommand.ExecuteNonQuery();
                            if (rowsInserted == 1)
                            {
                                successMessage = "Evacuee checked-in.";
                            }
                            else
                            {
                                errorMessage = "Failed to check-in one or more evacuees.";
                            }
                        }
                    }

                    // If all selected centers were successfully allocated
                    successMessage = "Selected evacuees checked-in successfully!";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            // Redirect to the Entry Log page after check-out or encountering an error
            Response.Redirect("/disaster/profile/entrylog/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage + "&successMessage=" + successMessage);
        }
    }

	public class LatestEntryInfo
	{
		public string memberID { get; set; }
		public string fullName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string barangayName { get; set; }
		public string entryStatus { get; set; }
	}
}
