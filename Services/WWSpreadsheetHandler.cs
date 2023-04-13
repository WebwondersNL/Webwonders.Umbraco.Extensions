using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Umbraco.Extensions;

namespace Webwonders.Extensions;

public interface IWWSpreadsheetHandler
{
    WWSpreadSheet ReadSpreadsheet<T>(string SpreadsheetFile, bool StopOnError = false) where T : class;
}

public class WWSpreadsheetHandler : IWWSpreadsheetHandler
{

    private readonly ILogger<WWSpreadsheetHandler> _logger;


    private class ObjectSpreadsheetRowDefinition
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public bool ColumnRequired { get; set; }
        public bool RepeatedColumn { get; set; }
    }

    private class ObjectSpreadsheetDefinition
    {
        public bool EmptyCellsAllowed { get; set; }
        public int? RepeatedFromColumn { get; set; }
        public List<ObjectSpreadsheetRowDefinition> RowDefinitions { get; set; }
        public ObjectSpreadsheetDefinition()
        {
            RowDefinitions = new List<ObjectSpreadsheetRowDefinition>();
        }
    }


    public WWSpreadsheetHandler(ILogger<WWSpreadsheetHandler> logger)
    {
        _logger = logger;
    }






    public WWSpreadSheet ReadSpreadsheet<T>(string SpreadsheetFile, bool StopOnError = false) where T : class
    {
        WWSpreadSheet result = null;

        FileInfo spreadsheetFile = new FileInfo(SpreadsheetFile);
        if (spreadsheetFile != null && spreadsheetFile.Exists)
        {
            // First: get the definition of the spreadsheet and its rows
            ObjectSpreadsheetDefinition spreadsheetDefinition = new ObjectSpreadsheetDefinition();
            if (typeof(T).GetCustomAttribute<WWSpreadsheetAttribute>() is WWSpreadsheetAttribute spreadsheetAttribute)
            {
                spreadsheetDefinition.EmptyCellsAllowed = spreadsheetAttribute.EmptyCellsAllowed;
                spreadsheetDefinition.RepeatedFromColumn = spreadsheetAttribute?.RepeatedFromColumn;
            }
            foreach (PropertyInfo propInfo in typeof(T).GetProperties())
            {
                if (propInfo.GetCustomAttribute<WWSpreadsheetColumnAttribute>() is WWSpreadsheetColumnAttribute columnAttribute)
                {
                    spreadsheetDefinition.RowDefinitions.Add(new ObjectSpreadsheetRowDefinition
                    {
                        PropertyName = propInfo.Name,
                        ColumnName = columnAttribute?.ColumnName ?? "",
                        ColumnRequired = columnAttribute?.ColumnRequired ?? false,
                        RepeatedColumn = columnAttribute?.RepeatedColumn ?? false,
                    });
                }
            }

            // Then: read the spreadsheet and try to fit in the definition
            result = ReadSpreadSheet(spreadsheetFile, spreadsheetDefinition, StopOnError);
        }

        return result;
    }


    /// <summary>
    /// Read the Spreadsheet and return as class Spreadsheet
    /// where the rows have cells that couple the column- and propertyname and the values
    /// </summary>
    /// <param name="spreadsheetFile"></param>
    /// <param name="spreadsheetDefinition"></param>
    /// <param name="StopOnError"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private WWSpreadSheet ReadSpreadSheet(FileInfo spreadsheetFile, ObjectSpreadsheetDefinition spreadsheetDefinition, bool StopOnError = false)
    {
        WWSpreadSheet result = null;

        if (spreadsheetFile != null && spreadsheetFile.Exists)
        {


            using (var stream = spreadsheetFile.Open(FileMode.Open))
            {
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                ISheet sheet = xssWorkbook.GetSheetAt(0);


                List<String> ColumnNames = new List<string>();
                Dictionary<int, String> ColumnValues = new Dictionary<int, string>();


                IRow headerRow = sheet.GetRow(0);
                int cellCount = headerRow.LastCellNum;


                for (int r = (sheet.FirstRowNum); r <= sheet.LastRowNum; r++)
                {

                    // Reinit the values
                    ColumnValues = new Dictionary<int, string>();

                    if (result == null)
                    {
                        result = new WWSpreadSheet();
                    }

                    // TODO empty rows
                    // Iterate rows
                    if (sheet.GetRow(r) is IRow currentRow
                        && (
                             (!spreadsheetDefinition.EmptyCellsAllowed && !currentRow.Cells.Any(c => c.CellType == CellType.Blank))
                             || (spreadsheetDefinition.EmptyCellsAllowed && currentRow.Cells.Any(c => c.CellType != CellType.Blank))
                           )
                        )
                    //&& (spreadsheetDefinition.EmptyCellsAllowed 
                    //    || (!spreadsheetDefinition.EmptyCellsAllowed && !currentRow.Cells.Any(c => c.CellType == CellType.Blank))))
                    {
                        // Iterate columns
                        for (int j = currentRow.FirstCellNum; j < cellCount; j++)
                        {
                            if (currentRow.GetCell(j) is ICell currentCell)
                            {
                                string currentCellValue = (currentCell.CellType != CellType.Blank) ? currentCell.ToString() : "";
                                // Is row the first: add to header, otherwise add to value
                                if (currentRow.RowNum == headerRow.RowNum)
                                {
                                    if (currentCell.CellType == CellType.Blank)
                                    {
                                        if (StopOnError)
                                        {
                                            result = null; // discard reading up to now
                                            throw new Exception($"Error in reading spreadsheet, first row contains empty column. Stopped reading");
                                        }
                                        else
                                        {
                                            _logger.LogError($"Error in reading spreadsheet, first row contains empty column. Column is skipped.");
                                        }
                                    }
                                    ColumnNames.Add(currentCellValue);
                                }
                                else
                                {
                                    ColumnValues.Add(currentCell.ColumnIndex, currentCellValue);
                                }

                            }
                        }

                        // for all rows except the header: create a WWSpreadsheetRow
                        // and all contained WWSpreadsheetCells
                        if (currentRow.RowNum != headerRow.RowNum)
                        {
                            if (ColumnValues.Where(x => !String.IsNullOrWhiteSpace(x.Value)).Any())
                            {
                                result.Rows.Add(new WWSpreadsheetRow(currentRow.RowNum - headerRow.RowNum + 1)); // number in spreadsheetrow is actual number + 2,
                                                                                                                 // so it skips the title and starts at 1 instead of 0
                                                                                                                 // this wil make the muber the same as the actual spreadsheet row number
                                for (int i = 0; i < ColumnNames.Count; i++)
                                {
                                    if (!String.IsNullOrEmpty(ColumnNames[i]))
                                    {
                                        string currentColumnsValue = "";
                                        if (ColumnValues.ContainsKey(i))
                                        {
                                            currentColumnsValue = ColumnValues[i];
                                        }
                                        // if the last columns are to be repeated and we are in one of those columns:
                                        // get the definition of the column to be repeated.
                                        // otherwise: get the definition of the current column
                                        ObjectSpreadsheetRowDefinition propDef = null;
                                        if (spreadsheetDefinition.RepeatedFromColumn != null
                                            && spreadsheetDefinition.RepeatedFromColumn.Value > 0
                                            && i >= spreadsheetDefinition.RepeatedFromColumn.Value)
                                        {
                                            propDef = spreadsheetDefinition.RowDefinitions.FirstOrDefault(x => x.RepeatedColumn);
                                        }
                                        else
                                        {
                                            propDef = spreadsheetDefinition.RowDefinitions.FirstOrDefault(x => x.ColumnName.ToLower() == ColumnNames[i].ToLower());
                                        }

                                        // if the definition is found:
                                        // add the row when valid
                                        // use the correct columnname by getting it from the previously collected names
                                        if (propDef != null)
                                        {
                                            if (propDef.ColumnRequired && currentColumnsValue.IsNullOrWhiteSpace())
                                            {
                                                if (StopOnError)
                                                {
                                                    result = null; // discard reading up to now.
                                                    throw new Exception($"Error in reading spreadsheet, row {currentRow.RowNum - headerRow.RowNum + 1},  column {i}. Stopped reading");
                                                }
                                                else
                                                {
                                                    _logger.LogError($"Error in reading spreadsheet, row {currentRow.RowNum - headerRow.RowNum + 1},  column {i}. Row is skipped.");
                                                }
                                            }
                                            else
                                            {
                                                // only add if required and given or not required
                                                //result.Rows[currentRow.RowNum - headerRow.RowNum - 1].Cells.Add(new WWSpreadsheetCell { ColumName = ColumnNames[i], ColumnValue = ColumnValues[i], PropertyName = propDef.PropertyName, IsRequired = propDef.ColumnRequired });
                                                // add to the last row, we always go from top to bottom in the spreadsheet
                                                result.Rows.Last().Cells.Add(new WWSpreadsheetCell { ColumName = ColumnNames[i], ColumnValue = currentColumnsValue, PropertyName = propDef.PropertyName, IsRequired = propDef.ColumnRequired });
                                            }
                                        }
                                    }
                                    // in later rows: if the property is found in the definition:
                                    // make cell of columname, value and propertyName and add to current row in fullspreadsheet
                                    // only if the column has a name to avoid probles with KeyValue (columns without title cannot be mapped)
                                    // row - startRow - 1, because first row contains columnames and is ignored in result
                                }
                            }
                        } // Save row
                    }


                }
            }

        }
        return result;
    }


}
