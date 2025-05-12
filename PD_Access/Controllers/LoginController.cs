using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Data.OleDb;
using System.Configuration;
using System.Diagnostics.Metrics;
using PD_Access.Models;
using static System.Collections.Specialized.BitVector32;
using Newtonsoft.Json.Linq;
using System.Web;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;


namespace PD_Access.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                // Replace with your authentication logic
                if (model.Username == "admin" && model.Password == "password")
                {
                    // On success, redirect to dashboard or home
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Invalid username or password.";
                }
            }
            return View(model);
        }
    }
}
