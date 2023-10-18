using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Add
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public List<BarangayInfo> Barangays { get; set; }
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			// Fetch the list of barangays from database
			Barangays = GetBarangaysFromDatabase();
		}

		private List<BarangayInfo> GetBarangaysFromDatabase()
		{
			var barangayData = new List<BarangayInfo>();

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					string sql = "SELECT barangayID, barangayName FROM barangay";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								barangayData.Add(new BarangayInfo
								{
									barangayID = reader.GetString(0),
									barangayName = reader.GetString(1)
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

			return barangayData;
		}
		
		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve evacuation center details from the form
			familyInfo.familyID = Request.Form["familyID"];
			familyInfo.serialNum = Request.Form["serialNum"];
			familyInfo.streetAddress = Request.Form["streetAddress"];
			familyInfo.barangayID = Request.Form["barangayID"];
			familyInfo.mobileNum = Request.Form["mobileNum"];
			familyInfo.telephoneNum = Request.Form["telephoneNum"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'evacuation_center' table
					string sql = "INSERT INTO family " +
								 "(familyID, serialNum, streetAddress, barangayID, mobileNum, telephoneNum) " +
								 "VALUES (@familyID, @serialNum, @streetAddress, @barangayID, @mobileNum, @telephoneNum)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@familyID", familyInfo.familyID);
						command.Parameters.AddWithValue("@serialNum", familyInfo.serialNum);
						command.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
						command.Parameters.AddWithValue("@barangayID", familyInfo.barangayID);
						command.Parameters.AddWithValue("@mobileNum", familyInfo.mobileNum);
						command.Parameters.AddWithValue("@telephoneNum", familyInfo.telephoneNum);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Family added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/family/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}
}
