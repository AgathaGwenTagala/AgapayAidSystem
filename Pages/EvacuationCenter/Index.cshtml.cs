using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.EvacuationCenter
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public List<EvacuationInfo> listEvacuation = new List<EvacuationInfo>();
		public string SortBy { get; set; }
		public string SortOrder { get; set; }
		public string successMessage = "";
		public string errorMessage = "";

		public void OnGet(string sortBy, string sortOrder)
        {
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
                    string sql = "SELECT ec.*, b.barangayName " +
								 "FROM evacuation_center AS ec " +
								 "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID";

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
								EvacuationInfo evacuationInfo = new EvacuationInfo();
								evacuationInfo.centerID = reader.GetString(0);
								evacuationInfo.centerName = reader.GetString(1);
								evacuationInfo.centerType = reader.GetString(2);
								evacuationInfo.streetAddress = reader.GetString(3);
								evacuationInfo.barangayID = reader.GetString(4);
								evacuationInfo.mobileNum = reader.IsDBNull(5) ? null : reader.GetString(5);
								evacuationInfo.telephoneNum = reader.IsDBNull(6) ? null : reader.GetString(6);
								evacuationInfo.maxCapacity = reader.GetInt32(7).ToString();
								evacuationInfo.status = reader.GetString(8);
								evacuationInfo.toilet = reader.GetInt32(9).ToString();
								evacuationInfo.bathingCubicle = reader.GetInt32(10).ToString();
								evacuationInfo.communityKitchen = reader.GetInt32(11).ToString();
								evacuationInfo.washingArea = reader.GetInt32(12).ToString();
								evacuationInfo.womenChildSpace = reader.GetInt32(13).ToString();
								evacuationInfo.multipurposeArea = reader.GetInt32(14).ToString();
                                evacuationInfo.barangayName = reader.GetString(15);
                                listEvacuation.Add(evacuationInfo);
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

		public JsonResult OnGetSearch(string query)
		{
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT ec.*, b.barangayName " +
								 "FROM evacuation_center AS ec " +
								 "INNER JOIN barangay AS b ON ec.barangayID = b.barangayID " +
								 "WHERE ec.centerName LIKE @query OR ec.maxCapacity LIKE @query OR ec.status LIKE @query OR b.barangayName LIKE @query;";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@query", $"%{query}%");

						using (MySqlDataReader reader = command.ExecuteReader())
						{
							List<EvacuationInfo> searchResults = new List<EvacuationInfo>();
							while (reader.Read())
							{
								EvacuationInfo evacuationInfo = new EvacuationInfo();
								evacuationInfo.centerID = reader.GetString(0);
								evacuationInfo.centerName = reader.GetString(1);
								evacuationInfo.centerType = reader.GetString(2);
								evacuationInfo.streetAddress = reader.GetString(3);
								evacuationInfo.barangayID = reader.GetString(4);
								evacuationInfo.mobileNum = reader.IsDBNull(5) ? null : reader.GetString(5);
								evacuationInfo.telephoneNum = reader.IsDBNull(6) ? null : reader.GetString(6);
								evacuationInfo.maxCapacity = reader.GetInt32(7).ToString();
								evacuationInfo.status = reader.GetString(8);
								evacuationInfo.toilet = reader.GetInt32(9).ToString();
								evacuationInfo.bathingCubicle = reader.GetInt32(10).ToString();
								evacuationInfo.communityKitchen = reader.GetInt32(11).ToString();
								evacuationInfo.washingArea = reader.GetInt32(12).ToString();
								evacuationInfo.womenChildSpace = reader.GetInt32(13).ToString();
								evacuationInfo.multipurposeArea = reader.GetInt32(14).ToString();
								evacuationInfo.barangayName = reader.GetString(15);
								searchResults.Add(evacuationInfo);
							}
							return new JsonResult(searchResults);
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return new JsonResult(new List<EvacuationInfo>());
			}
		}
	}

    public class EvacuationInfo
    {
		public string centerID { get; set; }
		public string centerName { get; set; }
		public string centerType { get; set; }
		public string streetAddress { get; set; }
		public string barangayID { get; set; }
		public string mobileNum { get; set; }
		public string telephoneNum { get; set; }
		public string maxCapacity { get; set; }
		public string status { get; set; }
		public string toilet { get; set; }
		public string bathingCubicle { get; set; }
		public string communityKitchen { get; set; }
		public string washingArea { get; set; }
		public string womenChildSpace { get; set; }
		public string multipurposeArea { get; set; }
        public string barangayName { get; set; }
    }
}
