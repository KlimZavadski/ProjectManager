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
                        return new PartialViewResult() { ViewName = "EditString" };
                    }
                    
            }
            return new PartialViewResult();
        }

        public PartialViewResult GetPropertyHistory(String Smth, String PropertyID, String ProjectID)
        {
            Guid PropID = Guid.Parse(PropertyID);
            Guid ProjID = Guid.Parse(ProjectID);
            return PartialView(manager.GetHistoryByProjectIDByPropertyID(ProjID,PropID));
        }

        public PartialViewResult GetResultNew(String PropertyID, String ProjectID)
        {
            String PropertyType = manager.GetPropertyTypePerID(PropertyID);
            Guid ProjID = Guid.Parse(ProjectID);
            Guid PropID = Guid.Parse(PropertyID);
            switch (PropertyType)
            {
                case "Bool":
                    {
                        bool res = bool.Parse((new Repository()).PropertiesOfProjects.Where(q => q.PropertyID == PropID && q.ProjectID == ProjID).Select(q => q.PropertyValue).Single());
                        return PartialView("EditBool", new MyBool { a=res, test="testing"});
                    }
                default:
                    {
                        var results = (new Repository()).PropertiesOfProjects.Where(q => q.PropertyID == PropID && q.ProjectID == ProjID).Select(q => q.PropertyValue).ToList<String>();
                        PartialViewResult a = new PartialViewResult();
                        return PartialView("Editfields", results);
                    }
            }
            return new PartialViewResult();
        }

        public PartialViewResult GetPropertyBool(String a1, String a2)
        {
            Guid g1=Guid.Parse(a1);
            Guid g2=Guid.Parse(a2);

            if ((new Repository()).
                PropertiesOfProjects.
                Where(q=>q.PropertyID==g1 && q.ProjectID==g2).
                First().Property.Type.TypeName=="Bool")
            {
                return PartialView("testgetproperty");
            }
            else
            {
                return new PartialViewResult();
            }
        }

        public PartialViewResult SaveChangesDb(String types, String Values, String PropertyID, String ProjectID)
        {
            Guid PropID=Guid.Parse(PropertyID);
            Guid ProjID=Guid.Parse(ProjectID);
            if (types=="Bool")
            {
                var rep = new Repository();
                var a = rep.PropertiesOfProjects.Where(q => q.PropertyID == PropID && q.ProjectID == ProjID).First();
                rep.Histories.AddObject(new History { 
                    RecordID=Guid.NewGuid(),
                    ProjectID=ProjID,
                    PropertyID=PropID,
                    PropertyValue=a.PropertyValue,
                    PropertyDateTimeModified=a.PropertyDateTimeModified,
                    PropertyPersonIDModified=a.PropertyPersonIDModified
                });
                a.PropertyValue = Values;
                a.PropertyDateTimeModified = DateTime.Now;
                rep.SaveChanges();
                return PartialView("DisplayBool", new MyBool { a=bool.Parse(Values), test=Values});
            }
            else
            {
                var rep = new Repository();
                var a = rep.PropertiesOfProjects.Where(q => q.PropertyID == PropID && q.ProjectID == ProjID);
                foreach (var item in a)
                {
                    rep.Histories.AddObject(new History
                    {
                        RecordID = Guid.NewGuid(),
                        ProjectID = ProjID,
                        PropertyID = PropID,
                        PropertyValue = item.PropertyValue,
                        PropertyDateTimeModified = item.PropertyDateTimeModified,
                        PropertyPersonIDModified = item.PropertyPersonIDModified
                    });
                    rep.PropertiesOfProjects.DeleteObject(item);
                }
                List<ValueTransfer> listValues = manager.ParseInputValues(Values);
                foreach (var value in listValues)
                {
                    /*
                    rep.DefaultValues.AddObject(new DefaultValue
                    {
                        PropertyID = PropID,
                        Value = value.Value,
                        ValueID = Guid.NewGuid()
                    });*/
                    /*
                    if (value.IsAssigned)
                    {*/
                        rep.PropertiesOfProjects.AddObject(new PropertiesOfProject
                        {
                            ProjectID = ProjID,
                            PropertyID = PropID,
                            RecordID = Guid.NewGuid(),
                            PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),              // TODO: Autorization
                            PropertyValue = value.Value,
                            PropertyDateTimeModified = DateTime.Now
                        });
                    /*}*/
                }
                rep.SaveChanges();
                return PartialView("DisplayFields", rep.PropertiesOfProjects.Where(q=>q.PropertyID==PropID && q.ProjectID==ProjID));
            }
        }
    }
}
