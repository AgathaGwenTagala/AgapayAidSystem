using AgapayAidSystem.Pages.Disaster;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgapayAidSystem.Pages
{
    public class IndexModel : PageModel
    {
		private readonly ILogger<IndexModel> _logger;
		private readonly IConfiguration _configuration;
		public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public List<EvacuationCenterLogInfo> listCenterLog { get; set; } = new List<EvacuationCenterLogInfo>();
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

					// Fetch info of selected disaster from the database
					string sql = "SELECT * FROM disaster ORDER BY dateOccured DESC LIMIT 1;";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								disasterInfo.disasterID = reader.GetString(0);
								disasterInfo.disasterName = reader.GetString(1);
								disasterInfo.disasterType = reader.GetString(2);
								disasterInfo.description = reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("MMMM dd, yyyy");
							}
						}
					}
					string disasterID = disasterInfo.disasterID;

                    // Fetch evacuation center log data related to the selected disaster
                    string logSql = "SELECT log.*, ec.centerName, ec.maxCapacity, b.barangayName, " +
									"(SELECT COUNT(assignmentID) FROM ec_staff_assignment " +
									"WHERE centerLogID = log.centerLogID AND status = 'Assigned') AS totalStaff " +
									"FROM evacuation_center_log AS log " +
									"INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
									"INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
									"WHERE log.disasterID = @disasterID ORDER BY openingDateTime";
					using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
					{
						logCommand.Parameters.AddWithValue("@disasterID", disasterID);
						using (MySqlDataReader logReader = logCommand.ExecuteReader())
						{
							while (logReader.Read())
							{
								EvacuationCenterLogInfo logInfo = new EvacuationCenterLogInfo();
								logInfo.centerLogID = logReader.GetString(0);
								logInfo.disasterID = logReader.GetString(1);
								logInfo.centerID = logReader.GetString(2);
								logInfo.occupancy = logReader.GetInt32(3).ToString();
								logInfo.openingDateTime = logReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								logInfo.closingDateTime = logReader.IsDBNull(5) ? null : logReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								logInfo.status = logReader.GetString(6);
								logInfo.centerName = logReader.GetString(7);
								logInfo.maxCapacity = logReader.GetInt32(8).ToString();
								logInfo.barangayName = logReader.GetString(9);
								logInfo.totalStaff = logReader.GetInt32(10).ToString();
								listCenterLog.Add(logInfo);
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

        public int GetTotalDistinctEvacueesCount(string disasterID)
        {
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctEvacueesPerDisaster " +
                         "FROM disaster d " +
                         "LEFT JOIN evacuation_center_log ecl ON d.disasterID = ecl.disasterID " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE d.disasterID = @disasterID;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalCheckInCount(string disasterID)
        {
            string sql = "SELECT COUNT(e.entryLogID) AS totalEvacuees " +
                         "FROM entry_log e " +
                         "JOIN evacuation_center_log el ON e.centerLogID = el.centerLogID " +
                         "WHERE el.disasterID = @disasterID AND e.entryStatus = 'Check-in'";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalCheckOutCount(string disasterID)
        {
            string sql = "SELECT COUNT(e.entryLogID) AS totalEvacuees " +
                         "FROM entry_log e " +
                         "JOIN evacuation_center_log el ON e.centerLogID = el.centerLogID " +
                         "WHERE el.disasterID = @disasterID AND e.entryStatus = 'Check-out'";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalCenterCount(string disasterID)
        {
            string sql = "SELECT COUNT(centerLogID) AS totalCenter " +
                         "FROM evacuation_center_log " +
                         "WHERE disasterID = @disasterID";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalOpenCenterCount(string disasterID)
        {
            string sql = "SELECT COUNT(centerLogID) AS totalOpenCenter " +
                         "FROM evacuation_center_log " +
                         "WHERE disasterID = @disasterID AND status = 'Open'";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalClosedCenterCount(string disasterID)
        {
            string sql = "SELECT COUNT(centerLogID) AS totalClosedCenter " +
                         "FROM evacuation_center_log " +
                         "WHERE disasterID = @disasterID AND status = 'Closed'";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalDistinctBarangaysCount(string disasterID)
        {
            string sql = "SELECT COUNT(DISTINCT fm.familyID) AS totalDistinctBarangaysPerDisaster " +
                         "FROM disaster d " +
                         "LEFT JOIN evacuation_center_log ecl ON d.disasterID = ecl.disasterID " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "LEFT JOIN family f ON fm.familyID = f.familyID " +
                         "WHERE d.disasterID = @disasterID;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalDistinctFamiliesCount(string disasterID)
        {
            string sql = "SELECT COUNT(DISTINCT fm.familyID) AS totalDistinctFamiliesPerDisaster " +
                         "FROM disaster d " +
                         "LEFT JOIN evacuation_center_log ecl ON d.disasterID = ecl.disasterID " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE d.disasterID = @disasterID;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalDistinctFemaleCount(string disasterID)
        {
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctFemalePerDisaster " +
                         "FROM disaster d " +
                         "LEFT JOIN evacuation_center_log ecl ON d.disasterID = ecl.disasterID " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE d.disasterID = @disasterID AND fm.sex = 'F';";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }

        public int GetTotalDistinctMaleCount(string disasterID)
        {
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctMalePerDisaster " +
                         "FROM disaster d " +
                         "LEFT JOIN evacuation_center_log ecl ON d.disasterID = ecl.disasterID " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE d.disasterID = @disasterID AND fm.sex = 'M';";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return 0;
            }
        }
    }
}