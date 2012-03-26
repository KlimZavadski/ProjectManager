using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectManager.WebUI.Models;

namespace ProjectManager.WebUI.Controllers
{
    public class EditPropertyController : Controller
    {
        private ProjectDataManager manager;
        public EditPropertyController()
        {
            manager = new ProjectDataManager();
        }

        [HttpPost]
        public PartialViewResult GetPropertyType(String ID)
        {
            String PropertyType = manager.GetPropertyTypePerID(ID);
            ViewBag.PropertyType = PropertyType;
            return PartialView();
        }


        /// <summary>
        /// когда обращ первый раз
        /// </summary>
        [HttpPost]
        public PartialViewResult GetViewForProperty(String PropertyID, String ProjectID)
        {
            String PropertyType = manager.GetPropertyTypePerID(PropertyID);
            Guid ProjID = Guid.Parse(ProjectID);
            Guid PropID = Guid.Parse(PropertyID);
            return GetView(PropertyType,String.Empty,ProjID,PropID);
        }

        /// <summary>
        /// когда уже заполнили и нажимаем Save
        /// </summary>
        [HttpPost]
        public PartialViewResult GetViewNew(String Values, String PropertyID, String ProjectID)
        {
            Guid ProjID = Guid.Parse(ProjectID);
            Guid PropID = Guid.Parse(PropertyID);
            String PropertyType = manager.GetPropertyTypePerID(PropertyID);
            return GetView(PropertyType, Values, ProjID, PropID);
        }

        public PartialViewResult GetView(String PropertyType, String Values, Guid ProjectID, Guid PropertyID)
        {
            PartialViewResult MyView = null;
            switch (PropertyType)
            {
                case "Bool":
                    {

                        return new PartialViewResult();
                    }
                case "Date":
                    {
                        return new PartialViewResult();
                    }
                case "Number":
                    {
                        return new PartialViewResult();
                    }
                case "NumberArray":
                    {
                        return new PartialViewResult();
                    }
                case "String":
                    {
                        if (Values == String.Empty)
                        {
                            String PropertyValue = manager.GetPropertyValuePerPropertyID(PropertyID, ProjectID);
                            MyView = PartialView("EditString", new MyString(PropertyValue));
                            MyView.ViewBag.OK = "true";
                            return MyView;
                        }
                        else
                        {
                            MyString myString=new MyString(Values);
                            MyView = PartialView("EditString", myString);
                            if (myString.IsCorrect)
                            {
                                MyView.ViewBag.OK = "true";
                                manager.MovePropertyToHistory(ProjectID, PropertyID, Values);
                            }
                            else
                            {
                                MyView.ViewBag.OK = "false";
                            }
                            return MyView;
                        }
                        //return new PartialViewResult() { ViewName="EditString"};
                    }
                case "StringArray":
                    {
                        return new PartialViewResult();
                    }
                default:
                    {
                        return new PartialViewResult() { ViewName="EditBool"};
                    }
            }
        }

        public PartialViewResult GetPropertyHistory(String Smth, String PropertyID, String ProjectID)
        {
            Guid PropID = Guid.Parse(PropertyID);
            Guid ProjID = Guid.Parse(ProjectID);
            return PartialView(manager.GetHistoryByProjectIDByPropertyID(ProjID,PropID));
        }
    }
}
