Option Strict Off

Imports System.Data.SqlClient
Imports System.Diagnostics
Imports System.Data 'AD/12323
Imports GenericDBAccess 'AS/12323xt
Imports System.Data.Common 'AS/12323xt
Imports GenericDBAccess.ECGenericDatabaseAccess 'AS/12323xu
Imports Oracle.ManagedDataAccess.Client 'AS/12323xw
Imports Microsoft.Office.Interop.MSProject 'AS/15597b

Partial Class DataMappingPage
    Inherits clsComparionCorePage

    Private Const SESS_MAP_PRJID As String = "MapPrj"
    Private Const SESS_CONNECT_DB As String = "ConnectDB" 'AS/12323
    Private Const SESS_COLS_SELECTED As String = "ColsSelected" 'AS/15285f

    Private _SQL_IgnoreDBs As String() = {"master", "tempdb", "model", "msdb", "aspstate"}
    Private _SQL_IgnoreTables As String() = {"dtproperties"}
    Private Const MAPKEY_DELIMITER As String = "_&_" 'AS/14629d

    Private _OracleConnection As OracleConnection = Nothing 'AS/12323xw
    Private _mspAplication As Microsoft.Office.Interop.MSProject.Application 'AS/15597c

    'ExternalDB constants 'AS/15285===
    Private Const SESS_ExternalDB_CONN_STR As String = "ExternalDBConnStr"
    Private Const SESS_ExternalDB_TYPE As String = "ExternalDBType" 'AS/15285c
    Private Const SESS_ExternalDB_NAME As String = "ExternalDBName"
    Private Const SESS_ExternalDB_TBL As String = "ExternalDBTbl"
    Private Const SESS_ExternalDB_COL As String = "ExternalDBCol"
    Private Const SESS_ExternalDB_MAPKEY As String = "ExternalDBMapKey"
    Private Const SESS_ExternalDB_ATTR As String = "ExternalDBAttr"
    Private Const SESS_ExternalDB_MAPINFO As String = "ExternalDBMapInfo"
    Private Const SESS_ExternalDB_VALUES As String = "ExternalDBValues"
    Private Const SESS_ExternalECC_Covobj As String = "ExternalECCCovobj" 'AS/24231h
    Private Const SESS_ExternalECC_User As String = "ExternalECCUser" 'AS/24231h

    Private _ExternalDBConnection As clsDatabaseAdvanced = Nothing
    'Private externalDBtype As clsDataMapping.enumMappedDBType = -1 'AS/15285
    'AS/15285==



    'AS/12323xw=
    Public Enum importMapDataToUsers 'AS/12323zt
        usrAll = 0
        usrCurrent = 1
        usrSelected = 2
        usrCombined = 3
        usrPhantom = 4
    End Enum

    Public Enum importMapDataReplaceOption 'AS/12323zt
        ' optNewAlternative = 0 'AS/14506
        optReplaceExisting = 1
        optReplaceEmpty = 2
        optReplaceAlt = 3 'AS/14506b
    End Enum

    'Public Enum importMapDataToDG 'AS/12323s=== 'AS/15285q
    '    dgcolAlternatives = 0
    '    dgcolCosts = 1
    '    dgcolRisks = 2
    '    dgcolCustomAttributes = 3 'import data from the mapped DB field as custom attributes
    '    dgcolAltsNonPWJudgments = 4 'nonPW judgments - Ratings, UC, SF
    '    dgcolInfodocs = 5 'import data from the mapped DB field as infodocs
    'End Enum

    Public Enum DataInterchangeInclude 'AS/15116e
        diCurrentColumn = 0
        diSelectedColumns = 1
        diAllMappedColumns = 2
    End Enum


    Public Enum importECCInclude 'AS/12323e
        impAltsNames = 0 'names only
        impAltsDefAttributes = 1 'default attributes
        impAltsAllAttributes = 2 'default and custom attributes
        dgcolAltsNonPWJudgments = 3 'nonPW judgments - Ratings, UC, SF
        impAltsInfodocs = 4 'infodocs 
        impAltsEverything = 5 'all atributes and data

        impAltsNamesAndData = 6 'AS/12323i===
        impAltsNamesAndInfodocs = 7
        impAltsNamesAndDataAndInfodocs = 8

        impAltsDefAttributesAndData = 9
        impAltsDefAttributesAndInfodocs = 10
        impAltsDefAttributesAndDataAndInfodocs = 11

        impAltsAllAttributesAndData = 12
        impAltsAllAttributesAndInfodocs = 13
        impAltsAllAttributesAndDataAndInfodocs = 14 'AS/12323i==
    End Enum

    Public Enum importSubhierarchyOptions 'AS/4488a
        'impSubhierarchyStructure = 0 'copy only structure and assign default names such as New node 1, New node 2 ... 'AS/4488d=== commented out
        'impSubhierarchyNoJudjments = 1 'copy only nodes but not any data
        'impSubhierarchyWithJudgments = 2 'copy nodes with judjments
        'impSubhierarchyAndInfodocs = 3 'copy nodes and ther infodocs but not any data
        'impSubhierarchyWithJudgmentsAndInfodocs = 4 'copy everything 'AS/4488d==
        impSubhierarchyNoInfodocs = 0 'AS/4488d
        impSubhierarchyWithInfodocs = 1 'AS/4488d
    End Enum

    Public Enum importAltsDuplicates 'AS/12323e
        impDuplicateAddNewKeepData = 1 'add duplicate as new alt -- rename but keep all other attrributes/data
        impDuplicateAddNewNotKeepData = 4 'add duplicate as new alt -- don't keep any attrributes/data (to do that, just assign new Guid)
        impDuplicateSkip = 2 'skip duplicate
        impDuplicateReplace = 3 'overwrite duplicate alt in the current project
    End Enum

    Public impAltsDuplicates As importAltsDuplicates = importAltsDuplicates.impDuplicateAddNewKeepData
    Public impOverwriteScale As Boolean 'AS/12323e 
    Public impAltsinclude As importECCInclude = importECCInclude.impAltsNames 'AS/12323h===
    'Public importMapDataToDGCol As importMapDataToDG = importMapDataToDG.dgcolAlternatives 'AS/12323w sql import options 'AS/15285q
    Public importMapDataToUser As importMapDataToUsers = importMapDataToUsers.usrCurrent 'AS/12323zt
    Public importMapDataAndReplace As importMapDataReplaceOption = importMapDataReplaceOption.optReplaceExisting 'AS/12323zt 'AS/14506
    Public importCreateNewAlt As Boolean 'AS/14506
    Public importReplaceAlt As Boolean 'AS/14506b
    Public DataInterchangeIncludeColumns As DataInterchangeInclude = DataInterchangeInclude.diCurrentColumn ' = 0, 1 or 2 for current, selected and all columns 'AS/15116e 

    Private customAttributes As List(Of clsAttribute) = Nothing
    Private srcCovObj As clsNode = Nothing
    Private destCovObj As clsNode = Nothing 'AS/12323h==
    'Private externalData As Collection 'AS/12323zm later removed NBU
    Private exValuesToImport As List(Of clsDataMappingValue) 'AS/12323xg
    Private eccValuesToExport As List(Of clsDataMappingValue) 'AS/14629b
    Private eccColumnsSelectedForDI As List(Of String) 'contains columns' GUID's 'AS/15116e
    Private isImport As Boolean 'AS/24192

    Public Sub New()
        MyBase.New(_PGID_REPORT_DATA_MAPPING)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not isCallback AndAlso Not IsPostBack Then
            AlignHorizontalCenter = False
            AlignVerticalCenter = False
            pnlLoadingPanel.Caption = ResString("msgLoading")   ' D4305
            pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}<div id='divPleaseWait'></div>", ResString("lblPleaseWait"), ImagePath + "process.gif")  ' D4305
        End If

        'debug.print("Page_Load, dm = " & CheckVar("dm", Guid.Empty).ToString) 'AS/12323output
        'debug.print(CheckVar("aguid", Guid.Empty).ToString) 'AS/12323output
        'debug.print(CheckVar("oguid", Guid.Empty).ToString) 'AS/12323output

        If Not isCallback AndAlso Not IsPostBack Then 'AS/12323xq===

            'Dim existingDM As clsDataMapping = ExternalDB_GetDataMappingByGuid(CheckVar("dm", Guid.Empty)) ' D4512 'AS/21354h
            Dim mappedColGUID As String = Guid.Empty.ToString 'AS/21354h===
            mappedColGUID = CheckVar("aguid", "").Trim
            If mappedColGUID = "" Then mappedColGUID = CheckVar("oguid", "").Trim
            Dim existingDM As clsDataMapping = ExternalDB_GetDataMappingByGuid(New Guid(mappedColGUID)) 'AS/21354h==

            If existingDM Is Nothing Then 'AS/12323xz=== 'in case it is an attribute with no mapping -- check the Alternatives column if it is mapped and if yes, get the connection string for it
                existingDM = ExternalDB_GetDataMappingByMappedColGuid(clsProjectDataProvider.dgColAltName)
            End If 'AS/22603 XXX moved up

            If existingDM IsNot Nothing Then
                ExternalDB_ConnectionString = existingDM.externalDBconnString
                ExternalDB_Type = existingDM.externalDBType 'AS/15285p
                ExternalDB_Name = existingDM.externalDBname 'AS/24189d
            Else
                ExternalDB_ConnectionString = ""
                'ecc_ConnectionString = "" 
            End If
            'End If 'AS/22603 XXX moved up

            'set existingDM back to the selected column
            'existingDM = ExternalDB_GetDataMappingByGuid(CheckVar("dm", Guid.Empty)) 'AS/12323xz==
            existingDM = ExternalDB_GetDataMappingByGuid(New Guid(mappedColGUID)) 'AS/21354h

            If existingDM IsNot Nothing Then 'AS/12323xq enclosed
                ExternalDB_ConnectionString = existingDM.externalDBconnString
                ExternalDB_Name = existingDM.externalDBname
                ExternalDB_Table = existingDM.externalTblName
                ExternalDB_Column = existingDM.externalColName
                ExternalDB_MapKey = existingDM.externalMapkeyColName
                ExternalDB_Type = existingDM.externalDBType 'AS/15285m 'AS/15285p fixed the name
            Else
                'ExternalDB_ConnectionString = "" 
                ExternalDB_Table = ""
                ExternalDB_Column = ""
                ExternalDB_MapKey = ""
            End If

        End If 'AS/12323xq==

        If CheckVar("ajax", False) Then onAjax()
    End Sub

    ' D4584 ===
    Private ReadOnly Property SESS_ProjectID_Postfix As String
        Get
            Return String.Format("_{0}", App.ProjectID)
        End Get
    End Property
    ' D4584 ==

    Public ReadOnly Property SourceModelID As Integer
        Get
            If Session(SESS_MAP_PRJID + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CInt(Session(SESS_MAP_PRJID + SESS_ProjectID_Postfix))
            End If
            Return -1
        End Get
    End Property

    Public ReadOnly Property ConnectedDB_ID As Integer 'AS/12323k
        Get
            If Session(SESS_CONNECT_DB + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CInt(Session(SESS_CONNECT_DB + SESS_ProjectID_Postfix))
            End If
            Return -1
        End Get
    End Property

    Private ReadOnly Property SourceModel As clsProject
        Get
            Return clsProject.ProjectByID(SourceModelID, App.ActiveProjectsList)
        End Get
    End Property

    Private ReadOnly Property CurrentProject As clsProject 'AS/12323d
        Get
            Return App.ActiveProject
        End Get
    End Property

    Private ReadOnly Property ProjManager As clsProjectManager 'AS/12323xe
        Get
            Return CurrentProject.ProjectManager
        End Get
    End Property

    Private Function doConnectEccModel(tPrjID As Integer) As Boolean
        Dim tPrj As clsProject = clsProject.ProjectByID(tPrjID, App.ActiveProjectsList)
        If tPrj IsNot Nothing AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then
            Session(SESS_MAP_PRJID + SESS_ProjectID_Postfix) = tPrjID
            Return True
        End If
        Return False
    End Function

    Private Function doConnectDB(tDBID As Integer) As Boolean 'AS/12323k
        Dim tPrj As clsProject = clsProject.ProjectByID(tDBID, App.ActiveProjectsList)
        If tPrj IsNot Nothing AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then
            Session(SESS_CONNECT_DB + SESS_ProjectID_Postfix) = tDBID
            Return True
        End If
        Return False
    End Function

    Public Function GetUsers(PM As clsProjectManager) As List(Of clsUser) 'AS/12323zs
        Dim tLst As New List(Of clsUser)
        If PM IsNot Nothing Then
            For Each tUser As clsUser In PM.UsersList
                tLst.Add(tUser)
            Next
        End If
        Return tLst
    End Function

    Public Function GetUsersListJSON(tList As List(Of clsUser)) As String 'AS/24231j
        Dim sLst As String = ""
        If tList IsNot Nothing Then
            For Each tUser As clsUser In tList
                sLst += String.Format("{2}['{0}','{1}']", JS_SafeString(tUser.UserID), JS_SafeString(tUser.UserName), IIf(sLst = "", "", ","))
            Next
        End If
        Return "[" + sLst + "]"
    End Function


    Public Function GetUsersListSelect(tList As List(Of clsUser)) As String 'AS/12323zs
        Dim sLst As String = ""
        If tList IsNot Nothing Then
            For Each tAttr As clsUser In tList
                sLst += String.Format("<option value='{0}'>{1}</option>", tAttr.UserName, SafeFormString(tAttr.UserName))
            Next
        End If
        Return sLst
    End Function

    Public Function GetMappedAttributesAndNodes(PM As clsProjectManager) As List(Of Object) 'AS/12323t
        Dim tLst As New List(Of Object) 'wed replaced List(Of clsAttribute) with List(Of Object)
        If PM IsNot Nothing Then
            For Each tAttr As clsAttribute In PM.Attributes.AttributesList
                If tAttr.DataMappingGUID <> Guid.Empty Then
                    tLst.Add(tAttr) 'AS/15624m
                End If
            Next

            For Each tNode As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                If tNode.DataMappingGUID <> Guid.Empty Then
                    tLst.Add(tNode) 'AS/15624m
                End If
            Next

        End If
        Return tLst

    End Function

    Public Function GetMappedColumnsList(tList As List(Of Object)) As String 'AS/12323t 'AS/15624m replaced with List of object
        Dim sLst As String = ""
        If tList IsNot Nothing Then
            'add Alternatives column to the dropdown 'AS/15285h===
            Dim newDM As clsDataMapping = GetDataMappingByCol(clsProjectDataProvider.dgColAltName.ToString)
            If newDM IsNot Nothing Then
                If newDM.DataMappingGUID <> Guid.Empty Then
                    sLst += String.Format("<option value='{0}'>{1}</option>", newDM.eccMappedColID, SafeFormString("Alternatives List"))
                End If
            End If 'AS/15285h==

            For Each tMappedCol As Object In tList 'AS/15624m
                'If TypeOf (tMappedCol) Is clsAttribute Then 'AS/24178===
                '    'sLst += String.Format("<option value='{0}'>{1}</option>", tAttr.Name, SafeFormString(tAttr.Name)) 'AS/12323zp replaced the first arg tAttr.ID with tAttr.Name
                '    sLst += String.Format("<option value='{0}'>{1}</option>", tMappedCol.ID, SafeFormString(tMappedCol.Name)) 'AS/12323zp replaced the first arg tAttr.Name with tAttr.ID
                'ElseIf TypeOf (tMappedCol) Is clsNode Then
                '    sLst += String.Format("<option value='{0}'>{1}</option>", tMappedCol.nodeguidid, SafeFormString(tMappedCol.nodename))
                'End If 'AS/24178==
                If TypeOf (tMappedCol) Is clsAttribute Then 'AS/24178===
                    Dim sName As String = tMappedCol.Name
                    If sName = ATTRIBUTE_RISK_NAME Then sName = "P.Failure"
                    sLst += String.Format("<option value='{0}'>{1}</option>", tMappedCol.ID, SafeFormString(sName))
                ElseIf TypeOf (tMappedCol) Is clsNode Then
                    If isImport Then 'AS/24192 enclosed and added Else part
                        If ProjectManager.UserID > -1 Then 'AS/24192
                            sLst += String.Format("<option value='{0}'>{1}</option>", tMappedCol.nodeguidid, SafeFormString(tMappedCol.nodename))
                        End If
                    Else 'AS/24192===
                        sLst += String.Format("<option value='{0}'>{1}</option>", tMappedCol.nodeguidid, SafeFormString(tMappedCol.nodename))
                    End If
                End If 'AS/24192==
            Next
        End If

        Return sLst

    End Function

    ' D4130 ===
    Public Function GetNonPWCovbj(PM As clsProjectManager) As List(Of clsNode)
        Dim tLst As New List(Of clsNode)
        If PM IsNot Nothing Then
            For Each tNode As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                Select Case tNode.MeasureType
                    Case ECMeasureType.mtPairwise
                    Case Else
                        tLst.Add(tNode)
                End Select
            Next
        End If
        Return tLst
    End Function

    Public Function GetNodesListSelect(tList As List(Of clsNode)) As String
        Dim sLst As String = ""
        If tList IsNot Nothing Then
            For Each tNode As clsNode In tList
                sLst += String.Format("<option value='{0}'>{1}</option>", tNode.NodeGuidID, SafeFormString(tNode.NodeName)) 'AS/12323zv
                'sLst += String.Format("<option value='{0}'>{1}</option>", tNode.NodeName, SafeFormString(tNode.NodeName)) 'AS/12323zu
            Next
        End If
        Return sLst
    End Function

    Private Function getMappedDBName() As String 'AS/24189e
        Dim sName As String = ""
        Select Case ExternalDB_Type'AS/15285 
            Case clsDataMapping.enumMappedDBType.mdtAccess
                If ExternalDB_Connection() IsNot Nothing Then
                    sName = IO.Path.GetFileNameWithoutExtension(ExternalDB_Connection.ConnectionString)
                End If
            Case clsDataMapping.enumMappedDBType.mdtSQL
                If ExternalDB_Connection() IsNot Nothing Then
                    'sName = ExternalDB_Connection.Database
                    Dim DbConnection As New SqlConnection(ExternalDB_Connection.ConnectionString)
                    sName = DbConnection.Database
                End If
            Case clsDataMapping.enumMappedDBType.mdtMSProject
                If ExternalDB_ConnectionString <> "" Then
                    sName = IO.Path.GetFileNameWithoutExtension(ExternalDB_ConnectionString)
                End If
            Case clsDataMapping.enumMappedDBType.mdtECC
                If SourceModel IsNot Nothing Then sName = SourceModel.ProjectName
        End Select
        Return sName

    End Function

    Public Function GetDBtypesList() As String 'AS/12323s
        Dim sLst As String = ""

        Dim sDBtypes As New List(Of clsDataMapping.enumMappedDBType)
        Dim sActive As clsDataMapping.enumMappedDBType = clsDataMapping.enumMappedDBType.mdtECC
        If App.Options.ProjectUseDataMapping Then
            sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtSQL)
            sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtAccess)
            'sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtOracle) 'AS/24200
            sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtMSProject) 'AS/15597 'AS/15624r 'AS/15597e
            'sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtMSProjectServer) 'AS/15597 'AS/15624r 
            sActive = clsDataMapping.enumMappedDBType.mdtSQL 'AS/12323xz
        End If
        sDBtypes.Add(clsDataMapping.enumMappedDBType.mdtECC) 'AS/15624r 'AS/17262

        'debug.print("GetDBtypesList, dm = " & CheckVar("dm", Guid.Empty).ToString) 'AS/12323output
        'Dim existingDM As clsDataMapping = ExternalDB_GetDataMappingByGuid(CheckVar("dm", Guid.Empty)) 'AS/21354i
        Dim mappedColGUID As String = Guid.Empty.ToString 'AS/21354i===
        mappedColGUID = CheckVar("aguid", "").Trim
        If mappedColGUID = "" Then mappedColGUID = CheckVar("oguid", "").Trim 'AS/21354k
        Dim existingDM As clsDataMapping = ExternalDB_GetDataMappingByGuid(New Guid(mappedColGUID)) 'AS/21354i==

        If existingDM Is Nothing Then 'AS/12323xz=== check if it is an attribute with no mapping yet but Alternatives already mapped
            existingDM = ExternalDB_GetDataMappingByMappedColGuid(clsProjectDataProvider.dgColAltName)
        End If 'AS/12323xz==

        If existingDM IsNot Nothing Then sActive = existingDM.externalDBType
        For Each sType As clsDataMapping.enumMappedDBType In sDBtypes
            sLst += String.Format("<option value='{0}'{2}>{1}</option>", CInt(sType), SafeFormString(ResString(String.Format("lbl_{0}", sType))), If(sType = sActive, " selected", ""))
        Next
        Return sLst
    End Function

    Public Function GetProjectsList() As String
        Dim sList As String = ""
        For Each tPrj As clsProject In App.ActiveProjectsList
            If tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then
                sList += String.Format("{0}<option value='{1}'{3}>{2}</option>", IIf(sList = "", "", vbNewLine), tPrj.ID, SafeFormString(ShortString(tPrj.ProjectName, 85, True)), IIf(SourceModelID = tPrj.ID, " selected", ""))
            End If
        Next
        Return sList
    End Function

    Public Function GetAttributesListJSON(tList As List(Of clsAttribute)) As String 'AS/24231c

        Dim sLst As String = ""
        If tList IsNot Nothing Then
            For Each tAttr As clsAttribute In tList
                sLst += String.Format("{2}['{0}','{1}']", JS_SafeString(tAttr.ID.ToString), JS_SafeString(tAttr.Name), IIf(sLst = "", "", ","))
            Next
        End If
        Return "[" + sLst + "]"
    End Function

    Public Function GetNodesListJSON(tList As List(Of clsNode)) As String
        Dim sLst As String = ""
        If tList IsNot Nothing Then
            For Each tNode As clsNode In tList
                sLst += String.Format("{2}['{0}','{1}']", JS_SafeString(tNode.NodeGuidID.ToString), JS_SafeString(tNode.NodeName), IIf(sLst = "", "", ","))
            Next
        End If
        Return "[" + sLst + "]"
    End Function

    Public Property ExternalDB_ConnectionString() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_CONN_STR + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_CONN_STR + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get
        Set(value As String)
            Session(SESS_ExternalDB_CONN_STR + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_Type() As clsDataMapping.enumMappedDBType 'AS/15285c
        Get
            If Session(SESS_ExternalDB_TYPE + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_TYPE + SESS_ProjectID_Postfix))
            Else
                'Return -1 'AS/15285m 'AS/15285o put back
                Return clsDataMapping.enumMappedDBType.mdtSQL 'AS/15285m made SQL as default
            End If
        End Get
        Set(value As clsDataMapping.enumMappedDBType)
            Session(SESS_ExternalDB_TYPE + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_Name() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_NAME + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_NAME + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get
        Set(value As String)
            Session(SESS_ExternalDB_NAME + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_Table() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_TBL + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_TBL + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get

        Set(value As String)
            Session(SESS_ExternalDB_TBL + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_Column() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_COL + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_COL + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get
        Set(value As String)
            Session(SESS_ExternalDB_COL + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_MapKey() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_MAPKEY + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_ExternalDB_MAPKEY + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get
        Set(value As String)
            Session(SESS_ExternalDB_MAPKEY + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_MapInfo() As List(Of clsDataMapping) 'AS/15285
        Get
            If Session(SESS_ExternalDB_MAPINFO + SESS_ProjectID_Postfix) IsNot Nothing Then Return Session(SESS_ExternalDB_MAPINFO + SESS_ProjectID_Postfix) Else Return Nothing
        End Get
        Set(value As List(Of clsDataMapping))
            Session(SESS_ExternalDB_MAPINFO + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_MapValues() As List(Of clsDataMappingValue) 'AS/15285

        Get
            If Session(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) IsNot Nothing Then Return Session(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) Else Return Nothing
        End Get
        Set(value As List(Of clsDataMappingValue))
            Session(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_Attribute() As String 'AS/15285
        Get
            If Session(SESS_ExternalDB_ATTR + SESS_ProjectID_Postfix) IsNot Nothing Then Return CStr(Session(SESS_ExternalDB_ATTR + SESS_ProjectID_Postfix)) Else Return ""
        End Get
        Set(value As String)
            Session(SESS_ExternalDB_ATTR + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalECC_Covobj() As String 'AS/24231h
        Get
            If Session(SESS_ExternalECC_Covobj + SESS_ProjectID_Postfix) IsNot Nothing Then Return CStr(Session(SESS_ExternalECC_Covobj + SESS_ProjectID_Postfix)) Else Return ""
        End Get
        Set(value As String)
            Session(SESS_ExternalECC_Covobj + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalECC_User() As String 'AS/24231h
        Get
            If Session(SESS_ExternalECC_User + SESS_ProjectID_Postfix) IsNot Nothing Then Return CStr(Session(SESS_ExternalECC_User + SESS_ProjectID_Postfix)) Else Return ""
        End Get
        Set(value As String)
            Session(SESS_ExternalECC_User + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    Public Property ExternalDB_MappedValuesImported() As List(Of clsDataMappingValue) 'AS/15285
        Get
            If GetCookie(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) IsNot Nothing Then Return Session(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) Else Return Nothing
        End Get
        Set(value As List(Of clsDataMappingValue))
            SetCookie(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix, value.ToString)
        End Set

    End Property

    Public Property ExternalDB_MappedValuesExported() As List(Of clsDataMappingValue) 'AS/15285
        Get
            If GetCookie(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) IsNot Nothing Then Return Session(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix) Else Return Nothing
        End Get
        Set(value As List(Of clsDataMappingValue))
            SetCookie(SESS_ExternalDB_VALUES + SESS_ProjectID_Postfix, value.ToString)
        End Set

    End Property


    Public Property ExternalDB_MappedColumnsExported() As String 'AS/15285f
        Get
            If Session(SESS_COLS_SELECTED + SESS_ProjectID_Postfix) IsNot Nothing Then
                Return CStr(Session(SESS_COLS_SELECTED + SESS_ProjectID_Postfix))
            Else
                Return ""
            End If
        End Get

        Set(value As String)
            Session(SESS_COLS_SELECTED + SESS_ProjectID_Postfix) = value
        End Set
    End Property

    'Private Function ImportAlternativeName(destAltsHierarchy As clsHierarchy, alt As clsNode) As Boolean 'AS/12323d 'AS/24195 NBU, commented out

    '    'calculate max nodeID 
    '    Dim max As Integer = 0
    '    For Each a As clsNode In destAltsHierarchy.Nodes
    '        If max < a.NodeID Then
    '            max = a.NodeID
    '        End If
    '    Next

    '    Try
    '        Dim dupAlt As clsNode = destAltsHierarchy.GetNodeByID(alt.NodeGuidID) 'check if the alt is a duplicate or not 'AS/12323f
    '        If dupAlt Is Nothing Then 'AS/24189b===
    '            dupAlt = destAltsHierarchy.GetNodeByID(alt.NodeID)
    '            If dupAlt Is Nothing Then 'alt to add doesn't exist yet
    '                destAltsHierarchy.AddNode(alt, -1)
    '                'debug.print(alt.NodeName & "  " & alt.NodeID.ToString & "  " & alt.NodeGuidID.ToString)
    '                'debug.print(destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeName)
    '                destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeMappedID = alt.NodeID 'keep the original nodeID as nodeMappedID
    '                destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeID = max + 1 ' continue enumeration for the imported alts to avoid duplicates in the UniqueID column
    '                max = max + 1
    '                Return True
    '            Else
    '                alt.NodeID = max
    '                max = max + 1
    '                Return True
    '            End If
    '        End If 'AS/24189b==

    '        If dupAlt Is Nothing Then 'alt to add doesn't exist yet
    '            destAltsHierarchy.AddNode(alt, -1)
    '            '''debug.print(alt.NodeName & "  " & alt.NodeID.ToString & "  " & alt.NodeGuidID.ToString)
    '            '''debug.print(destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeName)
    '            destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeMappedID = alt.NodeID 'keep the original nodeID as nodeMappedID
    '            destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeID = max + 1 ' continue enumeration for the imported alts to avoid duplicates in the UniqueID column
    '            max = max + 1
    '            Return True
    '        Else 'AS/12323f
    '            'promt user and set importAltsDuplicates
    '            Select Case impAltsDuplicates
    '                Case importAltsDuplicates.impDuplicateAddNewKeepData
    '                    'replace Guid with the new one but keep name and all other attrributes/data according to the options
    '                    alt.NodeName = "New Alternative (former " & alt.NodeName & ", keeping data)"
    '                    destAltsHierarchy.AddNode(alt, -1)
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeMappedID = alt.NodeID 'keep the original nodeID as nodeMappedID
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeID = max + 1 ' continue enumeration for the imported alts to avoid duplicates in the UniqueID column
    '                    max = max + 1

    '                Case importAltsDuplicates.impDuplicateAddNewNotKeepData
    '                    'replace Guid with the new one
    '                    alt.NodeName = "New Alternative (former " & alt.NodeName & ", Not keeping data)" 'AS/12323f==
    '                    alt.NodeGuidID = Guid.NewGuid
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeMappedID = alt.NodeID 'keep the original nodeID as nodeMappedID
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeID = max + 1 ' continue enumeration for the imported alts to avoid duplicates in the UniqueID column
    '                    max = max + 1

    '                Case importAltsDuplicates.impDuplicateReplace
    '                    destAltsHierarchy.DeleteNode(dupAlt)
    '                    destAltsHierarchy.AddNode(alt, -1)
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeMappedID = alt.NodeID 'keep the original nodeID as nodeMappedID
    '                    destAltsHierarchy.Nodes(destAltsHierarchy.Nodes.Count - 1).NodeID = max + 1 ' continue enumeration for the imported alts to avoid duplicates in the UniqueID column
    '                    max = max + 1

    '                Case importAltsDuplicates.impDuplicateSkip
    '                    'do nothing
    '            End Select
    '            Return True
    '        End If
    '    Catch ex As System.Exception
    '        ''debug.print(ex.Message)
    '    End Try
    '    Return False
    'End Function

    Private Function ImportAlternativeDefaultAttributes(altGUID As Guid) As Boolean 'AS/12323d
        Try
            'Import and attach default alternative's attribites -- Cost and Risk
            Dim cost As Double = SourceModel.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_COST_ID, altGUID)
            Dim risk As Double = SourceModel.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_ID, altGUID)
            '''debug.print(alt.NodeName & "  " & cost.ToString)

            If cost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                ProjManager.Attributes.SetAttributeValue(ATTRIBUTE_COST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, cost, altGUID, Guid.Empty)
                '''debug.print(alt.NodeName & "  " & cost.ToString)
            End If

            If risk <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                ProjManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, risk, altGUID, Guid.Empty)
                '''debug.print(alt.NodeName & "  " & risk.ToString)
            End If
            Return True
        Catch ex As System.Exception
            ''debug.print(ex.Message)
            Return False
        End Try

    End Function

    Private Function ImportAlternativeCustomAttributes(customAttributes As List(Of clsAttribute)) As Boolean 'AS/12323d
        Try
            'Import and attach custom alternative's attribites
            Dim fHasAttribs As Boolean = False

            For Each attr As clsAttribute In customAttributes
                ' copy all Alternative attributes
                If attr.Type = AttributeTypes.atAlternative Then 'AS/12323c=== mimiced from Default.aspx.vb, Private Function CreateAssociatedModel(sName As String, tSrcPrjID As Integer) As clsProject
                    fHasAttribs = True
                    ' copy categories if exists
                    If Not Guid.Empty.Equals(attr.EnumID) Then
                        Dim tEnum As clsAttributeEnumeration = SourceModel.ProjectManager.Attributes.GetEnumByID(attr.EnumID)
                        If tEnum IsNot Nothing Then
                            Dim tNewEnum As New clsAttributeEnumeration()
                            tNewEnum.ID = tEnum.ID
                            tNewEnum.Name = tEnum.Name
                            For Each tEI As clsAttributeEnumerationItem In tEnum.Items
                                Dim tnewEI As New clsAttributeEnumerationItem()
                                tnewEI.ID = tEI.ID
                                tnewEI.Value = tEI.Value
                                tNewEnum.Items.Add(tnewEI)
                            Next
                            ProjManager.Attributes.Enumerations.Add(tNewEnum)
                        End If
                    End If
                    ' create new attrib
                    Dim tNewAttr As clsAttribute = ProjManager.Attributes.AddAttribute(attr.ID, attr.Name, attr.Type, attr.ValueType, attr.DefaultValue, attr.IsDefault, attr.EnumID)
                End If

                ' copy attributes' values 
                If SourceModel IsNot Nothing Then 'AS/12323p enclosed
                    For Each tVal As clsAttributeValue In SourceModel.ProjectManager.Attributes.ValuesList
                        If tVal.AttributeID.Equals(attr.ID) Then
                            ProjManager.Attributes.SetAttributeValue(tVal.AttributeID, tVal.UserID, tVal.ValueType, tVal.Value, tVal.ObjectID, tVal.AdditionalID)
                        End If
                    Next 'AS/12323c==
                End If 'AS/12323p
            Next

            If fHasAttribs Then
                With ProjManager
                    .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                End With
            End If
            Return True
        Catch ex As System.Exception
            ''debug.print(ex.Message)
            Return False
        End Try
    End Function

    Private Function ImportAlternativeData(srcCovObj As clsNode, destCovObj As clsNode, alt As clsNode, ImportUCandSFIfNotTheSameScale As Boolean) As Boolean 'AS/12323d

        SourceModel.ProjectManager.StorageManager.Reader.LoadUserJudgments()
        ProjManager.StorageManager.Reader.LoadUserJudgments()

        If Not IsPWMeasurementType(srcCovObj.MeasureType) AndAlso (srcCovObj.MeasureType = destCovObj.MeasureType) Then
            If srcCovObj.MeasureType = ECMeasureType.mtDirect OrElse ImportUCandSFIfNotTheSameScale AndAlso srcCovObj.MeasureType <> ECMeasureType.mtRatings OrElse srcCovObj.MeasurementScaleID.Equals(destCovObj.MeasurementScaleID) Then
                For Each jSource As clsNonPairwiseMeasureData In srcCovObj.Judgments.JudgmentsFromAllUsers
                    Dim sourceUser As clsUser = SourceModel.ProjectManager.GetUserByID(jSource.UserID)
                    Dim destUser As clsUser = ProjManager.GetUserByEMail(sourceUser.UserEMail)
                    If destUser IsNot Nothing Then
                        Dim newJ As clsNonPairwiseMeasureData = Nothing

                        ' fill newJ with correct data, make sure to pass uDest.UserID as userid
                        Select Case srcCovObj.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                ''debug.print("Cannot import pairwise judgment")
                            Case ECMeasureType.mtRatings
                                Dim srcR As clsRatingMeasureData = CType(jSource, clsRatingMeasureData)
                                newJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                            Case ECMeasureType.mtStep
                                Dim srcSF As clsStepMeasureData = CType(jSource, clsStepMeasureData)
                                newJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                            Case ECMeasureType.mtDirect
                                Dim srcDirect As clsDirectMeasureData = CType(jSource, clsDirectMeasureData)
                                newJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                Dim srcUC As clsUtilityCurveMeasureData = CType(jSource, clsUtilityCurveMeasureData)
                                newJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                            Case Else
                                newJ = Nothing
                        End Select
                        If newJ IsNot Nothing Then
                            destCovObj.Judgments.AddMeasureData(newJ)
                        End If
                    End If
                Next
            End If
            ProjManager.StorageManager.Writer.SaveUserJudgments()
            Return True
        Else
            Return False
        End If

    End Function

    Private Function ImportAlternativeInfodoc(alt As clsNode) As Boolean 'AS/12323e
        Try
            Dim doc As String = SourceModel.ProjectManager.InfoDocs.GetNodeInfoDoc(alt.NodeGuidID)

            If doc IsNot Nothing AndAlso doc.Trim <> "" Then
                ''debug.print(alt.NodeName & "  " & doc)
                ProjManager.InfoDocs.SetNodeInfoDoc(alt.NodeGuidID, doc)
            End If
            Return True
        Catch ex As System.Exception
            ''debug.print(ex.Message)
            Return False
        End Try

        Return False
    End Function

    Private Function doImportSubHierarchy(srcNodeGuid As Guid, destParentNodeGuid As Guid, impSubhierarchyOptions As importSubhierarchyOptions) As Boolean 'AS/4488a incorporated the sub
        If SourceModel IsNot Nothing Then

            destParentNodeGuid = New Guid("983fe80c-8ec9-4136-b52e-4372fa82142c") 'Performance  in "Car Purchase - w_Rat"
            'srcNodeGuid = New Guid("f6012c65-9942-4e15-b4ec-8a0776193d6e") 'Financials (1 level, 2 children) in IT Portfolio
            'srcNodeGuid = New Guid("05ef2c0d-43f5-4d5b-8692-039fbcaa0491") 'Minimize Risks (2 levels, 2 children) in IT Portfolio
            'srcNodeGuid =  New Guid("258bf8c0-2352-4256-bc1c-ce1fa5eef53c") ' Ensure Readiness (2nd level, 2 children on the 3rd level) in IT Portfolio
            'srcNodeGuid =  New Guid("ca1248fd-fa0f-42b2-be2b-bfd489b0fc47") 'Internal Access in IT Portfolio - covering
            srcNodeGuid = New Guid("2b507653-e11c-4859-94bf-40686cbaf353") 'Leverage Knowledge   in IT Portfolio

            Dim srcNode As clsNode = Nothing
            Dim destParentNode As clsNode = Nothing
            Dim srcHierarchy As clsHierarchy = SourceModel.HierarchyObjectives
            Dim destHierarchy As clsHierarchy = CurrentProject.HierarchyObjectives

            If srcNodeGuid <> Guid.Empty And destParentNodeGuid <> Guid.Empty Then
                'If srcNodeGuid <> "" And destParentNodeGuid <> "" Then
                srcNode = srcHierarchy.GetNodeByID(srcNodeGuid)
                destParentNode = destHierarchy.GetNodeByID(destParentNodeGuid)
            Else
                Return False
            End If

            If srcNode Is Nothing Or destParentNode Is Nothing Then
                Return False
            End If

            'import
            Dim rv As Integer = UNDEFINED_INTEGER_VALUE

            'add head of sub-hierarchy
            'SourceModel.ProjectManager.DeleteJudgmentsForNode(srcNode.NodeGuidID) 'AS/4488d+- NEED?
            If Not importSubhierarchyOptions.impSubhierarchyWithInfodocs Then 'AS/4488d===
                srcNode.InfoDoc = ""
            End If 'AS/4488d==
            rv = destHierarchy.AddNode(srcNode, destParentNode.NodeID, True)

            'get and add descendants
            Dim srcDescendants As List(Of clsNode) = New List(Of clsNode)
            Dim srcPlex As List(Of clsNode) = Nothing
            srcPlex = srcNode.GetNodeDescendants(srcNode, srcDescendants)
            For Each node As clsNode In srcPlex
                'SourceModel.ProjectManager.DeleteJudgmentsForNode(Node.NodeGuidID) 'AS/4488d+- NEED?
                If impSubhierarchyOptions <> importSubhierarchyOptions.impSubhierarchyWithInfodocs Then 'AS/4488d===
                    node.InfoDoc = ""
                End If 'AS/4488d==
                rv = destHierarchy.AddNode(node, node.ParentNodeID, True)
            Next

            'save added data
            ProjManager.StorageManager.Writer.SaveProject(True)

            'reload the project
            ProjManager.StorageManager.Reader.LoadProject()
            Return True

        End If
        Return False
    End Function

    Private Function doImportJudgmentsMatrix(srcParentNodeGuid As Guid, destParentNodeGuid As Guid, srcUser As clsUser, destUsersList As List(Of clsUser), CopyMode As CopyJudgmentsMode) As Boolean 'AS/13258
        'MsgBox("Feature is on hold; priority 5") 'AS/17315
        'Return False 'AS/17315

        If SourceModel IsNot Nothing Then

            'AS/17315=== AS_hardcoded, to be replaced with real values when UI is ready
            destParentNodeGuid = New Guid("983fe80c-8ec9-4136-b52e-4372fa82142c") 'Performance  in "Car Purchase - w_Rat"
            'srcParentNodeGuid = New Guid("f6012c65-9942-4e15-b4ec-8a0776193d6e") 'Financials (1 level, 2 children) in IT Portfolio
            'srcParentNodeGuid = New Guid("05ef2c0d-43f5-4d5b-8692-039fbcaa0491") 'Minimize Risks (2 levels, 2 children) in IT Portfolio
            'srcParentNodeGuid =  New Guid("258bf8c0-2352-4256-bc1c-ce1fa5eef53c") ' Ensure Readiness (2nd level, 2 children on the 3rd level) in IT Portfolio
            'srcParentNodeGuid =  New Guid("ca1248fd-fa0f-42b2-be2b-bfd489b0fc47") 'Internal Access in IT Portfolio - covering
            srcParentNodeGuid = New Guid("2b507653-e11c-4859-94bf-40686cbaf353") 'Leverage Knowledge   in IT Portfolio

            srcUser = New clsUser()
            srcUser.UserID = 0 'Facilitator in IT Portfolio

            Dim destUsr As clsUser = ProjManager.GetUserByID(New Guid("eb373825-6dc2-464f-a379-fdbebaebad3e")) 'Administrator  In "Car Purchase - w_Rat"
            destUsersList = New List(Of clsUser)
            destUsersList.Add(destUsr)
            'AS/17315==

            Dim srcHierarchy As clsHierarchy = SourceModel.HierarchyObjectives
            Dim destHierarchy As clsHierarchy = CurrentProject.HierarchyObjectives
            Dim srcParentNode As clsNode = srcHierarchy.GetNodeByID(srcParentNodeGuid)
            Dim destParentNode As clsNode = destHierarchy.GetNodeByID(destParentNodeGuid)
            Dim srcMatrix As List(Of clsNode) = srcParentNode.GetNodesBelow(srcParentNode.NodeID) 'get nodes in the source matri
            Dim destMatrix As List(Of clsNode) = destParentNode.GetNodesBelow(destParentNode.NodeID) 'get nodes in the destination matrix

            Dim copySuccess As Boolean = True

            'AS/17315a===
            If srcParentNode Is Nothing Or destParentNode Is Nothing Then
                Return False
            ElseIf srcParentNode.Children.Count <> destParentNode.Children.Count Then
                Return False
            ElseIf srcParentNode.MeasureType <> destParentNode.MeasureType Then
                Return False
            ElseIf srcParentNode.MeasureType <> ECMeasureType.mtPairwise Then
                Return False
            End If
            'AS/17315a===

            'For n As Integer = 0 To srcMatrix.Count - 1
            SourceModel.ProjectManager.StorageManager.Reader.LoadUserJudgments(srcUser)
            Dim srcJudgments As List(Of clsCustomMeasureData) = srcParentNode.Judgments.JudgmentsFromUser(srcUser.UserID)

            'Dim destNode As clsNode = destMatrix(n)

            For Each destUser In destUsersList
                'If CopyMode = CopyJudgmentsMode.AddMissing Or CopyMode = CopyJudgmentsMode.UpdateAndAddMissing Then 'AS/17315a===
                '    ProjManager.StorageManager.Reader.LoadUserJudgments(destUser)
                'Else
                '    ProjManager.DeleteJudgmentsForNode(destNode.NodeGuidID, destUser.UserID)
                'End If
                'destNode.Judgments.Weights.ClearUserWeights(destUser.UserID) 'AS/17315a==

                ProjManager.StorageManager.Reader.LoadUserJudgments(destUser) 'AS/17315a
                destParentNode.Judgments.Weights.ClearUserWeights(destUser.UserID) 'AS/17315a

                Dim destJudgments As List(Of clsCustomMeasureData) = destParentNode.Judgments.JudgmentsFromUser(destUser.UserID)

                For i As Integer = 0 To srcJudgments.Count - 1
                    Dim doAdd As Boolean = True
                    Dim srcJ As clsCustomMeasureData = srcJudgments(i)
                    If Not srcJ.IsUndefined Then
                        Dim destJ As clsCustomMeasureData = Nothing
                        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)

                        destJ = destJudgments(i)
                        Dim destPW As clsPairwiseMeasureData = CType(destJ, clsPairwiseMeasureData)

                        If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(destParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID) IsNot Nothing Then
                            If CType(destParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID) Is Nothing Or CType(destParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID).Value = 0 Then 'AS/11725===
                                destJ = New clsPairwiseMeasureData(destPW.FirstNodeID, destPW.SecondNodeID, srcPW.Advantage, srcPW.Value, destPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                            Else
                                doAdd = False
                            End If 'AS/11725==
                        Else
                            destJ = New clsPairwiseMeasureData(destPW.FirstNodeID, destPW.SecondNodeID, srcPW.Advantage, srcPW.Value, destPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                        End If


                        If doAdd AndAlso destJ IsNot Nothing Then
                            destParentNode.Judgments.AddMeasureData(destJ)
                            copySuccess = True

                        End If
                    End If
                Next i

                'destNode.PWOutcomesJudgments.Weights.ClearUserWeights(destUser.UserID) 'AS/17315a===
                'Dim pwoJudgments As List(Of clsCustomMeasureData) = destNode.PWOutcomesJudgments.JudgmentsFromUser(srcUser.UserID)
                'For Each srcJ As clsCustomMeasureData In pwoJudgments
                '    If Not srcJ.IsUndefined Then
                '        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                '        If CopyMode <> CopyJudgmentsMode.AddMissing OrElse CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(destNode.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID, srcPW.ParentNodeID, srcPW.OutcomesNodeID) Is Nothing Then
                '            Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                '            destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                '            destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                '            destNode.PWOutcomesJudgments.AddMeasureData(destJ)
                '        End If
                '    End If
                'Next 'AS/17315a==

                ProjManager.StorageManager.Writer.SaveUserJudgments(destUser, Now)
                destUser.LastJudgmentTime = VERY_OLD_DATE

            Next
            If Not copySuccess Then
                Return False
            End If
            'Next

            'reload the project
            ProjManager.StorageManager.Reader.LoadProject()
            Return True

        End If
        Return False
    End Function

    ' D4305 ===

    Public Function ExternalDB_GetDataMappingByGuid(dmGUID As Guid) As clsDataMapping 'AS/12323xq
        Dim dm As clsDataMapping = Nothing
        For Each dm In ProjManager.DataMappings
            'If dm.DataMappingGUID = dmGUID Then 'AS/21354h
            If dm.eccMappedColID = dmGUID Then 'AS/21354h
                Return dm
            End If
        Next
        Return Nothing
    End Function

    Public Function ExternalDB_GetDataMappingByMappedColGuid(colGUID As Guid) As clsDataMapping 'AS/12323xz
        Dim dm As clsDataMapping = Nothing
        For Each dm In ProjManager.DataMappings
            If dm.eccMappedColID = colGUID Then
                Return dm
            End If
        Next
        Return Nothing
    End Function

    Private Function ExternalDB_Connection() As Object 'AS/15285 'AS/15285b redid entire sub 'AS/15285m

        Select Case ExternalDB_Type'AS/15285 
            Case clsDataMapping.enumMappedDBType.mdtAccess, clsDataMapping.enumMappedDBType.mdtSQL 'access, SQL
                If ExternalDB_Connection Is Nothing Then 'AS/15285c
                    Dim provider As DBProviderType = DBProviderType.dbptSQLClient

                    If ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtAccess Then
                        provider = DBProviderType.dbptOLEDB 'AS/15285c
                        ExternalDB_Name = "" 'AS/15285p
                    End If

                    _ExternalDBConnection = New clsDatabaseAdvanced("", provider)

                    _ExternalDBConnection.ConnectionString = ExternalDB_ConnectionString
                    If ExternalDB_Name <> "" Then _ExternalDBConnection.DatabaseName = ExternalDB_Name

                    Try
                        _ExternalDBConnection.Connect()
                    Catch ex As System.Exception
                        'debug.print(ex.Message)
                    End Try

                    Return _ExternalDBConnection
                End If

            Case 4 'Oracle
                If _OracleConnection Is Nothing Then
                    Try
                        _OracleConnection = New OracleConnection(ExternalDB_ConnectionString)
                        _OracleConnection.Open()
                    Catch ex As System.Exception
                        'debug.print(ex.Message)
                    End Try
                End If
                Return _OracleConnection

            Case clsDataMapping.enumMappedDBType.mdtMSProject 'AS/15597c
                If MyComputer.FileSystem.FileExists(ExternalDB_ConnectionString) Then 'AS/16441 enclosed
                    Dim connected As Boolean = False
                    If _mspAplication Is Nothing Then
                        Try
                            _mspAplication = New Microsoft.Office.Interop.MSProject.Application
                            connected = _mspAplication.FileOpenEx(ExternalDB_ConnectionString,) 'actually for MS Project its a full path to the file, e.g. "E:\Dropbox\Public\EC\Models EC\AS Test Models\MS Project to ECC\EHFhouse2 - Copy.mpp")
                            _mspAplication.Visible = False 'True
                        Catch ex As System.Exception
                            Debug.Print(ex.Message)
                        End Try
                    End If
                    Return _mspAplication 'AS/16441===
                Else
                    'MsgBox("MS Project file not found",, "Data Mapping")
                    _mspAplication = Nothing
                End If
                Return _mspAplication 'AS/16441==
        End Select

    End Function

    Private Function ExternalDB_GetDBsList() As List(Of String) 'AS/15285
        Dim Lst As New List(Of String)
        Try
            If ExternalDB_Connection.Connect Then
                Dim Data As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("Select [name] FROM sys.databases WHERE owner_sid<>0x01 ORDER BY [name]")
                If Data IsNot Nothing Then
                    For Each tRow As Dictionary(Of String, Object) In Data
                        Dim sName As String = CStr(tRow("name"))
                        If Not _SQL_IgnoreDBs.Contains(sName.ToLower) Then Lst.Add(sName)
                    Next
                End If
            End If
        Catch ex As System.Exception
        End Try
        Return Lst
    End Function

    Private Function ExternalDB_GetTablesList() As List(Of String) 'AS/15285
        Dim Lst As New List(Of String)
        Try
            Select Case ExternalDB_Type 'AS/15285 incorporated Selects Case for db type
                Case clsDataMapping.enumMappedDBType.mdtECC
                        '
                Case clsDataMapping.enumMappedDBType.mdtAccess
                    If ExternalDB_Connection.Connect Then
                        Dim schemaTable As System.Data.DataTable
                        schemaTable = GetDBSchemaTables(DBProviderType.dbptOLEDB, ExternalDB_ConnectionString)

                        For i As Integer = 0 To schemaTable.Rows.Count - 1
                            '''debug.print(schemaTable.Rows(i)!TABLE_NAME.ToString & "   " & schemaTable.Rows(i)!TABLE_TYPE.ToString)
                            If Not schemaTable.Rows(i)!TABLE_TYPE.ToString.ToUpper = "ACCESS TABLE" And Not schemaTable.Rows(i)!TABLE_TYPE.ToString.ToUpper = "SYSTEM TABLE" Then
                                Dim sName As String = schemaTable.Rows(i)!TABLE_NAME.ToString
                                Lst.Add(sName)
                            End If
                        Next
                    End If

                Case clsDataMapping.enumMappedDBType.mdtSQL
                    If ExternalDB_Connection.Connect Then
                        Dim Data As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("SELECT TABLE_NAME as name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME")
                        If Data IsNot Nothing Then
                            For Each tRow As Dictionary(Of String, Object) In Data
                                Dim sName As String = CStr(tRow("name"))
                                If Not _SQL_IgnoreTables.Contains(sName.ToLower) Then Lst.Add(sName)
                            Next
                        End If
                    End If

                Case clsDataMapping.enumMappedDBType.mdtOracle 'AS/15285b addedv piece for Oracle
                    If Oracle_Connection() IsNot Nothing Then
                        Dim oraCommand As OracleCommand = Oracle_Connection.CreateCommand()
                        oraCommand.CommandText = "select table_name from user_tables"
                        Dim oraDataReader As OracleDataReader = oraCommand.ExecuteReader()
                        While oraDataReader.Read()
                            Dim sName As String = oraDataReader.GetString(0)
                            Lst.Add(sName)
                        End While
                        oraCommand = Nothing
                        oraDataReader.Close()
                        oraDataReader = Nothing
                    End If

                Case Else ' mdtNone
                    MsgBox("No mapped database found.",, "Data Mapping") '"AS debug message"
            End Select

        Catch ex As System.Exception
            MsgBox(ex.Message,, "Data Mapping") '"AS debug message"
            'debug.print(ex.Message)
        End Try
        Return Lst
    End Function
    ' D4305 ==

    Private Function ExternalDB_GetColumnsList(sTblName As String) As List(Of String) 'AS/15285
        Dim Lst As New List(Of String)
        Try
            Select Case ExternalDB_Type
                Case clsDataMapping.enumMappedDBType.mdtAccess 'Access

                    Dim dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, ExternalDB_ConnectionString)
                    dbConnection.Open()

                    Dim filterValues As String() = {Nothing, Nothing, sTblName, Nothing}
                    Dim columns = dbConnection.GetSchema("Columns", filterValues)

                    For Each row As DataRow In columns.Rows
                        '''debug.print(row("column_name").ToString & "   " & row("data_type").ToString)
                        Dim sName As String = row("column_name").ToString
                        Lst.Add(sName)
                    Next

                    dbConnection.Close()
                    dbConnection = Nothing

                Case clsDataMapping.enumMappedDBType.mdtSQL 'SQL
                    If ExternalDB_Connection.Connect Then
                        Dim Data As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("SELECT COLUMN_NAME as name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" & sTblName & "'")
                        If Data IsNot Nothing Then
                            For Each tRow As Dictionary(Of String, Object) In Data
                                Dim sName As String = CStr(tRow("name"))
                                Lst.Add(sName)
                            Next
                        End If
                    End If
                Case clsDataMapping.enumMappedDBType.mdtOracle 'Oracle
                    If Oracle_Connection() IsNot Nothing Then
                        Dim oraCommand As OracleCommand = Oracle_Connection.CreateCommand()
                        oraCommand.CommandText = "select column_name from all_tab_columns where table_name = '" & sTblName & "'"
                        Dim oraDataReader As OracleDataReader = oraCommand.ExecuteReader()
                        While oraDataReader.Read()
                            Dim sName As String = oraDataReader.GetString(0)
                            Lst.Add(sName)
                        End While

                        oraCommand = Nothing
                        oraDataReader.Close()
                        oraDataReader = Nothing
                    End If

            End Select

        Catch ex As System.Exception
            ''debug.print(ex.Message)
        End Try
        Return Lst
    End Function

    Private Function ExternalDB_GetColumnValues(DM As clsDataMapping, isMapkeyCol As Boolean) As List(Of String) 'AS/15285 'AS/24231a replaced parameters with dm

        Dim sColName As String = DM.externalColName 'AS/24231f more to 'AS/24231a
        If isMapkeyCol Then sColName = DM.externalMapkeyColName 'AS/24231f

        Dim Lst As New List(Of String)
        Try
            Select Case ExternalDB_Type'AS/15285b incorporated
                Case clsDataMapping.enumMappedDBType.mdtECC 'ECC

                    If DM.eccMappedColType = clsDataMapping.enumMappedColType.mapAlternaives Then 'AS/24231a===

                        Dim srcAltsHierarchy As clsHierarchy = SourceModel.HierarchyAlternatives
                        For Each alt As clsNode In srcAltsHierarchy.Nodes
                            Dim sVal As String = ""
                            If Not isMapkeyCol Then
                                sVal = alt.NodeName
                            Else
                                sVal = Left(alt.NodeGuidID.ToString, 6)
                            End If
                            Lst.Add(sVal)
                        Next

                    ElseIf DM.eccMappedColType = clsDataMapping.enumMappedColType.mapAttributes Then
                        'customAttributes = SourceModel.ProjectManager.Attributes.GetAlternativesAttributes(True)

                        Dim srcAttrGUID As Guid = New Guid(CheckVar("attrfrom", "")) 'AS/24231d===
                        If Not srcAttrGUID.Equals(Guid.Empty) Then
                            'Dim srcAttr As clsAttribute = SourceModel.ProjectManager.Attributes.GetAttributeByID(srcAttrGUID)
                            'Debug.Print(srcAttr.Name)

                            Dim srcAltsHierarchy As clsHierarchy = SourceModel.HierarchyAlternatives
                            For Each alt As clsNode In srcAltsHierarchy.Nodes
                                Dim sVal As String = ""
                                If Not isMapkeyCol Then
                                    Dim av As Object = SourceModel.ProjectManager.Attributes.GetAttributeValue(srcAttrGUID, alt.NodeGuidID)
                                    sVal = av.ToString
                                Else
                                    sVal = Left(alt.NodeGuidID.ToString, 6)
                                End If
                                Lst.Add(sVal)
                            Next
                        End If 'AS/24231d==

                    ElseIf DM.eccMappedColType = clsDataMapping.enumMappedColType.mapCosts Then
                        'AS/24231f===
                        For Each raAlt As RAAlternative In SourceModel.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull
                            Dim sVal As String = ""
                            If Not isMapkeyCol Then
                                Dim av As Object = raAlt.Cost
                                sVal = av.ToString
                            Else
                                sVal = Left(raAlt.ID.ToString, 6)
                            End If
                            Lst.Add(sVal)
                        Next 'AS/24231f==

                    ElseIf DM.eccMappedColType = clsDataMapping.enumMappedColType.mapRisks Then
                        'AS/24231f===
                        For Each raAlt As RAAlternative In SourceModel.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull
                            Dim sVal As String = ""
                            If Not isMapkeyCol Then
                                Dim av As Object = raAlt.RiskOriginal
                                sVal = av.ToString
                            Else
                                sVal = Left(raAlt.ID.ToString, 6)
                            End If
                            Lst.Add(sVal)
                        Next 'AS/24231f==

                    ElseIf DM.eccMappedColType = clsDataMapping.enumMappedColType.mapJudgments Then

                        'Dim srcUser As clsUser = SourceModel.ProjectManager.User 'AS/24231e=== 'AS/24231k
                        Dim srcUser As clsUser = SourceModel.ProjectManager.GetUserByID(CheckVar("userfrom", 0)) 'AS/24231e=== 'AS/24231k

                        SourceModel.ProjectManager.StorageManager.Reader.LoadUserJudgments(srcUser)

                        Dim srcObjGuid As Guid = New Guid(CheckVar("covobjfrom", ""))
                        If srcObjGuid <> Guid.Empty Then srcCovObj = SourceModel.HierarchyObjectives.GetNodeByID(srcObjGuid)

                        Dim srcAltsHierarchy As clsHierarchy = SourceModel.HierarchyAlternatives

                        Select Case srcCovObj.MeasureType

                            Case ECMeasureType.mtDirect

                                For Each alt As clsNode In srcAltsHierarchy.Nodes
                                    Dim sVal As String = ""
                                    If Not isMapkeyCol Then
                                        Dim srcJ As clsDirectMeasureData = CType(GetJudgment(srcCovObj, alt.NodeID, srcUser.UserID), clsDirectMeasureData)
                                        If Not IsNothing(srcJ) Then 'AS/24231k enclosed
                                            sVal = srcJ.SingleValue.ToString
                                        End If
                                    Else
                                        sVal = Left(alt.NodeGuidID.ToString, 6)
                                    End If
                                    Lst.Add(sVal)
                                Next

                            Case ECMeasureType.mtRatings
                                For Each alt As clsNode In srcAltsHierarchy.Nodes
                                    Dim sVal As String = ""
                                    If Not isMapkeyCol Then
                                        Dim srcJ As clsRatingMeasureData = CType(GetJudgment(srcCovObj, alt.NodeID, srcUser.UserID), clsRatingMeasureData)
                                        If Not IsNothing(srcJ) Then 'AS/24231k enclosed
                                            sVal = srcJ.SingleValue.ToString
                                        End If
                                    Else
                                        sVal = Left(alt.NodeGuidID.ToString, 6)
                                    End If
                                    Lst.Add(sVal)
                                Next

                            Case ECMeasureType.mtStep
                                For Each alt As clsNode In srcAltsHierarchy.Nodes
                                    Dim sVal As String = ""
                                    If Not isMapkeyCol Then
                                        Dim srcJ As clsStepMeasureData = CType(GetJudgment(srcCovObj, alt.NodeID, srcUser.UserID), clsStepMeasureData)
                                        If Not IsNothing(srcJ) Then 'AS/24231k enclosed
                                            sVal = srcJ.SingleValue.ToString
                                        End If
                                    Else
                                        sVal = Left(alt.NodeGuidID.ToString, 6)
                                    End If
                                    Lst.Add(sVal)
                                Next

                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                For Each alt As clsNode In srcAltsHierarchy.Nodes
                                    Dim sVal As String = ""
                                    If Not isMapkeyCol Then
                                        Dim srcJ As clsUtilityCurveMeasureData = CType(GetJudgment(srcCovObj, alt.NodeID, srcUser.UserID), clsUtilityCurveMeasureData)
                                        If Not IsNothing(srcJ) Then 'AS/24231k enclosed
                                            sVal = srcJ.SingleValue.ToString
                                        End If
                                    Else
                                        sVal = Left(alt.NodeGuidID.ToString, 6)
                                    End If
                                    Lst.Add(sVal)
                                Next

                        End Select 'AS/24231e==
                    End If 'AS/24231a==

                Case clsDataMapping.enumMappedDBType.mdtAccess 'Access 'AS/15285c 
                    Dim dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, ExternalDB_ConnectionString)
                    dbConnection.Open()

                    Dim sSQL As String
                    Dim oCommand As DbCommand = GetDBCommand(DBProviderType.dbptOLEDB)
                    Dim affected As Integer
                    oCommand.Connection = dbConnection

                    Dim dbReader As DbDataReader

                    Try
                        'sSQL = "Select TOP 3 [" & sColName & "] As name FROM " & DM.externalTblName 'AS/12323debug
                        sSQL = "Select [" & sColName & "] As name FROM " & DM.externalTblName
                        oCommand.CommandText = sSQL
                        affected = DBExecuteNonQuery(DBProviderType.dbptOLEDB, oCommand)

                        dbReader = DBExecuteReader(DBProviderType.dbptOLEDB, oCommand)

                        If dbReader.HasRows Then
                            While dbReader.Read
                                Dim sName As String = dbReader(0).ToString
                                ''debug.print(sName)
                                If isMapkeyCol Then 'AS/14629h===
                                    sName = sName & MAPKEY_DELIMITER & DM.externalTblName
                                End If
                                Lst.Add(sName)
                            End While
                        End If

                        dbReader.Close()
                        dbReader = Nothing

                    Catch ex As System.Exception
                        ''debug.print(ex.Message)
                    End Try

                    dbConnection.Close()
                    dbConnection = Nothing
                Case clsDataMapping.enumMappedDBType.mdtSQL 'SQL
                    If ExternalDB_Connection.Connect Then

                        Dim ExternalDBData As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("SELECT [" & sColName & "] as colvalue FROM " & DM.externalTblName) 'AS/14405
                        'Dim ExternalDBData As List(Of Dictionary(Of String, Object)) 'AS/12323debug===
                        'Data = ExternalDB_Connection.SelectBySQL("SELECT TOP(1) " & sColName & " as colvalue FROM " & DM.externalTblName)
                        'Data = ExternalDB_Connection.SelectBySQL("SELECT TOP(2) " & sColName & " as colvalue FROM " & DM.externalTblName)
                        'ExternalDBData = ExternalDB_Connection.SelectBySQL("SELECT TOP(3) " & sColName & " as colvalue FROM " & DM.externalTblName)
                        'Data = ExternalDB_Connection.SelectBySQL("SELECT TOP(4) " & sColName & " as colvalue FROM " & DM.externalTblName)
                        'Data = ExternalDB_Connection.SelectBySQL("SELECT TOP(5) " & sColName & " as colvalue FROM " & DM.externalTblName) 'AS/12323debug==

                        If ExternalDBData IsNot Nothing Then
                            Dim sValue As String = ""
                            For Each tRow As Dictionary(Of String, Object) In ExternalDBData
                                If Not IsDBNull(tRow("colvalue")) Then 'AS/14404 redid the piece
                                    If Not isMapkeyCol Then 'AS/14629d encllosed and added Else part
                                        sValue = CStr(tRow("colvalue"))
                                    Else
                                        sValue = CStr(tRow("colvalue")) & MAPKEY_DELIMITER & DM.externalTblName
                                    End If
                                Else
                                    sValue = ""
                                End If
                                Lst.Add(sValue)
                            Next
                        End If
                    End If

                Case clsDataMapping.enumMappedDBType.mdtOracle 'Oracle
                    If Oracle_Connection() IsNot Nothing Then
                        Dim oraCommand As OracleCommand = Oracle_Connection.CreateCommand()
                        oraCommand.CommandText = "Select " & sColName & " As name FROM " & DM.externalTblName 'AS/12323debug
                        'oraCommand.CommandText = "Select " & sColName & " As name FROM " & DM.externalTblName & " WHERE ROWNUM <= 3" 'AS/12323debug
                        Dim oraDataReader As OracleDataReader = oraCommand.ExecuteReader()
                        While oraDataReader.Read()
                            'Dim sName As String = oraDataReader.GetString(0)
                            Dim sName As String = oraDataReader.GetOracleValue(0).ToString 'prevent rte if column datatype is other then string
                            Lst.Add(sName)
                        End While

                        oraCommand = Nothing
                        oraDataReader.Close()
                        oraDataReader = Nothing
                    End If

                    '============================================================= 'AS/15597c===
                Case clsDataMapping.enumMappedDBType.mdtMSProject 'mpp file
                    Dim mspActiveProj As Microsoft.Office.Interop.MSProject.Project = _mspAplication.ActiveProject

                    For Each task As Microsoft.Office.Interop.MSProject.Task In mspActiveProj.Tasks
                        If DM.eccMappedColType = clsDataMapping.enumMappedColType.mapAlternaives Then
                            If Not isMapkeyCol Then
                                Lst.Add(task.Name)
                            Else
                                'debug.print(task.UniqueID.ToString & "  " & task.ID.ToString)
                                Lst.Add(task.UniqueID) 'AS/16439
                            End If

                        Else
                            Select Case sColName.ToLower
                                Case "cost"
                                    'debug.print(task.Cost)
                                    Lst.Add(task.Cost)
                                Case "start"
                                    'debug.print(task.Start)
                                    Lst.Add(task.Start.ToString)
                                Case "finish"
                                    'debug.print(task.Finish)
                                    Lst.Add(task.Finish.ToString)
                            End Select
                        End If
                    Next
                    'AS/15597b==

            End Select

        Catch ex As System.Exception
            MsgBox(ex.Message,, "Data Mapping") '"AS debug message"
        End Try
        Return Lst
    End Function

    Private Function ExternalDB_TableHasNotnullableColumns(dbConnection As DbConnection, sTblName As String, sKeyColumn As String, ByRef sNotnullableColumn As String) As Boolean 'AS/22602c 'AS/22683 added sNotnullableColumn

        'For Each dm As clsDataMapping In ProjManager.DataMappings 
        '    If dm.externalTblName = sTblName Then
        '        Debug.Print(dm.externalColName & "  IsNullable = " & dm.externalColIsNullable.ToString)
        '    End If
        'Next 

        Dim nonNullableFound As Boolean = False

        Select Case ExternalDB_Type'AS/15285b incorporated 
            Case clsDataMapping.enumMappedDBType.mdtECC 'ECC

            Case clsDataMapping.enumMappedDBType.mdtAccess 'Access
                If ExternalDB_Connection.Connect Then

                    Dim filterValues As String() = {Nothing, Nothing, sTblName, Nothing}
                    Dim columns = dbConnection.GetSchema("Columns", filterValues)

                    For Each row As DataRow In columns.Rows
                        ''debug.print(row("column_name").ToString & "   " & row("is_nullable").ToString)

                        'If (row("column_name")).ToString.ToLower <> sColName.ToLower And (row("column_name")).ToString.ToLower <> sMapColName.ToLower Then 'AS/22602c
                        If CStr(row("is_nullable")).ToLower = "false" Then
                            If Not row("data_type") = 11 AndAlso row("column_name") <> sKeyColumn Then 'not boolean 'AS/22683 enclosed
                                nonNullableFound = True
                                sNotnullableColumn = row("column_name") 'AS/22683
                                Return nonNullableFound
                            End If 'AS/22683
                        End If

                    Next
                End If


            Case clsDataMapping.enumMappedDBType.mdtSQL 'SQL
                If ExternalDB_Connection.Connect Then
                    'Dim ExternalDBColumnProperties As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("Select column_name As colname, is_nullable as nullable from INFORMATION_SCHEMA.COLUMNS where table_name Like '" & sTblName & "'") 'AS/24173
                    Dim ExternalDBColumnProperties As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("Select column_name As colname, is_nullable as nullable, data_type as coltype from INFORMATION_SCHEMA.COLUMNS where table_name Like '" & sTblName & "'") 'AS/24173

                    If ExternalDBColumnProperties IsNot Nothing Then
                        Dim sName As String = ""
                        For Each tName As Dictionary(Of String, Object) In ExternalDBColumnProperties
                            If Not IsDBNull(tName("colname")) And Not IsDBNull(tName("nullable")) Then
                                sName = CStr(tName("colname"))
                                If CStr(tName("nullable")).ToLower = "no" Then
                                    'debug.print(CStr(tName("colname")) & "   " & CStr(tName("nullable")))
                                    If Not tName("coltype") = "bit" Then 'not boolean 'AS/22683 enclosed
                                        nonNullableFound = True
                                        sNotnullableColumn = tName("colname") 'AS/22683
                                        Return nonNullableFound
                                    End If 'AS/22683
                                End If
                            End If
                        Next
                    End If
                End If

            Case clsDataMapping.enumMappedDBType.mdtOracle 'Oracle 'AS/15285b=== 
                If Oracle_Connection() IsNot Nothing Then
                    Dim restrictions() As String = {Nothing, sTblName, Nothing}
                    Dim myDataTable As DataTable = Oracle_Connection.GetSchema("Columns", restrictions)
                    Dim i As Int32
                    'debug.print("Columns")
                    For i = 0 To myDataTable.Columns.Count - 1
                        'debug.print(myDataTable.Columns(i).Caption & "  " & myDataTable.Rows(0)(i).ToString() & Chr(9))
                        Select Case myDataTable.Columns(i).Caption.ToLower
                            Case "nullable"
                                If myDataTable.Rows(0)(i).ToString().ToLower = "y" Then
                                    nonNullableFound = True
                                Else
                                    nonNullableFound = False
                                End If
                        End Select
                    Next
                End If 'AS/15285b==
        End Select

        Return nonNullableFound

    End Function

    Private Function ExternalDB_TableHasNotnullableColumns_OLD(sColName As String, sTblName As String) As Boolean 'AS/15285

        Dim nonNullableFound As Boolean = False

        Select Case ExternalDB_Type'AS/15285b incorporated 
            Case clsDataMapping.enumMappedDBType.mdtECC 'ECC

            Case clsDataMapping.enumMappedDBType.mdtAccess 'Access
                'goes to another ExternalDB_TableHasNotnullableColumns

            Case clsDataMapping.enumMappedDBType.mdtSQL 'SQL
                If ExternalDB_Connection.Connect Then
                    Dim ExternalDBColumnProperties As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("select column_name as colname, is_nullable as nullable from INFORMATION_SCHEMA.COLUMNS where table_name like '" & sTblName & "'")

                    If ExternalDBColumnProperties IsNot Nothing Then
                        Dim sName As String = ""
                        For Each tName As Dictionary(Of String, Object) In ExternalDBColumnProperties
                            If Not IsDBNull(tName("colname")) And Not IsDBNull(tName("nullable")) Then
                                sName = CStr(tName("colname"))
                                If sName.ToLower <> sColName.ToLower Then
                                    If CStr(tName("nullable")).ToLower = "no" Then
                                        'debug.print(CStr(tName("colname")) & "   " & CStr(tName("nullable")))
                                        nonNullableFound = True
                                        Return nonNullableFound
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If

            Case clsDataMapping.enumMappedDBType.mdtOracle 'Oracle 'AS/15285b=== 
                If Oracle_Connection() IsNot Nothing Then
                    Dim restrictions() As String = {Nothing, sTblName, Nothing}
                    Dim myDataTable As DataTable = Oracle_Connection.GetSchema("Columns", restrictions)
                    Dim i As Int32
                    'debug.print("Columns")
                    For i = 0 To myDataTable.Columns.Count - 1
                        'debug.print(myDataTable.Columns(i).Caption & "  " & myDataTable.Rows(0)(i).ToString() & Chr(9))
                        If (myDataTable.Columns(i).Caption.ToLower) <> sColName.ToLower Then
                            Select Case myDataTable.Columns(i).Caption.ToLower
                                Case "nullable"
                                    If myDataTable.Rows(0)(i).ToString().ToLower = "y" Then
                                        nonNullableFound = True
                                    Else
                                        nonNullableFound = False
                                    End If
                            End Select
                        End If
                    Next
                End If 'AS/15285b==
        End Select

        Return nonNullableFound

    End Function

    Private Function ExternalDB_GetColumnProperties(ByRef dm As clsDataMapping, isMapkeyCol As Boolean) As Boolean 'AS/15285 'AS/22603
        'Private Function ExternalDB_GetColumnProperties(sColName As String, sTblName As String, ByRef exColDatatype As String, ByRef exColMaxLength As String, ByRef exColNullable As Boolean) As Boolean 'AS/15285 'AS/22603

        Dim sColName As String = dm.externalColName 'AS/22603===
        If isMapkeyCol Then sColName = dm.externalMapkeyColName 'AS/22602a
        Dim sTblName As String = dm.externalTblName
        Dim exColDatatype As String = ""
        Dim exColMaxLength As String = ""
        Dim exColNullable As Boolean = False 'AS/22603==

        Dim success As Boolean = False 'AS/22602a

        Try
            Select Case ExternalDB_Type'AS/15285b incorporated
                Case clsDataMapping.enumMappedDBType.mdtECC ' ECC

                Case clsDataMapping.enumMappedDBType.mdtAccess 'Access
                    If ExternalDB_Connection.Connect Then
                        Dim dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, ExternalDB_ConnectionString)
                        dbConnection.Open()

                        Dim filterValues As String() = {Nothing, Nothing, sTblName, Nothing}
                        Dim columns = dbConnection.GetSchema("Columns", filterValues)

                        For Each row As DataRow In columns.Rows
                            'debug.print(row("column_name").ToString & "   " & row("data_type").ToString)

                            If Not IsDBNull(row("column_name")) Then
                                If (row("column_name")).ToString.ToLower = sColName.ToLower Then
                                    If Not IsDBNull(row("data_type")) Then
                                        exColDatatype = CStr(row("data_type"))
                                        'Select Case exColDatatype
                                        'Case "129", "130", "200", "201", "202", "203" 'String Data Types
                                        If Not IsDBNull(row("CHARACTER_MAXIMUM_LENGTH")) Then
                                            exColMaxLength = CStr(row("CHARACTER_MAXIMUM_LENGTH")) 'AS/16276a
                                        End If
                                        'Return True 'AS/16276a moved down
                                    End If
                                    'End If 'AS/22603 moved down

                                    If Not IsDBNull(row("IS_NULLABLE")) Then
                                        If CStr(row("IS_NULLABLE")).ToLower = "true" Then
                                            exColNullable = True
                                        Else
                                            exColNullable = False
                                        End If
                                    End If
                                    'Return True 'AS/16276a moved down 'AS/22602a
                                    success = True 'AS/22602a
                                    Exit For 'AS/22602c
                                End If 'AS/22603 moved down
                            End If

                        Next

                        dbConnection.Close()
                        dbConnection = Nothing
                    End If

                Case clsDataMapping.enumMappedDBType.mdtSQL 'SQL
                    If ExternalDB_Connection.Connect Then
                        Dim ExternalDBColumnProperties As List(Of Dictionary(Of String, Object)) = ExternalDB_Connection.SelectBySQL("select data_type as coltype, character_maximum_length as maxlength, is_nullable as nullable from INFORMATION_SCHEMA.COLUMNS where table_name like '" & sTblName & "' and column_name Like '" & sColName & "'")

                        If ExternalDBColumnProperties IsNot Nothing Then
                            Dim sType As String = ""
                            Dim sLength As String = ""
                            Dim tType As Dictionary(Of String, Object) = ExternalDBColumnProperties(0)
                            If Not IsDBNull(tType("coltype")) Then
                                exColDatatype = CStr(tType("coltype"))
                            End If

                            If Not IsDBNull(tType("maxlength")) Then
                                exColMaxLength = CStr(tType("maxlength"))
                            End If

                            If Not IsDBNull(tType("nullable")) Then
                                If CStr(tType("nullable")).ToLower = "yes" Then
                                    exColNullable = True
                                Else
                                    exColNullable = False
                                End If
                            End If

                            'Return True 'AS/22602a
                            success = True 'AS/22602a
                        End If
                    End If

                Case clsDataMapping.enumMappedDBType.mdtOracle 'Oracle
                    If Oracle_Connection() IsNot Nothing Then
                        Dim restrictions() As String = {Nothing, sTblName, sColName}
                        Dim myDataTable As DataTable = Oracle_Connection.GetSchema("Columns", restrictions)
                        Dim i As Int32
                        'debug.print("Columns")
                        For i = 0 To myDataTable.Columns.Count - 1
                            'debug.print(myDataTable.Columns(i).Caption & "  " & myDataTable.Rows(0)(i).ToString() & Chr(9))
                            Select Case myDataTable.Columns(i).Caption.ToLower
                                Case "datatype"
                                    exColDatatype = myDataTable.Rows(0)(i).ToString()
                                Case "nullable"
                                    If myDataTable.Rows(0)(i).ToString().ToLower = "y" Then
                                        exColNullable = True
                                    Else
                                        exColNullable = False
                                    End If
                                Case "length"
                                    exColMaxLength = myDataTable.Rows(0)(i).ToString()
                            End Select

                        Next
                    End If
            End Select

            If isMapkeyCol Then  'AS/22603===
                If exColDatatype <> "" Then dm.externalMapcolDatatype = exColDatatype
                If exColMaxLength <> "" Then dm.externalMapcolMaxLength = exColMaxLength
                dm.externalMapcolIsNullable = exColNullable
            Else
                If exColDatatype <> "" Then dm.externalColDatatype = exColDatatype
                If exColMaxLength <> "" Then dm.externalColMaxLength = exColMaxLength
                dm.externalColIsNullable = exColNullable
            End If 'AS/22603==

        Catch ex As System.Exception
            MsgBox(ex.Message,, "Data Mapping") '"AS debug message"
        End Try
        'Return False 'AS/22602a
        Return success 'AS/22602a

    End Function

    Private Sub onAjax()
        Dim sResult As String = ""

        Dim sAction = CheckVar(_PARAM_ACTION, "").Trim.ToLower
        'debug.print("sAction = " & sAction) 'AS/12323output

        'Dim externalDBtype As clsDataMapping.enumMappedDBType = CheckVar("dbtype", -1) 'AS/15285 'AS/15285
        If CheckVar("dbtype", -1) <> -1 Then ExternalDB_Type = CheckVar("dbtype", -1)
        'If ExternalDB_Type = -1 Then
        '    MsgBox("No external database type detected",, "As debug message from onAjax")
        '   ' Exit Sub
        'End If

        Select Case sAction
            Case "connect_ecc"
                Dim prjID As Integer = CheckVar("prj", -1) 'AS/24189d===
                sResult = Bool2Num(doConnectEccModel(CheckVar("prj", -1)))

                If SourceModel IsNot Nothing Then ExternalDB_Name = SourceModel.ProjectName

                impAltsinclude = getECCImportOptions() 'moved here from Case "cov_obj"
                impOverwriteScale = CheckVar("scaleoverwrite", False)
                impAltsDuplicates = CType(CheckVar("duplicates", CInt(importAltsDuplicates.impDuplicateAddNewKeepData)), importAltsDuplicates)

                sResult = doDataMappingToExternalDB(sAction) 'AS/24189d==

                If SourceModel IsNot Nothing Then sResult = GetNodesListJSON(GetNonPWCovbj(SourceModel.ProjectManager))

            Case "connect_db"
                sResult = Bool2Num(doConnectDB(CheckVar("remoteDB", -1)))

            Case "importjudg" 'AS/13258===
                Dim srcUser As clsUser = Nothing
                Dim destUsers As List(Of clsUser) = Nothing
                Dim srcNode As clsNode = Nothing 'is srcParentNode
                Dim destNode As clsNode = Nothing 'is destParentNode
                Dim PasteOption As CopyJudgmentsMode = CopyJudgmentsMode.Replace

                'doImportJudgmentsMatrix(srcNode.NodeGuidID, destNode.NodeGuidID, srcUser, destUsers, PasteOption) 'AS/13258== 'AS/17315
                doImportJudgmentsMatrix(Guid.Empty, Guid.Empty, srcUser, destUsers, PasteOption) 'AS/13258== 'AS/17315

            Case "importobj" 'AS/4448f
                doImportSubHierarchy(Guid.Empty, Guid.Empty, importSubhierarchyOptions.impSubhierarchyWithInfodocs) 'AS/4488f

            'Case "importalt"

            '    ' D4129 === 
            '    'AS/24231b moved the piece that was here to Case "cov_obj"

            '    'Dim impOverwriteScale As Boolean = CheckVar("scaleoverwrite", False) 'AS/24231b
            '    ''Dim impAltsDuplicates As importAltsDuplicates = CType(CheckVar("duplicates", CInt(importAltsDuplicates.impDuplicateAddNewKeepData)), importAltsDuplicates) 'AS/12323g
            '    'impAltsDuplicates = CType(CheckVar("duplicates", CInt(importAltsDuplicates.impDuplicateAddNewKeepData)), importAltsDuplicates) 'AS/12323g 'AS/24231b
            '    Dim optFromNodeGUID As String = CheckVar("covobjfrom", "")    ' D4130 
            '    Dim optToNodeGUID As String = CheckVar("datato", "")        ' D4130 

            '    sResult = Bool2Num(doImportDataFromECCModel(impAltsinclude, optFromNodeGUID, optToNodeGUID)) 'AS/12323h
            '    ' D4129 ==

            Case "get_attr"  'AS/24231c

                If SourceModel IsNot Nothing Then sResult = GetAttributesListJSON(SourceModel.ProjectManager.Attributes.GetAlternativesAttributes(True)) 'AS/24231c

            Case "cov_obj"  ' D4130

                impAltsinclude = getECCImportOptions() 'AS/24231b===
                impOverwriteScale = CheckVar("scaleoverwrite", False)
                impAltsDuplicates = CType(CheckVar("duplicates", CInt(importAltsDuplicates.impDuplicateAddNewKeepData)), importAltsDuplicates)

                sResult = doDataMappingToExternalDB(sAction) 'AS/24231b==

                If SourceModel IsNot Nothing Then sResult = GetNodesListJSON(GetNonPWCovbj(SourceModel.ProjectManager)) ' D4130

            Case "get_user" 'AS/24231j
                If SourceModel IsNot Nothing Then sResult = GetUsersListJSON(GetUsers(SourceModel.ProjectManager))

            Case "externaldb_connect", "externaldb__name", "externaldb_table", "externaldb_column", "externaldb_mkey", "externaldb_attr", "externaldb_judg", "mapcol", "externaldb_import", "externaldb_export", "externaldb_createmapping", "select_col", "importalt" 'AS/15285=== AS/15597f added "select_col" 'AS/24231b added "importalt"
                'importMapDataToDGCol = CType(CheckVar("importas", CInt(importMapDataToDG.dgcolAlternatives)), importMapDataToDG) 'AS/12323w 'AS/15285f 'AS/15285q
                importCreateNewAlt = CBool(CheckVar("newalt", 0))
                importReplaceAlt = CBool(CheckVar("replacealt", 0))
                DataInterchangeIncludeColumns = CheckVar("includecolumns", -1) 'AS/15285d
                ExternalDB_Type = CheckVar("dbtype", ExternalDB_Type) 'AS/15285m
                isImport = CBool(CheckVar("isimport", 0)) 'AS/24192

                If ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtAccess Then 'AS/15285m===
                    If sAction = "externaldb_connect" Then
                        sAction = "externaldb__name"
                    End If 'AS/15285m==

                ElseIf ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtECC Then 'AS/24231b
                    importCreateNewAlt = True 'AS/24231b
                End If

                sResult = doDataMappingToExternalDB(sAction) 'AS/15285==

            Case "externaldb_cancel" 'AS/16024b
                ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtNone 'AS/16024b

        End Select

        If sResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If

    End Sub

    Private Function getECCImportOptions() As importECCInclude 'AS/24231b

        Dim optInclude As importECCInclude = CType(CheckVar("include", CInt(importECCInclude.impAltsNames)), importECCInclude) 'AS/12323h 'AS/12323i put back
        'impAltsinclude = CType(CheckVar("include", CInt(importECCInclude.impAltsNames)), importECCInclude) 'AS/12323h 'AS/12323i commented out
        Dim optData As Boolean = CheckVar("data", False)
        Dim optInfodocs As Boolean = CheckVar("infodocs", False)
        Dim optEverything As Boolean = CheckVar("everything", False)

        If optEverything Then
            impAltsinclude = importECCInclude.impAltsEverything
        Else
            If optInclude = importECCInclude.impAltsDefAttributes Then 'default attributes 'AS/12323i===
                If optData And Not optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsDefAttributesAndData
                ElseIf optInfodocs And Not optData Then
                    impAltsinclude = importECCInclude.impAltsDefAttributesAndInfodocs
                ElseIf optData And optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsDefAttributesAndDataAndInfodocs
                Else
                    impAltsinclude = optInclude
                End If
            ElseIf optInclude = importECCInclude.impAltsAllAttributes Then 'all attributes
                If optData And Not optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsAllAttributesAndData
                ElseIf optInfodocs And Not optData Then
                    impAltsinclude = importECCInclude.impAltsAllAttributesAndInfodocs
                ElseIf optData And optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsAllAttributesAndDataAndInfodocs
                Else
                    impAltsinclude = optInclude
                End If
            ElseIf optInclude = importECCInclude.impAltsNames Then 'alts only
                If optData And Not optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsNamesAndData
                ElseIf optInfodocs And Not optData Then
                    impAltsinclude = importECCInclude.impAltsNamesAndInfodocs
                ElseIf optData And optInfodocs Then
                    impAltsinclude = importECCInclude.impAltsNamesAndDataAndInfodocs
                Else
                    impAltsinclude = optInclude
                End If
            End If
        End If

    End Function

    Private Function geteccColumnsSelectedForDI() As List(Of String) 'AS/15285f
        'returns list of GUID's

        Dim lst As New List(Of String)
        ExternalDB_MappedColumnsExported = CheckVar("selected_col", "")

        If ExternalDB_MappedColumnsExported = "" Then
            'MsgBox("No selected columns found",, "As debug message from onAjax") 'AS/22683a
            Return Nothing
        End If

        Dim s() As String = Split(ExternalDB_MappedColumnsExported, ",")
        For i = 0 To UBound(s)
            lst.Add(s(i))
        Next
        Return lst

    End Function

    Private Function GetDataMapping(mappedColGUID As String) As clsDataMapping 'AS/15285

        Dim newDM As clsDataMapping = GetDataMappingByCol(mappedColGUID)

        'read initial mapping data and create and set up new DataMaping object in ProjectManager
        If newDM Is Nothing Then 'AS/14373 enclosed and added Else part -- check for existing column GUID
            newDM = ProjManager.AddDataMapping(clsDataMapping.enumMappedDBType.mdtSQL, ExternalDB_ConnectionString, ExternalDB_Name, ExternalDB_Table, ExternalDB_Column, ExternalDB_MapKey) 'AS/12323xm
            If newDM IsNot Nothing Then
                newDM.eccMappedColID = New Guid(mappedColGUID)
                newDM.externalDBType = ExternalDB_Type 'AS/15285c
                newDM.eccMappedColType = GetMappedColType(newDM.eccMappedColID.ToString) 'AS/15624a
                newDM.externalDBname = getMappedDBName() 'AS/24189e
                'MsgBox("Data Mapping already exists For this column") 'AS/15285e
                Return newDM
            Else
                Return Nothing
            End If
        Else
            newDM.externalColName = ExternalDB_Column
            newDM.externalDBconnString = ExternalDB_ConnectionString
            newDM.externalDBname = ExternalDB_Name
            newDM.externalDBType = ExternalDB_Type 'AS/15285c
            newDM.externalMapkeyColName = ExternalDB_MapKey
            newDM.externalTblName = ExternalDB_Table
            'MsgBox("Data Mapping has been successfully created") 'AS/15285e
            Return newDM
        End If

        Return Nothing

    End Function

    Private Sub doImportDataFromMSProject() 'AS/15597e
        'imports data to Costs, Start, and Finish columns

        Dim dm As clsDataMapping = Nothing
        dm = GetDataMappingByCol("b6ff0096-989a-457c-9ea7-4E72c58f65da") 'AS/15597e===
        If dm Is Nothing Then
            dm = GetDataMapping("b6ff0096-989a-457c-9ea7-4E72c58f65da")
        End If
        dm.eccMappedColType = clsDataMapping.enumMappedColType.mapAlternaives
        dm.externalColName = "Task Name"
        doImportDataFromExternalDB("doimport", dm)
        dm.eccMappedColType = clsDataMapping.enumMappedColType.mapAttributes 'AS/15597e==

        dm = GetDataMappingByCol(ATTRIBUTE_COST_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_COST_ID.ToString)
        End If
        dm.externalColName = "Cost"
        doImportDataFromExternalDB("doimport", dm)

        dm = GetDataMappingByCol(ATTRIBUTE_START_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_START_ID.ToString)
        End If
        dm.externalColName = "Start"
        doImportDataFromExternalDB("doimport", dm)

        dm = GetDataMappingByCol(ATTRIBUTE_FINISH_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_FINISH_ID.ToString)
        End If
        dm.externalColName = "Finish"
        doImportDataFromExternalDB("doimport", dm)

    End Sub

    Private Sub GetAndExportDataToMSProject() 'AS/15597f
        'exports data from Costs, Start, and Finish columns

        Dim dm As clsDataMapping = Nothing
        dm = GetDataMappingByCol(ATTRIBUTE_COST_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_COST_ID.ToString)
        End If

        If Not getEccDataToExport(dm) Then
            MsgBox("No data To export",, "Data Mapping") '"As debug message"
            Exit Sub
        End If

        doExportDataToMSProject(dm) 'AS/15597h temporary do not export Costs until 16001 is fixed 'AS/16216c

        dm = GetDataMappingByCol(ATTRIBUTE_START_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_START_ID.ToString)
        End If

        If Not getEccDataToExport(dm) Then
            MsgBox("No data To export",, "Data Mapping") '"As debug message"
            Exit Sub
        End If

        doExportDataToMSProject(dm)

        dm = GetDataMappingByCol(ATTRIBUTE_FINISH_ID.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(ATTRIBUTE_FINISH_ID.ToString)
        End If

        If Not getEccDataToExport(dm) Then
            MsgBox("No data To export",, "Data Mapping") '"As debug message"
            Exit Sub
        End If

        doExportDataToMSProject(dm)

        dm = GetDataMappingByCol(clsProjectDataProvider.dgColTotal.ToString)
        If dm Is Nothing Then
            dm = GetDataMapping(clsProjectDataProvider.dgColTotal.ToString)
            dm.externalColName = "Priority"
        End If

        If Not getEccDataToExport(dm) Then
            MsgBox("No data To export",, "Data Mapping") '"As debug message"
            Exit Sub
        End If

        doExportDataToMSProject(dm)
    End Sub

    Private Function doExportDataToMSProject(dm As clsDataMapping) As Boolean 'AS/15597f

        Dim mspActiveProj As Microsoft.Office.Interop.MSProject.Project = _mspAplication.ActiveProject
        Dim i As Integer = 0
        Try
            For Each task As Microsoft.Office.Interop.MSProject.Task In mspActiveProj.Tasks 'AS/16439===
                For Each mv As clsDataMappingValue In eccValuesToExport

                    If mv.exMapKey = task.UniqueID Then
                        If task.OutlineChildren.Count = 0 Then
                            mv.exMapKey = Replace(getOriginalMapkeyValue(mv.exMapKey), " '", "''") 'get original mapkey value and check for apostrophe
                            Select Case dm.eccMappedColID
                                Case clsProjectDataProvider.dgColAltName
                                    task.Name = mv.exValue
                                Case ATTRIBUTE_COST_ID
                                    task.Cost = mv.exValue
                                Case ATTRIBUTE_START_ID
                                    'task.Start = mv.exValue 'AS/16440b
                                    task.Start = CDate(mv.exValue) 'AS/16440b
                                Case ATTRIBUTE_FINISH_ID
                                    'task.Finish = mv.exValue 'AS/16440b
                                    task.Finish = CDate(mv.exValue) 'AS/16440b
                                Case clsProjectDataProvider.dgColTotal
                                    task.Priority = 1000 * mv.exValue 'task.Priority = 1000 * cGlobalAlts(AID$).TotalPriority 'R9/0886
                            End Select
                        End If
                    End If
                Next
            Next 'AS/16439==

        Catch ex As System.Exception
            MsgBox("Export error occured: " & ex.Message,, "Data Mapping") '"AS debug message"
            'debug.print(ex.Message)
        End Try

        Return True

    End Function

    Private Sub doImportDataFromExternalDB(sAction As String, newDM As clsDataMapping)  'AS/15285 'AS/24231b converted from function to sub

        getExternalDataToImport(newDM) 'AS/24231b moved piece that was here to the sub

        If newDM.eccMappedColType = clsDataMapping.enumMappedColType.mapAlternaives Then 'import to the DG Alternatives column 'AS/15624a=== 'AS/15624a===
            importAlternativesFromExternalDB(exValuesToImport)

        ElseIf newDM.eccMappedColType = clsDataMapping.enumMappedColType.mapAttributes Then 'import to the DG custom attribute column 
            importAtributesFromExternalDB(newDM.eccMappedColID, exValuesToImport)

        ElseIf newDM.eccMappedColType = clsDataMapping.enumMappedColType.mapCosts Then 'AS/16001=== 'import to the DG Cost column 
            importDefaultAtributesFromExternalDB(newDM.eccMappedColID, exValuesToImport)

        ElseIf newDM.eccMappedColType = clsDataMapping.enumMappedColType.mapRisks Then 'import to the DG P.Failour column 
            importDefaultAtributesFromExternalDB(newDM.eccMappedColID, exValuesToImport) 'AS/16001==

        ElseIf newDM.eccMappedColType = clsDataMapping.enumMappedColType.mapJudgments Then
            Dim sNamesNotFound As String = "" 'AS/16039a
            importNonPWFromExternalDB(newDM.eccMappedColID, exValuesToImport, importMapDataToUsers.usrSelected, sNamesNotFound) 'AS/16036 replaced usrCurrent with 'AS/16039a added parameter sNamesNotFound
            If sNamesNotFound <> "" Then 'AS/16039a===
                Dim sMsg As String = "The following Rating scale intensity or intensities not found: " & sNamesNotFound
                sMsg = sMsg & ". You can add the missing intensities to the rating scale in the model and try again."
                MsgBox(sMsg,, "Data Mapping")
            End If 'AS/16039a==
        End If 'AS/15624a== 'AS/15624a==

    End Sub

    Private Sub getExternalDataToImport(newDM As clsDataMapping) 'AS/24231b
        'create lists of actual values and map keys to import
        Dim exColValuesToImport As List(Of String) = ExternalDB_GetColumnValues(newDM, False) 'get records 'AS/14629d added identificator if it is a Mapkey 'AS/15597d added eccMappedColType 'AS/24231a replaced all parameters with newDM
        Dim exKeyColValuesToImport As List(Of String) = ExternalDB_GetColumnValues(newDM, True) 'AS/14629d added identificator if it is a Mapkey  'AS/24231a replaced all parameters with new

        If exValuesToImport Is Nothing Then
            exValuesToImport = ExternalDB_MappedValuesImported
        End If

        If Not exValuesToImport Is Nothing Then 'AS/14407=== in case if row was deleted/added in the source db
            If exValuesToImport.Count <> exColValuesToImport.Count Then
                exValuesToImport = Nothing
            End If
        End If 'AS/14407==

        If exValuesToImport Is Nothing Then 'create new value AS/12323xg===
            exValuesToImport = New List(Of clsDataMappingValue)
            For i = 0 To exColValuesToImport.Count - 1
                Dim mv As New clsDataMappingValue
                mv.exValue = exColValuesToImport(i)
                mv.exMapKey = exKeyColValuesToImport(i)
                exValuesToImport.Add(mv)
            Next
        Else ' update values for new table/col selected
            For i As Integer = 0 To exValuesToImport.Count - 1
                exValuesToImport(i).exValue = exColValuesToImport(i)
            Next
        End If 'AS/12323xg==
    End Sub

    Private Function getEccDataToExport(newDM As clsDataMapping) As Boolean 'AS/15116c

        'create lists of actual values and map keys to export
        Dim eccColValuesToExport As List(Of String) = ECC_GetColumnValues(newDM.eccMappedColID.ToString) 'get data to export
        Dim eccKeyColValuesToExport As List(Of String) = ECC_GetColumnValues(ATTRIBUTE_MAPKEY_ID.ToString)

        If eccValuesToExport Is Nothing Then
            eccValuesToExport = ExternalDB_MappedValuesExported
        End If

        If Not eccValuesToExport Is Nothing Then 'in case if row was deleted/added in the source db
            If eccValuesToExport.Count <> eccColValuesToExport.Count Then
                eccValuesToExport = Nothing
            End If
        End If

        If eccValuesToExport Is Nothing Then 'create new value
            eccValuesToExport = New List(Of clsDataMappingValue)
            For i = 0 To eccColValuesToExport.Count - 1
                Dim mv As New clsDataMappingValue
                mv.exValue = eccColValuesToExport(i)
                mv.exMapKey = eccKeyColValuesToExport(i)
                eccValuesToExport.Add(mv)

            Next
        Else ' update values for new table/col selected
            For i As Integer = 0 To eccValuesToExport.Count - 1
                eccValuesToExport(i).exValue = eccColValuesToExport(i)
            Next
        End If

        If Not eccColValuesToExport Is Nothing Then
            Return True
        End If
        Return False

    End Function

    Private Sub importAtributesFromExternalDB(attrGUID As Guid, exValuesToImport As List(Of clsDataMappingValue)) 'AS/12323xj revisited the sub to follow the replace options

        Dim attr As clsAttribute = ProjManager.Attributes.GetAttributeByID(attrGUID) 'AS/14487===
        If attr IsNot Nothing AndAlso (attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti) Then
            importEnumerationsFromExternalDB(attr, exValuesToImport)
            'Exit Sub 'AS/14505
        End If 'AS/14487==

        If importMapDataAndReplace = importMapDataReplaceOption.optReplaceExisting Then 'replace all 
            For Each mv As clsDataMappingValue In exValuesToImport
                If mv.eccAltGuid = Guid.Empty Then 'AS/14415===
                    Dim alt As clsNode = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetAlternativeByMapkey(mv.exMapKey)
                    If alt IsNot Nothing Then
                        mv.eccAltGuid = alt.NodeGuidID
                    End If
                End If 'AS/14415==

                'ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, mv.exValue, mv.eccAltGuid, Guid.Empty) 'AS/14488b
                Dim res As Boolean 'AS/14488b=== mimiced SetUserAttribute (CWSw\CoreWS_OperationContracts.vb)
                Select Case attr.ValueType
                    Case AttributeValueTypes.avtString
                        res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, CStr(mv.exValue), mv.eccAltGuid, Guid.Empty)
                    Case AttributeValueTypes.avtLong
                        If IsNumeric(mv.exValue) Then 'AS/14496 enclosed and added Int function
                            Dim v As Long
                            If Long.TryParse(CStr(Int(mv.exValue)), v) Then
                                res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, Int(mv.exValue), mv.eccAltGuid, Guid.Empty)
                            End If
                        End If
                    Case AttributeValueTypes.avtDouble
                        Dim v As Double     ' D1858
                        If Double.TryParse(CStr(mv.exValue), v) Then
                            res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, CDbl(mv.exValue), mv.eccAltGuid, Guid.Empty)
                        End If
                    Case AttributeValueTypes.avtBoolean
                        Dim v As Boolean
                        If Boolean.TryParse(CStr(mv.exValue), v) Then
                            res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, mv.exValue, mv.eccAltGuid, Guid.Empty)
                        End If
                    Case AttributeValueTypes.avtEnumeration 'AS/14505
                        res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, mv.exValue, mv.eccAltGuid, Guid.Empty) 'AS/14505
                End Select 'AS/14488b==
            Next

        ElseIf importMapDataAndReplace = importMapDataReplaceOption.optReplaceEmpty Then 'replace empty
            For Each mv As clsDataMappingValue In exValuesToImport
                If mv.eccAltGuid = Guid.Empty Then 'AS/14415===
                    Dim alt As clsNode = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetAlternativeByMapkey(mv.exMapKey)
                    If alt IsNot Nothing Then
                        mv.eccAltGuid = alt.NodeGuidID
                    End If
                End If 'AS/14415==

                Dim av As Object = ProjManager.Attributes.GetAttributeValue(attrGUID, mv.eccAltGuid)
                If av Is Nothing Then
                    'ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, mv.exValue, mv.eccAltGuid, Guid.Empty) 'AS/145791
                    Dim res As Boolean 'AS/14488b===
                    Select Case attr.ValueType
                        Case AttributeValueTypes.avtString
                            res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, CStr(mv.exValue), mv.eccAltGuid, Guid.Empty)
                        Case AttributeValueTypes.avtLong
                            If IsNumeric(mv.exValue) Then 'AS/14496 enclosed and added Int function
                                Dim v As Long
                                If Long.TryParse(CStr(Int(mv.exValue)), v) Then
                                    res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, Int(mv.exValue), mv.eccAltGuid, Guid.Empty)
                                End If
                            End If
                        Case AttributeValueTypes.avtDouble
                            Dim v As Double     ' D1858
                            If Double.TryParse(CStr(mv.exValue), v) Then
                                res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, CDbl(mv.exValue), mv.eccAltGuid, Guid.Empty)
                            End If
                        Case AttributeValueTypes.avtBoolean
                            Dim v As Boolean
                            If Boolean.TryParse(CStr(mv.exValue), v) Then
                                res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, mv.exValue, mv.eccAltGuid, Guid.Empty)
                            End If
                        Case AttributeValueTypes.avtEnumeration 'AS/14505
                            res = ProjManager.Attributes.SetAttributeValue(attrGUID, UNDEFINED_USER_ID, attr.ValueType, mv.exValue, mv.eccAltGuid, Guid.Empty) 'AS/14505
                    End Select 'AS/14488b==
                End If
            Next
        End If

        ProjManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID, UNDEFINED_USER_ID) 'AS/12323xb 'AS/12323xs

        For i As Integer = 0 To ProjManager.DataMappings.Count - 1 'AS/12323xi=== update attribute DataMappingGUID
            attr = ProjManager.Attributes.GetAttributeByID(attrGUID)

            If attr IsNot Nothing AndAlso attr.DataMappingGUID.Equals(Guid.Empty) Then  ' D4474
                If attrGUID = ProjManager.DataMappings(i).eccMappedColID Then
                    attr.DataMappingGUID = ProjManager.DataMappings(i).DataMappingGUID
                    '''debug.print("attr name = " & attr.Name & "  attr DataMappingGUID = " & attr.DataMappingGUID.ToString)
                    ProjManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID)
                End If
            End If
        Next 'AS/12323xi==

    End Sub

    Private Sub importDefaultAtributesFromExternalDB(mappedColGUID As Guid, exValuesToImport As List(Of clsDataMappingValue)) 'AS/16001

        For Each mv As clsDataMappingValue In exValuesToImport
            If mv.eccAltGuid = Guid.Empty Then
                Dim alt As clsNode = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetAlternativeByMapkey(mv.exMapKey)
                If alt IsNot Nothing Then
                    mv.eccAltGuid = alt.NodeGuidID
                End If
            End If

            If mv.exValue <> "" And IsNumeric(mv.exValue) Then
                Select Case mappedColGUID
                    Case ATTRIBUTE_COST_ID
                        ProjManager.ResourceAligner.SetAlternativeCost(mv.eccAltGuid.ToString, mv.exValue)
                    Case ATTRIBUTE_RISK_ID
                        ProjManager.ResourceAligner.SetAlternativeRisk(mv.eccAltGuid.ToString, mv.exValue)
                End Select
            End If
        Next
        App.ActiveProject.SaveRA()

        Dim attr As clsAttribute = ProjManager.Attributes.GetAttributeByID(mappedColGUID) 'update attribute DataMappingGUID 'AS/21354j===
        For i As Integer = 0 To ProjManager.DataMappings.Count - 1
            If attr IsNot Nothing AndAlso attr.DataMappingGUID.Equals(Guid.Empty) Then
                If mappedColGUID = ProjManager.DataMappings(i).eccMappedColID Then
                    attr.DataMappingGUID = ProjManager.DataMappings(i).DataMappingGUID
                End If
            End If
        Next
        ProjManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID) 'AS/21354j==

    End Sub

    Private Sub importEnumerationsFromExternalDB(attr As clsAttribute, exValuesToImport As List(Of clsDataMappingValue)) 'AS/14487

        For Each mv As clsDataMappingValue In exValuesToImport
            'set eccAltGuid to NodeGuidID if empty (later moved moved up from the bottom of the sub)
            If mv.eccAltGuid = Guid.Empty Then
                Dim alt As clsNode = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetAlternativeByMapkey(mv.exMapKey)
                If alt IsNot Nothing Then
                    mv.eccAltGuid = alt.NodeGuidID
                End If
            End If

            Dim aEnum As clsAttributeEnumeration = ProjManager.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
            If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                aEnum = New clsAttributeEnumeration
                aEnum.ID = Guid.NewGuid
                aEnum.Name = attr.Name
                aEnum.Items = New List(Of clsAttributeEnumerationItem)
                attr.EnumID = aEnum.ID
                ProjManager.Attributes.Enumerations.Add(aEnum)
            End If

            Dim eItem As clsAttributeEnumerationItem = GetEnumerationByName(mv.exValue.ToString, aEnum) 'AS/14576=== revisited the piece
            If eItem Is Nothing Then
                If attr.DefaultValue Is Nothing Then 'AS/14592 'enclosed and added Else part
                    If mv.exValue.ToString.Trim = "" Then 'AS/14592a added If
                        mv.exValue = Guid.NewGuid
                    Else
                        eItem = aEnum.AddItem(mv.exValue)
                        mv.exValue = eItem.ID
                    End If
                Else 'AS/14592
                    mv.exValue = New Guid(attr.DefaultValue.ToString)
                End If
            Else
                mv.exValue = eItem.ID
            End If 'AS/14576==

            ProjManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID)

        Next

    End Sub


    Private Function GetEnumerationByName(enumname As String, aEnum As clsAttributeEnumeration) As clsAttributeEnumerationItem 'AS/14576
        For Each eItem As clsAttributeEnumerationItem In aEnum.Items
            If enumname.Trim = eItem.Value.Trim Then
                Return eItem
            End If
        Next
        Return Nothing
    End Function


    Private Sub importAlternativesFromExternalDB(exValuesToImport As List(Of clsDataMappingValue)) 'AS/12323xf redid the sub

        'get guid's of covering objectives to set the contributions after adding new alts 'AS/12323n=== 
        Dim parent_guids As New List(Of Guid)
        If ProjManager IsNot Nothing Then
            For Each tNode As clsNode In ProjManager.Hierarchy(ProjManager.ActiveHierarchy).TerminalNodes
                parent_guids.Add(tNode.NodeGuidID)
            Next
        End If 'AS/12323n==

        Dim AH As ECCore.clsHierarchy = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            For Each mv As clsDataMappingValue In exValuesToImport
                If importCreateNewAlt Then 'AS/14506

                    If Not AlternativeExists(mv.eccAltGuid) Then 'AS/12323xk
                        If Not MapkeyExists(mv.exMapKey) Then 'AS/14407 enclosed
                            Dim alt As ECCore.clsNode = AH.AddNode(-1)
                            alt.NodeName = mv.exValue
                            mv.eccAltGuid = alt.NodeGuidID

                            Dim raAlt As New RAAlternative 'AS/21354e===
                            raAlt.ID = alt.NodeGuidID.ToString
                            raAlt.Name = alt.NodeName
                            ProjManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Add(raAlt) 'AS/21354e==

                            'set mapkey for imported alt
                            ProjManager.Attributes.SetAttributeValue(ATTRIBUTE_MAPKEY_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, mv.exMapKey, alt.NodeGuidID, Guid.Empty) 'AS/12323xb
                            'ProjManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID, UNDEFINED_USER_ID) 'AS/12323xb 'AS/14629k commented out

                            'set contributions for imported alt
                            ProjManager.UpdateContributions(alt.NodeGuidID, parent_guids, CType(ProjManager.ActiveHierarchy, ECHierarchyID)) 'AS/12323zh
                        Else 'AS/14407=== delete this alt
                            'ask user if it should be deleted or not, otherwise, if we delete the alternative from Datagrid automatically it may also delete other existing alternatives. 
                            'AH.DeleteNode(AH.GetNodeByID((mv.eccAltGuid)))
                        End If 'AS/14407
                    End If
                    'Else 'If importMapDataAndReplace = importMapDataReplaceOption.optReplaceExisting Then 'AS/14506 replaced with Else 'AS/14506b===
                    '    If mv.eccAltGuid <> Guid.Empty Then
                    '        If MapkeyExists(mv.exMapKey) Then
                    '            Dim alt As ECCore.clsNode = AH.GetNodeByID(mv.eccAltGuid)
                    '            If alt IsNot Nothing Then
                    '                alt.NodeName = mv.exValue
                    '            End If
                    '        End If
                    '    Else
                    '        Dim alt As ECCore.clsNode = AH.GetAlternativeByMapkey(mv.exMapKey)
                    '        If alt IsNot Nothing Then
                    '            alt.NodeName = mv.exValue
                    '            mv.eccAltGuid = alt.NodeGuidID
                    '        End If
                    '    End If 'AS/14406== 'AS/14506b==
                End If

                If importReplaceAlt Then 'AS/14506b=== update alt name if alt already exists
                    If mv.eccAltGuid <> Guid.Empty Then
                        If MapkeyExists(mv.exMapKey) Then
                            Dim alt As ECCore.clsNode = AH.GetNodeByID(mv.eccAltGuid)
                            If alt IsNot Nothing Then
                                alt.NodeName = mv.exValue
                            End If
                        End If
                    Else
                        Dim alt As ECCore.clsNode = AH.GetAlternativeByMapkey(mv.exMapKey)
                        If alt IsNot Nothing Then
                            alt.NodeName = mv.exValue
                            mv.eccAltGuid = alt.NodeGuidID
                        End If
                    End If
                End If 'AS/14506b==

            Next

            ProjManager.StorageManager.Writer.SaveProject(True)

        End If 'AS/12323za==

        For Each mv As clsDataMappingValue In exValuesToImport 'AS/12323output===
            'debug.print(mv.eccAltGuid.ToString)
            'debug.print(mv.exValue)
            'debug.print(mv.exMapKey)
        Next 'AS/12323output==

    End Sub

    Private Function AlternativeExists(altGuid As Guid) As Boolean 'AS/12323xk

        Dim AH As ECCore.clsHierarchy = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy)
        For Each alt As clsNode In AH.Nodes
            ''debug.print(alt.NodeName & "  " & alt.NodeGuidID.ToString) 'AS/12323output
            If alt.NodeGuidID = altGuid Then
                Return True
            End If
        Next
        Return False

    End Function

    Private Function MapkeyExists(mapkey As String) As Boolean 'AS/14407

        Try
            For Each attrValue As clsAttributeValue In ProjManager.Attributes.GetAttributesValues(ATTRIBUTE_MAPKEY_ID)
                ''debug.print(attrValue.ToString & "  " & attrValue.Value.ToString) 'AS/12323output
                If attrValue.Value = mapkey Then
                    Return True
                End If
            Next
            Return False
        Catch ex As System.Exception
            ''debug.print(ex.Message)
            Debug.Assert(False)
            Return False
        End Try

    End Function

    Public Function IsGroupSelected() As Integer 'AS/16038
        'Dim is_groupuser As Integer = CheckVar("isgroup", 0) 'AS/16038 'AS/21354f
        Dim uid As Integer = CheckVar("usrid", UNDEFINED_INTEGER_VALUE) 'AS/21354f===

        If IsCombinedUserID(uid) Then
            Return 1
        Else
            Return 0
        End If 'AS/21354f==

    End Function

    Public Function MappingExists() As Integer 'AS/16011

        Try
            Dim colGUID As String = CheckVar("aguid", "")
            If colGUID = "" Then colGUID = CheckVar("oguid", "") 'AS/16215
            If colGUID = "" Then colGUID = "b6ff0096-989a-457c-9ea7-4E72c58f65da" 'dgColAltNameGuid

            For Each dm As clsDataMapping In ProjManager.DataMappings
                If colGUID = dm.eccMappedColID.ToString Then
                    Return 1
                End If
            Next
            Return 0
        Catch ex As System.Exception
            ''debug.print(ex.Message)
            Debug.Assert(False)
            Return 0
        End Try

    End Function

    Public Function getColtype() As Integer 'AS/24231j
        Dim coltype As Integer
        Dim destColGUID As String = CheckVar("aguid", "").Trim 'AS/12323xm
        If destColGUID = "" Then destColGUID = CheckVar("oguid", "").Trim 'AS/15624d

        coltype = GetMappedColType(destColGUID)
        Return coltype
    End Function

    Private Function GetMappedColType(mappedColGUID As String) As clsDataMapping.enumMappedColType 'AS/15624a
        Dim CPA As clsAttributes = ProjManager.Attributes
        Dim coltype As Integer = -1 'AS/15624b

        If mappedColGUID = clsProjectDataProvider.dgColAltName.ToString Then
            coltype = clsDataMapping.enumMappedColType.mapAlternaives
        ElseIf mappedColGUID = ATTRIBUTE_COST_ID.ToString Then 'AS/16001===
            coltype = clsDataMapping.enumMappedColType.mapCosts
        ElseIf mappedColGUID = ATTRIBUTE_RISK_ID.ToString Then
            coltype = clsDataMapping.enumMappedColType.mapRisks 'AS/16001==
        End If

        If coltype = -1 Then 'AS/15624b
            For Each attr As clsAttribute In CPA.AttributesList
                If attr.ID.ToString = mappedColGUID Then
                    ''debug.print(attr.Name & "   " & attr.DataMapping.externalValuePKeys & "   " & dm.externalValuePKeys) 'AS/12323xa
                    coltype = clsDataMapping.enumMappedColType.mapAttributes
                End If
            Next
        End If

        If coltype = -1 Then 'AS/15624b
            For Each co As clsNode In ProjManager.Hierarchy(ProjManager.ActiveHierarchy).TerminalNodes
                If co.NodeGuidID.ToString = mappedColGUID Then
                    ''debug.print(co.NodeName)
                    coltype = clsDataMapping.enumMappedColType.mapJudgments
                End If

            Next
        End If

        Return coltype

    End Function

    Private Sub importNonPWFromExternalDB(nonPWColGUID As Guid, exValuesToImport As List(Of clsDataMappingValue), importToUsers As importMapDataToUsers, ByRef sNames As String) 'AS/15624a 'AS/1-22-19 removed parameter destUsers

        Dim i As Integer = 0 'AS/12323zx
        Dim destUser As clsUser = ProjManager.User 'AS/15624j
        'Dim destUsers As List(Of clsUser) = New List(Of clsUser) 'AS/1-22-19

        Select Case importToUsers
            Case importMapDataToUsers.usrCurrent
                destUser = ProjManager.User 'AS/1-22-19
                'destUsers.Add(ProjManager.User) 'AS/1-22-19
            Case importMapDataToUsers.usrAll
                'destUsers = ProjManager.UsersList 'AS/1-22-19
            Case importMapDataToUsers.usrCombined
                'TBD if needed
            Case importMapDataToUsers.usrSelected
                Dim uid As Integer = CheckVar("usrid", UNDEFINED_INTEGER_VALUE) 'AS/16036===
                If uid <> UNDEFINED_INTEGER_VALUE And uid <> -1 Then 'AS/1-22-19 added <>-1
                    destUser = ProjManager.GetUserByID(uid) 'AS/1-22-19
                    'destUsers.Add(ProjManager.GetUserByID(uid)) 'AS/1-22-19
                Else
                    destUser = ProjManager.User 'AS/1-22-19
                    'destUsers.Add(ProjManager.User) 'AS/1-22-19
                End If 'AS/16036==
            Case importMapDataToUsers.usrPhantom
                'TBD if needed
        End Select
        If IsNothing(destUser) Then 'AS/16038=== 'AS/1-22-19===
            destUser = ProjManager.User
        End If 'AS/16038== 'AS/1-22-19==
        'If destUsers.Count = 0 Then  'AS/1-22-19===
        '    destUsers.Add(ProjManager.User)
        'End If 'AS/1-22-19==

        ProjManager.StorageManager.Reader.LoadUserJudgments(destUser) 'AS/1-22-19 moved down

        Dim covobj As clsNode = CurrentProject.HierarchyObjectives.GetNodeByID(nonPWColGUID) 'AS/15624c=== moved up

        Dim rScale As clsRatingScale = Nothing 'AS/15624c=== 
        If covobj.MeasureType = ECMeasureType.mtRatings Then
            'rScale = ProjManager.MeasureScales.GetRatingScaleByName("Scale For " & covobj.NodeName) 'AS/16039a
            rScale = ProjManager.MeasureScales.GetRatingScaleByID(covobj.MeasurementScaleID) 'AS/16039a

            If IsNothing(rScale) Then 'AS/15624a=== 
                'create new scale
                'rScale = ProjManager.MeasureScales.AddRatingScale 'AS/15624j
                'rScale.Name = "Scale For " + covobj.NodeName 'AS/15624j

                MsgBox("There is no Ratings scale for '" & covobj.NodeName & "'. Please create the scale in the model and try again.",, "Data Mapping") 'AS/15624j===
                Exit Sub
                'Else 'AS/16039c===
                '    If rScale.Name.ToLower = "Default Rating Scale".ToLower Then 'AS/16039a===
                '        'Dim sPrompt As String = "Press 'YES' if you really want to make changes to the Default Rating Scale. Otherwise press 'NO', then you may go to Measure > Measurement Methods > For Alternatives screen and select or create another scale and try again." 'AS/16039a
                '        Dim sPrompt As String = "There is only the Default Ratings Scale for '" & covobj.NodeName & "' which cannot be changed. Please create new scale in the model and try again." 'AS/16039a===
                '        MsgBox(sPrompt,, "Data Mapping")
                '        Exit Sub 'AS/16039a==
                '        'Dim rvMsg As MsgBoxResult 'AS/16039a===
                '        'rvMsg = MsgBox(sPrompt, MsgBoxStyle.YesNo, "Data Mapping")
                '        'Select Case rvMsg
                '        '    Case MsgBoxResult.No
                '        '        Exit Sub
                '        '    Case MsgBoxResult.Yes
                '        '        'Do nothing, just continue
                '        'End Select 'AS/16039a==
                '    End If 'AS/16039a== 'AS/16039c==
            End If

        ElseIf covobj.MeasureType = ECMeasureType.mtPairwise Or covobj.MeasureType = ECMeasureType.mtPWAnalogous Then
            MsgBox("Cannot import to PW",, "Data Mapping")
            Exit Sub 'AS/15624j==
        End If 'AS/15624f== 

        For Each mv As clsDataMappingValue In exValuesToImport
            i = i + 1
            Dim ratingFound As Boolean = False 'AS/15624e

            Dim alt As clsNode = Nothing

            If mv.eccAltGuid = Guid.Empty Then 'AS/14415===
                alt = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetAlternativeByMapkey(mv.exMapKey)
                If alt IsNot Nothing Then
                    mv.eccAltGuid = alt.NodeGuidID
                Else 'AS/15624d=== 
                    'MsgBox("No Map Key found. Make sure you are mapping to the same table.",, "AS debug message from importNonPWFromExternalDB") 'AS/21354g
                    'Exit Sub 'AS/15624d== 'AS/21354g
                    Continue For 'AS/21354g
                End If
            Else 'AS/15624m===
                If alt Is Nothing Then
                    alt = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetNodeByID(mv.eccAltGuid)
                End If 'AS/15624m==
            End If 'AS/14415==

            Dim newJ As clsCustomMeasureData = Nothing
            'Dim RD As clsRatingMeasureData 'AS/16302+-
            Dim ForceAdd As Boolean = False 'AS/16302

            'For Each destUser In destUsers 'AS/1-22-19
            'ProjManager.StorageManager.Reader.LoadUserJudgments(destUser) 'AS/1-22-19 moved here from above

            Select Case covobj.MeasureType

                Case ECMeasureType.mtDirect 'AS/12323zy

                    Dim currJ As clsDirectMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsDirectMeasureData)
                    If Not IsNothing(currJ) Then 'AS/21354d enclosed
                        If IsNumeric(mv.exValue) Then 'AS/15624j enclosed
                            newJ = New clsDirectMeasureData(currJ.NodeID, currJ.ParentNodeID, destUser.UserID, CSng(mv.exValue), False, currJ.Comment)
                        Else
                            'do nothing, just skip it
                        End If
                    Else 'AS/21354d===
                        newJ = New clsDirectMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, CSng(mv.exValue), False, "")
                    End If 'AS/21354d==

                Case ECMeasureType.mtRatings
                    Dim rating As clsRating = Nothing 'AS/15624c
                    'Dim sNames As String = "" 'AS/15624i 'AS/16039a
                    Dim sMsg As String = "" 'AS/15624i

                    If TypeOf (mv.exValue) Is String And Not IsNumeric(mv.exValue) Then 'importing intensities names
                        For Each rat As clsRating In rScale.RatingSet
                            'debug.print(rat.Name)
                            If rat.Name.ToUpper = mv.exValue.ToString.ToUpper Then 'check if intensity name to be imported already exists in the scale and skip it
                                rating = rat 'get this rating; below it will replace the current judgment with this one (the way it works in ECD)
                                ratingFound = True
                            End If
                        Next

                        If ratingFound Then 'if not, then skip it and go to next value to import 'AS/15624g=== moved up
                            'covobj.RatingScaleID(False) = rScale.ID 'AS/15624c 'AS/15624i===
                            'newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment'AS/15624a
                            If Not IsNothing(rating) Then
                                newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, rating, rScale, False, "")
                            Else
                                'MsgBox("rating Is Nothing",, "As debug message from importNonPWFromExternalDB") 'AS/22683b
                            End If
                        Else 'create list of names that not found; will prompt user later
                            If Trim(mv.exValue) <> "" Then 'AS/16039a===
                                If Not sNames.Contains(mv.exValue) Then 'AS/16039c enclosed
                                    If sNames = "" Then
                                        sNames = mv.exValue
                                    Else
                                        sNames = sNames & ", " & mv.exValue
                                    End If
                                End If
                            End If 'AS/16039c
                        End If 'AS/16039a==

                    ElseIf IsNumeric(mv.exValue) Then 'direct entry (numbers between 0 and 1)
                        If mv.exValue >= 0 And mv.exValue <= 1 Then
                            '============================================================================================================
                            'Dim currJ As clsRatingMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsRatingMeasureData) 'AS/15624i 'AS/21354g===
                            'If IsNothing(currJ) Then 'no judjment and/or ratings scale yet
                            '    MsgBox("currJ Is Nothing",, "AS debug message from importNonPWFromExternalDB")
                            'Else 'AS/21354g==
                            Dim R As New clsRating 'AS/16302
                            R.ID = -1
                            R.Name = "Direct Entry from External DB"
                            'R.Value = Single.Parse(FixStringWithSingleValue(CStr(dbReader("DATA"))))
                            R.Value = Single.Parse(mv.exValue.ToString)

                            newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, R, Nothing)

                            newJ.ModifyDate = Now
                            ForceAdd = True

                            'covobj.Judgments.AddMeasureData(RD, True) 'AS/16302==

                            ''If currJ.IsUndefined Then 'no judgment yet 'AS/12323zx=== 'AS/15624h===
                            ''check if value already exists in the existing RatingScale, may be the case when already imported
                            'For Each rat As clsRating In rScale.RatingSet
                            '        'debug.print(rat.Value.ToString)
                            '        If rat.Value = CSng(mv.exValue) Then
                            '            ' MsgBox("Judgment -- undefined, Rating(intesity) - -exists: rat.Value = " & rat.Value.ToString) 'AS/12323zy===
                            '            'currJ.Rating = rat 'AS/15624g===
                            '            'currJ.IsUndefined = False
                            '            'covobj.RatingScaleID(False) = rScale.ID
                            '            ''newJJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment) 'AS/12323zx==
                            '            'Exit Select 'AS/12323zy== 'AS/15624g==
                            '            rating = rat 'AS/15624g
                            '            ratingFound = True 'AS/15624g
                            '        End If
                            '    Next

                            '    'if value to be imported not found, create new rating (intensity) and add it to rating set
                            '    'see example in ECCore\ECCore\StorageReaderAHP.vb\Private Function LoadProjectFromAHP() As Boolean --- from line 770
                            '    If Not ratingFound Then 'AS/15624g enclosed
                            '        rating = rScale.AddIntensity()
                            '        rating.Name = "New rating " & rating.ID.ToString
                            '        rating.Comment = ""
                            '        rating.Value = CSng(mv.exValue)
                            '    End If 'AS/15624g

                            ''currJ.Rating = rating
                            ''currJ.IsUndefined = False
                            ''covobj.RatingScaleID(False) = rScale.ID
                            ''Else 'judgment already exists 'AS/12323zy=== 'AS/15624h==

                            ''If importMapDataAndReplace = importMapDataReplaceOption.optReplaceExisting Then 'AS/15624h removed If
                            ''    check If the rating value already exists in the existing RatingScale

                            'For Each rat As clsRating In rScale.RatingSet
                            '    If rat.Value = CSng(mv.exValue) Then 'if found, then replace current alt-covobj rating with this one

                            'If Not IsNothing(currJ.Rating) Then 'AS/15624i enclosed 'AS/16302===
                            '    'MsgBox("Judgment -- exists, Rating -- exists: currJ.Rating.Value = " & currJ.Rating.Value.ToString) 'AS/15624g===
                            '    currJ.Rating.Value = CSng(mv.exValue)
                            '    currJ.IsUndefined = False
                            '    covobj.RatingScaleID(False) = rScale.ID
                            '    newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment) 'AS/12323zx==
                            'Else
                            '    'MsgBox("Judgment -- exists, Rating -- is Nothing",, "AS debug message from importNonPWFromExternalDB")
                            '    Exit Select 'AS/15624g==
                            'End If 'AS/16302==

                            'rating = rat 'AS/15624g
                            'ratingFound = True 'AS/15624g
                            '    End If
                            'Next

                            'If the Then rating value To be imported Not found, create New rating (intensity), 
                            'add it to rating set And assign the current alt-covobj pair
                            'If Not ratingFound Then 'AS/15624g enclosed
                            '        rating = rScale.AddIntensity()
                            '        rating.Name = "New rating " & rating.ID.ToString
                            '        rating.Comment = ""
                            '        rating.Value = CSng(mv.exValue)
                            '    End If 'AS/15624g

                            'currJ.Rating = rating
                            '    currJ.IsUndefined = False
                            '    covobj.RatingScaleID(False) = rScale.ID
                            '    'End If
                            '    'End If 'AS/12323zy== 'AS/15624h
                            'End If 'AS/15624h 'AS/21354g
                            '============================================================================================================

                        Else
                            MsgBox("Invalid rating input. Value must be in-between 0 and 1",, "Data Mapping") '"AS debug message"
                        End If
                    End If

                            'Next

                            'If Not ratingFound Then 'skip it and go to next value to import 'AS/15624g=== moved up
                            '    Exit Select
                            'End If 'AS/15624e== 'AS/15624g==
                            'End If 'AS/15624c== 'AS/15624f

                            'covobj.RatingScaleID(False) = rScale.ID 'AS/15624c 'AS/15624i===
                            ''newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment'AS/15624a
                            'If Not IsNothing(rating) Then
                            '    newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, rating, rScale, False, "")
                            'Else
                            '    MsgBox("rating is Nothing",, "AS debug message from importNonPWFromExternalDB")
                            'End If 'AS/15624i==

                Case ECMeasureType.mtStep 'AS/12323zy
                    If IsNumeric(mv.exValue) Then 'AS/15624m enclosed
                        Dim currJ As clsStepMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsStepMeasureData)
                        Dim stepFunc As clsStepFunction = CType(covobj.MeasurementScale, clsStepFunction)

                        For Each intervl As clsStepInterval In stepFunc.Intervals
                            ''debug.print(intervl.Name & "  " & intervl.ID.ToString & "  " & intervl.Low.ToString & "  " & intervl.High.ToString & "  " & intervl.Value.ToString & "  " & intervl.StepFunction.Name)
                        Next

                        Dim interval As clsStepInterval
                        interval = stepFunc.AddInterval
                        'interval.Value = dm.externalValue

                        '''debug.print(dm.externalValue.ToString)
                        ''debug.print(interval.ID.ToString & "  ", interval.Name & "  " & interval.Low.ToString & "  " & interval.Value.ToString)

                        If Not IsNothing(currJ) Then 'AS/21354d enclosed and added Else part
                            currJ.Value = CSng(mv.exValue) 'AS/15624d
                            currJ.IsUndefined = False
                            covobj.StepFunctionID(False) = stepFunc.ID 'C0300
                            newJ = New clsStepMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Value, currJ.StepFunction, currJ.IsUndefined, currJ.Comment)
                        Else
                            newJ = New clsStepMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, CSng(mv.exValue), stepFunc, False, "")
                        End If 'AS/21354d==
                    End If'AS/15624m

                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve 'AS/12323zy
                    'For Each co As clsNode In ProjManager.Hierarchy(ProjManager.ActiveHierarchy).TerminalNodes 'AS/12323output===
                    '    If co.MeasureType = ECMeasureType.mtRegularUtilityCurve Then
                    '        For Each al As clsNode In CurrentProject.HierarchyAlternatives.Nodes
                    '            Dim J As clsUtilityCurveMeasureData = CType(GetJudgment(co, al.NodeID, destUser.UserID), clsUtilityCurveMeasureData)
                    '            ''debug.print(co.NodeName & "  " & al.NodeName)
                    '            ''debug.print(J.Data.ToString)
                    '            ''debug.print(J.ObjectValue.ToString)
                    '            ''debug.print(J.SingleValue.ToString)
                    '        Next
                    '    End If
                    'Next 'AS/12323output==
                    If IsNumeric(mv.exValue) Then 'AS/15624m enclosed
                        Dim currJ As clsUtilityCurveMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsUtilityCurveMeasureData)
                        'If currJ.IsUndefined Then 'no judgment yet 'AS/15624f removed If
                        If Not IsNothing(currJ) Then 'AS/21354d enclosed and added Else part
                            currJ.Data = CSng(mv.exValue) 'AS/15624d
                            'currJ.ObjectValue = dm.externalValue
                            currJ.IsUndefined = False
                            newJ = New clsUtilityCurveMeasureData(currJ.NodeID, currJ.ParentNodeID, destUser.UserID, currJ.Data, currJ.UtilityCurve, currJ.IsUndefined, currJ.Comment)
                        Else 'AS/21354d===
                            Dim ucFunc As clsCustomUtilityCurve = CType(covobj.MeasurementScale, clsCustomUtilityCurve)
                            newJ = New clsUtilityCurveMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, CSng(mv.exValue), ucFunc, False, "")
                        End If 'AS/21354d==
                    End If 'AS/15624m
            End Select
            If newJ IsNot Nothing Then
                'covobj.Judgments.AddMeasureData(newJ, CopyJudgmentsMode.Replace) 'AS/16302
                covobj.Judgments.AddMeasureData(newJ, ForceAdd) 'AS/16302
                ForceAdd = False 'set to True only for SF 'AS/16302 
            End If

            'ProjManager.StorageManager.Writer.SaveUserJudgments(destUser, Now) 'AS/1-22-19 moved up into For Each destUser
            'destUser.LastJudgmentTime = VERY_OLD_DATE 'AS/1-22-19 moved up into For Each destUser

        Next

        ProjManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID, UNDEFINED_USER_ID) 'AS/12323xb 'AS/12323xs

        If covobj IsNot Nothing AndAlso covobj.DataMappingGUID.Equals(Guid.Empty) Then  'AS/15624m=== update nodes DataMappingGUID
            For i = 0 To ProjManager.DataMappings.Count - 1
                If covobj.NodeGuidID = ProjManager.DataMappings(i).eccMappedColID Then
                    covobj.DataMappingGUID = ProjManager.DataMappings(i).DataMappingGUID
                    'debug.print("covobj name = " & covobj.NodeName & "  covobj DataMappingGUID = " & covobj.DataMappingGUID.ToString)
                End If
            Next
        End If
        'ProjManager.StorageManager.Writer.SaveDataMapping() 'AS/15624m==

        ProjManager.StorageManager.Writer.SaveUserJudgments(destUser, Now) 'AS/1-22-19 moved up into For Each destUser
        destUser.LastJudgmentTime = VERY_OLD_DATE 'AS/1-22-19 moved up into For Each destUser

        'Next 'AS/1-22-19

        ProjManager.StorageManager.Reader.LoadUserJudgments(destUser) 'AS/1-22-19
        'ProjManager.StorageManager.Reader.LoadUserJudgments(ProjManager.User) 'AS/1-22-19


    End Sub

    Private Sub importNonPWFromSQL(nonPWColGUID As Guid, exValuesToImport As List(Of clsDataMappingValue), importToUsers As importMapDataToUsers, Optional destUsers As List(Of clsUser) = Nothing) 'AS/12323xk
        MsgBox("Under development")
        Exit Sub

        'Dim i As Integer = 0 'AS/12323zx
        'Select Case importToUsers
        '    Case importMapDataToUsers.usrCurrent
        '        Dim destUser As clsUser = ProjManager.User

        '        ProjManager.StorageManager.Reader.LoadUserJudgments(destUser)

        '        For Each dm As clsDataMapping In externalData
        '            i = i + 1 'AS/12323zx
        '            Dim covobj As clsNode = CurrentProject.HierarchyObjectives.GetNodeByID(dm.eccMappedColID)
        '            'Dim alt As clsNode = CurrentProject.HierarchyAlternatives.GetNodeByID(dm.eccAltGuid)
        '            '''debug.print(covobj.NodeName & "   " & alt.NodeName & "   " & destUser.UserName)

        '            Dim newJ As clsCustomMeasureData = Nothing
        '            Select Case covobj.MeasureType
        '                Case ECMeasureType.mtRatings
        '                    'Dim currJ As clsRatingMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsRatingMeasureData)
        '                    Dim rScale As clsRatingScale = ProjManager.MeasureScales.GetRatingScaleByName("Scale For " & covobj.NodeName)

        'If currJ.IsUndefined Then 'no judgment yet 'AS/12323zx===
        '    'check if value already exists in the existing RatingScale
        '    For Each rat As clsRating In rScale.RatingSet
        '        ''debug.print(rat.Value.ToString)
        '        'If rat.Value = CSng(dm.externalValue) Then
        '        '    ' MsgBox("Judgment -- undefined, Rating(intesity) - -exists: rat.Value = " & rat.Value.ToString) 'AS/12323zy===
        '        '    currJ.Rating = rat
        '        '    currJ.IsUndefined = False
        '        '    covobj.RatingScaleID(False) = rScale.ID
        '        '    'newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment) 'AS/12323zx==
        '        '    Exit Select 'AS/12323zy==
        '        'End If
        '    Next

        '    'if value to be imported not found, create new rating (intensity) and add it to rating set
        '    'see example in ECCore\ECCore\StorageReaderAHP.vb\Private Function LoadProjectFromAHP() As Boolean --- from line 770
        '    Dim rating As clsRating
        '    rating = rScale.AddIntensity()
        '    'rating.ID = intensity.ID
        '    rating.Name = "New rating " & CStr(i)
        '    rating.Comment = ""
        '    'rating.Value = CSng(dm.externalValue)

        '    'currJ.Rating = rating
        '    'currJ.IsUndefined = False
        '    covobj.RatingScaleID(False) = rScale.ID
        'Else 'judgment already exists 'AS/12323zy===
        '    If importMapDataAndReplace = importMapDataReplaceOption.optReplaceExisting Then
        '        'check if the rating value already exists in the existing RatingScale
        '        For Each rat As clsRating In rScale.RatingSet
        '            'If rat.Value = CSng(dm.externalValue) Then 'if found, then replace current alt-covobj rating with this one
        '            '    MsgBox("Judgment -- exists, Rating -- exists: rat.Value = " & rat.Value.ToString)
        '            '    currJ.Rating = rat
        '            '    currJ.IsUndefined = False
        '            '    covobj.RatingScaleID(False) = rScale.ID
        '            '    newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment) 'AS/12323zx==
        '            '    Exit Select
        '            'End If
        '        Next

        '        'if the rating value to be imported not found, create new rating (intensity), 
        '        'add it to rating set And assign the current alt-covobj pair
        '        Dim rating As clsRating
        '        rating = rScale.AddIntensity()
        '        rating.Name = "New rating " & CStr(i)
        '        rating.Comment = ""
        '        rating.Value = CSng(dm.externalValue)

        '        currJ.Rating = rating
        '        currJ.IsUndefined = False
        '        covobj.RatingScaleID(False) = rScale.ID
        '    End If
        '    newJ = New clsRatingMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Rating, currJ.RatingScale, currJ.IsUndefined, currJ.Comment)
        'End If 'AS/12323zy==

        'Case ECMeasureType.mtStep 'AS/12323zy
        'Dim currJ As clsStepMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsStepMeasureData)
        'Dim stepFunc As clsStepFunction = CType(covobj.MeasurementScale, clsStepFunction)

        'For Each intervl As clsStepInterval In stepFunc.Intervals
        '    ''debug.print(intervl.Name & "  " & intervl.ID.ToString & "  " & intervl.Low.ToString & "  " & intervl.High.ToString & "  " & intervl.Value.ToString & "  " & intervl.StepFunction.Name)
        'Next

        'Dim interval As clsStepInterval
        'interval = stepFunc.AddInterval
        ''interval.Value = dm.externalValue

        ''''debug.print(dm.externalValue.ToString)
        '''debug.print(interval.ID.ToString & "  ", interval.Name & "  " & interval.Low.ToString & "  " & interval.Value.ToString)

        ''currJ.Value = dm.externalValue
        'currJ.IsUndefined = False
        'covobj.StepFunctionID(False) = stepFunc.ID 'C0300

        'newJ = New clsStepMeasureData(alt.NodeID, covobj.NodeID, destUser.UserID, currJ.Value, currJ.StepFunction, currJ.IsUndefined, currJ.Comment)

        'Case ECMeasureType.mtDirect 'AS/12323zy

        'Dim currJ As clsDirectMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsDirectMeasureData)

        'If currJ.IsUndefined Then 'no judgment yet 
        '    'currJ.DirectData = dm.externalValue
        '    'currJ.ObjectValue = dm.externalValue
        '    currJ.IsUndefined = False
        '    newJ = New clsDirectMeasureData(currJ.NodeID, currJ.ParentNodeID, destUser.UserID, currJ.DirectData, currJ.IsUndefined, currJ.Comment)
        'End If

        'Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve 'AS/12323zy
        'For Each co As clsNode In ProjManager.Hierarchy(ProjManager.ActiveHierarchy).TerminalNodes 'AS/12323output===
        '    If co.MeasureType = ECMeasureType.mtRegularUtilityCurve Then
        '        For Each al As clsNode In CurrentProject.HierarchyAlternatives.Nodes
        '            Dim J As clsUtilityCurveMeasureData = CType(GetJudgment(co, al.NodeID, destUser.UserID), clsUtilityCurveMeasureData)
        '            ''debug.print(co.NodeName & "  " & al.NodeName)
        '            ''debug.print(J.Data.ToString)
        '            ''debug.print(J.ObjectValue.ToString)
        '            ''debug.print(J.SingleValue.ToString)
        '        Next
        '    End If
        'Next 'AS/12323output==

        'Dim currJ As clsUtilityCurveMeasureData = CType(GetJudgment(covobj, alt.NodeID, destUser.UserID), clsUtilityCurveMeasureData)
        'If currJ.IsUndefined Then 'no judgment yet 
        '    'currJ.Data = dm.externalValue
        '    'currJ.ObjectValue = dm.externalValue
        '    currJ.IsUndefined = False
        '    newJ = New clsUtilityCurveMeasureData(currJ.NodeID, currJ.ParentNodeID, destUser.UserID, currJ.Data, currJ.UtilityCurve, currJ.IsUndefined, currJ.Comment)
        'End If

        'Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
        '                    MsgBox("cannot import to PW")
        '            End Select
        '            If newJ IsNot Nothing Then
        '                covobj.Judgments.AddMeasureData(newJ, CopyJudgmentsMode.Replace)
        '            End If
        '        Next

        '        ProjManager.StorageManager.Writer.SaveUserJudgments(destUser, Now)
        '        destUser.LastJudgmentTime = VERY_OLD_DATE
        '        ProjManager.StorageManager.Reader.LoadUserJudgments(destUser)

        '    Case importMapDataToUsers.usrAll


        '    Case importMapDataToUsers.usrCombined

        '    Case importMapDataToUsers.usrSelected

        '    Case importMapDataToUsers.usrPhantom

        'End Select



    End Sub

    Private Sub importInfodocsFromSQL(mappedColName As String, externalData As Collection) 'AS/12323zr
        ' MsgBox("not implemented yet")
        'Dim fUpdated As Boolean = False
        'For Each dm As clsDataMapping In externalData
        '    Dim sContent As String = dm.externalValue
        '    Try
        '        If importMapDataAndReplace = importMapDataReplaceOption.optReplaceExisting Then 'AS/12323zt
        '            ProjManager.InfoDocs.SetNodeInfoDoc(dm.eccAltGuid, sContent)
        '        Else
        '            Dim altInfodoc As String = ProjManager.InfoDocs.GetNodeInfoDoc(dm.eccAltGuid)
        '            If altInfodoc.Trim = "" Then
        '                ProjManager.InfoDocs.SetNodeInfoDoc(dm.eccAltGuid, sContent)
        '            End If
        '        End If
        '    Catch ex As system.Exception
        '        ''debug.print(ex.Message)
        '    End Try
        'Next

        'fUpdated = ProjManager.StorageManager.Writer.SaveInfoDocs()

    End Sub

    Private Function AlreadyImported(dm As clsDataMapping) As Boolean 'AS/12323zg

        Return False 'AS/12323xf temporary hardcoded

        'AS/12323output=== 
        ''debug.print("*** AlreadyImported ***")
        '''debug.print("alternative ID = " & dm.eccAltGuid.ToString)           'ID of the alt to which the value (cost, risk, ...) was imported , for new alternatives eccAltGuid = eccValueGuid
        ''debug.print("cov obj ID = " & dm.eccMappedColID.ToString)
        ''debug.print("import to = " & dm.eccMappedColType.ToString)          'corresponds to the option to import as what selected by user
        '''debug.print("guid of imported value = " & dm.eccValueGuid.ToString) 'guid assigned to the imported value

        '''debug.print("actual imported value = " & dm.externalValue.ToString) 'actual imported value -- alternative name, or cost, or risk, or attr value, or nonPW judgment.
        '''debug.print("mapped value primary keys = " & dm.externalValuePKeys) 'summary of values in the PK columns  
        '''debug.print("Alt map key = " & dm.externalAltMapKey)                'value from the column selected by user as Key column
        'AS/12323output==

        Select Case dm.eccMappedColType
            Case clsDataMapping.enumMappedColType.mapAlternaives
                Dim AH As ECCore.clsHierarchy = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy) 'AS/12323za=== moved code here and later may remove the sub entirely
                For Each alt As clsNode In AH.Nodes
                    ''debug.print(alt.NodeName & "  " & alt.NodeGuidID.ToString) 'AS/12323output
                    'If alt.DataMapping.externalValuePKeys <> "" Then 'AS/12323xb enclosed -- filter out existing 'regular' alternatives 
                    '    If alt.DataMapping.externalValuePKeys = dm.externalValuePKeys Then
                    '        ''debug.print("Alternative  " & alt.NodeName & "  AlreadyImported = True")
                    '        Return True
                    '    Else
                    '        ''debug.print("Alternative  " & alt.NodeName & "  AlreadyImported = False")
                    '        Return False
                    '    End If
                    'End If 'AS/12323xb
                Next

            Case clsDataMapping.enumMappedColType.mapKey 'AS/12323zn 'AS/12323xa
                Dim AH As ECCore.clsHierarchy = ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy) 'AS/12323za=== moved code here and later may remove the sub entirely
                For Each alt As clsNode In AH.Nodes
                    ''debug.print(alt.NodeName & "  " & alt.NodeGuidID.ToString) 'AS/12323output
                    'If alt.DataMapping.externalValuePKeys <> "" Then 'AS/12323xb enclosed -- filter out existing 'regular' alternatives 
                    '    If alt.DataMapping.externalValuePKeys = dm.externalValuePKeys Then
                    '        ''debug.print("MapKey for  " & alt.NodeName & "  AlreadyImported = True")
                    '        Return True
                    '    Else
                    '        ''debug.print("MapKey for  " & alt.NodeName & "  AlreadyImported = False")
                    '        Return False
                    '    End If
                    'End If 'AS/12323xb
                Next


            Case clsDataMapping.enumMappedColType.mapAttributes, clsDataMapping.enumMappedColType.mapCosts, clsDataMapping.enumMappedColType.mapRisks
                Dim CPA As clsAttributes = ProjManager.Attributes
                For Each attr As clsAttribute In CPA.AttributesList
                    'If attr.DataMapping.externalValuePKeys <> "" Then
                    '    ''debug.print(attr.Name & "   " & attr.DataMapping.externalValuePKeys & "   " & dm.externalValuePKeys) 'AS/12323xa
                    '    If attr.DataMapping.externalValuePKeys = dm.externalValuePKeys Then
                    '        Return True
                    '    End If
                    'End If
                Next

            'Case clsDataMapping.enumMappedColType.mapCosts

            'Case clsDataMapping.enumMappedColType.mapRisks

            Case clsDataMapping.enumMappedColType.mapJudgments


        End Select

        Return False

    End Function

    Private Function AlreadyImported(destAttrName As String) As Boolean 'AS/12323zh

        Dim CPA As clsAttributes = ProjManager.Attributes
        For Each attr As clsAttribute In CPA.AttributesList
            If attr.Name = destAttrName Then
                Return True
            End If
        Next

        ''debug.print("Attribute  " & destAttrName & " already imported")
        Return False

    End Function

    Protected Sub Page_PreRenderComplete1(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        Try
            If ExternalDB_Connection() IsNot Nothing AndAlso ExternalDB_Connection.Connection IsNot Nothing AndAlso ExternalDB_Connection.Connection.State = System.Data.ConnectionState.Open Then ExternalDB_Connection.Close()
        Catch ex As System.Exception
        End Try
    End Sub
    ' D4305 ==
    Private Function NodeExists(objGuid As Guid) As Boolean 'AS/12323zc
        Try
            Dim X As clsNode
            X = CurrentProject.HierarchyObjectives.GetNodeByID(objGuid)
            If Not IsNothing(X) Then
                Return True
            End If
        Catch ex As System.Exception
            ''debug.print(ex.Message)
        End Try
        Return False
    End Function

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData 'AS/12323zv
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function

    Private Function doDataMappingToExternalDB(sAction As String) As String 'AS/15285 'AS/22683c

        ''AS/12323output=== temporary debugging pieces
        'For Each node As clsNode In CurrentProject.HierarchyObjectives.Nodes
        '    'debug.print(node.NodeName & "  " & node.NodeID.ToString & "  " & node.NodeGuidID.ToString)
        'Next

        'For Each node As clsNode In CurrentProject.HierarchyAlternatives.Nodes
        '    'debug.print(node.NodeName & "  " & node.NodeID.ToString & "  " & node.NodeGuidID.ToString)
        'Next 'AS/12323output==


        Dim destColGUID As String = CheckVar("aguid", "").Trim 'AS/12323xm
        If destColGUID = "" Then destColGUID = CheckVar("oguid", "").Trim 'AS/15624d
        Dim fResult As Boolean = False
        Dim sMsg As String = ""
        Dim sLst As String = ""
        Dim sRes As String = ""
        Dim sSnapshotComment = "" 'AS/12323xs

        'Dim dmConnection As DbConnection = Nothing 'AS/22603 'AS/16440a moved down
        Dim fConnected As Boolean = False 'AS/16440a
        Dim import_success As Boolean = False 'AS/22683 'AS/24189a moved up
        Dim map_success As Boolean = False 'AS/24400

        Select Case ExternalDB_Type'AS/15285 incorporated Select case and added pieces for Access and Oracle 
            Case clsDataMapping.enumMappedDBType.mdtECC 'ECC

                'ExternalDB_Name = CheckVar("prj", ExternalDB_Name) 'AS/24231k=== 'AS/24189d
                ExternalECC_Covobj = CheckVar("covobj", ExternalECC_Covobj)
                ExternalECC_User = CheckVar("user", ExternalECC_User)

                If SourceModel IsNot Nothing Then 'AS/24189g=== enclosed to prevent RTE in virgin models if not click "Connect"
                    If SourceModel.ProjectName <> "" Then fConnected = True 'AS/24231b
                End If 'AS/24189g==

            Case Else 'any external db - either SQL, Access, Oracle, MS Project

                If CheckVar("str", "") <> "" Then ExternalDB_ConnectionString = CheckVar("str", "") 'AS/12323xq (in addition to D4508 below)

                If ExternalDB_ConnectionString <> "" Then
                    ExternalDB_Name = CheckVar("db", ExternalDB_Name) ' D4508 ===
                    ExternalDB_Table = CheckVar("tbl", ExternalDB_Table)
                    ExternalDB_Column = CheckVar("col", ExternalDB_Column)
                    ExternalDB_MapKey = CheckVar("mapkey", ExternalDB_MapKey)
                    ExternalDB_Attribute = CheckVar("attr", ExternalDB_Attribute) ' D4508 ==
                    ExternalDB_Type = CheckVar("dbtype", ExternalDB_Type) 'AS/15285m

                    Try
                        'dmConnection = ExternalDB_Connection.connection() 'AS/22603 'AS/16440a moved down
                        If ExternalDB_Connection() Is Nothing Then
                            fConnected = False
                        Else
                            fConnected = True
                        End If
                    Catch ex As System.Exception
                        sMsg = ex.Message
                    End Try

                Else
                    'sMsg = "Empty connection string" 'AS/15624k
                End If

        End Select 'AS/15285 

        'Dim success As Boolean = False 'AS/22683 'AS/24189a moved up

        'If Not IsNothing(dmConnection) Then 'AS/16440a
        If fConnected Then 'AS/16440a
            Select Case sAction
                Case "connect_ecc" 'AS/24189g===
                    sMsg = "External model successfully connected"
                    'Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")'AS/24189g== 'AS/25796

                Case "externaldb_connect"
                    If ExternalDB_Type = 3 Then 'do for SQL only
                        If ExternalDB_ConnectionString <> "" Then
                            Dim DBs As List(Of String) = ExternalDB_GetDBsList()
                            For Each sName As String In DBs
                                sLst += String.Format("{0}'{1}'", IIf(sLst = "", "", ","), JS_SafeString(sName))
                            Next
                        End If
                        If sLst = "" Then sMsg = "No databases found. Please make sure you are using a valid connection string."
                        fResult = True
                    End If

                Case "externaldb__name"
                    Dim Tbls As List(Of String) = ExternalDB_GetTablesList()
                    For Each sName As String In Tbls
                        sLst += String.Format("{0}'{1}'", IIf(sLst = "", "", ","), JS_SafeString(sName))
                    Next
                    If sLst = "" Then sMsg = "No tables found"
                    fResult = True

                Case "externaldb_table" 'AS/12323l===
                    Dim Cols As List(Of String) = ExternalDB_GetColumnsList(ExternalDB_Table)
                    For Each sName As String In Cols
                        sLst += String.Format("{0}'{1}'", IIf(sLst = "", "", ","), JS_SafeString(sName))
                    Next
                    If sLst = "" Then sMsg = "No columns found"
                    fResult = True 'AS/12323l==

                Case "externaldb_createmapping", "cov_obj" 'AS/15116d 'AS/24231b added "cov_obj" for ECC model
                    Dim mapping_allowed As Boolean = MappingAllowed(destColGUID)
                    If mapping_allowed Then
                        Dim newDM As clsDataMapping = GetDataMapping(destColGUID)
                        'newDM.eccMappedColType = GetMappedColType(destColGUID) 'AS/15624b

                        Dim colGUID As Guid = New Guid(destColGUID) 'AS/15624q=== update attribute or node DataMappingGUID
                        For i As Integer = 0 To ProjManager.DataMappings.Count - 1
                            Dim attr As clsAttribute = ProjManager.Attributes.GetAttributeByID(colGUID)

                            If attr IsNot Nothing AndAlso attr.DataMappingGUID.Equals(Guid.Empty) Then  ' D4474
                                If colGUID = ProjManager.DataMappings(i).eccMappedColID Then
                                    attr.DataMappingGUID = ProjManager.DataMappings(i).DataMappingGUID
                                    '''debug.print("attr name = " & attr.Name & "  attr DataMappingGUID = " & attr.DataMappingGUID.ToString)
                                    ProjManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, ProjManager.StorageManager.ProjectLocation, ProjManager.StorageManager.ProviderType, ProjManager.StorageManager.ModelID)
                                End If
                            Else 'DG col is not an attribute but a CO
                                Dim covobj As clsNode = CurrentProject.HierarchyObjectives.GetNodeByID(colGUID)
                                If covobj IsNot Nothing AndAlso covobj.DataMappingGUID.Equals(Guid.Empty) Then  'update nodes DataMappingGUID
                                    If covobj.NodeGuidID = ProjManager.DataMappings(i).eccMappedColID Then
                                        covobj.DataMappingGUID = ProjManager.DataMappings(i).DataMappingGUID
                                        ''debug.print("covobj name = " & covobj.NodeName & "  covobj DataMappingGUID = " & covobj.DataMappingGUID.ToString)
                                    End If
                                End If
                            End If
                        Next 'AS/15624q==

                        map_success = True 'AS/24400

                    End If

                    fResult = True

                    If ProjManager.DataMappings IsNot Nothing Then
                        ExternalDB_MapInfo = ProjManager.DataMappings
                    End If

                    ProjManager.StorageManager.Writer.SaveProject(True)

                Case "externaldb_import", "importalt" 'AS/12323r 'AS/24231b added "importalt"
                    App.ActiveProject.SaveStructure("Before importing from External Source", True, True, "Data Mapping") 'AS/12323xs

                    Dim newDM As clsDataMapping = GetDataMapping(destColGUID)
                    If Not newDM Is Nothing Then
                        newDM.eccMappedColType = GetMappedColType(destColGUID) 'AS/15624b

                        If ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtMSProject Then 'AS/15597h 
                            doImportDataFromMSProject() 'AS/15597e

                        ElseIf ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtECC Then 'AS/24231b
                            'doImportDataFromECCModel(newDM) 'AS/24231b
                            doImportDataFromExternalDB("doimport", newDM) 'AS/24231b

                        Else ' do for all db's other than MS Project 'AS/15597d enclosed

                            Dim import_allowed As Boolean = ImportAllowed() 'AS/22683b
                            If import_allowed Then 'AS/22683b enclosed

                                Select Case DataInterchangeIncludeColumns
                                    Case DataInterchangeInclude.diCurrentColumn

                                        doImportDataFromExternalDB("doimport", newDM) 'AS/15116b==
                                        import_success = True'AS/22683

                                    Case DataInterchangeInclude.diSelectedColumns 'AS/15116e=== 

                                        For Each colGUID As String In eccColumnsSelectedForDI
                                            Dim dm As clsDataMapping = GetDataMappingByCol(colGUID)
                                            If Not IsNothing(dm) Then 'AS/16038 enclosed
                                                doImportDataFromExternalDB("doimport", dm)
                                            End If 'AS/16038
                                        Next

                                        import_success = True'AS/22683

                                    Case DataInterchangeInclude.diAllMappedColumns

                                        For Each dm As clsDataMapping In ProjManager.DataMappings
                                            doImportDataFromExternalDB("doimport", dm)
                                        Next

                                        import_success = True 'AS/22683

                                End Select 'AS/15116e==
                            End If 'AS/22683b

                        End If 'AS/15597d==
                    End If

                    fResult = True 'AS/12323l==

                    If exValuesToImport IsNot Nothing Then 'AS/12323xg===
                        ExternalDB_MappedValuesImported = exValuesToImport
                    End If 'AS/12323xg==

                    If ProjManager.DataMappings IsNot Nothing Then 'AS/12323zm===
                        ExternalDB_MapInfo = ProjManager.DataMappings
                    End If 'AS/12323zm==

                    ProjManager.StorageManager.Writer.SaveProject(True) 'AS/12323zw=== moved up into Select

                    App.ActiveProject.SaveStructure("After importing from External Source", True, True, sSnapshotComment) 'AS/12323xs


                Case "externaldb_export" 'AS/14629b added the case

                    UpdateAlternativesList() 'AS/22602h make sure the AlternativesFull is up to date in case user deleted alts from datagrid

                    If ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtMSProject Then 'AS/15597f===
                        GetAndExportDataToMSProject()

                    Else ' do for all db's other than MS Project 'AS/15597f==
                        Dim dmConnection As DbConnection = ExternalDB_Connection.connection() 'AS/16440a

                        If Not IsNothing(dmConnection) Then 'AS/16440a
                            Dim ShowMsgbox As Boolean = True 'AS/22602c

                            Dim export_allowed As Integer = ExportAllowed()  'AS/22683 enclosed
                            If export_allowed <> 0 And export_allowed <> 2 Then '0 - nulls not allowed; 2 = Cancel 
                                Select Case DataInterchangeIncludeColumns
                                    Case DataInterchangeInclude.diCurrentColumn

                                        Dim newDM As clsDataMapping = GetDataMapping(destColGUID)

                                        If Not newDM Is Nothing Then
                                            newDM.eccMappedColType = GetMappedColType(destColGUID) 'AS/15624b
                                            If getEccDataToExport(newDM) Then
                                                If doExportDataToExternalDB(newDM, dmConnection, ShowMsgbox) Then 'AS/22683====
                                                    import_success = True
                                                End If 'AS/22683==
                                            End If
                                        End If

                                    Case DataInterchangeInclude.diSelectedColumns 'AS/15116e===

                                        For Each colGUID As String In eccColumnsSelectedForDI
                                            Dim dm As clsDataMapping = GetDataMappingByCol(colGUID)

                                            If Not dm Is Nothing Then
                                                If dm.DataMappingGUID <> Guid.Empty Then
                                                    If getEccDataToExport(dm) Then
                                                        If doExportDataToExternalDB(dm, dmConnection, ShowMsgbox) Then 'AS/22683====
                                                            import_success = True
                                                        End If 'AS/22683==
                                                    End If
                                                End If
                                            End If
                                        Next

                                    Case DataInterchangeInclude.diAllMappedColumns

                                        For Each dm As clsDataMapping In ProjManager.DataMappings
                                            If getEccDataToExport(dm) Then
                                                If doExportDataToExternalDB(dm, dmConnection, ShowMsgbox) Then 'AS/22602f=== 'AS/22602g=== 'AS/22683
                                                    import_success = True
                                                End If
                                            End If 'AS/22683
                                        Next

                                End Select 'AS/15116e==
                            End If 'AS/22683
                        End If 'AS/16440a
                    End If 'AS/15597f 'AS/15116c==

                    fResult = True

                    If eccValuesToExport IsNot Nothing Then
                        ExternalDB_MappedValuesExported = eccValuesToExport
                    End If

                    If eccColumnsSelectedForDI IsNot Nothing Then 'AS/15285f===
                        ExternalDB_MappedColumnsExported = eccColumnsSelectedForDI.ToString
                    End If 'AS/15285f==

                    If ProjManager.DataMappings IsNot Nothing Then
                        ExternalDB_MapInfo = ProjManager.DataMappings
                    End If

                    ProjManager.StorageManager.Writer.SaveProject(True)

            End Select
        Else 'AS/16441
            'sMsg = "Cannot connect To the external source." 'AS/24189f===
            sMsg = "No connection to the external source detected. Make sure you clicked the 'Connect' button."
            Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping") 'AS/24189f==
            fResult = True
        End If

        If Not ExternalDB_Connection() Is Nothing Then 'AS/15285b enclosed
            If ExternalDB_Type <> clsDataMapping.enumMappedDBType.mdtOracle And ExternalDB_Type <> clsDataMapping.enumMappedDBType.mdtMSProject Then 'AS/15285m enclosed otherwise getting rte with Oracle
                If sMsg = "" AndAlso ExternalDB_Connection.LastError <> "" Then sMsg = ExternalDB_Connection.LastError
            End If

        End If 'AS/15285b

        'sRes = String.Format("[{0},'{1}',[{2}]]", Bool2Num(fResult), JS_SafeString(sMsg), sLst)
        'If ExternalDB_MapInfo IsNot Nothing Then 'AS/12323xl=== 'AS/24173===
        '    sRes = String.Format("[{0},'{1}',[{2}],'{3}']", Bool2Num(fResult), JS_SafeString(sMsg), sLst, ExternalDB_MapInfo(ExternalDB_MapInfo.Count - 1).DataMappingGUID.ToString)
        'Else
        '    sRes = String.Format("[{0},'{1}',[{2}]]", Bool2Num(fResult), JS_SafeString(sMsg), sLst)
        'End If 'AS/24173==
        sRes = String.Format("[{0},'{1}',[{2}]]", Bool2Num(fResult), JS_SafeString(sMsg), sLst) 'AS/24173===
        If ExternalDB_MapInfo IsNot Nothing Then
            If ExternalDB_MapInfo.Count > 0 Then
                sRes = String.Format("[{0},'{1}',[{2}],'{3}']", Bool2Num(fResult), JS_SafeString(sMsg), sLst, ExternalDB_MapInfo(ExternalDB_MapInfo.Count - 1).DataMappingGUID.ToString)
            End If
        End If 'AS/24173==


        ''debug.print(sRes) 'AS/12323xl==

        If import_success Then 'AS/22683===
            Select Case sAction
                Case "externaldb_import"
                    sMsg = "Data imported successfully"
                Case "externaldb_export"
                    sMsg = "Data exported successfully"
            End Select
            Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
        End If 'AS/22683==

        If map_success Then 'AS/24400===
            sMsg = "The column was mapped successfully"
            Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
        End If 'AS/24400==

        Return sRes 'AS/12323zw

    End Function

    Private Function MappingAllowed(mappedColGUID As String) As Boolean 'AS/22683c
        Dim sMsg As String = ""

        If ExternalDB_Type = clsDataMapping.enumMappedDBType.mdtECC Then 'return True if mapped to another ECC model 'AS/24231b===
            If SourceModel.ProjectName <> "" Then
                Return True
            End If
        End If 'AS/24231b==

        'trap empty mapping info
        If ExternalDB_Table = "".Trim Or ExternalDB_Column = "".Trim Or ExternalDB_MapKey = "".Trim Then
            If ExternalDB_Type <> clsDataMapping.enumMappedDBType.mdtMSProject Then 'AS/15597d enclosed
                sMsg = "Either TblName or ColName or MapKey is empty. No mapping created"
            End If
        End If

        Dim newDM As clsDataMapping = GetDataMapping(mappedColGUID)
        If newDM Is Nothing Then
            sMsg = "Could not create mapping." '"AS debug message"
        End If

        If sMsg <> "" Then
            Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
            Return False
        End If

        Return True
    End Function

    Public Function ExportAllowed(Optional ShowMsg As Boolean = True) As Integer 'AS/22683 'AS/24191 added ShowMsg

        Dim dmConnection As DbConnection = Nothing
        Dim sMsg As String = ""

        'check if mappings exist 'AS/24173a===
        If ProjManager.DataMappings.Count = 0 Then
            If ShowMsg Then 'AS/24191 enclosed
                sMsg = "No mapped columns found. Aborting operation."
                Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
            End If
            Return 0
        End If 'AS/24173a==

        Try
            dmConnection = ExternalDB_Connection.connection()
        Catch ex As System.Exception
            sMsg = ex.Message
        End Try

        If Not IsNothing(dmConnection) Then

            'check for not nullable columns
            Dim mapping_found_for_notnullable As Boolean = False
            Dim mapping As clsDataMapping = ProjManager.DataMappings(0)
            Dim NotnulableFieldName As String = ""

            If ExternalDB_TableHasNotnullableColumns(dmConnection, mapping.externalTblName, mapping.externalMapkeyColName, NotnulableFieldName) Then
                For Each dm In ProjectManager.DataMappings
                    If dm.externalColName = NotnulableFieldName Then
                        mapping_found_for_notnullable = True
                        If getEccDataToExport(dm) Then
                            For Each mv As clsDataMappingValue In eccValuesToExport
                                If IsNothing(mv.exValue) Then
                                    sMsg = "One Or more fields In the  '" & mapping.externalTblName & "' table cannot contain a Null value because the Required property for this field is set to True.  Enter a value in this field. "
                                    sMsg = sMsg & "No data exported."
                                    Exit For
                                End If
                            Next
                        End If
                        If sMsg <> "" Then
                            Exit For
                        End If
                    End If
                Next

                'If Not mapping_found_for_notnullable Then 'non-nullable col in db is not mapped, but anyway it must have a value 'AS/24173a===
                '    sMsg = "Field '" & mapping.externalTblName & "." & NotnulableFieldName & "' cannot contain a Null value because the Required property for this field is set to True.  Enter a value in this field. "
                '    sMsg = sMsg & "No data exported."
                'End If 'AS/24173a=
            End If

            'check for selected columns if appropriate 'AS/22683a===
            If DataInterchangeIncludeColumns = DataInterchangeInclude.diSelectedColumns Then
                If IsNothing(eccColumnsSelectedForDI) Then
                    eccColumnsSelectedForDI = geteccColumnsSelectedForDI() '
                    If IsNothing(eccColumnsSelectedForDI) Then
                        sMsg = "No selected columns found. Aborting operation."
                    End If
                Else 'AS/22683b===
                    If eccColumnsSelectedForDI.Count = 0 Then 'AS/22683b=== 
                        MsgBox("No selected columns found. Aborting operation.")
                    End If 'AS/22683b==
                End If
            End If

            If sMsg <> "" Then
                Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                Return 0
            End If 'AS/22683a==

            'check for empty map keys 
            Dim eccKeyColValuesToExport As List(Of String) = ECC_GetColumnValues(ATTRIBUTE_MAPKEY_ID.ToString)
            For Each val As String In eccKeyColValuesToExport
                If Trim(val) = "" Then
                    sMsg = "There are one or more rows in the Data Grid that don't have Map Keys or required data.  They will be ignored." & Chr(13)
                    sMsg = sMsg & "Click OK to continue or Cancel to exit."
                    Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal + MsgBoxStyle.OkCancel, "Data Mapping")
                    Return CInt(rvMsg)
                    Exit For
                End If
            Next

        Else
            Return 0
        End If
        Return -1

    End Function

    Public Function ImportAllowed(Optional ShowMsg As Boolean = True) As Boolean 'AS/22683b 'AS/24191 added ShowMsg; changed to 'As Boolean'

        Dim dmConnection As DbConnection = Nothing
        Try
            dmConnection = ExternalDB_Connection.connection()
        Catch ex As System.Exception
            Dim sMsg As String = ex.Message
        End Try

        If Not IsNothing(dmConnection) Then
            Dim sMsg As String = ""

            'check if mappings exist
            If DataInterchangeInclude.diAllMappedColumns Then
                If ProjManager.DataMappings.Count = 0 Then
                    sMsg = "No mapped columns found. Aborting operation."
                    If ShowMsg Then 'AS/24191 enclosed
                        Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                    End If
                    Return False
                End If
            End If

            'trap empty mapping info
            If ExternalDB_Table = "".Trim Or ExternalDB_Column = "".Trim Or ExternalDB_MapKey = "".Trim Then
                If ExternalDB_Type <> clsDataMapping.enumMappedDBType.mdtMSProject Then 'AS/15597d enclosed
                    sMsg = "Either TblName or ColName or MapKey is empty. Canceling the operation"
                    If ShowMsg Then 'AS/24191 enclosed
                        Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                    End If
                    Return False
                End If
            End If

            'check for selected columns if appropriate 
            If DataInterchangeIncludeColumns = DataInterchangeInclude.diSelectedColumns Then
                If IsNothing(eccColumnsSelectedForDI) Then
                    eccColumnsSelectedForDI = geteccColumnsSelectedForDI() '
                    If IsNothing(eccColumnsSelectedForDI) Then
                        sMsg = "No selected columns found. Aborting operation."
                        If ShowMsg Then 'AS/24191 enclosed
                            Dim rvMsg As MsgBoxResult = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                        End If
                        Return False
                    End If
                End If
            End If

        Else
            Return False
        End If

        Return True

    End Function

    Private Function GetDataMappingByCol(colGUID As String) As clsDataMapping 'AS/14373
        For Each dm As clsDataMapping In ProjManager.DataMappings
            If dm.eccMappedColID.ToString = colGUID Then
                Return dm
            End If
        Next
        Return Nothing
    End Function

    Public Function ECC_GetColumnValues(colGUID As String) As List(Of String) 'AS/14629b

        Dim Lst As New List(Of String)

        Dim retVal As String = ""
        Dim i As Integer = 0
        Dim fromColName As String = "altname"

        Dim attr As clsAttribute = ProjManager.Attributes.GetAttributeByID(New Guid(colGUID))
        Dim covobj As clsNode = CurrentProject.HierarchyObjectives.GetNodeByID(New Guid(colGUID))
        If colGUID = clsProjectDataProvider.dgColTotal.ToString Then 'AS/15597g=== added Totals cols
            fromColName = "totals"
        ElseIf colGUID = ATTRIBUTE_COST_ID.ToString Then  'AS/16001a===
            fromColName = "cost"
        ElseIf colGUID = ATTRIBUTE_RISK_ID.ToString Then
            fromColName = "risk" 'AS/16001a==
        ElseIf attr IsNot Nothing Then 'AS/15597g==
            fromColName = "attribute"
        Else 'AS/16039b===
            If covobj IsNot Nothing Then
                fromColName = "covobj"
            End If 'AS/16039b==
        End If

        Select Case fromColName
            Case "altname"
                For Each alt As clsNode In ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).TerminalNodes
                    Lst.Add(alt.NodeName)
                Next
            Case "attribute"
                For Each alt As clsNode In ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).TerminalNodes
                    'Dim attrValues As String = ""
                    Dim sValue As String = ""
                    If attr.ValueType = AttributeValueTypes.avtDouble Or attr.ValueType = AttributeValueTypes.avtLong Then
                        Try
                            'debug.print(attr.Name & ", value = " & ProjManager.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID).ToString)

                            Dim aVal As Double = CDbl(ProjManager.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID))
                            If aVal = Int32.MinValue Then
                                sValue = " "
                                'attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
                            Else
                                sValue = JS_SafeNumber(aVal)
                                'attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("{0}", JS_SafeString(sValue))
                            End If
                        Catch
                            sValue = " "
                            'attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
                        End Try
                    Else
                        sValue = ProjManager.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
                        'attrValues += CStr(IIf(attrValues <> "", ",", "")) + String.Format("'{0}'", JS_SafeString(sValue))
                    End If
                    Lst.Add(sValue)
                Next
            Case "covobj" 'AS/16039b===
                'MsgBox("Under construction")
                Dim uid As Integer = CheckVar("usrid", UNDEFINED_INTEGER_VALUE) 'AS/22565b
                For Each alt As clsNode In ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).TerminalNodes
                    Dim sValue As String = ""
                    'Dim currJ As clsNonPairwiseMeasureData = GetJudgment(covobj, alt.NodeID, ProjManager.User.UserID)

                    Select Case covobj.MeasureType
                        Case ECMeasureType.mtRatings
                            'Dim currJ As clsRatingMeasureData = GetJudgment(covobj, alt.NodeID, ProjManager.User.UserID) 'AS/22565b
                            Dim currJ As clsRatingMeasureData = GetJudgment(covobj, alt.NodeID, uid) 'AS/22565b
                            'sValue = currJ.Rating.Name 'AS/16302
                            Try 'AS/16334
                                If Not IsNothing(currJ.Rating.RatingScale) Then 'AS/16302===
                                    sValue = currJ.Rating.Name
                                Else
                                    sValue = JS_SafeNumber(currJ.Rating.Value)
                                End If 'AS/16302==
                            Catch 'AS/16334===
                                sValue = " "
                            End Try'AS/16334==

                        Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                            'Dim currJ As clsNonPairwiseMeasureData = GetJudgment(covobj, alt.NodeID, ProjManager.User.UserID) 'AS/22565b
                            Dim currJ As clsNonPairwiseMeasureData = GetJudgment(covobj, alt.NodeID, uid) 'AS/22565b

                            Try
                                'Dim aVal As Double = CDbl(currJ.SingleValue) 'AS/16276b
                                Dim aVal As Double = CDbl(currJ.ObjectValue) 'AS/16276b
                                If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then 'AS/16335 added NaN
                                    sValue = " "
                                Else
                                    sValue = JS_SafeNumber(aVal)
                                End If
                            Catch
                                sValue = " "
                            End Try
                    End Select 'AS/16039b==

                    Lst.Add(sValue)
                Next

            Case "totals" 'AS/15597g=== 

                Dim sValue As String = ""

                Dim CalcTarget As clsCalculationTarget
                If IsCombinedUserID(ProjManager.User.UserID) Then
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjManager.CombinedGroups.GetCombinedGroupByUserID(ProjManager.User.UserID))
                Else
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, ProjManager.GetUserByID(ProjManager.User.UserID))
                End If
                ProjManager.CalculationsManager.Calculate(CalcTarget, ProjManager.Hierarchy(ProjManager.ActiveHierarchy).Nodes(0), ProjManager.ActiveHierarchy, ProjManager.ActiveAltsHierarchy)

                For Each alt As clsNode In ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).TerminalNodes
                    Dim total = alt.UnnormalizedPriority

                    If total = Int32.MinValue Or total.Equals(Double.NaN) Then 'AS/16335 added NaN
                        sValue = " "
                    Else
                        sValue = JS_SafeNumber(total)
                    End If

                    Lst.Add(sValue)
                Next

            Case "cost" 'AS/16001a===

                Dim sValue As String = ""
                For Each alt As RAAlternative In ProjManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull
                    sValue = JS_SafeNumber(alt.Cost)
                    Lst.Add(sValue)
                Next

            Case "risk"

                Dim sValue As String = ""
                For Each alt As RAAlternative In ProjManager.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull
                    sValue = JS_SafeNumber(alt.RiskOriginal)
                    Lst.Add(sValue)
                Next 'AS/16001a==

                'Dim DGData As Object = Nothing
                'Dim DGPriority As Double = Nothing
                'Dim sValue As String = ""

                ' below -- mimiced from DataGrid.aspx.vb, Protected Sub RadToolbarMain_ButtonClick (button "Download")

                'For Each alt As clsNode In ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).TerminalNodes
                '    For Each tNode As clsNode In ProjManager.Hierarchy(ProjManager.ActiveHierarchy).TerminalNodes
                '        Select Case tNode.MeasureType
                '            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                '                DGPriority = tNode.Judgments.Weights.GetUserWeights(ProjManager.User.UserID, ECSynthesisMode.smIdeal, ProjManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                '                DGData = DGPriority
                '            Case ECMeasureType.mtRatings
                '                Dim rData As clsRatingMeasureData = CType(tNode.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, tNode.NodeID, ProjManager.User.UserID)
                '                If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                '                    DGData = IIf(rData.Rating.ID <> -1, rData.Rating.Name, rData.Rating.Value)
                '                    DGPriority = rData.Rating.Value
                '                Else
                '                    DGData = ""
                '                    DGPriority = 0
                '                End If
                '            Case Else
                '                Dim nonpwData As clsNonPairwiseMeasureData = CType(tNode.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, tNode.NodeID, ProjManager.User.UserID)
                '                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                '                    DGData = CSng(nonpwData.ObjectValue)
                '                    DGPriority = nonpwData.SingleValue
                '                Else
                '                    DGData = ""
                '                    DGPriority = 0
                '                End If
                '        End Select
                '    Next

        End Select

        Return Lst
    End Function

    Private Function doExportDataToExternalDB(dm As clsDataMapping, dmConnection As DbConnection, ByRef ShowMsgbox As Boolean) As Boolean 'AS/15285 'AS/22603 added dmConnection 'AS/22602c

        'get column datatype and size 'AS/14629d===
        'Dim exColDatatype As String = "" 'AS/22603===
        'Dim exColMaxLength As String = ""
        'Dim exColIsNullable As Boolean = False
        'Dim rv As Boolean = ExternalDB_GetColumnProperties(dm.externalColName, dm.externalTblName, exColDatatype, exColMaxLength, exColIsNullable) 'AS/14629d== 'AS/22603==
        Dim rv As Boolean = ExternalDB_GetColumnProperties(dm, False) 'AS/14629d== 'AS/22603
        rv = ExternalDB_GetColumnProperties(dm, True) 'if TRUE, get properties for the external db column used as ID 'AS/22603

        'Dim exMapcolDatatype As String = "" 'AS/14629e===  'AS/22603===
        'Dim exMapcolMaxLength As String = ""
        'Dim exMapcolIsNullable As Boolean = False
        ''rv = ExternalDB_GetColumnProperties(dm.externalColName, dm.externalTblName, exMapcolDatatype, exMapcolMaxLength, exMapcolIsNullable) 'AS/14629e== moved up out of the loop 'AS/22603
        'rv = ExternalDB_GetColumnProperties(dm) 'AS/14629e== moved up out of the loop 'AS/22603==

        Dim dbReader As DbDataReader
        'Dim m_ProviderType As DBProviderType = DBProviderType.dbptSQLClient 'AS/16042

        Dim m_ProviderType As DBProviderType = DBProviderType.dbptUnspecified 'AS/16042===
        Select Case ExternalDB_Type
            Case clsDataMapping.enumMappedDBType.mdtSQL
                m_ProviderType = DBProviderType.dbptSQLClient
            Case clsDataMapping.enumMappedDBType.mdtAccess
                m_ProviderType = DBProviderType.dbptOLEDB
            Case clsDataMapping.enumMappedDBType.mdtOracle
                m_ProviderType = DBProviderType.dbptOracle
            Case clsDataMapping.enumMappedDBType.mdtMSProject
                m_ProviderType = DBProviderType.dbptOLEDB
            Case clsDataMapping.enumMappedDBType.mdtMSProjectServer
                MsgBox("External DB type for MS Project Server not specified",, "Data Mapping") '"AS debug message"
            Case Else
                MsgBox("External DB type not specified",, "Data Mapping") '"AS debug message"
        End Select 'AS/16042==

        Dim oCommand As DbCommand = GetDBCommand(m_ProviderType)
        Dim oCommandGetRows As DbCommand = GetDBCommand(m_ProviderType)
        Dim affected As Integer
        Dim found As Boolean = False
        Dim sMsg As String
        Dim rvMsg As MsgBoxResult 'AS/14629j

        'oCommand.Connection = ExternalDB_Connection.Connection() 'AS/22603===
        'oCommandGetRows.Connection = ExternalDB_Connection.Connection()
        oCommand.Connection = dmConnection
        oCommandGetRows.Connection = dmConnection 'AS/22603==

        For Each mv As clsDataMappingValue In eccValuesToExport
            oCommand.Parameters.Clear()

            'If IsNothing(mv.exValue) Then Continue For 'AS/22602b 'AS/22602c===
            ''==================================
            ''check for empty map keys
            'If Trim(mv.exMapKey) = "" Then
            '    'sMsg = "There are one Or more rows in the Data Grid that don't have Map Keys or valid data." 'AS/14629j=== revisited the piece
            '    'sMsg = "Please go to Structure tab and enter valid Map Keys values. You may also press 'Cancel' to skip the current row (no new record added)."
            '    'rvMsg = MsgBox(sMsg, MsgBoxStyle.OkCancel)
            '    'Select Case rvMsg
            '    '    Case MsgBoxResult.Ok
            '    '        Return False
            '    '    Case MsgBoxResult.Cancel
            '    '        Continue For
            '    'End Select 'AS/14629j==
            '    Dim externalMapkeyDatatype As String = getExternalColDatatype(dm, True) 'AS/22620 'True if get for ID column
            '    Select Case externalMapkeyDatatype'AS/22620 enclosed the below in Select
            '        Case "num"
            '            oCommand.CommandText = "SELECT MAX( [" & dm.externalMapkeyColName & "] ) FROM [" & dm.externalTblName & "] "
            '            Dim res As Object = DBExecuteScalar(m_ProviderType, oCommand)
            '            If Not TypeOf (res) Is DBNull Then
            '                If IsNumeric(res) Then
            '                    mv.exMapKey = (res + 1).ToString
            '                Else
            '                    rvMsg = MsgBox("No mapping found for key '" & mv.exMapKey)
            '                    Continue For
            '                End If
            '            End If
            '        Case "date"
            '            mv.exMapKey = Now.Date
            '        Case Else 'use GUID to create  new key
            '            mv.exMapKey = Left(Guid.NewGuid.ToString, 8)
            '    End Select
            'End If

            ''for strings, make sure that data size fits the max size of the field 
            '''debug.print("colname = " & dm.externalColName & " datatype = " & dm.externalColDatatype & " value = " & (mv.exValue.ToString)) 'AS/14629g===
            '''debug.print("TypeName = " & TypeName(mv.exValue.ToString) & " VbTypeName = " & VbTypeName(mv.exValue.ToString))

            'Dim continueFor As Boolean = False
            ''If Not ValidDataToExport(dm.externalColDatatype, exColMaxLength, mv.exValue, continueFor) Then 'AS/16276a=== 'AS/22603
            'If Not ValidDataToExport(dm, mv, continueFor) Then 'AS/16276a=== 'AS/22603
            '    'Return False 'AS/21354l
            '    Continue For 'AS/21354l
            'Else
            '    If continueFor Then Continue For
            'End If 'AS/16276a== 'AS/22602c==

            If IsNothing(mv.exValue) Or Not ValidDataToExport(dm, mv) Then 'AS/22602c===
                'If ShowMsgbox Then 'AS/22602i===
                '    sMsg = "There are one or more rows in the Data Grid that don't have Map Keys or required data. " & Chr(13)
                '    sMsg = sMsg & "They will be ignored."
                '    rvMsg = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                '    ShowMsgbox = False
                'End If 'AS/22602i==
                Continue For
            End If 'AS/22602c== 

            'get original mapkey value
            mv.exMapKey = getOriginalMapkeyValue(mv.exMapKey)

            'do in case string contains apostrophe
            mv.exMapKey = Replace(mv.exMapKey, " '", "''")

            'see if the current mapkey already exists in the table
            oCommandGetRows.CommandText = "SELECT [" & dm.externalMapkeyColName & "] FROM [" & dm.externalTblName & "] WHERE [" & dm.externalMapkeyColName & "] like '" & mv.exMapKey & "'"
            dbReader = DBExecuteReader(m_ProviderType, oCommandGetRows)
            If dbReader.HasRows Then
                found = True
            Else
                found = False
            End If
            dbReader.Close()


            Try
                If found Then
                    'found = True 'AS/22602e
                    'oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = '" & mv.exValue & "' WHERE [" & dm.externalMapkeyColName & "] = '" & mv.exMapKey & "'" 'AS/16042 'AS/16276a===
                    'If Not IsNumeric(mv.exMapKey) Then 'AS/16042===
                    '    oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = '" & mv.exValue & "' WHERE [" & dm.externalMapkeyColName & "] = '" & mv.exMapKey & "'"
                    'Else
                    '    oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = '" & mv.exValue & "' WHERE [" & dm.externalMapkeyColName & "] = " & mv.exMapKey
                    'End If 'AS/16042== 
                    'affected = DBExecuteNonQuery(m_ProviderType, oCommand) 'AS/16276a==

                    Dim valApostrophe As String = "" 'AS/16276a===
                    Dim mkApostrophe As String = ""

                    If Not IsNumeric(mv.exValue) Then
                        valApostrophe = "'"
                    End If
                    If Not IsNumeric(mv.exMapKey) Then
                        mkApostrophe = "'"
                    End If

                    'oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = " & valApostrophe & mv.exValue & valApostrophe & " WHERE [" & dm.externalMapkeyColName & "] = " & mkApostrophe & mv.exMapKey & mkApostrophe 'AS/22620
                    Dim externalColDatatype As String = getExternalColDatatype(dm) 'AS/22620===
                    Select Case externalColDatatype
                        Case "bool"
                            oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = " & mv.exValue & " WHERE [" & dm.externalMapkeyColName & "] = " & mkApostrophe & mv.exMapKey & mkApostrophe
                        Case Else
                            oCommand.CommandText = "UPDATE [" & dm.externalTblName & "] SET [" & dm.externalColName & "] = " & valApostrophe & mv.exValue & valApostrophe & " WHERE [" & dm.externalMapkeyColName & "] = " & mkApostrophe & mv.exMapKey & mkApostrophe
                    End Select 'AS/22620==

                    Debug.Print(oCommand.CommandText)
                    If Len(mv.exValue.ToString.Trim) > 0 Then
                        affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                    End If 'AS/16276a==

                Else 'matching map key was not found
                    'found = False  'AS/22602e 
                    'rvMsg = MsgBox("No record with the key '" & mv.exMapKey & "' found. Do you want to add new record to the table?", MsgBoxStyle.YesNo) 'AS/22602b
                    'rvMsg = MsgBox("You are about to add new record(s) to the table. Proceed?", MsgBoxStyle.YesNo) 'AS/22602b 'AS/22602c

                    'Select Case rvMsg
                    '    Case MsgBoxResult.Yes 'add new record
                    '        'check for the length of the key string, skip the row if exceeds
                    '        'If CInt(exMapcolMaxLength) <> -1 And Len(mv.exMapKey) > CInt(exMapcolMaxLength) Then 'AS/22603
                    '        Select Case dm.externalMapcolDatatype 'AS/22602b
                    '            Case "129", "130", "200", "201", "202", "203" 'String Data Types 'AS/22602b
                    '                If CInt(dm.externalMapcolMaxLength) <> -1 And Len(mv.exMapKey) > CInt(dm.externalMapcolMaxLength) Then 'AS/22603
                    '                    sMsg = "The key name '" & mv.exMapKey & "' is too long - it contains "
                    '                    sMsg = sMsg & Len(mv.exMapKey) & " characters.  "
                    '                    sMsg = sMsg & "Please trancate it to " & dm.externalMapcolMaxLength & " characters. "
                    '                    sMsg = sMsg & "The record will be ignored. You can export it later." & Chr(13) & Chr(13)
                    '                    sMsg = sMsg & "Press 'OK' to continue or 'Cancel' to exit export operation."
                    '                    Dim rvMaxLength = MsgBox(sMsg, MsgBoxStyle.OkCancel)

                    '                    Select Case rvMaxLength
                    '                        Case MsgBoxResult.Ok
                    '                    'do nothing, continue to INSERT command below
                    '                        Case MsgBoxResult.Cancel
                    '                            Return False
                    '                    End Select
                    '                End If
                    '        End Select

                    'Dim TableHasNotnullableColumn As Boolean = False 'AS/22602c===
                    'Select Case ExternalDB_Type'AS/15285b===
                    '    Case clsDataMapping.enumMappedDBType.mdtAccess 'Access
                    '        TableHasNotnullableColumn = ExternalDB_TableHasNotnullableColumns(dm.externalColName, dm.externalMapkeyColName, dm.externalTblName)
                    '    Case Else
                    '        TableHasNotnullableColumn = ExternalDB_TableHasNotnullableColumns(dm.externalColName, dm.externalTblName)
                    'End Select 'AS/15285b==

                    'If Not TableHasNotnullableColumn Then'AS/22602c==
                    Debug.Print(dm.externalColName & ", " & mv.exValue & ", " & mv.exMapKey)
                    oCommand.CommandText = "INSERT INTO [" & dm.externalTblName & "] ([" & dm.externalColName & "], [" & dm.externalMapkeyColName & "]) VALUES (?, ?)"
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, Replace(dm.externalColName, " ", "_"), mv.exValue)) 'AS/14629d incorporated Replace to handle names with space
                    oCommand.Parameters.Add(GetDBParameter(m_ProviderType, Replace(dm.externalMapkeyColName, " ", "_"), mv.exMapKey))
                    affected = DBExecuteNonQuery(m_ProviderType, oCommand)
                    'Else 'AS/22602c===
                    'MsgBox("Cannot insert new row - null values not allowed",, "AS debug message")`
                    'End If'AS/22602c==

                    '    Case MsgBoxResult.No 'don't add new record
                    '        'debug.print(rvMsg.ToString)
                    '        Continue For 'AS/22602c 'do nothing, proceed to the next row
                    'End Select
                End If
            Catch ex As System.Exception
                'MsgBox("Export error occured: " & ex.Message,, "AS debug message") 'AS/22602c 
                Debug.Print(ex.Message) 'AS/22602d===
                'If ShowMsgbox Then 'AS/22602i===
                '    If ExternalDB_TableHasNotnullableColumns(dmConnection, dm.externalTblName) Then
                '        If ex.HResult = -2147467259 Then 'AS/22602f enclosed and added Else part
                '            sMsg = "One or more fields in the  '" & dm.externalTblName & "' table cannot contain a Null value because the Required property for this field is set to True.  Enter a value in this field. "
                '            rvMsg = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                '            ShowMsgbox = False
                '        Else
                '            sMsg = "There are one or more rows in the Data Grid that don't have Map Keys or required data. " & Chr(13)
                '            sMsg = sMsg & "They will be ignored."
                '            rvMsg = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                '            ShowMsgbox = False
                '        End If
                '    End If
                '    'Continue For 'AS/22602f
                'End If 'AS/22602d== 

                'oCommand = Nothing 'AS/22602f===
                'oCommandGetRows = Nothing
                'dbReader = Nothing

                'Return False 'AS/22602f== 'AS/22602i==

                Select Case ex.HResult 'AS/22602i===
                    Case -2147467259
                        Debug.Print(dm.externalColName & ", " & mv.exValue & ", " & mv.exMapKey & ", " & ex.HResult & "  " & ex.Message)

                        'If ShowMsgbox Then 'AS/22683===
                        '    sMsg = "One or more fields in the  '" & dm.externalTblName & "' table cannot contain a Null value because the Required property for this field is set to True.  Enter a value in this field. "
                        '    rvMsg = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                        '    ShowMsgbox = False
                        'End If 'AS/22683==

                    Case -2147217904 'Parameter ? has no default value
                        Debug.Print(dm.externalColName & ", " & mv.exValue & ", " & mv.exMapKey & ", " & ex.HResult & "  " & ex.Message)
                        'If ShowMsgbox Then 'AS/22683===
                        '    sMsg = "There are one or more rows in the Data Grid that don't have Map Keys or required data. " & Chr(13)
                        '    sMsg = sMsg & "They will be ignored."
                        '    rvMsg = MsgBox(sMsg, MsgBoxStyle.SystemModal, "Data Mapping")
                        '    ShowMsgbox = False
                        'End If 'AS/22683==

                    Case -2147467261 'object reference not set to an instance of an object
                        Debug.Print(dm.externalColName & ", " & mv.exValue & ", " & mv.exMapKey & ", " & ex.HResult & "  " & ex.Message)

                    Case Else
                        Debug.Print(dm.externalColName & ", " & mv.exValue & ", " & mv.exMapKey & ", " & ex.HResult & "  " & ex.Message)

                End Select 'AS/22602i==

            End Try

        Next

        oCommand = Nothing
        oCommandGetRows = Nothing
        dbReader = Nothing

        Return True

    End Function

    Private Function ValidDataToExport(dm As clsDataMapping, ByRef mv As clsDataMappingValue) As Boolean 'AS/16276a 'AS/22603 replaced exValue As Object with mv As clsDataMappingValue 'AS/22603 'AS/22602c 'AS/22602e added byRef for mv
        'Private Function ValidDataToExport(dm As clsDataMapping, mv As clsDataMappingValue, ByRef continueFor As Boolean) As Boolean 'AS/16276a 'AS/22603 replaced exValue As Object with mv As clsDataMappingValue 'AS/22603 'AS/22602c
        'Private Function ValidDataToExport(exColDatatype As String, exColMaxLength As String, mv As clsDataMappingValue, ByRef continueFor As Boolean) As Boolean 'AS/16276a 'AS/22603 replaced exValue As Object with mv As clsDataMappingValue 'AS/22603

        Dim sMsg As String
        Dim exValue As Object = mv.exValue 'AS/22603===
        Dim exColDatatype As String = dm.externalColDatatype
        Dim exColMaxLength As String = dm.externalColMaxLength 'AS/22603==

        Select Case ExternalDB_Type
            Case clsDataMapping.enumMappedDBType.mdtSQL
                Select Case exColDatatype 'AS/16276a=== moved to ValidDataToExport
                    Case "char", "varchar", "text", "nchar", "nvarchar", "ntext" 'Character Strings Data Types
                        If CInt(exColMaxLength) <> -1 And Len(exValue) > CInt(exColMaxLength) Then
                            sMsg = "The length of the strings to be exported must not exceed the max size of the field." & Chr(13)
                            sMsg = sMsg & "Please make sure that length of the data is less then " & exColMaxLength & " characters."
                            sMsg = sMsg & Chr(13) & "Aborting export operation."
                            MsgBox(sMsg)
                            Return False

                        End If
                    Case "bigint", "int", "smallint", "tinyint", "decimal", "numeric", "money", "smallmoney", "float", "real" 'Numeric Data Types 'AS/22602c removed "bit", made special Select for it )
                        'debug.print("IsNumeric = " & IsNumeric(exValue).ToString)
                        'HERE - ADD CHECKS FOR VALID VALUES
                        If Not IsNumeric(exValue) Then
                            'If Trim(exValue.ToString) = "" Then continueFor = True  'AS/14629l 'AS/22602c
                            If Trim(exValue.ToString) = "" Then Return False 'AS/22602c

                            'sMsg = "Cannot export a non-numeric value to the numeric column, it will be ignored. " & Chr(13) & Chr(13) 'AS/22602b===
                            'sMsg = sMsg & "Press 'OK' to continue or 'Cancel' to exit export operation."
                            'Dim rvMaxLength = MsgBox(sMsg, MsgBoxStyle.OkCancel)

                            '''debug.print("Not ValidDataToExport for: " & dm.externalColName & ",  " & mv.exMapKey & ",  " & mv.exValue.ToString) 'AS/22603
                            'Select Case rvMaxLength
                            '    Case MsgBoxResult.Ok
                            '        continueFor = True
                            '    Case MsgBoxResult.Cancel
                            '        Return False
                            'End Select 'AS/22602b==
                        End If

                    Case "bit" 'boolean 'AS/22602c===
                        If Not IsNumeric(exValue) Then
                            If exValue.ToString.ToLower = "yes" Then
                                mv.exValue = 1 'AS/22602e replaced exValue with mv.exValue
                            Else
                                mv.exValue = 0 'AS/22602e
                            End If
                        End If 'AS/22602c==

                    Case "datetime", "smalldatetime", "date", "time" 'Date and Time Data Types
                        If Not IsDate(exValue) Then
                            'continueFor = True 'AS/22602b 'AS/22602c
                            Return False 'AS/22602c
                            'sMsg = "Cannot export a non-date value to the date column, it will be ignored. " & Chr(13) & Chr(13) 'AS/22602b===
                            'sMsg = sMsg & "Press 'OK' to continue or 'Cancel' to exit export operation."
                            'Dim rvMaxLength = MsgBox(sMsg, MsgBoxStyle.OkCancel)

                            'Select Case rvMaxLength
                            '    Case MsgBoxResult.Ok
                            '        If (TypeName(exValue)).ToLower = "string" Then
                            '            continueFor = True
                            '        Else
                            '            exValue = Nothing
                            '        End If
                            '    Case MsgBoxResult.Cancel
                            '        Return False
                            'End Select 'AS/22602b==
                        End If
                    Case "binary", "varbinary", "varbinary(max)", "image" 'Binary Data Types
                        'debug.print("Binary value = " & exValue.ToString)

                    Case "ExternalDB_variant", "timestamp", "uniqueidentifier", "xml", "cursor", "table" 'Misc Data Types
                        'debug.print("Misc value = " & exValue.ToString)

                End Select 'AS/14629g== 'AS/16276a==

            Case clsDataMapping.enumMappedDBType.mdtAccess
                Select Case exColDatatype
                    Case "129", "130", "200", "201", "202", "203" 'String Data Types
                        If CInt(exColMaxLength) <> -1 And CInt(exColMaxLength) <> 0 And Len(exValue) > CInt(exColMaxLength) Then '0 - for memo field
                            sMsg = "The length of the strings to be exported must not exceed the max size of the field." & Chr(13)
                            sMsg = sMsg & "Please make sure that length of the data is less then " & exColMaxLength & " characters."
                            sMsg = sMsg & Chr(13) & "Aborting export operation."
                            MsgBox(sMsg)
                            Return False
                        End If

                    Case "2", "3", "4", "5", "6", "16", "17", "18", "19", "20", "131"  'Numeric Data Types (11 is boolean) 'AS/22602c removed "11"
                        'debug.print("IsNumeric = " & IsNumeric(exValue).ToString)
                        'HERE - ADD CHECKS FOR VALID VALUES
                        If Not IsNumeric(exValue) Then
                            'If Trim(exValue.ToString) = "" Then continueFor = True  'AS/14629l 'AS/22602c
                            Return False 'AS/22602c

                            'sMsg = "Cannot export a non-numeric value To the numeric column, it will be ignored. " & Chr(13) & Chr(13) 'AS/22602b===
                            'sMsg = sMsg & "Press 'OK' to continue or 'Cancel' to exit export operation."
                            'Dim rvMaxLength = MsgBox(sMsg, MsgBoxStyle.OkCancel)

                            'Select Case rvMaxLength
                            '    Case MsgBoxResult.Ok
                            '        continueFor = True
                            '    Case MsgBoxResult.Cancel
                            '        Return False
                            'End Select 'AS/22602b==
                        End If

                    Case "11"  'boolean 'AS/22602c===
                        If Not IsNumeric(exValue) Then
                            If exValue.ToString.ToLower = "yes" Then
                                mv.exValue = 1 'AS/22602e replaced exValue with mv.exValue
                            Else
                                mv.exValue = 0 'AS/22602e
                            End If
                        End If 'AS/22602c==

                    Case "133", "134", "135", "137" 'Date and Time Data Types
                        If Not IsDate(exValue) Then
                            'continueFor = True 'AS/22602b 'AS/22602c
                            Return False 'AS/22602c
                            'sMsg = "Cannot export a non-date value to the date column, it will be ignored. " & Chr(13) & Chr(13) 'AS/22602b===
                            'sMsg = sMsg & "Press 'OK' to continue or 'Cancel' to exit export operation."
                            'Dim rvMaxLength = MsgBox(sMsg, MsgBoxStyle.OkCancel)

                            'Select Case rvMaxLength
                            '    Case MsgBoxResult.Ok
                            '        If (TypeName(exValue)).ToLower = "string" Then
                            '            continueFor = True
                            '        Else
                            '            exValue = Nothing
                            '        End If
                            '    Case MsgBoxResult.Cancel
                            '        Return False
                            'End Select 'AS/22602b==
                        End If

                    Case "128", "201", "205", "204" 'Binary Data Types
                        'debug.print("Binary value = " & exValue.ToString)

                    Case Else 'Misc Data Types
                        If Not IsNothing(exValue) Then 'AS/21354l enclosed
                            'debug.print("Misc value = " & exValue.ToString)
                        Else 'AS/21354l===
                            Return False
                        End If 'AS/21354l

                End Select 'AS/14629g== 'AS/16276a==   

            Case clsDataMapping.enumMappedDBType.mdtOracle
               'TBD
            Case clsDataMapping.enumMappedDBType.mdtMSProject
                'TBD
            Case clsDataMapping.enumMappedDBType.mdtMSProjectServer
                'TBD
            Case Else
                'TBD
        End Select
        Return True
    End Function
    Private Function getOriginalMapkeyValue(eccMapkeyValue As String) As String 'AS/14629d

        Dim sKey As String = ""
        Dim start = InStr(eccMapkeyValue, MAPKEY_DELIMITER)
        If start > 0 Then
            sKey = Left(eccMapkeyValue, start - 1) 'AS/14629e otherwise returned 1_
        Else
            sKey = eccMapkeyValue
        End If

        Return sKey

    End Function

    Private Function getExternalColDatatype(dm As clsDataMapping, Optional getForMapkey As Boolean = False) As String 'AS/22620

        Dim dt As String = dm.externalColDatatype
        If getForMapkey Then dt = dm.externalMapcolDatatype

        Dim rv As String = ""

        Select Case ExternalDB_Type
            Case clsDataMapping.enumMappedDBType.mdtSQL
                Select Case dt
                    Case "char", "varchar", "text", "nchar", "nvarchar", "ntext" 'Character Strings Data Types
                        rv = "str"
                    Case "bigint", "int", "smallint", "tinyint", "decimal", "numeric", "money", "smallmoney", "float", "real" 'Numeric Data Types
                        rv = "num"
                    Case "bit" 'boolean
                        rv = "bool"
                    Case "datetime", "smalldatetime", "date", "time" 'Date and Time Data Types
                        rv = "date"
                    Case "binary", "varbinary", "varbinary(max)", "image" 'Binary Data Types
                        rv = "binary"
                    Case "ExternalDB_variant", "timestamp", "uniqueidentifier", "xml", "cursor", "table" 'Misc Data Types
                        rv = "misc"
                End Select

            Case clsDataMapping.enumMappedDBType.mdtAccess
                Select Case dt
                    Case "129", "130", "200", "201", "202", "203" 'String Data Types
                        rv = "str"
                    Case "2", "3", "4", "5", "6", "16", "17", "18", "19", "20", "131"  'Numeric Data Types 
                        rv = "num"
                    Case "11" 'boolean
                        rv = "bool"
                    Case "133", "134", "135", "137" 'Date and Time Data Types
                        rv = "date"
                    Case "128", "201", "205", "204" 'Binary Data Types
                        rv = "binary"
                    Case Else 'Misc Data Types
                        rv = "misc"
                End Select
            Case clsDataMapping.enumMappedDBType.mdtOracle
               'TBD
            Case clsDataMapping.enumMappedDBType.mdtMSProject
                'TBD
            Case clsDataMapping.enumMappedDBType.mdtMSProjectServer
                'TBD
            Case Else
                'TBD
        End Select
        Return rv

    End Function

    Private Function UpdateAlternativesList() As Boolean 'AS/22602h
        Dim sc As RAScenario = ProjManager.ResourceAligner.Scenarios.ActiveScenario
        For i As Integer = sc.AlternativesFull.Count - 1 To 0 Step -1
            If ProjManager.AltsHierarchy(ProjManager.ActiveAltsHierarchy).GetNodeByID(New Guid(sc.AlternativesFull(i).ID)) Is Nothing Then
                sc.AlternativesFull.RemoveAt(i)
            End If
        Next
    End Function

    'Private Function getListOfColumnsForDataInterchange(IncludeColumns As DataInterchangeInclude) As List(Of clsDataMapping) 'AS/15116e
    '    Dim lst As New List(Of clsDataMapping)

    '    Dim attributes As List(Of clsAttribute) = ProjManager.Attributes.GetAlternativesAttributes(True) ' get non-default attribute only

    '    'add Alternatives column and Cost and Risk if already mapped
    '    Dim newDM As clsDataMapping = GetDataMappingByCol(clsProjectDataProvider.dgColAltName.ToString)
    '    If newDM.DataMappingGUID <> Guid.Empty Then lst.Add(newDM)

    '    newDM = GetDataMappingByCol(ATTRIBUTE_COST_ID.ToString)
    '    If newDM.DataMappingGUID <> Guid.Empty Then lst.Add(newDM)

    '    newDM = GetDataMappingByCol(ATTRIBUTE_RISK_ID.ToString)
    '    If newDM.DataMappingGUID <> Guid.Empty Then lst.Add(newDM)

    '    'add custom attributes
    '    For Each attr As clsAttribute In attributes
    '        newDM = GetDataMappingByCol(attr.ID.ToString)
    '        If newDM.DataMappingGUID <> Guid.Empty Then lst.Add(newDM)
    '    Next

    '    Return lst
    'End Function

    Private Function Oracle_Connection() As OracleConnection 'AS/12323xw

        If _OracleConnection Is Nothing Then
            Try
                _OracleConnection = New OracleConnection(ExternalDB_ConnectionString)
                _OracleConnection.Open()
            Catch ex As System.Exception
            End Try
        Else '===
            If _OracleConnection.State = ConnectionState.Closed Then
                Try
                    _OracleConnection.Open()
                Catch ex As System.Exception
                    'debug.print(ex.Message)
                End Try
            End If '==
        End If
        Return _OracleConnection

    End Function

End Class
