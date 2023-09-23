using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Net;

namespace AgapayAidSystem.Pages.UserManagement
{
    public class CreateECStaffModel : PageModel
    {
        public UserInfo userInfo { get; set; } = new UserInfo();
        public string userID { get; set; } = "";
        public string firstName { get; set; } = "";
        public string middleName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string extName { get; set; } = "";
        public string sex { get; set; } = "";
        public string birthdate { get; set; } = "";
        public string mobileNum { get; set; } = "";
        public string emailAddress { get; set; } = "";
        public string errorMessage = "";
        public string successMessage = "";


        public void OnGet()
        {

        }

        public void OnPost() 
        {
            
        }
    }
}
