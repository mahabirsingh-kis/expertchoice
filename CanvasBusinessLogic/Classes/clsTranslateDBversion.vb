Imports Microsoft.VisualBasic
Imports System.Data.Common
Imports ECCore.MiscFuncs
Imports ECCore
Imports System.Data
Imports ExpertChoice.Service.Common
Imports ExpertChoice.Service.StringFuncs
Imports GenericDBAccess.ECGenericDatabaseAccess.GenericDB
Imports Canvas

'Translate Old Models=== OldModelsTest ==========================================================
'Search for "OM - later" for the things temporaryly commented out 

'AS/7-30-15 - FB 7774 RTE in Comparion when trying to upload old model
'AS/6-26-15 - FB 7569 RTE: 'ERROR [HY001] [Microsoft][ODBC Microsoft Access Driver] Cannot open any more tables
'AS/om0018 - case 7435 Can't read in ahpz file to racomparion -- added AID field to MustsMustnots table
'AS/om0017 - more to 'AS/om0016 -- check if table exists first
'AS/om0016 - case 7224 Can't upload ECD sample model to Comparion -- old models may not have RAconstraint field in the RAcontraints table
'AS/om0015 - case 7197 RTE: 'Argument 'Index' is not a valid value.' -- Implemented version number in ECC (“ECC_AHPversion”) in the MProperties table similar to the “Version” in ECD – thus we always know which way to proceed with any downloaded model
'AS/om0014 - case 7197 RTE: 'Argument 'Index' is not a valid value.'
'AS/om0013 - added fields aType, aIsDefault, aHasDefaultValue, aDefaultValue to the Project.ahp file as werll as code to create them when translating.
'AS/om0012 - added field AttributeGuidID to the Project.ahp file
'AS/om0011 - dbReader was not closed which generated server error on upload
'AS/om0010 - modified logic to check if the model is old. Also added check FieldExists when translating
'AS/om0009 - UDColsDefs was missing the AttributeGuidID column 
'AS/om0008 - add attributes fields to the UD cols
'AS/om0007 - COMPLETED 6726 "ABU sample model categories are missing in ECD and ECC"
'AS/om0006 - Fixed: The same alt (e.g. A1) in the Attributesvalues table had different aObjectID's
'AS/om0005 = D2751 AD incoprorated Sub CheckOld_AHP to check for the db version
'AS/om0004 - fixed rte which prevented uploading becsuse m_cnn was never closed
'AS/om0003 - translating functions in progress
'AS/om0002 - incorporated clsTranslateDBversion, copied code from ECD, modTranslateDBversion to it. Implemented 'get db version to translate'. 
'AS/om0001 - 10/1/2014 created branch FB6095OldModels

'===Adding PrimaryKey:
'    Function SelectAll() As DataTable
'C:\Data_from_AS3\Comparion\Work\SpyronControls\clsDataProvider.vb

'===Function GetAHPDBVersion in 
'C:\Data_from_AS3\Comparion\Work\ECCore\ECCore\ECTypes.vb

'===Examples of tableExists,fieldExists, write parameters to database ---
'Function SaveToDatabase in C:\Data_from_AS3\Comparion\Work\ECCore\CanvasModule\clsPipeParameters.vb

'===to get db version ---
'function LoadFromDatabase in the same class

'===INSERT INTO MProperties --
'Function SaveMProperties_AHPDatabase in
'C:\Data_from_AS3\Comparion\Work\ECCore\ECCore\StorageWriters.vb

'==='C0052
'	Updated projectDB.ahp with 2 tables: Properties and PipeMessages
'	Saving/Loading pipe properties and pipe messages to/from ahp-file

'Translate Old Models== =============================================================


