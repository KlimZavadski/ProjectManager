using System;
using System.Collections.Generic;
using System.IO;
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
        private ProjectDataManager manager;
        private const String SessionDisplayedField = "DisplayedField";
        private const String SessionReturnUrl = "ReturnUrl";


        public ProjectController()
        {
            manager = new ProjectDataManager();
        }
        
        public ActionResult List(String filter)
        {
            ProjectListViewModel projectList;
            List<String> displayedField = GetDisplayedFields();
            if (displayedField == null || displayedField.Count == 0)
            {
                Session[SessionDisplayedField] = GetDefaultFields();
            }
            if (filter != null && filter != "")
            {
                if (ProjectManager.WebUI.Models.Filter.IsFilterStringValid(filter))
                {
                    ProjectManager.WebUI.Models.Filter filters = new ProjectManager.WebUI.Models.Filter(filter);
                    projectList = manager.GetProjectList(filters, GetDisplayedFields().ToArray());
                    return View(projectList);
                }
            }
            projectList = manager.GetAllProjects(GetDisplayedFields().ToArray());
            return View(projectList);           
        }

        [HttpGet]
        public ActionResult Details(String returnUrl, Guid id)
        {
            if (returnUrl != null)
            {
                Session[SessionReturnUrl] = returnUrl;
            }
            ProjectViewModel projectViewMode = manager.GetProjectViewModel(id);
            return View(projectViewMode);
        }
      
        [HttpGet]
        public ActionResult Add(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            ProjectViewModel projectViewMode = manager.AddNewProject();
            return View(projectViewMode);
        }

        [HttpPost]
        public ActionResult Add(ProjectViewModel projectViewModel)
        {            
            manager.SaveProject(projectViewModel);
            return RedirectToAction("Details", new { returnUrl = "", id = projectViewModel.ProjectId });
        }

        public ActionResult Delete(Guid id)
        {
            manager.DeleteProject(id);
            return RedirectToAction("BackToUrl");
        }

        [HttpGet]
        public ActionResult EditHeaders(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            DisplayedFieldViewModel displayedFieldViewModel =
                new DisplayedFieldViewModel(GetDisplayedFields(), manager.GetColumnList());
            return View(displayedFieldViewModel);
        }

        [HttpPost]
        public ActionResult EditHeaders(DisplayedFieldViewModel displayedField)
        {
            if (displayedField.PropertiesList.Count != 0)
            {
                displayedField.NormilizeProperties(GetDefaultFields());
                Session[SessionDisplayedField] = displayedField.PropertiesList;
            }
            return Redirect((String)Session[SessionReturnUrl]);
        }

        [HttpGet]
        public ActionResult Import(String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            ExportViewModel exportViewModel = new ExportViewModel(null, GetTableNames());
            return View(exportViewModel);
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase fileUpload, String tableName)
        {
            if (fileUpload != null)
            {
                String fullPathFileName = manager.Import(fileUpload, tableName);
                System.IO.File.Delete(fullPathFileName);
            }
            return RedirectToAction("BackToUrl");
        }

        [HttpGet]
        public ActionResult Export(String filter, String returnUrl)
        {
            Session[SessionReturnUrl] = returnUrl;
            ExportViewModel exportViewModel = new ExportViewModel(null, GetTableNames());
            return View(exportViewModel);
        }

        [HttpPost]
        public ActionResult Export(ExportViewModel exportViewModel)
        {
            if (exportViewModel.TableName != null)
            {
                String fileName = "ExportTable_" + exportViewModel.TableName + ".csv";
                Response.Clear();
                Response.ContentType = "application/csv";
                Response.AddHeader("Content-Type", "application/csv");
                Response.AddHeader("Content-Disposition", "inline; filename = " + fileName);
                String fullPathFileName = manager.Export(fileName, exportViewModel.TableName);
                Response.WriteFile(fullPathFileName);
                Response.End();
                System.IO.File.Delete(fullPathFileName);
            }
            return RedirectToAction("BackToUrl");
        }

        public ActionResult BackToUrl()
        {
            if (Session[SessionReturnUrl] == null || (String)Session[SessionReturnUrl] == "")
            {
                RedirectToAction("List", "Project");
            }
            return Redirect((String)Session[SessionReturnUrl]);
        }

        private List<String> GetDisplayedFields()
        {
            return (List<String>)Session[SessionDisplayedField];
        }

        private List<String> GetDefaultFields()
        {
            int i = 1;
            List<String> defaultField = new List<String>();
            while (true)
            {
                String nameField = WebConfigWorker.GetAppSetting("field" + i.ToString());
                if (nameField != null)
                {
                    defaultField.Add(nameField);
                }
                else break;
                i++;
            }
            return defaultField;
        }

        private List<String> GetTableNames()
        {
            return new List<String> { "DefaultValues", "History", "Persons",
                "Projects", "Properties", "PropertiesOfProjects", "Types" };
        }

        [HttpPost]
        public PartialViewResult AddNewPropertyDialog(NewPropertyTransfer newPropertyTransfer)
        {
            Guid newPropertyId = manager.AddNewPropertyToDb(newPropertyTransfer);
            var a = manager.GetProjectViewModel(newPropertyTransfer.ProjectId);
            var b = a.Properties.Where(x => x.PropertyId == newPropertyId);
            return PartialView(b);
        }
    }
}
