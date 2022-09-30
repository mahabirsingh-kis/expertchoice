Imports System.Runtime.Serialization
Imports System.Text
Imports ExpertChoice.Misc

Namespace ECCore

    <Serializable()> _
    Public Class AlternativeRiskDataDataContract
        Private _AlternativeID As Guid
        Private _intID As Integer
        Private _RiskValue As Double
        Private _LikelihoodValue As Double
        Private _WRTGlobalPriorityLikelihood As Double
        Private _NormalizedLikelihood As Double
        Private _ImpactValue As Double
        Private _WRTGlobalPriorityImpact As Double
        Private _NormalizedImpact As Double

        <DataMember()> Public Property AlternativeID() As Guid
            Get
                Return _AlternativeID
            End Get
            Set(value As Guid)
                _AlternativeID = value
            End Set
        End Property

        <DataMember()> Public Property intID() As Integer
            Get
                Return _intID
            End Get
            Set(value As Integer)
                _intID = value
            End Set
        End Property

        <DataMember()> Public Property RiskValue() As Double
            Get
                Return _RiskValue
            End Get
            Set(value As Double)
                _RiskValue = value
            End Set
        End Property

        <DataMember()> Public Property LikelihoodValue() As Double
            Get
                Return _LikelihoodValue
            End Get
            Set(value As Double)
                _LikelihoodValue = value
            End Set
        End Property

        <DataMember()> Public Property WRTGlobalPriorityLikelihood() As Double
            Get
                Return _WRTGlobalPriorityLikelihood
            End Get
            Set(value As Double)
                _WRTGlobalPriorityLikelihood = value
            End Set
        End Property

        <DataMember()> Public Property ImpactValue() As Double
            Get
                Return _ImpactValue
            End Get
            Set(value As Double)
                _ImpactValue = value
            End Set
        End Property

        <DataMember()> Public Property WRTGlobalPriorityImpact() As Double
            Get
                Return _WRTGlobalPriorityImpact
            End Get
            Set(value As Double)
                _WRTGlobalPriorityImpact = value
            End Set
        End Property

        Public SumLikelihood As Double = 1
        Public MaxLikelihood As Double = 1

        Public SumImpact As Double = 1
        Public MaxImpact As Double = 1

        Public MaxRisk As Double = 1

        Public AverageLoss As Double = 0
    End Class

    <Serializable(), KnownType(GetType(AlternativeRiskDataDataContract))> _
    Public Class RiskDataWRTNodeDataContract
        <DataMember()> Public Property AlternativesData As List(Of AlternativeRiskDataDataContract)
        '<DataMember()> Public Property IsSOSG1() As Boolean = False 'Sum of some sources greater than 1
        '<DataMember()> Public Property IsSOEG1() As Boolean = False 'Sum of events (Likelihood) greater than 1
        '<DataMember()> Public Property IsSOOG1() As Boolean = False 'Sum of some objectives (impact hierarchy) greater than 1
        '<DataMember()> Public Property IsSOIG1() As Boolean = False 'Sum of impacts (Impact events) greater than 1
        '<DataMember()> Public Property IsEG1() As Boolean = False   'One of events has likelihood greater than 1
        '<DataMember()> Public Property IsIG1() As Boolean = False   'One of events has impact greater than 1
    End Class

    <Serializable(), KnownType(GetType(RiskDataWRTNodeDataContract)), KnownType(GetType(clsBowTieData2))> _
    Public Class clsBowTieDataContract2
        <DataMember()> Public Property UserID As Integer
        <DataMember()> Public Property AlternativesTableData As RiskDataWRTNodeDataContract
        <DataMember()> Public Property BowTieData As Dictionary(Of Guid, clsBowTieData2)
    End Class

    <Serializable(), KnownType(GetType(clsBowTiePriority))> _
    Public Class clsBowTieData2
        <DataMember()> Public Property LikelihoodContributedNodeIDs As List(Of Guid)
        <DataMember()> Public Property LikelihoodValues As List(Of clsBowTiePriority)
        <DataMember()> Public Property ImpactContributedNodeIDs As List(Of Guid)
        <DataMember()> Public Property ImpactValues As List(Of clsBowTiePriority)
    End Class

    <Serializable()> _
    Public Class clsBowTiePriority
        <DataMember()> Public Property CovObjID As Guid

        <DataMember()> Public Property CovObjValueAbsolute As Double
        <DataMember()> Public Property AltWRTCovObjValueAbsolute As Double
        <DataMember()> Public Property MultipliedValueAbsolute As Double

        <DataMember()> Public Property CovObjValueRelative As Double
        <DataMember()> Public Property AltWRTCovObjValueRelative As Double
        <DataMember()> Public Property MultipliedValueRelative As Double
    End Class

    <Serializable()> _
    Public Class ControlSortAttribute

        <DataMember()> Public Property ObjectiveID As Guid
        <DataMember()> Public Property EventID As Guid
        <DataMember()> Public Property SortString As String

    End Class

    Public Class clsRiskData
        Public Property Name() As String
        Private _RiskValue As Double = 0
        Public Property RiskValue() As Double
            Get
                Return _RiskValue
            End Get
            Set(value As Double)
                _RiskValue = value
            End Set
        End Property

        Private _ImpactValue As Double = 0
        Public Property ImpactValue() As Double
            Get
                Return _ImpactValue
            End Get
            Set(value As Double)
                _ImpactValue = value
            End Set
        End Property

        Private _LikelihoodValue As Double = 0
        Public Property LikelihoodValue() As Double
            Get
                Return _LikelihoodValue
            End Get
            Set(value As Double)
                _LikelihoodValue = value
            End Set
        End Property

        Public Property OriginalImpactValue() As Double
        Public Property OriginalLikelihoodValue() As Double
        Public Property OriginalRiskValue() As Double

        Public Property WRTGlobalPriorityImpact() As Double
        Public Property WRTGlobalPriorityLikelihood() As Double

        Public Property SumLikelihood() As Double = 0
        Public Property MaxLikelihood() As Double = 0

        Public Property ID() As Guid
        Public Property intID() As Integer = 0

        Public Property Causes As New List(Of clsPriorityData)
        Public Property Consequences As New List(Of clsPriorityData)

        Public Property HasContributionsL As Boolean = True
        Public Property HasContributionsI As Boolean = True
    End Class

    Public Class clsPriorityData
        Public GuidID As Guid = Guid.Empty
        Public Title As String = ""
        Public NodePath As String = ""
        Public InfoDoc As String = ""
        Public RiskNodeType As ECTypes.RiskNodeType = RiskNodeType.ntUncertainty

        Public PriorityL As Double
        Public PriorityV As Double
        Public PriorityLV As Double

        Public IsVisible As Boolean = False

        Public Sub New()
            PriorityL = 0
            PriorityV = 0
            PriorityLV = 0
            IsVisible = False
        End Sub

        Public Sub New(Id As Guid, name As String)
            Me.New()
            Me.GuidID = Id
            Me.Title = name
        End Sub
    End Class

    Public Class ExportRiskData
        Public EventName As String

        Public Property Causes As New List(Of clsPriorityData)
        Public Property Consequences As New List(Of clsPriorityData)

        Public SumCausesPriority As Double
        Public SumConsequencesPriority As Double
        Public RiskPriority As Double

    End Class

    Public Class ExportModelRisk
        Private Const _VAR_AUTHOR As String = "%%AUTHOR%%"
        Private Const _VAR_DATE As String = "%%DATE%%"
        Private Const _VAR_TIME As String = "%%TIME%%"
        Private Const _VAR_ExpandedColumnCount As String = "%%ExpandedColumnCount%%"
        Private Const _VAR_ExpandedRowCount As String = "%%ExpandedRowCount%%"
        Private Const _VAR_COLUMNS_DATA As String = "%%COLUMNS_DATA%%"
        Private Const _VAR_OPTIONS_DATA As String = "%%OPTIONS_DATA%%"
        Private Const _VAR_TABLE_DATA As String = "%%TABLE_DATA%%"
        Private Const _VAR_FOOTER_DATA As String = "%%FOOTER_DATA%%"
        Private Const _VAR_OBJECTIVES As String = "%%OBJECTIVES%%"

        Private Const _DEFAULT_TITLE_COLUMN_WIDTH As Integer = 120
        Private Const _DEFAULT_PRIORITY_COLUMN_WIDTH As Integer = 70

        Private RowCount As Integer
        Private ColCount As Integer

        Private _Source As List(Of ExportRiskData)
        Public Property Source() As List(Of ExportRiskData)
            Get
                Return _Source
            End Get
            Set(ByVal value As List(Of ExportRiskData))
                _Source = value
            End Set
        End Property

        Public Sub New(ByVal _source As List(Of ExportRiskData))
            Me.Source = _source
            Me.RowCount = 0
            Me.ColCount = 0
        End Sub

        Public Function ExportToExcelXML(data As String) As String
            Dim sResult As String = ""

            data = data.Replace(_VAR_AUTHOR, "")
            data = data.Replace(_VAR_DATE, DateTime.Now.ToShortDateString)
            data = data.Replace(_VAR_TIME, DateTime.Now.ToShortTimeString)

            data = data.Replace(_VAR_COLUMNS_DATA, GetColumns)
            data = data.Replace(_VAR_OPTIONS_DATA, "")
            data = data.Replace(_VAR_FOOTER_DATA, "")
            data = data.Replace(_VAR_OBJECTIVES, "")
            'Table data
            Dim sTable As String = ""
            For Each dataItem As ExportRiskData In Source
                sTable += GetTable(dataItem)
            Next

            data = data.Replace(_VAR_TABLE_DATA, sTable)

            RowCount += 1
            If ColCount < 3 Then ColCount = 3
            data = data.Replace(_VAR_ExpandedColumnCount, ColCount.ToString)
            data = data.Replace(_VAR_ExpandedRowCount, RowCount.ToString)

            sResult = data.ToString

            Return sResult
        End Function

        Private Function GetColumns() As String
            Dim sb As StringBuilder = New StringBuilder()
            'Causes
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_TITLE_COLUMN_WIDTH.ToString)
            ColCount += 1
            sb.AppendLine("")
            'a
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'b
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'a * b
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'Event
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_TITLE_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'c * d
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'd
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'c
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_PRIORITY_COLUMN_WIDTH.ToString)
            sb.AppendLine("")
            ColCount += 1
            'Consequences
            sb.AppendFormat("<Column ss:AutoFitWidth=""0"" ss:Width=""{0}""/>", _DEFAULT_TITLE_COLUMN_WIDTH.ToString)
            ColCount += 1
            sb.AppendLine("")
            Return sb.ToString()
        End Function

        Private Function GetTable(data As ExportRiskData) As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.AppendLine("<Row ss:AutoFitHeight=""1"">")
            sb.AppendFormat("<Cell ss:StyleID=""s63lkl""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblObjectivesSources%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63lkl""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblRiskLegendA%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63lkl""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblRiskLegendB%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63lkl""><Data ss:Type=""String"">{0}</Data></Cell>", "%%optSortByCauseLkh%%")
            sb.AppendFormat("<Cell ss:StyleID=""s69event""><Data ss:Type=""String"">{0}</Data></Cell>", data.EventName)
            sb.AppendFormat("<Cell ss:StyleID=""s63imp""><Data ss:Type=""String"">{0}</Data></Cell>", "%%optSortByConsequenceImp%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63imp""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblRiskLegendC%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63imp""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblRiskLegendD%%")
            sb.AppendFormat("<Cell ss:StyleID=""s63imp""><Data ss:Type=""String"">{0}</Data></Cell>", "%%lblConsequences%%")
            sb.AppendLine("</Row>")
            RowCount += 1

            Dim i As Integer = 0
            Dim j As Integer = 0
            Dim Count As Integer = CInt(If(data.Causes.Count > data.Consequences.Count, data.Causes.Count, data.Consequences.Count))

            For k As Integer = 0 To Count - 1
                sb.AppendLine("<Row ss:AutoFitHeight=""1"">")

                If i < data.Causes.Count Then
                    sb.AppendFormat("<Cell ss:StyleID=""s64text""><Data ss:Type=""String"">{0}</Data></Cell>", data.Causes(i).Title)
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Causes(i).PriorityL * 100).Replace(",", "."))
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Causes(i).PriorityV * 100).Replace(",", "."))
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Causes(i).PriorityLV * 100).Replace(",", "."))
                    i += 1
                Else
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                End If

                sb.AppendFormat("<Cell ss:StyleID=""s69""><Data ss:Type=""String"" /></Cell>")

                If j < data.Consequences.Count Then
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Consequences(j).PriorityLV * 100).Replace(",", "."))
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Consequences(j).PriorityV * 100).Replace(",", "."))
                    sb.AppendFormat("<Cell ss:StyleID=""s64val""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.Consequences(j).PriorityL * 100).Replace(",", "."))
                    sb.AppendFormat("<Cell ss:StyleID=""s64text""><Data ss:Type=""String"">{0}</Data></Cell>", data.Consequences(j).Title)

                    j += 1
                Else
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                    sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
                End If

                sb.AppendLine("</Row>")
                RowCount += 1
            Next

            sb.AppendLine("<Row ss:AutoFitHeight=""0"">")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s63lkl""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.SumCausesPriority * 100).Replace(",", "."))
            sb.AppendFormat("<Cell ss:StyleID=""s69risk""><Data ss:Type=""String"">{0}</Data></Cell>", "Risk (L * I)")
            sb.AppendFormat("<Cell ss:StyleID=""s63imp""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.SumConsequencesPriority * 100).Replace(",", "."))
            sb.AppendLine("</Row>")
            RowCount += 1

            sb.AppendLine("<Row ss:AutoFitHeight=""0"">")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendFormat("<Cell ss:StyleID=""s63risk""><Data ss:Type=""Number"">{0}</Data></Cell>", ExpertChoice.Service.Double2String(data.RiskPriority * 100).Replace(",", "."))
            sb.AppendLine("</Row>")
            RowCount += 1

            sb.AppendLine("<Row ss:AutoFitHeight=""0"">")
            sb.AppendFormat("<Cell ss:StyleID=""s69emp""><Data ss:Type=""String"" /></Cell>")
            sb.AppendLine("</Row>")
            RowCount += 1

            Return sb.ToString()
        End Function

    End Class

End Namespace
