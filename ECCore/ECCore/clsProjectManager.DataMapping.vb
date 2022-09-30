Imports System.Linq

Namespace ECCore
    Partial Public Class clsProjectManager
        Public Property UseDataMapping As Boolean = False
        Public Property DataMappings() As New List(Of clsDataMapping)
        Public Function AddDataMapping(DBType As clsDataMapping.enumMappedDBType, DBconnString As String, DBname As String, TblName As String, ColName As String, MapkeyColName As String) As clsDataMapping 'AS/12323xe
            Dim DM As New clsDataMapping(DBType, DBconnString, DBname, TblName, ColName, MapkeyColName)
            DataMappings.Add(DM)
            Return DM
        End Function
    End Class

End Namespace
