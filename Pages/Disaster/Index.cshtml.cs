using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster
{
	public class IndexModel : PageModel
	{
		public List<DisasterInfo> listDisaster = new List<DisasterInfo>();

		// Properties for sorting
		public string SortBy { get; set; } //Disaster Name
		public string SortOrder { get; set; } // Ascending, Descending
		public DateTime DateOccurrence { get; set; }

		public void OnGet(string sortBy, string sortOrder)
		{
			try
			{
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM disaster";

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
								DisasterInfo disasterInfo = new DisasterInfo();

								disasterInfo.disasterID = "" + reader.GetString(0);
								disasterInfo.disasterName = "" + reader.GetString(1);
								disasterInfo.disasterType = "" + reader.GetString(2);
								disasterInfo.description = "" + reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
								listDisaster.Add(disasterInfo);
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
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM disaster WHERE disasterName LIKE @query OR disasterType LIKE @query OR description LIKE @query OR dateOccured LIKE @query";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@query", $"%{query}%");

						using (MySqlDataReader reader = command.ExecuteReader())
						{
							List<DisasterInfo> searchResults = new List<DisasterInfo>();
							while (reader.Read())
							{
								DisasterInfo disasterInfo = new DisasterInfo();
								disasterInfo.disasterID = "" + reader.GetString(0);
								disasterInfo.disasterName = "" + reader.GetString(1);
								disasterInfo.disasterType = "" + reader.GetString(2);
								disasterInfo.description = "" + reader.GetString(3);
								disasterInfo.dateOccured = reader.GetDateTime("dateOccured").ToString("yyyy-MM-dd");
								searchResults.Add(disasterInfo);
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

	public class DisasterInfo
	{
		public string disasterID { get; set; }
		public string disasterName { get; set; }
		public string disasterType { get; set; }
		public string description { get; set; }
		public string dateOccured { get; set; }
	}

}