using AgapayAidSystem.Pages.disaster.profile.entrylog;
using AgapayAidSystem.Pages.Family;
using AgapayAidSystem.Pages.Family.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Disaster.Profile.entrylog.addfamily
{
	public class IndexModel : PageModel
	{
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public MemberInfo memberInfo { get; set; } = new MemberInfo();
		public List<BarangayInfo> Barangays { get; set; }
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			// Fetch the list of barangays from database
			Barangays = GetBarangaysFromDatabase();

			string centerLogID = Request.Query["centerLogID"];

			if (string.IsNullOrEmpty(centerLogID))
			{
				errorMessage = "Invalid centerLogID.";
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Fetch info of selected center log from the database
					string logSql = "SELECT log.centerLogID, d.disasterID, d.disasterName, ec.centerName " +
									"FROM evacuation_center_log AS log " +
									"INNER JOIN evacuation_center AS ec ON log.centerID = ec.centerID " +
									"INNER JOIN disaster AS d ON log.disasterID = d.disasterID " +
									"WHERE log.centerLogID = @centerLogID";
					using (MySqlCommand logCommand = new MySqlCommand(logSql, connection))
					{
						logCommand.Parameters.AddWithValue("@centerLogID", centerLogID);
						using (MySqlDataReader logReader = logCommand.ExecuteReader())
						{
							if (logReader.Read())
							{
								logInfo.centerLogID = logReader.GetString(0);
								logInfo.disasterID = logReader.GetString(1);
								logInfo.disasterName = logReader.GetString(2);
								logInfo.centerName = logReader.GetString(3);
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
			familyInfo.streetAddress = Request.Form["streetAddress"];
			familyInfo.barangayID = Request.Form["barangayID"];
			familyInfo.mobileNum = Request.Form["mobileNum"];
			familyInfo.telephoneNum = Request.Form["telephoneNum"];

			// Retrieve family head details from the form
			memberInfo.firstName = Request.Form["firstName"];
			memberInfo.middleName = Request.Form["middleName"];
			memberInfo.lastName = Request.Form["lastName"];
			memberInfo.extName = Request.Form["extName"];
			memberInfo.sex = Request.Form["sex"];
			memberInfo.birthdate = Request.Form["birthdate"];
			memberInfo.relationship = "Family head";
			memberInfo.civilStatus = Request.Form["civilStatus"];
			memberInfo.education = Request.Form["education"];
			memberInfo.occupation = Request.Form["occupation"];
			memberInfo.religion = Request.Form["religion"];
			memberInfo.isIndigenousPerson = Request.Form["isIndigenousPerson"];
			memberInfo.healthCondition = Request.Form["healthCondition"];
			memberInfo.remarks = Request.Form["remarks"];

			// Others
			string centerLogID = Request.Form["centerLogID"];
			string redirectType = Request.Form["redirectType"];
			string newFamilyID = null;

			if (string.IsNullOrEmpty(memberInfo.middleName))
			{
				memberInfo.middleName = null;
			}

			if (string.IsNullOrEmpty(memberInfo.extName))
			{
				memberInfo.extName = null;
			}

			if (string.IsNullOrEmpty(memberInfo.healthCondition))
			{
				memberInfo.healthCondition = "No existing health condition";
			}

			if (string.IsNullOrEmpty(memberInfo.remarks))
			{
				memberInfo.remarks = null;
			}

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

					// Step 1: Insert family details into the 'family' table
					string sql = "INSERT INTO family " +
								 "(streetAddress, barangayID, mobileNum, telephoneNum) " +
								 "VALUES (@streetAddress, @barangayID, @mobileNum, @telephoneNum);";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
						command.Parameters.AddWithValue("@barangayID", familyInfo.barangayID);
						command.Parameters.AddWithValue("@mobileNum", familyInfo.mobileNum);
						command.Parameters.AddWithValue("@telephoneNum", familyInfo.telephoneNum);
						command.ExecuteNonQuery();
						Console.WriteLine("Successfully inserted into 'family' table.");
					}

					// Step 2: Retrieve the last inserted familyID from the 'family' table
					string selectMaxFamilyIDSql = "SELECT MAX(familyID) FROM family WHERE streetAddress = @streetAddress";
					using (MySqlCommand selectMaxFamilyID = new MySqlCommand(selectMaxFamilyIDSql, connection))
					{
						selectMaxFamilyID.Parameters.AddWithValue("@streetAddress", familyInfo.streetAddress);
						newFamilyID = selectMaxFamilyID.ExecuteScalar()?.ToString();
					}
					Console.WriteLine($"Retrieved new FamilyID: {newFamilyID} with streetAddress {familyInfo.streetAddress}");

					// Step 3: Insert family head details into the 'family_member' table
					string memberSql = "INSERT INTO family_member " +
									   "(familyID, firstName, middleName, lastName, extName, sex, birthdate, relationship, " +
									   "civilStatus, education, occupation, religion, isIndigenousPerson, healthCondition, remarks) " +
									   "VALUES (@familyID, @firstName, @middleName, @lastName, @extName, @sex, @birthdate, @relationship, " +
									   "@civilStatus, @education, @occupation, @religion, @isIndigenousPerson, @healthCondition, @remarks)";
					using (MySqlCommand memberCommand = new MySqlCommand(memberSql, connection))
					{
						memberCommand.Parameters.AddWithValue("@familyID", newFamilyID);
						memberCommand.Parameters.AddWithValue("@firstName", memberInfo.firstName);
						memberCommand.Parameters.AddWithValue("@middleName", memberInfo.middleName);
						memberCommand.Parameters.AddWithValue("@lastName", memberInfo.lastName);
						memberCommand.Parameters.AddWithValue("@extName", memberInfo.extName);
						memberCommand.Parameters.AddWithValue("@sex", memberInfo.sex);
						memberCommand.Parameters.AddWithValue("@birthdate", memberInfo.birthdate);
						memberCommand.Parameters.AddWithValue("@relationship", memberInfo.relationship);
						memberCommand.Parameters.AddWithValue("@civilStatus", memberInfo.civilStatus);
						memberCommand.Parameters.AddWithValue("@education", memberInfo.education);
						memberCommand.Parameters.AddWithValue("@occupation", memberInfo.occupation);
						memberCommand.Parameters.AddWithValue("@religion", memberInfo.religion);
						memberCommand.Parameters.AddWithValue("@isIndigenousPerson", memberInfo.isIndigenousPerson);
						memberCommand.Parameters.AddWithValue("@healthCondition", memberInfo.healthCondition);
						memberCommand.Parameters.AddWithValue("@remarks", memberInfo.remarks);
						memberCommand.ExecuteNonQuery();
						Console.WriteLine($"Successfully inserted family head for familyID {newFamilyID}");
					}
				}
				successMessage = "Family added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				errorOccurred = true;
			}

			if (errorOccurred)
			{
				// Show an error message banner on the current page
				Response.Redirect("/disaster/profile/entrylog/addfamily/index?centerLogID=" + centerLogID + "&errorMessage=" + errorMessage);
			}
			else
			{
				if (redirectType == "add")
				{
					Response.Redirect("/disaster/profile/entrylog/addfamily/member?familyID=" + newFamilyID + "&centerLogID=" + centerLogID);
				}
				else
				{
					Response.Redirect("/disaster/profile/entrylog/checkin?centerLogID=" + centerLogID + "&successMessage=" + successMessage);
				}
			}
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}
}