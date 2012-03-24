using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data.Objects;
using ProjectManager.WebUI.Models.ViewModels;
using System.Text.RegularExpressions;



namespace ProjectManager.WebUI.Models
{
    public class ProjectDataManager
    {
        private Repository repository;
        private const String Start = "SELECT pop.ProjectID FROM dbo.PropertiesOfProjects as pop, dbo.Properties as p WHERE";

        public ProjectDataManager()
        {
            repository = new Repository();
        }

        public ProjectListViewModel GetAllProjects(String[] displayedField)
        {
            Guid[] guid = repository.Projects
                .Select(x => x.ProjectID)
                .ToArray<Guid>();
            return GetProjectListById(displayedField, guid);
        }

        public ProjectListViewModel GetProjectList(Filter filter, String[] headers)
        {
            Guid[] projectIDs = SQLQuery(filter);
            return GetProjectListById(headers, projectIDs);
        }

        public List<String> GetColumnList()
        {
            return repository.Properties
                .Select(x => x.PropertyName)
                .ToList<String>();
        }


        private Guid[] SQLQuery(Filter filter)
        {
            object[] parameters = GetObjectParametrs(filter);
            String queryString = GetQueryString(filter, 0);
            return repository.ExecuteStoreQuery<Guid>(queryString, parameters).ToArray<Guid>();
        }

        private String GetQueryString(Filter filter, int i)
        {
            if (i / 2 < filter.FilterArray.Count<FilterObject>())
            {
                return " (" + Start + " (p.PropertyName = {" + i.ToString() + "} AND pop.PropertyValue " +
                    filter.FilterArray[i / 2].Sign + " {" + (i + 1).ToString() +
                    "} AND p.PropertyID = pop.PropertyID)) " + filter.FilterArray[i / 2].Operator +
                    GetQueryString(filter, i + 2);
            }
            else
            {
                return "";
            }
        }

        private object[] GetObjectParametrs(Filter filter)
        {
            List<object> objectArray = new List<object>(filter.FilterArray.Count<FilterObject>() * 2);
            foreach (FilterObject obj in filter.FilterArray)
            {
                objectArray.Add(obj.Name);
                objectArray.Add(obj.Value);
            }
            return objectArray.ToArray<object>();
        }


        public ProjectListViewModel GetProjectListById(String[] displayedField, Guid[] projectIDs)
        {
            ProjectListViewModel projectList = new ProjectListViewModel(displayedField, projectIDs.Count());
            foreach (Guid id in projectIDs)
            {
                projectList.Projects.Add(GetProjectViewModel(id));
            }
            return projectList;
        }

        public ProjectViewModel GetProjectViewModel(Guid id)
        {
            ProjectViewModel projectViewModel = new ProjectViewModel();
            var result = repository.PropertiesOfProjects
                .Where(x => x.ProjectID == id)
                .GroupBy(x => x.PropertyID);
            projectViewModel.ProjectId = id;
            if (result == null)
            {
                return null;
            }
            else
            {
                Project project = repository.Projects.Where(x => x.ProjectID == id).SingleOrDefault();
                projectViewModel.ProjectId = project.ProjectID;
                projectViewModel.CreateBy = project.CreateBy;
                projectViewModel.CreateByName = repository.Persons
                    .Where(x => x.PersonID == project.CreateBy).SingleOrDefault().PersonName;
                projectViewModel.CreateAt = project.CreateAt;
                foreach (var properties in result)
                {
                    PropertyViewModel propertyViewModel = 
                        GetPropertyViewModel(properties.ToArray<PropertiesOfProject>());
                    projectViewModel.Properties.Add(propertyViewModel);
                }
                return projectViewModel;
            }
        }

