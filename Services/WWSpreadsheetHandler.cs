using System;
using System.Collections;
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
    /// <summary>
    /// Reads the spreadsheet and returns a class with the rows and columns
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="SpreadsheetFile">Spreadsheet to read</param>
    /// <param name="StopOnError">If true: stops reading after an error, result will be null</param>
    /// <returns>WWSpreadsheet containing data</returns>
    WWSpreadSheet? ReadSpreadsheet<T>(string SpreadsheetFile, bool StopOnError = false) where T : class;


    /// <summary>
    /// Writes data to a spreadsheet, returning a memory stream
    /// </summary>
    /// <typeparam name="T">type of class to write</typeparam>
    /// <param name="data">IEnumerable of <typeparamref name="T"/> containing data</param>
    /// <param name="StopOnError">If true: stops writing after an error, result will be null</param>
    /// <returns>Memorystream with spreadsheet</returns>
    MemoryStream? WriteSpreadsheet<T>(IEnumerable<T> data, bool StopOnError) where T : class;

}

public class WWSpreadsheetHandler : IWWSpreadsheetHandler
{

    private readonly ILogger<WWSpreadsheetHandler> _logger;


    private class ObjectSpreadsheetColumnDefinition
    {
        public string? PropertyName { get; set; }
        public string? ColumnName { get; set; }
        public bool ColumnRequired { get; set; }
        public bool RepeatedColumn { get; set; }
    }

    private class ObjectSpreadsheetDefinition
    {
        public bool EmptyCellsAllowed { get; set; }
        public int? RepeatedFromColumn { get; set; }
        public List<ObjectSpreadsheetColumnDefinition> ColumnDefinitions { get; set; }
        public ObjectSpreadsheetDefinition()
        {
            ColumnDefinitions = new List<ObjectSpreadsheetColumnDefinition>();
        }
    }


    public WWSpreadsheetHandler(ILogger<WWSpreadsheetHandler> logger)
    {
        _logger = logger;
    }





