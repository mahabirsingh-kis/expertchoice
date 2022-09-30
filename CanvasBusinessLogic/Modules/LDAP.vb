Imports ECCore.ECTypes
Imports System.DirectoryServices

Namespace ExpertChoice.Service

    Public Module LDAP

        ''' <summary>
        ''' Returns the list of clsUser (filled only UserEmail and UserName) from LDAP under specified user
        ''' </summary>
        ''' <param name="sLDAPQuery">Should be valid, like LDAP://.../ </param>
        ''' <param name="sLDAPUserName">Use domain prefix if available</param>
        ''' <param name="sLDAPUSerPsw"></param>
        ''' <param name="sSearch"></param>
        ''' <param name="sError"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetLDAPUsersList(ByVal sLDAPQuery As String, ByVal sLDAPSearchQuery As String, ByVal sLDAPUserName As String, ByVal sLDAPUSerPsw As String, Optional ByVal sSearch As String = "", Optional ByRef sError As String = Nothing) As List(Of clsUser) 'L0425
            Dim Lst As New List(Of clsUser)
            'If sSearch = "" Then sSearch = "*" Else sSearch = "*" + sSearch + "*" 'L0410 'L0439
            Dim Entry As New DirectoryEntry(sLDAPQuery, sLDAPUserName, sLDAPUSerPsw)
            Dim UserSearcher As DirectorySearcher = New DirectorySearcher(Entry, String.Format(sLDAPSearchQuery, sSearch), Nothing, SearchScope.Subtree) 'L0425
            UserSearcher.PageSize = 500
            UserSearcher.SizeLimit = 100
            UserSearcher.Sort.PropertyName = "name"
            UserSearcher.PropertiesToLoad.Clear()
            UserSearcher.PropertiesToLoad.Add("mail")
            UserSearcher.PropertiesToLoad.Add("displayname")
            UserSearcher.PropertiesToLoad.Add("userprincipalname")
            UserSearcher.PropertiesToLoad.Add("samaccountname")
            Try
                Dim Results As SearchResultCollection = UserSearcher.FindAll()
                For Each tRecord As SearchResult In Results
                    Dim tUser As New clsUser
                    ' D1043 ===

                    If tRecord.Properties.Contains("mail") Then tUser.UserEMail = CStr(tRecord.Properties.Item("mail")(0))
                    If tUser.UserEMail = "" AndAlso tRecord.Properties.Contains("userprincipalname") Then tUser.UserEMail = CStr(tRecord.Properties.Item("userprincipalname")(0))
                    If tUser.UserEMail = "" AndAlso tRecord.Properties.Contains("samaccountname") Then tUser.UserEMail = CStr(tRecord.Properties.Item("samaccountname")(0))
                    If tRecord.Properties.Contains("displayname") Then tUser.UserName = CStr(tRecord.Properties.Item("displayname")(0))
                    ' D1043 ==
                    'tUser.UserEMail = tRecord.Properties.Item("mail")(0)
                    'tUser.UserName = tRecord.Properties.Item("name")(0)
                    'For Each sName As String In tRecord.Properties.PropertyNames
                    '    tUser.Comment += String.Format("{0}: {1}" + vbCrLf, sName, tRecord.Properties(sName)(0))
                    'Next
                    If tUser.UserEMail <> "" Then Lst.Add(tUser) ' D1043
                Next
            Catch ex As Exception
                If sError IsNot Nothing Then sError = ex.Message
            End Try
            Return Lst
        End Function

        Public Function CheckDirectoryEntry(ByVal sLDAPQuery As String, ByVal sLDAPUserName As String, ByVal sLDAPUSerPsw As String, Optional ByRef sError As String = Nothing) As Boolean
            Try
                Dim Entry As New DirectoryEntry(sLDAPQuery, sLDAPUserName, sLDAPUSerPsw)
                Return Entry.Properties.Count > 0
            Catch ex As Exception
                sError = ex.Message
                Return False
            End Try
        End Function
    End Module

End Namespace
