Imports ECCore
Imports Canvas
Imports System.Runtime.Serialization
'Imports System.ServiceModel
'Imports System.ServiceModel.Activation
Imports System.Threading
Imports System.IO 'C0606
Imports System.Data.Common 'C0606
Imports System.ComponentModel 'C0606
'Imports System.Drawing 'C0606

Namespace Canvas
    <DataContract()> _
    Public Enum GUILocation As Integer 'C0600
        <EnumMember()> None = -1
        <EnumMember()> Treeview = 0
        <EnumMember()> Board = 1
        <EnumMember()> RecycleBin = 2
        <EnumMember()> Alternatives = 4 'C0638
        <EnumMember()> BoardImpact = 5  'A0812
    End Enum

    <DataContract()> _
    Public Enum MeetingEntry As Integer
        <EnumMember()> _
        Joined = 1
        <EnumMember()> _
        Exited = 2
    End Enum

    <DataContract()> _
    <Serializable()> Public Class clsVisualNodeAttributes 'C0600
        Private mX As Double
        <DataMember()> _
        Public Property X() As Double
            Get
                If mX = UNDEFINED_INTEGER_VALUE Then mX = 0
                Return mX
            End Get
            Set(ByVal value As Double)
                mX = value
            End Set
        End Property

        Private mY As Double
        <DataMember()> _
        Public Property Y() As Double
            Get
                If mY = UNDEFINED_INTEGER_VALUE Then mY = 0
                Return mY
            End Get
            Set(ByVal value As Double)
                mY = value
            End Set
        End Property

        Private mHeight As Double
        <DataMember()> _
        Public Property Height() As Double
            Get
                Return mHeight
            End Get
            Set(ByVal value As Double)
                mHeight = value
            End Set
        End Property

        Private mWidth As Double
        <DataMember()> _
        Public Property Width() As Double
            Get
                Return mWidth
            End Get
            Set(ByVal value As Double)
                mWidth = value
            End Set
        End Property

        Private mBackGroundColor As Integer 'C0607
        <DataMember()> _
        Public Property BackGroundColor() As Integer 'C0607
            Get
                Return mBackGroundColor
            End Get
            Set(ByVal value As Integer)
                mBackGroundColor = value
            End Set
        End Property

        Private mForeGroundColor As Integer 'C0607
        <DataMember()> _
        Public Property ForeGroundColor() As Integer 'C0607
            Get
                Return mForeGroundColor
            End Get
            Set(ByVal value As Integer)
                mForeGroundColor = value
            End Set
        End Property

        Private mFont As String 'C0607
        <DataMember()> _
        Public Property Font() As String 'C0607
            Get
                Return mFont
            End Get
            Set(ByVal value As String)
                mFont = value
            End Set
        End Property

        Public Function Clone() As clsVisualNodeAttributes 'C0659
            Return CType(Me.MemberwiseClone(), clsVisualNodeAttributes)
        End Function
    End Class

    <DataContract()> _
    <Serializable()> Public Class clsVisualNode 'C0600

        'C0638===
        'Private mID As Integer
        '<DataMember()> _
        'Public Property ID() As Integer
        '    Get
        '        Return mID
        '    End Get
        '    Set(ByVal value As Integer)
        '        mID = value
        '    End Set
        'End Property

        'Private mParentNodeID As Integer
        '<DataMember()> _
        'Public Property ParentNodeID() As Integer
        '    Get
        '        Return mParentNodeID
        '    End Get
        '    Set(ByVal value As Integer)
        '        mParentNodeID = value
        '    End Set
        'End Property
        'C0638==

        Private mGuidID As Guid
        <DataMember()> _
        Public Property GuidID() As Guid
            Get
                Return mGuidID
            End Get
            Set(ByVal value As Guid)
                mGuidID = value
            End Set
        End Property

        Private mParentGuidID As Guid 'C0640
        <DataMember()> _
        Public Property ParentGuidID() As Guid
            Get
                Return mParentGuidID
            End Get
            Set(ByVal value As Guid)
                mParentGuidID = value
            End Set
        End Property

        Private mText As String
        <DataMember()> _
        Public Property Text() As String
            Get
                Return mText
            End Get
            Set(ByVal value As String)
                mText = value
            End Set
        End Property

        Private mLocation As GUILocation = GUILocation.Board
        <DataMember()> _
        Public Property Location() As GUILocation
            Get
                Return mLocation
            End Get
            Set(ByVal value As GUILocation)
                mLocation = value
            End Set
        End Property

        'C0602===
        'Private mPrevLocation As GUILocation
        '<DataMember()> _
        'Public Property PreviousLocation() As GUILocation
        '    Get
        '        Return mPrevLocation
        '    End Get
        '    Set(ByVal value As GUILocation)
        '        mPrevLocation = value
        '    End Set
        'End Property
        'C0602==

        Private mAuthor As String
        <DataMember()> _
        Public Property Author() As String
            Get
                Return mAuthor
            End Get
            Set(ByVal value As String)
                mAuthor = value
            End Set
        End Property

        Private mLastModifiedBy As String
        <DataMember()> _
        Public Property LastModifiedBy() As String
            Get
                Return mLastModifiedBy
            End Get
            Set(ByVal value As String)
                mLastModifiedBy = value
            End Set
        End Property

        Private mAttributes As clsVisualNodeAttributes
        <DataMember()> _
        Public Property Attributes() As clsVisualNodeAttributes
            Get
                Return mAttributes
            End Get
            Set(ByVal value As clsVisualNodeAttributes)
                mAttributes = value
            End Set
        End Property

        Private mHasInfoDoc As Boolean = False 'A0346
        <DataMember()> _
        Public Property HasInfoDoc() As Boolean 'C0828 'A0346
            Get
                'mHasInfoDoc = mInfoDoc <> ""
                Return mHasInfoDoc
            End Get
            Set(ByVal value As Boolean)
                mHasInfoDoc = value
            End Set
        End Property

        Private mInfoDoc As String
        <DataMember()> _
        Public Property InfoDoc() As String 'C0829
            Get
                Return mInfoDoc
            End Get
            Set(ByVal value As String)
                mInfoDoc = value
                mHasInfoDoc = value <> ""     ' D1083
            End Set
        End Property

        Public Sub New()
            mAttributes = New clsVisualNodeAttributes 'C0606
            mLastModifiedBy = "" 'C0607
            mAuthor = "" 'C0607
            mText = "" 'C0607
            'mParentNodeID = -1 'C0608 'C0638
            ProsList = New List(Of clsVisualNode)
            ConsList = New List(Of clsVisualNode)
            Comments = New Dictionary(Of Guid, String)
        End Sub

        Public Function Clone() As clsVisualNode 'A0107
            'Return CType(Me.MemberwiseClone(), clsVisualNode)

            'C0659===
            Dim res As clsVisualNode = CType(Me.MemberwiseClone(), clsVisualNode)
            res.Attributes = Me.Attributes.Clone
            Return res
            'C0659==
        End Function

        'A0393 ===
        Private _ConsList As New List(Of clsVisualNode)
        <DataMember()> _
        Public Property ConsList() As List(Of clsVisualNode)
            Get
                Return _ConsList
            End Get
            Set(ByVal value As List(Of clsVisualNode))
                _ConsList = value
            End Set
        End Property

        Private _ProsList As New List(Of clsVisualNode)
        <DataMember()> _
        Public Property ProsList() As List(Of clsVisualNode)
            Get
                Return _ProsList
            End Get
            Set(ByVal value As List(Of clsVisualNode))
                _ProsList = value
            End Set
        End Property

        Private _Comments As New Dictionary(Of Guid, String)
        <DataMember()> _
        Public Property Comments() As Dictionary(Of Guid, String)
            Get
                Return _Comments
            End Get
            Set(ByVal value As Dictionary(Of Guid, String))
                _Comments = value
            End Set
        End Property

        Private _IsProsConsPaneVisible As Boolean = False
        <DataMember()> _
        Public Property IsProsConsPaneVisible() As Boolean
            Get
                Return _IsProsConsPaneVisible
            End Get
            Set(ByVal value As Boolean)
                _IsProsConsPaneVisible = value
            End Set
        End Property

        Private _IsAlternative As Boolean = False
        <DataMember()> _
        Public Property IsAlternative() As Boolean
            Get
                Return _IsAlternative
            End Get
            Set(ByVal value As Boolean)
                _IsAlternative = value
            End Set
        End Property

        Private _WasAlreadyMovedToHierarchy As Boolean = False
        <DataMember()> _
        Public Property WasAlreadyMovedToHierarchy() As Boolean
            Get
                Return _WasAlreadyMovedToHierarchy
            End Get
            Set(ByVal value As Boolean)
                _WasAlreadyMovedToHierarchy = value
            End Set
        End Property
        'A0393 ==

        <DataMember()> Public Property ChildrenAlts As New List(Of Guid)
    End Class

    <Serializable()> Public Class clsAntiguaPanel 'C0600
        'C0606===
        Private Const DUMMY_INTEGER As Integer = 0 'C0638

        Private Const CHUNK_NODE As Integer = 1000
        Private Const CHUNK_NODE_ID As Integer = 1001
        Private Const CHUNK_NODE_GUID_ID As Integer = 1002
        Private Const CHUNK_NODE_TEXT As Integer = 1003
        Private Const CHUNK_NODE_AUTHOR As Integer = 1004
        Private Const CHUNK_NODE_LAST_MODIFIED_BY As Integer = 1005
        Private Const CHUNK_NODE_LOCATION As Integer = 1006
        Private Const CHUNK_NODE_PARENT_NODE_ID As Integer = 1007

        Private Const CHUNK_NODE_ATTRIBUTES As Integer = 2000
        Private Const CHUNK_NODE_ATTRIBUTE_X As Integer = 2001
        Private Const CHUNK_NODE_ATTRIBUTE_Y As Integer = 2002
        Private Const CHUNK_NODE_ATTRIBUTE_WIDTH As Integer = 2003
        Private Const CHUNK_NODE_ATTRIBUTE_HIGHT As Integer = 2004
        Private Const CHUNK_NODE_ATTRIBUTE_BACKGROUND_COLOR As Integer = 2005
        Private Const CHUNK_NODE_ATTRIBUTE_FOREGROUND_COLOR As Integer = 2006
        Private Const CHUNK_NODE_ATTRIBUTE_FONT As Integer = 2007
        'C0606==

        Private Const PARENT_GUID_COMMENT As String = "PARENT_GUID_COMMENT"

        Private mPrjManager As clsProjectManager

        Private mNodes As List(Of clsVisualNode)

        Private mPanelType As GUILocation 'C0606

        Private mPanelLoaded As Boolean = False ' D4954

        ' D4954 ===
        Public ReadOnly Property IsPanelLoaded As Boolean
            Get
                Return mPanelLoaded
            End Get
        End Property
        ' D4954 ==

        Public Property PanelType() As GUILocation 'C0606
            Get
                Return mPanelType
            End Get
            Set(ByVal value As GUILocation)
                mPanelType = value
            End Set
        End Property

        Public Property Nodes() As List(Of clsVisualNode)
            Get
                Return mNodes
            End Get
            Set(ByVal value As List(Of clsVisualNode))
                mNodes = value
            End Set
        End Property

        'C0638===
        'Public Function GetNodeByID(ByVal NodeID As Integer) As clsVisualNode
        '    For Each vNode As clsVisualNode In mNodes
        '        If vNode.ID = NodeID Then
        '            Return vNode
        '        End If
        '    Next
        '    Return Nothing
        'End Function

        'Public Sub RemoveNodeByID(ByVal NodeID As Integer)
        '    For i As Integer = mNodes.Count - 1 To 0 Step -1
        '        If mNodes(i).ID = NodeID Then
        '            mNodes.RemoveAt(i)
        '            Return
        '        End If
        '    Next
        'End Sub
        'C0638==

        Public Function GetNodeByTitle(ByVal NodeTitle As String) As clsVisualNode
            NodeTitle = NodeTitle.Trim
            If NodeTitle <> "" Then
                For Each vNode As clsVisualNode In mNodes
                    If vNode.Text.Trim = NodeTitle Then
                        Return vNode
                    End If
                Next
            End If
            Return Nothing
        End Function


        'C0638===
        Public Function GetNodeByGuid(ByVal NodeGuid As Guid) As clsVisualNode
            For Each vNode As clsVisualNode In mNodes
                If vNode.GuidID = NodeGuid Then
                    Return vNode
                End If
            Next
            Return Nothing
        End Function

        Public Sub RemoveNodeByGuid(ByVal NodeGuid As Guid)
            For i As Integer = mNodes.Count - 1 To 0 Step -1
                If mNodes(i).GuidID = NodeGuid Then
                    mNodes.RemoveAt(i)
                    Return
                End If
            Next
        End Sub
        'C0638==

        Public Function AddNode(ByVal node As clsVisualNode) As clsVisualNode
            'node.ParentNodeID = -1 'C0638
            'node.GuidID = New Guid 'C0609

            mNodes.Add(node)
            Return node
        End Function

        'Public Sub New(ByVal ProjectManager As clsProjectManager) 'C0606
        Public Sub New(ByVal ProjectManager As clsProjectManager, ByVal PanelType As GUILocation) 'C0606
            mPrjManager = ProjectManager
            mNodes = New List(Of clsVisualNode)
            mPanelType = PanelType 'C0606
        End Sub

        Private Function CreateStream() As MemoryStream 'C0606
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            For Each node As clsVisualNode In Nodes
                BW.Write(CHUNK_NODE)

                'BW.Write(node.ID) 'C0638
                BW.Write(DUMMY_INTEGER) 'C0638
                BW.Write(node.GuidID.ToByteArray)

                'A0396 ===
                BW.Write(node.IsAlternative)
                BW.Write(node.WasAlreadyMovedToHierarchy) 'A0399
                BW.Write(node.IsProsConsPaneVisible) 'A0399

                If node.Comments Is Nothing Then node.Comments = New Dictionary(Of Guid, String)
                BW.Write(node.Comments.Count + 1)
                For Each comment As KeyValuePair(Of Guid, String) In node.Comments
                    BW.Write(comment.Key.ToByteArray)
                    BW.Write(If(comment.Value Is Nothing, "", comment.Value))
                Next
                BW.Write(node.ParentGuidID.ToByteArray)
                BW.Write(PARENT_GUID_COMMENT)

                If node.ProsList Is Nothing Then node.ProsList = New List(Of clsVisualNode)
                BW.Write(node.ProsList.Count)
                For Each Pro As clsVisualNode In node.ProsList
                    BW.Write(Pro.GuidID.ToByteArray)

                    BW.Write(Pro.IsAlternative)
                    BW.Write(Pro.WasAlreadyMovedToHierarchy)

                    If Pro.Comments Is Nothing Then Pro.Comments = New Dictionary(Of Guid, String)
                    BW.Write(Pro.Comments.Count)
                    For Each comment As KeyValuePair(Of Guid, String) In Pro.Comments
                        BW.Write(comment.Key.ToByteArray)
                        BW.Write(If(comment.Value Is Nothing, "", comment.Value))
                    Next

                    BW.Write(If(Pro.Author Is Nothing, "", Pro.Author))
                    BW.Write(If(Pro.LastModifiedBy Is Nothing, "", Pro.LastModifiedBy))
                    BW.Write(If(Pro.Text Is Nothing, "", Pro.Text))
                Next

                If node.ConsList Is Nothing Then node.ConsList = New List(Of clsVisualNode)
                BW.Write(node.ConsList.Count)
                For Each Con As clsVisualNode In node.ConsList
                    BW.Write(Con.GuidID.ToByteArray)

                    BW.Write(Con.IsAlternative)
                    BW.Write(Con.WasAlreadyMovedToHierarchy)

                    If Con.Comments Is Nothing Then Con.Comments = New Dictionary(Of Guid, String)
                    BW.Write(Con.Comments.Count)
                    For Each comment As KeyValuePair(Of Guid, String) In Con.Comments
                        BW.Write(comment.Key.ToByteArray)
                        BW.Write(If(comment.Value Is Nothing, "", comment.Value))
                    Next

                    BW.Write(If(Con.Author Is Nothing, "", Con.Author))
                    BW.Write(If(Con.LastModifiedBy Is Nothing, "", Con.LastModifiedBy))
                    BW.Write(If(Con.Text Is Nothing, "", Con.Text))
                Next

                'A0396 ==

                'BW.Write(node.Author) 'C0607
                BW.Write(If(node.Author Is Nothing, "", node.Author)) 'C0607
                'BW.Write(node.LastModifiedBy) 'C0607
                BW.Write(If(node.LastModifiedBy Is Nothing, "", node.LastModifiedBy)) 'C0607
                'BW.Write(node.Text) 'C0607
                BW.Write(If(node.Text Is Nothing, "", node.Text)) 'C0607

                'BW.Write(node.ParentNodeID) 'C0638
                BW.Write(DUMMY_INTEGER) 'C0638

                BW.Write(node.Location)

                BW.Write(CHUNK_NODE_ATTRIBUTES)

                BW.Write(node.Attributes.X)
                BW.Write(node.Attributes.Y)
                BW.Write(node.Attributes.Width)
                BW.Write(node.Attributes.Height)
                BW.Write(node.Attributes.BackGroundColor)
                BW.Write(node.Attributes.ForeGroundColor)
                'A0076 Dim fontString As String = tc.ConvertToString(node.Attributes.Font)
                BW.Write(If(node.Attributes.Font Is Nothing, "", node.Attributes.Font)) 'C0607
            Next

            BW.Close()
            Return MS
        End Function

        Private Function GetStructureType() As StructureType
            Dim sType As StructureType
            If mPrjManager.ActiveHierarchy = ECHierarchyID.hidImpact Then
                Select Case PanelType
                    Case GUILocation.Board
                        sType = StructureType.stAntiguaDashboard 'A0812
                    Case GUILocation.Treeview
                        sType = StructureType.stAntiguaTreeViewImpact
                    Case GUILocation.RecycleBin
                        sType = StructureType.stAntiguaRecycleBinImpact
                    Case Else
                        sType = -1
                End Select
            Else
                Select Case PanelType
                    Case GUILocation.Board
                        sType = StructureType.stAntiguaDashboard
                    Case GUILocation.Treeview
                        sType = StructureType.stAntiguaTreeView
                    Case GUILocation.RecycleBin
                        sType = StructureType.stAntiguaRecycleBin
                    Case Else
                        sType = -1
                End Select
            End If
            Return sType
        End Function

        Private Function WriteToStreamsDatabase(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim MS As MemoryStream = CreateStream()

            Dim sType As StructureType = GetStructureType()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim transaction As DbTransaction = Nothing

                Try
                    transaction = dbConnection.BeginTransaction
                    oCommand.Transaction = transaction

                    oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", sType))

                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                    If MS.ToArray.Length <> 0 Then 'C0612
                        oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", sType))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    End If

                    transaction.Commit()
                Catch ex As Exception
                    If transaction IsNot Nothing Then transaction.Rollback()
                Finally
                    oCommand = Nothing
                    transaction.Dispose()
                    dbConnection.Close()
                End Try
            End Using

            Return True
        End Function

        Private Function WriteToOldFileFormat(ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0783
            Dim MS As MemoryStream = CreateStream()

            Dim sType As StructureType = GetStructureType()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim affected As Integer
                If MS.ToArray.Length <> 0 Then
                    oCommand.CommandText = "INSERT INTO AntiguaInfo (StructureType, StreamSize, Stream) VALUES (?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", sType))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))

                    'C0
                    If ProviderType = DBProviderType.dbptODBC Then
                        Dim streamParam As Odbc.OdbcParameter = oCommand.CreateParameter
                        streamParam.OdbcType = Odbc.OdbcType.Image
                        streamParam.ParameterName = "Stream"
                        streamParam.Size = MS.ToArray.Length
                        streamParam.Value = MS.ToArray
                        oCommand.Parameters.Add(streamParam)
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    End If

                    'oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                End If

            End Using

            Return True
        End Function

        Public Function SavePanel(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0606
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return WriteToStreamsDatabase(StorageType, Location, ProviderType, ModelID)
                Case ECModelStorageType.mstAHPFile, ECModelStorageType.mstAHPDatabase, ECModelStorageType.mstCanvasDatabase 'C0783
                    'Debug.Print("Saving Antigua information to old format: " + GetStructureType().ToString + " " + StorageType.ToString)
                    Return WriteToOldFileFormat(Location, ProviderType)    '-D0884
                    'Return True     ' D0884
            End Select
            Return False
        End Function

        Private Function ParseStreamOld(ByVal BR As BinaryReader) As Boolean 'C0962
            'Stream.Seek(0, SeekOrigin.Begin)
            'Dim BR As New BinaryReader(Stream)

            BR.BaseStream.Seek(0, SeekOrigin.Begin)

            Dim res As Boolean = True

            While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And res
                Dim chunk = BR.ReadInt32
                If chunk = CHUNK_NODE Then
                    Dim node As New clsVisualNode

                    'node.ID = BR.ReadInt32 'C0638
                    BR.ReadInt32() 'C0638 reading dummy integer

                    node.GuidID = New Guid(BR.ReadBytes(16))

                    node.Author = BR.ReadString
                    node.LastModifiedBy = BR.ReadString
                    node.Text = BR.ReadString
                    'node.ParentNodeID = BR.ReadInt32 'C0638
                    BR.ReadInt32() 'C0638 reading dummy integer
                    node.Location = BR.ReadInt32

                    chunk = BR.ReadInt32
                    If chunk = CHUNK_NODE_ATTRIBUTES Then
                        node.Attributes.X = BR.ReadDouble
                        node.Attributes.Y = BR.ReadDouble
                        node.Attributes.Width = BR.ReadDouble
                        node.Attributes.Height = BR.ReadDouble
                        node.Attributes.BackGroundColor = BR.ReadInt32 'C0607
                        node.Attributes.ForeGroundColor = BR.ReadInt32 'C0607
                        node.Attributes.Font = BR.ReadString 'C0607

                        Nodes.Add(node) 'C0610
                    Else
                        res = False
                    End If
                Else
                    res = False
                End If
            End While

            'BR.Close()
            Return res
        End Function

        Private Function ParseStream(ByVal Stream As MemoryStream) As Boolean 'C0606
            Stream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(Stream)

            Dim res As Boolean = True

            Try
                While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And res
                    Dim chunk = BR.ReadInt32
                    If chunk = CHUNK_NODE Then
                        Dim node As New clsVisualNode

                        'node.ID = BR.ReadInt32 'C0638
                        BR.ReadInt32() 'C0638 reading dummy integer

                        node.GuidID = New Guid(BR.ReadBytes(16))


                        'A0396 ===
                        node.IsAlternative = BR.ReadBoolean
                        node.WasAlreadyMovedToHierarchy = BR.ReadBoolean 'A0399
                        node.IsProsConsPaneVisible = BR.ReadBoolean 'A0399

                        Dim count As Integer = BR.ReadInt32()
                        For i As Integer = 0 To count - 1
                            Dim GuidValue As Guid = New Guid(BR.ReadBytes(16))
                            Dim StringValue As String = BR.ReadString
                            If StringValue = PARENT_GUID_COMMENT Then
                                node.ParentGuidID = GuidValue
                            Else
                                node.Comments.Add(GuidValue, StringValue)
                            End If
                        Next

                        Dim ProsCount As Integer = BR.ReadInt32()
                        For i As Integer = 0 To ProsCount - 1
                            Dim Pro As clsVisualNode = New clsVisualNode

                            Pro.GuidID = New Guid(BR.ReadBytes(16))


                            Pro.IsAlternative = BR.ReadBoolean
                            Pro.WasAlreadyMovedToHierarchy = BR.ReadBoolean

                            Dim ProCommentsCount As Integer = BR.ReadInt32
                            For j As Integer = 0 To ProCommentsCount - 1
                                Dim Key As Guid = New Guid(BR.ReadBytes(16))
                                Dim Value As String = BR.ReadString
                                Pro.Comments.Add(Key, Value)
                            Next

                            Pro.Author = BR.ReadString
                            Pro.LastModifiedBy = BR.ReadString
                            Pro.Text = BR.ReadString

                            node.ProsList.Add(Pro)
                        Next


                        Dim ConsCount As Integer = BR.ReadInt32()
                        For i As Integer = 0 To ConsCount - 1
                            Dim Con As clsVisualNode = New clsVisualNode

                            Con.GuidID = New Guid(BR.ReadBytes(16))


                            Con.IsAlternative = BR.ReadBoolean
                            Con.WasAlreadyMovedToHierarchy = BR.ReadBoolean

                            Dim ConCommentsCount As Integer = BR.ReadInt32
                            For j As Integer = 0 To ConCommentsCount - 1
                                Dim Key As Guid = New Guid(BR.ReadBytes(16))
                                Dim Value As String = BR.ReadString
                                Con.Comments.Add(Key, Value)
                            Next

                            Con.Author = BR.ReadString
                            Con.LastModifiedBy = BR.ReadString
                            Con.Text = BR.ReadString

                            node.ConsList.Add(Con)
                        Next

                        'A0396 ==



                        node.Author = BR.ReadString
                        node.LastModifiedBy = BR.ReadString
                        node.Text = BR.ReadString
                        'node.ParentNodeID = BR.ReadInt32 'C0638
                        BR.ReadInt32() 'C0638 reading dummy integer
                        node.Location = BR.ReadInt32

                        chunk = BR.ReadInt32
                        If chunk = CHUNK_NODE_ATTRIBUTES Then
                            node.Attributes.X = BR.ReadDouble
                            node.Attributes.Y = BR.ReadDouble
                            node.Attributes.Width = BR.ReadDouble
                            node.Attributes.Height = BR.ReadDouble
                            node.Attributes.BackGroundColor = BR.ReadInt32 'C0607
                            node.Attributes.ForeGroundColor = BR.ReadInt32 'C0607
                            node.Attributes.Font = BR.ReadString 'C0607

                            Nodes.Add(node) 'C0610
                        Else
                            res = False
                        End If
                    Else
                        res = False
                    End If
                End While
            Catch ex As Exception 'C0962
                'BR.Close()
                res = ParseStreamOld(BR)
            Finally
                BR.Close() 'C0962
            End Try

            'BR.Close() 'C0962
            Return res
        End Function

        Private Function ReadFromStreamsDatabase(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0606
            Dim sType As StructureType = GetStructureType()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", sType))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim MS As New MemoryStream

                If dbReader.HasRows Then
                    dbReader.Read()

                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.

                    If bufferSize <> 0 Then 'C0612
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, retval)
                        bw.Flush()

                        'bw.Close()

                        dbReader.Close()
                        dbConnection.Close()

                        Return ParseStream(MS)
                    Else 'C0612
                        Return True 'C0612
                    End If
                Else
                    dbReader.Close()
                    dbConnection.Close()
                    Return False
                End If
            End Using

        End Function

        Private Function ReadFromOldFileFormat(ByVal Location As String, ByVal ProviderType As DBProviderType) As Boolean 'C0783
            If Not TableExists(Location, ProviderType, "AntiguaInfo") Then
                Return False
            End If

            Dim sType As StructureType = GetStructureType()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM AntiguaInfo WHERE StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", sType))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim MS As New MemoryStream

                If dbReader.HasRows Then
                    dbReader.Read()

                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.

                    If bufferSize <> 0 Then 'C0612
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, retval)
                        bw.Flush()

                        'bw.Close()

                        dbReader.Close()
                        dbConnection.Close()

                        Return ParseStream(MS)
                    Else 'C0612
                        Return True 'C0612
                    End If
                Else
                    dbReader.Close()
                    dbConnection.Close()
                    Return False
                End If
            End Using
        End Function

        Public Function LoadPanel(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean 'C0606
            mPanelLoaded = False    ' D4954
            Nodes.Clear()
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    mPanelLoaded = ReadFromStreamsDatabase(StorageType, Location, ProviderType, ModelID)    ' D4954
                Case ECModelStorageType.mstAHPFile, ECModelStorageType.mstAHPDatabase, ECModelStorageType.mstCanvasDatabase 'C0783
                    'Debug.Print("Loading Antigua information from old format: " + GetStructureType().ToString + " " + StorageType.ToString)
                    mPanelLoaded = ReadFromOldFileFormat(Location, ProviderType)    ' D4954
            End Select
            Return mPanelLoaded ' D4954
        End Function
    End Class

    <Serializable()> Public Class clsAntiguaInfoDocs 'C0829 + D1083
        Dim mPrjManager As clsProjectManager

        Public Property ProjectManager() As clsProjectManager
            Get
                Return mPrjManager
            End Get
            Set(ByVal value As clsProjectManager)
                mPrjManager = value
            End Set
        End Property

        Private Function CreateAntiguaInfoDocsStream() As MemoryStream 'C0829
            Dim MS As New MemoryStream

            Dim BW As New BinaryWriter(MS)

            Dim count As Integer = 0
            For Each vNode As clsVisualNode In ProjectManager.AntiguaDashboard.Nodes
                If vNode.HasInfoDoc Then
                    count += 1  ' D1083
                End If
            Next
            For Each vNode As clsVisualNode In ProjectManager.AntiguaRecycleBin.Nodes
                If vNode.HasInfoDoc Then
                    count += 1  ' D1083
                End If
            Next

            BW.Write(count)

            For Each vNode As clsVisualNode In ProjectManager.AntiguaDashboard.Nodes
                If vNode.HasInfoDoc Then
                    BW.Write(vNode.GuidID.ToByteArray)
                    BW.Write(vNode.InfoDoc)
                End If
            Next
            For Each vNode As clsVisualNode In ProjectManager.AntiguaRecycleBin.Nodes
                If vNode.HasInfoDoc Then
                    BW.Write(vNode.GuidID.ToByteArray)
                    BW.Write(vNode.InfoDoc)
                End If
            Next

            BW.Close()

            Return MS
        End Function

        Public Function SaveAntiguaInfoDocs() As Boolean 'C0829
            Dim MS As MemoryStream = CreateAntiguaInfoDocsStream()

            With ProjectManager.StorageManager
                Using dbConnection As DbConnection = GetDBConnection(.ProviderType, .ProjectLocation)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProjectManager.StorageManager.ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "ProjectID", .ModelID))
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "StructureType", StructureType.stAntiguaInfoDocs))

                    Dim affected As Integer = DBExecuteNonQuery(.ProviderType, oCommand)

                    If MS.ToArray.Length > 4 Then 'C0612    ' D4405 // was <>0, now >4 since there count (int) is always passed, but for 0 infodocs no more bytes
                        oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(.ProviderType, "ProjectID", .ModelID))
                        oCommand.Parameters.Add(GetDBParameter(.ProviderType, "StructureType", StructureType.stAntiguaInfoDocs))
                        oCommand.Parameters.Add(GetDBParameter(.ProviderType, "StreamSize", MS.ToArray.Length))
                        oCommand.Parameters.Add(GetDBParameter(.ProviderType, "Stream", MS.ToArray))
                        affected = DBExecuteNonQuery(.ProviderType, oCommand)
                    End If
                End Using
            End With

            Return True
        End Function

        Private Function ParseAntiguaInfoDocsStream(ByVal mStream As MemoryStream) As Boolean 'C0829
            mStream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(mStream)

            Dim Count As Int32 = BR.ReadInt32
            Dim j As Integer = 0
            While (mStream.Position < mStream.Length - 1) And (j <> Count)
                Dim NodeGuid As Guid = New Guid(BR.ReadBytes(16))
                Dim NodeInfoDoc As String = BR.ReadString

                Dim VisualNode As clsVisualNode = GetVisualNode(NodeGuid)
                If VisualNode IsNot Nothing Then
                    VisualNode.InfoDoc = NodeInfoDoc
                End If
                j += 1
            End While

            BR.Close()
            Return True
        End Function

        ' D4198 ===
        Private Function RestoreLostAntiguaInfoDocs() As Boolean
            Dim fRestored As Boolean = False
            For Each tInfodoc As clsInfoDoc In ProjectManager.InfoDocs.InfoDocs
                If Not tInfodoc.TargetID.Equals(Guid.Empty) Then
                    Dim VNode As clsVisualNode = GetVisualNode(tInfodoc.TargetID)
                    If VNode IsNot Nothing Then
                        VNode.InfoDoc = tInfodoc.InfoDoc
                        fRestored = True
                    End If
                End If
            Next
            If fRestored Then SaveAntiguaInfoDocs()
        End Function
        ' D4198 ==

        Public Function LoadAntiguaInfoDocs() As Boolean
            With ProjectManager.StorageManager
                If Not CheckDBConnection(.ProviderType, .ProjectLocation) Then
                    Return False
                End If

                Dim MS As New MemoryStream
                Using dbConnection As DbConnection = GetDBConnection(.ProviderType, .ProjectLocation)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(.ProviderType)
                    oCommand.Connection = dbConnection

                    oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"

                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "ProjectID", .ModelID))
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "StructureType", CInt(StructureType.stAntiguaInfoDocs)))

                    Dim dbReader As DbDataReader
                    dbReader = DBExecuteReader(.ProviderType, oCommand)

                    If dbReader.HasRows Then
                        dbReader.Read()

                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, retval)
                        bw.Flush()

                        'bw.Close()
                    End If

                    dbReader.Close()
                End Using

                If MS.Length <> 0 Then
                    Return ParseAntiguaInfoDocsStream(MS)
                Else
                    If ProjectManager.InfoDocs.InfoDocs.Count > 0 Then RestoreLostAntiguaInfoDocs() ' D4198
                    Return True
                End If
            End With
        End Function

        Private Function GetVisualNode(ByVal VisualNodeGuid As Guid) As clsVisualNode
            Dim VisualNode As clsVisualNode = Nothing

            VisualNode = ProjectManager.AntiguaDashboard.GetNodeByGuid(VisualNodeGuid)
            If VisualNode Is Nothing Then
                VisualNode = ProjectManager.AntiguaRecycleBin.GetNodeByGuid(VisualNodeGuid)
            End If

            Return VisualNode
        End Function

        Public Function GetAntiguaInfoDoc(ByVal VisualNodeGuid As Guid) As String
            Dim VisualNode As clsVisualNode = GetVisualNode(VisualNodeGuid)

            If VisualNode IsNot Nothing Then
                Return VisualNode.InfoDoc
            Else
                Return ""
            End If
        End Function

        Public Function SetAntiguaInfoDoc(ByVal VisualNodeGuid As Guid, ByVal InfoDoc As String) As Boolean
            Dim VisualNode As clsVisualNode

            VisualNode = ProjectManager.AntiguaDashboard.GetNodeByGuid(VisualNodeGuid)
            If VisualNode Is Nothing Then
                VisualNode = ProjectManager.AntiguaRecycleBin.GetNodeByGuid(VisualNodeGuid)
            End If

            If VisualNode IsNot Nothing Then
                VisualNode.InfoDoc = InfoDoc
                SaveAntiguaInfoDocs()
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub New(ByVal ProjectManager As clsProjectManager)
            mPrjManager = ProjectManager
        End Sub
    End Class

End Namespace