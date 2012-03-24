using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectManager.WebUI.Models
{
    public class NewPropertyTransfer
    {
        public String PropertyName { get; set; }
        public bool PropertyIsPublic { get; set; }
        public bool PropertyIsNumber { get; set; }
        public String PropertyValues { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class ValueTransfer
    {
        public String Value { get; set; }
        public bool IsAssigned { get; set; }
    }
}