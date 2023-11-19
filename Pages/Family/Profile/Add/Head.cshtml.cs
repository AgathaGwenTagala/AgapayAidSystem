using AgapayAidSystem.Pages.Family;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace AgapayAidSystem.Pages.Family.Profile.Add
{
	public class HeadModel : PageModel
	{
		private readonly IConfiguration _configuration;
		public HeadModel(IConfiguration configuration) => _configuration = configuration;
		public AddFamilyHeadInfo addfamilyheadInfo { get; set; } = new AddFamilyHeadInfo();
		public List<CivilStatus> Status { get; set; }
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
		{
			// Fetch the list of barangays from database
			Status = GetStatusFromDatabase();
		}

		private List<CivilStatus> GetStatusFromDatabase()
		{
			var statusData = new List<CivilStatus>();

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					string sql = "SELECT memberID, civilStatus FROM family_member";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								statusData.Add(new CivilStatus
								{
									memberID = reader.GetString(0),
									civilStatus = reader.GetString(1)
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

			return statusData;
		}

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Please correct the errors below.";
				return;
			}

			// Retrieve disaster details from the form
			addfamilyheadInfo.firstName = Request.Form["firstName"];
			addfamilyheadInfo.middleName = Request.Form["middleName"];
			addfamilyheadInfo.lastName = Request.Form["lastName"];
			addfamilyheadInfo.extName = Request.Form["extName"];
			addfamilyheadInfo.sex = Request.Form["sex"];
			addfamilyheadInfo.birthdate = Request.Form["birthdate"];
			addfamilyheadInfo.relationship = Request.Form["relationship"];
			addfamilyheadInfo.civilStatus = Request.Form["civilStatus"];
			addfamilyheadInfo.education = Request.Form["education"];
			addfamilyheadInfo.occupation = Request.Form["occupation"];
			addfamilyheadInfo.religion = Request.Form["religion"];
			addfamilyheadInfo.ethnicity = Request.Form["ethnicity"];
			addfamilyheadInfo.healthCondition = Request.Form["healthCondition"];
			addfamilyheadInfo.remarks = Request.Form["remarks"];


			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'family_member' table
					string sql = "INSERT INTO family_member" +
								 "(firstName, middleName, lastName, extName, sex, birthdate, civilStatus, education, occupation, religion, ethnicity, healthCondition, remarks) " +
								 "VALUES (@firstName, @middleName, @lastName, @extName, @sex, @birthdate, @civilStatus, @education, @occupation, @religion, @ethnicity, @healthCondition, @remarks)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@firstName", addfamilyheadInfo.firstName);
						command.Parameters.AddWithValue("@middleName", addfamilyheadInfo.middleName);
						command.Parameters.AddWithValue("@lastName", addfamilyheadInfo.lastName);
						command.Parameters.AddWithValue("@extName", addfamilyheadInfo.extName);
						command.Parameters.AddWithValue("@sex", addfamilyheadInfo.sex);
						command.Parameters.AddWithValue("@birthdate", addfamilyheadInfo.birthdate);
						command.Parameters.AddWithValue("@relationship", addfamilyheadInfo.relationship);
						command.Parameters.AddWithValue("@civilStatus", addfamilyheadInfo.civilStatus);
						command.Parameters.AddWithValue("@education", addfamilyheadInfo.education);
						command.Parameters.AddWithValue("@occupation", addfamilyheadInfo.occupation);
						command.Parameters.AddWithValue("@religion", addfamilyheadInfo.religion);
						command.Parameters.AddWithValue("@ethnicity", addfamilyheadInfo.ethnicity);
						command.Parameters.AddWithValue("@healthCondition", addfamilyheadInfo.healthCondition);
						command.Parameters.AddWithValue("@remarks", addfamilyheadInfo.remarks);
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Family head added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			try
			{
				string connectionString = _configuration.GetConnectionString("DefaultConnection");
				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					// Insert data into the 'family_member' table
					string sql = "INSERT INTO family_member (relationship) VALUES (@relationship)";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@relationship", "Family head");
						command.ExecuteNonQuery();
					}
				}

				successMessage = "Family head added successfully!";
			}

			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			Response.Redirect("/family/index?errorMessage=" + errorMessage + "&successMessage=" + successMessage);
		}
	}

	public class CivilStatus
	{
		public string memberID { get; set; }
		public string civilStatus { get; set; }
	}

	public class AddFamilyHeadInfo
	{
		public string firstName { get; set; }
		public string middleName { get; set; }
		public string lastName { get; set; }
		public string extName { get; set; }
		public string sex { get; set; }
		public string birthdate { get; set; }
		public string relationship { get; set; }
		public string civilStatus { get; set; }
		public string education { get; set; }
		public string occupation { get; set; }
		public string religion { get; set; }
		public string ethnicity { get; set; }
		public string healthCondition { get; set; }
		public string remarks { get; set; }
		public string memberID { get; set; }
		public string familyID { get; set; }
		public string serialNum { get; set; }
	}
}

