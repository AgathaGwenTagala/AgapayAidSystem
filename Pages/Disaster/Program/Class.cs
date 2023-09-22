using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgapayAidSystem.Pages.Disaster.Program
{
	public class Class
	{
		[Key] // This annotation specifies that DisasterID is the primary key
		public string DisasterID { get; set; }
		[Required]

		public string DisasterName { get; set; }
		public DisasterType disasterType { get; set; }
		public string Description { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public enum DisasterType
	{
		Typhoon,
		Flood,
		Landslide,
		Earthquake,
		VolcanicEruption,
		Tsunami,
		Fire,
		IndustrialAccident,
		Epidemic
	}
}
