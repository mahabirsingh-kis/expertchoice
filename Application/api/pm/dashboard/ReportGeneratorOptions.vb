Imports GemBox.Document

<Serializable> Public Class ReportGeneratorOptions 'AS/18226    ' D6455
    Public ReportScenario As Integer = 0
    Public ReportColorScheme As String = "green" 'AS/18712w

    '====== Global report settings
    Public ReportTitle As String = "Report"     ' D6455
    Public ReportFontSize As Single = 12 'make it by section
    Public ReportFontName As String = "Arial"
    Public ReportUserID As Integer = -1 'default is Combined user
    Public ShowModelDescription As Boolean = False
    Public ReportIncludeHierarchyEditable As Boolean = True

    Public ReportIncludeParts As List(Of ReportGeneratorPart)
    Public ReportIncludePartName As String = "" 'AS/18226
    Public ReportInsertPageBreaksThru As Boolean = True
    Public ReportTitleFontSize As Single = 16
    Public ReportSpaceAfterTitle As Double = 20

    Public IncludeModelDescription As Boolean = True 'AS/18226a=== include the following reports in Combined Report
    Public IncludeHierarchyEditable As Boolean = True
    Public IncludeHierarchyImage As Boolean = True 'AS/18226b
    Public IncludeGoalDescription As Boolean = True
    Public IncludeObjectivesDescriptions As Boolean = True
    Public IncludeAlternativesDescriptions As Boolean = True 'AS/18226b
    Public IncludeDataGrid As Boolean = True
    Public IncludeObjectives As Boolean = True
    Public IncludeAlternatives As Boolean = True
    Public IncludeAlternativesChart As Boolean = True
    Public IncludeSurveyResults As Boolean = True
    Public IncludeInconsistency As Boolean = True
    Public IncludeOverallResults As Boolean = True
    Public IncludeParticipants As Boolean = True       ' D7090
    Public IncludeMeasurementMethods As Boolean = True ' D7090
    Public IncludeConsensus As Boolean = True          ' D7090
    Public IncludeResourceAligner As Boolean = True    ' D7090
    Public IncludeObjectivesAlternativesPriorities As Boolean = True
    Public IncludeJudgmentsOfObjectives As Boolean = True
    Public IncludeJudgmentsOfAlternatives As Boolean = True
    Public IncludeObjectivesAndAlternatives As Boolean = True
    Public IncludeContributions As Boolean = True
    Public IncludeJudgmentsOverview As Boolean = True
    Public IncludeObjectivesPriorities As Boolean = True
    Public IncludeAlternativesPriorities As Boolean = True
    Public IncludeEvaluationProgress As Boolean = True 'AS/18226a==

    '===== header and footer
    Public ReportShowDate As Boolean = False
    Public ReportShowTime As Boolean = False
    Public ReportShowPageNumbers As Boolean = False
    Public ReportShowUserName As Boolean = False
    Public ReportShowCompanyName As Boolean = False
    'ADD LATER:
    'Ideal/distr
    'normalized/unnorm
    'include ideal alt
    'CIS
    'constructor - order of parts ReportSectionOptions

    '===== Hierarchy settings -- REVISIT: the most are no longer applicable since will insert the hierarchy as picture
    Public HierarchyStartNewPage As Boolean = True 'AS/18226 insert page break before the section
    Public HierarchyIncludeSections As List(Of Section) 'e.g. user may collapse/open some nodes to show different parts of the hierarchy; may be useful for large hierarchies.
    'Public HierarchyReportScenario As Integer = 0 'AS/18226 e.g. user may collapse/open some nodes to show different parts of the hierarchy; may be useful for large hierarchies.
    Public HierarchyFontSize As Single = ReportFontSize
    'Public HierarchyShowPriorities As Boolean = False
    'Public HierarchyShowPriorityIcons As Boolean = False
    'Public HierarchyShowTextboxBorders As Boolean = False
    'Public HierarchyShowInfodocForObjective As Boolean = False
    'Public IsCompleteHierarchy As Boolean = False 'AS/18226

    'Public HierarchyUserID As Integer = ReportUserID
    'Public HierarchyShowNodeBorders As Boolean = False

    '<NonSerialized> Public HierarchyBackcolor As Color = Color.White
    '<NonSerialized> Public NodeBackcolor As Color = Color.White
    '<NonSerialized> Public NodeForecolor As Color = Color.Black

    'Public HierarchyVerticalSpace As Double
    'Public HierarchyNodeIndent As Double = 10
    Public HierarchyNodeLeft As Double = 10
    Public HierarchyNodeTop As Double = 10
    Public HierarchyNodeWidth As Double = 120
    Public HierarchyNodeHeight As Double = 20

    'Public HierarchyShowConnectors As Boolean = True
    Public HierarchyConnectorIndent As Double = 15
    '<NonSerialized> Public HierarchyConnectorColor As Color = Color.Blue
    'Public HierarchyConnectorThickness As Double = 0

    '===== Data grid
    Public DatagridFontSize As Single = ReportFontSize
    Public DatagridNumberOfDecimals As Integer = 4
    Public DatagridShowCosts As Boolean = True
    Public DatagridShowRisks As Boolean = True
    Public DatagridShowAttributes As Boolean = False
    Public DatagridShowSelectedCovObjective As Boolean = False
    Public DatagridShowSelectedPlex As Boolean = False
    Public DatagridShowAllCovObjectives As Boolean = True
    Public DatagridShowAllAlts As Boolean = False

    Public DatagridShowDataOrValues As DatagridShowDataValues
    Public DatagridShowSelectedCovObjectives As DatagridShowSelectedCovObjs
    Public DatagridShowSelectedAlternatives As DatagridShowSelectedAlts

    '===== Documents - infodocs, comments
    Public InfodocFontSize As Single = ReportFontSize
    Public CommentFontSize As Single = ReportFontSize
    Public ShowInfodoc As Boolean = False
    Public ShowComment As Boolean = False 'comments are only for judgments
    Public DocLeftIndent As Double = 10
    Public DocRightIndent As Double = 20
    Public DocSpaceAfter As Double = 0
    Public DocSpaceBefore As Double = 0

    '===== Picture
    Public PictureTitleFontSize As Double = ReportTitleFontSize
    Public PictureWidth As Double = 500 '400  500:670 = ratio 4:6
    Public PictureHeght As Double = 670 '100 
    Public PicOrientation As PictureOrientation

    '===== Alternatives table
    Public AltsTableShowSelectedAlts As Boolean = False 'show either all or only selected alternatives
    Public AltsTableShowPriorities As Boolean = False
    Public AltsTableShowInfodocs As Boolean = False

    Public Sub New()

    End Sub

    <Serializable> Public Enum PictureOrientation As Integer
        picPortrait = 0
        picLandscape = 1
    End Enum

    <Serializable> Public Enum DatagridShowDataValues As Integer
        dgShowData = 0
        dgShowValues = 1
    End Enum

    <Serializable> Public Enum DatagridShowSelectedCovObjs As Integer
        dgShowAllCovObjectives = 0
        dgShowSelectedPlex = 1
        dgShowSelectedCovObjective = 2
    End Enum

    <Serializable> Public Enum DatagridShowSelectedAlts As Integer
        dgShowAllAlts = 0
        dgShowSelectedAlts = 1
    End Enum

    'Public Property ReportFontSize As Single
    '    Get
    '        Return _ReportFontSize
    '    End Get
    '    Set(value As Single)
    '        _ReportFontSize = value
    '    End Set
    'End Property
    'Public Property ReportFontName As String
    '    Get
    '        Return _ReportFontName
    '    End Get
    '    Set(value As String)
    '        _ReportFontName = value
    '    End Set
    'End Property
    'Public Property DatagridNumberOfDecimals As Integer
    '    Get
    '        Return _DatagridNumberOfDecimals
    '    End Get
    '    Set(value As Integer)
    '        _DatagridNumberOfDecimals = value
    '    End Set
    'End Property
    'Public Property ShowModelDescription As Boolean
    '    Get
    '        Return _ShowModelDescription
    '    End Get
    '    Set(value As Boolean)
    '        _ShowModelDescription = value
    '    End Set
    'End Property
    'Public Property HierarchyReportScenario As Integer 'e.g., user may collapse/open some nodes to show different parts of the hierarchy; may be useful for large hierarchies.
    '    Get
    '        Return _HierarchyReportScenario
    '    End Get
    '    Set(value As Integer)
    '        _HierarchyReportScenario = value
    '    End Set
    'End Property
    'Public Property HierarchyStartNewPage As Boolean 'insert page break before the section
    '    Get
    '        Return _HierarchyStartNewPage
    '    End Get
    '    Set(value As Boolean)
    '        _HierarchyStartNewPage = value
    '    End Set
    'End Property
    'Public Property HierarchyFontSize As Single
    '    Get
    '        Return _HierarchyFontSize
    '    End Get
    '    Set(value As Single)
    '        _HierarchyFontSize = value
    '    End Set
    'End Property
    'Public Property HierarchyShowPriorities As Boolean
    '    Get
    '        Return _HierarchyShowPriorities
    '    End Get
    '    Set(value As Boolean)
    '        _HierarchyShowPriorities = value
    '    End Set
    'End Property
    'Public Property HierarchyShowPriorityIcons As Boolean
    '    Get
    '        Return _HierarchyShowPriorityIcons
    '    End Get
    '    Set(value As Boolean)
    '        _HierarchyShowPriorityIcons = value
    '    End Set
    'End Property
    'Public Property HierarchyShowTextboxBorders As Boolean
    '    Get
    '        Return _HierarchyShowTextboxBorders
    '    End Get
    '    Set(value As Boolean)
    '        _HierarchyShowTextboxBorders = value
    '    End Set
    'End Property
    'Public Property HierarchyShowInfodocForObjective As Boolean
    '    Get
    '        Return _HierarchyShowInfodocForObjective
    '    End Get
    '    Set(value As Boolean)
    '        _HierarchyShowInfodocForObjective = value
    '    End Set
    'End Property
    'Public Property IsCompleteHierarchy As Boolean
    '    Get
    '        Return _IsCompleteHierarchy
    '    End Get
    '    Set(value As Boolean)
    '        _IsCompleteHierarchy = value
    '    End Set
    'End Property
End Class
