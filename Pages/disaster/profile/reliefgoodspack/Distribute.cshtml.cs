using AgapayAidSystem.Pages.disaster.profile.reliefdistribution;
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
        public ReliefPackInfo reliefPackInfo { get; set; } = new ReliefPackInfo();
        public DistributionInfo distributionInfo { get; set; } = new DistributionInfo();
        public List<EligibleFamInfo> listEligibleFam { get; set; } = new List<EligibleFamInfo>();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string centerLogID = Request.Query["centerLogID"];
            string packID = Request.Query["packID"];
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

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
            //string sql = "SELECT e.entryLogID, mv.memberID, mv.fullName, mv.relationship " +
            //             "FROM family_member_view mv " +
            //             "JOIN entry_log e ON mv.memberID = e.memberID " +
            //             "WHERE familyID = @familyID " +
            //             "ORDER BY mv.memberID;";

            // this will get all family members of the family even if their entryStatus is not 'Check-in'
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

            // Retrieve input from the form
            string centerLogID = Request.Form["centerLogID"];
            distributionInfo.packID = Request.Form["packID"];
            // distributionInfo.assignmentID = Request.Form["assignmentID"];
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

                    // Insert data into the 'distribution_record' table
                    string sql = "INSERT INTO distribution_record " +
                                 "(packID, entryLogID, qty) " +
                                 "VALUES (@packID, @entryLogID, @qty);";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@packID", distributionInfo.packID);
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
