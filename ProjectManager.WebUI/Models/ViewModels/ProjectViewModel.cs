using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.WebUI.Models.ViewModels
{
	public class ProjectViewModel
	{
        [HiddenInput(DisplayValue = false)]
        public Guid ProjectId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime CreateAt { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid CreateBy { get; set; }

		public List<PropertyViewModel> Properties { get; set; }

		public ProjectViewModel()
		{
			Properties = new List<PropertyViewModel>();
		}
	}

	public class PropertyViewModel
	{      

        [HiddenInput(DisplayValue = false)]
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
        [HiddenInput(DisplayValue = false)]
        public Guid RecordId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid PropertyPersonIdModified { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime PropertyDateTimeModified { get; set; }

        public String Value { get; set; }

        public PropertyValue()
        {
            Value = "";
        }
    }
}