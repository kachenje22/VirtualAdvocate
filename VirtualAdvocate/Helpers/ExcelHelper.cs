#region NameSpaces
using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.IO;
using System.Linq;
#endregion
#region VirtualAdvocate.Helpers
namespace VirtualAdvocate.Helpers
{
    #region ExcelHelper
    public class ExcelHelper
    {
        #region ConvertCSVtoDataTable
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine()
                    .Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        #endregion
    }
    #endregion

    #region CSVFunctions
    public static class CSVFunctions
    {

        #region ImportFromCSV
        /// CONVERT CSV DATA TO DATATABLE...
        public static DataTable ImportFromCSV(string strFileName)
        {
            DataTable objDT = null;
            TextFieldParser parser = null;
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fs);
                string[] Lines = File.ReadAllLines(strFileName);
                string[] Fields = Lines[0].Split(new char[] { ',', '\t' });
                int Cols = Fields.GetLength(0);
                objDT = new DataTable();
                objDT.TableName = "Table";
                for (int i = 0; i < Cols; i++)
                    objDT.Columns.Add(Fields[i].Replace("\"", ""), typeof(string));
                DataRow Row;
                for (int i = 1; i < Lines.GetLength(0); i++)
                {
                    parser = new TextFieldParser(new StringReader(Lines[i]));
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        Fields = parser.ReadFields();
                        if (Fields.Length >= 1)
                        {
                            Row = objDT.NewRow();
                            foreach (string field in Fields)
                            {
                                for (int j = 0; j < Cols; j++)
                                {
                                    if (Fields.Length > j)
                                        Row[j] = Fields[j].Replace("\"", "").TrimStart().TrimEnd();
                                    else
                                        Row[j] = string.Empty;
                                }
                            }
                            objDT.Rows.Add(Row);
                        }
                    }
                    parser.Close();
                }
            }
            catch (Exception ex)
            {
                objDT = null;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                if (sr != null)
                    sr.Close();
            }
            if (objDT != null && objDT.Rows.Count > 0)
                objDT = objDT.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is System.DBNull ||
                    string.Compare((field.ToString()).Trim(), string.Empty) == 0)).CopyToDataTable();
            return objDT;
        }
        #endregion

        #region ConvertToCsvCell
        private static string ConvertToCsvCell(string value)
        {
            var mustQuote = value.Any(x => x == ',' || x == '\"' || x == '\r' || x == '\n');

            if (!mustQuote)
            {
                return value;
            }

            value = value.Replace("\"", "\"\"");

            return string.Format("\"{0}\"", value);
        }
        #endregion

        #region DataTableToCsv
        public static void DataTableToCsv(DataTable dt, string destinationFilePath)
        {
            try
            {
                char delimiter = ',';
                char quote = '"';
                char escape = '"';
                using (StreamWriter sw = new StreamWriter(destinationFilePath))
                {
                    string csvLine = string.Empty;
                    foreach (System.Data.DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName.Contains(delimiter.ToString()))
                        {
                            col.ColumnName = quote + col.ColumnName + quote;
                        }

                        csvLine = csvLine + col.ColumnName + delimiter;
                    }
                    sw.WriteLine(csvLine.Remove(csvLine.Length - 1));

                    csvLine = string.Empty;
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (System.Data.DataColumn col in dt.Columns)
                        {
                            string field = string.Empty;
                            if (row[col] is DateTime)
                            {
                                field = string.Format("{0:MM/dd/yyyy hh:mm tt}", row[col] as DateTime?);
                            }
                            else if (row[col] is Decimal)
                            {
                                field = string.Format("{0:#########0.00###}", row[col] as Decimal?);
                            }
                            else
                            {
                                field = row[col].ToString()
                                    .Replace("\r", string.Empty)
                                    .Replace("\n", string.Empty)
                                    .Replace(quote.ToString(), escape.ToString() + quote.ToString())
                                    .Trim();
                            }

                            if (field is string && field.StartsWith("0"))
                                field = "\t" + field;

                            if (field.Contains(delimiter.ToString()) || field.Contains(quote.ToString()))
                            {
                                field = quote + field + quote;
                            }

                            csvLine = csvLine + field + delimiter;
                        }

                        sw.WriteLine(csvLine.Remove(csvLine.Length - 1));
                        csvLine = string.Empty;
                    }
                    sw.Close();
                }

                FileAttributes attributes = File.GetAttributes(destinationFilePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(destinationFilePath, attributes & ~FileAttributes.ReadOnly);
            }
            catch (Exception e)
            {

            }
        }
        #endregion

    }
    #endregion
}
#endregion