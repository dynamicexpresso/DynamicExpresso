using DynamicExpressoWebShell.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DynamicExpressoWebShell.Controllers
{
    public class InterpreterController : Controller
    {
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Eval(string expression)
        {
            try
            {
                var result = WebShell.Current.Eval(expression);

                //if (result == null)
                //    return Json(new { success = true, result = "<null>" });

                string prettifyOutput = JsonConvert.SerializeObject(result, Formatting.Indented);

                return Json(new { success = true, result = prettifyOutput });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, result = ex.Message });
            }
        }

    }
}
