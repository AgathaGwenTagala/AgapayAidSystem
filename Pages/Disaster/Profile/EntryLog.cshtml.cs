using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgapayAidSystem.Pages.Disaster.Profile
{
    public class EntryLogModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public EntryLogModel(IConfiguration configuration) => _configuration = configuration;
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
        {
        }
    }
}
