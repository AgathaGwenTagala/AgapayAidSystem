using AgapayAidSystem.Pages.Disaster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.EvacuationCenter
{
    public class AddEvacuationCenterModel : PageModel
    {
        public EvacuationInfo evacuationInfo { get; set; } = new EvacuationInfo();
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
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
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

		private bool TryParseInt(string formValue, out int result)
		{
			if (int.TryParse(formValue, out result))
			{
				return true;
			}
			return false;
		}

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve evacuation center details from the form
			evacuationInfo.centerName = Request.Form["centerName"];
			evacuationInfo.centerType = Request.Form["centerType"];
			evacuationInfo.streetAddress = Request.Form["streetAddress"];
			evacuationInfo.barangayID = Request.Form["barangayID"];
			evacuationInfo.mobileNum = Request.Form["mobileNum"];
			evacuationInfo.telephoneNum = Request.Form["telephoneNum"];

			if (TryParseInt(Request.Form["maxCapacity"], out int maxCapacity) &&
				TryParseInt(Request.Form["toilet"], out int toilet) &&
				TryParseInt(Request.Form["bathingCubicle"], out int bathingCubicle) &&
				TryParseInt(Request.Form["communityKitchen"], out int communityKitchen) &&
				TryParseInt(Request.Form["washingArea"], out int washingArea) &&
				TryParseInt(Request.Form["womenChildSpace"], out int womenChildSpace) &&
				TryParseInt(Request.Form["multipurposeArea"], out int multipurposeArea))
			{
				evacuationInfo.maxCapacity = maxCapacity;
				evacuationInfo.toilet = toilet;
				evacuationInfo.bathingCubicle = bathingCubicle;
				evacuationInfo.communityKitchen = communityKitchen;
				evacuationInfo.washingArea = washingArea;
				evacuationInfo.womenChildSpace = womenChildSpace;
				evacuationInfo.multipurposeArea = multipurposeArea;
			}
			else
			{
				errorMessage = "Invalid value for one or more fields. Please enter valid integers.";
				return;
			}

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
				string connectionString = "server=localhost;user=root;database=agapayaid;port=3306;password=12345;";
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'evacuation_center' table
					string sql = "INSERT INTO evacuation_center " +
								 "(centerName, centerType, streetAddress, barangayID, mobileNum, telephoneNum, maxCapacity, " +
								 "toilet, bathingCubicle, communityKitchen, washingArea, womenChildSpace, multipurposeArea) " +
								 "VALUES (@centerName, @centerType, @streetAddress, @barangayID, @mobileNum, @telephoneNum, @maxCapacity, " +
								 "@toilet, @bathingCubicle, @communityKitchen, @washingArea, @womenChildSpace, @multipurposeArea)";

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
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Evacuation center added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/EvacuationCenter/Index");
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}
}
