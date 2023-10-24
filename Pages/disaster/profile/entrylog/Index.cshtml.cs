using AgapayAidSystem.Pages.Disaster;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.entrylog
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public List<EntryLogInfo> listEntryLog { get; set; } = new List<EntryLogInfo>();
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
        {
            string disasterID = Request.Query["disasterID"];
            string centerLogID = Request.Query["centerLogID"];

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch info of selected disaster from the database
                    string sql = "SELECT disasterID, disasterName FROM disaster where disasterID = @disasterID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                disasterInfo.disasterID = reader.GetString(0);
								disasterInfo.disasterName = reader.GetString(1);
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

					// Fetch entry log data related to the selected center log
					string entrySql = "SELECT e.*, CONCAT(mem.firstName, " +
									  "(CASE WHEN (mem.middleName IS NOT NULL) " +
									  "THEN CONCAT(' ', LEFT(mem.middleName, 1), '.') ELSE '' END), " +
									  "(CASE WHEN (mem.middleName IS NOT NULL) THEN ' ' ELSE ' ' END),  mem.lastName, " +
									  "(CASE WHEN (mem.extName IS NOT NULL) " +
									  "THEN CONCAT(' ', mem.extName) ELSE '' END)) AS fullName " +
									  "FROM entry_log e " +
									  "INNER JOIN family_member mem ON e.memberID = mem.memberID " +
									  "WHERE centerLogID = @centerLogID;";
					using (MySqlCommand entryCommand = new MySqlCommand(entrySql, connection))
					{
						entryCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader entryReader = entryCommand.ExecuteReader())
						{
							while (entryReader.Read())
							{
								EntryLogInfo entryInfo = new EntryLogInfo();
								entryInfo.entryLogID = entryReader.GetString(0);
								entryInfo.memberID = entryReader.GetString(1);
								entryInfo.centerLogID = entryReader.GetString(2);
								entryInfo.checkInDate = entryReader.GetDateTime(3).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								entryInfo.checkOutDate = entryReader.IsDBNull(4) ? null : entryReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								entryInfo.entryStatus = entryReader.GetString(5);
								entryInfo.fullName = entryReader.GetString(6);
								listEntryLog.Add(entryInfo);
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
				return;
			}

			if (selectedEvacuees == null || selectedEvacuees.Length == 0)
			{
				errorMessage = "Please select at least one evacuee to check-out.";
				return;
			}

			string centerLogID = Request.Form["centerLogID"];

			if (string.IsNullOrEmpty(centerLogID))
			{
				errorMessage = "Missing centerLogID";
				return;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					foreach (string entryLogID in selectedEvacuees)
					{
						string insertSql = "UPDATE entry_log " +
										   "SET checkOutDate = CURRENT_TIMESTAMP(), entryStatus = 'Check-out' " +
										   "WHERE entryLogID = @entryLogID";

						using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
						{
							insertCommand.Parameters.AddWithValue("@entryLogID", entryLogID);

							int rowsInserted = insertCommand.ExecuteNonQuery();
							if (rowsInserted == 1)
							{
								successMessage = "Evacuee checked-out.";
							}
							else
							{
								errorMessage = "Failed to check-out one or more evacuees.";
								return;
							}
						}
					}

					// If all selected centers were successfully allocated
					successMessage = "Selected evacuee(s) checked-out successfully!";
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

	public class EntryLogInfo
	{
		public string entryLogID { get; set; }
		public string memberID { get; set; }
		public string centerLogID { get; set; }
		public string checkInDate { get; set; }
		public string checkOutDate { get; set; }
		public string entryStatus { get; set; }
		public string fullName { get; set; }
	}
}
