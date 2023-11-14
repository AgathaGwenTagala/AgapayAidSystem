using AgapayAidSystem.Pages.Disaster.Profile;
using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.informationboard
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
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
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName, " +
                                    "ec.toilet, ec.bathingCubicle, ec.communityKitchen, ec.washingArea, " +
                                    "ec.womenChildSpace, ec.multipurposeArea, " +
                                    "CONCAT(ec.streetAddress, ', Brgy. ', b.barangayName, ', ', " +
                                    "b.municipalityCity, ', ', b.province) AS fullAddress, " +
                                    "ec.centerType, ec.mobileNum, ec.telephoneNum, " +
                                    "log.openingDateTime, log.closingDateTime " +
									"FROM evacuation_center_log AS log " +
									"INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
									"INNER JOIN disaster AS d ON log.disasterID = d.disasterID " +
                                    "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
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
                                logInfo.toilet = logReader.GetInt32(4).ToString();
                                logInfo.bathingCubicle = logReader.GetInt32(5).ToString();
                                logInfo.communityKitchen = logReader.GetInt32(6).ToString();
                                logInfo.washingArea = logReader.GetInt32(7).ToString();
                                logInfo.womenChildSpace = logReader.GetInt32(8).ToString();
                                logInfo.multipurposeArea = logReader.GetInt32(9).ToString();
                                logInfo.fullAddress = logReader.GetString(10);
                                logInfo.centerType = logReader.GetString(11);
                                logInfo.mobileNum = logReader.IsDBNull(12) ? null : logReader.GetString(12);
                                logInfo.telephoneNum = logReader.IsDBNull(13) ? null : logReader.GetString(13);
                                logInfo.openingDateTime = logReader.GetDateTime(14).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                logInfo.closingDateTime = logReader.IsDBNull(15) ? null : logReader.GetDateTime(15).ToString("yyyy-MM-dd hh:mm tt").ToUpper();

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

		public int GetAssignedStaffCount()
		{
			string centerLogID = Request.Query["centerLogID"];
			string sql = "SELECT COUNT(*) " +
						 "FROM ec_staff_assignment " +
						 "WHERE centerLogID = @centerLogID and status = 'Assigned';";
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

        public string GetAssignedManagerFullName()
        {
            string centerLogID = Request.Query["centerLogID"];
            string staffFullName = null;

            string sql = "SELECT staffFullName " +
                         "FROM assigned_staff_view " +
                         "WHERE role = 'Manager' AND centerLogID = @centerLogID;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@centerLogID", centerLogID);
                        staffFullName = (string)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return staffFullName;
        }

        public string GetAssignedAsstManagerFullName()
        {
            string centerLogID = Request.Query["centerLogID"];
            string staffFullName = null;

            string sql = "SELECT staffFullName " +
                         "FROM assigned_staff_view " +
                         "WHERE role = 'Asst. Manager' AND centerLogID = @centerLogID;";
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@centerLogID", centerLogID);
                        staffFullName = (string)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return staffFullName;
        }

        public List<string> GetAssignedStaffFullName()
        {
            string centerLogID = Request.Query["centerLogID"];
            List<string> staffFullName = new List<string>();

            string sql = "SELECT staffFullName " +
                         "FROM assigned_staff_view " +
                         "WHERE role = 'Staff' AND centerLogID = @centerLogID;";

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                staffFullName.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return staffFullName;
        }

        public int GetTotalDistinctFamiliesCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.familyID) AS totalDistinctFamilies " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID;";
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

        public int GetTotalDistinctIndividualsCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctIndividuals " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID;";
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

        public int GetTotalDistinctFemaleCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctFemale " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND fm.sex = 'F';";
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

        public int GetTotalDistinctMaleCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctMale " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND fm.sex = 'M';";
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

        public int GetTotalDistinctBeneficiaryCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.familyID) AS totalDistinctBeneficiary " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "LEFT JOIN family f ON fm.familyID = f.familyID " +
                         "WHERE ecl.centerLogID = @centerLogID AND f.beneficiary = '4P''s';";
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

        public int GetTotalDistinctPregnantWomanCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctPregnantWoman " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND el.remarks = 'Pregnant Woman';";
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

        public int GetTotalDistinctChildrenCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctChildren " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND el.remarks = 'Children';";
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

        public int GetTotalDistinctElderlyCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctElderly " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND el.remarks = 'Elderly';";
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

        public int GetTotalDistinctPwdCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT COUNT(DISTINCT fm.memberID) AS totalDistinctPwd " +
                         "FROM evacuation_center_log ecl " +
                         "LEFT JOIN entry_log el ON ecl.centerLogID = el.centerLogID " +
                         "LEFT JOIN family_member fm ON el.memberID = fm.memberID " +
                         "WHERE ecl.centerLogID = @centerLogID AND el.remarks = 'PWD';";
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
}
