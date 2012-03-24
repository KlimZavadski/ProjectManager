using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ProjectManager.WebUI.Models
{
    public static class WebConfigWorker
    {
        public static String GetAddSetting(String nameOfSetting)
        {
            Configuration configuration =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ControlSystemProjects");
            KeyValueConfigurationElement setting = configuration.AppSettings.Settings[nameOfSetting];
            if (setting != null)
            {
                return setting.Value;
            }
            return null;
        }

        public static String GetConnectionString()
        {
            return @"data source=.\SQLEXPRESS;
			         attachdbfilename=|DataDirectory|\ProjectsDataBase.mdf;
			         integrated security=True;
			         user instance=True;
			         multipleactiveresultsets=True;";
        }
    }
}