using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Objects;
using LINQtoCSV;
using ProjectManager.WebUI.Models.ViewModels;
using System.Text.RegularExpressions;


namespace ProjectManager.WebUI.Models
{
    public class ProjectDataManager
    {
        private Repository repository;
        private const String Start = "SELECT pop.ProjectID FROM dbo.PropertiesOfProjects as pop, dbo.Properties as p WHERE";
        private String downloadPath;
        private String uploadPath;
        private const int CountNumbersInValues = 10;

        public ProjectDataManager()
        {
            repository = new Repository();
            downloadPath = AppDomain.CurrentDomain.BaseDirectory + "Content\\DownloadFiles\\";
            uploadPath = AppDomain.CurrentDomain.BaseDirectory + "Content\\UploadFiles\\";
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
            if (i / 2 < filter.FilterList.Count)
            {
                return " (" + Start + " (p.PropertyName = {" + i.ToString() + "} AND pop.PropertyValue " +
                    filter.FilterList[i / 2].Sign + " {" + (i + 1).ToString() +
                    "} AND p.PropertyID = pop.PropertyID)) " + filter.FilterList[i / 2].Operator +
                    GetQueryString(filter, i + 2);
            }
            else
            {
                return "";
            }
        }

        private object[] GetObjectParametrs(Filter filter)
        {
            List<object> objectArray = new List<object>(filter.FilterList.Count * 2);
            foreach (FilterObject obj in filter.FilterList)
            {
                objectArray.Add(obj.Name);
                objectArray.Add(AddValue(obj.Value));
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
                projectViewModel.CreateByName = GetUserName(project.CreateBy);
                projectViewModel.CreateAt = project.CreateAt;
                foreach (var properties in result)
                {
                    PropertyViewModel propertyViewModel = 
                        GetPropertyViewModel(properties.ToArray<PropertiesOfProject>());
                    projectViewModel.Properties.Add(propertyViewModel);
                }
                projectViewModel.Properties.Sort((x, y) => String.Compare(x.PropertyName, y.PropertyName));
                MoveTitlePropertyFirst(projectViewModel);
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
                    propertyValue.Value = TryParse(property.PropertyValue);
                    propertyValueList.Add(propertyValue);
                }
                return propertyValueList;
            }
        }

        private void MoveTitlePropertyFirst(ProjectViewModel projectViewModel)
        {
            PropertyViewModel titlePropertyViewModel = projectViewModel.Properties
                .Where(x => x.PropertyName == "Title")
                .SingleOrDefault();
            if (titlePropertyViewModel != null)
            {
                projectViewModel.Properties.Remove(titlePropertyViewModel);
                projectViewModel.Properties.Insert(0, titlePropertyViewModel);
            }
        }

        private String TryParse(String value)
        {
            int int_number;
            if (Int32.TryParse(value, out int_number))
            {
                return int_number.ToString();
            }
            return value;
        }

        private String AddValue(String value)
        {
            int int_number;
            if (Int32.TryParse(value, out int_number))
            {
                String newValue = int_number.ToString();
                return GetNulls(CountNumbersInValues - newValue.Length) + newValue;
            }
            return value;
        }

        private String GetNulls(int count)
        {
            String nulls = "";
            while (count > 0)
            {
                nulls += "0";
                count--;
            }
            return nulls;
        }


        public void SaveProject(ProjectViewModel projectViewModel)
        {
            var result = repository.Projects.Where(x => x.ProjectID == projectViewModel.ProjectId).SingleOrDefault();
            if (result == null)
            {
                CreateProject(projectViewModel);
            }
            EditProject(projectViewModel);
            repository.SaveChanges();
        }

        private void CreateProject(ProjectViewModel projectViewModel)
        {
            repository.Projects.AddObject(new Project
                {
                    ProjectID = projectViewModel.ProjectId,
                    CreateBy = projectViewModel.CreateBy,
                    CreateAt = projectViewModel.CreateAt
                });
        }

