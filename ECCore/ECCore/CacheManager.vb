Imports ECCore
Imports Canvas

Namespace ECCore

    <Serializable>
    Public Class CacheManager
        Private ModelStructure As New Dictionary(Of StructureType, Nullable(Of DateTime))
        Private UserData As New Dictionary(Of Integer, Dictionary(Of UserDataType, Nullable(Of DateTime)))

        Public Sub ClearAll()
            ModelStructure.Clear()
            UserData.Clear()
        End Sub

        Public Sub ClearModelStructure()
            ModelStructure.Clear()
        End Sub

        Public Sub ClearUserData(Optional UserID As Integer = UNDEFINED_USER_ID)
            If UserID = UNDEFINED_USER_ID Then
                UserData.Clear()
            Else
                If UserData.ContainsKey(UserID) Then UserData.Remove(UserID)
            End If
        End Sub

        Public Property StructureLoaded(StructureType As StructureType) As Nullable(Of DateTime)
            Get
                Dim res As Nullable(Of DateTime)
                ModelStructure.TryGetValue(StructureType, res)
                Return res
            End Get
            Set
                If ModelStructure.ContainsKey(StructureType) Then
                    If Value Is Nothing Then
                        ModelStructure.Remove(StructureType)
                    Else
                        ModelStructure(StructureType) = Value
                    End If
                Else
                    ModelStructure.Add(StructureType, Value)
                End If
            End Set
        End Property

        Public Property UserDataLoaded(UserID As Integer, UserDataType As UserDataType) As Nullable(Of DateTime)
            Get
                Dim ud As Dictionary(Of UserDataType, Nullable(Of DateTime))
                If UserData.TryGetValue(UserID, ud) Then
                    Dim time As Nullable(Of DateTime)
                    ud.TryGetValue(UserDataType, time)
                    Return time
                Else
                    Return Nothing
                End If
            End Get
            Set
                If UserData.ContainsKey(UserID) Then
                    If UserData(UserID).ContainsKey(UserDataType) Then
                        If Value Is Nothing Then
                            UserData(UserID).Remove(UserDataType)
                        Else
                            UserData(UserID)(UserDataType) = Value
                        End If
                    Else
                        UserData(UserID).Add(UserDataType, Value)
                    End If
                Else
                    UserData.Add(UserID, New Dictionary(Of UserDataType, Nullable(Of DateTime)) From {{UserDataType, Value}})
                End If
            End Set
        End Property

        Public Sub PrintModelStructureCache()
            For Each kvp As KeyValuePair(Of StructureType, Nullable(Of DateTime)) In ModelStructure
                Debug.Print(kvp.Key.ToString + ": " + If(kvp.Value Is Nothing, "Nothing", CType(kvp.Value, DateTime).ToString("hh:mm:ss.ffff tt")))
            Next
        End Sub
    End Class
End Namespace