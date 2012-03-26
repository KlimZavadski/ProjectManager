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
        public static List<SelectListItem> ConvertToSelectList(this HtmlHelper htmlHelper, List<String> columnList, String currentItem)
        {
            List<String> newColumnList = new List<String>(columnList);
            newColumnList.Remove(currentItem);
            newColumnList.Sort();
            List<SelectListItem> selectListItems = new List<SelectListItem>(newColumnList.Count);
            foreach (String column in newColumnList)
            {
                SelectListItem item = new SelectListItem();
                item.Text = item.Value = column;
                selectListItems.Add(item);
            }
            return selectListItems;
        }
    }
}