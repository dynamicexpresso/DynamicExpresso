using DynamicExpressoWebShell.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicExpressoWebShell.AspNetCore.Controllers
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

                string prettifyOutput = JsonConvert.SerializeObject(result, Formatting.Indented);

                return Ok(new { success = true, result = prettifyOutput });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, result = ex.Message });
            }
        }

    }
}
