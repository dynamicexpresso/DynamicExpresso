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
                if (expression != null && expression.Length > 200)
                    throw new Exception("Live demo doesn't support expression with more than 200 characters.");

                var interpreter = new DynamicExpresso.Interpreter();
                var result = interpreter.Eval(expression);

                if (result == null)
                    return Json(new { success = true, result = "<null>" });

                return Json(new { success = true, result = result.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, result = ex.Message });
            }
        }

    }
}
