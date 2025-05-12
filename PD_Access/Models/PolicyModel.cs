using System.Collections.Generic;

namespace PD_Access.Models
{
    public class PolicyModel
    {

        public int Id { get; set; }
        public string SectionGroupName { get; set; }
        public string SectionNumber { get; set; }
        public string SectionTitle   { get; set; }


        public class PolicyViewModel
        {
            public List<PolicyModel> SectionGroupDropdownData { get; set; }
            public List<PolicyModel> SectionNumberDropdownData { get; set; }
            public List<PolicyModel> SectionTitleDropdownData { get; set; }
            public string SavedContent { get; set; }
        }


    }
}
