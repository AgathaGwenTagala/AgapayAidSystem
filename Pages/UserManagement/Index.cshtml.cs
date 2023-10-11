using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public List<UserInfo> listUsers = new List<UserInfo>();

        public void OnGet()
        {
            try
            {
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM user";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserInfo userInfo = new UserInfo();
                                userInfo.userID = reader.GetString(0);
                                userInfo.username = reader.GetString(1);
                                userInfo.password = reader.GetString(2);
                                userInfo.userType = reader.GetString(3);
                                userInfo.userPhoto = reader.IsDBNull(4) ? null : (byte[])reader.GetValue(4);
                                userInfo.createdAt = reader.GetDateTime(5).ToString("yyyy-MMM-dd hh:mm:ss tt").ToUpper();
                                listUsers.Add(userInfo);
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
					string sql = "SELECT * FROM user WHERE username LIKE @query OR userType LIKE @query OR createdAt LIKE @query";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@query", $"%{query}%");

						using (MySqlDataReader reader = command.ExecuteReader())
						{
							List<UserInfo> searchResults = new List<UserInfo>();
							while (reader.Read())
							{
								UserInfo userInfo = new UserInfo();
								userInfo.userID = reader.GetString(0);
								userInfo.username = reader.GetString(1);
								userInfo.password = reader.GetString(2);
								userInfo.userType = reader.GetString(3);
                                userInfo.userPhoto = reader.IsDBNull(4) ? null : (byte[])reader.GetValue(4);
                                userInfo.createdAt = reader.GetDateTime(5).ToString("yyyy-MM-dd hh:mm:ss tt").ToUpper();
                                searchResults.Add(userInfo);
							}
							return new JsonResult(searchResults);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.ToString());
				return new JsonResult(new List<UserInfo>());
			}
		}
	}

    public class UserInfo
    {
        public string userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
        public byte[] userPhoto { get; set; }
        public string createdAt { get; set; }
        public IFormFile Photo { get; set; }
    }
}