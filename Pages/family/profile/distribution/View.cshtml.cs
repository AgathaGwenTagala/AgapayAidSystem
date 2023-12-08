using AgapayAidSystem.Pages.Disaster.Profile.reliefgoodspack;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.family.profile.distribution
{
    public class ViewModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public ViewModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public List<PackInclusionInfo> listPackInclusion { get; set; } = new List<PackInclusionInfo>();
		public string errorMessage = "";
		public string successMessage = "";
		public string packID = "";
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

            string familyID = Request.Query["familyID"];
			packID = Request.Query["packID"];
			
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected family from the database
					string familySql = "SELECT familyID, serialNum FROM family " +
									   "WHERE familyID = @familyID";
					using (MySqlCommand familyCommand = new MySqlCommand(familySql, connection))
					{
						familyCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyReader = familyCommand.ExecuteReader())
						{
							if (familyReader.Read())
							{
								familyInfo.familyID = familyReader.GetString(0);
								familyInfo.serialNum = familyReader.GetString(1);
							}
						}
					}

					// Fetch pack inclusion related to the selected relief goods pack
					string sql = "select * from pack_inclusion_view WHERE packID = @packID;";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@packID", packID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								PackInclusionInfo inclusion = new PackInclusionInfo();
								inclusion.packInclusionID = reader.GetString(0);
								inclusion.inventoryID = reader.GetString(1);
								inclusion.packID = reader.GetString(2);
								inclusion.qty = reader.GetInt32(3).ToString();
								inclusion.totalQty = reader.GetInt32(4).ToString();
								inclusion.itemName = reader.GetString(5);
								inclusion.itemType = reader.GetString(6);
								inclusion.unitMeasure = reader.GetString(7).ToLower();
								inclusion.disasterName = reader.GetString(8);
								inclusion.centerName = reader.GetString(9);
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
}
