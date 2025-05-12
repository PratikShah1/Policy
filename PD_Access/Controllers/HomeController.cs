using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PD_Access.Models;
using System.Data.OleDb;
using System.Configuration;
using static PD_Access.Models.UserView;
using System.Diagnostics.Metrics;

namespace PD_Access.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AccessDbConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
            var users = GetUsersFromDatabase();
            return View(users);
        }

        private List<UserView> GetUsersFromDatabase()
        {
            var users = new List<UserView>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                using (var command = new OleDbCommand("SELECT User_ID, FirstName, LastName, Email FROM Users", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserView
                            { 
                                Id = Convert.ToInt32(reader["User_ID"]),
                                FirstName = reader["FirstName"].ToString() ?? string.Empty,
                                LastName = reader["LastName"].ToString() ?? string.Empty,
                                Email = reader["Email"].ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }

            return users;
        }

        public ActionResult Edit(int id)
        {
            var user = GetUserById(id);
            //user.Counties = GetAllCounties();
            return PartialView("_EditUserPartial", user);
        }


        [HttpPost]
        public ActionResult Save(UserView user)
        {


            try
            {
                if (user.Id == 0)
                {
                    AddUser(user);
                }
                else
                {
                    UpdateUser(user);
                }
                UpdateUserRoles(user.Id, user.Roles);
                UpdateUserCounties (user.Id, user.Counties);
                return RedirectToAction("Index");
            }


            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Save post action");
                return View("Error");
            }

        }


        private void UpdateUserRoles(int userId, List<Roles> roles)
        {
            try
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();

                    // First, delete existing roles for the user
                    var deleteCommand = new OleDbCommand("DELETE FROM User_Role WHERE User_ID = @UserId", connection);
                    deleteCommand.Parameters.AddWithValue("@UserId", userId);
                    deleteCommand.ExecuteNonQuery();

                    // Then, insert updated roles
                    foreach (var role in roles)
                    {
                        if (role.IsAssigned)
                        {
                            var insertCommand = new OleDbCommand("INSERT INTO User_Role (User_ID, Roles_ID) VALUES (@UserId, @RoleId)", connection);
                            insertCommand.Parameters.AddWithValue("@UserId", userId);
                            insertCommand.Parameters.AddWithValue("@RoleId", role.role_id);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles");
            }
        }

        private void UpdateUserCounties(int userId, List<County> counties)
        {
            try
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();

                    // First, delete existing roles for the user
                    var deleteCommand = new OleDbCommand("DELETE FROM User_County WHERE User_ID = @UserId", connection);
                    deleteCommand.Parameters.AddWithValue("@UserId", userId);
                    deleteCommand.ExecuteNonQuery();

                    // Then, insert updated roles
                    foreach (var county in counties)
                    {
                        if (county.IsAssigned)
                        {
                            var insertCommand = new OleDbCommand("INSERT INTO User_County (User_ID, County_ID) VALUES (@UserId, @CountyId)", connection);
                            insertCommand.Parameters.AddWithValue("@UserId", userId);
                            insertCommand.Parameters.AddWithValue("@CountyId", county.County_ID);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles");
            }
        }

        private UserView GetUserById(int id)
        {
            UserView user = null;
           ;
            using (var connection = new OleDbConnection(_connectionString))
            {
                var command = new OleDbCommand("SELECT U.User_ID, U.FirstName, U.LastName,U.Email, UR.Roles_ID FROM Users U INNER JOIN User_Role UR ON U.User_ID = UR.User_ID WHERE U.User_ID = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new UserView
                    {
                        Id = (int)reader["User_ID"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString() ?? string.Empty,
                        Email = reader["Email"].ToString() ?? string.Empty,
                        Roles = GetRolesByUserId(id),
                        Counties = GetAllCounties(),
                        
                    };
                }

                // Mark assigned counties
                var assignedCounties = GetCountiesByUserId(id);
                foreach (var county in user.Counties)
                {
                    if (assignedCounties.Any(ac => ac.County_ID == county.County_ID))
                    {
                        county.IsAssigned = true;
                    }
                }


            }


            return user;
           
        }

        private List<Roles> GetRolesByUserId(int userId)
        {
            var roles = new List<Roles>();
            using (var connection = new OleDbConnection(_connectionString))
            {


                var command = new OleDbCommand("SELECT UR.User_ID,R.Role_ID, IIf(UR.User_ID Is Not Null,1,0) AS Role_Assigned\r\nFROM Roles AS R RIGHT JOIN User_Role AS UR ON R.Role_ID = UR.Roles_ID  WHERE UR.User_ID = @Id", connection);
                command.Parameters.AddWithValue("@Id", userId);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    roles.Add(new Roles
                    {
                        Id = (int)reader["User_ID"],
                        role_id = (int)reader["Role_ID"],
                        IsAssigned = (int)reader["Role_Assigned"] == 1
                    });
                }
            }
            return roles;
        }

        private List<County> GetCountiesByUserId(int userId)
        {
            var counties = new List<County>();
            using (var connection = new OleDbConnection(_connectionString))
            {


                var command = new OleDbCommand("SELECT UC.User_ID,C.County_ID, IIf(UC.User_ID Is Not Null,1,0) AS County_Assigned\r\nFROM Counties AS C RIGHT JOIN User_County AS UC ON C.County_ID = UC.County_ID  WHERE UC.User_ID = @Id", connection);
                command.Parameters.AddWithValue("@Id", userId);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    counties.Add(new County
                    {
                        Id = (int)reader["User_ID"],
                        County_ID = (int)reader["County_ID"],
                        IsAssigned = (int)reader["County_Assigned"] == 1
                    });
                }
            }
            return counties;
        }

        private void UpdateUser(UserView user)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                var command = new OleDbCommand("UPDATE Users SET FirstName = @FirstName, LastName = @LastName,Email = @Email WHERE User_Id = @Id", connection);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Id", user.Id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public ActionResult Create()
        {
            var newUser = new UserView
            {
                Roles = new List<Roles>
        {
            new Roles { role_id = 1, IsAssigned = false },
            new Roles { role_id = 2, IsAssigned = false },
            new Roles { role_id = 3, IsAssigned = false },
            new Roles { role_id = 4, IsAssigned = false }
        },
                Counties = GetAllCounties()
            };
            return PartialView("_EditUserPartial", newUser);
        }

        [HttpPost]
        public ActionResult Create(UserView user)
        {
            try
            {
                AddUser(user);
                UpdateUserRoles(user.Id, user.Roles);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create post action");
                return View("Error");
            }
        }

        private void AddUser(UserView user)
        {
            try
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    var command = new OleDbCommand("INSERT INTO Users (FirstName, LastName, Email) VALUES (@FirstName, @LastName, @Email)", connection);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    connection.Open();
                    command.ExecuteNonQuery();

                    // Get the newly inserted user's ID
                    command = new OleDbCommand("SELECT @@IDENTITY", connection);
                    user.Id = (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new user");
            }
        }

        private List<County> GetAllCounties()
        {
            var counties = new List<County>();

            try
            {
                using (var connection = new OleDbConnection(_connectionString))
                {
                    var command = new OleDbCommand("SELECT County_ID, County_Name FROM Counties", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        counties.Add(new County
                        {
                            County_ID = (int)reader["County_ID"],
                            County_Name = reader["County_Name"].ToString(),
                            IsAssigned = false // Default to false for new user
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching counties from database");
            }

            return counties;
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
