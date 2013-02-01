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
        public ActionResult Eval(string expression)
        {
            try
            {
                var interpreter = new DynamicExpresso.Interpreter();
                var result = interpreter.Eval(expression);

                if (result == null)
                    return Content("<null>");

                return Json(new { success = true, result = result.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, result = ex.Message });
            }
        }

    }
}
