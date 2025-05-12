using System.Collections.Generic;

namespace PD_Access.Models
{
    public class ModifyPolicyModel
    {
        public int Id { get; set; }
        public string SectionGroupName { get; set; }
        public string SectionNumber { get; set; }
        public string SectionTitle { get; set; }
        public int section_number { get; set; }
        public string section_name { get; set; }


        public class PolicyViewModel
        {
            public List<ModifyPolicyModel> SectionGroupDropdownData { get; set; }
            public List<ModifyPolicyModel> SectionNumberDropdownData { get; set; }
            public List<ModifyPolicyModel> SectionTitleDropdownData { get; set; }
            public string SavedContent { get; set; }
        }


        public int PolicySectionNumber { get; set; }
        public string PolicySectionTitle { get; set; }
        public string PolicySectionName { get; set; }
        public int PolicySectionNumberName { get; set; }
        public string PolicySectionNameNumber { get; set; }
        public string PolicyText { get; set; }

        public string modify_what { get; set; }
        public string modify_why { get; set; }
        public string modify_user_id { get; set; }
        public int modify_value {get;set;}
        public string modify_action { get;set; }



    }
}
