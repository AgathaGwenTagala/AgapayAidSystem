using AgapayAidSystem.Pages.account;
using AgapayAidSystem.Pages.ECStaff;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.ecstaff
{
    public class AssignmentHistoryModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public AssignmentHistoryModel(IConfiguration configuration) => _configuration = configuration;

		public ECStaffInfo ecStaffInfo = new ECStaffInfo();
		public List<AssignmentInfo> listAssignment = new List<AssignmentInfo>();
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

			if (UserType == "EC Staff")
			{
				Response.Redirect("/accessdenied");
				return;
			}

			string ecStaffID = Request.Query["ecStaffID"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM assignment_history_view " +
								 "WHERE ecStaffID = @ecStaffID " +
								 "ORDER BY assignmentDate";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@ecStaffID", ecStaffID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								AssignmentInfo assignmentInfo = new AssignmentInfo();
								assignmentInfo.assignmentID = reader.GetString(0);
								assignmentInfo.centerLogID = reader.GetString(1);
								assignmentInfo.ecStaffID = reader.GetString(2);
								assignmentInfo.role = reader.GetString(3);
								//assignmentInfo.assignmentDate = reader.GetDateTime(4).ToString("yyyy-MM-dd hh:mm tt").ToUpper();
								//assignmentInfo.completionDate = reader.IsDBNull(5) ? "-" : reader.GetDateTime(5).ToString("yyyy-MM-dd  hh:mm tt").ToUpper();

                                // Convert to UTC+8
                                DateTime dateUtc1 = reader.GetDateTime(4);
                                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"); // Use the time zone for UTC+8
                                assignmentInfo.assignmentDate = TimeZoneInfo.ConvertTimeFromUtc(dateUtc1, tzInfo).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();

                                DateTime? dateUtc2 = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                                assignmentInfo.completionDate = dateUtc2.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(dateUtc2.Value, tzInfo).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper() : null;

                                assignmentInfo.status = reader.GetString(6);
								assignmentInfo.userID = reader.GetString(7);
								assignmentInfo.disasterName = reader.GetString(8);
								assignmentInfo.centerName = reader.GetString(9);
								listAssignment.Add(assignmentInfo);
							}
						}
					}

					string ECsql = "SELECT * FROM ec_staff_view WHERE ecStaffID = @ecStaffID";
					using (MySqlCommand command = new MySqlCommand(ECsql, connection))
					{
						command.Parameters.AddWithValue("@ecStaffID", ecStaffID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								ecStaffInfo.ecStaffID = reader.GetString(0);
								ecStaffInfo.firstName = reader.GetString(2);
								ecStaffInfo.middleName = reader.IsDBNull(3) ? "-" : reader.GetString(3);
								ecStaffInfo.lastName = reader.GetString(4);
								ecStaffInfo.extName = reader.IsDBNull(5) ? "-" : reader.GetString(5);
								ecStaffInfo.sex = reader.GetString(6);
								ecStaffInfo.birthdate = reader.GetDateTime(7).ToString("yyyy-MM-dd");
								ecStaffInfo.mobileNum = reader.GetString(8);
								ecStaffInfo.emailAddress = reader.GetString(9);
								ecStaffInfo.availabilityStatus = reader.GetString(10);
								ecStaffInfo.fullName = reader.GetString(11);
								ecStaffInfo.age = reader.GetInt64(12).ToString();
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
