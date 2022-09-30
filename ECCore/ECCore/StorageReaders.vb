Imports ECCore
Imports ECCore.MiscFuncs
Imports Canvas
Imports System.Data.Common
Imports System.IO
Imports System.ComponentModel

<Serializable()> Public MustInherit Class clsStorageReader
    Inherits clsStorage

    Public MustOverride Function LoadProject() As Boolean

End Class
