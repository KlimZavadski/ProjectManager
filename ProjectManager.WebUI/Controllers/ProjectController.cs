using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectManager.WebUI.Models;
using ProjectManager.WebUI.Models.ViewModels;
using System.Text.RegularExpressions;

namespace ProjectManager.WebUI.Controllers
{
    public class ProjectController : Controller
    {
        //String filterString = "%Coverage% < 90 && %Platform% == '.NET'";
        //BulkDownload.Import(@"c:\Bulk.csv", "Types");
        private ProjectDataManager manager;
        private const String SessionDisplayedField = "DisplayedField";
        private const String SessionReturnUrl = "ReturnUrl";


        public ProjectController()
        {
            manager = new ProjectDataManager();
        }
        
        public ActionResult List(String filter)
        {
            List<String> displayedField = GetDisplayedField();
            if (displayedField == null || displayedField.Count == 0)
            {
                Session[SessionDisplayedField] = GetDefaultField();
            }

            if (filter != null && filter != "") // TODO: and filter is valid
            {
                ProjectManager.WebUI.Models.Filter filters = new ProjectManager.WebUI.Models.Filter(filter);
                ProjectListViewModel projectList = manager.GetProjectList(filters, GetDisplayedField().ToArray());
                return View(projectList);
            }
            else
            {
                ProjectListViewModel projectList = manager.GetAllProjects(GetDisplayedField().ToArray());
                return View(projectList);
            }            
        }

        [HttpGet]
        public ActionResult Details(String returnUrl, Guid id)
        {
            Session[SessionReturnUrl] = returnUrl;
            ProjectViewModel projectViewMode = manager.GetProjectViewModel(id);            
            return View(projectViewMode);
        }

        [HttpGet]
        public ActionResult Edit(String returnUrl, Guid id)
        {
            Session[SessionReturnUrl] = returnUrl;
            if (id == null)
            {
                return RedirectToAction("BackToUrl", "Project");
            }
            else
            {
                ProjectViewModel projectViewMode = manager.GetProjectViewModel(id);
                return View(projectViewMode);
            }
        }

        [HttpPost]
        public ActionResult Edit(ProjectViewModel projectViewModel)
        {
            manager.SaveProject(projectViewModel);            
            return RedirectToAction("List");
        }
        
        [HttpGet]
        public ActionResult Add(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            ProjectViewModel projectViewMode = manager.AddNewProject();
            return View("Edit", projectViewMode);
        }

        public ActionResult Delete(String returnUrl, Guid id)
        {
            Session[SessionReturnUrl] = returnUrl;
            manager.DeleteProject(id);
            return RedirectToAction("BackToUrl");
        }

        [HttpGet]
        public ActionResult EditHeaders(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            DisplayedFieldViewModel displayedField =
                new DisplayedFieldViewModel(GetDisplayedField(), manager.GetColumnList());
            return View(displayedField);
        }

        [HttpPost]
        public ActionResult EditHeaders(DisplayedFieldViewModel displayedField)
        {
            if (displayedField.PropertiesList.Count != 0)
            {
                displayedField.NormilizeProperties(GetDefaultField());
                Session[SessionDisplayedField] = displayedField.PropertiesList;
            }
            return Redirect((String)Session[SessionReturnUrl]);
        }

        public ActionResult Export(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            String tableName = "Types";
            Bulk.Export("BulkExport.csv", tableName);
            return View("BackToUrl");
        }

        public ActionResult Import(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            String tableName = "Types";
            Bulk.Import("BulkImport.csv", tableName);
            return View("BackToUrl");
        }

        public ActionResult BackToUrl()
        {
            return Redirect((String)Session[SessionReturnUrl]);
        }

        private List<String> GetDisplayedField()
        {
            return (List<String>)Session[SessionDisplayedField];
        }

        private List<String> GetDefaultField()
        {
            int i = 1;
            List<String> defaultField = new List<String>();
            while (true)
            {
                String nameField = WebConfigWorker.GetAddSetting("field" + i.ToString());
                if (nameField != null)
                {
                    defaultField.Add(nameField);
                }
                else break;
                i++;
            }
            return defaultField;
        }

        [HttpPost]
        public PartialViewResult AddNewPropertyDialog(NewPropertyTransfer newPropertyTransfer)
        {
            manager.AddNewPropertyToDb(newPropertyTransfer);
            return PartialView();
        }
    }
}
