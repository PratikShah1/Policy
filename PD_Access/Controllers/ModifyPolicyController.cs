using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Configuration;
using static PD_Access.Models.ModifyPolicyModel;
using System.Diagnostics.Metrics;
using PD_Access.Models;
using static System.Collections.Specialized.BitVector32;
using Newtonsoft.Json.Linq;
using System.Web;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;




namespace PD_Access.Controllers
{

    public class ModifyPolicyController : Controller
    {
        private readonly ILogger<ModifyPolicyController> _logger;
        private readonly string _connectionString;

        public ModifyPolicyController(ILogger<ModifyPolicyController> logger, IConfiguration configuration)
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
            var sections = GetSections();
            var model = new PolicyViewModel
            {
                SectionGroupDropdownData = sectionGroupDropdownData,
                SectionNumberDropdownData = sectionNumberDropdownData,
                SectionTitleDropdownData = sectionTitleDropdownData,
                // SavedContent = savedContent
            };

            ViewBag.Sections = sections;
           

            return View(model);

        }
        private List<ModifyPolicyModel> GetSectionGroupDropdownData()
        {
            var data = new List<ModifyPolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT Section_grp_id, Section_Name FROM tbl_section_grp", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new ModifyPolicyModel
                    {
                        Id = reader.GetInt32(0),
                        SectionGroupName = reader.GetString(1)
                    });
                }
            }
            return data;
        }

        private List<ModifyPolicyModel> GetSectionNumberDropdownData()
        {
            var data = new List<ModifyPolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT ID, Section_Number, section_name FROM tbl_section_name_number", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new ModifyPolicyModel
                    {
                        Id = reader.GetInt32(0),
                        section_number = reader.GetInt32(1),
                        section_name = reader.GetString(2)
                    });
                }
            }
            return data;
        }
        private List<ModifyPolicyModel> GetSectionTitleDropdownData()
        {
            var data = new List<ModifyPolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT tbl_section_title.[section_ number], tbl_section_title.section_title\r\nFROM tbl_section_title", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new ModifyPolicyModel
                    {
                        section_number = reader.GetInt32(0),
                        SectionTitle = reader.GetString(1)
                    });
                }
            }
            return data;
        }

        [HttpPost]
        public JsonResult GetSectionGroupDropdownDataByTitle(int sectionTitleId)
        {
            var data = new List<ModifyPolicyModel>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT Section_grp_ID, Section_Name FROM tbl_section_grp WHERE section_number_title = @title_id", connection);
                command.Parameters.AddWithValue("@title_id", sectionTitleId);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new ModifyPolicyModel
                    {
                        Id = reader.GetInt32(0),
                        SectionGroupName = reader.GetString(1)
                    });
                }
            }
            return Json(data);
        }

        public JsonResult GetSectionNumberDropdownDataByGroup(int selectedGroupId)
        {
            var data = new List<ModifyPolicyModel>();
           
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT section_number, Section_Name FROM tbl_section_name_number WHERE section_grp_id = @section_id order by section_number", connection);
                command.Parameters.AddWithValue("@section_id", selectedGroupId);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(new ModifyPolicyModel
                    {
                        section_number = reader.GetInt32(0),
                        SectionGroupName = reader.GetString(1),

                    });
                }
            }
            return Json(data);
        }

        public List<ModifyPolicyModel> GetSectionsByTitle(int sectionTitleId)
        {
            List<ModifyPolicyModel> sections = new List<ModifyPolicyModel>();
            string query = "";
            if (sectionTitleId == -1)
            {
                 query = @"SELECT tbl_section_title.[section_ number], tbl_section_title.section_title, 
                     tbl_section_grp.Section_Name, tbl_section_name_number.Section_Number, 
                     tbl_section_name_number.section_Name,tbl_section_content.section_Content
                     FROM ((tbl_section_title INNER JOIN tbl_section_grp 
                     ON tbl_section_title.[section_ number] = tbl_section_grp.section_number_title) 
                     INNER JOIN tbl_section_name_number 
                     ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id)
                     INNER JOIN tbl_section_content 
                     ON tbl_section_name_number.Section_Number = tbl_section_content.Section_Number
                     ORDER BY tbl_section_name_number.Section_Number;";
            }
            else
            {
                 query = @"SELECT tbl_section_title.[section_ number], tbl_section_title.section_title, 
                     tbl_section_grp.Section_Name, tbl_section_name_number.Section_Number, 
                     tbl_section_name_number.section_Name,tbl_section_content.section_Content
                     FROM ((tbl_section_title INNER JOIN tbl_section_grp 
                     ON tbl_section_title.[section_ number] = tbl_section_grp.section_number_title) 
                     INNER JOIN tbl_section_name_number 
                     ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id)
                     INNER JOIN tbl_section_content 
                     ON tbl_section_name_number.Section_Number = tbl_section_content.Section_Number
                     WHERE tbl_section_title.[section_ number] = @sectionTitleId
                     ORDER BY tbl_section_name_number.Section_Number;";
            }

                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@sectionTitleId", sectionTitleId);
                    connection.Open();
                    OleDbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ModifyPolicyModel policy = new ModifyPolicyModel()
                        {
                            PolicySectionNumber = reader.GetInt32(0),
                            PolicySectionTitle = reader.GetString(1),
                            PolicySectionName = reader.GetString(2),
                            PolicySectionNumberName = reader.GetInt32(3),
                            PolicySectionNameNumber = reader.GetString(4),
                            PolicyText = reader.GetString(5)
                        };
                        sections.Add(policy);
                    }
                }
                return sections;
            
        }
        public List<ModifyPolicyModel> GetSectionsByGroup(int selectedGroupId)
        {
            List<ModifyPolicyModel> sections = new List<ModifyPolicyModel>();
           

            string query = @"SELECT tbl_section_title.[section_ number], tbl_section_title.section_title, 
                     tbl_section_grp.Section_Name, tbl_section_name_number.Section_Number,
                     tbl_section_name_number.section_Name,tbl_section_content.section_Content,tbl_section_content.sec_grp_num
                     FROM ((tbl_section_title INNER JOIN tbl_section_grp 
                     ON tbl_section_title.[section_ number] = tbl_section_grp.section_number_title) 
                     INNER JOIN tbl_section_name_number 
                     ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id)
                     INNER JOIN tbl_section_content 
                     ON tbl_section_name_number.Section_Number = tbl_section_content.Section_Number
                     WHERE  tbl_section_content.[sec_grp_id] = @selectedGroupId
                     ORDER BY tbl_section_name_number.Section_Number;";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@selectedGroupId", selectedGroupId);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ModifyPolicyModel policy = new ModifyPolicyModel()
                    {
                        PolicySectionNumber = reader.GetInt32(0),
                        PolicySectionTitle = reader.GetString(1),
                        PolicySectionName = reader.GetString(2),
                        PolicySectionNumberName = reader.GetInt32(3),
                        PolicySectionNameNumber = reader.GetString(4),
                        PolicyText = reader.GetString(5)
                    };
                    sections.Add(policy);
                }
            }
            return sections;
        }

        public List<ModifyPolicyModel> GetSectionsByNumber(int selectedNumber)
        {
            List<ModifyPolicyModel> sections = new List<ModifyPolicyModel>();
            

            string query = @"SELECT tbl_section_title.[section_ number], tbl_section_title.section_title, 
                     tbl_section_grp.Section_Name, tbl_section_name_number.Section_Number,
                     tbl_section_name_number.section_Name,tbl_section_content.section_Content,tbl_section_content.sec_grp_num
                     FROM ((tbl_section_title INNER JOIN tbl_section_grp 
                     ON tbl_section_title.[section_ number] = tbl_section_grp.section_number_title) 
                     INNER JOIN tbl_section_name_number 
                     ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id)
                     INNER JOIN tbl_section_content 
                     ON tbl_section_name_number.Section_Number = tbl_section_content.Section_Number
                     WHERE  tbl_section_content.[Section_Number] = @selectedNumber
                     ORDER BY tbl_section_name_number.Section_Number;";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@selectedNumber", selectedNumber);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ModifyPolicyModel policy = new ModifyPolicyModel()
                    {
                        PolicySectionNumber = reader.GetInt32(0),
                        PolicySectionTitle = reader.GetString(1),
                        PolicySectionName = reader.GetString(2),
                        PolicySectionNumberName = reader.GetInt32(3),
                        PolicySectionNameNumber = reader.GetString(4),
                        PolicyText = reader.GetString(5)
                    };
                    sections.Add(policy);
                }
            }
            return sections;
        }

        public List<ModifyPolicyModel> GetSections()
        {
            List<ModifyPolicyModel> sections = new List<ModifyPolicyModel>();
           

            string query = @"SELECT tbl_section_title.[section_ number], tbl_section_title.section_title, 
                     tbl_section_grp.Section_Name, tbl_section_name_number.Section_Number, 
                     tbl_section_name_number.section_Name,tbl_section_content.section_Content
                     FROM ((tbl_section_title INNER JOIN tbl_section_grp 
                     ON tbl_section_title.[section_ number] = tbl_section_grp.section_number_title) 
                     INNER JOIN tbl_section_name_number 
                     ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id)
                     INNER JOIN tbl_section_content 
                     ON tbl_section_name_number.Section_Number = tbl_section_content.Section_Number
                     ORDER BY tbl_section_name_number.Section_Number;";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(query, connection);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ModifyPolicyModel policy = new ModifyPolicyModel()
                    {
                        PolicySectionNumber = reader.GetInt32(0),
                        PolicySectionTitle = reader.GetString(1),
                        PolicySectionName = reader.GetString(2),
                        PolicySectionNumberName = reader.GetInt32(3),
                        PolicySectionNameNumber = reader.GetString(4),
                        PolicyText = reader.GetString(5)
                    };
                    GetCommentsByPolicySectionNumberName(policy.PolicySectionNumberName);
                    sections.Add(policy);
                }
            }
            return sections;
        }
        public ActionResult Modify(int id)
        {
            var Policy = GetPolicyById(id);
            //user.Counties = GetAllCounties();
            return PartialView("_EditPolicyPartial", Policy);
        }
        private ModifyPolicyModel GetPolicyById(int id)
        {
            ModifyPolicyModel policy = null;
            ;
            using (var connection = new OleDbConnection(_connectionString))
            {
                var command = new OleDbCommand("SELECT tbl_section_content.section_Content, tbl_section_content.Section_Number, tbl_section_name_number.section_Name, tbl_section_grp.Section_Name, tbl_section_title.section_title\r\nFROM tbl_section_title INNER JOIN (tbl_section_grp INNER JOIN (tbl_section_content INNER JOIN tbl_section_name_number ON tbl_section_content.Section_Number = tbl_section_name_number.Section_Number) ON tbl_section_grp.Section_grp_id = tbl_section_name_number.section_grp_id) ON (tbl_section_content.title_id = tbl_section_title.title_ID) AND (tbl_section_title.[section_ number] = tbl_section_grp.section_number_title)\r\nWHERE (((tbl_section_content.Section_Number)=@Id))", connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    policy = new ModifyPolicyModel
                    {
                        PolicyText = reader.GetString(0),
                        PolicySectionNumberName = reader.GetInt32(1),
                        PolicySectionNameNumber = reader.GetString(2),
                        PolicySectionName = reader.GetString(3),
                        PolicySectionTitle = reader.GetString(4),
                    };
                }
            }
            return policy;
        }

        [HttpPost]
        public JsonResult Save([FromBody] ModifyPolicyModel model)
        {
            
                using (var connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OleDbCommand("INSERT INTO tbl_content_comment (section_number, what_change, why_change, user_id,addelmod) VALUES (@PolicySectionNumberName, @What, @Why, @UserId, @Action)", connection);
                    command.Parameters.AddWithValue("@PolicySectionNumberName", model.PolicySectionNumberName);
                    command.Parameters.AddWithValue("@What", model.modify_what);
                    command.Parameters.AddWithValue("@Why", model.modify_why);
                    command.Parameters.AddWithValue("@UserId", model.modify_user_id);
                    command.Parameters.AddWithValue("@Action", model.modify_value);
                    command.ExecuteNonQuery();
                }
                return Json(new { success = true });
          
            return Json(new { success = false, message = "Validation failed." });
        }


        public List<ModifyPolicyModel> GetCommentsByPolicySectionNumberName(int policySectionNumberName)
        {
            var comments = new List<ModifyPolicyModel>();
          
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT what_change, why_change, user_id,addelmod FROM tbl_content_comment WHERE section_number = @PolicySectionNumberName order by user_id", connection);
                command.Parameters.AddWithValue("@PolicySectionNumberName", policySectionNumberName);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    comments.Add(new ModifyPolicyModel
                    {
                        modify_what = reader.GetString(0),
                        modify_why = reader.GetString(1),
                        modify_user_id = reader.GetString(2),
                        modify_action = reader.GetInt32(3) == 1 ? "Add" :
                                        reader.GetInt32(3) == 2 ? "Delete" :
                                        reader.GetInt32(3) == 3 ? "Modify" : "Unknown"

                    });
                }
            }
            return comments;
        }

        [HttpGet]
        public JsonResult GetLikedSections(string userId)
        {
            var likedSectionIds = new List<int>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT section_number FROM tbl_content_like WHERE user_id = @userId", connection);
                command.Parameters.AddWithValue("@userId", userId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    likedSectionIds.Add(reader.GetInt32(0));
                }
            }

            return Json(likedSectionIds);
        }

        [HttpPost]
        public JsonResult ToggleLike(int sectionId, string userId)
        {
            bool alreadyLiked = false;

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                // Check if like already exists
                var checkCmd = new OleDbCommand("SELECT COUNT(*) FROM tbl_content_like WHERE section_number = @section AND user_id = @user", connection);
                checkCmd.Parameters.AddWithValue("@section", sectionId);
                checkCmd.Parameters.AddWithValue("@user", userId);

                int count = (int)checkCmd.ExecuteScalar();
                alreadyLiked = count > 0;

                if (alreadyLiked)
                {
                    var deleteCmd = new OleDbCommand("DELETE FROM tbl_content_like WHERE section_number = @section AND user_id = @user", connection);
                    deleteCmd.Parameters.AddWithValue("@section", sectionId);
                    deleteCmd.Parameters.AddWithValue("@user", userId);
                    deleteCmd.ExecuteNonQuery();
                }
                else
                {
                    var insertCmd = new OleDbCommand("INSERT INTO tbl_content_like (section_number, user_id) VALUES (@section, @user)", connection);
                    insertCmd.Parameters.AddWithValue("@section", sectionId);
                    insertCmd.Parameters.AddWithValue("@user", userId);
                    insertCmd.ExecuteNonQuery();
                }
            }

            return Json(new { liked = !alreadyLiked });
        }
        [HttpGet]
        public JsonResult GetLikeCounts()
        {
            var counts = new Dictionary<int, int>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT section_number, COUNT(*) FROM tbl_content_like GROUP BY section_number", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int section = reader.GetInt32(0);
                    int count = reader.GetInt32(1);
                    counts[section] = count;
                }
            }

            return Json(counts);
        }
        [HttpGet]
        public JsonResult GetLikersBySection(int sectionNumber)
        {
            var users = new List<object>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT user_id, County FROM tbl_content_like WHERE section_number = @section", connection);
                command.Parameters.AddWithValue("@section", sectionNumber);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new
                    {
                        UserId = reader.GetString(0),
                        County = reader.GetInt32(1)
                    });
                }
            }

            return Json(users);
        }


    }
}

