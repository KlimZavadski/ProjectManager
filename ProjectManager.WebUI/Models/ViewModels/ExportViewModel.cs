using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.WebUI.Models.ViewModels
{
    public class ExportViewModel
    {
        public String TableName { get; set; }

        public List<SelectListItem> TableNamesList { get; set; }


        public ExportViewModel()
        {
            this.TableName = "";
        }

        public ExportViewModel(String tableName, List<String> listTableNames)
        {
            this.TableName = tableName;
            this.TableNamesList = new List<SelectListItem>(listTableNames.Count);
            foreach (String name in listTableNames)
            {
                SelectListItem item = new SelectListItem();
                item.Text = item.Value = name;
                this.TableNamesList.Add(item);
            }
        }
    }
}