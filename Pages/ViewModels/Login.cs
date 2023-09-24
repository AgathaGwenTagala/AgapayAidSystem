using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AgapayAidSystem.Pages.ViewModels
{
	public class Login
	{
		[Required]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
