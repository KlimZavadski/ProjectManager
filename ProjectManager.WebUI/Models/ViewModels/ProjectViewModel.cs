using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.WebUI.Models.ViewModels
{
	public class ProjectViewModel
	{
        public Guid ProjectId { get; set; }

        public DateTime CreateAt { get; set; }

        public Guid CreateBy { get; set; }

        public String CreateByName { get; set; }

		public List<PropertyViewModel> Properties { get; set; }

		public ProjectViewModel()
		{
			Properties = new List<PropertyViewModel>();
		}
	}

	public class PropertyViewModel
	{
        public Guid PropertyId { get; set; }
       
		public String PropertyName { get; set; }

        public List<PropertyValue> PropertyValues { get; set; }

		public PropertyViewModel()
		{            
			PropertyName = "";
			PropertyValues = new List<PropertyValue>();
		}
	}

    public class PropertyValue
    {
        public Guid RecordId { get; set; }

        public Guid PropertyPersonIdModified { get; set; }

        public DateTime PropertyDateTimeModified { get; set; }

        public String Value { get; set; }

        public PropertyValue()
        {
            Value = "";
        }
    }
}