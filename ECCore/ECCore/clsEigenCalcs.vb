Namespace ECCore.MathFuncs

    ''' <summary>
    ''' This class was designed to separate matrix' eigen vector calculations.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class clsEigenCalcs

        Private mMatrix(,) As Double
        Private mEMatrix(,) As Double

        Private mMatrixSize As Integer

        Private mICIndex As Double = 0.0
        Private mICRatio As Double = 0.0
        Private mLambdaMax As Double = 0.0
        Private mEps As Double = 0.00001

        Private mMissingJudgmentsCount As Integer = 0
        Private mMainEvaluated As Boolean

        Private mEigenVector() As Double

        ''' <summary>
        ''' Sets matrix and it's size.
        ''' </summary>
        ''' <param name="Matrix">Matrix for eigen vector calculations</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <remarks></remarks>
        Public Sub SetMatrix(ByVal Matrix As Double(,), ByVal MatrixSize As Integer)
            mMatrix = Matrix
            mMatrixSize = MatrixSize

            InitEMatrix()
            InitEigenVector()

            mICIndex = 0.0
            mICRatio = 0.0
            mLambdaMax = 0.0

            mMissingJudgmentsCount = GetMissingJudgmentsCount()
            mMainEvaluated = MainEvaluated()
            FillMissingJudgments()
        End Sub

        ''' <summary>
        ''' Matrix for eigen vector calculations
        ''' </summary>
        ''' <value></value>
        ''' <returns>Matrix for eigen vector calculations</returns>
        ''' <remarks>Setting the matrix is available via SetMatrix function.</remarks>
        Public ReadOnly Property Matrix() As Double(,)
            Get
                Return mMatrix
            End Get
        End Property

        ''' <summary>
        ''' Size of the matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>Size of the matrix</returns>
        ''' <remarks>Setting the matrix and it's size is available via SetMatrix function.</remarks>
        Public ReadOnly Property MatrixSize() As Integer
            Get
                Return mMatrixSize
            End Get
        End Property

        ''' <summary>
        ''' Inconsistency index of pairwise matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>Inconsistency index of pairwise matrix</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property InconIndex() As Double
            Get
                Return mICIndex
            End Get
        End Property

        ''' <summary>
        ''' Inconsistency ratio of pairwise matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>Inconsistency ratio of pairwise matrix</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property InconRatio() As Double
            Get
                Return GetICRatio()
            End Get
        End Property

        ''' <summary>
        ''' LamdbaMax of pairwise matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>LamdbaMax of pairwise matrix</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LambdaMax() As Double
            Get
                Return mLambdaMax
            End Get
        End Property

        ''' <summary>
        ''' Accuracy used in calculations
        ''' </summary>
        ''' <value></value>
        ''' <returns>Accuracy used in calculations</returns>
        ''' <remarks>Default value is 0.00001</remarks>
        Public Property Accuracy() As Double
            Get
                Return mEps
            End Get
            Set(ByVal value As Double)
                mEps = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the number of missing judgments in pairwise matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>Returns the number of missing judgments in pairwise matrix</returns>
        ''' <remarks>Missing judgments count is obtained by calculating the number of zeros in the pairwise matrix above TopLeft-BottomRight diagonal of a matrix.</remarks>
        Public ReadOnly Property MissingJudgmentsCount() As Integer
            Get
                Return mMissingJudgmentsCount
            End Get
        End Property

        ''' <summary>
        ''' Initializes identity matrix used in calculations
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub InitEMatrix()
            ReDim mEMatrix(mMatrixSize - 1, mMatrixSize - 1)

            For i As Integer = 0 To mMatrixSize - 1
                For j As Integer = 0 To mMatrixSize - 1
                    mEMatrix(i, j) = If(i = j, 1, 0)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Initializes result eigen vector with zeros
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub InitEigenVector()
            ReDim mEigenVector(mMatrixSize - 1)

            For i As Integer = 0 To mMatrixSize - 1
                mEigenVector(i) = 0
            Next
        End Sub

        ''' <summary>
        ''' Returns resulting eigen vector of a matrix
        ''' </summary>
        ''' <value></value>
        ''' <returns>Eigen vector of a matrix</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property EigenVector() As Double()
            Get
                Return mEigenVector
            End Get
        End Property

        ''' <summary>
        ''' Specifies whether main diagonal of the pairwise matrix has been evaluated.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Returns True if main diagonal has been evaluated, otherwise returns False</returns>
        ''' <remarks>Main diagonal is a diagonal right above TopLeft-BottomRight diagonal of the matrix.</remarks>
        Public ReadOnly Property MainDiagonalEvaluated() As Boolean
            Get
                Return mMainEvaluated
            End Get
        End Property

        ''' <summary>
        ''' Returns missing judgments count
        ''' </summary>
        ''' <returns>Returns missing judgments count</returns>
        ''' <remarks>See MissingJudgmentsCount property.</remarks>
        Private Function GetMissingJudgmentsCount() As Integer
            Dim count As Integer = 0
            For i As Integer = 0 To mMatrixSize - 2
                For j As Integer = i + 1 To mMatrixSize - 1
                    If mMatrix(i, j) = 0 Then
                        count += 1
                    End If
                Next
            Next
            Return count
        End Function

        ''' <summary>
        ''' Returns True is main diagonal is evaluated in pairwise matrix, otherwise returns False
        ''' </summary>
        ''' <returns>Returns True is main diagonal is evaluated in pairwise matrix, otherwise returns False</returns>
        ''' <remarks>See MainDiagonalEvaluated property.</remarks>
        Private Function MainEvaluated() As Boolean
            If mMatrixSize = 0 Then
                Return False
            End If

            Dim evaluated As Boolean = True
            For i As Integer = 0 To mMatrixSize - 2
                If mMatrix(i, i + 1) = 0 Then
                    evaluated = False
                End If
            Next

            Return evaluated
        End Function

        ''' <summary>
        ''' Modifies pairwise matrix with missing judgments using Harker's algorithm.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub FillMissingJudgments()
            ' using Harker's algorithm

            For i As Integer = 0 To mMatrixSize - 1
                For j As Integer = i To mMatrixSize - 1
                    If mMatrix(i, j) = 0 Then
                        mMatrix(i, i) += 1
                        mMatrix(j, j) += 1
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' Calculates eigen vector of a matrix using formula with limit
        ''' </summary>
        ''' <remarks>Not used right now as a primary calculation algorithm.</remarks>
        Protected Overridable Sub CalculateEigenVector1()
            ' using formula with limit
            Dim n As Integer = mMatrixSize

            Dim err As Double
            Dim c As Integer
            err = 1
            c = 1

            Dim B(,) As Double
            Dim B1(,) As Double

            Dim RS() As Double
            Dim CS() As Double

            Dim Res1() As Double
            Dim Res2() As Double

            Dim tmp() As Double

            Dim D As Double

            ReDim Res1(n - 1)
            ReDim Res2(n - 1)
            ReDim RS(n - 1)
            ReDim CS(n - 1)
            ReDim tmp(n - 1)
            ReDim B(n - 1, n - 1)
            ReDim B1(n - 1, n - 1)

            Dim i As Integer
            For i = 0 To n - 1
                Res1(i) = 0
                'Res1(i) = mMatrix(0, i)
            Next

            While (err > mEps)
                CopySquareMatrix(mEMatrix, B, n)
                For i = 1 To c
                    MultSquareMatrix(mMatrix, B, B1, n)
                    CopySquareMatrix(B1, B, n)
                Next
                RowSumInSquareMatrix(B, n, RS)
                ColSumInSquareMatrix(B, n, CS)
                D = VectorSum(CS, n)
                MultVectorByScalar(RS, 1 / D, Res2, n)
                VectorSubAbs(Res1, Res2, tmp, n)
                err = VectorSum(tmp, n)
                CopyVector(Res2, Res1, n)

                Dim s As String = ""
                For j As Integer = 0 To n - 1
                    s += Res1(j).ToString
                Next
                'Debug.Print("Res1: " + s)
                s = ""
                For j As Integer = 0 To n - 1
                    s += Res2(j).ToString
                Next
                'Debug.Print("Res2: " + s)

                c = c + 1
            End While

            ColSumInSquareMatrix(mMatrix, n, CS)

            For i = 0 To n - 1
                mEigenVector(i) = Res2(i)
            Next

            mLambdaMax = VectorScalarMult(CS, Res2, n)
            mICIndex = (mLambdaMax - n) / (n - 1)
        End Sub

        ''' <summary>
        ''' Calculates eigen vector of a matrix using EC11 algorithm
        ''' </summary>
        ''' <remarks>This is a current calculation algorithm. In derived classes you can override this function is changes in calculations are needed.</remarks>
        Protected Overridable Sub CalculateEigenVector2()
            ' using EC11 algorithm
            Dim n As Integer = mMatrixSize

            Dim done As Boolean = False
            Dim b As Boolean
            Dim i As Integer

            Dim savedM(,) As Double
            ReDim savedM(n - 1, n - 1)
            CopySquareMatrix(mMatrix, savedM, n)

            Dim Res1() As Double
            Dim Res2() As Double
            ReDim Res1(n - 1)
            ReDim Res2(n - 1)

            Dim maxIterations As Integer = 200

            If n <= 10 Then
                mEps = 0.000000001
            ElseIf n <= 15 Then
                mEps = 0.00000001
            ElseIf n <= 20 Then
                mEps = 0.0000001
            ElseIf n <= 30 Then
                mEps = 0.000001
            Else
                mEps = 0.00001
            End If

            For i = 0 To n - 1
                Res1(i) = mMatrix(i, 0) ' initialize with the first column of the matrix
            Next

            ''Debug.Print("")
            Dim iterCount As Integer = 0
            While Not done And iterCount <= maxIterations
                iterCount += 1
                'Dim str As String = ""
                'For j As Integer = 0 To n - 1
                '    str += Res1(j).ToString
                'Next
                ''Debug.Print("Res1: " + str)
                'str = ""
                'For j As Integer = 0 To n - 1
                '    str += Res2(j).ToString
                'Next
                ''Debug.Print("Res2: " + str)


                MultSquareMatrixByVector(mMatrix, Res1, Res2, n)
                NormalizeVector(Res2, n)

                b = True
                i = 0
                While b And (i < n)
                    b = Math.Abs(Res1(i) - Res2(i)) <= mEps
                    i += 1
                End While

                If Not b Then
                    CopyVector(Res2, Res1, n)
                Else
                    done = True
                End If
            End While

            For i = 0 To n - 1
                mEigenVector(i) = Res2(i)
            Next

            ' calculate inconsistency index
            Dim CI As Double = CalculateICIndex(savedM, n)
            Dim s As Double = 0
            For k As Integer = 0 To n - 1
                s += savedM(0, k) * Res2(k)
            Next
            If Res2(0) <> 0 Then
                mLambdaMax = s / Res2(0)
            End If


            '--
            'Dim CS() As Double
            'ReDim CS(n - 1)
            'ColSumInSquareMatrix(mMatrix, n, CS)
            'mLambdaMax = VectorScalarMult(CS, Res2, n)
            '--

            If CI <> 0 Then
                'C0002===
                'If mMissingJudgmentsCount = 0 Then
                'mICIndex = (mLambdaMax - n) / (n - 1)
                'Else
                'mICIndex = ((mLambdaMax - n) / (n - 1)) / CI
                'End If
                'C0002==
                mICIndex = ((mLambdaMax - n) / (n - 1)) / CI 'C0002 'C0003
                If mICIndex < 0 Then mICIndex = 0 'C1030 (sometimes mLambdaMax is little less than n, a round off error)
            End If
        End Sub

        Protected Function CalcLeftEigen(M As Double(,)) As Double()
            Dim n As Integer = mMatrixSize

            Dim v As Double()
            ReDim v(n - 1)

            Dim wOld As Double()
            ReDim wOld(n - 1)

            For i As Integer = 0 To n - 1
                v(i) = M(i, 0)
                wOld(i) = v(i)
            Next

            Dim Converge As Boolean = False

            While Not Converge
                MultVectorByMatrix(wOld, M, v, n, n)
                NormalizeVector(v, n)
                'check convergence
                Converge = True
                For i As Integer = 0 To n - 1
                    If Math.Abs(v(i) - wOld(i)) > mEps Then
                        Converge = False
                        Exit For
                    End If
                Next
                If Not Converge Then
                    For i As Integer = 0 To n - 1
                        wOld(i) = v(i)
                    Next
                End If
            End While

            'Debug.Print("Left Eigen Vector: ")
            For i As Integer = 0 To n - 1
                'Debug.Print(v(i).ToString)
            Next

            Dim multV As Double()
            ReDim multV(n - 1)

            MultVectorByMatrix(v, M, multV, n, n)

            'Debug.Print("Left Eigen Vector multilied by Matrix: ")
            For i As Integer = 0 To n - 1
                'Debug.Print(multV(i).ToString)
            Next

            'Debug.Print("Left Eigen Vector multilied by Matrix and then divided by LambdaMax (this should be equal to Left Eigen Vector - checking if left eigen is correct): ")
            For i As Integer = 0 To n - 1
                If LambdaMax <> 0 Then
                    'Debug.Print((multV(i) / LambdaMax).ToString)
                Else
                    'Debug.Print("0")
                End If
            Next

            Return v
        End Function

        ''' <summary>
        ''' Calculates inconsistency index of given matrix
        ''' </summary>
        ''' <param name="M">Matrix</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <returns>Inconsistency index of matrix M (MatrixSize x MatrixSize)</returns>
        ''' <remarks></remarks>
        Protected Overridable Function CalculateICIndex(ByVal M(,) As Double, ByVal MatrixSize As Integer) As Double
            Dim n As Integer = MatrixSize

            If n > 15 Then
                Return -1
            End If

            If n <= 2 Then
                Return 0
            End If

            Dim missing As Integer = mMissingJudgmentsCount

            If missing > n * (n - 1) / 2 - n Then
                Return 0
            End If

            Dim x As Integer
            Dim index As Integer = 0

            For i As Integer = 3 To n
                x = i * (i - 1) / 2 - i
                For j As Integer = 0 To x
                    index += 1
                    If i = n And j = missing Then
                        If index <= 455 Then
                            Return GetRInc(index)
                        Else
                            Return -1
                        End If
                    End If
                Next
            Next
        End Function

        ''' <summary>
        ''' Calculates inconsistency ratio of a matrix
        ''' </summary>
        ''' <returns>Inconsistency ratio of a matrix</returns>
        ''' <remarks>Matrix should be set using SetMatrix function</remarks>
        Private Function GetICRatio()
            Dim n As Integer = mMatrixSize

            If n <= 2 Then
                Return 0
            End If

            If n > 15 Then
                Return -1
            Else
                Return mICIndex / GetRandomICIndex(n)
            End If
        End Function

        ''' <summary>
        ''' Returns RInc value used in inconsistency index calculations with missing judgments
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns>Returns RInc value used in inconsistency index calculations with missing judgments</returns>
        ''' <remarks></remarks>
        Protected Overridable Function GetRInc(ByVal index As Integer) As Double
            If index > 455 Then
                Return -1
            End If

            Dim RInc() As Double = New Double(455) {}

            RInc(0) = 0.0
            RInc(1) = 0.524798
            RInc(2) = 0.885525
            RInc(3) = 0.586181
            RInc(4) = 0.305721
            RInc(5) = 1.109603
            RInc(6) = 0.929794
            RInc(7) = 0.753945
            RInc(8) = 0.578654
            RInc(9) = 0.40088
            RInc(10) = 0.209687
            RInc(11) = 1.248525
            RInc(12) = 1.130791
            RInc(13) = 1.011786
            RInc(14) = 0.899072
            RInc(15) = 0.784339
            RInc(16) = 0.673577
            RInc(17) = 0.551588
            RInc(18) = 0.428321
            RInc(19) = 0.298776
            RInc(20) = 0.159703
            RInc(21) = 1.340638
            RInc(22) = 1.258079
            RInc(23) = 1.177866
            RInc(24) = 1.095937
            RInc(25) = 1.010882
            RInc(26) = 0.931079
            RInc(27) = 0.846418
            RInc(28) = 0.768697
            RInc(29) = 0.68728
            RInc(30) = 0.608921
            RInc(31) = 0.520272
            RInc(32) = 0.42979
            RInc(33) = 0.334024
            RInc(34) = 0.2328
            RInc(35) = 0.12295
            RInc(36) = 1.404015
            RInc(37) = 1.345213
            RInc(38) = 1.281236
            RInc(39) = 1.220447
            RInc(40) = 1.159021
            RInc(41) = 1.099818
            RInc(42) = 1.038897
            RInc(43) = 0.981949
            RInc(44) = 0.921855
            RInc(45) = 0.861171
            RInc(46) = 0.800061
            RInc(47) = 0.739759
            RInc(48) = 0.679541
            RInc(49) = 0.61444
            RInc(50) = 0.550374
            RInc(51) = 0.482955
            RInc(52) = 0.418362
            RInc(53) = 0.346263
            RInc(54) = 0.271889
            RInc(55) = 0.187941
            RInc(56) = 0.096368
            RInc(57) = 1.453623
            RInc(58) = 1.403196
            RInc(59) = 1.35289
            RInc(60) = 1.308871
            RInc(61) = 1.260744
            RInc(62) = 1.213565
            RInc(63) = 1.167524
            RInc(64) = 1.12167
            RInc(65) = 1.076941
            RInc(66) = 1.032166
            RInc(67) = 0.984555
            RInc(68) = 0.936397
            RInc(69) = 0.892079
            RInc(70) = 0.843351
            RInc(71) = 0.796615
            RInc(72) = 0.754357
            RInc(73) = 0.702631
            RInc(74) = 0.655433
            RInc(75) = 0.604886
            RInc(76) = 0.559354
            RInc(77) = 0.504604
            RInc(78) = 0.455802
            RInc(79) = 0.3982
            RInc(80) = 0.345085
            RInc(81) = 0.28465
            RInc(82) = 0.221381
            RInc(83) = 0.153532
            RInc(84) = 0.077026
            RInc(85) = 1.486882
            RInc(86) = 1.449657
            RInc(87) = 1.409195
            RInc(88) = 1.375561
            RInc(89) = 1.336401
            RInc(90) = 1.29584
            RInc(91) = 1.261634
            RInc(92) = 1.226417
            RInc(93) = 1.186202
            RInc(94) = 1.150671
            RInc(95) = 1.114156
            RInc(96) = 1.074076
            RInc(97) = 1.038174
            RInc(98) = 1.001585
            RInc(99) = 0.966195
            RInc(100) = 0.926667
            RInc(101) = 0.889592
            RInc(102) = 0.857121
            RInc(103) = 0.817509
            RInc(104) = 0.783038
            RInc(105) = 0.739745
            RInc(106) = 0.703546
            RInc(107) = 0.66985
            RInc(108) = 0.628844
            RInc(109) = 0.589912
            RInc(110) = 0.549922
            RInc(111) = 0.51011
            RInc(112) = 0.470014
            RInc(113) = 0.427166
            RInc(114) = 0.38185
            RInc(115) = 0.340635
            RInc(116) = 0.290525
            RInc(117) = 0.239734
            RInc(118) = 0.18558
            RInc(119) = 0.12956
            RInc(120) = 0.069135
            RInc(121) = 1.512402
            RInc(122) = 1.481821
            RInc(123) = 1.451974
            RInc(124) = 1.419872
            RInc(125) = 1.392137
            RInc(126) = 1.363117
            RInc(127) = 1.332152
            RInc(128) = 1.29789
            RInc(129) = 1.265526
            RInc(130) = 1.240872
            RInc(131) = 1.206383
            RInc(132) = 1.176049
            RInc(133) = 1.14806
            RInc(134) = 1.114958
            RInc(135) = 1.087022
            RInc(136) = 1.056561
            RInc(137) = 1.024961
            RInc(138) = 0.993591
            RInc(139) = 0.967907
            RInc(140) = 0.936322
            RInc(141) = 0.905087
            RInc(142) = 0.875095
            RInc(143) = 0.843812
            RInc(144) = 0.814955
            RInc(145) = 0.78556
            RInc(146) = 0.754244
            RInc(147) = 0.723207
            RInc(148) = 0.69379
            RInc(149) = 0.666884
            RInc(150) = 0.631451
            RInc(151) = 0.602084
            RInc(152) = 0.569533
            RInc(153) = 0.537568
            RInc(154) = 0.501964
            RInc(155) = 0.466683
            RInc(156) = 0.435521
            RInc(157) = 0.400519
            RInc(158) = 0.367412
            RInc(159) = 0.330129
            RInc(160) = 0.292925
            RInc(161) = 0.247988
            RInc(162) = 0.204172
            RInc(163) = 0.161966
            RInc(164) = 0.110765
            RInc(165) = 0.057692
            RInc(166) = 1.537052
            RInc(167) = 1.512892
            RInc(168) = 1.483826
            RInc(169) = 1.458745
            RInc(170) = 1.432271
            RInc(171) = 1.413141
            RInc(172) = 1.38313
            RInc(173) = 1.359214
            RInc(174) = 1.331448
            RInc(175) = 1.309026
            RInc(176) = 1.281296
            RInc(177) = 1.256626
            RInc(178) = 1.228864
            RInc(179) = 1.205165
            RInc(180) = 1.182959
            RInc(181) = 1.15519
            RInc(182) = 1.133998
            RInc(183) = 1.10431
            RInc(184) = 1.08141
            RInc(185) = 1.054026
            RInc(186) = 1.03009
            RInc(187) = 1.006228
            RInc(188) = 0.978166
            RInc(189) = 0.956734
            RInc(190) = 0.929632
            RInc(191) = 0.90175400000000006
            RInc(192) = 0.880044
            RInc(193) = 0.853905
            RInc(194) = 0.827031
            RInc(195) = 0.805755
            RInc(196) = 0.778149
            RInc(197) = 0.753798
            RInc(198) = 0.728618
            RInc(199) = 0.701592
            RInc(200) = 0.675916
            RInc(201) = 0.651245
            RInc(202) = 0.627767
            RInc(203) = 0.602359
            RInc(204) = 0.576021
            RInc(205) = 0.546931
            RInc(206) = 0.521348
            RInc(207) = 0.494248
            RInc(208) = 0.465545
            RInc(209) = 0.439398
            RInc(210) = 0.40627
            RInc(211) = 0.379972
            RInc(212) = 0.349436
            RInc(213) = 0.316035
            RInc(214) = 0.282566
            RInc(215) = 0.249352
            RInc(216) = 0.215677
            RInc(217) = 0.177464
            RInc(218) = 0.139448
            RInc(219) = 0.095431
            RInc(220) = 0.050304
            RInc(221) = 1.554726
            RInc(222) = 1.532934
            RInc(223) = 1.511617
            RInc(224) = 1.489902
            RInc(225) = 1.466052
            RInc(226) = 1.44746
            RInc(227) = 1.424823
            RInc(228) = 1.403936
            RInc(229) = 1.382132
            RInc(230) = 1.364042
            RInc(231) = 1.339561
            RInc(232) = 1.31646
            RInc(233) = 1.294614
            RInc(234) = 1.272292
            RInc(235) = 1.251347
            RInc(236) = 1.232298
            RInc(237) = 1.209625
            RInc(238) = 1.192101
            RInc(239) = 1.168278
            RInc(240) = 1.146896
            RInc(241) = 1.124608
            RInc(242) = 1.104945
            RInc(243) = 1.083615
            RInc(244) = 1.060432
            RInc(245) = 1.040464
            RInc(246) = 1.017079
            RInc(247) = 0.998363
            RInc(248) = 0.978008
            RInc(249) = 0.955817
            RInc(250) = 0.935564
            RInc(251) = 0.912539
            RInc(252) = 0.891952
            RInc(253) = 0.869414
            RInc(254) = 0.851877
            RInc(255) = 0.831027
            RInc(256) = 0.80806
            RInc(257) = 0.787033
            RInc(258) = 0.765925
            RInc(259) = 0.743406
            RInc(260) = 0.723538
            RInc(261) = 0.704822
            RInc(262) = 0.6798
            RInc(263) = 0.658331
            RInc(264) = 0.636647
            RInc(265) = 0.614979
            RInc(266) = 0.593498
            RInc(267) = 0.571807
            RInc(268) = 0.543517
            RInc(269) = 0.525475
            RInc(270) = 0.501369
            RInc(271) = 0.479104
            RInc(272) = 0.456399
            RInc(273) = 0.431042
            RInc(274) = 0.406208
            RInc(275) = 0.382756
            RInc(276) = 0.358609
            RInc(277) = 0.334359
            RInc(278) = 0.303843
            RInc(279) = 0.27677
            RInc(280) = 0.253414
            RInc(281) = 0.217294
            RInc(282) = 0.190777
            RInc(283) = 0.156304
            RInc(284) = 0.119964
            RInc(285) = 0.082939
            RInc(286) = 0.041249
            RInc(287) = 1.571859
            RInc(288) = 1.550894
            RInc(289) = 1.533429
            RInc(290) = 1.51492
            RInc(291) = 1.497511
            RInc(292) = 1.480128
            RInc(293) = 1.460021
            RInc(294) = 1.437396
            RInc(295) = 1.422862
            RInc(296) = 1.401864
            RInc(297) = 1.383911
            RInc(298) = 1.368073
            RInc(299) = 1.347574
            RInc(300) = 1.329143
            RInc(301) = 1.311077
            RInc(302) = 1.293179
            RInc(303) = 1.280042
            RInc(304) = 1.256873
            RInc(305) = 1.237197
            RInc(306) = 1.220051
            RInc(307) = 1.201922
            RInc(308) = 1.181827
            RInc(309) = 1.164307
            RInc(310) = 1.146626
            RInc(311) = 1.130001
            RInc(312) = 1.109489
            RInc(313) = 1.094341
            RInc(314) = 1.072629
            RInc(315) = 1.056834
            RInc(316) = 1.038282
            RInc(317) = 1.01695
            RInc(318) = 1.004433
            RInc(319) = 0.984386
            RInc(320) = 0.968009
            RInc(321) = 0.946693
            RInc(322) = 0.92975
            RInc(323) = 0.912898
            RInc(324) = 0.89559
            RInc(325) = 0.875695
            RInc(326) = 0.856205
            RInc(327) = 0.837566
            RInc(328) = 0.821854
            RInc(329) = 0.802669
            RInc(330) = 0.787586
            RInc(331) = 0.763692
            RInc(332) = 0.748355
            RInc(333) = 0.726521
            RInc(334) = 0.711184
            RInc(335) = 0.691417
            RInc(336) = 0.676178
            RInc(337) = 0.655001
            RInc(338) = 0.636501
            RInc(339) = 0.619815
            RInc(340) = 0.599656
            RInc(341) = 0.582709
            RInc(342) = 0.566613
            RInc(343) = 0.542722
            RInc(344) = 0.528605
            RInc(345) = 0.505221
            RInc(346) = 0.483554
            RInc(347) = 0.468446
            RInc(348) = 0.444615
            RInc(349) = 0.426062
            RInc(350) = 0.405703
            RInc(351) = 0.381645
            RInc(352) = 0.363396
            RInc(353) = 0.339644
            RInc(354) = 0.318352
            RInc(355) = 0.293073
            RInc(356) = 0.273259
            RInc(357) = 0.24915
            RInc(358) = 0.22191
            RInc(359) = 0.19549
            RInc(360) = 0.167197
            RInc(361) = 0.140346
            RInc(362) = 0.106907
            RInc(363) = 0.073673
            RInc(364) = 0.037916
            RInc(365) = 1.582128
            RInc(366) = 1.570134
            RInc(367) = 1.549642
            RInc(368) = 1.535807
            RInc(369) = 1.518322
            RInc(370) = 1.507496
            RInc(371) = 1.487493
            RInc(372) = 1.469523
            RInc(373) = 1.452685
            RInc(374) = 1.439122
            RInc(375) = 1.423782
            RInc(376) = 1.404097
            RInc(377) = 1.389286
            RInc(378) = 1.373614
            RInc(379) = 1.356172
            RInc(380) = 1.343948
            RInc(381) = 1.326337
            RInc(382) = 1.31255
            RInc(383) = 1.294761
            RInc(384) = 1.279645
            RInc(385) = 1.265842
            RInc(386) = 1.246177
            RInc(387) = 1.227988
            RInc(388) = 1.214746
            RInc(389) = 1.200682
            RInc(390) = 1.183356
            RInc(391) = 1.166872
            RInc(392) = 1.155006
            RInc(393) = 1.140756
            RInc(394) = 1.119675
            RInc(395) = 1.106599
            RInc(396) = 1.090258
            RInc(397) = 1.072904
            RInc(398) = 1.057449
            RInc(399) = 1.039775
            RInc(400) = 1.028525
            RInc(401) = 1.012912
            RInc(402) = 0.997791
            RInc(403) = 0.981242
            RInc(404) = 0.965154
            RInc(405) = 0.950569
            RInc(406) = 0.931514
            RInc(407) = 0.917257
            RInc(408) = 0.901608
            RInc(409) = 0.888644
            RInc(410) = 0.870813
            RInc(411) = 0.856744
            RInc(412) = 0.842106
            RInc(413) = 0.822344
            RInc(414) = 0.805749
            RInc(415) = 0.787735
            RInc(416) = 0.776085
            RInc(417) = 0.759648
            RInc(418) = 0.744349
            RInc(419) = 0.725533
            RInc(420) = 0.713833
            RInc(421) = 0.696605
            RInc(422) = 0.682689
            RInc(423) = 0.661616
            RInc(424) = 0.648204
            RInc(425) = 0.631615
            RInc(426) = 0.617862
            RInc(427) = 0.602149
            RInc(428) = 0.586213
            RInc(429) = 0.567397
            RInc(430) = 0.550948
            RInc(431) = 0.537121
            RInc(432) = 0.519371
            RInc(433) = 0.499328
            RInc(434) = 0.484357
            RInc(435) = 0.469175
            RInc(436) = 0.446666
            RInc(437) = 0.433622
            RInc(438) = 0.41631
            RInc(439) = 0.399531
            RInc(440) = 0.37886
            RInc(441) = 0.361002
            RInc(442) = 0.34158
            RInc(443) = 0.323768
            RInc(444) = 0.308251
            RInc(445) = 0.28018
            RInc(446) = 0.267009
            RInc(447) = 0.245382
            RInc(448) = 0.22069
            RInc(449) = 0.19742
            RInc(450) = 0.175429
            RInc(451) = 0.14757
            RInc(452) = 0.122506
            RInc(453) = 0.095709
            RInc(454) = 0.068864
            RInc(455) = 0.031597

            Return RInc(index)
        End Function

        ''' <summary>
        ''' Returns random inconsistency index for matrix with specified size
        ''' </summary>
        ''' <param name="MatrixSize">Size of the matrix</param>
        ''' <returns>Returns random inconsistency index for matrix with specified size</returns>
        ''' <remarks></remarks>
        Protected Overridable Function GetRandomICIndex(ByVal MatrixSize As Integer) As Double
            Select Case mMatrixSize
                Case 0
                    Return 0
                Case 1
                    Return 0
                Case 2
                    Return 0
                Case 3
                    Return 0.52
                Case 4
                    Return 0.89
                Case 5
                    Return 1.11
                Case 6
                    Return 1.25
                Case 7
                    Return 1.35
                Case 8
                    Return 1.4
                Case 9
                    Return 1.45
                Case 10
                    Return 1.49
                Case 11
                    Return 1.51
                Case 12
                    Return 1.54
                Case 13
                    Return 1.56
                Case 14
                    Return 1.57
                Case 15
                    Return 1.58
                Case Else
                    Return 0
            End Select
        End Function

        Public Sub CalcPrelim(ByRef M As Double(,), ByRef numMissing As Integer)
            Dim n As Integer = mMatrixSize

            ReDim M(n - 1, n - 1)

            numMissing = 0
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    M(i, j) = mMatrix(i, j)
                Next
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = i + 1 To n - 1
                    If M(i, j) <> 0 Then
                        M(j, i) = 1 / M(i, j)
                    Else
                        M(j, i) = 0
                        M(i, i) = M(i, i) + 1
                        M(j, j) = M(j, j) + 1
                        numMissing = numMissing + 1
                    End If
                Next
            Next
        End Sub

        Public Sub CalcLeftEigen(M As Double(,), ByRef WLEFT As Double())
            WLEFT = CalcLeftEigen(M)
        End Sub

        ''' <summary>
        ''' Performs calculation of eigen vector and inconsistency index
        ''' </summary>
        ''' <remarks>Can be overridden. But it is more preferable to override only CalculateEigenVector2 function</remarks>
        Public Overridable Sub Calculate()
            mICIndex = 0.0
            mICRatio = 0.0
            mLambdaMax = 0.0
            InitEigenVector()

            'CalculateEigenVector1()
            CalculateEigenVector2()
        End Sub
    End Class
End Namespace