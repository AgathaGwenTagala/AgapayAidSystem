using AgapayAidSystem.Pages.Family;
using AgapayAidSystem.Pages.Family.Profile.Add;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AgapayAidSystem.Pages.family.add
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public FamilyInfo familyInfo { get; set; } = new FamilyInfo();
		public MemberInfo memberInfo { get; set; } = new MemberInfo();
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
					string newFamilyID;
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
						Console.WriteLine($"Successfully inserted family head for familyID {familyInfo.familyID}");
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
				Response.Redirect("/family/add/index?errorMessage=" + errorMessage);
			}
			else
			{
				// Redirect to the Add Member page after successful add
				Response.Redirect("/family/add/member?successMessage=" + successMessage);
			}
		}
	}

	public class BarangayInfo
	{
		public string barangayID { get; set; }
		public string barangayName { get; set; }
	}

	public class MemberInfo
	{
		public string memberID { get; set; }
		public string familyID { get; set; }
		public string fullName { get; set; }
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string extName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string age { get; set; }
		public string relationship { get; set; }
		public string civilStatus { get; set; }
		public string education { get; set; }
		public string occupation { get; set; }
		public string religion { get; set; }
		public string isIndigenousPerson { get; set; }
		public string healthCondition { get; set; }
		public string remarks { get; set; }
	}
}
