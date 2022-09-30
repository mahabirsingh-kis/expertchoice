Imports Canvas.PipeParameters

Namespace ExpertChoice.Data

    Public Class clsInfodocState

        Public Property ApplicationID As Canvas.PipeParameters.ecAppliationID
        Public Property Width As Integer = 0
        Public Property Height As Integer = 0
        Public Property isCollapsed As Boolean = False

        Public Function Encode() As String
            Return String.Format("w_{0}={1}&h_{0}={2}&c_{0}=3", CInt(ApplicationID), Width, Height, CStr(IIf(isCollapsed, 1, 0)))
        End Function

        Shared Function Decode(tAppID As ecAppliationID, sParams As String, ByRef tData As clsInfodocState) As Boolean
            Dim tRes As Boolean = False
            If Not String.IsNullOrEmpty(sParams) Then
                Dim sID As String = CInt(tAppID).ToString
                Dim tParams As Specialized.NameValueCollection = System.Web.HttpUtility.ParseQueryString(sParams.Trim.ToLower)
                Dim tmpVal As Integer
                If tParams("w_" + sID) IsNot Nothing Then
                    If Integer.TryParse(tParams("w_" + sID), tmpVal) Then
                        tData.Width = tmpVal
                        tRes = True
                    End If
                End If
                If tParams("h_" + sID) IsNot Nothing Then
                    If Integer.TryParse(tParams("h_" + sID), tmpVal) Then
                        tData.Height = tmpVal
                        tRes = True
                    End If
                End If
                If tParams("c_" + sID) IsNot Nothing Then
                    If Integer.TryParse(tParams("c_" + sID), tmpVal) Then
                        tData.isCollapsed = tmpVal = 1
                        tRes = True
                    End If
                End If
            End If
            Return tRes
        End Function

        Public Sub New(Optional tAppID As Canvas.PipeParameters.ecAppliationID = ecAppliationID.appComparion, Optional tWidth As Integer = -1, Optional tHeight As Integer = -1, Optional fIsCollapsed As Boolean = False)
            ApplicationID = tAppID
            If tWidth > 0 Then Width = tWidth
            If tHeight > 0 Then Height = tHeight
            isCollapsed = fIsCollapsed
        End Sub

    End Class

End Namespace