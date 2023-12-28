using AgapayAidSystem.Pages.disaster.profile.staffassignment;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;


namespace AgapayAidSystem.Pages.disaster.profile.reliefgoodspack
{
    public class DistributeModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public DistributeModel(IConfiguration configuration) => _configuration = configuration;
        public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public StaffAssignmentInfo assignmentInfo { get; set; } = new StaffAssignmentInfo();
		public ReliefPackInfo reliefPackInfo { get; set; } = new ReliefPackInfo();
        public DistributionInfo distributionInfo { get; set; } = new DistributionInfo();
        public List<EligibleFamInfo> listEligibleFam { get; set; } = new List<EligibleFamInfo>();
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
            
            if (UserType == "LGU Staff")
            {
                Response.Redirect("/accessdenied");
                return;
            }

            string centerLogID = Request.Query["centerLogID"];
            string packID = Request.Query["packID"];
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

					// Fetch assigned staff user information
					string assignedSql = "SELECT esa.* " +
										 "FROM ec_staff_assignment esa " +
										 "JOIN ec_staff ec ON esa.ecStaffID = ec.ecStaffID " +
										 "WHERE ec.userID = @userID AND esa.status = 'Assigned';";
					using (MySqlCommand assignedCommand = new MySqlCommand(assignedSql, connection))
					{
						assignedCommand.Parameters.AddWithValue("@userID", UserId);
						using (MySqlDataReader assignedReader = assignedCommand.ExecuteReader())
						{
							if (assignedReader.Read())
							{
								assignmentInfo.assignmentID = assignedReader.GetString(0);
								assignmentInfo.centerLogID = assignedReader.GetString(1);
								assignmentInfo.ecStaffID = assignedReader.GetString(2);
								assignmentInfo.role = assignedReader.GetString(3);
								assignmentInfo.assignmentDate = assignedReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.completionDate = assignedReader.IsDBNull(5) ? null : assignedReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.status = assignedReader.GetString(6);

								if (assignmentInfo.status != "Assigned")
								{
									Response.Redirect("/accessdenied");
									return;
								}

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

                    // Fetch relief pack data related to the selected center log
                    string reliefSql = "SELECT *, (packQty - distributedQty) AS remainingQty " +
                                       "FROM pack " +
                                       "WHERE packID = @packID;";
                    using (MySqlCommand reliefCommand = new MySqlCommand(reliefSql, connection))
                    {
                        reliefCommand.Parameters.AddWithValue("@packID", packID);
                        using (MySqlDataReader reliefReader = reliefCommand.ExecuteReader())
                        {
                            if (reliefReader.Read())
                            {
                                reliefPackInfo.packID = reliefReader.GetString(0);
                                reliefPackInfo.centerLogID = reliefReader.GetString(1);
                                reliefPackInfo.packQty = reliefReader.GetInt32(2).ToString();
                                reliefPackInfo.distributedQty = reliefReader.GetInt32(3).ToString();
                                reliefPackInfo.status = reliefReader.GetString(4);
                                reliefPackInfo.createdAt = reliefReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                reliefPackInfo.distributedAt = reliefReader.IsDBNull(6) ? null : reliefReader.GetDateTime(6).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
                                reliefPackInfo.remainingQty = reliefReader.GetInt32(7).ToString();
                            }
                        }
                    }

                    // Fetch eligible families for distribution related to the selected pack
                    string eligibleFamSql = "CALL eligible_family(@centerLogID, @packID);";
                    using (MySqlCommand eligibleFamCommand = new MySqlCommand(eligibleFamSql, connection))
                    {
                        eligibleFamCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
                        eligibleFamCommand.Parameters.AddWithValue("@packID", packID);
                        using (MySqlDataReader eligibleFamReader = eligibleFamCommand.ExecuteReader())
                        {
                            while (eligibleFamReader.Read())
                            {
                                EligibleFamInfo eligibleFamInfo = new EligibleFamInfo();
                                eligibleFamInfo.familyID = eligibleFamReader.GetString(0);
                                eligibleFamInfo.serialNum = eligibleFamReader.GetString(1);
                                listEligibleFam.Add(eligibleFamInfo);
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

        public List<MemberInfo> GetEligibleMember(string familyID)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            string sql = "SELECT CAST(MIN(e.entryLogID) AS CHAR) AS entryLogID, " +
                         "mv.memberID, mv.fullName, mv.relationship " +
                         "FROM family_member_view mv " +
                         "JOIN entry_log e ON mv.memberID = e.memberID " +
                         "WHERE familyID = @familyID " +
                         "GROUP BY mv.memberID;";
            try
                {
                    string connectionString = _configuration.GetConnectionString("DefaultConnection");
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@familyID", familyID);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    members.Add(new MemberInfo
                                    {
                                        entryLogID = reader.GetString(0),
                                        memberID = reader.GetString(1),
                                        fullName = reader.GetString(2),
                                        relationship = reader.GetString(3)
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

            return members;
        }

        public void OnPost()
        {
            bool errorOccurred = false;

            if (!ModelState.IsValid)
            {
                errorMessage = "Please correct the errors below.";
                errorOccurred = true;
            }

            // Retrieve userID
			UserId = HttpContext.Session.GetString("UserId");

			// Retrieve input from the form
			string centerLogID = Request.Form["centerLogID"];
            distributionInfo.packID = Request.Form["packID"];
            distributionInfo.assignmentID = "";
            distributionInfo.entryLogID = Request.Form["entryLogID"];
            distributionInfo.qty = Request.Form["qty"];

            if (string.IsNullOrEmpty(distributionInfo.entryLogID))
            {
                errorMessage = "Missing entyrLogID";
                errorOccurred = true;
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

					// Fetch assignmentID of logged in user 
					string assignSql = "SELECT esa.assignmentID " +
                                       "FROM ec_staff_assignment esa " +
                                       "JOIN ec_staff ec ON esa.ecStaffID = ec.ecStaffID " +
                                       "JOIN user u ON ec.userID = u.userID " +
                                       "WHERE ec.userID = @userID AND esa.status = 'Assigned';";
					using (MySqlCommand assignCommand = new MySqlCommand(assignSql, connection))
					{
						assignCommand.Parameters.AddWithValue("@userID", UserId);
						using (MySqlDataReader assignReader = assignCommand.ExecuteReader())
						{
							if (assignReader.Read())
							{
								distributionInfo.assignmentID = assignReader.GetString(0);
							}
						}
					}

					// Insert data into the 'distribution_record' table
					string sql = "INSERT INTO distribution_record " +
                                 "(packID, assignmentID, entryLogID, qty) " +
								 "VALUES (@packID, @assignmentID, @entryLogID, @qty);";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@packID", distributionInfo.packID);
                        command.Parameters.AddWithValue("@assignmentID", distributionInfo.assignmentID);
                        command.Parameters.AddWithValue("@entryLogID", distributionInfo.entryLogID);
                        command.Parameters.AddWithValue("@qty", distributionInfo.qty);
                        command.ExecuteNonQuery();
                    }
                }

                successMessage = "Pack distributed successfully!";
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                // Show an error message banner on the current page
                Response.Redirect("/disaster/profile/reliefgoodspack/distribute?packID=" + distributionInfo.packID + "&centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
            }
            else
            {
                // Redirect to the Relief Goods Pack page after successful add
                Response.Redirect("/disaster/profile/reliefgoodspack/distribute?packID=" + distributionInfo.packID + "&centerLogID=" + centerLogID + "&successMessage=" + successMessage);
            }
        }
    }

    public class EligibleFamInfo
    {
        public string packID { get; set; }
        public string centerLogID { get; set; }
        public string familyID { get; set; }
        public string serialNum { get; set; }
    }

    public class MemberInfo
    {
        public string entryLogID { get; set; }
        public string memberID { get; set; }
        public string fullName { get; set; }
        public string relationship { get; set; }
    }

}
