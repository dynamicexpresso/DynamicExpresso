using DynamicExpressoWebShell.Services;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Mvc;

namespace DynamicExpressoWebShell.Controllers
{
	public class InterpreterController : Controller
	{
		private readonly WebShell _webShell;

		public InterpreterController(WebShell webShell)
		{
			_webShell = webShell;
		}

		[HttpPost]
		public ActionResult Eval(string expression)
		{
			try
			{
				var result = _webShell.Eval(expression);

				//if (result == null)
				//    return Json(new { success = true, result = "<null>" });

				var prettifyOutput = JsonConvert.SerializeObject(result, Formatting.Indented);

				return Json(new { success = true, result = prettifyOutput });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, result = ex.Message });
			}
		}

	}
}
