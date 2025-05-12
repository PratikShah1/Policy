using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PD_Access.Models;
using System.Data.OleDb;
using System.Configuration;
using static PD_Access.Models.PolicyModel;
using System.Diagnostics.Metrics;

namespace PD_Access.Controllers
{
    public class PolicyController : Controller
    {
        private readonly ILogger<PolicyController> _logger;
        private readonly string _connectionString;

        public PolicyController(ILogger<PolicyController> logger, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AccessDbConnection");
            _logger = logger;
        }
        public IActionResult Index()
        {


            var sectionGroupDropdownData = GetSectionGroupDropdownData();
            var sectionNumberDropdownData = GetSectionNumberDropdownData();
            var sectionTitleDropdownData = GetSectionTitleDropdownData();
           // var savedContent = GetSavedContent();

            var model = new PolicyViewModel
            {
                SectionGroupDropdownData = sectionGroupDropdownData,
                SectionNumberDropdownData = sectionNumberDropdownData,
                SectionTitleDropdownData = sectionTitleDropdownData,
               // SavedContent = savedContent
            };
            return View(model);


        }

        private List<PolicyModel> GetSectionGroupDropdownData()
        {
            var data = new List<PolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT Section_ID, Section_Name FROM tbl_section_grp", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new PolicyModel
                    {
                        Id = reader.GetInt32(0),
                        SectionGroupName = reader.GetString(1)
                    });
                }
            }
            return data;
        }

        private List<PolicyModel> GetSectionNumberDropdownData()
        {
            var data = new List<PolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT ID, Section_Number FROM tbl_section_number", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new PolicyModel
                    {
                        Id = reader.GetInt32(0),
                        SectionNumber = reader.GetString(1)
                    });
                }
            }
            return data;
        }
        private List<PolicyModel> GetSectionTitleDropdownData()
        {
            var data = new List<PolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT title_id, Section_Title FROM tbl_section_title", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new PolicyModel
                    {
                        Id = reader.GetInt32(0),
                        SectionTitle = reader.GetString(1)
                    });
                }
            }
            return data;
        }

        //private string GetSavedContent()
        //{
        //    string content = string.Empty;
        //    using (var connection = new OleDbConnection(_connectionString))
        //    {
        //        connection.Open();
        //        var command = new OleDbCommand("SELECT TOP 1 section_content FROM tbl_section_content ORDER BY ID DESC", connection);
        //        var reader = command.ExecuteReader();
        //        if (reader.Read())
        //        {
        //            content = reader.GetString(0);
        //        }
        //    }
        //    return content;
        //}


        [HttpPost]
            public ActionResult Save(string dropdown1, string dropdown2, string editor1)
            {
        
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OleDbCommand("INSERT INTO tbl_section_content (section_content) VALUES (@Content)", connection);
                    //command.Parameters.AddWithValue("@SectionGroupID", dropdown1);
                    //command.Parameters.AddWithValue("@SectionNumberID", dropdown2);
                    //command.Parameters.AddWithValue("@SectionTitleID", dropdown3);
                    command.Parameters.AddWithValue("@Content", editor1);
                    command.ExecuteNonQuery();
                }

                
                return RedirectToAction("Index"); ;
            }

        }









    }
}