        private void EditProject(ProjectViewModel projectViewModel)
        {
            foreach (PropertyViewModel propertyViewModel in projectViewModel.Properties)
            {
                foreach (PropertyValue propertyValue in propertyViewModel.PropertyValues)
                {
                    if (propertyValue.RecordId == Guid.Empty)         // New property was added
                    {
                        propertyValue.RecordId = Guid.NewGuid();
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
                    RecordID = property.RecordId,
                    ProjectID = projectId,
                    PropertyID = propertyId,
                    PropertyValue = AddValue(property.Value),
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
                PropertyValue = AddValue(property.PropertyValue),
                PropertyPersonIDModified = property.PropertyPersonIDModified,
                PropertyDateTimeModified = property.PropertyDateTimeModified
            });

        }


        public ProjectViewModel AddNewProject()
        {
            ProjectViewModel projectViewModel = new ProjectViewModel();
            projectViewModel.ProjectId = Guid.NewGuid();
            projectViewModel.CreateBy = GetCurrentUserId();
            projectViewModel.CreateAt = DateTime.Now;
            projectViewModel.CreateByName = GetCurrentUserName();
            projectViewModel.Properties.Add(SetTitleProperty());
            return projectViewModel;
        }

        private PropertyViewModel SetTitleProperty()
        {
            PropertyViewModel propertyViewModel = new PropertyViewModel();
            propertyViewModel.PropertyId = repository.Properties
                .Where(x => x.PropertyName == "Title")
                .Select(x => x.PropertyID)
                .SingleOrDefault();
            propertyViewModel.PropertyName = "Title";
            propertyViewModel.PropertyValues.Add(new PropertyValue
                                                     {
                                                         PropertyDateTimeModified = DateTime.Now,
                                                         PropertyPersonIdModified = GetCurrentUserId(),
                                                     });
            return propertyViewModel;
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


        public String Import(HttpPostedFileBase fileUpload, String tableName)
        {
            String fullPathFileName = downloadPath + fileUpload.FileName;
            fileUpload.SaveAs(fullPathFileName);
            String commandString = "BULK INSERT dbo." + tableName + " FROM '" + fullPathFileName +
                "' WITH (FIELDTERMINATOR = ';', FIRSTROW = 1)";
            repository.ExecuteStoreCommand(commandString, new ObjectParameter[] { });
            return fullPathFileName;
        }

        public String Export(String fileName, String tableName)
        {
            String fullPathFileName = uploadPath + fileName;
            CsvContext csvContext = new CsvContext();
            CsvFileDescription csvFileDescription = new CsvFileDescription
            {
                QuoteAllFields = false,
                SeparatorChar = ';',
                FirstLineHasColumnNames = true,
                FileCultureName = "en-US"
            };
            switch (tableName)
            {
                case "DefaultValues":
                    var result_1 = repository.DefaultValues
                        .Select(x => new
                        {
                            Value = x.Value,
                            PropertyID = x.PropertyID,
                            ValueID = ""
                        });
                    csvContext.Write(result_1, fullPathFileName, csvFileDescription);
                    break;
                case "History":
                    var result_2 = repository.Histories
                        .Select(x => new
                        {
                            PropertyDateTimeModified = x.PropertyDateTimeModified,
                            PropertyPersonIDModified = x.PropertyPersonIDModified,
                            PropertyValue = x.PropertyValue,
                            PropertyID = x.PropertyID,
                            ProjectID = x.ProjectID,
                            RecordID = ""
                        });
                    csvContext.Write(result_2, fullPathFileName, csvFileDescription);
                    break;
                case "Persons":
                    var result_3 = repository.Persons
                        .Select(x => new
                        {
                            PersonName = x.PersonName,
                            PersonID = ""
                        });
                    csvContext.Write(result_3, fullPathFileName, csvFileDescription);
                    break;
                case "Projects":
                    var result_4 = repository.Projects
                        .Select(x => new
                        {
                            CreateAt = x.CreateAt,
                            CreateBy = x.CreateBy,
                            ProjectID = ""
                        });
                    csvContext.Write(result_4, fullPathFileName, csvFileDescription);
                    break;
                case "Properties":
                    var result_5 = repository.Properties
                        .Select(x => new
                        {
                            PropertyIsDefault = x.PropertyIsDefault,
                            PropertyIsPublic = x.PropertyIsPublic,
                            PropertyName = x.PropertyName,
                            PropertyTypeID = x.PropertyTypeID,
                            PropertyID = ""
                        });
                    csvContext.Write(result_5, fullPathFileName, csvFileDescription);
                    break;
                case "PropertiesOfProjects":
                    var result_6 = repository.PropertiesOfProjects
                        .Select(x => new
                        {
                            PropertyDateTimeModified = x.PropertyDateTimeModified,
                            PropertyPersonIDModified = x.PropertyPersonIDModified,
                            PropertyValue = x.PropertyValue,
                            PropertyID = x.PropertyID,
                            ProjectID = x.ProjectID,
                            RecordID = ""
                        });
                    csvContext.Write(result_6, fullPathFileName, csvFileDescription);
                    break;
                case "Types":
                    var result_7 = repository.Types
                        .Select(x => new
                        {
                            TypeName = x.TypeName,
                            TypeID = ""
                        });
                    csvContext.Write(result_7, fullPathFileName, csvFileDescription);
                    break;
            }
            DeleteEmptyField(fullPathFileName);
            return fullPathFileName;
        }

        private void DeleteEmptyField(String fullPathFileName)
        {
            StreamReader streamReader = new StreamReader(fullPathFileName);
            String input = streamReader.ReadToEnd();
            streamReader.Close();
            StreamWriter streamWriter = new StreamWriter(fullPathFileName);
            streamWriter.Write(input.Replace("\"\"", ""));
            streamWriter.Close();
        }


        private Guid GetCurrentUserId()
        {
            String currentUserName = GetCurrentUserName();
            Guid personId = GetUserId(currentUserName);
            if (personId == Guid.Empty)
            {
                return AddUser(currentUserName);
            }
            return personId;
        }

        private String GetCurrentUserName()
        {
            return Environment.UserDomainName + "/" + Environment.UserName;
        }

        private Guid GetUserId(String userName)
        {
            return repository.Persons
                .Where(x => x.PersonName == userName)
                .Select(x => x.PersonID)
                .SingleOrDefault();
        }

        private String GetUserName(Guid id)
        {
            return repository.Persons
                .Where(x => x.PersonID == id)
                .Select(x => x.PersonName)
                .SingleOrDefault();
        }

        private Guid AddUser(String userName)
        {
            Person person = new Person();
            person.PersonID = Guid.NewGuid();
            person.PersonName = userName;
            repository.Persons.AddObject(person);
            repository.SaveChanges();
            return person.PersonID;
        }


        


        public Guid AddNewPropertyToDb(NewPropertyTransfer propertyToAdd)
        {
            var result = repository.Properties
                .Where(q => q.PropertyName == propertyToAdd.PropertyName)
                .FirstOrDefault();
            if (result == null)
            {
                List<ValueTransfer> listValues = null;
                Guid propertyType = Guid.Empty;
                propertyType = GetNewPropertyTypeID(propertyToAdd.PropertyType);/////new
                Guid propertyGuid = Guid.NewGuid();
                repository.Properties.AddObject(new Property
                {
                    PropertyID = propertyGuid,
                    PropertyName = propertyToAdd.PropertyName,
                    PropertyIsPublic = propertyToAdd.PropertyIsPublic,
                    PropertyIsDefault = false,
                    PropertyTypeID = propertyType
                });

                switch (propertyToAdd.PropertyType)
                {
                    case "Bool":
                        {
                            repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
                            {
                                ProjectID = propertyToAdd.ProjectId,
                                PropertyID = propertyGuid,
                                RecordID = Guid.NewGuid(),
                                PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),              // TODO: Autorization
                                PropertyValue = propertyToAdd.PropertyValues,
                                PropertyDateTimeModified = DateTime.Now
                            });
                            break;
                        }
                    case "DateTime":
                    case "Number":
                        {
                            listValues = ParseInputValues(propertyToAdd.PropertyValues);
                            foreach (var value in listValues)
                            {
                                repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
                                {
                                    ProjectID = propertyToAdd.ProjectId,
                                    PropertyID = propertyGuid,
                                    RecordID = Guid.NewGuid(),
                                    PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),              // TODO: Autorization
                                    PropertyValue = value.Value,
                                    PropertyDateTimeModified = DateTime.Now
                                });
                            }
                            break;
                        }
                    case "String":
                        {
                            listValues = ParseInputValues(propertyToAdd.PropertyValues);
                            foreach (var value in listValues)
                            {
                                repository.DefaultValues.AddObject(new DefaultValue
                                {
                                    PropertyID = propertyGuid,
                                    Value = value.Value,
                                    ValueID = Guid.NewGuid()
                                });
                                if (value.IsAssigned)
                                {
                                    repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
                                    {
                                        ProjectID = propertyToAdd.ProjectId,
                                        PropertyID = propertyGuid,
                                        RecordID = Guid.NewGuid(),
                                        PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),              // TODO: Autorization
                                        PropertyValue = value.Value,
                                        PropertyDateTimeModified = DateTime.Now
                                    });
                                }
                            }
                            break;
                        }
                }
                repository.SaveChanges();
                return propertyGuid;
            }
            return Guid.Empty;
        }

        private Guid GetNewPropertyTypeID(String newPropertyType)
        {
            return repository.Types.Where(q => q.TypeName == newPropertyType).Select(q => q.TypeID).Single();            
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

        public List<ValueTransfer> ParseInputValues(String inputString)
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

        public String GetPropertyTypePerID(String id)
        {
            var keyId = Guid.Parse(id);
            return repository.Properties
                .Where(q => q.PropertyID == keyId)
                .Single()
                .Type
                .TypeName;
        }

        public String GetPropertyValuePerPropertyID(Guid propId, Guid projId)
        {
            return repository.PropertiesOfProjects
                .Where(q => q.ProjectID == projId && q.PropertyID == propId)
                .First()
                .PropertyValue;
        }

        public void MovePropertyToHistory(Guid projId, Guid propId, String Value)
        {
            var oldProperty = repository.PropertiesOfProjects
                .Where(q => q.ProjectID == projId && q.PropertyID == propId)
                .Single();
            repository.Histories.AddObject(new History
            {
                RecordID = Guid.NewGuid(),
                ProjectID = oldProperty.ProjectID,
                PropertyID = oldProperty.PropertyID,
                PropertyValue = oldProperty.PropertyValue,
                PropertyPersonIDModified = oldProperty.PropertyPersonIDModified,
                PropertyDateTimeModified = oldProperty.PropertyDateTimeModified
            });
            repository.PropertiesOfProjects.DeleteObject(oldProperty);
            repository.PropertiesOfProjects.AddObject(new PropertiesOfProject
            {
                RecordID = Guid.NewGuid(),
                ProjectID = projId,
                PropertyID = propId,
                PropertyValue = Value,
                PropertyPersonIDModified = Guid.Parse("e9e71093-7327-427a-86cb-50f255bc8301"),
                PropertyDateTimeModified = DateTime.Now
            });
            repository.SaveChanges();
        }

        public IQueryable<History> GetHistoryByProjectIDByPropertyID(Guid projId, Guid propId)
        {
            return repository.Histories.Where(q => q.ProjectID == projId && q.PropertyID == propId).OrderByDescending(q => q.PropertyDateTimeModified);
        }
    }
}