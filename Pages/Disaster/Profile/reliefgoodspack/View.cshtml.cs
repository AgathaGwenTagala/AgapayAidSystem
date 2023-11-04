using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile.reliefgoodspack
{
    public class ViewModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public ViewModel(IConfiguration configuration) => _configuration = configuration;
		public List<PackInclusionInfo> listPackInclusion { get; set; } = new List<PackInclusionInfo>();
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

					// Fetch relief pack data related to the selected center log
					string sql = "SELECT p.*, (pp.packQty * p.qty) AS totalQty, i.itemName, i.itemType, i.unitMeasure " +
								 "FROM pack_inclusion p " +
								 "JOIN pack pp ON p.packID = pp.packID " +
								 "JOIN batch_inclusion b ON p.batchInclusionID = b.batchInclusionID " +
								 "JOIN item i ON b.itemID = i.itemID " +
								 "WHERE pp.packID = @packID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@packID", packID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								PackInclusionInfo inclusion = new PackInclusionInfo();
								inclusion.packInclusionID = reader.GetString(0);
								inclusion.batchInclusionID = reader.GetString(1);
								inclusion.packID = reader.GetString(2);
								inclusion.qty = reader.GetInt32(3).ToString();
								inclusion.totalQty = reader.GetInt32(4).ToString();
								inclusion.itemName = reader.GetString(5);
								inclusion.itemType = reader.GetString(6);
								inclusion.unitMeasure = reader.GetString(7);
								listPackInclusion.Add(inclusion);
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

	public class PackInclusionInfo
	{
		public string packInclusionID { get; set; }
		public string batchInclusionID { get; set; }
		public string packID { get; set; }
		public string qty { get; set; }
		public string totalQty { get; set; }
		public string itemName { get; set; }
		public string itemType { get; set; }
		public string unitMeasure { get; set; }
	}
}
