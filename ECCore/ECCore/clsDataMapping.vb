Imports System.Collections
Imports System.IO
Imports ECCore
Imports Canvas

<Serializable> Public Class clsDataMapping
    'AS/12323 incorporated the class

    Public DataMappingGUID As New Guid

    'External db
    Public Property externalDBType As enumMappedDBType = enumMappedDBType.mdtNone 'may be SQL Server, Access, Oracle, another ECC Project
    Public Property externalDBconnString As String = ""
    Public Property externalDBname As String = ""
    Public Property externalTblName As String = ""
    Public Property externalColName As String = ""
    Public Property externalMapkeyColName As String = "" 'Name of external Map Key column selected by user in UI 
    Public Property externalColDatatype As String = "" 'AS/22602c===
    Public Property externalColMaxLength As String = ""
    Public Property externalColIsNullable As Boolean = False
    Public Property externalMapcolDatatype As String = ""
    Public Property externalMapcolMaxLength As String = ""
    Public Property externalMapcolIsNullable As Boolean = False 'AS/22602c==

    'current ECC project
    Public Property eccMappedColID As Guid = Guid.Empty 'GUID of the mapped DG column
    Public Property eccMappedColType As enumMappedColType = enumMappedColType.mapNone 'corresponds to the option to import as what selected by user

    '=== fieds which were removed when moving clsDataMepping to clsProjectManager
    'Public externalValue As Object                     'actual imported value -- alternative name, or cost, or risk, or attr value, or nonPW judgment.  
    'Public externalAltMapKey As String                 'value from the column selected by user as Key column; the same as MapKey in ECD 
    'Public eccValueGuid As Guid                         'guid assigned to the newly imported value
    'Public eccAltGuid As Guid                           'ID of the alt to which the value (cost, risk, ...) was imported , for new alternatives eccAltGuid = eccValueGuid
    '==

    'AltID
    'ParticipantID
    'CoveringObjID
    'Priority

    Public Enum enumMappedColType As Integer
        mapNone = -1
        mapAlternaives = 0
        mapCosts = 1
        mapRisks = 2
        mapAttributes = 3
        mapInfodocs = 4 'AS/12323zg
        mapJudgments = 5
        mapKey = 6 'AS/12323zn
    End Enum

    Public Enum enumMappedDBType As Integer
        mdtNone = -1
        mdtECC = 1
        mdtAccess = 2
        mdtSQL = 3
        mdtOracle = 4
        mdtMSProject = 5 'AS/15597
        mdtMSProjectServer = 6 'AS/15597
    End Enum

    Public Sub New() 'AS/12323xm
        DataMappingGUID = Guid.NewGuid
        eccMappedColID = Guid.Empty
    End Sub

    Public Sub New(DBType As enumMappedDBType, DBconnString As String, DBname As String, TblName As String, ColName As String, MapkeyColName As String) 'AS/12323xm
        DataMappingGUID = Guid.NewGuid
        externalDBType = DBType
        externalDBconnString = DBconnString
        externalDBname = DBname
        externalTblName = TblName
        externalColName = ColName
        externalMapkeyColName = MapkeyColName

        eccMappedColID = Guid.Empty
        eccMappedColType = enumMappedColType.mapNone 'AS/15624a
    End Sub

End Class

<Serializable> Public Class clsDataMappingValue
    Public exValue As Object = "" 'AS/14505 replaced As String with As Object
    Public exMapKey As String = ""
    Public eccAltGuid As Guid = Guid.Empty
End Class

