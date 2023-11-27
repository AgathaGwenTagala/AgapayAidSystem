using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.disaster.profile.staffassignment
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public List<StaffAssignmentInfo> listAssignment { get; set; } = new List<StaffAssignmentInfo>();
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

					// Fetch info of selected center log from the database
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName, log.status " +
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
								logInfo.status = logReader.GetString(4);
							}
						}
					}

					// Fetch staff assignment data related to the selected center log
					string assignSql = "SELECT * FROM staff_profile_view " +
									  "WHERE centerLogID = @centerLogID ORDER BY assignmentDate;";
					using (MySqlCommand assignCommand = new MySqlCommand(assignSql, connection))
					{
						assignCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader assignReader = assignCommand.ExecuteReader())
						{
							while (assignReader.Read())
							{
								StaffAssignmentInfo assignmentInfo = new StaffAssignmentInfo();
								assignmentInfo.assignmentID = assignReader.GetString(0);
								assignmentInfo.centerLogID = assignReader.GetString(1);
								assignmentInfo.ecStaffID = assignReader.GetString(2);
								assignmentInfo.role = assignReader.GetString(3);
								assignmentInfo.assignmentDate = assignReader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.completionDate = assignReader.IsDBNull(5) ? null : assignReader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								assignmentInfo.status = assignReader.GetString(6);
								assignmentInfo.fullName = assignReader.GetString(7);
								assignmentInfo.sex = assignReader.GetString(8);
								assignmentInfo.age = assignReader.GetInt64(9).ToString();
								assignmentInfo.birthdate = assignReader.GetDateTime(10).ToString("MMMM dd, yyyy");
								assignmentInfo.mobileNum = assignReader.GetString(11);
								assignmentInfo.emailAddress = assignReader.GetString(12);
								listAssignment.Add(assignmentInfo);
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

		public void OnPost(string[] selectedStaff)
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
			}

			if (selectedStaff == null || selectedStaff.Length == 0)
			{
				errorMessage = "Please select at least one staff member.";
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
					foreach (string assignmentID in selectedStaff)
					{
						string insertSql = "UPDATE ec_staff_assignment " +
										   "SET completionDate = CURRENT_TIMESTAMP(), status = 'Completed' " +
										   "WHERE assignmentID = @assignmentID";

						using (MySqlCommand insertCommand = new MySqlCommand(insertSql, connection))
						{
							insertCommand.Parameters.AddWithValue("@assignmentID", assignmentID);

							int rowsInserted = insertCommand.ExecuteNonQuery();
							if (rowsInserted == 1)
							{
								successMessage = "Assignment completed.";
							}
							else
							{
								errorMessage = "Failed to complete one or more assignment.";
							}
						}
					}

					// If all selected staff assignment were successfully completed
					//successMessage = "Selected staff assigment completed successfully!";
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			// Redirect to the Staff Assignment page after completion or encountering an error
			Response.Redirect("/disaster/profile/staffassignment/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage + "&successMessage=" + successMessage);
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

	public class StaffAssignmentInfo
	{ 
		public string assignmentID { get; set; }
		public string centerLogID { get; set; }
		public string ecStaffID { get; set; }
		public string role { get; set; }
		public string assignmentDate { get; set; }
		public string completionDate { get; set; }
		public string status { get; set; }
		public string fullName { get; set; }
		public string sex { get; set; }
		public string age { get; set; }
		public string birthdate { get; set; }
		public string mobileNum { get; set; }
		public string emailAddress { get; set; }
	}
}
