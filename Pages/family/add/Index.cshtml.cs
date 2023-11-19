using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.family.add
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
					string sql = "SELECT barangayID, barangayName FROM barangay ORDER BY barangayName";
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
			familyInfo.streetAddress = Request.Form["streetAddress"];
			familyInfo.barangayID = Request.Form["barangayID"];
			familyInfo.mobileNum = Request.Form["mobileNum"];
			familyInfo.telephoneNum = Request.Form["telephoneNum"];
			familyInfo.beneficiary = Request.Form["beneficiary"];
			familyInfo.serialNum = Request.Form["serialNum"];

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Step 1: Insert data into the 'family' table
					string sql = "INSERT INTO family " +
								 "(streetAddress, barangayID, mobileNum, telephoneNum, beneficiary) " +
								 "VALUES (@streetAddress, @barangayID, @mobileNum, @telephoneNum, @beneficiary);";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
						command.Parameters.AddWithValue("@barangayID", familyInfo.barangayID);
						command.Parameters.AddWithValue("@mobileNum", familyInfo.mobileNum);
						command.Parameters.AddWithValue("@telephoneNum", familyInfo.telephoneNum);
						command.Parameters.AddWithValue("@beneficiary", familyInfo.beneficiary);
						command.ExecuteNonQuery();
					}

					// Step 2: Fetch the last inserted familyID
				}

				successMessage = "Family added successfully!";
				Response.Redirect("/family/profile/add/head?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}
}
