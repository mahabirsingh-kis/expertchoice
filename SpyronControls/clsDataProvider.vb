Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.Odbc
Imports System.IO
Imports System.Web.Hosting
Imports SpyronControls.Spyron.Core
Imports ExpertChoice.Database

'L0067 ===
Namespace Spyron.DataSources

    Public Class clsSurveysResultsDS
        Public Survey As clsSurvey
        Public Sub New(ByRef ASurvey As clsSurvey)
            Survey = ASurvey
        End Sub
        Public Function SelectAll() As DataTable
            Dim ds As DataSet
            Dim keys(1) As DataColumn
            ds = New DataSet()
            ds.Tables.Add("Answers")
            ds.Tables("Answers").Columns.Add("ID", Type.GetType("System.Int32"))
            ds.Tables("Answers").Columns.Add("RespondentName", Type.GetType("System.String"))
            ds.Tables("Answers").Columns.Add("QuestionText", Type.GetType("System.String"))
            ds.Tables("Answers").Columns.Add("AnswerText", Type.GetType("System.String"))
            ds.Tables("Answers").Columns.Add("Selected", Type.GetType("System.Int32"))
            keys(0) = ds.Tables("Answers").Columns("ID")
            ds.Tables("Answers").PrimaryKey = keys
            Dim i As Integer = 1
            For Each Respondent As clsRespondent In Survey.Respondents
                For Each Page As clsSurveyPage In Survey.Pages
                    For Each Question As clsQuestion In Page.Questions
                        For Each AnswerVariant As clsVariant In Question.Variants
                            If Not Respondent.AnswerByQuestionGUID(Question.AGUID) Is Nothing Then
                                If Respondent.AnswerByQuestionGUID(Question.AGUID).AnswerVariants.Contains(AnswerVariant) Then
                                    ds.Tables("Answers").Rows.Add(i, Respondent.Name, Question.Text, AnswerVariant.Text, 1)
                                Else
                                    ds.Tables("Answers").Rows.Add(i, Respondent.Name, Question.Text, AnswerVariant.Text, 0)
                                End If
                                i += 1
                            End If
                        Next
                    Next
                Next
            Next
            Return ds.Tables("Answers")
        End Function
    End Class

    'L0071 ===
    Public Class clsStatisticResultsDS
        Public Survey As clsSurvey
        Public Sub New(ByRef ASurvey As clsSurvey)
            Survey = ASurvey
        End Sub

        Public Function SelectAll(ByVal GroupFilterGUID As String) As DataTable
            Dim ds As DataSet
            ds = New DataSet()
            ds.Tables.Add("StatisticResults")
            ds.Tables("StatisticResults").Columns.Add("Page", Type.GetType("System.String")) 'L0444
            ds.Tables("StatisticResults").Columns.Add("Question", Type.GetType("System.String"))
            ds.Tables("StatisticResults").Columns.Add("Answer", Type.GetType("System.String"))
            ds.Tables("StatisticResults").Columns.Add("Percent", Type.GetType("System.Double"))
            ds.Tables("StatisticResults").Columns.Add("RespondentsCount", Type.GetType("System.Int32"))
            ds.Tables("StatisticResults").Columns.Add("QGUID", Type.GetType("System.String"))

            If Survey IsNot Nothing Then
                Dim QuestionIndex As Integer = 0 'L0444
                Dim PageIndex As Integer = 0 'L0444
                For Each APage As clsSurveyPage In Survey.Pages
                    PageIndex += 1 'L0444
                    For Each AQuestion As clsQuestion In APage.Questions

                        If AQuestion.Type <> QuestionType.qtAlternativesSelect And _
                           AQuestion.Type <> QuestionType.qtObjectivesSelect And _
                           AQuestion.Type <> QuestionType.qtComment Then

                            If (AQuestion.Type = QuestionType.qtVariantsRadio) Or _
                               (AQuestion.Type = QuestionType.qtVariantsCombo) Or _
                               (AQuestion.Type = QuestionType.qtVariantsCheck) Or _
                               (AQuestion.Type = QuestionType.qtImageCheck) Then
                                QuestionIndex += 1 'L0444
                                'L0486 ===
                                Dim ARespondentCounter As Integer = 0
                                For Each ARespondent As clsRespondent In Survey.Respondents
                                    Dim ShowRow As Boolean = True
                                    If ShowRow Then
                                        Dim RespondentAnswer As clsAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                                        If RespondentAnswer IsNot Nothing Then
                                            If RespondentAnswer.AnswerVariants.Count > 0 Then
                                                ARespondentCounter += 1
                                            End If
                                        End If
                                    End If
                                Next
                                'Next
                                'L0486 ==

                                For Each AVariant As clsVariant In AQuestion.Variants

                                    Dim AVariantCounter As Integer = 0
                                    For Each ARespondent As clsRespondent In Survey.Respondents
                                        Dim RespondentAnswer As clsAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                                        If RespondentAnswer IsNot Nothing Then
                                            For Each AAVariant As clsVariant In RespondentAnswer.AnswerVariants
                                                If AAVariant.AGUID = AVariant.AGUID Then
                                                    If Not ((AAVariant.Type = Core.VariantType.vtOtherLine Or AAVariant.Type = Core.VariantType.vtOtherMemo) And AAVariant.VariantValue.Trim() = "") Then
                                                        AVariantCounter += 1
                                                    End If

                                                End If
                                            Next
                                        End If
                                    Next
                                    'Next

                                    If ARespondentCounter > 0 Then 'L0486
                                        'Dim Percent As Double = AVariantCounter / Survey.RespondentsCount 'L0486 -
                                        Dim Percent As Double = AVariantCounter / ARespondentCounter 'L0486
                                        ds.Tables("StatisticResults").Rows.Add(String.Format("{0}. {1}", PageIndex.ToString("00"), APage.Title), String.Format("{0}. {1}", QuestionIndex.ToString("000"), AQuestion.Text), AVariant.Text, Percent, AVariantCounter, AQuestion.AGUID.ToString) 'L0444
                                    End If
                                Next

                            Else

                            End If
                        End If
                    Next
                Next
            End If
            Return ds.Tables("StatisticResults")
        End Function
    End Class
    'L0071 ==

    'L0042 ===
    Public Class clsRespondentAnswersDS

        Public Survey As clsSurvey
        Public Sub New(ByRef ASurvey As clsSurvey)
            Survey = ASurvey
        End Sub

        Public Function SelectAll(ByVal GroupFilterGUID As String) As DataTable
            Dim ds As DataSet
            Dim keys(1) As DataColumn
            ds = New DataSet()
            ds.Tables.Add("RespondentAnswers")
            ds.Tables("RespondentAnswers").Columns.Add("ID", Type.GetType("System.Int32")) 'L0058
            ds.Tables("RespondentAnswers").Columns.Add("RespondentName", Type.GetType("System.String"))
            ds.Tables("RespondentAnswers").Columns.Add("RespondentEmail", Type.GetType("System.String"))

            If Survey IsNot Nothing Then


                For Each APage As clsSurveyPage In Survey.Pages
                    ds.Tables("RespondentAnswers").Columns.Add("Page_" + APage.AGUID.ToString, Type.GetType("System.String")) 'L0444 'L0448
                    For Each AQuestion As clsQuestion In APage.Questions
                        If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtObjectivesSelect Then 'L0067
                            ds.Tables("RespondentAnswers").Columns.Add("Question_" + AQuestion.AGUID.ToString, Type.GetType("System.String"))
                        End If
                    Next
                Next
                ds.Tables("RespondentAnswers").Columns.Add("TotalAnswers", Type.GetType("System.Int32")) 'L0346
                keys(0) = ds.Tables("RespondentAnswers").Columns("ID")
                ds.Tables("RespondentAnswers").PrimaryKey = keys
                Dim i As Integer = 1

                Dim ADataRow(ds.Tables("RespondentAnswers").Columns.Count - 1) As Object
                For Each Respondent As clsRespondent In Survey.Respondents
                    If Respondent.Answers.Count > 0 Then
                        ADataRow.SetValue(i, 0)
                        ADataRow.SetValue(Respondent.Name, 1)
                        ADataRow.SetValue(Respondent.Email, 2)
                        Dim j As Integer = 3
                        For Each Page As clsSurveyPage In Survey.Pages
                            Dim QuestionColIndex As Integer = j 'L0444
                            j += 1 'L0444
                            For Each Question As clsQuestion In Page.Questions
                                If Question.Type <> QuestionType.qtComment And Question.Type <> QuestionType.qtObjectivesSelect Then 'L0067
                                    Dim AVALUESSTR As String = ""
                                    Dim TIMESTAMP As Nullable(Of DateTime) = Nothing
                                    For Each RA As clsAnswer In Respondent.Answers
                                        If RA.Question.AGUID = Question.AGUID Then
                                            TIMESTAMP = RA.AnswerDate
                                            For Each RAV As clsVariant In RA.AnswerVariants
                                                If RAV.Text = RAV.VariantValue Then
                                                    If AVALUESSTR = "" Then
                                                        AVALUESSTR = RAV.Text
                                                    Else
                                                        AVALUESSTR += ", " + RAV.Text
                                                    End If
                                                Else
                                                    If Not ((RAV.Type = Core.VariantType.vtOtherLine Or RAV.Type = Core.VariantType.vtOtherMemo) And RAV.VariantValue = "") Then
                                                        If AVALUESSTR = "" Then
                                                            If RAV.Text = "" Then
                                                                AVALUESSTR = RAV.VariantValue
                                                            Else
                                                                AVALUESSTR = RAV.Text + " " + RAV.VariantValue
                                                            End If

                                                        Else
                                                            AVALUESSTR += ", " + RAV.Text + " " + RAV.VariantValue
                                                        End If
                                                    End If
                                                End If

                                            Next
                                        End If
                                    Next
                                    Dim TIMESTAMPStr As String = ""
                                    If TIMESTAMP.HasValue Then TIMESTAMPStr = TIMESTAMP.ToString
                                    ADataRow.SetValue(AVALUESSTR, j) 'L0346 'L0355 'L0444
                                    If TIMESTAMPStr <> "" Then ADataRow.SetValue(TIMESTAMPStr, QuestionColIndex) 'L0444
                                    j += 1
                                End If
                            Next
                        Next
                        ADataRow.SetValue(Respondent.Answers.Count, j) 'L0346
                        ds.Tables("RespondentAnswers").Rows.Add(ADataRow)
                        i += 1
                    End If
                Next
            End If
            Return ds.Tables("RespondentAnswers")
        End Function
    End Class
    'L0042 ==

    'L0058 ===
    Public Class clsGroupFilterDS
        Public Survey As clsSurvey
        Public Sub New(ByRef ASurvey As clsSurvey)
            Survey = ASurvey
        End Sub
        Public Function SelectAll() As DataTable
            Dim ds As DataSet
            Dim keys(1) As DataColumn
            ds = New DataSet()
            ds.Tables.Add("GroupFilter")
            ds.Tables("GroupFilter").Columns.Add("RespondentName", Type.GetType("System.String"))
            ds.Tables("GroupFilter").Columns.Add("RespondentEmail", Type.GetType("System.String"))

            For Each APage As clsSurveyPage In Survey.Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.Type <> QuestionType.qtComment Then
                        ds.Tables("GroupFilter").Columns.Add("Question_" + AQuestion.AGUID.ToString, Type.GetType("System.String"))
                    End If
                Next
            Next

            keys(0) = ds.Tables("GroupFilter").Columns("RespondentEmail")
            ds.Tables("GroupFilter").PrimaryKey = keys
            Dim i As Integer = 1

            Dim ADataRow(ds.Tables("GroupFilter").Columns.Count - 1) As Object
            For Each Respondent As clsRespondent In Survey.Respondents
                If Respondent.Answers.Count > 0 Then
                    ADataRow.SetValue(Respondent.Name, 0)
                    ADataRow.SetValue(Respondent.Email, 1)
                    Dim j As Integer = 2
                    For Each Page As clsSurveyPage In Survey.Pages
                        For Each Question As clsQuestion In Page.Questions
                            If Question.Type <> QuestionType.qtComment Then
                                If Respondent.AnswerByQuestionGUID(Question.AGUID) IsNot Nothing Then
                                    ADataRow.SetValue(Respondent.AnswerByQuestionGUID(Question.AGUID).AnswerValuesString, j)
                                Else
                                    ADataRow.SetValue("", j)
                                End If
                                j += 1
                            End If
                        Next
                    Next
                    ds.Tables("GroupFilter").Rows.Add(ADataRow)
                    i += 1
                End If
            Next
            Return ds.Tables("GroupFilter")
        End Function
    End Class
    'L0058 ==

    Public Class clsRespondentsDS
        Public Survey As clsSurvey
        Public Sub New(ByRef ASurvey As clsSurvey)
            Survey = ASurvey
        End Sub

        Public Function SelectAll() As DataTable
            Dim ds As DataSet
            Dim keys(1) As DataColumn
            ds = New DataSet()
            ds.Tables.Add("Respondents")
            ds.Tables("Respondents").Columns.Add("ID", Type.GetType("System.String")) 'L0038
            ds.Tables("Respondents").Columns.Add("RespondentEmail", Type.GetType("System.String"))
            ds.Tables("Respondents").Columns.Add("RespondentName", Type.GetType("System.String"))
            ds.Tables("Respondents").Columns.Add("GivenAnswers", Type.GetType("System.Int32"))
            keys(0) = ds.Tables("Respondents").Columns("ID")
            ds.Tables("Respondents").PrimaryKey = keys

            For Each Respondent As clsRespondent In Survey.Respondents
                ds.Tables("Respondents").Rows.Add(Respondent.AGUID.ToString, Respondent.Email, Respondent.Name, Respondent.Answers.Count) 'L0038
            Next
            Return ds.Tables("Respondents")
        End Function
    End Class
