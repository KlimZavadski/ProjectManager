using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.WebUI.Models
{
    public class DisplayedFieldViewModel
    {
        public List<String> PropertiesList { get; set; }
        public List<SelectListItem> ColumnsList { get; set; } 


        public DisplayedFieldViewModel()
        {
            this.PropertiesList = new List<String>();
            this.ColumnsList = new List<SelectListItem>();
        }

        public DisplayedFieldViewModel(List<String> displayedField, List<String> columnsList)
        {
            this.PropertiesList = new List<String>(displayedField);
            this.ColumnsList = new List<SelectListItem>(columnsList.Count);
            foreach (String column in columnsList)
            {
                SelectListItem item = new SelectListItem();
                item.Text = item.Value = column;
                this.ColumnsList.Add(item);
            }
        }

        public void NormilizeProperties(List<String> previousProperties)
        {
            for (int i = 0; i < this.PropertiesList.Count; i++)
            {
                if (this.PropertiesList[i] == "")
                {
                    this.PropertiesList[i] = previousProperties[i];
                }
            }
            DeleteEmptyProperties();
        }

        private void DeleteEmptyProperties()
        {
            int i = 0;
            while (i < this.PropertiesList.Count)
            {
                if (this.PropertiesList[i] == "null")
                {
                    this.PropertiesList.RemoveAt(i);
                    continue;
                }
                i++;
            }
        }
    }
}