        public PropertyViewModel GetPropertyViewModel(PropertiesOfProject[] properties)
        {
            if (properties.Count<PropertiesOfProject>() == 0)
            {
                return null;
            }
            else
            {
                PropertyViewModel propertyViewModel = new PropertyViewModel();
                propertyViewModel.PropertyId = properties[0].PropertyID;
                propertyViewModel.PropertyName = properties[0].Property.PropertyName;
                propertyViewModel.PropertyValues = GetPropertyValues(properties);
                return propertyViewModel;
            }
        }

        public List<PropertyValue> GetPropertyValues(PropertiesOfProject[] properties)
        {
            if (properties.Count<PropertiesOfProject>() == 0)
            {
                return null;
            }
            else
            {
                List<PropertyValue> propertyValueList =
                    new List<PropertyValue>(properties.Count<PropertiesOfProject>());
                foreach (PropertiesOfProject property in properties)
                {
                    PropertyValue propertyValue = new PropertyValue();
                    propertyValue.RecordId = property.RecordID;
                    propertyValue.PropertyPersonIdModified = property.PropertyPersonIDModified;
                    propertyValue.PropertyDateTimeModified = property.PropertyDateTimeModified;
                    propertyValue.Value = property.PropertyValue;
                    propertyValueList.Add(propertyValue);
                }
                return propertyValueList;
            }
        }


        public void SaveProject(ProjectViewModel projectViewModel)
        {
            if (projectViewModel.ProjectId == null)
            {
                CreateProject(projectViewModel);
            }
            else
            {
                EditProject(projectViewModel);
            }
            repository.SaveChanges();
        }

        private void CreateProject(ProjectViewModel projectViewModel)
        {
            repository.Projects.AddObject(new Project
                {
                    ProjectID = Guid.NewGuid(),
                    CreateBy = projectViewModel.CreateBy,
                    CreateAt = projectViewModel.CreateAt
                });
            repository.SaveChanges();
            EditProject(projectViewModel);
        }

        private void EditProject(ProjectViewModel projectViewModel)
        {
            foreach (PropertyViewModel propertyViewModel in projectViewModel.Properties)
            {
                foreach (PropertyValue propertyValue in propertyViewModel.PropertyValues)
                {
                    if (propertyValue.RecordId == null)         // New property was added
                    {
                        AddPropertyToPropertiesOfProjectTable(projectViewModel.ProjectId,
                                propertyViewModel.PropertyId, propertyValue);
                    }
                    else       // Add old property to History Table and Add new property in PropertiesOfProject Table
                    {
                        PropertiesOfProject oldProperty = repository.PropertiesOfProjects
                            .Where(x => x.ProjectID == projectViewModel.ProjectId &&
                                x.PropertyID == propertyViewModel.PropertyId &&
                                x.RecordID == propertyValue.RecordId)
                            .SingleOrDefault();
                        if (oldProperty != null)
                        {
                            AddPropertyToHistoryTable(oldProperty);
                            repository.PropertiesOfProjects.DeleteObject(oldProperty);
                            AddPropertyToPropertiesOfProjectTable(projectViewModel.ProjectId,
                                propertyViewModel.PropertyId, propertyValue);
                        }
                    }
                }
            }
        }

        private void AddPropertyToPropertiesOfProjectTable(Guid projectId, Guid propertyId, PropertyValue property)
        {
            repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
                {
                    RecordID = Guid.NewGuid(),
                    ProjectID = projectId,
                    PropertyID = propertyId,
                    PropertyValue = property.Value,
                    PropertyPersonIDModified = property.PropertyPersonIdModified,
                    PropertyDateTimeModified = property.PropertyDateTimeModified
                });

        }

        private void AddPropertyToHistoryTable(PropertiesOfProject property)
        {
            repository.Histories.AddObject(new History
            {
                RecordID = Guid.NewGuid(),
                ProjectID = property.ProjectID,
                PropertyID = property.PropertyID,
                PropertyValue = property.PropertyValue,
                PropertyPersonIDModified = property.PropertyPersonIDModified,
                PropertyDateTimeModified = property.PropertyDateTimeModified
            });

        }

