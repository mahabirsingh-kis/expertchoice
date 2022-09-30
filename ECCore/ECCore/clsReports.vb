Imports Newtonsoft.Json

Namespace ECCore

    <Serializable> <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum ecReportItemType
        Unspecified = 0

        ModelDescription = 1

        Objectives = 3
        Alternatives = 5
        Participants = 6

        EvalProgress = 20

        DataGrid = 50

        ' D6011 ===
        AlternativesChart = 60
        ObjectivesChart = 61
        AltsGrid = 68
        ObjsGrid = 69

        DSA = 62
        PSA = 63
        GSA = 64
        Analysis2D = 65
        ASA = 66
        HTH = 67
        ' D6011 ==
        ProsAndCons = 70    ' D6614
        PortfolioGrid = 71
        Infodoc = 94        ' D7011
        Counter = 95        ' D6573
        'Image = 96         ' D6573
        Page = 97           ' D6573
        Space = 98          ' D6548
        PageBreak = 99      ' D6525
        NewItem = 999       ' D6605
    End Enum

    ' D6503 ===
    <Serializable> <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum ecReportType
        Document = 1
        Spreadsheet = 2
        Presentation = 4
        Dashboard = 8   ' D6548
    End Enum
    ' D6503 ==

    <Serializable> <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum ecReportsStreamType
        Unspecified = -1
        ECReport = 0    ' D6971
        ECReports = 64  ' D6971
        ECDocument = ecReportType.Document
        ECDocuments = ecReportType.Document Or 64
        ECSpreadsheet = ecReportType.Spreadsheet
        ECSpreadsheets = ecReportType.Spreadsheet Or 64
        ECPresentation = ecReportType.Presentation
        ECPresentations = ecReportType.Spreadsheet Or 64
        ECDashboard = ecReportType.Dashboard
        ECDashboards = ecReportType.Dashboard Or 64 ' D6904
        ECAllReports = 128
    End Enum
    ' D6503 ==

    ' D6548 ===
    <Serializable>
    Public Enum ecReportCategory
        All = 0
        Report = 1
        Dashboard = 2
    End Enum
    ' D6548 ==

    ' D6869 ===
    <Serializable> Public Class clsReportsMeta
        Public Type As ecReportsStreamType = ecReportsStreamType.ECAllReports
        Public Version As String = ""
        Public Title As String = ""
        Public Comment As String = ""
        Public ProjectReff As String = ""

        Public Function Serialize() As String
            Return JsonConvert.SerializeObject(Me, Formatting.None)
        End Function

        Public Shared Function Deserialize(json As String, Optional ByRef ErrorMsg As String = Nothing) As clsReportsMeta
            Dim Res As clsReportsMeta = Nothing
            Try
                'Dim Conf As New JsonSerializerSettings
                'Conf.Error = OnError()
                Res = JsonConvert.DeserializeObject(Of clsReportsMeta)(json)
                If Res IsNot Nothing AndAlso (Res.Version = "" OrElse Res.Type = ecReportsStreamType.Unspecified) Then Res = Nothing
            Catch ex As Exception
                If ErrorMsg IsNot Nothing Then ErrorMsg = ex.Message
            End Try
            Return Res
        End Function

        Public Shared Function ParseJSON(PM As clsProjectManager, sJSON As String, ByRef Data As clsReportsCollection, ByRef sError As String) As clsReportsMeta
            Dim Meta As clsReportsMeta = Nothing
            If Not String.IsNullOrEmpty(sJSON) Then
                sJSON = sJSON.TrimStart(CChar("[")).TrimEnd(CChar("]"))
                Dim sLines As String() = sJSON.Split(CChar(vbCr))
                Data = New clsReportsCollection(PM)

                If sLines.Count > 1 Then
                    Meta = clsReportsMeta.Deserialize(sLines(0).TrimEnd(CChar(",")), sError)
                    ' D6884 ===
                    If Meta IsNot Nothing AndAlso String.IsNullOrEmpty(sError) Then
                        sJSON = sJSON.Substring(sLines(0).Length)
                        sJSON = sJSON.TrimEnd(CChar("]"))   ' cut terminal "]" symbol
                    End If
                    ' D6884 ==
                End If
                If Meta Is Nothing Then
                    Meta = New clsReportsMeta()
                    Meta.Type = ecReportsStreamType.Unspecified
                    sError = ""
                End If

                sJSON = sJSON.Trim(CChar(vbCr)).Trim(CChar(vbLf)).Trim()

                Dim fParsed As Boolean = False
                If String.IsNullOrEmpty(sError) Then

                    Select Case Meta.Type
                        Case ecReportsStreamType.ECAllReports, ecReportsStreamType.ECDashboards, ecReportsStreamType.ECDocuments, ecReportsStreamType.ECPresentations, ecReportsStreamType.ECReports, ecReportsStreamType.ECSpreadsheets, ecReportsStreamType.Unspecified
                            Data.Load(sJSON)
                            If Data.LastError <> "" Then sError = Data.LastError
                            If String.IsNullOrEmpty(sError) AndAlso Data IsNot Nothing AndAlso Data.Reports IsNot Nothing AndAlso Data.Reports.Count > 0 Then
                                If Meta.Type = ecReportsStreamType.Unspecified Then Meta.Type = ecReportsStreamType.ECAllReports
                                fParsed = True
                            End If
                    End Select

                    If Not fParsed Then
                        Dim tmpReport As New clsReport(ecReportType.Document, "")
                        Select Case Meta.Type
                            Case ecReportsStreamType.ECDashboard, ecReportsStreamType.ECDocument, ecReportsStreamType.ECPresentation, ecReportsStreamType.ECReport, ecReportsStreamType.ECSpreadsheet, ecReportsStreamType.Unspecified
                                tmpReport = clsReport.Deserialize(sJSON, sError)  ' D6884
                                If String.IsNullOrEmpty(sError) AndAlso tmpReport IsNot Nothing AndAlso (tmpReport.Name <> "" OrElse tmpReport.Items.Count > 0) Then
                                    Data = New clsReportsCollection(PM)
                                    Data.Reports.Add(tmpReport.ID, tmpReport)
                                    Meta.Type = CType(tmpReport.ReportType, ecReportsStreamType)
                                    fParsed = True
                                End If
                        End Select
                    End If
                End If
            End If
            Return Meta
        End Function

    End Class
    ' D6869 ==

    <Serializable> Public Class clsReportsCollection

        Public Shared ActualVersion As String = "1.0.1"     ' D6548 + D6869

        'Public Version As String = ActualVersion    ' D6548 -D6869
        Public Reports As Dictionary(Of Integer, clsReport) = Nothing

        Friend ProjectManager As clsProjectManager = Nothing
        Private _LastError As String = ""

        Public Function GetNextReportID() As Integer    ' D6886
            Return If(Reports IsNot Nothing AndAlso Reports.Count > 0, Reports.Keys.Max() + 1, 1)   ' D6503
        End Function

        Public Function AddReport(ReportType As ecReportType, Name As String, Optional Comment As String = "") As clsReport ' D6521
            If Reports Is Nothing Then Return Nothing   ' D6503
            Dim tReport As New clsReport(ReportType, Name, Comment) ' D6521
            With tReport
                .ID = GetNextReportID()
            End With
            Reports.Add(tReport.ID, tReport)
            Return tReport
        End Function

        Public Function DeleteReport(ID As Integer) As Boolean
            If Reports IsNot Nothing AndAlso Reports.ContainsKey(ID) Then   ' D6503
                Return Reports.Remove(ID)
            Else
                Return False
            End If
        End Function

        Public Function CloneReport(ID As Integer) As clsReport
            If Reports IsNot Nothing AndAlso Reports.ContainsKey(ID) Then   ' D6503
                Dim newReport As clsReport = Reports(ID).Clone()
                If newReport IsNot Nothing Then
                    newReport.ID = GetNextReportID()
                    Reports.Add(newReport.ID, newReport)
                    Return newReport
                End If
            End If
            Return Nothing
        End Function

        Public Function Load(Optional json As String = Nothing) As Boolean
            _LastError = ""
            If Reports Is Nothing Then Reports = New Dictionary(Of Integer, clsReport) Else Reports.Clear() ' D6503
            If String.IsNullOrEmpty(json) AndAlso ProjectManager IsNot Nothing Then json = ProjectManager.Parameters.ProjectReports ' D6900
            If Not String.IsNullOrEmpty(json) Then
                Dim Reports As clsReportsCollection = clsReportsCollection.Deserialize(json, _LastError)
                If Reports IsNot Nothing Then
                    With Reports
                        'Me.Version = .Version  ' -D6869
                        If Me.Reports IsNot Nothing Then
                            Me.Reports = .Reports
                            For Each ID As Integer In Me.Reports.Keys   ' Check for a "null" sub-collections
                                If Me.Reports(ID).Options Is Nothing Then Me.Reports(ID).Options = New Dictionary(Of String, Object)    ' D6574
                                If Me.Reports(ID).Items Is Nothing Then
                                    Me.Reports(ID).Items = New Dictionary(Of Integer, clsReportItem)
                                    ' D6503 ===
                                Else
                                    For Each ItemID As Integer In Me.Reports(ID).Items.Keys
                                        If Me.Reports(ID).Items(ItemID).ItemOptions Is Nothing Then Me.Reports(ID).Items(ItemID).ItemOptions = New Dictionary(Of String, Object)            ' D6529
                                        If Me.Reports(ID).Items(ItemID).ContentOptions Is Nothing Then Me.Reports(ID).Items(ItemID).ContentOptions = New Dictionary(Of String, Object)      ' D6548
                                        ' D7302 ===
                                        If Not String.IsNullOrEmpty(Me.Reports(ID).Items(ItemID).EditURL) AndAlso Not Me.Reports(ID).Items(ItemID).EditURL.StartsWith("/") AndAlso Not Me.Reports(ID).Items(ItemID).EditURL.Contains(":") Then
                                            Me.Reports(ID).Items(ItemID).EditURL = "/" + Me.Reports(ID).Items(ItemID).EditURL
                                        End If
                                        ' D7302 ==
                                    Next
                                    Me.Reports(ID).SortItemsByIndex()   ' D6503
                                End If
                                ' D6503 ==
                            Next
                        End If
                    End With
                End If
            End If
            Return String.IsNullOrEmpty(_LastError)
        End Function

        Public Function Save(Optional json As String = Nothing, Optional SaveToDatabase As Boolean = True) As Boolean
            If ProjectManager IsNot Nothing Then
                If String.IsNullOrEmpty(json) Then json = Serialize()
                ProjectManager.Parameters.ProjectReports = json
                If SaveToDatabase Then Return ProjectManager.Parameters.Save Else Return True
            End If
            Return False
        End Function

        ' D6899 ===
        Public Function AddToCollection(SrcReports As clsReportsCollection, Category As ecReportCategory) As List(Of clsReport)
            Dim tRes As New List(Of clsReport)
            If SrcReports IsNot Nothing Then
                Dim Dashs As Dictionary(Of Integer, clsReport) = SrcReports.ByCategory(Category)
                If Dashs IsNot Nothing AndAlso Dashs.Count > 0 Then
                    For Each ID As Integer In Dashs.Keys
                        Dim tRep As clsReport = Dashs(ID)
                        tRep.ID = GetNextReportID()
                        Reports.Add(tRep.ID, tRep)
                        tRes.Add(tRep)
                    Next
                End If
            End If
            Return tRes
        End Function
        ' D6899 ==

        ' D6907 ===
        Public Function AddToReport(SrcReports As clsReportsCollection, ByRef DestReport As clsReport, Category As ecReportCategory) As List(Of clsReportItem)
            Dim tRes As New List(Of clsReportItem)
            If SrcReports IsNot Nothing And DestReport IsNot Nothing Then
                Dim Dashs As Dictionary(Of Integer, clsReport) = SrcReports.ByCategory(Category)
                If Dashs IsNot Nothing AndAlso Dashs.Count > 0 Then
                    For Each ID As Integer In Dashs.Keys
                        Dim tRep As clsReport = Dashs(ID)
                        For Each ItemID As Integer In tRep.Items.Keys
                            Dim tItem As clsReportItem = tRep.Items(ItemID).Clone
                            tItem.ID = DestReport.GetNextItemID()
                            DestReport.Items.Add(tItem.ID, tItem)
                            tRes.Add(tItem)
                        Next
                    Next
                End If
            End If
            Return tRes
        End Function
        ' D6907 ==

        Public Function LastError() As String
            Return _LastError
        End Function

        Public Function Serialize() As String   ' D6511
            'Version = ActualVersion ' D6548 -D6869
            Return JsonConvert.SerializeObject(Me, Formatting.None) ' D6871
        End Function

        Shared Function Deserialize(json As String, Optional ByRef ErrorMsg As String = Nothing) As clsReportsCollection    ' D6511
            Try
                Dim settings As New JsonSerializerSettings
                settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                settings.DefaultValueHandling = DefaultValueHandling.Ignore
                settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                settings.MissingMemberHandling = MissingMemberHandling.Ignore
                settings.NullValueHandling = NullValueHandling.Ignore
                settings.TypeNameHandling = TypeNameHandling.Auto
                Return JsonConvert.DeserializeObject(Of clsReportsCollection)(json, settings)
            Catch ex As Exception
                If ErrorMsg IsNot Nothing Then ErrorMsg = ex.Message
                Return Nothing
            End Try
        End Function

        ' D6869 ===
        Public Function ByCategory(Category As ecReportCategory) As Dictionary(Of Integer, clsReport)
            Dim SrcList As New Dictionary(Of Integer, clsReport)
            If Category = ecReportCategory.All Then
                SrcList = Reports
            Else
                SrcList = New Dictionary(Of Integer, clsReport)
                For Each id As Integer In Reports.Keys
                    Dim report As clsReport = Reports(id)
                    Dim accept As Boolean = False
                    Select Case Category
                        Case ecReportCategory.Dashboard
                            accept = report.ReportType = ecReportType.Dashboard
                        Case ecReportCategory.Report
                            accept = report.ReportType <> ecReportType.Dashboard
                        Case Else
                            accept = True
                    End Select
                    If (accept) Then SrcList.Add(id, report)
                Next
            End If
            Return SrcList
        End Function

        Public Function ByCategoryAsCollection(Category As ecReportCategory) As clsReportsCollection
            Dim tRes As New clsReportsCollection(ProjectManager)
            With tRes
                .Reports = ByCategory(Category)
            End With
            Return tRes
        End Function

        ' D6869 ==

        Public Function Clone() As clsReportsCollection
            Return clsReportsCollection.Deserialize(Me.Serialize())
        End Function

        Public Sub New(PM As clsProjectManager)
            Reports = New Dictionary(Of Integer, clsReport)
            ProjectManager = PM
        End Sub

    End Class


    <Serializable> Public Class clsReport

        Public ID As Integer = 0
        Public Name As String = ""
        Public Comment As String = ""
        Public ReportType As ecReportType = ecReportType.Document
        Public Options As Dictionary(Of String, Object) = Nothing           ' D6574
        Public Items As Dictionary(Of Integer, clsReportItem) = Nothing

        Public Function GetNextItemID() As Integer  ' D6886
            Return If(Items IsNot Nothing AndAlso Items.Count > 0, Items.Keys.Max() + 1, 1) ' D6503
        End Function

        ' D6503 ===
        Public Function SortItemsByIndex() As Integer
            Dim index As Integer = 0
            If Items IsNot Nothing AndAlso Items.Count > 0 Then
                Dim sorted As IEnumerable(Of clsReportItem) = SortedByIndex()   ' DD6506
                For Each tItem As clsReportItem In sorted
                    index += 1
                    Items(tItem.ID).Index = index
                Next
            End If
            Return index
        End Function
        ' D6503 ==

        ' D6506 ===
        Public Function SortedByIndex() As IEnumerable(Of clsReportItem)
            Return Items?.Values.OrderBy(Function(v) v.Index)
        End Function
        ' D6506 ==

        Public Function AddItem(ItemType As ecReportItemType, Name As String, Optional Comment As String = "", Optional Index As Integer = -1) As clsReportItem   ' D6503 + D6522
            If Items Is Nothing Then Return Nothing ' D6503
            'Dim tItem As New clsReportItem(ItemType, Name, Comment)
            Dim tItem As New clsReportItem(ItemType.ToString, Name, Comment)
            With tItem
                .ID = GetNextItemID()
                .Index = If(Index < 0, Items.Count + 1, Index)  ' D6503
            End With
            Items.Add(tItem.ID, tItem)
            SortItemsByIndex()  ' D6503
            Return tItem
        End Function

        Public Function CloneItem(ID As Integer) As clsReportItem
            If Items IsNot Nothing AndAlso Items.ContainsKey(ID) Then   ' D6503
                Dim newItem As clsReportItem = Items(ID).Clone()
                If newItem IsNot Nothing Then
                    newItem.ID = GetNextItemID()
                    Items.Add(newItem.ID, newItem)
                    SortItemsByIndex()  ' D6578
                    Return newItem
                End If
            End If
            Return Nothing
        End Function

        Public Function Serialize() As String   ' D6521
            Return JsonConvert.SerializeObject(Me, Formatting.None) ' D6870
        End Function

        Shared Function Deserialize(json As String, Optional ByRef ErrorMsg As String = Nothing) As clsReport   ' D6521
            Try
                Return JsonConvert.DeserializeObject(Of clsReport)(json)
            Catch ex As Exception
                If ErrorMsg IsNot Nothing Then ErrorMsg = ex.Message
                Return Nothing
            End Try
        End Function

        Public Function Clone() As clsReport
            Return clsReport.Deserialize(Me.Serialize())
        End Function

        Public Sub New(ReportType As ecReportType, Name As String, Optional Comment As String = "") ' D6503
            Me.ReportType = ReportType  ' D6503
            Me.Name = Name
            Me.Comment = Comment
            Options = New Dictionary(Of String, Object) ' D6574
            Items = New Dictionary(Of Integer, clsReportItem)
        End Sub

    End Class


    <Serializable> Public Class clsReportItem

        Public ID As Integer = 0
        Public Name As String = ""
        Public Comment As String = ""
        'Public ItemType As ecReportItemType = ecReportItemType.Unspecified
        Public ItemType As String = ecReportItemType.Unspecified.ToString   ' D7020
        Public Index As Integer = 0

        Public Disabled As Boolean = False
        Public ItemOptions As Dictionary(Of String, Object) = Nothing       ' D6503 + D6529 + D6548
        Public ContentOptions As Dictionary(Of String, Object) = Nothing    ' D6548

        Public PageID As Integer = 0
        Public EditURL As String = ""
        Public ExportURL As String = ""

        Public Function Serialize() As String   ' D6521
            Return JsonConvert.SerializeObject(Me, Formatting.None) ' D6870
        End Function

        Shared Function Deserialize(json As String, Optional ByRef ErrorMsg As String = Nothing) As clsReportItem   ' D6521
            Try
                Return JsonConvert.DeserializeObject(Of clsReportItem)(json)
            Catch ex As Exception
                If ErrorMsg IsNot Nothing Then ErrorMsg = ex.Message
                Return Nothing
            End Try
        End Function

        Public Function Clone() As clsReportItem
            Return clsReportItem.Deserialize(Me.Serialize())
        End Function

        'Public Sub New(ItemType As ecReportItemType, Name As String, Optional Comment As String = "")
        Public Sub New(ItemType As String, Name As String, Optional Comment As String = "")
            Me.ItemType = ItemType
            Me.Name = Name
            Me.Comment = Comment
            ItemOptions = New Dictionary(Of String, Object)     ' D6503 + D6529
            ContentOptions = New Dictionary(Of String, Object)  ' D6548
        End Sub

    End Class

End Namespace
