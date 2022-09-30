Namespace Macros

    Public Enum _OperatorType
        ttIdent
        ttIf
        ttThen
        ttElse
        ttCondition
        ttOpenBracket
        ttCloseBracket
        ttComma
        ttStrConst
        ttNumber
        ttIfNotF
        ttAddOp
        ttMulOp
        ttNot
        ttUnknown
        ttSpace
        ttBoolean
        ttFloat
        ttGoTo
    End Enum

    Public Class clsMacros

        Delegate Function _Func(ByVal Params As ArrayList) As Object

        Private Structure _Funcs
            Dim Addr As _Func
            Dim Name As String
            Dim ReturnType As _OperatorType
            Dim ParamTypes() As _OperatorType
        End Structure

        Public Structure _Exception
            Dim Text As String
            Dim CaretPos As Integer
            Dim ScriptError As Boolean
        End Structure

        Public _Ex As _Exception

        Dim errStack As String = "Stack is empty"
        Dim errSyntax As String = "Syntax error at caret position {0}"
        Dim errExecute As String = "Runtime error, cannot execute term at caret position {0}"

        Private Structure _Lexeme
            Dim Text As String
            Dim Type As _OperatorType
            Dim Length As Integer
            Dim Index As Integer
            Dim Position As Integer
        End Structure

        Private Text As String
        Private EOS As Integer = -1
        Private EOP As Integer = -1
        Private _ScriptError As Boolean = False

        Class clsOperatorList
            Inherits ArrayList
            Public Function GetItem(ByVal index As Integer) As Object
                If index >= Count Then
                    'ScriptError = True
                    '_Ex.Text = "List index out of bounds"
                    '_Ex.CaretPos = 0
                    Return Nothing
                Else
                    Return Item(index)
                End If
            End Function
        End Class

        Private OperatorList As New clsOperatorList
        Private PolizList As New clsOperatorList
        Private Funcs As New clsOperatorList

        Private Property ScriptError() As Boolean
            Get
                Return _ScriptError
            End Get
            Set(ByVal value As Boolean)
                _ScriptError = value
                _Ex.ScriptError = value
            End Set
        End Property

        ''' <summary> 
        ''' Parses code string and runs macros
        ''' </summary>
        ''' <param name="Text">Code string</param>
        ''' <returns>Errors</returns>
        ''' <remarks></remarks>
        Public Function Execute(ByVal Text As String) As Boolean
            Me.Text = Text
            _Ex.Text = "Successful"
            OperatorList.Clear()
            OperatorList = GetOperatorList()
            EOS = 0
            PolizList.Clear()
            Macros()
            If Not ScriptError Then Run()
            Return Not ScriptError
        End Function
        ''' <summary>
        ''' Check syntax of expression
        ''' </summary>
        ''' <param name="Text">Expression</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TestSyntax(ByVal Text As String) As Boolean
            Me.Text = Text
            _Ex.Text = "Successful"
            OperatorList.Clear()
            OperatorList = GetOperatorList()
            EOS = 0
            PolizList.Clear()
            Macros()
            Return Not ScriptError
        End Function

        ''' <summary>
        ''' Stack
        ''' </summary>
        ''' <remarks></remarks>
        Private Stack As New ArrayList
        Private Function Pop() As Object
            'A0001
            If Stack.Count > 0 Then
                Dim StackTop As Object = Stack.Item(Stack.Count - 1)
                Stack.RemoveAt(Stack.Count - 1)
                Return StackTop
            Else
                ScriptError = True
                _Ex.Text = "Stack error"
                _Ex.CaretPos = 0
                Return Nothing
            End If
        End Function
        Private Sub Push(ByVal Element As Object)
            Stack.Add(Element)
        End Sub
        '</stack>

        Public Sub DeclareFunction(ByVal Name As String, ByVal addr As _Func, ByVal ReturnType As _OperatorType, _
                            ByVal ParamArray Params() As _OperatorType)
            Dim f As _Funcs
            f.Name = Name.ToLower.Trim
            f.Addr = addr
            f.ReturnType = ReturnType
            ReDim f.ParamTypes(UBound(Params, 1))
            For i As Integer = 0 To UBound(Params, 1)
                f.ParamTypes(i) = Params(i)
            Next
            Funcs.Add(f)
        End Sub

        Private Function Declared(ByVal Name As String) As Integer
            Dim f As _Funcs
            For i As Integer = 0 To Funcs.Count - 1
                f = CType(Funcs.GetItem(i), _Funcs)
                If f.Name = Name Then
                    Return i
                    Exit Function
                End If
            Next i
            Return -1
        End Function

        Private ReadOnly Property GetOperatorList() As clsOperatorList
            Get
                Dim index As Integer = 0
                Dim Arr As New clsOperatorList
                Dim LList As _Lexeme
                While index < Text.Length
                    LList = ExtractLexeme(index)
                    Arr.Add(LList)
                End While
                Return Arr
            End Get
        End Property

        Private Function ExtractLexeme(ByRef Index As Integer) As _Lexeme
            Dim s As String = Text
            Dim L As _Lexeme
            While (Index < s.Length) And (Asc(s(Index)) <= Asc(" "))
                Index += 1
            End While
            L.Text = ""
            L.Type = _OperatorType.ttUnknown
            If Index < s.Length Then
                L.Index = Index
                L.Position = Index
                L.Text = s(Index)
                Select Case s(Index)
                    Case CChar(" ")
                        L.Type = _OperatorType.ttSpace
                    Case CChar("(")
                        L.Type = _OperatorType.ttOpenBracket
                    Case CChar(")")
                        L.Type = _OperatorType.ttCloseBracket
                    Case CChar("+"), CChar("-")
                        L.Type = _OperatorType.ttAddOp
                    Case CChar("*"), CChar("/")
                        L.Type = _OperatorType.ttMulOp
                    Case CChar(",")
                        L.Type = _OperatorType.ttComma
                    Case CChar(">"), CChar("<"), CChar("=")
                        L.Type = _OperatorType.ttCondition
                    Case CChar("""")
                        L.Type = _OperatorType.ttStrConst
                        Index += 1
                        Try
                            While (Index < s.Length) And (s(Index) <> """")
                                Index += 1
                            End While
                        Catch ex As Exception
                            ScriptError = True
                            _Ex.Text = String.Format(errSyntax, Index)
                            _Ex.CaretPos = Index
                        End Try
                        L.Text = s.Substring(L.Index + 1, Index - L.Index - 1)
                    Case CChar("a") To CChar("z"), CChar("A") To CChar("Z"), CChar("_")
                        L.Type = _OperatorType.ttIdent
                        Try
                            While (Index < s.Length) And _
                            ((((Asc(s(Index)) >= Asc("A")) And (Asc(s(Index)) <= Asc("z")))) Or _
                            (Asc(s(Index)) >= Asc("0")) And (Asc(s(Index)) <= Asc("9")))
                                Index += 1
                            End While
                        Catch ex As Exception
                            ScriptError = True
                            _Ex.Text = String.Format(errSyntax, Index)
                            _Ex.CaretPos = Index
                        End Try
                        Index -= 1
                        L.Text = s.Substring(L.Index, Index - L.Index + 1).ToLower
                        If L.Text = "if" Then L.Type = _OperatorType.ttIf
                        If L.Text = "then" Then L.Type = _OperatorType.ttThen
                        If L.Text = "else" Then L.Type = _OperatorType.ttElse
                        If L.Text = "true" Then L.Type = _OperatorType.ttBoolean
                        If L.Text = "false" Then L.Type = _OperatorType.ttBoolean
                        If L.Text = "or" Then L.Type = _OperatorType.ttMulOp
                        If L.Text = "and" Then L.Type = _OperatorType.ttMulOp
                    Case CChar("0") To CChar("9")
                        Index += 1
                        Try
                            While (Index < s.Length) And (Asc(s(Index)) >= Asc("0")) And (Asc(s(Index)) <= Asc("9"))
                                L.Text = L.Text + s(Index)
                                Index += 1
                            End While

                        Catch ex As Exception
                            ScriptError = True
                            _Ex.Text = String.Format(errSyntax, Index)
                            _Ex.CaretPos = Index
                        End Try

                        If (Index < s.Length) Then
                            If (s(Index) = ".") Then
                                L.Text = L.Text + "."
                                Index += 1
                                If (Index < s.Length) And (Asc(s(Index)) >= Asc("0")) And (Asc(s(Index)) <= Asc("9")) Then
                                    Try
                                        While (Index < s.Length) And (Asc(s(Index)) >= Asc("0")) And (Asc(s(Index)) <= Asc("9"))
                                            L.Text = L.Text + s(Index)
                                            Index += 1

                                        End While

                                    Catch ex As Exception
                                        ScriptError = True
                                        _Ex.Text = String.Format(errSyntax, Index)
                                        _Ex.CaretPos = Index
                                    End Try
                                End If
                            End If
                        End If
                        '    if not Error then
                        L.Type = _OperatorType.ttNumber
                        Index -= 1
                End Select
            End If
            Index += 1
            L.Text = L.Text.Trim
            L.Length = L.Text.Length
            Return L
        End Function
        ''' <summary>
        ''' Syntax grammar main rule
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub Macros()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            Select Case L.Type
                Case _OperatorType.ttIdent, _OperatorType.ttIf
                    Operators()
                Case Else
                    ScriptError = True
                    _Ex.CaretPos = L.Position
                    _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
            End Select
        End Sub
        Private Sub Operators()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttElse Then Exit Sub
            Simple_Operator()
            Operators()
        End Sub
        Private Sub Simple_Operator()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            Select Case L.Type
                Case _OperatorType.ttIf
                    IfStatement()
                    'EOS += 1
                Case _OperatorType.ttIdent
                    SubCall()
            End Select
        End Sub
        Private Sub FormLexeme(ByRef L As _Lexeme, ByVal Text As String, ByVal Type As _OperatorType)
            L.Text = Text
            L.Type = Type
        End Sub
        Private Sub IfStatement()
            Dim L, L1, L2 As _Lexeme
            Dim i, j As Integer
            EOS += 1
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            Expression()
            L1.Text = ""
            L2.Text = ""
            FormLexeme(L1, "IfNotF", _OperatorType.ttIfNotF)
            PolizList.Add(L1)
            i = PolizList.Count - 1
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type <> _OperatorType.ttThen Then
                ScriptError = True
                _Ex.CaretPos = L.Position
                _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
            Else
                EOS += 1
                Operators()
                FormLexeme(L2, "GoTo", _OperatorType.ttGoTo)
                j = PolizList.Count
                PolizList.Add(L2)
                L = CType(PolizList.GetItem(i), _Lexeme)
                L.Index = PolizList.Count - 1
                PolizList.RemoveAt(i)
                PolizList.Insert(i, L)

                if_1()

                L = CType(PolizList.GetItem(j), _Lexeme)
                L.Index = PolizList.Count
                PolizList.RemoveAt(j)
                PolizList.Insert(j, L)

            End If
        End Sub
        Private Sub if_1()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttElse Then
                EOS += 1
                L = CType(OperatorList.GetItem(EOS), _Lexeme)
                Operators()
            End If
        End Sub
        Private Sub SubCall()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If (L.Type <> _OperatorType.ttIdent) Then
                ScriptError = True
                _Ex.CaretPos = L.Position
                _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
            Else
                EOS += 1
                Parameters()
                PolizList.Add(L)
            End If
        End Sub
        Private Sub Expression_List()
            Expression()
            Next_Expression()
        End Sub
        Private Sub Next_Expression()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttComma Then
                EOS += 1
                L = CType(OperatorList.GetItem(EOS), _Lexeme)
                Expression()
                Next_Expression()
            End If
        End Sub
        Private Function Sign() As Integer
            Dim Result As Integer = 1
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then
                Return 1
                Exit Function
            End If
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttAddOp Then
                If (L.Text = "+") Or (L.Text = "-") Then
                    If L.Text = "-" Then Result = -1
                    If L.Text = "+" Then Result = 1
                    EOS += 1
                Else
                    ScriptError = True
                    _Ex.CaretPos = L.Position
                    _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
                End If
            End If
            Return Result
        End Function
        Private Sub Simple_Expression()
            Dim L1, L2 As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L1 = CType(OperatorList.GetItem(EOS), _Lexeme)
            L2.Text = ""
            If Sign() = -1 Then
                FormLexeme(L1, "0", _OperatorType.ttNumber)
                L1.Type = _OperatorType.ttNumber
                PolizList.Add(L1)
                Term()
                FormLexeme(L2, "-", _OperatorType.ttAddOp)
                PolizList.Add(L2)
            Else
                Term()
            End If
            Simple_Expression1()
        End Sub
        Private Sub Simple_Expression1()
            Dim L, l1 As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttAddOp Then
                EOS += 1
                l1 = CType(OperatorList.GetItem(EOS), _Lexeme)
                Term()
                PolizList.Add(L)
                Simple_Expression1()
            End If
        End Sub
        Private Sub Term()
            Factor()
            Term1()
        End Sub
        Private Sub Factor()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            Select Case L.Type
                Case _OperatorType.ttIdent
                    SubCall()
                Case _OperatorType.ttNumber, _OperatorType.ttStrConst, _OperatorType.ttBoolean
                    PolizList.Add(L)
                    EOS += 1
                Case _OperatorType.ttOpenBracket
                    EOS += 1
                    L = CType(OperatorList.GetItem(EOS), _Lexeme)
                    Expression()
                    L = CType(OperatorList.GetItem(EOS), _Lexeme)
                    If L.Type <> _OperatorType.ttCloseBracket Then
                        ScriptError = True
                        _Ex.CaretPos = L.Position
                        _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
                    Else
                        EOS += 1
                    End If
                Case _OperatorType.ttNot
                    EOS += 1
                    Factor()
                    PolizList.Add(L)
                Case Else
                    ScriptError = True
                    _Ex.CaretPos = L.Position
                    _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
            End Select
        End Sub
        Private Sub Term1()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttMulOp Then
                EOS += 1
                'Expression()
                ' A0001 ===
                Factor()
                Term1()
                ' A0001 ===
                PolizList.Add(L)
            End If
        End Sub
        Private Sub Parameters()
            Try
                Dim L As _Lexeme
                If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
                L = CType(OperatorList.GetItem(EOS), _Lexeme)
                If L.Type = _OperatorType.ttOpenBracket Then
                    PolizList.Add(L) ' adding brackets
                    EOS += 1
                    L = CType(OperatorList.GetItem(EOS), _Lexeme)
                    Expression_List()
                    L = CType(OperatorList.GetItem(EOS), _Lexeme)
                    If L.Type <> _OperatorType.ttCloseBracket Then
                        ScriptError = True
                        _Ex.CaretPos = L.Position
                        _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
                    Else
                        PolizList.Add(L)
                        EOS += 1
                    End If
                Else
                    EOS -= 1
                End If
            Finally
            End Try
        End Sub
        Private Sub Expression()
            Dim L As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If (L.Type = _OperatorType.ttAddOp) Or (L.Type = _OperatorType.ttIdent) Or (L.Type = _OperatorType.ttOpenBracket) _
             Or (L.Type = _OperatorType.ttBoolean) _
              Or (L.Type = _OperatorType.ttStrConst) Or (L.Type = _OperatorType.ttNot) _
               Or (L.Type = _OperatorType.ttNumber) Then
                Simple_Expression()
                Expression1()
            End If
        End Sub
        Private Sub Expression1()
            Dim L, L1 As _Lexeme
            If ScriptError Or (EOS >= OperatorList.Count) Then Exit Sub
            L = CType(OperatorList.GetItem(EOS), _Lexeme)
            If L.Type = _OperatorType.ttCondition Then
                EOS += 1
                L1 = CType(OperatorList.GetItem(EOS), _Lexeme)
                Expression()
                PolizList.Add(L)
            End If
        End Sub

        ''' <summary>
        ''' Translator main rule
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub Run()
            Stack.Clear()
            EOP = 0
            Dim P As _Lexeme
            If (PolizList.Count <= 0) Or (ScriptError) Then Exit Sub
            While (EOP < PolizList.Count) And (Not ScriptError)
                P = CType(PolizList.GetItem(EOP), _Lexeme)
                Select Case P.Type
                    Case _OperatorType.ttNumber, _OperatorType.ttStrConst, _OperatorType.ttBoolean, _
                _OperatorType.ttOpenBracket, _OperatorType.ttCloseBracket
                        Push(P)
                    Case _OperatorType.ttIdent
                        DoIdent()
                    Case _OperatorType.ttAddOp, _OperatorType.ttMulOp
                        DoAddMulOperation()
                    Case _OperatorType.ttNot
                        DoNot()
                    Case _OperatorType.ttCondition
                        DoCondition()
                    Case _OperatorType.ttIfNotF
                        DoIfNotF()
                    Case _OperatorType.ttGoTo
                        DoGoTo()
                End Select
                EOP += 1
            End While
        End Sub
        Private Function Compatible(ByVal Op1 As _Lexeme, ByVal Op2 As _Lexeme, ByRef T As _OperatorType) As Boolean
            T = Op1.Type
            Return (Op1.Type = Op2.Type) Or (Op1.Type = _OperatorType.ttBoolean)
        End Function
        Private Sub DoAddMulOperation()
            Dim P, Op1, Op2, Res As _Lexeme
            Dim T As _OperatorType
            P = CType(PolizList.GetItem(EOP), _Lexeme)
            Op2 = CType(Pop(), _Lexeme)
            Op1 = CType(Pop(), _Lexeme)
            If ScriptError Then Exit Sub
            Res = Op1
            If Compatible(Op1, Op2, T) Then
                Select Case T
                    Case _OperatorType.ttNumber, _OperatorType.ttFloat
                        Select Case P.Text(0)
                            Case CChar("+")
                                Res.Text = CStr(Val(Op1.Text) + Val(Op2.Text))
                            Case CChar("-")
                                Res.Text = CStr(Val(Op1.Text) - Val(Op2.Text))
                            Case CChar("*")
                                Res.Text = CStr(Val(Op1.Text) * Val(Op2.Text))
                            Case CChar("/")
                                Res.Text = CStr(Val(Op1.Text) / Val(Op2.Text))
                            Case Else
                        End Select
                    Case _OperatorType.ttBoolean
                        If P.Text = "mod" Then
                            Res.Text = CStr(CInt(Op1.Text) Mod CInt(Op2.Text))
                        Else
                            If P.Text = "div" Then
                                Res.Text = CStr(CInt(Op1.Text) \ CInt(Op2.Text))
                            Else
                                If P.Text = "and" Then
                                    Res.Text = CStr(CBool(Op1.Text) And CBool(Op2.Text))
                                Else
                                    If P.Text = "or" Then
                                        Res.Text = CStr(CBool(Op1.Text) Or CBool(Op2.Text))
                                    Else
                                        ScriptError = True
                                        _Ex.CaretPos = P.Position
                                        _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
                                    End If
                                End If
                            End If
                        End If
                    Case _OperatorType.ttStrConst
                        If P.Text = "+" Then
                            Res.Text = Op1.Text + Op2.Text
                        Else
                            ScriptError = True
                            _Ex.CaretPos = P.Position
                            _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)
                        End If
                End Select

            Else
                ScriptError = True
                _Ex.CaretPos = P.Position
                _Ex.Text = String.Format(errSyntax, _Ex.CaretPos)

            End If

            Res.Type = T
            Push(Res)
        End Sub
        Private Sub DoIdent()
            Dim T, P, R As _Lexeme
            Dim op As _Func
            Dim f As _Funcs
            Dim s As String
            Dim i As Integer
            T = CType(PolizList.GetItem(EOP), _Lexeme)
            i = Declared(T.Text)
            If i > -1 Then
                P = CType(Pop(), _Lexeme)
                If ScriptError Then Exit Sub
                f = CType(Funcs(i), _Funcs)
                op = f.Addr
                If P.Type = _OperatorType.ttCloseBracket Then
                    Dim pr As New ArrayList
                    While P.Type <> _OperatorType.ttOpenBracket
                        P = CType(Pop(), _Lexeme)
                        If ScriptError Then Exit Sub
                        If P.Type <> _OperatorType.ttOpenBracket Then pr.Insert(0, P.Text)
                    End While
                    s = CStr(op.Invoke(pr))
                    If f.ReturnType <> _OperatorType.ttUnknown Then
                        R.Text = s
                        R.Type = f.ReturnType
                        Push(R)
                    End If
                Else
                    If UBound(f.ParamTypes, 1) >= 0 Then
                        s = CStr(op.Invoke(Nothing))
                        If f.ReturnType <> _OperatorType.ttUnknown Then
                            R.Text = s
                            R.Type = f.ReturnType
                            Push(R)
                        End If
                    Else
                        ScriptError = True
                        _Ex.CaretPos = P.Position
                        _Ex.Text = String.Format(errExecute, _Ex.CaretPos)
                    End If
                End If
            End If
        End Sub
        Private Sub DoNot()
            Dim Op As _Lexeme
            Op = CType(Pop(), _Lexeme)
            If ScriptError Then Exit Sub
            If (Op.Type = _OperatorType.ttBoolean) Then
                Op.Text = CStr(Not CBool(Op.Text))
            Else
                ScriptError = True
                _Ex.CaretPos = Op.Position
                _Ex.Text = String.Format(errExecute, _Ex.CaretPos)
            End If
            Push(Op)
        End Sub
        Private Sub DoCondition()
            Dim LeftSide, RightSide, L, Res As _Lexeme
            Dim ResultType As _OperatorType
            If ScriptError Then Exit Sub
            RightSide = CType(Pop(), _Lexeme)
            LeftSide = CType(Pop(), _Lexeme)
            If ScriptError Then Exit Sub
            Res.Text = ""
            L = CType(PolizList.GetItem(EOP), _Lexeme)
            If Compatible(LeftSide, RightSide, ResultType) Then
                Res.Type = _OperatorType.ttBoolean
                Select Case L.Text
                    Case ">"
                        Res.Text = CStr(Val(LeftSide.Text) > Val(RightSide.Text))
                    Case "<"
                        Res.Text = CStr(Val(LeftSide.Text) < Val(RightSide.Text))
                    Case "="
                        Res.Text = CStr(Val(LeftSide.Text) = Val(RightSide.Text))
                    Case ">="
                        Res.Text = CStr(Val(LeftSide.Text) >= Val(RightSide.Text))
                    Case "<="
                        Res.Text = CStr(Val(LeftSide.Text) <= Val(RightSide.Text))
                    Case "<>"
                        Res.Text = CStr(Val(LeftSide.Text) <> Val(RightSide.Text))
                    Case Else
                        ScriptError = True
                        _Ex.CaretPos = L.Position
                        _Ex.Text = String.Format(errExecute, _Ex.CaretPos)

                End Select
                Push(Res)
            End If
        End Sub
        Private Sub DoIfNotF()
            Dim P, L As _Lexeme
            L = CType(PolizList.GetItem(EOP), _Lexeme)
            P = CType(Pop(), _Lexeme)
            If ScriptError Then Exit Sub
            If (P.Type = _OperatorType.ttBoolean) And (CBool(P.Text) = False) Then
                EOP = L.Index
            End If
        End Sub
        Private Sub DoGoTo()
            Dim P As _Lexeme
            P = CType(PolizList.GetItem(EOP), _Lexeme)
            EOP = P.Index - 1
        End Sub

    End Class
End Namespace