        public ProjectViewModel AddNewProject()
        {
            ProjectViewModel projectViewModel = new ProjectViewModel();
            projectViewModel.CreateBy = Guid.NewGuid();       // TODO: authorise
            projectViewModel.CreateAt = DateTime.Now;
            return projectViewModel;
        }

        public void DeleteProject(Guid id)
        {
            DeletePropertiesOfProject(id);
            DeleteHistoryOfProject(id);
            Project project = repository.Projects
                .Where(x => x.ProjectID == id)
                .SingleOrDefault();
            if (project != null)
            {
                repository.Projects.DeleteObject(project);
            }
            repository.SaveChanges();
        }

        private void DeletePropertiesOfProject(Guid id)
        {
            PropertiesOfProject[] propertiesOfProject = repository.PropertiesOfProjects
                .Where(x => x.ProjectID == id)
                .ToArray();
            foreach (PropertiesOfProject property in propertiesOfProject)
            {
                repository.PropertiesOfProjects.DeleteObject(property);
            }
        }

        private void DeleteHistoryOfProject(Guid id)
        {
            History[] histories = repository.Histories
                .Where(x => x.ProjectID == id)
                .ToArray();
            foreach (History history in histories)
            {
                repository.Histories.DeleteObject(history);
            }
        }
        
       
        public void AddNewPropertyToDb(NewPropertyTransfer propertyToAdd)
        {
            if (repository.Properties.Where(q => q.PropertyName == propertyToAdd.PropertyName).FirstOrDefault() == null)
            {
                List<ValueTransfer> listValues = ParseInputValues(propertyToAdd.PropertyValues);
                Guid PropertyType = GetNewPropertyTypeID(listValues.Count, propertyToAdd.PropertyIsNumber);               

                Guid PropertyGuid = Guid.NewGuid();

                repository.Properties.AddObject(new Property
                {
                    PropertyID = PropertyGuid,
                    PropertyName = propertyToAdd.PropertyName,
                    PropertyIsPublic = propertyToAdd.PropertyIsPublic,
                    PropertyIsDefault = false,
                    PropertyTypeID = PropertyType
                });

                foreach (var value in listValues)
                {
                    repository.DefaultValues.AddObject(new DefaultValue
                    {
                        PropertyID = PropertyGuid,
                        Value = value.Value,
                        ValueID = Guid.NewGuid()
                    });                    

                    if (value.IsAssigned)
                    {
                        repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
                        {
                            ProjectID = propertyToAdd.ProjectId,
                            PropertyID = PropertyGuid,
                            RecordID = Guid.NewGuid(),
                            PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),
                            PropertyValue = value.Value,
                            PropertyDateTimeModified = DateTime.Now
                        });
                    }
                }
                repository.SaveChanges();
            }
        }

        private Guid GetNewPropertyTypeID(int countValues, bool isNumber)
        {
            if (isNumber)
            {
                return repository.Types.Where(q => q.TypeName == "Number").Select(q => q.TypeID).Single();
            }
            else
            {
                if (countValues == 1)
                {
                    return repository.Types.Where(q => q.TypeName == "String").Select(q => q.TypeID).Single();
                }
                else
                {
                    return repository.Types.Where(q => q.TypeName == "Array").Select(q => q.TypeID).Single();
                }
            }
        }

        private List<ValueTransfer> ParseInputValues(String inputString)
        {
            List<ValueTransfer> listValues = new List<ValueTransfer>();
            ValueTransfer item = null;
            bool change = true;
            Regex myReg = new Regex(@"[A-z 0-9]+");
            foreach (Match m in myReg.Matches(inputString))
            {
                if (change)
                {
                    item = new ValueTransfer();
                    item.Value = m.Value;
                    change = false;
                }
                else
                {
                    item.IsAssigned = bool.Parse(m.Value);
                    listValues.Add(item);
                    change = true;
                }
            }
            return listValues;
        }        
    }
}