using AgapayAidSystem.Pages.Disaster.Profile;
using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using AgapayAidSystem.Pages.disaster.profile.staffassignment;

namespace AgapayAidSystem.Pages.disaster.profile.informationboard
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public DisasterInfo disasterInfo { get; set; } = new DisasterInfo();
		public InformationBoard infoBoard { get; set; } = new InformationBoard();
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
        public EcLogNotification ecLogNotif { get; set; } = new EcLogNotification();

        public string errorMessage = "";
        public List<string> staffFullName = new List<string>();
        public string errorMessage = "";
		public string successMessage = "";
        public string UserId { get; set; }
        public string UserType { get; set; }

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
                                    "log.openingDateTime, log.closingDateTime, log.status " +
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
                                logInfo.status = logReader.GetString(16);
                            }
                        }
					}

					// Fetch information board data related to the selected center log
					string infoSql = "CALL get_information_board(@centerLogID)";
					using (MySqlCommand infoCommand = new MySqlCommand(infoSql, connection))
					{
						infoCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader infoReader = infoCommand.ExecuteReader())
						{
							if (infoReader.Read())
							{
								infoBoard.assignedStaffCount = infoReader.GetInt32(0);
								infoBoard.assignedManager = infoReader.IsDBNull(1) ? null : infoReader.GetString(1);
								infoBoard.assignedAsstManager = infoReader.IsDBNull(2) ? null : infoReader.GetString(2);
								infoBoard.totalDistinctFamilies = infoReader.GetInt32(3);
								infoBoard.totalDistinctIndividuals = infoReader.GetInt32(4);
								infoBoard.totalDistinctMale = infoReader.GetInt32(5);
								infoBoard.totalDistinctFemale = infoReader.GetInt32(6);
								infoBoard.totalDistinctPregnantWoman = infoReader.GetInt32(7);
								infoBoard.totalDistinctChildren = infoReader.GetInt32(8);
								infoBoard.totalDistinctElderly = infoReader.GetInt32(9);
								infoBoard.totalDistinctPwd = infoReader.GetInt32(10);
								infoBoard.totalDistinctIP = infoReader.GetInt32(11);
							}
						}
					}

                    // Fetch list of assigned staff related to the selected center log
                    string staffSql = "SELECT staffFullName " +
                                      "FROM assigned_staff_view " +
                                      "WHERE role = 'Staff' AND centerLogID = @centerLogID;";
                    using (MySqlCommand staffCommand = new MySqlCommand(staffSql, connection))
                    {
                        staffCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        using (MySqlDataReader staffReader = staffCommand.ExecuteReader())
                        {
                            while (staffReader.Read())
                            {
                                staffFullName.Add(staffReader.GetString(0));
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

		public void OnPost()
		{
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			string centerLogID = Request.Form["centerLogID"];
			string disasterID = Request.Form["disasterID"];
			string centerName = Request.Form["centerName"];

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
					string sql = "CALL close_evacuation_center(@centerLogID);";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerLogID", centerLogID);
						command.ExecuteNonQuery();
					}
				}

				successMessage = centerName + " closed successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/informationboard/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Disaster page after successful close
				Response.Redirect("/disaster/profile/index?disasterID=" + disasterID + "&successMessage=" + successMessage);
				//Response.Redirect("/disaster/profile/informationboard/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
			}
		}

		public int GetRemainingInventoryCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT count(*) FROM inventory_item_view " +
                         "WHERE centerLogID = @centerLogID AND remainingQty > 0;";
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

        public int GetRemainingPackCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT count(*) AS remainingPacks FROM pack " +
                         "WHERE centerLogID = @centerLogID AND status = 'Packed';";
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

        public int GetRemainingAssessmentCount()
        {
            string centerLogID = Request.Query["centerLogID"];
            string sql = "SELECT count(*) AS remainingAssessment FROM distinct_family_head_view " +
                         "WHERE centerLogID = @centerLogID;";
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

    public class InformationBoard
    {
		public int assignedStaffCount { get; set; }
		public string assignedManager { get; set; }
		public string assignedAsstManager { get; set; }
		public int totalDistinctFamilies { get; set; }
		public int totalDistinctIndividuals { get; set; }
		public int totalDistinctMale { get; set; }
		public int totalDistinctFemale { get; set; }
		public int totalDistinctPregnantWoman { get; set; }
		public int totalDistinctChildren { get; set; }
		public int totalDistinctElderly { get; set; }
		public int totalDistinctPwd { get; set; }
		public int totalDistinctIP { get; set; }
	}
}
