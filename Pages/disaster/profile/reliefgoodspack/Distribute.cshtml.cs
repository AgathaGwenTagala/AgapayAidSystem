using AgapayAidSystem.Pages.Disaster.Profile.reliefgoodspack;
using AgapayAidSystem.Pages.Disaster.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgapayAidSystem.Pages.disaster.profile.reliefgoodspack
{
    public class DistributeModel : PageModel
    {
		private readonly IConfiguration _configuration;
		public DistributeModel(IConfiguration configuration) => _configuration = configuration;
		public EvacuationCenterLogInfo logInfo { get; set; } = new EvacuationCenterLogInfo();
		public PackInclusionInfo packInclusionInfo { get; set; } = new PackInclusionInfo();
		public string errorMessage = "";
		public string successMessage = "";
		public string packID = "";

		public void OnGet()
        {
			string centerLogID = Request.Query["centerLogID"];
			packID = Request.Query["packID"];
		}
    }
}
