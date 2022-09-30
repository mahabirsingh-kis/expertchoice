Imports ECCore

Namespace ECCore
    <Serializable()> Public MustInherit Class clsStorageWriter
        Inherits clsStorage

        Public MustOverride Function SaveProject(Optional ByVal StructureOnly As Boolean = False) As Boolean
    End Class
End Namespace