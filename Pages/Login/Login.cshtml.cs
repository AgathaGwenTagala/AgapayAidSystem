using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AgapayAidSystem.Pages.Login
{
    public class LoginModel : PageModel
    {
        [BindProperty]
		public Credential Credential { get; set; }

        public void OnGet()
        {
        }

        public void OnPost()
        {
        }
    }

    public class Credential
    {
        [Required]
		[Display(Name = "Username:")]
		public string UserName { get; set; }

		[Required]
		[Display(Name = "Password:")]
		[DataType(DataType.Password)]
        public string Password { get; set; }
	}
}
