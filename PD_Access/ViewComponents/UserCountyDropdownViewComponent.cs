using System.Configuration;
using System.Data.OleDb;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using PD_Access.Controllers;
using PD_Access.Models;
using System.Diagnostics;
using static PD_Access.Models.ModifyPolicyModel;
using static System.Collections.Specialized.BitVector32;
using Newtonsoft.Json.Linq;
using System.Web;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Text.Json;

namespace PD_Access.ViewComponents
{
    
    public class UserCountyDropdownViewComponent : ViewComponent
    {
        private readonly ILogger<ModifyPolicyController> _logger;
        private readonly string _connectionString;

        public UserCountyDropdownViewComponent(ILogger<ModifyPolicyController> logger, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AccessDbConnection");
            _logger = logger;
        }

        public class CountyItem
        {
            public string county_Name { get; set; }
            public int county_Value { get; set; }
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userName = (User as ClaimsPrincipal)?.FindFirst("name")?.Value;
            HttpContext.Session.SetString("UserDisplayName", userName);


            var selectedCounty = HttpContext.Request.Query["county"].ToString();

            ViewData["SelectedCounty"] = selectedCounty;


            var counties = new List<CountyItem>();

            if (string.IsNullOrEmpty(userName))
                return View(counties);

            // Check if the user has public policy in the user object. IF yes get all the counties from the local database
            if (HttpContext.Session.GetString("PublicPolicy") == "true" || HttpContext.Session.GetString("ReadOnly") == "true")
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OleDbCommand("SELECT * FROM Counties order by County_Name", connection);
                    var reader = command.ExecuteReader();


                    while (await reader.ReadAsync())
                    {
                        counties.Add(new CountyItem
                        {
                            
                            county_Name = reader["County_Name"].ToString()
                        });
                    }

                }
            }
            else
            {

                var jsonString = HttpContext.Session.GetString("GroupNames");

                // Deserialize to a list of strings
                var groupNames = JsonSerializer.Deserialize<List<string>>(jsonString);

                // Sort the list (optional)
                groupNames.Sort();

                // Convert to list of CountyItem
                 counties = groupNames.Select(name => new CountyItem
                {
                    county_Name = name
                }).ToList();

            }

            //HttpContext.Session.SetString("DefaultCounty", counties[0].county_Name);
            return View(counties);
        }
    }

}
