using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgapayAidSystem.Pages.disaster.profile.entrylog
{
    public class IndexModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public IndexModel(IConfiguration configuration) => _configuration = configuration;
		public string errorMessage = "";
		public string successMessage = "";

		public void OnGet()
        {
        }
    }
}