Public Class clsTranslateDBversion
    'AS/om0002 - incorporated this class, copied code from ECD, modTranslateDBversion


    Private Const MAKE_NULLABLE As Integer = -1 'A9/0360
    Private Const MAKE_NOT_NULLABLE As Integer = 0 'A9/0360

    Private m_cnn As DbConnection
    Private m_ProviderType As DBProviderType 'AS/om0003
    Private m_DataSet As DataSet

    'Dim m_cat As New ADOX.Catalog
    'Dim m_tbl As ADOX.table
    'Dim m_fld As ADOX.column
    'Dim m_pk As ADOX.Key
    Dim m_ECDatabaseVersion As String
    Dim cTables As Collection 'A8/3754


    Public Sub New(ByVal ProviderType As DBProviderType, ByVal sConnectionString As String) 'AS/om0003

        m_cnn = GetDBConnection(ProviderType, sConnectionString)
        m_ProviderType = ProviderType
        m_DataSet = New DataSet

        m_cnn.Open()

    End Sub

    Public Function VersionTranslated(sDatabaseVersionToTranslate As String, bSuppressPromptsIfDownloadedFromCore As Boolean, AHP_ChangedInECD As Boolean) As Boolean 'AS/6-28-16 added AHP_ChangedInECD

        m_ECDatabaseVersion = Left(sDatabaseVersionToTranslate, 1) & "." & Mid(sDatabaseVersionToTranslate, 3)

        Call TranslateToVersion_318(AHP_ChangedInECD) 'AS/6-28-16 added AHP_ChangedInECD

        If m_cnn.State = ConnectionState.Open Then m_cnn.Close() 'AS/om0004
        m_cnn = Nothing 'AS/om0004

        'm_cat = Nothing 'A9/1466===
        'm_tbl = Nothing
        'm_fld = Nothing
        'm_pk = Nothing 'A9/1466==

        VersionTranslated = True

    End Function


    Private Sub TranslateToVersion_318(AHP_ChangedInECD As Boolean) 'AS/6-28-16 added AHP_ChangedInECD

        Dim sSQL As String
        Dim oCommandFrom As DbCommand = GetDBCommand(m_ProviderType)
        Dim oCommandTo As DbCommand = GetDBCommand(m_ProviderType)
        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType) 'AS/om0014
        Dim affected As Integer
        oCommandFrom.Connection = m_cnn
        oCommandTo.Connection = m_cnn
        oCommand.Connection = m_cnn 'AS/om0014

        Dim dbReader As DbDataReader
        Dim cMap_aIDtoUID As New Collection         'map aID (AttributeGuidID) to UID, UID as the key
        Dim cMap_EnumGUIDtoCVID As New Collection   'maps EnumGuidID to CVID, CVID as the key
        Dim cMap_aObjectIDtoAID As New Collection   'maps aObjectID to aObjectID_String (AID), AID as the key
        Dim cCategoryMapping As New Collection      'maps AttributeGuidID to CategoryGuidID, CategoryGuidID as the key. In fact it is ECCore_CategoryMappings table 'AS/om0007
        Dim newGUID As String
        Dim sParamValue As String
        Dim cValueTypes As Collection               'holds valuetypes (integers) for UID's, UID as the key 'AS/6-26-15
        cValueTypes = getValueTypesByUIDs() 'AS/6-26-15

        If TableExists("MustsMustNots") Then 'AS/om0018===
            If Not FieldExists("AID", "MustsMustNots") Then
                sSQL = "ALTER TABLE MustsMustNots ADD COLUMN AID VARCHAR (25)"
                oCommandTo.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

                oCommand.CommandText = "SELECT * FROM MustsMustNots"
                dbReader = DBExecuteReader(m_ProviderType, oCommand)

                While dbReader.Read
                    sSQL = "UPDATE MustsMustNots SET AID = 'A" & CStr(dbReader("row")) & "' WHERE row = " & dbReader("row").ToString
                    oCommandTo.CommandText = sSQL
                    affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                End While
                dbReader.Close()
            End If
        End If 'AS/om0018==

        If TableExists("RAconstraints") Then 'AS/om0017 enclosed
            If Not FieldExists("RAConstraint", "RAconstraints") Then 'AS/om0016===
                sSQL = "ALTER TABLE RAconstraints ADD COLUMN RAConstraint VARCHAR (255)"
                oCommand.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)

                oCommand.CommandText = "SELECT * FROM RAconstraints ORDER by SOrder"
                dbReader = DBExecuteReader(m_ProviderType, oCommand)

                Dim cValues As New Collection
                While dbReader.Read
                    sSQL = "UPDATE RAconstraints SET RAconstraint = '" & CStr(dbReader("Constraint")) & "'"
                    oCommandTo.CommandText = sSQL
                    affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                End While

                dbReader.Close()

                sSQL = "ALTER TABLE [RAconstraints] DROP COLUMN [Constraint]"
                oCommand.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            End If

            If Not FieldExists("CCID", "RAconstraints") Then 'AS/11-20-15a===
                sSQL = "ALTER TABLE RAconstraints ADD COLUMN CCID VARCHAR (8), AssociatedUDcolKey VARCHAR (8),AssociatedCVID VARCHAR (8), ECC_LinkedAttributeID VARCHAR (50), ECC_LinkedEnumID VARCHAR (50), ECC_ID INTEGER"
                oCommand.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)

                'set CCID's
                oCommandFrom.CommandText = "SELECT DISTINCT RAconstraint FROM RAconstraints"
                dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)
                Dim i As Integer
                i = 1
                While dbReader.Read
                    oCommandTo.CommandText = "UPDATE RAconstraints SET CCID = " & i & " WHERE RAconstraint = '" & dbReader("RAconstraint").ToString & "'"
                    i = i + 1 'AS/11-20-15b
                    affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                End While
                dbReader.Close()
            Else 'AS/11-24-15a=== check CCID values, and if all 0's, set them up
                oCommandFrom.CommandText = "SELECT CCID FROM RAconstraints"
                dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

                Dim bAllZeros As Boolean = True
                While dbReader.Read
                    If CInt(dbReader(0)) > 0 Then 'AS/11-24-15b replaced <> with > b/c may be negative due to one of the intermediate builds
                        bAllZeros = False
                        Exit While
                    End If
                End While
                dbReader.Close()

                If bAllZeros Then
                    oCommandFrom.CommandText = "SELECT DISTINCT RAconstraint FROM RAconstraints"
                    dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

                    Dim i As Integer = 1
                    While dbReader.Read
                        oCommandTo.CommandText = "UPDATE RAconstraints SET CCID = " & i & " WHERE RAconstraint = '" & dbReader("RAconstraint").ToString & "'"
                        i = i + 1
                        affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                    End While
                    dbReader.Close()
                End If 'AS/11-24-15a==

            End If
            'End If 'AS/12-18-15 moved down

            If Not FieldExists("AssociatedCVID", "RAconstraints") Then 'AS/11-24-15===
                sSQL = "ALTER TABLE RAconstraints ADD COLUMN AssociatedCVID VARCHAR (8)"
                oCommand.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            End If

            If Not FieldExists("ECC_LinkedAttributeID", "RAconstraints") Then
                sSQL = "ALTER TABLE RAconstraints ADD COLUMN ECC_LinkedAttributeID VARCHAR (50), ECC_LinkedEnumID VARCHAR (50), ECC_ID INTEGER"
                oCommand.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            End If 'AS/11-24-15==
        End If 'AS/12-18-15 moved here from above

        If Not FieldExists("AttributeGuidID", "UDColsDefs") Then 'AS/om0010 
            'sSQL = "ALTER TABLE UDColsDefs ADD COLUMN AttributeGuidID VARCHAR (255)" 'AS/om0009=== 
            sSQL = "ALTER TABLE UDColsDefs ADD COLUMN AttributeGuidID VARCHAR (255), aType INTEGER, aIsDefault YesNo, aHasDefaultValue YesNo, aDefaultValue MEMO"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo) 'AS/om0009==
        End If 'AS/om0010

        'Delete obsolete rows from UDcolsData() 'AS/7-30-15===
        sSQL = "DELETE FROM UDColsData WHERE UID NOT in (SELECT UID FROM UDColsDEfs)"
        oCommand.CommandText = sSQL
        affected = DBExecuteNonQuery(m_ProviderType, oCommand) 'AS/7-30-15==

        If Not FieldExists("aObjectID", "UDColsData") Then 'AS/om0010 enclosed
            sSQL = "ALTER TABLE UDColsData ADD COLUMN aObjectID VARCHAR (255)" 'AS/om0008===
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
        End If

        If Not FieldExists("CategoryGuidID", "CategoryValues") Then 'AS/om0010 enclosed
            sSQL = "ALTER TABLE CategoryValues ADD COLUMN CategoryGuidID VARCHAR (50), EnumGuidID VARCHAR (50), AttributeGuidID VARCHAR (50) "
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            sSQL = "ALTER TABLE Categories ADD COLUMN CategoryGuidID VARCHAR (255)"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            sSQL = "DELETE from UDColsData WHERE PID > (SELECT min(PID) from UDColsData)" 'AS/9-30-15===
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo) 'AS/9-30-15==

            sSQL = "UPDATE UDColsData SET UDColsData.PID = " & UNDEFINED_USER_ID
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo) 'AS/om0008==
        End If 'AS/om0010

        If Not AHP_ChangedInECD Then Exit Sub 'AS/6-28-16

        'create attributes tables '
        If TableExists("Attributes") Then 'AS/om0014===
            'oCommand.CommandText = "DROP TABLE Attributes"
            'affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            oCommandFrom.CommandText = "SELECT * FROM Attributes" 'AS/12-09-15b
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom) 'AS/12-09-15b

            'If dbReader.HasRows Then 'AS/12-09-15b enclosed 'AS/2-9-16a===
            Dim bAttributesHasRows As Boolean = dbReader.HasRows
            dbReader.Close() 'AS/2-9-16a===
            If bAttributesHasRows Then 'AS/12-09-15b enclosed 'AS/2-9-16a==
                Try
                    Try
                        oCommand.CommandText = "DROP TABLE Attributes"
                        DBExecuteNonQuery(m_ProviderType, oCommand)
                    Catch ex As Exception
                    End Try
                    'affected = DBExecuteNonQuery(m_ProviderType, oCommand) ' -D3644
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                End Try
            End If 'AS/12-09-15b
            'dbReader.Close() 'AS/12-09-15b 'AS/2-9-16a moved up
        End If 'AS/om0014==

        If Not TableExists("Attributes") Then
            sSQL = "CREATE TABLE [Attributes] (aID VARCHAR (255), aName VARCHAR (255), aType INTEGER, aValueType INTEGER, aIsDefault YesNo, aHasDefaultValue YesNO, aDefaultValue MEMO)"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            oCommandFrom.CommandText = "SELECT * FROM UDColsDefs"
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

            While dbReader.Read
                oCommandTo.CommandText = "INSERT INTO Attributes (aID, aName, aType, aValueType, aIsDefault, aHasDefaultValue, aDefaultValue) VALUES (?,?,?,?,?,?,?)"
                oCommandTo.Parameters.Clear()
                newGUID = Guid.NewGuid().ToString
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aID", newGUID))
                cMap_aIDtoUID.Add(newGUID, dbReader("UID").ToString)
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aName", dbReader("UDColName")))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aType", 2))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aValueType", getValueType(CStr(dbReader("UID")))))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aIsDefault", False))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aHasDefaultValue", False))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aDefaultValue", ""))
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
            End While
            dbReader.Close()
        End If

        If TableExists("ECCore_Categories") Then 'AS/om0014===

            oCommandFrom.CommandText = "SELECT * FROM Categories" 'AS/12-09-15b
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom) 'AS/12-09-15b
            If dbReader.HasRows Then 'AS/12-09-15b enclosed
                Try
                    Try
                        oCommand.CommandText = "DROP TABLE ECCore_Categories"
                        DBExecuteNonQuery(m_ProviderType, oCommand)
                    Catch ex As Exception
                    End Try
                    'affected = DBExecuteNonQuery(m_ProviderType, oCommand) ' -D3644
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                End Try
            End If 'AS/12-09-15b
            dbReader.Close() 'AS/12-09-15b
        End If 'AS/om0014==

        If Not TableExists("ECCore_Categories") Then
            sSQL = "CREATE TABLE [ECCore_Categories] (CategoryGuidID VARCHAR, CategoryName VARCHAR)"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            If Not TableExists("ECCore_CategoryMappings") Then
                sSQL = "CREATE TABLE [ECCore_CategoryMappings] (AttributeGuidID VARCHAR, CategoryGuidID VARCHAR)"
                oCommandTo.CommandText = sSQL
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
            End If

            oCommandFrom.CommandText = "SELECT * FROM Categories"
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

            While dbReader.Read
                'INSERT INTO ECCore_Categories
                oCommandTo.CommandText = "INSERT INTO ECCore_Categories (CategoryGuidID, CategoryName) VALUES (?,?)"
                oCommandTo.Parameters.Clear()
                newGUID = Guid.NewGuid().ToString
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "CategoryGuidID", newGUID))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "CategoryName", dbReader("CategoryName")))
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

                'INSERT INTO ECCore_CategoryMappings
                oCommandTo.CommandText = "INSERT INTO ECCore_CategoryMappings (AttributeGuidID, CategoryGuidID) VALUES (?, ?)"
                oCommandTo.Parameters.Clear()
                sParamValue = cMap_aIDtoUID(dbReader("CategoryID")).ToString  'AttributeGuidID
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "AttributeGuidID", sParamValue))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "CategoryGuidID", newGUID))
                cCategoryMapping.Add(sParamValue, newGUID) 'thi1
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            End While
            dbReader.Close()
        End If


        If TableExists("ECCore_CategoryEnums") Then 'AS/om0014===
            oCommandFrom.CommandText = "SELECT * FROM ECCore_CategoryEnums" 'AS/12-09-15b
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom) 'AS/12-09-15b
            If dbReader.HasRows Then 'AS/12-09-15b enclosed
                Try
                    Try
                        oCommand.CommandText = "DROP TABLE ECCore_CategoryEnums"
                        DBExecuteNonQuery(m_ProviderType, oCommand)
                    Catch ex As Exception
                    End Try
                    'affected = DBExecuteNonQuery(m_ProviderType, oCommand) ' -D3644
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                End Try
            End If 'AS/12-09-15b
            dbReader.Close() 'AS/12-09-15b
        End If 'AS/om0014==

        If Not TableExists("ECCore_CategoryEnums") Then
            sSQL = "CREATE TABLE [ECCore_CategoryEnums] (CategoryGuidID VARCHAR, EnumGuidID VARCHAR, EnumValue VARCHAR)"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            oCommandFrom.CommandText = "SELECT * FROM CategoryValues"
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

            While dbReader.Read
                oCommandTo.CommandText = "INSERT INTO ECCore_CategoryEnums (CategoryGuidID, EnumGuidID, EnumValue) VALUES (?,?,?)"
                oCommandTo.Parameters.Clear()
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "CategoryGuidID", getCategoryGUIDbyUID(dbReader("CategoryID").ToString, cMap_aIDtoUID)))
                sParamValue = getEnumGUIDbyCVID(cMap_EnumGUIDtoCVID, CStr(dbReader("ValueID")))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "EnumGuidID", sParamValue))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "EnumValue", dbReader("ValueName")))

                If Not ItemExists(cMap_EnumGUIDtoCVID, dbReader("ValueID")) Then 'AS/om0007===
                    cMap_EnumGUIDtoCVID.Add(sParamValue, dbReader("ValueID").ToString)
                End If 'AS/om0007==

                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
            End While
            dbReader.Close()
        End If


        If TableExists("AttributesValues") Then 'AS/om0014===
            oCommandFrom.CommandText = "SELECT * FROM AttributesValues" 'AS/12-09-15b
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom) 'AS/12-09-15b
            If dbReader.HasRows Then 'AS/12-09-15b enclosed
                Try
                    Try
                        oCommand.CommandText = "DROP TABLE AttributesValues"
                        DBExecuteNonQuery(m_ProviderType, oCommand)
                    Catch ex As Exception
                    End Try
                    'affected = DBExecuteNonQuery(m_ProviderType, oCommand) ' -D3644
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                End Try
            End If 'AS/12-09-15b
            dbReader.Close() 'AS/12-09-15b
        End If 'AS/om0014==

        If Not TableExists("AttributesValues") Then 'AS/om0007=== moved down
            sSQL = "CREATE TABLE [AttributesValues] (UserID INTEGER, aID VARCHAR, aValueType INTEGER, aValue MEMO, aObjectID VARCHAR,aObjectID_String VARCHAR)"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)

            oCommandFrom.CommandText = "SELECT * FROM UDColsData WHERE AID <> '" & "A0 '" 'AS/10-13-15 added WHERE clause
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)
            Dim aValue As String
            Dim IsCategorical As Boolean

            While dbReader.Read
                oCommandTo.CommandText = "INSERT INTO AttributesValues (UserID, aID, aValueType,aValue,aObjectID,aObjectID_String) VALUES (?,?,?,?,?,?)"
                oCommandTo.Parameters.Clear()
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "UserID", UNDEFINED_USER_ID))
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aID", cMap_aIDtoUID(dbReader("UID"))))
                'oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aValueType", getValueType(dbReader("UID")))) 'AS/6-26-15
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aValueType", cValueTypes(dbReader("UID")))) 'AS/6-26-15
                aValue = getAValueByData(dbReader("Data").ToString, IsCategorical, cMap_EnumGUIDtoCVID)
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aValue", aValue))

                If Not ItemExists(cMap_aObjectIDtoAID, dbReader("AID")) Then  'AS/om0006===
                    'cMap_aObjectIDtoAID.Add(aValue, dbReader("AID")) 'AS/om0014
                    If Len(dbReader("aObjectID").ToString) <> 36 Then 'AS/om0014===
                        newGUID = Guid.NewGuid().ToString
                        cMap_aObjectIDtoAID.Add(newGUID, dbReader("AID").ToString)
                    Else
                        cMap_aObjectIDtoAID.Add(dbReader("aObjectID"), dbReader("AID").ToString)
                    End If 'AS/om0014==
                End If 'AS/om0006==
                'oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aObjectID", Guid.NewGuid().ToString)) 'AS/om0006
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aObjectID", getaObjectIDByAID(dbReader("AID").ToString, cMap_aObjectIDtoAID))) 'AS/om0006
                oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "aObjectID_String", dbReader("AID")))
                affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
            End While
            dbReader.Close()
        End If 'AS/om0007==

        Try 'AS/om0015===
            oCommand.CommandText = "INSERT INTO MProperties (PropertyName, PValue) VALUES (?, ?)"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "PropertyName", "ECC_AHPversion"))
            ' oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "PValue", "3.18")) 'AS/6-10-16
            oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "PValue", AHP_DB_LATEST_VERSION)) 'AS/6-10-16
            affected = DBExecuteNonQuery(m_ProviderType, oCommand)
        Catch ex As Exception
            'do nothing
        End Try

        '===== TRANSLATE to version 3.24 'AS/6-10-16===
        'Remove costs for participants from AltsData table
        sSQL = "DELETE Data FROM AltsData WHERE WRT = 'N0' AND PID > 1"
        oCommand.CommandText = sSQL
        affected = DBExecuteNonQuery(m_ProviderType, oCommand)

        'If already combined and Facilitator has costs differ from combined, replace the facilitator value with the combined
        sSQL = "SELECT DATA, AID FROM AltsData WHERE PID=1 and WRT = 'N0'"
        oCommandFrom.CommandText = sSQL
        dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

        While dbReader.Read
            sSQL = "UPDATE AltsData SET DATA = '" & CStr(dbReader("Data")) & "' WHERE AID = '" & CStr(dbReader("AID")) & "' AND PID=0 AND WRT='N0'"
            oCommandTo.CommandText = sSQL
            affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
        End While
        dbReader.Close()

        'make sure all facilitator costs are copied to combined (may be not if the costs were not combined yet)
        sSQL = "SELECT DATA, AID FROM AltsData WHERE PID=0 and WRT = 'N0'"
        oCommand.CommandText = sSQL
        dbReader = DBExecuteReader(m_ProviderType, oCommand)

        Dim AD As RAAlternative
        Dim cAD As New Collection

        While dbReader.Read
            AD = New RAAlternative
            AD.ID = dbReader("AID").ToString
            If Not String2Double(CStr(dbReader("Data")), AD.Cost) Then AD.Cost = UNDEFINED_INTEGER_VALUE ' D3644

            cAD.Add(AD)
        End While
        dbReader.Close()

        If cAD.Count > 0 Then
            sSQL = "SELECT * FROM AltsData WHERE PID=0"
            oCommand.CommandText = sSQL
            dbReader = DBExecuteReader(m_ProviderType, oCommand)

            For Each AD In cAD
                Try
                    oCommandTo.CommandText = "INSERT INTO AltsData (PID, AID, WRT, Data) VALUES (?, ?,?,?)"
                    oCommandTo.Parameters.Clear()
                    oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "PID", 1))
                    oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "AID", AD.ID))
                    oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "WRT", "N0"))
                    oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "Data", AD.Cost))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                Catch ex As Exception
                    'do nothing
                End Try
            Next
        End If
        dbReader.Close()

        'Copy the combined costs (actually, now these are the only ones which exist) from AltsData to RAbenefits base scenario (PSID = 0)
        If TableExists("RAbenefits") Then
            sSQL = "SELECT DATA, AID FROM AltsData WHERE PID=1 and WRT = 'N0'"
            oCommandFrom.CommandText = sSQL
            dbReader = DBExecuteReader(m_ProviderType, oCommandFrom)

            If dbReader.HasRows Then
                While dbReader.Read
                    Try 'add new row to RAbenefits. If it already exists, exeption occurrs
                        oCommandTo.CommandText = "INSERT INTO RAbenefits (PSID, AID, Benefit, Costs) VALUES (?, ?,?,?)"
                        oCommandTo.Parameters.Clear()
                        oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "PSID", 0))
                        oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "AID", dbReader("AID")))
                        oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "Benefit", 0))
                        oCommandTo.Parameters.Add(GetDBParameter(m_ProviderType, "Costs", dbReader("Data")))
                        affected = DBExecuteNonQuery(m_ProviderType, oCommandTo)
                    Catch ex As Exception 'if record exists, then update
                        sSQL = "UPDATE RAbenefits SET Costs = '" & dbReader("Data").ToString & "' WHERE AID = '" & dbReader("AID").ToString & "' AND PSID=0"
                        oCommand.CommandText = sSQL
                        affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                    End Try
                End While
            End If
            dbReader.Close()
        End If
        'AS/6-10-16==



        oCommand = Nothing 'AS/om0015==
        oCommandFrom = Nothing 'AS/6-26-15
        oCommandTo = Nothing 'AS/6-26-15

    End Sub

    Private Function getEnumGUIDbyCVID(cMap_EnumGUIDtoCVID As Collection, EnumGUID As String) As String 'wedX

        Dim RV As String
        If ItemExists(cMap_EnumGUIDtoCVID, EnumGUID) Then
            RV = cMap_EnumGUIDtoCVID(EnumGUID).ToString
        Else
            RV = Guid.NewGuid().ToString
        End If

        Return RV
    End Function


    Private Function getaObjectIDByAID(AID As String, cMap_aObjectIDtoAID As Collection) As String 'AS/om0006

        Dim RV As String

        'If Not ItemExists(cMap_aObjectIDtoAID, AID) Then 'AS/om0007
        If ItemExists(cMap_aObjectIDtoAID, AID) Then 'AS/om0007
            RV = cMap_aObjectIDtoAID(AID).ToString
        Else
            RV = Guid.NewGuid().ToString
        End If
        Return RV

    End Function


    Private Function getCategoryGUIDbyUID(UID As String, cMap_aIDtoUID As Collection) As String 'wedX

        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)
        oCommand.Connection = m_cnn

        Dim dbReaderRV As DbDataReader

        oCommand.CommandText = "SELECT CategoryGuidID FROM ECCore_CategoryMappings WHERE AttributeGuidID = '" & cMap_aIDtoUID(UID).ToString & "'"
        dbReaderRV = DBExecuteReader(m_ProviderType, oCommand)
        Dim RV As String
        dbReaderRV.Read()

        If dbReaderRV.HasRows Then
            RV = dbReaderRV("CategoryGuidID").ToString
        Else
            RV = ""
        End If

        dbReaderRV.Close()
        oCommand = Nothing 'AS/6-26-15
        Return RV

    End Function

    Private Function getValueType(UID As String) As Integer 'AS/om0010

        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)
        oCommand.Connection = m_cnn

        Dim dbReaderRV As DbDataReader

        oCommand.CommandText = "SELECT Categorical, UDColDataType FROM UDColsDefs WHERE UID = '" & UID & "'"
        dbReaderRV = DBExecuteReader(m_ProviderType, oCommand)
        Dim RV As Integer
        dbReaderRV.Read()

        If CBool(dbReaderRV("Categorical")) Then
            RV = 4
            dbReaderRV.Close()
            oCommand = Nothing 'AS/6-26-15
            Return RV
        End If

        Select Case CInt(dbReaderRV("UDColDataType"))
            Case 8 'String
                RV = 0 'aRVString
            Case 11 'Boolean
                RV = 1 'aRVBoolean
            Case 3 'Long
                RV = 2 'aRVLong
            Case 5 'Double
                RV = 3 'aRVDouble
            Case 0  'string
                RV = 0 'string 
            Case Else
                RV = 0
        End Select
        dbReaderRV.Close()
        oCommand = Nothing 'AS/6-26-15
        Return RV


    End Function


    Private Function getValueTypesByUIDs() As Collection 'AS/6-26-15

        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)
        oCommand.Connection = m_cnn

        Dim dbReaderRV As DbDataReader
        'oCommand.CommandText = "SELECT Categorical, UDColDataType, UID FROM UDColsDefs"
        oCommand.CommandText = "SELECT * FROM UDColsDefs"
        dbReaderRV = DBExecuteReader(m_ProviderType, oCommand)

        Dim RV As New Collection

        While dbReaderRV.Read
            If CBool(dbReaderRV("Categorical")) Then
                RV.Add(4, dbReaderRV("UID").ToString)
            Else
                Select Case CInt(dbReaderRV("UDColDataType"))
                    Case 8 'String
                        RV.Add(0, dbReaderRV("UID").ToString) 'aRVString
                    Case 11 'Boolean
                        RV.Add(1, dbReaderRV("UID").ToString) 'aRVBoolean
                    Case 3 'Long
                        RV.Add(2, dbReaderRV("UID").ToString) 'aRVLong
                    Case 5 'Double
                        RV.Add(3, dbReaderRV("UID").ToString) 'aRVDouble
                    Case 0  'string
                        RV.Add(0, dbReaderRV("UID").ToString) 'string 
                    Case Else
                        RV.Add(0, dbReaderRV("UID").ToString)
                End Select
            End If
        End While
        dbReaderRV.Close()
        oCommand = Nothing

        Return RV

    End Function

    Private Function getAValueByData(sData As String, ByRef IsCategorical As Boolean, cMap_EnumGUIDtoCVID As Collection) As String 'AS/om0007 added ByRef
        'if categorical then aValue = GUID, if non-cat then  aValue = actual value in rUDColsData

        Dim RV As String
        IsCategorical = False

        If Left(sData, 2) = "CV" Then 'categorical
            IsCategorical = True
            If ItemExists(cMap_EnumGUIDtoCVID, sData) Then
                RV = cMap_EnumGUIDtoCVID(sData).ToString
            Else
                RV = Guid.NewGuid().ToString
            End If
        Else
            RV = sData
        End If

        Return RV
    End Function


    Public Function FieldExists(FieldName As String, TableName As String) As Boolean 'AS/om0003 

        Dim dbReader As DbDataReader
        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)

        oCommand.Connection = m_cnn
        oCommand.CommandText = "SELECT * FROM [" & TableName & "]"
        dbReader = DBExecuteReader(m_ProviderType, oCommand)

        For i As Integer = 0 To dbReader.FieldCount - 1
            If dbReader.GetName(i).ToLower = FieldName.ToLower Then
                dbReader.Close() 'AS/om0011
                oCommand = Nothing 'AS/6-26-15
                Return True
            End If
        Next i
        dbReader.Close() 'AS/om0011
        oCommand = Nothing 'AS/6-26-15
        Return False

    End Function

    Private Function TableExists(ByVal TableName As String, Optional ByVal ConnectionString As String = "") As Boolean 'AS/om0003

        If ConnectionString = "" Then
            ConnectionString = m_cnn.ConnectionString
        End If

        Dim schemaTable As System.Data.DataTable
        schemaTable = GetDBSchemaTables(m_ProviderType, ConnectionString)

        For i As Integer = 0 To schemaTable.Rows.Count - 1
            If schemaTable.Rows(i)!TABLE_NAME.ToString.ToLower = TableName.ToLower Then
                Return True
            End If
        Next

        Return False
    End Function


    Private Function CdbVersionToSng(inData As String) As Single 'wedX
        'converts database version to single
        'if cannot convert, returns 0

        On Error Resume Next
        'CdbVersionToSng = CSng(Left(inData, 1) & gDecimalSymbol & MID(inData, 3))
        CdbVersionToSng = CSng(Left(inData, 1) & "." & Mid(inData, 3))
        On Error GoTo 0

    End Function


    Private Function ItemExists(InCol As Collection, Item As Object) As Boolean
        'returns True if item exists in the collection

        Dim RV As Boolean
        If InCol Is Nothing Then
            Return False
            Exit Function
        End If

        On Error Resume Next
        Dim X As Object
        X = InCol(Item)
        RV = Not (Err.Number = 5 Or Err.Number = 9)
        On Error GoTo 0

        Return RV
    End Function

    Public Function TimePeriodsRemoved() As Boolean 'AS/4-29-16

        RemoveTimePeriodsInfo()

        If m_cnn.State = ConnectionState.Open Then m_cnn.Close()
        m_cnn = Nothing

        Return True

    End Function

    Private Sub RemoveTimePeriodsInfo()  'AS/4-29-16

        Using m_cnn
            Dim dbReader As DbDataReader
            Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)
            oCommand.Connection = m_cnn

            oCommand.CommandText = "SELECT * FROM AltsGlobal ORDER BY sOrder"
            dbReader = DBExecuteReader(m_ProviderType, oCommand)

            Dim periodFieldExists As Boolean = False
            Dim basePeriodAIDFieldExists As Boolean = False
            Dim arTimePeriods() As String
            ReDim arTimePeriods(0)
            Dim arTPBases() As String
            ReDim arTPBases(0)

            If Not dbReader Is Nothing Then
                While dbReader.Read
                    For j As Integer = 0 To dbReader.FieldCount - 1
                        If dbReader.GetName(j).ToLower = "period" Then
                            periodFieldExists = True
                        End If
                        If dbReader.GetName(j).ToLower = "baseperiodaid" Then
                            basePeriodAIDFieldExists = True
                        End If
                    Next
                    If periodFieldExists And basePeriodAIDFieldExists Then
                        If CInt(dbReader("Period")) > 0 Then
                            arTimePeriods(UBound(arTimePeriods)) = dbReader("AID").ToString
                            ReDim Preserve arTimePeriods(UBound(arTimePeriods) + 1)
                        End If
                        If Not IsDBNull(dbReader("BasePeriodAID")) Then 'AS/5-2-16 enclosed
                            If dbReader("BasePeriodAID").ToString <> "" Then
                                arTPBases(UBound(arTPBases)) = dbReader("AltName").ToString
                                ReDim Preserve arTPBases(UBound(arTPBases) + 1)
                            End If
                        End If 'AS/5-2-16
                    End If
                End While
            End If
            dbReader.Close()

            ' Delete alternatives which are time periods
            Dim affected As Integer
            Dim i As Long
            For i = 0 To UBound(arTimePeriods) - 1
                oCommand.CommandText = "DELETE FROM AltsGlobal WHERE AID =?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            Next i

            For i = 0 To UBound(arTimePeriods) - 1
                oCommand.CommandText = "DELETE FROM AltsData WHERE AID =?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            Next i

            For i = 0 To UBound(arTimePeriods) - 1
                oCommand.CommandText = "DELETE FROM AltsPrty WHERE AID =?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            Next i

            For i = 0 To UBound(arTimePeriods) - 1
                oCommand.CommandText = "DELETE FROM AltsValues WHERE AID =?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            Next i

            For i = 0 To UBound(arTimePeriods) - 1
                oCommand.CommandText = "DELETE FROM UDColsData  WHERE AID =?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                affected = DBExecuteNonQuery(m_ProviderType, oCommand)
            Next i

            If TableExists("AttributesValues") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM AttributesValues WHERE aObjectID_String =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "aObjectID_String", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("GroupMembers") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM GroupMembers WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("FundingPools") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM FundingPools WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("MustsMustNots") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM MustsMustNots WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("RABenefits") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM RABenefits WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("RAConstraints") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM RAConstraints WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            If TableExists("RARisks") Then
                For i = 0 To UBound(arTimePeriods) - 1
                    oCommand.CommandText = "DELETE FROM RARisks WHERE AID =?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, "AID", arTimePeriods(CInt(i))))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                Next i
            End If

            '  Rename base alternatives back to original name which was before converting to time periods -- remove ":P00"
            For i = 0 To UBound(arTPBases) - 1
                If Right(arTPBases(CInt(i)), 4).ToLower = ":p00" Then
                    oCommand.CommandText = "UPDATE AltsGlobal SET AltName= '" & Left(arTPBases(CInt(i)), Len(arTPBases(CInt(i))) - 4) & "' WHERE AltName ='" & arTPBases(CInt(i)) & "'"
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                End If
            Next i

            oCommand = Nothing
            dbReader = Nothing

        End Using
    End Sub
End Class