    ///<inheritdoc />
    public WWSpreadSheet? ReadSpreadsheet<T>(string SpreadsheetFile, bool StopOnError = false) where T : class
    {
        WWSpreadSheet? result = null;

        var spreadsheetFile = new FileInfo(SpreadsheetFile);
        if (spreadsheetFile != null && spreadsheetFile.Exists)
        {
            // First: get the definition of the spreadsheet and its rows
            var spreadsheetDefinition = new ObjectSpreadsheetDefinition();
            if (typeof(T).GetCustomAttribute<WWSpreadsheetAttribute>() is WWSpreadsheetAttribute spreadsheetAttribute)
            {
                spreadsheetDefinition.EmptyCellsAllowed = spreadsheetAttribute.EmptyCellsAllowed;
                spreadsheetDefinition.RepeatedFromColumn = spreadsheetAttribute?.RepeatedFromColumn;
            }
            foreach (PropertyInfo propInfo in typeof(T).GetProperties())
            {
                if (propInfo.GetCustomAttribute<WWSpreadsheetColumnAttribute>() is WWSpreadsheetColumnAttribute columnAttribute)
                {
                    spreadsheetDefinition.ColumnDefinitions.Add(new ObjectSpreadsheetColumnDefinition
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




    ///<inheritdoc />
    public MemoryStream? WriteSpreadsheet<T>(IEnumerable<T> data, bool StopOnError) where T : class
    {
        // First: get the definition of the spreadsheet and its rows
        var spreadsheetDefinition = new ObjectSpreadsheetDefinition();
        if (typeof(T).GetCustomAttribute<WWSpreadsheetAttribute>() is WWSpreadsheetAttribute spreadsheetAttribute)
        {
            spreadsheetDefinition.EmptyCellsAllowed = spreadsheetAttribute.EmptyCellsAllowed;
            spreadsheetDefinition.RepeatedFromColumn = spreadsheetAttribute?.RepeatedFromColumn;
        }
        foreach (PropertyInfo propInfo in typeof(T).GetProperties())
        {
            if (propInfo.GetCustomAttribute<WWSpreadsheetColumnAttribute>() is WWSpreadsheetColumnAttribute columnAttribute)
            {
                spreadsheetDefinition.ColumnDefinitions.Add(new ObjectSpreadsheetColumnDefinition
                {
                    PropertyName = propInfo.Name,
                    ColumnName = columnAttribute?.ColumnName ?? "",
                    ColumnRequired = columnAttribute?.ColumnRequired ?? false,
                    RepeatedColumn = columnAttribute?.RepeatedColumn ?? false
                    //false, // No repeated properties yet (perhaps in the future for a property containing a list)
                });
            }
        }

        if (spreadsheetDefinition.ColumnDefinitions.Count == 0)
        {
            if (!StopOnError)
            {
                return null;
            }
            throw new Exception("WWSpreadsheethandler, WriteSpreadsheet: No columns defined for spreadsheet");
        }

        // Next: write the data to the spreadsheet
        var workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet();
        IRow headerRow = sheet.CreateRow(0);
        for (int i = 0; i < spreadsheetDefinition.ColumnDefinitions.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(spreadsheetDefinition.ColumnDefinitions[i].ColumnName);
        }
        foreach (T element in data)
        {
            IRow row = sheet.CreateRow(sheet.LastRowNum + 1);
            for (int i = 0; i < spreadsheetDefinition.ColumnDefinitions.Count; i++)
            {
                var columnDefinition = spreadsheetDefinition.ColumnDefinitions[i];

                object? value = null;
                if (!String.IsNullOrWhiteSpace(columnDefinition.PropertyName))
                {
                    value = element.GetType().GetProperty(columnDefinition.PropertyName)?.GetValue(element);
                }

                if (!spreadsheetDefinition.EmptyCellsAllowed && value == null && StopOnError)
                {
                    return null;
                }
                if (columnDefinition.ColumnRequired && value == null && StopOnError)
                {
                    return null;
                }


                if (columnDefinition.RepeatedColumn && i == spreadsheetDefinition.ColumnDefinitions.Count - 1)
                {
                    // repeated column (can only be the last one): write the value as an array
                    object[]? array = null;
                    if (value != null)
                    {
                        // make sure that value is an array or enumerable
                        if (value.GetType().IsArray)
                        {
                            array = (object[])value;
                        }
                        else if (value is IEnumerable enumerable)
                        {
                            array = enumerable.Cast<object>().ToArray();
                        }
                        if (array != null)
                        {
                            //create cells for each element in the array
                            for (int j = 0; j < array.Length; j++)
                            {
                                row.CreateCell(i + j).SetCellValue(array[j]?.ToString() ?? "");
                            }
                        }
                    }
                }
                else
                {
                    // Normal column, just write the value
                    row.CreateCell(i).SetCellValue(value?.ToString() ?? "");
                }
            }
        }

        using var exportData = new MemoryStream();
        workbook.Write(exportData, true); // true to leave stream open
        return exportData;
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
    private WWSpreadSheet? ReadSpreadSheet(FileInfo spreadsheetFile, ObjectSpreadsheetDefinition spreadsheetDefinition, bool StopOnError = false)
    {
        WWSpreadSheet? result = null;

        if (spreadsheetFile != null && spreadsheetFile.Exists)
        {


            using var stream = spreadsheetFile.Open(FileMode.Open);         
            stream.Position = 0;
            
            var xssWorkbook = new XSSFWorkbook(stream);
            ISheet sheet = xssWorkbook.GetSheetAt(0);


            var ColumnNames = new List<string>();
            var ColumnValues = new Dictionary<int, string>();


            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;


            for (int r = (sheet.FirstRowNum); r <= sheet.LastRowNum; r++)
            {

                // Reinit the values
                ColumnValues = new Dictionary<int, string>();

                result ??= new WWSpreadSheet();

                // TODO empty rows
                if (sheet.GetRow(r) is IRow currentRow)
                {
                    if (!spreadsheetDefinition.EmptyCellsAllowed && currentRow.Cells.Any(c => c.CellType == CellType.Blank))
                    {
                        if (StopOnError)
                        {
                            result = null; // discard reading up to now
                            throw new Exception($"Error in reading spreadsheet, row {r} contains empty cells. Stopped reading");
                        }
                    }
                    bool firstRowErrorLogged = false;
                    // Iterate columns 
                    for (int j = currentRow.FirstCellNum; j < cellCount; j++)
                    {
                        if (currentRow.GetCell(j) is ICell currentCell)
                        {
                            string currentCellValue = (currentCell.CellType != CellType.Blank) ? (currentCell.ToString() ?? "") 
                                                                                               : "";
                            // Is row the first: add to header, otherwise add to value
                            if (currentRow.RowNum == headerRow.RowNum)
                            {
                                // Headerrow can not contain empty cells: it would be impossible to match the columns to the properties
                                // so depending on the StopOnError flag, either throw an exception or log an error and discard the column
                                if (currentCell.CellType == CellType.Blank)
                                {
                                    if (StopOnError)
                                    {
                                        result = null; // discard reading up to now
                                        throw new Exception($"Error in reading spreadsheet, first row contains empty column. Stopped reading");
                                    }
                                    else
                                    {
                                        if (!firstRowErrorLogged)
                                        {
                                            _logger.LogError($"Error in reading spreadsheet, first row contains empty column. Column is skipped.");
                                            firstRowErrorLogged = true;
                                        }

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
                                    ObjectSpreadsheetColumnDefinition? propDef = null;
                                    if (spreadsheetDefinition.RepeatedFromColumn != null
                                        && spreadsheetDefinition.RepeatedFromColumn.Value > 0
                                        && i >= spreadsheetDefinition.RepeatedFromColumn.Value)
                                    {
                                        propDef = spreadsheetDefinition.ColumnDefinitions.FirstOrDefault(x => x.RepeatedColumn);
                                    }
                                    else
                                    {
                                        propDef = spreadsheetDefinition.ColumnDefinitions.FirstOrDefault(x => x.ColumnName?.ToLower() == ColumnNames[i].ToLower());
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
                                                throw new Exception($"Error in reading spreadsheet, row {currentRow.RowNum - headerRow.RowNum + 1},  column {i + 1}. Stopped reading");
                                            }
                                            else
                                            {
                                                _logger.LogError("Error in reading spreadsheet, row {row},  column {counter}. Row is skipped.", currentRow.RowNum - headerRow.RowNum + 1, i + 1);
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
        return result;
    }


}
