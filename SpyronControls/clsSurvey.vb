Imports Microsoft.VisualBasic
'Imports SpyronControls.Macros
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports SpyronControls.Spyron.DataSources 'L0067
Imports System.Data 'L0067

Namespace Spyron.Core

    'Enumerations

    <Serializable()> Public Enum QuestionType
        qtComment = 0
        qtOpenLine = 1
        qtOpenMemo = 2
        qtVariantsRadio = 3
        qtVariantsCheck = 4
        qtVariantsCombo = 5
        qtRatingScale = 6
        qtPairwiseScale = 7
        qtMatrixCheck = 8
        qtMatrixOpen = 9
        qtMatrixVariantsCombo = 10
        qtImageCheck = 11
        'L0018 ===
        qtObjectivesSelect = 12
        qtAlternativesSelect = 13
        'L0018 ==
        qtNumber = 14
        qtNumberColumn = 15
    End Enum

    <Serializable()> Public Enum VariantsSelectType
        vstRadio = 1
        vstCheck = 2
        vstCombo = 3
    End Enum

    <Serializable()> Public Enum VariantValueType
        vvtString = 0
        vvtInt = 1
        vvtFloat = 2
        vvtDateTime = 3
    End Enum

    Public Enum VariantType
        vtText = 0
        vtSimple = 1
        vtOtherLine = 2
        vtOtherMemo = 3
    End Enum

    'Utilities

    ''L0005 ===
    '<Serializable()> Public Class clsGroupFilter
    '    Private fGUID As Guid 'L0020
    '    Private fRespondents As New ArrayList
    '    Private fVariants As New ArrayList
    '    Private fName As String
    '    Private fComments As String 'L0023
    '    Private fSurvey As clsSurvey
    '    Private fRule As String = "" 'L0023

    '    'L0020 ===
    '    Public Property AGUID() As Guid
    '        Get
    '            Return fGUID
    '        End Get
    '        Set(ByVal value As Guid)
    '            fGUID = value
    '        End Set
    '    End Property
    '    'L0020 ==

    '    Public Sub New(ByRef ASurvey As clsSurvey) 'L0023
    '        fSurvey = ASurvey
    '        fGUID = Guid.NewGuid() 'L0020
    '    End Sub

    '    Public Property Name() As String
    '        Get
    '            Return fName
    '        End Get
    '        Set(ByVal value As String)
    '            fName = value
    '        End Set
    '    End Property

    '    'L0023 ===
    '    Public Property Comments() As String
    '        Get
    '            Return fComments
    '        End Get
    '        Set(ByVal value As String)
    '            fComments = value
    '        End Set
    '    End Property
    '    'L0023 ==

    '    Public ReadOnly Property Respondents() As ArrayList
    '        Get
    '            Return fRespondents
    '        End Get
    '    End Property

    '    Public ReadOnly Property Variants() As ArrayList
    '        Get
    '            Return fVariants
    '        End Get
    '    End Property

    '    'L0023 ===
    '    Public Property Rule() As String
    '        Get
    '            Return fRule
    '        End Get
    '        Set(ByVal value As String)
    '            fRule = value
    '        End Set
    '    End Property
    '    'L0023 ==

    '    'L0025 ===
    '    Public ReadOnly Property ReadableRule() As String
    '        Get
    '            Dim AResult As String = Rule
    '            For Each APage As clsSurveyPage In fSurvey.Pages
    '                For Each AQuestion As clsQuestion In APage.Questions
    '                    'L0067 ===
    '                    AResult = AResult.Replace("Question_" + AQuestion.AGUID.ToString, AQuestion.Text)
    '                    'For Each AVariant As clsVariant In AQuestion.Variants
    '                    '    AResult = AResult.Replace("RE_RACVG({0},""" + AVariant.AGUID.ToString + """)", String.Format("[{0}]=[{1}]", AQuestion.Text, AVariant.Text))
    '                    'Next
    '                    'L0067 ==
    '                Next
    '            Next
    '            Return AResult
    '        End Get
    '    End Property
    '    'L0025 ==
    'End Class
    ''L0005 ==

    'L0070 ===
    '<Serializable()> Public Enum SurveyPageState
    '    spsAtBegin = 0
    '    spsAtEnd = 1
    '    spsDisabled = 2
    'End Enum
    'L0070 ==

    ''' <summary>
    ''' Represent Survey Page
    ''' </summary>
    ''' <remarks>Contains Questions and Branching Rules</remarks>
    <Serializable()> Public Class clsSurveyPage
        Private fSurvey As clsSurvey
        Private fID As Integer = -1
        Private fGUID As Guid 'L0020
        Private fTitle As String = ""
        Private fQuestions As New ArrayList

        'Private fPageState As SurveyPageState = SurveyPageState.spsAtBegin 'L0070

        Public ReadOnly Property Survey() As clsSurvey
            Get
                Return fSurvey
            End Get
        End Property
        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        'L0020 ===
        Public Property AGUID() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property
        'L0020 ==

        Public Property Title() As String
            Get
                Return fTitle
            End Get
            Set(ByVal value As String)
                fTitle = value
            End Set
        End Property

        Public ReadOnly Property Questions() As ArrayList
            Get
                Return fQuestions
            End Get
        End Property

        Public Property Question(ByVal index As Integer) As clsQuestion
            Get
                Return CType(Questions.Item(index), clsQuestion)
            End Get
            Set(ByVal value As clsQuestion)
                Questions.Item(index) = value
            End Set
        End Property

        ''L0070 ===
        'Public Property State() As SurveyPageState
        '    Get
        '        Return fPageState
        '    End Get
        '    Set(ByVal value As SurveyPageState)
        '        fPageState = value
        '    End Set
        'End Property
        ''L0070 ==

        Public Sub DeleteQuestionByGUID(ByVal QuestionGUID As Guid)
            For Each AQuestion As clsQuestion In Questions
                If AQuestion.AGUID = QuestionGUID Then
                    Questions.Remove(AQuestion)
                End If
            Next
        End Sub

        'L0478 ===
        Public Function GetQuestionByGUID(ByVal QuestionGUID As Guid) As clsQuestion
            For Each AQuestion As clsQuestion In Questions
                If AQuestion.AGUID = QuestionGUID Then
                    Return AQuestion
                End If
            Next
            Return Nothing
        End Function
        'L0478 ==

        Public Sub New(ByRef ASurvey As clsSurvey)
            fSurvey = ASurvey
            fGUID = Guid.NewGuid() 'L0020
        End Sub

        'L0026 ===
        Public Sub MoveQuestionUp(ByRef AQuestion As clsQuestion)
            Dim ATempQuestion As clsQuestion = New clsQuestion(Me)
            ATempQuestion = AQuestion
            Dim AQuestionIndex As Integer = Questions.IndexOf(AQuestion)
            If AQuestionIndex > 0 Then
                Questions.RemoveAt(AQuestionIndex)
                Questions.Insert(AQuestionIndex - 1, ATempQuestion)
            End If
        End Sub

        Public Sub MoveQuestionDown(ByRef AQuestion As clsQuestion)
            Dim ATempQuestion As clsQuestion = New clsQuestion(Me)
            ATempQuestion = AQuestion
            Dim AQuestionIndex As Integer = Questions.IndexOf(AQuestion)
            If AQuestionIndex < (Questions.Count - 1) Then
                Questions.RemoveAt(AQuestionIndex)
                Questions.Insert(AQuestionIndex + 1, ATempQuestion)
            End If
        End Sub
        'L0026 ==

        'L0040 ===
        Public Function GetNextPage() As clsSurveyPage 'L0043
            If Survey.Pages.IndexOf(Me) < Survey.Pages.Count - 1 Then 'L0044
                Return CType(Survey.Pages(Survey.Pages.IndexOf(Me) + 1), clsSurveyPage)
            End If
            Return Nothing
        End Function
        'L0040 ==

        'L0043 ===
        Public Function GetPrevPage() As clsSurveyPage
            If Survey.Pages.IndexOf(Me) > 0 Then
                Dim APage As clsSurveyPage = Survey.Pages(0)
                While APage.GetNextPage IsNot Nothing And APage.GetNextPage IsNot Me
                    APage = APage.GetNextPage
                End While
                Return APage
            End If
            Return Nothing
        End Function
        'L0043 ==

    End Class

    ''' <summary>
    ''' Represent Answer Variant. that Respondent can chose in Survey
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class clsVariant
        Private fID As Integer = -1
        Private fGUID As Guid 'L0020
        Private fType As VariantType
        Private fVariantText As String = ""
        Private fVariantValue As String = ""
        Private fVariantValueType As VariantValueType


        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        'L0020 ===
        Public Property AGUID() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property

        Public Sub New()
            fGUID = Guid.NewGuid() 'L0020
        End Sub
        'L0020 ==

        Public Property Type() As VariantType
            Get
                Return fType
            End Get
            Set(ByVal value As VariantType)
                fType = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return fVariantText.Trim()
            End Get
            Set(ByVal value As String)
                fVariantText = value.Trim()
            End Set
        End Property
        Public Property VariantValue() As String
            Get
                Return fVariantValue.Trim()
            End Get
            Set(ByVal value As String)
                fVariantValue = value.Trim()
            End Set
        End Property
        Public Property ValueType() As VariantValueType
            Get
                Return fVariantValueType
            End Get
            Set(ByVal value As VariantValueType)
                fVariantValueType = value
            End Set
        End Property



    End Class

    ''' <summary>
    ''' Represent Survey Question
    ''' </summary>
    ''' <remarks>Contains Variants and other property of Question</remarks>
    <Serializable()> Public Class clsQuestion
        Private fID As Integer = -1
        Private fGUID As Guid 'L0020
        Private fName As String = ""
        Private fText As String = ""
        'Private fPage As clsSurveyPage = Nothing 'L0478
        Private fType As QuestionType = QuestionType.qtComment 'L0478

        Private fVariants As New ArrayList
        Private fColumns As New ArrayList
        Private fRows As New ArrayList
        Private fMinSelectedVariants As Integer = 0 'L0047
        Private fMaxSelectedVariants As Integer = 0 'L0047

        Private fLinkedAttributeID As Guid = Guid.Empty

        Private fState As String = "1111111111"
        'Each char means different property of Question:
        '"0" - disable property, "1" - enable property
        'Position code of State string: 
        '0 - Visible
        '1 - Enabled
        '2 - AllowSkip

        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        'L0020 ===
        Public Property AGUID() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property
        'L0020 ==

        Public Property Name() As String
            Get
                Return fName
            End Get
            Set(ByVal value As String)
                fName = value
            End Set
        End Property
        Public Property Text() As String
            Get
                Return fText
            End Get
            Set(ByVal value As String)
                fText = value
            End Set
        End Property

        Public Property Type() As QuestionType
            Get
                Return fType
            End Get
            Set(ByVal value As QuestionType)
                fType = value
            End Set
        End Property

        Public ReadOnly Property Variants() As ArrayList
            Get
                Return fVariants
            End Get
        End Property
        Public ReadOnly Property Columns() As ArrayList
            Get
                Return fColumns
            End Get
        End Property
        Public ReadOnly Property Rows() As ArrayList
            Get
                Return fRows
            End Get
        End Property

        'L0047 ===
        Public Property MinSelectedVariants() As Integer
            Get
                Return fMinSelectedVariants
            End Get
            Set(ByVal value As Integer)
                If value < Variants.Count Or Type = QuestionType.qtAlternativesSelect Or Type = QuestionType.qtObjectivesSelect Then
                    If MaxSelectedVariants <> 0 And MaxSelectedVariants < value And value <> 0 Then
                        fMaxSelectedVariants = value
                    End If
                    fMinSelectedVariants = value
                End If
            End Set
        End Property

        Public Property MaxSelectedVariants() As Integer
            Get
                Return fMaxSelectedVariants
            End Get
            Set(ByVal value As Integer)
                If value <= Variants.Count Or Type = QuestionType.qtAlternativesSelect Or Type = QuestionType.qtObjectivesSelect Then
                    If MinSelectedVariants <> 0 And MinSelectedVariants > value And value <> 0 Then
                        fMaxSelectedVariants = MinSelectedVariants
                    Else
                        fMaxSelectedVariants = value
                    End If
                End If
            End Set
        End Property
        'L0047 ==

        Public Property LinkedAttributeID As Guid
            Get
                Return fLinkedAttributeID
            End Get
            Set(value As Guid)
                fLinkedAttributeID = value
            End Set
        End Property

        Public Property State() As String
            Get
                Return fState
            End Get
            Set(ByVal value As String)
                fState = value
            End Set
        End Property
        Public Property Visible() As Boolean
            Get
                Return State(0) = "1"
            End Get
            Set(ByVal value As Boolean)
                Dim StateChars() As Char = State.ToCharArray
                If value Then
                    StateChars(0) = CChar("1")
                Else
                    StateChars(0) = CChar("0")
                End If
                State = StateChars 'L0046
            End Set
        End Property
        Public Property Enabled() As Boolean
            Get
                Return State(1) = "1"
            End Get
            Set(ByVal value As Boolean)
                Dim StateChars() As Char = State.ToCharArray
                If value Then
                    StateChars(1) = CChar("1")
                Else
                    StateChars(1) = CChar("0")
                End If
                State = StateChars 'L0046
            End Set
        End Property
        Public Property AllowSkip() As Boolean
            Get
                Return State(2) = "1"
            End Get
            Set(ByVal value As Boolean)
                Dim StateChars() As Char = State.ToCharArray
                If value Then
                    StateChars(2) = CChar("1")
                Else
                    StateChars(2) = CChar("0")
                End If
                State = StateChars 'L0046
            End Set
        End Property

        Public Property SelectAllByDefault() As Boolean
            Get
                Return State(3) = "1"
            End Get
            Set(ByVal value As Boolean)
                Dim StateChars() As Char = State.ToCharArray
                If value Then
                    StateChars(3) = CChar("1")
                Else
                    StateChars(3) = CChar("0")
                End If
                State = StateChars
            End Set
        End Property

        Public Function VariantByID(ByVal VariantID As Integer) As clsVariant
            Dim AResult As clsVariant = Nothing
            For Each AVariant As clsVariant In Variants
                If AVariant.ID = VariantID Then AResult = AVariant
            Next
            Return AResult
        End Function

        Public Function VariantByGUID(ByVal VariantGUID As Guid) As clsVariant
            Dim AResult As clsVariant = Nothing
            For Each AVariant As clsVariant In Variants
                If AVariant.AGUID = VariantGUID Then AResult = AVariant
            Next
            Return AResult
        End Function

        'L0018 ===
        Public ReadOnly Property AllowVariants() As Boolean
            Get
                Return Me.Type <> QuestionType.qtComment And
                       Me.Type <> QuestionType.qtMatrixOpen And
                       Me.Type <> QuestionType.qtOpenLine And
                       Me.Type <> QuestionType.qtOpenMemo And
                       Me.Type <> QuestionType.qtObjectivesSelect And
                       Me.Type <> QuestionType.qtAlternativesSelect And
                       Me.Type <> QuestionType.qtNumber And
                       Me.Type <> QuestionType.qtNumberColumn
            End Get
        End Property
        'L0018 ==

        Public Sub New(ByRef APage As clsSurveyPage)
            'fPage = APage
            fGUID = Guid.NewGuid() 'L0020
        End Sub
    End Class


    '<Serializable()> Public Class clsRespondentGroup
    '    Private fID As Integer = -1
    '    Private fGUID As Guid 'L0020
    '    Private fGroupName As String = ""
    '    Private fRespondents As New ArrayList
    '    Private fComments As String = ""

    '    Public Property ID() As Integer
    '        Get
    '            Return fID
    '        End Get
    '        Set(ByVal value As Integer)
    '            fID = value
    '        End Set
    '    End Property

    '    'L0020 ===
    '    Public Property AGUID() As Guid
    '        Get
    '            Return fGUID
    '        End Get
    '        Set(ByVal value As Guid)
    '            fGUID = value
    '        End Set
    '    End Property

    '    Public Sub New()
    '        fGUID = Guid.NewGuid() 'L0020
    '    End Sub
    '    'L0020 ==

    '    Public Property GroupName() As String
    '        Get
    '            Return fGroupName
    '        End Get
    '        Set(ByVal value As String)
    '            fGroupName = value
    '        End Set
    '    End Property
    '    Public ReadOnly Property Respondents() As ArrayList
    '        Get
    '            Return fRespondents
    '        End Get
    '    End Property

    '    Public Property Comments() As String
    '        Get
    '            Return fComments
    '        End Get
    '        Set(ByVal value As String)
    '            fComments = value
    '        End Set
    '    End Property
    'End Class

    ''' <summary>
    ''' Represent Respondent Object
    ''' </summary>
    ''' <remarks>Contains his property and all his answers</remarks>
    <Serializable()> Public Class clsRespondent
        Private fID As Integer = -1
        Private fGUID As Guid 'L0020
        Private fName As String = ""
        Private fEmail As String = ""
        'Private fRespondentGroup As clsRespondentGroup
        Private fAnswers As New ArrayList
        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        'L0020 ===
        Public Property AGUID() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property
        'L0020 ==

        Public Property Name() As String
            Get
                Return fName
            End Get
            Set(ByVal value As String)
                fName = value
            End Set
        End Property
        Public Property Email() As String
            Get
                Return fEmail
            End Get
            Set(ByVal value As String)
                fEmail = value
            End Set
        End Property

        Public ReadOnly Property Answers() As ArrayList
            Get
                Return fAnswers
            End Get
        End Property

        'L0020 ===
        Public Function AnswerByQuestionGUID(ByVal QuestionGUID As Guid) As clsAnswer
            For Each Answer As clsAnswer In Answers
                If Answer.Question.AGUID.Equals(QuestionGUID) Then Return Answer
            Next
            Return Nothing
        End Function
        'L0020 ==

        Public Sub New()
            fGUID = Guid.NewGuid() 'L0020
        End Sub
    End Class

    ''' <summary>
    ''' Represent Respondent Answer
    ''' </summary>
    ''' <remarks>Contains all selected/entered AnswerVariants</remarks>
    <Serializable()> Public Class clsAnswer
        Private fID As Integer = -1
        Private fGUID As Guid 'L0020
        Private fQuestion As clsQuestion
        Private fAnswerVariants As New ArrayList
        Private fAnswerDate As DateTime

        Public Property ID() As Integer
            Get
                Return fID
            End Get
            Set(ByVal value As Integer)
                fID = value
            End Set
        End Property

        'L0020 ===
        Public Property AGUID() As Guid
            Get
                Return fGUID
            End Get
            Set(ByVal value As Guid)
                fGUID = value
            End Set
        End Property

        Public Sub New()
            fGUID = Guid.NewGuid() 'L0020
        End Sub
        'L0020 ==

        Public Property Question() As clsQuestion
            Get
                Return fQuestion
            End Get
            Set(ByVal value As clsQuestion)
                fQuestion = value
            End Set
        End Property
        Public ReadOnly Property AnswerVariants() As ArrayList
            Get
                Return fAnswerVariants
            End Get
        End Property

        Public ReadOnly Property AnswerGUIDsString() As String
            Get
                Dim Result As String = ""
                If Question.AllowVariants Then 'L0018
                    For Each AAnswer As clsVariant In AnswerVariants
                        Result += AAnswer.AGUID.ToString
                        If AnswerVariants.IndexOf(AAnswer) < (AnswerVariants.Count - 1) Then Result += ";"
                    Next
                End If
                Return Result
            End Get
        End Property

        Public ReadOnly Property AnswerValuesString() As String
            Get
                Dim Result As String = ""

                For Each AAnswer As clsVariant In AnswerVariants
                    'Result += AAnswer.VariantValue'LLL
                    'L0443 ===
                    'Or AAnswer.Type = VariantType.vtOtherLine Or AAnswer.Type = VariantType.vtOtherMemo
                    If (AAnswer.Type = VariantType.vtOtherLine Or AAnswer.Type = VariantType.vtOtherMemo) Then
                        Result += AAnswer.VariantValue.Trim()
                    Else
                        Result += AAnswer.Text.Trim()
                    End If
                    'L0443 ==

                    If AnswerVariants.IndexOf(AAnswer) < (AnswerVariants.Count - 1) Then Result += ";"

                Next
                Return Result
            End Get
        End Property
        Public Property AnswerDate() As DateTime
            Get
                Return fAnswerDate
            End Get
            Set(ByVal value As DateTime)
                fAnswerDate = value
            End Set
        End Property
    End Class

    ''' <summary>
    ''' Represent Survey Object
    ''' </summary>
    ''' <remarks>Contains Pages, RespondentGroups, OnGroupingRules</remarks>
    <Serializable()> Public Class clsSurvey

        Private fPages As New ArrayList
        Private fRespondents As New ArrayList

        Private fRespondentsStreamSize As Long = -1      ' D1135
        Private fRespondentsLastModify As Nullable(Of DateTime) = Nothing   ' D1135

        Private fActivePageGUID As Guid
        Private fSelectedQuestionGUID As Guid
        'Private fOnGroupingRules As New ArrayList

        ''L0005 ===
        'Private fGroupFilters As New ArrayList
        ''L0005 ==

        ' array of clsSurveyPage's
        Public ReadOnly Property Pages() As ArrayList
            Get
                Return fPages
            End Get
        End Property

        ' array of clsRespondents
        Public ReadOnly Property Respondents() As ArrayList
            Get
                Return fRespondents
            End Get
        End Property

        ' D1135 ===
        Friend Property RespondentsStreamSize() As Long
            Get
                Return fRespondentsStreamSize
            End Get
            Set(ByVal value As Long)
                fRespondentsStreamSize = value
            End Set
        End Property

        Public Property RespondentsLastModify() As Nullable(Of DateTime)
            Get
                Return fRespondentsLastModify
            End Get
            Set(ByVal value As Nullable(Of DateTime))
                fRespondentsLastModify = value
            End Set
        End Property
        ' D1135 ==

        'L0020 ===
        Public Property ActivePageGUID() As Guid
            Get
                Return fActivePageGUID
            End Get
            Set(ByVal value As Guid)
                fActivePageGUID = value
            End Set
        End Property
        'L0020 ==

        Public ReadOnly Property ActivePage() As clsSurveyPage
            Get
                Return PageByGUID(ActivePageGUID)
            End Get
        End Property

        Public Property SelectedQuestionGUID() As Guid
            Get
                Return fSelectedQuestionGUID
            End Get
            Set(ByVal value As Guid)
                fSelectedQuestionGUID = value
            End Set
        End Property

        Public ReadOnly Property SelectedQuestion() As clsQuestion
            Get
                Return QuestionByGUID(SelectedQuestionGUID)
            End Get
        End Property

        Public Function PageByID(ByVal PageID As Integer) As clsSurveyPage
            Dim AResultPage As clsSurveyPage = Nothing
            For Each APage As clsSurveyPage In Pages
                If APage.ID = PageID Then
                    AResultPage = APage
                End If
            Next
            Return AResultPage
        End Function

        'L0020 ===
        Public Function PageByGUID(ByVal PageGUID As Guid) As clsSurveyPage
            Dim AResultPage As clsSurveyPage = Nothing
            For Each APage As clsSurveyPage In Pages
                If APage.AGUID = PageGUID Then
                    AResultPage = APage
                End If
            Next
            Return AResultPage
        End Function

        Public Function QuestionByGUID(ByVal QuestionGUID As Guid) As clsQuestion
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.AGUID = QuestionGUID Then
                        Return AQuestion
                    End If
                Next
            Next
            Return Nothing
        End Function
        'L0020 ==

        'L0478 ===
        Public Function GetPageByQuestionGUID(ByVal QuestionGUID As Guid) As clsSurveyPage
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.AGUID = QuestionGUID Then
                        Return APage
                    End If
                Next
            Next
            Return Nothing
        End Function

        Public Function isSurveyContainsPipeModifiers() As Boolean
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.Type = QuestionType.qtAlternativesSelect OrElse AQuestion.Type = QuestionType.qtObjectivesSelect OrElse Not AQuestion.LinkedAttributeID.Equals(Guid.Empty) Then
                        Return True
                    End If
                Next
            Next
            Return False
        End Function

        ' D7365 ===
        Public Function AllowToSkipPage(pageIdx As Integer, UserEmail As String) As Boolean
            If Pages.Count > pageIdx Then
                Dim Respondent As clsRespondent = RespondentByEmail(UserEmail)
                Dim APage As clsSurveyPage = Pages(pageIdx)
                For Each AQuestion As clsQuestion In APage.Questions
                    If Not AQuestion.AllowSkip AndAlso AQuestion.Type <> QuestionType.qtObjectivesSelect AndAlso AQuestion.Type <> QuestionType.qtAlternativesSelect Then
                        If Respondent IsNot Nothing Then
                            Dim Answer As clsAnswer = Respondent.AnswerByQuestionGUID(AQuestion.AGUID)
                            If Answer Is Nothing OrElse String.IsNullOrEmpty(Answer.AnswerValuesString) Then Return False
                        End If
                    End If
                Next
            End If
            Return True
        End Function

        Public Function HasUndefined(pageIdx As Integer, UserEmail As String) As Boolean
            If UserEmail IsNot Nothing AndAlso Pages.Count > pageIdx Then
                Dim APage As clsSurveyPage = Pages(pageIdx)
                Dim Respondent As clsRespondent = RespondentByEmail(UserEmail)
                If Respondent IsNot Nothing Then
                    For Each tQuestion As clsQuestion In APage.Questions
                        Dim Answer As clsAnswer = Respondent.AnswerByQuestionGUID(tQuestion.AGUID)
                        If Answer Is Nothing OrElse String.IsNullOrEmpty(Answer.AnswerValuesString) Then Return True
                    Next
                End If
            End If
            Return False
        End Function
        ' D7365 ==

        Public Function GetQuestionPageIndex(ByVal QuestionGUID As Guid) As Integer
            Dim AResult As Integer = 1
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.AGUID = QuestionGUID Then Return AResult
                    If (AQuestion.Visible) And (AQuestion.Type <> QuestionType.qtComment) Then AResult += 1
                Next
            Next
            Return AResult
        End Function
        'L0478 ==

        Public Function QuestionByID(ByVal QuestionID As Integer) As clsQuestion
            Dim AResultQuestion As clsQuestion = Nothing
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.ID = QuestionID Then
                        AResultQuestion = AQuestion
                    End If
                Next
            Next
            Return AResultQuestion
        End Function

        Public Sub DeleteQuestionByGUID(ByVal QuestionGUID As Guid)
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.AGUID = QuestionGUID Then
                        APage.Questions.Remove(AQuestion)
                        Exit Sub
                    End If
                Next
            Next
        End Sub

        'L0028 ===
        Public Function QuestionByVariantGUID(ByVal VariantGUID As Guid) As clsQuestion
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.VariantByGUID(VariantGUID) IsNot Nothing Then
                        Return AQuestion
                    End If
                Next
            Next
            Return Nothing
        End Function
        'L0028 ==

        'Public Function RespondentGroupByGUID(ByVal GroupGUID As Guid) As clsRespondentGroup
        '    For Each AGroup As clsRespondentGroup In RespondentGroups
        '        If AGroup.AGUID = GroupGUID Then Return AGroup
        '    Next
        '    Return Nothing
        'End Function

        'Public Function RespondentGroupByID(ByVal GroupID As Integer) As clsRespondentGroup
        '    Dim AResultGroup As clsRespondentGroup = Nothing
        '    For Each AGroup As clsRespondentGroup In RespondentGroups
        '        If AGroup.ID = GroupID Then AResultGroup = AGroup
        '    Next
        '    Return AResultGroup
        'End Function

        Public Function RespondentByGUID(ByVal RespondentGUID As Guid) As clsRespondent
            For Each ARespondent As clsRespondent In Respondents
                If ARespondent.AGUID = RespondentGUID Then Return ARespondent
            Next
            Return Nothing
        End Function

        Public Function RespondentByID(ByVal RespondentID As Integer) As clsRespondent
            Dim AResult As clsRespondent = Nothing
            For Each ARespondent As clsRespondent In Respondents
                If ARespondent.ID = RespondentID Then AResult = ARespondent
            Next
            Return AResult
        End Function

        Public Function RespondentByEmail(ByVal EMail As String) As clsRespondent
            'For Each AGroup As clsRespondentGroup In RespondentGroups
            For Each ARespondent As clsRespondent In Respondents
                If ARespondent.Email.ToLower = EMail.ToLower Then Return ARespondent
            Next
            'Next
            Return Nothing
        End Function

        'L0005 ===
        ''L0067 ===
        'Protected Sub dsGroupFilterObject_ObjectCreating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.ObjectDataSourceEventArgs)
        '    e.ObjectInstance = New Spyron.DataSources.clsGroupFilterDS(Me)
        'End Sub

        'Public Function RefreshGroupFilterRespondents(ByVal GroupFilter As clsGroupFilter) As Boolean 'L0336
        '    Try 'L0336
        '        Dim dsGroupFilter As New Spyron.DataSources.clsGroupFilterDS(Me)
        '        Dim dsGroupFilterObject As New ObjectDataSource
        '        AddHandler dsGroupFilterObject.ObjectCreating, AddressOf dsGroupFilterObject_ObjectCreating
        '        dsGroupFilterObject.TypeName = "SpyronControls.Spyron.DataSources.clsGroupFilterDS"
        '        dsGroupFilterObject.SelectMethod = "SelectAll"
        '        dsGroupFilterObject.DataBind()
        '        dsGroupFilterObject.FilterExpression = GroupFilter.Rule

        '        Dim UserEmails As IEnumerable = dsGroupFilterObject.Select()
        '        GroupFilter.Respondents.Clear()
        '        For Each row As DataRowView In UserEmails
        '            Dim ARespondentEmail As String = row("RespondentEmail").ToString()
        '            Dim ARespondent As clsRespondent = RespondentByEmail(ARespondentEmail)
        '            If ARespondent IsNot Nothing Then
        '                GroupFilter.Respondents.Add(ARespondent)
        '            End If
        '        Next
        '        Return True 'L0336
        '    Catch ex As Exception 'L0336
        '        Return False 'L0336
        '    End Try
        'End Function
        ''L0067 ==

        'L0020 ===
        Public Function VariantByGUID(ByVal VariantGUID As Guid) As clsVariant
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    For Each AVariant As clsVariant In AQuestion.Variants
                        If AVariant.AGUID = VariantGUID Then
                            Return AVariant
                        End If
                    Next
                Next
            Next
            Return Nothing
        End Function
        'L0020 ==

        Public Function VariantByID(ByVal ID As Integer) As clsVariant
            For Each APage As clsSurveyPage In Pages
                For Each AQuestion As clsQuestion In APage.Questions
                    For Each AVariant As clsVariant In AQuestion.Variants
                        If AVariant.ID = ID Then
                            Return AVariant
                        End If
                    Next
                Next
            Next
            Return Nothing
        End Function
        'L0005 ==

        'L0026 ===
        ''' <summary>
        ''' Move specified APage up in the array of clsSurveyPage's
        ''' </summary>
        ''' <param name="APage"></param>
        ''' <remarks></remarks>
        Public Sub MovePageUp(ByRef APage As clsSurveyPage)
            Dim ATempPage As clsSurveyPage = New clsSurveyPage(Me)
            ATempPage = APage
            Dim APageIndex As Integer = Pages.IndexOf(APage)
            If APageIndex > 0 Then
                Pages.RemoveAt(APageIndex)
                Pages.Insert(APageIndex - 1, ATempPage)
            End If
        End Sub

        ''' <summary>
        ''' Move specified APage down in the array of clsSurveyPage's
        ''' </summary>
        ''' <param name="APage"></param>
        ''' <remarks></remarks>
        Public Sub MovePageDown(ByRef APage As clsSurveyPage)
            Dim ATempPage As clsSurveyPage = New clsSurveyPage(Me)
            ATempPage = APage
            Dim APageIndex As Integer = Pages.IndexOf(APage)
            If APageIndex < (Pages.Count - 1) Then
                Pages.RemoveAt(APageIndex)
                Pages.Insert(APageIndex + 1, ATempPage)
            End If
        End Sub
        'L0026 ==

        'L0040 ===
        ''' <summary>
        ''' Calculate Steps count for Evaluation Pipe
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function StepCount() As Integer 'L0043
            Dim aResult As Integer = 0
            If Pages.Count > 0 Then 'L0452
                Dim APage As clsSurveyPage = Pages(0)
                While APage IsNot Nothing
                    APage = APage.GetNextPage()
                    aResult += 1
                End While
            End If
            Return aResult
        End Function
        'L0040 ==

    End Class

End Namespace