End Namespace
'L0067 ==

Namespace Spyron.Data

    <Serializable()> Public Class clsSurveyDataProvider

        Public _FILE_ROOT As String = HostingEnvironment.ApplicationPhysicalPath

        ' supporting variables for operating with stream data
        <NonSerialized()> Private fStream As MemoryStream 'L0020
        <NonSerialized()> Private fBW As BinaryWriter 'L0020
        <NonSerialized()> Private fBR As BinaryReader 'L0020

        Const emInvalidDBVersion = "Survey DB Version is {0}, expected {1}, please update Comparion to latest version to use this Survey"
        Const emUnknownDBError = "Unknown error during reading Survey from {0}"

        Private fSurveyInfo As clsSurveyInfo

        Private fSurvey As New clsSurvey()

        Public CurrentDBVersion As New Version(1, 2)

        Public ReadOnly Property SurveyInfo() As clsSurveyInfo
            Get
                Return fSurveyInfo
            End Get
        End Property

        Public Property Survey() As clsSurvey
            Get
                Return fSurvey
            End Get
            Set(ByVal value As clsSurvey)
                fSurvey = value
            End Set
        End Property

        ''' <summary>
        ''' Try to open and read all content for specified file
        ''' </summary>
        ''' <param name="sFName">Filename for reading</param>
        ''' <returns>String with file content or just a error message.</returns>
        ''' <remarks></remarks>
        Public Function File_GetContent(ByVal sFName As String) As String
            Dim sResult As String = ""
            Try
                Dim sr As IO.StreamReader = New IO.StreamReader(sFName)
                sResult = sr.ReadToEnd
                sr.Close()
            Catch ex As Exception
                sResult = ""
            End Try
            Return sResult
        End Function

        ''' <summary>
        ''' Trying to open and read Survey structures from specified Storage Type.
        ''' Read data putted to the Survey property.
        ''' </summary>
        ''' <returns>True - if reading was successful</returns>
        ''' <remarks></remarks>
        Public Function OpenSurvey(ByVal ParticipantEmail As String) As Boolean 'L0442

            Select Case SurveyInfo.StorageType
                Case SurveyStorageType.sstDatabaseStream    ' D0376
                    Return OpenStreamProjectFromDatabase_Old(ParticipantEmail) 'L0022
                Case SurveyStorageType.sstFileStream 'L0022
                    Return OpenStreamProjectFromFile_Old() 'L0022

                Case SurveyStorageType.sstECCDatabaseStream_v1
                    Return OpenStreamProjectFromDatabase(ParticipantEmail)
                Case SurveyStorageType.sstFileStream_v1
                    Return OpenStreamProjectFromFile()
            End Select

        End Function

        ''' <summary>
        ''' Saves Survey structures to specified Storage Type.
        ''' </summary>
        ''' <returns>True - if saving was successful</returns>
        ''' <remarks>Data saves using Streams only. Old regular Database storage not supported.</remarks>
        Public Function SaveSurvey(ByVal ForceSaveRespondentData As Boolean) As Boolean 'L0441
            Select Case SurveyInfo.StorageType
                Case SurveyStorageType.sstDatabaseStream, SurveyStorageType.sstECCDatabaseStream_v1
                    Return SaveStreamProjectToDatabase(ForceSaveRespondentData)  ' D0380 'L0441
                Case SurveyStorageType.sstFileStream, SurveyStorageType.sstFileStream_v1
                    Return SaveStreamProjectToFile() 'L0022
                Case Else
                    Return False
            End Select
        End Function

        ' D1135 ===
        Public Sub LoadRespondentsInfo(ByVal oCommand As DbCommand)
            'Read RespondentGroups
            fStream = New MemoryStream()
            fBW = New BinaryWriter(fStream)
            fBR = New BinaryReader(fStream)
            LoadStreamFromDatabase(StreamDataType.sdtRespondentsInfo, oCommand)
            Survey.Respondents.Clear()
            ReadRespondentsInfoFromStream()
        End Sub
        ' D1135 ==

        ''' <summary>
        ''' Trying to open and read Survey structures from Streamed Database.
        ''' </summary>
        ''' <returns>True - if reading was successful</returns>
        ''' <remarks></remarks>
        Public Function OpenStreamProjectFromDatabase_Old(ByVal ParticipantEmail As String) As Boolean 'L0022 'L0442
            Survey = New clsSurvey

            Using DBConn As DbConnection = GetDBConnection(SurveyInfo.ProviderType, SurveyInfo.ConnectionString) ' D0350 + D2232
                Try
                    DBConn.Open()

                    Dim oCommand As DbCommand = GetDBCommand(SurveyInfo.ProviderType)
                    oCommand.Connection = DBConn

                    'Read Survey Properties
                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)
                    fBR = New BinaryReader(fStream)
                    LoadStreamFromDatabase(StreamDataType.sdtSurveyProperties, oCommand)
                    ReadSurveyPropertiesFromStream()

                    'Read Survey Pages
                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)
                    fBR = New BinaryReader(fStream)
                    LoadStreamFromDatabase(StreamDataType.sdtSurveyStructure, oCommand)
                    ReadPagesFromStream()

                    'Read Rules

                    ''Read RespondentGroups
                    LoadRespondentsInfo(oCommand)   ' D1135

                    For Each ARespondent As clsRespondent In Survey.Respondents
                        If ARespondent.Email = ParticipantEmail Or ParticipantEmail = "" Then 'L0442
                            fStream = New MemoryStream()
                            fBW = New BinaryWriter(fStream)
                            fBR = New BinaryReader(fStream)
                            LoadStreamFromDatabase(StreamDataType.sdtRespondentsData, oCommand, ARespondent.AGUID.ToString)
                            ReadRespondentsDataFromStream(ARespondent)
                        End If
                    Next

                    DBConn.Close()
                    Return True 'L0022
                Catch ex As Exception
                    Return False 'L0022
                End Try
            End Using

        End Function

        ''' <summary>
        ''' Trying to open and read Survey structures from Streamed ECC Database.
        ''' </summary>
        ''' <returns>True - if reading was successful</returns>
        ''' <remarks></remarks>
        Public Function OpenStreamProjectFromDatabase(ByVal ParticipantEmail As String) As Boolean
            Survey = New clsSurvey

            Using DBConn As DbConnection = GetDBConnection(SurveyInfo.ProviderType, SurveyInfo.ConnectionString) ' D0350 + D2232
                Try
                    DBConn.Open()

                    Dim oCommand As DbCommand = GetDBCommand(SurveyInfo.ProviderType)
                    oCommand.Connection = DBConn

                    'Read Survey DB Version
                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)
                    fBR = New BinaryReader(fStream)
                    LoadStreamFromDatabase(StreamDataType.stSpyronModelVersion, oCommand, "", SurveyInfo.ProjectID)
                    ReadSurveyDBVersionFromStream()

                    'If SurveyInfo.DBVersion > CurrentDBVersion Then
                    '    SurveyInfo.ErrorMessage = String.Format(emInvalidDBVersion, SurveyInfo.DBVersion, CurrentDBVersion)
                    '    Return False
                    'End If


                    'Read Survey Structure
                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)
                    fBR = New BinaryReader(fStream)

                    Dim AStreamDataType As StreamDataType = StreamDataType.stSpyronStructureWelcome
                    Select Case SurveyInfo.SurveyType
                        Case SurveyType.stWelcomeSurvey
                            AStreamDataType = StreamDataType.stSpyronStructureWelcome
                        Case SurveyType.stThankyouSurvey
                            AStreamDataType = StreamDataType.stSpyronStructureThankYou
                        Case SurveyType.stImpactWelcomeSurvey
                            AStreamDataType = StreamDataType.stSpyronStructureImpactWelcome
                        Case SurveyType.stImpactThankyouSurvey
                            AStreamDataType = StreamDataType.stSpyronStructureImpactThankyou
                    End Select
                    LoadStreamFromDatabase(AStreamDataType, oCommand, "", SurveyInfo.ProjectID)

                    ReadSurveyStructureFromStream()

                    'Init Respondents List
                    Survey.Respondents.Clear()
                    If SurveyInfo.ComparionUsersList IsNot Nothing Then
                        For Each User In SurveyInfo.ComparionUsersList
                            If ParticipantEmail = "" Or ParticipantEmail = User.Key Then
                                Dim ARespondent As New clsRespondent()
                                ARespondent.Email = User.Key
                                ARespondent.ID = User.Value.ID
                                ARespondent.Name = User.Value.UserName
                                Survey.Respondents.Add(ARespondent)
                            End If
                        Next
                    End If

                    'Read Respondent Answers
                    For Each ARespondent As clsRespondent In Survey.Respondents
                        fStream = New MemoryStream()
                        fBW = New BinaryWriter(fStream)
                        fBR = New BinaryReader(fStream)

                        Dim AAnswerStreamDataType As StreamDataType = StreamDataType.stSpyronAnswersWelcome
                        Select Case SurveyInfo.SurveyType
                            Case SurveyType.stWelcomeSurvey
                                AStreamDataType = StreamDataType.stSpyronAnswersWelcome
                            Case SurveyType.stThankyouSurvey
                                AStreamDataType = StreamDataType.stSpyronAnswersThankYou
                            Case SurveyType.stImpactWelcomeSurvey
                                AStreamDataType = StreamDataType.stSpyronAnswersImpactWelcome
                            Case SurveyType.stImpactThankyouSurvey
                                AStreamDataType = StreamDataType.stSpyronAnswersImpactThankyou
                        End Select
                        LoadStreamFromDatabase(AStreamDataType, oCommand, "", SurveyInfo.ProjectID, ARespondent.ID)

                        ReadRespondentsDataFromStream(ARespondent)
                    Next

                    DBConn.Close()
                    Return True 'L0022
                Catch ex As Exception
                    SurveyInfo.ErrorMessage = String.Format(emUnknownDBError, "database")
                    Return False 'L0022
                Finally
                    DBConn.Close()
                End Try
            End Using

        End Function


        'L0022 ===
        ''' <summary>
        ''' Trying to open and read Survey structures from Streamed binary File.
        ''' </summary>
        ''' <returns>True - if reading was successful</returns>
        ''' <remarks></remarks>
        Public Function OpenStreamProjectFromFile_Old() As Boolean
            Survey = New clsSurvey
            Try
                ' in this case SurveyInfo.ConnectionString contains file path
                Dim AFileStream As New FileStream(SurveyInfo.ConnectionString, FileMode.Open)   ' D0381
                fBR = New BinaryReader(AFileStream) ' D0381

                Dim FReadedChunk As Boolean = True 'L0023
                Dim ISOldVersion As Boolean = False

                While (fBR.BaseStream.Position < fBR.BaseStream.Length - 1) And (FReadedChunk) 'L0023
                    FReadedChunk = False 'L0023

                    'Read Survey Properties
                    If ReadSurveyPropertiesFromStream() Then
                        FReadedChunk = True  'L0023
                        ISOldVersion = True
                    Else
                        If Not ISOldVersion Then
                            AFileStream.Close()
                            Return OpenStreamProjectFromFile()
                        End If
                    End If


                    'Read Survey Pages
                    If ReadPagesFromStream() Then FReadedChunk = True 'L0023

                    'Read Rules

                    'Read RespondentGroups
                    If ReadRespondentsInfoFromStream() Then FReadedChunk = True 'L0023

                    'Read Respondents Answers
                    If ReadChunkIDFromStream(CHUNK_RESPONDENT_DATA) Then
                        FReadedChunk = True 'L0023
                        Dim ARespondentGUID As Guid
                        Dim ARespondent As clsRespondent = Nothing
                        ARespondentGUID = New Guid(fBR.ReadString())
                        ARespondent = Survey.RespondentByGUID(ARespondentGUID)
                        If ARespondent IsNot Nothing Then
                            ReadRespondentsDataFromStream(ARespondent)
                        End If
                    End If

                    'Read Data Analysis
                    If ReadAnalysisDataFromStream() Then FReadedChunk = True 'L0023
                End While
                AFileStream.Close()
            Catch ex As Exception
                Return False

            End Try

            Return True
        End Function
        'L0022 ==

        ''' <summary>
        ''' Trying to open and read Survey structures from Streamed binary File.
        ''' </summary>
        ''' <returns>True - if reading was successful</returns>
        ''' <remarks></remarks>
        Public Function OpenStreamProjectFromFile() As Boolean
            Survey = New clsSurvey
            Try
                ' in this case SurveyInfo.ConnectionString contains file path
                Dim AFileStream As New FileStream(SurveyInfo.ConnectionString, FileMode.Open)   ' D0381
                fBR = New BinaryReader(AFileStream) ' D0381

                Dim FReadedChunk As Boolean = True 'L0023

                While (fBR.BaseStream.Position < fBR.BaseStream.Length - 1) And (FReadedChunk) 'L0023
                    FReadedChunk = False 'L0023
                    SurveyInfo.DBVersion = New Version(fBR.ReadString)
                    'If SurveyInfo.DBVersion > CurrentDBVersion Then
                    '    SurveyInfo.ErrorMessage = String.Format(emInvalidDBVersion, SurveyInfo.DBVersion, CurrentDBVersion)
                    '    Return False
                    'End If

                    'Read Survey Structure
                    If ReadSurveyStructureFromStream() Then FReadedChunk = True 'L0023

                    Survey.Respondents.Clear()
                    For Each User In SurveyInfo.ComparionUsersList
                        Dim ARespondent As New clsRespondent()
                        ARespondent.Email = User.Key
                        ARespondent.ID = User.Value.ID
                        ARespondent.Name = User.Value.UserName
                    Next

                    'Read Respondents Answers
                    If ReadChunkIDFromStream(CHUNK_RESPONDENT_DATA) Then
                        FReadedChunk = True 'L0023
                        Dim ARespondentID As Integer = fBR.ReadInt32
                        Dim ARespondent As clsRespondent = Nothing
                        ARespondent = Survey.RespondentByID(ARespondentID)
                        If ARespondent IsNot Nothing Then
                            ReadRespondentsDataFromStream(ARespondent)
                        End If
                    End If
                End While
                AFileStream.Close()
            Catch ex As Exception
                SurveyInfo.ErrorMessage = String.Format(emUnknownDBError, "file")
                Return False

            End Try

            Return True
        End Function


        ''' <summary>
        ''' Saves Respondent Answers
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub SaveStreamRespondentAnswers(ByRef ARespondent As clsRespondent)

            Using DBConn As DbConnection = GetDBConnection(SurveyInfo.ProviderType, SurveyInfo.ConnectionString)    ' D2232
                Dim oCommand As DbCommand = GetDBCommand(SurveyInfo.ProviderType)
                Dim DBTransaction As DbTransaction = Nothing

                Try
                    DBConn.Open()
                    oCommand.Connection = DBConn
                    DBTransaction = DBConn.BeginTransaction
                    oCommand.Transaction = DBTransaction

                    If ARespondent IsNot Nothing Then
                        ' Save respondent answers
                        fStream = New MemoryStream()
                        fBW = New BinaryWriter(fStream)
                        WriteRespondentsDataToStream(ARespondent)
                        Dim sdt As StreamDataType = StreamDataType.stSpyronAnswersWelcome
                        Select Case SurveyInfo.SurveyType
                            Case SurveyType.stWelcomeSurvey
                                sdt = StreamDataType.stSpyronAnswersWelcome
                            Case SurveyType.stThankyouSurvey
                                sdt = StreamDataType.stSpyronAnswersThankYou
                            Case SurveyType.stImpactWelcomeSurvey
                                sdt = StreamDataType.stSpyronAnswersImpactWelcome
                            Case SurveyType.stImpactThankyouSurvey
                                sdt = StreamDataType.stSpyronAnswersImpactThankyou
                        End Select
                        SaveStreamToDatabase(sdt, oCommand, SurveyInfo.ProjectID, ARespondent.ID)
                    End If

                    DBTransaction.Commit()

                Catch ex As Exception
                    If DBTransaction IsNot Nothing AndAlso DBConn.State = System.Data.ConnectionState.Open Then
                        DBTransaction.Rollback()
                        DBConn.Close()
                    End If
                Finally
                    If DBConn.State = System.Data.ConnectionState.Open Then DBConn.Close()
                End Try

            End Using
        End Sub

        ' Logical dividing Survey Structures to different data types
        Enum StreamDataType
            sdtSurveyProperties = 1
            sdtSurveyStructure = 2
            sdtRespondentsInfo = 3
            sdtRespondentsData = 4
            sdtRules = 5

            stSpyronAnswersWelcome = 6
            stSpyronAnswersThankYou = 7
            stSpyronAnswersImpactWelcome = 8
            stSpyronAnswersImpactThankyou = 9

            stSpyronModelVersion = 16
            stSpyronStructureWelcome = 17
            stSpyronStructureThankYou = 18
            stSpyronStructureImpactWelcome = 50
            stSpyronStructureImpactThankyou = 51
        End Enum

        ' Declaration of Stream Chunks constants

        Public Const CHUNK_SURVEY_PROPERTIES As Integer = 1
        Public Const CHUNK_SURVEY_STRUCTURE As Integer = 2


        Public Const CHUNK_PAGE_LIST As Integer = 100
        Public Const CHUNK_PAGE As Integer = 101

        Public Const CHUNK_QUESTION_LIST As Integer = 200
        Public Const CHUNK_QUESTION As Integer = 201
        Public Const CHUNK_QUESTION_SELECTED_VARIANTS_OPTIONS = 202 'L0047
        Public Const CHUNK_QUESTION_LINKED_PARTICIPANT_ATTRIBUTE = 203

        Public Const CHUNK_VARIANT_LIST As Integer = 300
        Public Const CHUNK_VARIANT As Integer = 301

        Public Const CHUNK_RESPONDENTS_INFO As Integer = 400
        Public Const CHUNK_RESPONDENT_GROUP As Integer = 500
        Public Const CHUNK_RESPONDENT As Integer = 501

        Public Const CHUNK_RESPONDENT_DATA As Integer = 550
        Public Const CHUNK_RESPONDENT_ANSWER_LIST As Integer = 600
        Public Const CHUNK_RESPONDENT_ANSWER As Integer = 601

        Public Const CHUNK_RESPONDENT_GROUP_FILTER As Integer = 700

        Public Const CHUNK_RULE_LIST As Integer = 800
        Public Const CHUNK_RULE As Integer = 801

        'L0023 ===
        Public Const CHUNK_ANALYSIS_DATA As Integer = 1000

        Public Const CHUNK_GROUPFILTER_LIST As Integer = 1100
        Public Const CHUNK_GROUPFILTER As Integer = 1101
        'L0023 ==

        'L0070 ===
        Public Const CHUNK_MISC_PROPERTIES As Integer = 2000

        Public Const CHUNK_PAGE_PROPERTIES As Integer = 2001
        Public Const CHUNK_PAGE_STATE As Integer = 2002
        'L0070 ==

        Public Const DUMMY_SIZE_OF_THE_CHUNK As Integer = 0

        Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_MODEL_VERSION As Integer = 70016
        Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_WELCOME As Integer = 70017
        Public Const CHUNK_CANVAS_STREAMS_PROJECT_SPYRON_STRUCTURE_THANK_YOU As Integer = 70018

        Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_WELCOME As Integer = 80006
        Public Const CHUNK_CANVAS_STREAMS_PROJECT_USER_SPYRON_ANSWERS_THANK_YOU As Integer = 80007

        ''' <summary>
        ''' Trying to read specified ChunkID from Stream.
        ''' </summary>
        ''' <param name="ChunkID"></param>
        ''' <returns>Returns true if ChunkID can be read from Stream, else return false and move Stream position back.</returns>
        ''' <remarks></remarks>
        Public Function ReadChunkIDFromStream(ByVal ChunkID As Int32) As Boolean
            If (fBR.BaseStream.Position > fBR.BaseStream.Length - 1) Then Return False

            If fBR.ReadInt32 = ChunkID Then
                Return True
            Else
                fBR.BaseStream.Seek(-Len(ChunkID), SeekOrigin.Current)
                Return False
            End If
        End Function

        ''' <summary>
        ''' Supporting function. Helps to Convert DateTime to Long integer for stream.
        ''' </summary>
        ''' <param name="ADate"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function DateToLong(ByVal ADate As DateTime) As Long
            Dim strDate = ADate.ToString
            Return CDate(strDate).ToBinary
        End Function

        ''' <summary>
        ''' Supporting function. Helps to Convert Long integer to DateTime.
        ''' </summary>
        ''' <param name="ALong"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function LongToDate(ByVal ALong As Long) As DateTime
            Return Date.FromBinary(ALong)
        End Function

        Public Sub WriteSurveyStructureToStream()
            fBW.Write(CHUNK_SURVEY_STRUCTURE)

            'Survey Properties
            fBW.Write(CHUNK_SURVEY_PROPERTIES)
            Dim AStartPosition As Integer = fBW.BaseStream.Position
            fBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            fBW.Write(SurveyInfo.AGuid.ToString)
            fBW.Write(SurveyInfo.Title)
            fBW.Write(SurveyInfo.Comments)
            fBW.Write(SurveyInfo.HideIndexNumbers)

            Dim ASize As Integer = fBW.BaseStream.Position - AStartPosition
            fBW.Seek(-ASize, SeekOrigin.Current)
            fBW.Write(ASize)
            fBW.Seek(0, SeekOrigin.End)

            'Survey Pages
            fBW.Write(CHUNK_PAGE_LIST)
            AStartPosition = fBW.BaseStream.Position
            fBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            For Each APage As clsSurveyPage In Survey.Pages
                fBW.Write(CHUNK_PAGE)
                fBW.Write(APage.AGUID.ToString)
                fBW.Write(APage.Title)
                fBW.Write(CHUNK_QUESTION_LIST)
                For Each AQuestion As clsQuestion In APage.Questions
                    WriteQuestionToStream(AQuestion)
                Next
            Next

            ASize = fBW.BaseStream.Position - AStartPosition
            fBW.Seek(-ASize, SeekOrigin.Current)
            fBW.Write(ASize)
            fBW.Seek(0, SeekOrigin.End)
        End Sub

        Public Function ReadSurveyPropertiesFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_SURVEY_PROPERTIES) Then Return False
            Dim StartPosition As Int32 = fBR.BaseStream.Position 'L0073
            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32
            SurveyInfo.AGuid = New Guid(fBR.ReadString)
            SurveyInfo.Title = fBR.ReadString
            SurveyInfo.Comments = fBR.ReadString
            If SurveyInfo.DBVersion >= New Version(1,2) Then SurveyInfo.HideIndexNumbers = fBR.ReadBoolean
            If (fBR.BaseStream.Position - StartPosition) < ChunkSize Then 'L0073
                fBR.ReadInt32() 'L0073
            End If

            Return True
        End Function

        Public Function ReadSurveyDBVersionFromStream() As Boolean
            SurveyInfo.DBVersion = New Version(fBR.ReadString)
            Return True
        End Function

        Public Sub WriteSurveyDBVersionToStream(ByVal AVersion As Version)
            fBW.Write(AVersion.ToString)
        End Sub

        Public Function ReadSurveyStructureFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_SURVEY_STRUCTURE) Then Return False

            'Survey Properties
            If Not ReadChunkIDFromStream(CHUNK_SURVEY_PROPERTIES) Then Return False
            Dim StartPosition As Int32 = fBR.BaseStream.Position
            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32
            SurveyInfo.AGuid = New Guid(fBR.ReadString)
            SurveyInfo.Title = fBR.ReadString
            SurveyInfo.Comments = fBR.ReadString
            If SurveyInfo.DBVersion >= New Version(1,2) Then SurveyInfo.HideIndexNumbers = fBR.ReadBoolean
            If (fBR.BaseStream.Position - StartPosition) < ChunkSize Then
                fBR.ReadInt32()
            End If

            'Survey Pages
            If Not ReadChunkIDFromStream(CHUNK_PAGE_LIST) Then Return False
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_PAGE)
                Dim APage As clsSurveyPage
                APage = New clsSurveyPage(Survey)
                APage.AGUID = New Guid(fBR.ReadString)
                APage.Title = fBR.ReadString
                If ReadChunkIDFromStream(CHUNK_QUESTION_LIST) Then
                    While ReadChunkIDFromStream(CHUNK_QUESTION)
                        Dim AQuestion As clsQuestion = New clsQuestion(APage)
                        ReadQuestionFromStream(AQuestion)
                        APage.Questions.Add(AQuestion)
                    End While
                End If
                Survey.Pages.Add(APage)
            End While

            Return True
        End Function

        Public Sub WriteQuestionVariantsToStream(ByRef AQuestion As clsQuestion)
            fBW.Write(CHUNK_VARIANT_LIST)
            For Each AVariant As clsVariant In AQuestion.Variants
                fBW.Write(CHUNK_VARIANT)
                fBW.Write(AVariant.AGUID.ToString)
                fBW.Write(AVariant.Text)
                fBW.Write(AVariant.Type)
                fBW.Write(AVariant.ValueType)
                If AVariant.Type = Core.VariantType.vtOtherLine Or AVariant.Type = Core.VariantType.vtOtherMemo Then 'l0455
                    AVariant.VariantValue = ""
                End If
                fBW.Write(AVariant.VariantValue)
            Next
        End Sub

        Public Function ReadQuestionVariantsFromStream(ByRef AQuestion As clsQuestion) As Boolean
            If Not ReadChunkIDFromStream(CHUNK_VARIANT_LIST) Then Return False
            Dim AVariant As clsVariant
            While ReadChunkIDFromStream(CHUNK_VARIANT)
                AVariant = New clsVariant()
                AVariant.AGUID = New Guid(fBR.ReadString)
                AVariant.Text = fBR.ReadString
                AVariant.Type = fBR.ReadInt32
                AVariant.ValueType = fBR.ReadInt32
                AVariant.VariantValue = fBR.ReadString
                If AVariant.Type = Core.VariantType.vtOtherLine Or AVariant.Type = Core.VariantType.vtOtherMemo Then 'l0455
                    AVariant.VariantValue = ""
                End If
                AQuestion.Variants.Add(AVariant)
            End While

            Return True
        End Function

        Public Sub WriteQuestionToStream(ByRef AQuestion As clsQuestion)
            fBW.Write(CHUNK_QUESTION)

            fBW.Write(AQuestion.AGUID.ToString)
            fBW.Write(AQuestion.Type)
            fBW.Write(AQuestion.Name)
            fBW.Write(AQuestion.Text)
            fBW.Write(AQuestion.State)

            Select Case AQuestion.Type
                Case QuestionType.qtComment

                Case QuestionType.qtOpenLine

                Case QuestionType.qtOpenMemo

                Case QuestionType.qtVariantsRadio
                    WriteQuestionVariantsToStream(AQuestion)
                Case QuestionType.qtVariantsCheck
                    WriteQuestionVariantsToStream(AQuestion)
                Case QuestionType.qtVariantsCombo
                    WriteQuestionVariantsToStream(AQuestion)
                Case QuestionType.qtRatingScale

                Case QuestionType.qtPairwiseScale

                Case QuestionType.qtMatrixCheck

                Case QuestionType.qtMatrixOpen

                Case QuestionType.qtMatrixVariantsCombo

                Case QuestionType.qtImageCheck
                    WriteQuestionVariantsToStream(AQuestion)
                Case QuestionType.qtObjectivesSelect

                Case QuestionType.qtAlternativesSelect
                    WriteQuestionVariantsToStream(AQuestion)
                Case QuestionType.qtNumber, QuestionType.qtNumberColumn 

            End Select
            'L0047 ===
            ' Write Min and Max Selected Variants options for multi checks Question Types
            If AQuestion.Type = QuestionType.qtImageCheck Or AQuestion.Type = QuestionType.qtVariantsCheck Or AQuestion.Type = QuestionType.qtObjectivesSelect Or AQuestion.Type = QuestionType.qtAlternativesSelect Then
                fBW.Write(CHUNK_QUESTION_SELECTED_VARIANTS_OPTIONS)
                fBW.Write(AQuestion.MinSelectedVariants)
                fBW.Write(AQuestion.MaxSelectedVariants)
            End If
            'L0047 ==
            If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtObjectivesSelect And AQuestion.Type <> QuestionType.qtAlternativesSelect Then
                If Not AQuestion.LinkedAttributeID.Equals(Guid.Empty) Then
                    fBW.Write(CHUNK_QUESTION_LINKED_PARTICIPANT_ATTRIBUTE)
                    fBW.Write(AQuestion.LinkedAttributeID.ToString)
                End If
            End If

        End Sub

        Public Function ReadQuestionFromStream(ByRef AQuestion As clsQuestion) As Boolean

            AQuestion.AGUID = New Guid(fBR.ReadString)
            AQuestion.Type = fBR.ReadInt32
            AQuestion.Name = fBR.ReadString
            AQuestion.Text = fBR.ReadString
            AQuestion.State = fBR.ReadString

            Select Case AQuestion.Type
                Case QuestionType.qtComment

                Case QuestionType.qtOpenLine

                Case QuestionType.qtOpenMemo

                Case QuestionType.qtVariantsRadio
                    ReadQuestionVariantsFromStream(AQuestion)
                Case QuestionType.qtVariantsCheck
                    ReadQuestionVariantsFromStream(AQuestion)
                Case QuestionType.qtVariantsCombo
                    ReadQuestionVariantsFromStream(AQuestion)
                Case QuestionType.qtRatingScale

                Case QuestionType.qtPairwiseScale

                Case QuestionType.qtMatrixCheck

                Case QuestionType.qtMatrixOpen

                Case QuestionType.qtMatrixVariantsCombo

                Case QuestionType.qtImageCheck
                    ReadQuestionVariantsFromStream(AQuestion)
                Case QuestionType.qtObjectivesSelect

                Case QuestionType.qtAlternativesSelect
                    ReadQuestionVariantsFromStream(AQuestion)   ' D6717
                Case QuestionType.qtNumber, QuestionType.qtNumberColumn

            End Select

            'L0047 ===
            ' Read Min and Max Selected Variants options for multi checks Question Types
            If AQuestion.Type = QuestionType.qtImageCheck Or AQuestion.Type = QuestionType.qtVariantsCheck Or AQuestion.Type = QuestionType.qtObjectivesSelect Or AQuestion.Type = QuestionType.qtAlternativesSelect Then
                If ReadChunkIDFromStream(CHUNK_QUESTION_SELECTED_VARIANTS_OPTIONS) Then
                    AQuestion.MinSelectedVariants = fBR.ReadInt32
                    AQuestion.MaxSelectedVariants = fBR.ReadInt32
                End If
            End If
            'L0047 ==
            If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtObjectivesSelect And AQuestion.Type <> QuestionType.qtAlternativesSelect Then
                If ReadChunkIDFromStream(CHUNK_QUESTION_LINKED_PARTICIPANT_ATTRIBUTE) Then
                    AQuestion.LinkedAttributeID = New Guid(fBR.ReadString)
                End If
            End If

            Return True
        End Function

        Public Function ReadPagesFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_PAGE_LIST) Then Return False
            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_PAGE)
                Dim APage As clsSurveyPage
                APage = New clsSurveyPage(Survey)
                APage.AGUID = New Guid(fBR.ReadString)
                APage.Title = fBR.ReadString
                If ReadChunkIDFromStream(CHUNK_QUESTION_LIST) Then
                    While ReadChunkIDFromStream(CHUNK_QUESTION)
                        Dim AQuestion As clsQuestion = New clsQuestion(APage)
                        ReadQuestionFromStream(AQuestion)
                        APage.Questions.Add(AQuestion)
                    End While
                End If
                Survey.Pages.Add(APage)
            End While
            Return True
        End Function

        Public Function ReadRespondentsInfoFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_RESPONDENTS_INFO) Then Return False

            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_RESPONDENT_GROUP)
                fBR.ReadString() 'AGroup.AGUID = New Guid(fBR.ReadString)
                fBR.ReadString() 'AGroup.GroupName = fBR.ReadString
                fBR.ReadString() 'AGroup.Comments = fBR.ReadString
                While ReadChunkIDFromStream(CHUNK_RESPONDENT)
                    Dim ARespondent As New clsRespondent()
                    ARespondent.AGUID = New Guid(fBR.ReadString)
                    ARespondent.Name = fBR.ReadString
                    ARespondent.Email = fBR.ReadString
                    If SurveyInfo.ComparionUsersList.ContainsKey(ARespondent.Email) Then
                        ARespondent.ID = SurveyInfo.ComparionUsersList(ARespondent.Email).ID
                    End If
                    Survey.Respondents.Add(ARespondent)
                End While
            End While

            Return True
        End Function

        Public Function ReadAnalysisDataFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_ANALYSIS_DATA) Then Return False
            If Not ReadChunkIDFromStream(CHUNK_GROUPFILTER_LIST) Then Return False

            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_GROUPFILTER)
                'Dim AGroupFilter As clsGroupFilter = New clsGroupFilter(Survey)
                fBR.ReadString() 'AGroupFilter.AGUID = New Guid(fBR.ReadString)
                fBR.ReadString() 'AGroupFilter.Name = fBR.ReadString
                fBR.ReadString() 'AGroupFilter.Comments = fBR.ReadString
                fBR.ReadString() 'AGroupFilter.Rule = fBR.ReadString

                Dim RespondentsCount As Integer = fBR.ReadInt32
                For i = 1 To RespondentsCount
                    Dim ARespondent As clsRespondent
                    ARespondent = Survey.RespondentByGUID(New Guid(fBR.ReadString))
                Next
            End While

            Return True
        End Function
        'L0023 ==

        'L0070 ===
        Public Function ReadMiscPropertiesFromStream() As Boolean
            If Not ReadChunkIDFromStream(CHUNK_MISC_PROPERTIES) Then Return False
            If Not ReadChunkIDFromStream(CHUNK_PAGE_PROPERTIES) Then Return False

            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_PAGE_STATE)
                Dim PageGUID As String = fBR.ReadString
                Dim APage As clsSurveyPage
                'Dim APageState As SurveyPageState
                APage = Survey.PageByGUID(New Guid(PageGUID))
                fBR.ReadInt32() 'APageState = fBR.ReadInt32
                'If APage IsNot Nothing Then
                '    APage.State = APageState
                'End If
            End While

            Return True
        End Function
        'L0070 ==

        ' Respondents Data stores Respondents Answers
        Public Sub WriteRespondentsDataToStream(ByRef ARespondent As clsRespondent)
            fBW.Write(CHUNK_RESPONDENT_ANSWER_LIST)
            Dim AStartPosition As Integer = fBW.BaseStream.Position
            fBW.Write(DUMMY_SIZE_OF_THE_CHUNK)

            For Each AAnswer As clsAnswer In ARespondent.Answers
                fBW.Write(CHUNK_RESPONDENT_ANSWER)
                fBW.Write(AAnswer.AGUID.ToString)
                fBW.Write(DateToLong(AAnswer.AnswerDate))
                fBW.Write(AAnswer.Question.AGUID.ToString)
                fBW.Write(AAnswer.AnswerValuesString)
                fBW.Write(AAnswer.AnswerVariants.Count)
                For Each AVariant As clsVariant In AAnswer.AnswerVariants
                    fBW.Write(AVariant.AGUID.ToString)
                Next
            Next

            Dim ASize As Integer = fBW.BaseStream.Position - AStartPosition
            fBW.Seek(-ASize, SeekOrigin.Current)
            fBW.Write(ASize)
            fBW.Seek(0, SeekOrigin.End)
        End Sub

        Public Function ReadRespondentsDataFromStream(ByRef ARespondent As clsRespondent) As Boolean
            If Not ReadChunkIDFromStream(CHUNK_RESPONDENT_ANSWER_LIST) Then Return False

            Dim ChunkSize As Int32
            ChunkSize = fBR.ReadInt32

            While ReadChunkIDFromStream(CHUNK_RESPONDENT_ANSWER)
                Dim AAnswer As clsAnswer = New clsAnswer()
                AAnswer.AGUID = New Guid(fBR.ReadString)
                AAnswer.AnswerDate = LongToDate(fBR.ReadUInt64)
                Dim AQuestion As clsQuestion = Survey.QuestionByGUID(New Guid(fBR.ReadString))
                AAnswer.Question = AQuestion
                Dim AnswerValuesString As String = fBR.ReadString
                Dim AVariantsCount As Integer = fBR.ReadInt32
                Dim AVariant As clsVariant
                For i As Integer = 1 To AVariantsCount
                    AVariant = Survey.VariantByGUID(New Guid(fBR.ReadString))
                    If AVariant IsNot Nothing Then
                        Dim ARespondentVariant As New clsVariant()
                        ARespondentVariant.AGUID = AVariant.AGUID
                        ARespondentVariant.Text = AVariant.Text
                        ARespondentVariant.VariantValue = AVariant.VariantValue
                        ARespondentVariant.ValueType = AVariant.ValueType
                        ARespondentVariant.Type = AVariant.Type
                        'L0049 ===
                        If AVariant.Type = Core.VariantType.vtOtherLine Or AVariant.Type = Core.VariantType.vtOtherMemo Then
                            ARespondentVariant.VariantValue = AnswerValuesString.Split(CChar(";"))(i - 1) 'L0090
                        End If
                        'L0049 ==
                        AAnswer.AnswerVariants.Add(ARespondentVariant)
                    End If
                Next

                If AQuestion IsNot Nothing Then 'L0066

                    If AAnswer.Question.Type = QuestionType.qtMatrixOpen Or _
                        AAnswer.Question.Type = QuestionType.qtOpenLine Or _
                        AAnswer.Question.Type = QuestionType.qtOpenMemo Or _
                        AAnswer.Question.Type = QuestionType.qtNumber Or _
                        AAnswer.Question.Type = QuestionType.qtNumberColumn Then
                        For Each AStr As String In AnswerValuesString.Split(CChar(";"))
                            If AStr <> "" Then
                                AVariant = New clsVariant()
                                AVariant.VariantValue = AStr
                                AVariant.Type = Core.VariantType.vtOtherLine
                                AAnswer.AnswerVariants.Add(AVariant)
                            End If
                        Next
                    End If
                    ARespondent.Answers.Add(AAnswer)

                End If 'L0066
            End While
            Return True
        End Function

        ''' <summary>
        ''' Saves Stream to Database, using specified Stream Data Type and DbCommand
        ''' </summary>
        ''' <param name="AStreamType"></param>
        ''' <param name="oCommand"></param>
        ''' <remarks></remarks>
        Private Sub SaveStreamToDatabase(ByVal AStreamType As StreamDataType, ByRef oCommand As DbCommand, ByVal ProjectID As Integer, Optional ByVal UserID As Integer = -1)
            Dim affected As Integer = 0

            Select Case AStreamType
                Case StreamDataType.stSpyronStructureThankYou, StreamDataType.stSpyronStructureWelcome, StreamDataType.stSpyronModelVersion, StreamDataType.stSpyronStructureImpactWelcome, StreamDataType.stSpyronStructureImpactThankyou
                    oCommand.CommandText = "UPDATE ModelStructure SET StreamSize = ?, Stream = ?, ModifyDate = ? WHERE (StructureType = ?)AND(ProjectID = ?)"
                    oCommand.Parameters.Clear()

                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StreamSize", fStream.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "Stream", fStream.ToArray))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ModifyDate", Now()))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StructureType", AStreamType))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ProjectID", SurveyInfo.ProjectID))

                Case StreamDataType.stSpyronAnswersThankYou, StreamDataType.stSpyronAnswersWelcome, StreamDataType.stSpyronAnswersImpactThankyou, StreamDataType.stSpyronAnswersImpactWelcome
                    oCommand.CommandText = "UPDATE UserData SET StreamSize = ?, Stream = ?, ModifyDate = ? WHERE (DataType = ?)AND(ProjectID = ?)AND(UserID = ?)"
                    oCommand.Parameters.Clear()

                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StreamSize", fStream.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "Stream", fStream.ToArray))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ModifyDate", Now()))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "DataType", AStreamType))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ProjectID", SurveyInfo.ProjectID))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "UserID", UserID))
            End Select

            affected = DBExecuteNonQuery(SurveyInfo.ProviderType, oCommand)

            If affected = 0 Then
                Select Case AStreamType
                    Case StreamDataType.stSpyronStructureThankYou, StreamDataType.stSpyronStructureWelcome, StreamDataType.stSpyronStructureImpactWelcome, StreamDataType.stSpyronStructureImpactThankyou, StreamDataType.stSpyronModelVersion
                        oCommand.CommandText = "INSERT INTO ModelStructure (StreamSize, Stream, ModifyDate, StructureType, ProjectID) VALUES (?,?,?,?,?)"
                    Case StreamDataType.stSpyronAnswersThankYou, StreamDataType.stSpyronAnswersWelcome, StreamDataType.stSpyronAnswersImpactWelcome, StreamDataType.stSpyronAnswersImpactThankyou
                        oCommand.CommandText = "INSERT INTO UserData (StreamSize, Stream, ModifyDate, DataType, ProjectID, UserID) VALUES (?,?,?,?,?,?)"
                End Select

                DBExecuteNonQuery(SurveyInfo.ProviderType, oCommand)
            End If
        End Sub

        Private Sub SaveDBVersion(ByRef oCommand As DbCommand, ByVal ProjectID As Integer)
            Dim affected As Integer = 0

            oCommand.CommandText = "UPDATE ModelStructure SET StreamSize = ?, Stream = ?, ModifyDate = ? WHERE (StructureType = ?)AND(ProjectID = ?)"
            oCommand.Parameters.Clear()

            oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StreamSize", fStream.ToArray.Length))
            oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "Stream", fStream.ToArray))
            oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ModifyDate", Now()))
            oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StructureType", StreamDataType.stSpyronModelVersion))
            oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ProjectID", SurveyInfo.ProjectID))

            affected = DBExecuteNonQuery(SurveyInfo.ProviderType, oCommand)

            If affected = 0 Then
                oCommand.CommandText = "INSERT INTO ModelStructure (StreamSize, Stream, ModifyDate, StructureType, ProjectID) VALUES (?,?,?,?,?)"
                DBExecuteNonQuery(SurveyInfo.ProviderType, oCommand)
            End If
        End Sub

        ''' <summary>
        ''' Loads Stream from Database, using specified Stream Data Type and DbCommand
        ''' </summary>
        ''' <param name="AStreamType"></param>
        ''' <param name="oCommand"></param>
        ''' <param name="ObjectGUID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadStreamFromDatabase(ByVal AStreamType As StreamDataType, ByRef oCommand As DbCommand, Optional ByVal ObjectGUID As String = "", Optional ByVal ProjectID As Integer = -1, Optional ByVal UserID As Integer = -1) As Boolean
            Select Case AStreamType
                Case StreamDataType.stSpyronStructureWelcome, StreamDataType.stSpyronStructureThankYou, StreamDataType.stSpyronStructureImpactWelcome, StreamDataType.stSpyronStructureImpactThankyou, StreamDataType.stSpyronModelVersion
                    oCommand.CommandText = "SELECT * FROM ModelStructure WHERE (ProjectID = ?)AND(StructureType = ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ProjectID", ProjectID))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "StructureType", AStreamType))
                Case StreamDataType.stSpyronAnswersWelcome, StreamDataType.stSpyronAnswersThankYou, StreamDataType.stSpyronAnswersImpactWelcome, StreamDataType.stSpyronAnswersImpactThankyou
                    oCommand.CommandText = "SELECT * FROM UserData WHERE (ProjectID = ?)AND(UserID = ?)AND(DataType = ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ProjectID", ProjectID))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "UserID", UserID))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "DataType", AStreamType))
                Case Else
                    If ObjectGUID = "" Then
                        oCommand.CommandText = "SELECT * FROM SurveyStructure WHERE (DataType = ?)AND(SurveyID = ?)"
                    Else
                        oCommand.CommandText = "SELECT * FROM SurveyStructure WHERE (DataType = ?)AND(SurveyID = ?)AND(ObjectGUID = ?)"
                    End If
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "DataType", AStreamType))
                    oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "SurveyID", SurveyInfo.ID))
                    If ObjectGUID <> "" Then oCommand.Parameters.Add(GetDBParameter(SurveyInfo.ProviderType, "ObjectGUID", ObjectGUID))
            End Select

            Dim dbReader As DbDataReader
            dbReader = DBExecuteReader(SurveyInfo.ProviderType, oCommand)

            While dbReader.Read
                Dim bufferSize As Integer = dbReader("StreamSize")      ' The size of the BLOB buffer.
                Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                Dim retval As Long                   ' The bytes returned from GetBytes.
                Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                If AStreamType = StreamDataType.sdtRespondentsInfo Or AStreamType = StreamDataType.stSpyronAnswersWelcome Or AStreamType = StreamDataType.stSpyronAnswersThankYou Or AStreamType = StreamDataType.stSpyronAnswersImpactWelcome Or AStreamType = StreamDataType.stSpyronAnswersImpactThankyou Then
                    Survey.RespondentsStreamSize = bufferSize
                    If Not IsDBNull(dbReader("ModifyDate")) Then Survey.RespondentsLastModify = CType(dbReader("ModifyDate"), DateTime)
                End If

                retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                ' Continue reading and writing while there are bytes beyond the size of the buffer.
                Do While retval = bufferSize
                    fBW.Write(outbyte)
                    fBW.Flush()

                    ' Reposition the start index to the end of the last buffer and fill the buffer.
                    startIndex += bufferSize
                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                Loop

                ' Write the remaining buffer.
                fBW.Write(outbyte, 0, retval)
                fBW.Flush()
            End While

            fBW.Seek(0, SeekOrigin.Begin)
            dbReader.Close()

            Return True
        End Function

        ''' <summary>
        ''' Saves Project to Streamed Database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SaveStreamProjectToDatabase(ByVal ForceSaveRespondentAnswers As Boolean) As Boolean
            'If SurveyInfo.ErrorMessage = "" Then
            Using DBConn As DbConnection = GetDBConnection(SurveyInfo.ProviderType, SurveyInfo.ConnectionString) ' D0350 + D2232
                Dim oCommand As DbCommand = GetDBCommand(SurveyInfo.ProviderType)

                DBConn.Open()
                Dim DBTransaction As DbTransaction = DBConn.BeginTransaction
                oCommand.Connection = DBConn
                oCommand.Transaction = DBTransaction

                Try
                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)

                    'Save Survey DBVersion
                    WriteSurveyDBVersionToStream(CurrentDBVersion)
                    SaveStreamToDatabase(StreamDataType.stSpyronModelVersion, oCommand, SurveyInfo.ProjectID)

                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)

                    'Save Survey Structure
                    WriteSurveyStructureToStream()
                    Dim sdt As StreamDataType = StreamDataType.stSpyronStructureWelcome
                    Select Case SurveyInfo.SurveyType
                        Case SurveyType.stWelcomeSurvey
                            sdt = StreamDataType.stSpyronStructureWelcome
                        Case SurveyType.stThankyouSurvey
                            sdt = StreamDataType.stSpyronStructureThankYou
                        Case SurveyType.stImpactWelcomeSurvey
                            sdt = StreamDataType.stSpyronStructureImpactWelcome
                        Case SurveyType.stImpactThankyouSurvey
                            sdt = StreamDataType.stSpyronStructureImpactThankyou
                    End Select
                    SaveStreamToDatabase(sdt, oCommand, SurveyInfo.ProjectID)

                    fStream = New MemoryStream()
                    fBW = New BinaryWriter(fStream)

                    'L0022 ===
                    'Save Respondents Answers
                    If ForceSaveRespondentAnswers Then 'L0441
                        For Each ARespondent As clsRespondent In Survey.Respondents
                            fStream = New MemoryStream()
                            fBW = New BinaryWriter(fStream)
                            WriteRespondentsDataToStream(ARespondent)
                            sdt = StreamDataType.stSpyronAnswersWelcome
                            Select Case SurveyInfo.SurveyType
                                Case SurveyType.stWelcomeSurvey
                                    sdt = StreamDataType.stSpyronAnswersWelcome
                                Case SurveyType.stThankyouSurvey
                                    sdt = StreamDataType.stSpyronAnswersThankYou
                                Case SurveyType.stImpactWelcomeSurvey
                                    sdt = StreamDataType.stSpyronAnswersImpactWelcome
                                Case SurveyType.stImpactThankyouSurvey
                                    sdt = StreamDataType.stSpyronAnswersImpactThankyou
                            End Select
                            SaveStreamToDatabase(sdt, oCommand, SurveyInfo.ProjectID, ARespondent.ID)
                        Next
                    End If
                    'L0022 ==

                    DBTransaction.Commit()
                    DBConn.Close()
                Catch ex As Exception
                    DBTransaction.Rollback()
                    DBConn.Close()
                    Return False    ' D0380
                End Try
            End Using
            Return True ' D0380
            'Else
            '    Return False
            'End If
        End Function
        'L0020 ==

        'L0022 ===
        ''' <summary>
        ''' Saves Project to Streamed File
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SaveStreamProjectToFile() As Boolean
            'If SurveyInfo.ErrorMessage = "" Then
            Try
                Dim AFileStream As New FileStream(SurveyInfo.ConnectionString, FileMode.Create)
                fBW = New BinaryWriter(AFileStream)
                fBW.Write(CurrentDBVersion.ToString)

                'Save Survey Structure
                WriteSurveyStructureToStream()

                'Save Respondents Answers
                For Each ARespondent As clsRespondent In Survey.Respondents
                    fBW.Write(CHUNK_RESPONDENT_DATA)
                    fBW.Write(ARespondent.ID)
                    WriteRespondentsDataToStream(ARespondent)
                Next

                fBW.Flush()
                fBW.Close()
            Catch ex As Exception
                Return False
            End Try

            Return True
            'Else
            'Return False
            'End If
        End Function
        'L0022 ==

        Public Sub New(ByRef ASurveyInfo As clsSurveyInfo)
            fSurveyInfo = ASurveyInfo
        End Sub

    End Class

End Namespace