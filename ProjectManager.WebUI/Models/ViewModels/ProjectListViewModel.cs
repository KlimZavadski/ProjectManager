using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectManager.WebUI.Models.ViewModels
{
	public class ProjectListViewModel
	{
        public String[] Headers { get; set; }
		public List<ProjectViewModel> Projects { get; set; }

		public ProjectListViewModel(String[] headers, int projectCount)
		{
            List<String> headersList = new List<String>(headers.Count());
            headersList.Add("Title");
		    foreach (string header in headers)
		    {
                if (header != headersList[0])
                {
                    headersList.Add(header);
                }
		    }
		    Headers = headersList.ToArray();
			Projects = new List<ProjectViewModel>(projectCount);
		}
	}
}