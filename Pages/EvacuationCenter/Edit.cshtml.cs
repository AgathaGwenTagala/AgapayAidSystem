using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.EvacuationCenter
{
    public class EditEvacuationCenterModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public EditEvacuationCenterModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationInfo evacuationInfo { get; set; } = new EvacuationInfo();
		public List<BarangayInfo> Barangays { get; set; }
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
        {
			string centerID = Request.Query["centerID"];

			// Fetch the list of barangays from database
			Barangays = GetBarangaysFromDatabase();

			// Fetch info of selected evacuation center from the database
			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM evacuation_center where centerID = @centerID";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerID", centerID);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
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
			evacuationInfo.centerID = Request.Form["centerID"];
			evacuationInfo.centerName = Request.Form["centerName"];
			evacuationInfo.centerType = Request.Form["centerType"];
			evacuationInfo.streetAddress = Request.Form["streetAddress"];
			evacuationInfo.barangayID = Request.Form["barangayID"];
			evacuationInfo.mobileNum = Request.Form["mobileNum"];
			evacuationInfo.telephoneNum = Request.Form["telephoneNum"];
			evacuationInfo.maxCapacity = Request.Form["maxCapacity"];
			evacuationInfo.toilet = Request.Form["toilet"];
			evacuationInfo.bathingCubicle = Request.Form["bathingCubicle"];
			evacuationInfo.communityKitchen = Request.Form["communityKitchen"];
			evacuationInfo.washingArea = Request.Form["washingArea"];
			evacuationInfo.womenChildSpace = Request.Form["womenChildSpace"];
			evacuationInfo.multipurposeArea = Request.Form["multipurposeArea"];

			// Check if mobileNum is empty and set it to null
			if (string.IsNullOrWhiteSpace(evacuationInfo.mobileNum))
			{
				evacuationInfo.mobileNum = null;
			}

			// Check if telephoneNum is empty and set it to null
			if (string.IsNullOrWhiteSpace(evacuationInfo.telephoneNum))
			{
				evacuationInfo.telephoneNum = null;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert updated data into the 'evacuation_center' table
					string sql = "UPDATE evacuation_center " +
							     "SET centerName = @centerName, centerType = @centerType, streetAddress = @streetAddress, " +
								 "barangayID = @barangayID, mobileNum = @mobileNum, telephoneNum = @telephoneNum, maxCapacity = @maxCapacity, " +
								 "toilet = @toilet, bathingCubicle = @bathingCubicle, communityKitchen = @communityKitchen, " +
								 "washingArea = @washingArea, womenChildSpace = @womenChildSpace, multipurposeArea = @multipurposeArea " +
								 "WHERE centerID = @centerID";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@centerName", evacuationInfo.centerName);
						command.Parameters.AddWithValue("@centerType", evacuationInfo.centerType);
						command.Parameters.AddWithValue("@streetAddress", evacuationInfo.streetAddress);
						command.Parameters.AddWithValue("@barangayID", evacuationInfo.barangayID);
						command.Parameters.AddWithValue("@mobileNum", evacuationInfo.mobileNum);
						command.Parameters.AddWithValue("@telephoneNum", evacuationInfo.telephoneNum);
						command.Parameters.AddWithValue("@maxCapacity", evacuationInfo.maxCapacity);
						command.Parameters.AddWithValue("@toilet", evacuationInfo.toilet);
						command.Parameters.AddWithValue("@bathingCubicle", evacuationInfo.bathingCubicle);
						command.Parameters.AddWithValue("@communityKitchen", evacuationInfo.communityKitchen);
						command.Parameters.AddWithValue("@washingArea", evacuationInfo.washingArea);
						command.Parameters.AddWithValue("@womenChildSpace", evacuationInfo.womenChildSpace);
						command.Parameters.AddWithValue("@multipurposeArea", evacuationInfo.multipurposeArea);
						command.Parameters.AddWithValue("@centerID", evacuationInfo.centerID);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Evacuation center updated successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/evacuationcenter/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}
}
