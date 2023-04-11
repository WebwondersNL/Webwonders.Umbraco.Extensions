using System;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;


//***************
//https://github.com/schotime/NPoco/wiki/Mapping
//By default no mapping is required. It will be assumed that the table name will be the class name 
//and the primary key will be 'Id' if it's not specified with attributes, autoincrement is standard on
////***************



namespace Webwonders.Extensions;


#region DatabaseConstants

//// used to avoid nested transactions, which NPOCO allows, but SQL lite does not
//public enum TransactionState 
//{ 
//    Unspecified = 0,
//    NewTransaction = 1,
//    ExistingTransaction = 2,
//}

#endregion DatabaseConstants


#region DatabaseTables
public class WWDbBase
{
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    //[DateTime2]
    public DateTime Created { get; set; }
    //[DateTime2]
    public DateTime Modified { get; set; }

    //[DateTime2]
    [Index(IndexTypes.NonClustered)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? Deleted { get; set; }

}
#endregion DatabaseTables


#region DatabaseColumns
public class WWDbBaseColumns
{
    public const string Id = "Id";
    public const string Created = "Created";
    public const string Modified = "Modified";
    public const string Deleted = "Deleted";
}
#endregion DatabaseColumns

// removed: inheriting in viewmodels can give problems in binding
//#region ViewTables
//public class VwDbBase
//{
//    [DataMember(Name ="id")]
//    public int Id { get; set; }
//    public DateTime Created { get; set; }
//    public DateTime Modified { get; set; }
//    public DateTime? Deleted { get; set; }
//}
//#endregion ViewTables
