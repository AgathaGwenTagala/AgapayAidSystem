using AgapayAidSystem.Pages.Family.Profile;
using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;


namespace AgapayAidSystem.Pages.family.edit
{
    public class FamilyModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public FamilyModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public List<BarangayInfo> Barangays { get; set; }
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			string familyID = Request.Query["familyID"];

			// Fetch the list of barangays from database
			Barangays = GetBarangaysFromDatabase();

			// Fetch info of family from the database
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string familySql = "SELECT * FROM family WHERE familyID = @familyID";
					using (MySqlCommand familyCommand = new MySqlCommand(familySql, connection))
					{
						familyCommand.Parameters.AddWithValue("@familyID", familyID);
						using (MySqlDataReader familyReader = familyCommand.ExecuteReader())
						{
							if (familyReader.Read())
							{
								familyInfo.familyID = familyReader.GetString(0);
								familyInfo.streetAddress = familyReader.GetString(1);
								familyInfo.barangayID = familyReader.GetString(2);
								familyInfo.mobileNum = familyReader.GetString(3);
								familyInfo.telephoneNum = familyReader.IsDBNull(4) ? null : familyReader.GetString(4);
								familyInfo.serialNum = familyReader.GetString(5);
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
			bool errorOccurred = false;

			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				errorOccurred = true;
			}

			// Retrieve family details from the form
			familyInfo.familyID = Request.Form["familyID"];
			familyInfo.streetAddress = Request.Form["streetAddress"];
			familyInfo.barangayID = Request.Form["barangayID"];
			familyInfo.mobileNum = Request.Form["mobileNum"];
			familyInfo.telephoneNum = Request.Form["telephoneNum"];

			if (string.IsNullOrWhiteSpace(familyInfo.telephoneNum))
			{
				familyInfo.telephoneNum = null;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "UPDATE family " +
								 "SET streetAddress = @streetAddress, barangayID = @barangayID, " +
								 "mobileNum = @mobileNum, telephoneNum = @telephoneNum " +
								 "WHERE familyID = @familyID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
						command.Parameters.AddWithValue("@barangayID", familyInfo.barangayID);
						command.Parameters.AddWithValue("@mobileNum", familyInfo.mobileNum);
						command.Parameters.AddWithValue("@telephoneNum", familyInfo.telephoneNum);
						command.Parameters.AddWithValue("@familyID", familyInfo.familyID);
						command.ExecuteNonQuery();
						Console.WriteLine("Successfully inserted into 'family' table.");
					}
				}
				successMessage = "Family information updated successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/family/edit/family?familyID=" + familyInfo.familyID + "&errorMessage=" + errorMessage);
			}
			else
			{
				Response.Redirect("/family/profile/index?familyID=" + familyInfo.familyID + "&successMessage=" + successMessage);
			}
		}
	}
}
