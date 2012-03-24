using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace ProjectManager.WebUI.Models
{
    public class Bulk
    {
        public static void Import(String fileName, String tableName)
        {
            DataTable dataTable = CreateDataTable(fileName);
            using (SqlConnection sqlConnection =
                new SqlConnection(WebConfigWorker.GetConnectionString()))
            {
                sqlConnection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    if (tableName != "PropertiesOfProjects" && tableName != "Values")
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(i, i + 1);
                        }
                    }
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        public static void Export(String fileName, String tableName)
        {
            String queryString = @"UNLOAD TO 'c:\data.unl' SELECT * FROM dbo." + tableName + ";";
            Repository projectDataBaseEntities = new Repository();
            var p = projectDataBaseEntities.ExecuteStoreQuery<object>(queryString, null);

            //using (SqlConnection sqlConnection = 
            //    new SqlConnection(WebConfigWorker.GetConnectionString()))
            //{
            //    sqlConnection.Open();
            //    SqlCommand commandSourceData = new SqlCommand(
            //        "SELECT * FROM dbo." +
            //        tableName + " ;", sqlConnection);
            //    SqlDataReader reader = commandSourceData.ExecuteReader();

            //}
        }

        private static DataTable CreateDataTable(String fileName)
        {
            DataTable dataTable = new DataTable();
            using (StreamReader sr = File.OpenText(fileName))
            {
                String line;
                bool f = true;
                while ((line = sr.ReadLine()) != null)
                {
                    String[] data = line.Split(';');
                    if (data.Length > 0)
                    {
                        if (f)
                        {
                            foreach (var item in data)
                            {
                                dataTable.Columns.Add(new DataColumn());
                            }
                            f = false;
                        }
                        DataRow row = dataTable.NewRow();
                        row.ItemArray = data;
                        dataTable.Rows.Add(row);
                    }
                }
            }
            return dataTable;
        }

    }
}