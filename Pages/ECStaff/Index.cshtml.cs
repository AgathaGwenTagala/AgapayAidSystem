using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.ECStaff
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;

		public List<ECStaffInfo> listECStaff = new List<ECStaffInfo>();
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
					string sql = "SELECT * FROM ec_staff_view";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ECStaffInfo ECStaffInfo = new ECStaffInfo();
								ECStaffInfo.ecStaffID = reader.GetString(0);
								ECStaffInfo.firstName = reader.GetString(2);
								ECStaffInfo.middleName = reader.IsDBNull(3) ? null : reader.GetString(3);
								ECStaffInfo.lastName = reader.GetString(4);
								ECStaffInfo.extName = reader.IsDBNull(5) ? null : reader.GetString(5);
								ECStaffInfo.sex = reader.GetString(6);
								ECStaffInfo.birthdate = reader.GetDateTime(7).ToString("yyyy-MM-dd");
								ECStaffInfo.mobileNum = reader.GetString(8);
								ECStaffInfo.emailAddress = reader.GetString(9);
								ECStaffInfo.availabilityStatus = reader.GetString(10);
								ECStaffInfo.fullName = reader.GetString(11);
								ECStaffInfo.age = reader.GetInt64(12).ToString();
								listECStaff.Add(ECStaffInfo);
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

	public class ECStaffInfo
	{
		public string ecStaffID { get; set; }
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string extName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string mobileNum { get; set; }
		public string emailAddress { get; set; }
		public string availabilityStatus { get; set; }
		public string fullName { get; set; }
		public string age { get; set; }
	}
}
