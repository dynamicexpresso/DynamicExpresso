using System.Diagnostics;
using DynamicExpressoWebShell.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicExpressoWebShell.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
