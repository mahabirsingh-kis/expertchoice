Imports System.Linq

Namespace ECCore
    Partial Public Class clsProjectManager
#Region "Data Instance functions - OBSOLETE. For old project versions support only"
        Public Property DataInstances() As New List(Of clsDataInstance)
        Public Property DataInstanceUsers() As New List(Of clsUser)

        Public Function GetDataInstanceUserByID(ByVal id As Integer) As clsUser
            Return DataInstanceUsers.FirstOrDefault(Function(u) (u.UserID = id))
        End Function

        Public Function DataInstanceUserExists(ByVal DIUserID As Integer) As Boolean
            Return GetDataInstanceByID(DIUserID) IsNot Nothing
        End Function

        Public Function GetDataInstanceByID(ByVal id As Integer) As clsDataInstance
            Return DataInstances.FirstOrDefault(Function(di) (di.ID = id))
        End Function

        Public Function GetDataInstanceByUserID(ByVal UserID As Integer) As clsDataInstance
            Return DataInstances.FirstOrDefault(Function(di) (di.User.UserID = UserID))
        End Function

        Public Function AddDataInstance(ByVal Name As String) As clsDataInstance
            Dim DI As New clsDataInstance

            Dim maxID As Integer = -1
            Dim minUserID As Integer = -1

            For Each dInstance As clsDataInstance In DataInstances
                If dInstance.ID > maxID Then maxID = dInstance.ID
                If dInstance.User IsNot Nothing Then
                    If dInstance.User.UserID < minUserID Then
                        minUserID = dInstance.User.UserID
                    End If
                End If
            Next

            DI.ID = maxID + 1
            DI.Name = Name
            DI.Comment = ""

            Dim diUser As New clsUser
            diUser.UserID = minUserID - 1
            diUser.UserName = "DataInstanceUser" + DI.ID.ToString
            diUser.UserEMail = "DataInstanceUser" + DI.ID.ToString + "@expertchoice.com"
            diUser.LastJudgmentTime = Now

            DI.User = diUser

            DataInstanceUsers.Add(diUser)
            DataInstances.Add(DI)

            Return DI
        End Function

        Public Function CreateDataInstanceFromUser(ByVal AUser As clsUser) As clsDataInstance
            If AUser Is Nothing Then Return Nothing
            If Not UserExists(AUser.UserID) Then Return Nothing
            Dim di As clsDataInstance = AddDataInstance("DataInstanceFromUser_" + AUser.UserName)
            CopyUserJudgments(AUser, di.User)
            Return di
        End Function

#End Region

    End Class
End Namespace
