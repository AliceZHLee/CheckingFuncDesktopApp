using System;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace ICEChecker.DictWindow.Models {
    public class AbbrMapping {
        public int AbbrMappingID { get; set; }
        public string ProductName { get; set; }
        public string Abbr { get; set; }

    }
    public class ExcelDataService {
        OleDbConnection Conn;
        OleDbCommand Cmd;

        public ExcelDataService(string path) {
            string ExcelFilePath = path;
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ExcelFilePath + ";Extended Properties=Excel 12.0;Persist Security Info=True";
            Conn = new OleDbConnection(excelConnectionString);
        }

        /// <summary>  
        /// Method to Get All the Records from Excel  
        /// </summary>  
        /// <returns></returns>  
        public async Task<ObservableCollection<AbbrMapping>> ReadRecordFromEXCELAsync() {
            ObservableCollection<AbbrMapping> AbbrMappings = new ObservableCollection<AbbrMapping>();
            await Conn.OpenAsync();
            Cmd = new OleDbCommand();
            Cmd.Connection = Conn;
            Cmd.CommandText = "Select * from [Sheet1$]";
            var Reader = await Cmd.ExecuteReaderAsync();
            int id = 1;
            while (Reader.Read()) {
                AbbrMappings.Add(new AbbrMapping() {
                    AbbrMappingID = ++id,
                    ProductName = Reader["ProductName"].ToString(),
                    Abbr = Reader["Abbr"].ToString()
                }); 
            }
            Reader.Close();
            Conn.Close();
            return AbbrMappings;
        }

        /// <summary>  
        /// Method to Insert Record in the Excel  
        /// S1. If the EmpNo =0, then the Operation is Skipped.  
        /// S2. If the AbbrMapping is already exist, then it is taken for Update  
        /// </summary>  
        /// <param name="Emp"></param>  
        public async Task<bool> ManageExcelRecordsAsync(AbbrMapping item) {
            bool IsSave = false;
            if (item.AbbrMappingID != 0) {
                await Conn.OpenAsync();
                Cmd = new OleDbCommand();
                Cmd.Connection = Conn;
                Cmd.Parameters.AddWithValue("@AbbrMappingID", item.AbbrMappingID);
                Cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                Cmd.Parameters.AddWithValue("@Abbr", item.Abbr);
                

                if (!IsAbbrMappingRecordExistAsync(item).Result) {
                    Cmd.CommandText = "Insert into [Sheet1$] values (@AbbrMappingID,@ProductName,@Abbr)";
                }
                else {
                    Cmd.CommandText = "Update [Sheet1$] set AbbrMappingID=@AbbrMappingID,ProductName=@ProductName,Abbr=@Abbr where AbbrMappingID=@AbbrMappingID";

                }
                int result = await Cmd.ExecuteNonQueryAsync();
                if (result > 0) {
                    IsSave = true;
                }
                Conn.Close();
            }
            return IsSave;

        }
        /// <summary>  
        /// The method to check if the record is already available   
        /// in the workgroup  
        /// </summary>  
        /// <param name="emp"></param>  
        /// <returns></returns>  
        private async Task<bool> IsAbbrMappingRecordExistAsync(AbbrMapping stud) {
            bool IsRecordExist = false;
            Cmd.CommandText = "Select * from [Sheet1$] where AbbrMappingId=@AbbrMappingID";
            var Reader = await Cmd.ExecuteReaderAsync();
            if (Reader.HasRows) {
                IsRecordExist = true;
            }

            Reader.Close();
            return IsRecordExist;
        }
    }
}
