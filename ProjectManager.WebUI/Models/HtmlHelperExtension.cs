using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace ProjectManager.WebUI.Models
{
    public static class HtmlHelperExtension
    {
        public static DropDownList DropDownListExtension(this HtmlHelper htmlHelper, List<String> columnList, String currentItem)
        {            
            List<String> newColumnList = new List<String>(columnList);
            newColumnList.Remove(currentItem);
            DropDownList dropDownList = new DropDownList();
            dropDownList.DataSource = newColumnList;
            
            return dropDownList;
        }

        //public 
    }
}