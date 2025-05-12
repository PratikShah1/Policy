using System.Diagnostics.Metrics;

namespace PD_Access.Models
{
    public class UserView
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public List<Roles> Roles { get; set; }
        public List<County> Counties { get; set; }
    }


    public class Roles
        {
            public int Id { get; set; }
        public int role_id { get; set; }
            public bool IsAssigned { get; set; }
        }


    public class County
    {
        public int Id { get; set; }
        public int County_ID { get; set; }
        public string County_Name { get; set; }
        public bool IsAssigned { get; set; }

    }





}
