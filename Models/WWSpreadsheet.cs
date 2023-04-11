using System.Collections.Generic;

namespace Webwonders.Extensions;


public class WWSpreadsheetAttribute : System.Attribute
{
    public bool EmptyCellsAllowed { get; set; }
    public int RepeatedFromColumn { get; set; } // From this column the data gets repeated until there is no mora data. Columns start at base 0
}

public class WWSpreadsheetColumnAttribute : System.Attribute
{
    public string ColumnName { get; set; }
    public bool ColumnRequired { get; set; }
    public bool RepeatedColumn { get; set; }
}


public class WWSpreadsheetCell
{
    public string ColumName { get; set; }
    public string ColumnValue { get; set; }
    public string PropertyName { get; set; }
    public bool IsRequired { get; set; }
}

public class WWSpreadsheetRow
{
    public int Number { get; set; }
    //public Dictionary<string, string> Data { get; }
    public List<WWSpreadsheetCell> Cells { get; set; }
    public WWSpreadsheetRow(int number)
    {
        Number = number;
        Cells = new List<WWSpreadsheetCell>();
        //Data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase); // make sure the cases are ignored
    }
}

public class WWSpreadSheet
{
    public List<WWSpreadsheetRow> Rows { get; set; }
    public WWSpreadSheet()
    {
        Rows = new List<WWSpreadsheetRow>();
    }
}
