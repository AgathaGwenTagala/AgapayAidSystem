using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.ECStaffDatabase
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;

		public List<ECStaffDatabaseInfo> listECStaffDatabase = new List<ECStaffDatabaseInfo>();

		public string SortBy { get; set; }
		public string SortOrder { get; set; }
		
		public void OnGet(string sortBy, string sortOrder)
        {
            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
					connection.Open();
					string sql = "SELECT *, " +
								 "CONCAT(`firstName`,' ',LEFT(`middleName`, 1),'. ',`lastName`,' ', `extName`) AS `fullName` " +
								 "FROM ec_staff;";

					// Apply sorting based on user's selection
					if (!string.IsNullOrEmpty(sortBy))
					{
						sql += $" ORDER BY {sortBy} {(sortOrder == "desc" ? "DESC" : "ASC")}";
						SortBy = sortBy;
						SortOrder = sortOrder;
					}
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ECStaffDatabaseInfo ecstaffdatabaseInfo = new ECStaffDatabaseInfo();
								ecstaffdatabaseInfo.ecStaffID = reader.GetString(0);
								ecstaffdatabaseInfo.firstName = reader.GetString(2);
								ecstaffdatabaseInfo.middleName = reader.GetString(3);
								ecstaffdatabaseInfo.lastName = reader.GetString(4);
								ecstaffdatabaseInfo.extName = reader.GetString(5);
								ecstaffdatabaseInfo.sex = reader.GetString(6);
								ecstaffdatabaseInfo.birthdate = reader.GetDateTime(7).ToString("yyyy-MM-dd");
								ecstaffdatabaseInfo.mobileNum = reader.GetString(8);
								ecstaffdatabaseInfo.emailAddress = reader.GetString(9);
								ecstaffdatabaseInfo.availabilityStatus = reader.GetString(10);
								ecstaffdatabaseInfo.fullName = reader.GetString(11);
								listECStaffDatabase.Add(ecstaffdatabaseInfo);
							}
						}
					}
				}
			}

			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.ToString());
			}
		}

		public JsonResult OnGetSearch(string query)
		{
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT *, " +
								 "CONCAT(`firstName`,' ',LEFT(`middleName`, 1),'. ',`lastName`,' ', `extName`) AS `fullName` " +
								 "FROM ec_staff " +
								 "WHERE CONCAT(`firstName`,' ',LEFT(`middleName`, 1),'. ',`lastName`,' ', `extName`)  LIKE @query " +
								 "OR sex LIKE @query OR birthdate LIKE @query OR mobileNum LIKE @query OR availabilityStatus LIKE @query";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@query", $"%{query}%");
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							List<ECStaffDatabaseInfo> searchResults = new List<ECStaffDatabaseInfo>();
							while (reader.Read())
							{
								ECStaffDatabaseInfo ecstaffdatabaseInfo = new ECStaffDatabaseInfo();
								ecstaffdatabaseInfo.ecStaffID = reader.GetString(0);
								ecstaffdatabaseInfo.firstName = reader.GetString(2);
								ecstaffdatabaseInfo.middleName = reader.GetString(3);
								ecstaffdatabaseInfo.lastName = reader.GetString(4);
								ecstaffdatabaseInfo.extName = reader.GetString(5);
								ecstaffdatabaseInfo.sex = reader.GetString(6);
								ecstaffdatabaseInfo.birthdate = reader.GetDateTime(7).ToString("yyyy-MM-dd");
								ecstaffdatabaseInfo.mobileNum = reader.GetString(8);
								ecstaffdatabaseInfo.emailAddress = reader.GetString(9);
								ecstaffdatabaseInfo.availabilityStatus = reader.GetString(10);
								ecstaffdatabaseInfo.fullName = reader.GetString(11);
								searchResults.Add(ecstaffdatabaseInfo);
							}
							return new JsonResult(searchResults);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.ToString());
				return new JsonResult(new List<DisasterInfo>());
			}
		}
	}
	public class ECStaffDatabaseInfo
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
	}
}
