Partial Class EULAPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_EULA)
    End Sub

    ' D0919 ===
    Dim _EULAFile As String = Nothing
    Public EULAVersion As String = _EULA_REVISION

    Public ReadOnly Property GetEULAFile() As String
        Get
            If _EULAFile Is Nothing Then
                _EULAFile = ""
                If App.ActiveWorkgroup IsNot Nothing Then _EULAFile = App.ActiveWorkgroup.EULAFile ' D0922
                If _EULAFile = "" Then _EULAFile = CStr(IIf(App.isCommercialUseEnabled, _FILE_INC_EULA, _FILE_INC_EULA_NONCOMM)) Else EULAVersion = _EULAFile ' D3475
            End If
            Return _EULAFile
        End Get
    End Property
    ' D0919 ==

    ' D4628 ===
    Public Function GetPDFFile() As String
        Dim sPDF As String = GetEULAFile
        If sPDF.ToLower.EndsWith(".html") Then sPDF = sPDF.Substring(0, sPDF.Length - 5)
        If sPDF.ToLower.EndsWith(".htm") Then sPDF = sPDF.Substring(0, sPDF.Length - 4)
        sPDF += ".pdf"
        If Not MyComputer.FileSystem.FileExists(MapPath(_URL_EULA + GetEULAFile)) Then sPDF = ""
        Return sPDF
    End Function

    Public Function GetRedirectURL() As String
        Dim sURL As String = Request.Url.AbsoluteUri
        If App.ActiveUser IsNot Nothing Then
            If App.ActiveWorkgroup Is Nothing Then
                ' D4842 ===
                Dim WkgID As Integer = App.GetLastUsedWorkgroupID(App.UserWorkgroups, App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups))
                If WkgID < 0 Then
                    App.ApplicationError.Init(ecErrorStatus.errAccessDenied, CurrentPageID, ParseAllTemplates(App.GetMessageByAuthErrorCode(ecAuthenticateError.aeUserWorkgroupExpired), App.ActiveUser, Nothing))   ' D3993
                    FetchAccess(_PGID_START)
                Else
                    App.ActiveWorkgroup = App.DBWorkgroupByID(WkgID, True, True)
                End If
            End If
            If App.ActiveWorkgroup IsNot Nothing Then
                ' D4842 ==
                If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                    sURL = PageURL(_DEF_PGID_ONSYSTEMWORKGROUP)
                Else
                    If App.Options.isSingleModeEvaluation Then
                        sURL = PageURL(_PGID_EVALUATION,, True)  ' D6359
                    Else
                        sURL = PageURL(_PGID_PROJECTSLIST)
                    End If
                End If
            End If
            Dim sSessRet As String = SessVar(_SESS_RET_URL) ' D4943
            If Not String.IsNullOrEmpty(sSessRet) Then sURL = sSessRet  ' D4943
        Else
            sURL = PageURL(_PGID_START)
        End If
        Return sURL
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False   ' D0585
        AlignVerticalCenter = False     ' D0585 + D0952
        ShowNavigation = False          ' D0285 + D0349 + D0352
        ShowTopNavigation = False       ' D6260
    End Sub

End Class