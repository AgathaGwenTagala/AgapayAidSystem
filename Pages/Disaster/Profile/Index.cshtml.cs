using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration) => _configuration = configuration;
        public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
        public List<EvacuationCenterLogInfo> listCenterLog { get; set; } = new List<EvacuationCenterLogInfo>();
		public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string disasterID = Request.Query["disasterID"];

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch info of selected disaster from the database
                    string sql = "SELECT * FROM disaster where disasterID = @disasterID";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@disasterID", disasterID);
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

		public int GetTotalDistinctEvacueesCount()
		{
			string disasterID = Request.Query["disasterID"];
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

		public int GetTotalCheckInCount()
		{
			string disasterID = Request.Query["disasterID"];
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

		public int GetTotalCheckOutCount()
		{
			string disasterID = Request.Query["disasterID"];
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

        public int GetTotalCenterCount()
        {
            string disasterID = Request.Query["disasterID"];
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

        public int GetTotalOpenCenterCount()
        {
            string disasterID = Request.Query["disasterID"];
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

        public int GetTotalClosedCenterCount()
        {
            string disasterID = Request.Query["disasterID"];
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

        public int GetTotalDistinctBarangaysCount()
        {
            string disasterID = Request.Query["disasterID"];
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

        public int GetTotalDistinctFamiliesCount()
        {
            string disasterID = Request.Query["disasterID"];
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

        public int GetTotalDistinctFemaleCount()
		{
			string disasterID = Request.Query["disasterID"];
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

		public int GetTotalDistinctMaleCount()
		{
			string disasterID = Request.Query["disasterID"];
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

        public int GetNotificationCount(string centerLogID)
        {
            string sql = "SELECT SUM(total) AS grandTotal FROM (" +
                         "SELECT COUNT(*) AS total FROM inventory_item_view WHERE centerLogID = @centerLogID AND remainingQty > 0 " +
                         "UNION ALL " +
                         "SELECT COUNT(*) AS total FROM pack WHERE centerLogID = @centerLogID AND status = 'Packed' " +
                         "UNION ALL " +
                         "SELECT COUNT(*) AS total FROM distinct_family_head_view WHERE centerLogID = @centerLogID " +
                         ") AS subquery;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@centerLogID", centerLogID);
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

	public class EvacuationCenterLogInfo
    {
        public string centerLogID { get; set; }
        public string disasterID { get; set; }
        public string centerID { get; set; }
        public string occupancy { get; set; }
        public string openingDateTime { get; set; }
        public string closingDateTime { get; set; }
        public string status { get; set; }
        public string disasterName { get; set; }
        public string centerName { get; set; }
        public string maxCapacity { get; set; }
        public string fullAddress { get; set; }
        public string barangayName { get; set; }
        public string totalStaff { get; set; }
        public string toilet { get; set; }
        public string bathingCubicle { get; set; }
        public string communityKitchen { get; set; }
        public string washingArea { get; set; }
        public string womenChildSpace { get; set; }
        public string multipurposeArea { get; set; }
        public string centerType { get; set; }
        public string mobileNum { get; set; }
        public string telephoneNum { get; set; }
    }
}
