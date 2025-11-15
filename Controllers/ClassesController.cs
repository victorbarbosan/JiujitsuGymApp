using Microsoft.AspNetCore.Mvc;


namespace JiujitsuGymApp.Controllers
{
	public class ClassesController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
