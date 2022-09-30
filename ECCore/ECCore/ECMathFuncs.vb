Namespace ECCore.MathFuncs
    ''' <summary>
    ''' This module contains math functions for working with vectors, matrices, etc.
    ''' There are general functions for matrices and optimized functions for square matrices (see names of the functions)
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Public Module ECMathFuncs

        Public Enum StandardDeviationMode 'C0633
            sdmUnbiased = 0
            sdmBiased = 1
        End Enum

        ''' <summary>
        ''' Multiplies square matrix A by square matrix B and puts result in ResultMatrix. Both matrices are (MatrixSize x MatrixSize).
        ''' </summary>
        ''' <param name="A">First square matrix</param>
        ''' <param name="B">Second square matrix</param>
        ''' <param name="ResultMatrix">Result matrix</param>
        ''' <param name="MatrixSize">Size of matrices</param>
        ''' <remarks></remarks>
        Public Sub MultSquareMatrix(ByVal A(,) As Double, ByVal B(,) As Double, ByRef ResultMatrix(,) As Double, ByVal MatrixSize As Integer)
            Dim i As Integer, j As Integer, k As Integer
            Dim s As Double

            ReDim ResultMatrix(MatrixSize - 1, MatrixSize - 1)

            For i = 0 To MatrixSize - 1
                For j = 0 To MatrixSize - 1
                    s = 0
                    For k = 0 To MatrixSize - 1
                        s = s + A(i, k) * B(k, j)
                    Next
                    ResultMatrix(i, j) = s
                Next
            Next
        End Sub

        ''' <summary>
        ''' Multiplies matrix A by matrix B and puts result in ResultMatrix.
        ''' </summary>
        ''' <param name="A">First matrix</param>
        ''' <param name="B">Second matrix</param>
        ''' <param name="ResultMatrix">Result matrix</param>
        ''' <param name="RowCountA">Row count in first matrix</param>
        ''' <param name="ColCountA">Column count in first matrix</param>
        ''' <param name="ColCountB">Column count in second matrix</param>
        ''' <remarks>Matrix A should be (RowCountA x ColCountA).
        ''' Matrix B should be (ColCountA x ColCountB).
        ''' ResultMatrix will be (RowCountA x ColCountB).</remarks>
        Public Sub MultMatrix(ByVal A(,) As Double, ByVal B(,) As Double, ByRef ResultMatrix(,) As Double,
            ByVal RowCountA As Integer, ByVal ColCountA As Integer, ByVal ColCountB As Integer)

            Dim i As Integer, j As Integer, k As Integer
            Dim s As Double

            ReDim ResultMatrix(RowCountA - 1, ColCountB - 1)

            For i = 0 To RowCountA - 1
                For j = 0 To ColCountB - 1
                    s = 0
                    For k = 0 To ColCountA - 1
                        s = s + A(i, k) * B(k, j)
                    Next
                    ResultMatrix(i, j) = s
                Next
            Next
        End Sub

        ''' <summary>
        ''' Multiplies matrix A (RowCount x ColCount) by a vector V (ColCount x 1) and puts retuls in ResultVector (MatrixSize x 1)
        ''' </summary>
        ''' <param name="A">Matrix</param>
        ''' <param name="V">Vector</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <param name="RowCount">Row count in the matrix</param>
        ''' <param name="ColCount">Column count in the matrix. This is also a number of elements in a vector</param>
        ''' <remarks></remarks>
        Public Sub MultMatrixByVector(ByVal A(,) As Double, ByVal V() As Double, ByRef ResultVector() As Double, ByVal RowCount As Integer, ByVal ColCount As Integer)
            Dim i As Integer, j As Integer
            Dim s As Double

            ReDim ResultVector(RowCount - 1)

            For i = 0 To RowCount - 1
                s = 0
                For j = 0 To ColCount - 1
                    s = s + A(i, j) * V(j)
                Next
                ResultVector(i) = s
            Next
        End Sub


        Public Sub MultVectorByMatrix(ByVal V() As Double, ByVal A(,) As Double, ByRef ResultVector() As Double, ByVal RowCount As Integer, ByVal ColCount As Integer)
            Dim i As Integer, j As Integer
            Dim s As Double

            ReDim ResultVector(ColCount - 1)

            For i = 0 To ColCount - 1
                s = 0
                For j = 0 To RowCount - 1
                    s = s + V(j) * A(j, i)
                Next
                ResultVector(i) = s
            Next
        End Sub

        ''' <summary>
        ''' Multiplies square matrix A (MatrixSize x MatrixSize) by a vector V (MatrixSize x 1) and puts retuls in ResultVector (MatrixSize x 1)
        ''' </summary>
        ''' <param name="A">Matrix</param>
        ''' <param name="V">Vector</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <remarks></remarks>
        Public Sub MultSquareMatrixByVector(ByVal A(,) As Double, ByVal V() As Double, ByRef ResultVector() As Double, ByVal MatrixSize As Integer)
            Dim i As Integer, j As Integer
            Dim s As Double

            ReDim ResultVector(MatrixSize - 1)

            For i = 0 To MatrixSize - 1
                s = 0
                For j = 0 To MatrixSize - 1
                    s = s + A(i, j) * V(j)
                Next
                ResultVector(i) = s
            Next
        End Sub

        ''' <summary>
        ''' Multiplies vector V by a scalar N and puts result in ResultVector
        ''' </summary>
        ''' <param name="V">Vector</param>
        ''' <param name="N">Scalar</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <param name="VectorSize">Vector size (number of elements in the vector)</param>
        ''' <remarks>This function multiplies each element of the vector by a scalar N</remarks>
        Public Sub MultVectorByScalar(ByVal V() As Double, ByVal N As Double, ByRef ResultVector() As Double, ByVal VectorSize As Integer)
            ReDim ResultVector(VectorSize - 1)

            Dim i As Integer
            For i = 0 To VectorSize - 1
                ResultVector(i) = V(i) * N
            Next
        End Sub

        ''' <summary>
        ''' Returns scalar multiplication of two vectors
        ''' </summary>
        ''' <param name="V1">First vector</param>
        ''' <param name="V2">Second vector</param>
        ''' <param name="VectorSize">Size of the vector (number of elements)</param>
        ''' <returns>Returns Double value which is a scalar multiplication of two vectors</returns>
        ''' <remarks></remarks>
        Public Function VectorScalarMult(ByVal V1() As Double, ByVal V2() As Double, ByVal VectorSize As Integer) As Double
            Dim i As Integer
            Dim s As Double

            For i = 0 To VectorSize - 1
                s = s + V1(i) * V2(i)
            Next

            Return s
        End Function

        ''' <summary>
        ''' Calculates sums of elements in each row of a square Matrix and puts it in ResultVector
        ''' </summary>
        ''' <param name="Matrix">Matrix</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <remarks>In ResultVector each element holds the sum of elements of corresponding row of a Matrix</remarks>
        Public Sub RowSumInSquareMatrix(ByVal Matrix(,) As Double, ByVal MatrixSize As Integer, ByRef ResultVector() As Double)
            ReDim ResultVector(MatrixSize - 1)

            Dim i As Integer, j As Integer
            Dim s As Double

            For i = 0 To MatrixSize - 1
                s = 0
                For j = 0 To MatrixSize - 1
                    s = s + Matrix(i, j)
                Next
                ResultVector(i) = s
            Next
        End Sub

        ''' <summary>
        ''' Calculates sums of elements in each row of a Matrix and puts it in ResultVector
        ''' </summary>
        ''' <param name="Matrix">Matrix</param>
        ''' <param name="RowCount">Row count in the matrix</param>
        ''' <param name="ColCount">Column count in the matrix</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <remarks>In ResultVector each element holds the sum of elements of corresponding row of a Matrix</remarks>
        Public Sub RowSumInMatrix(ByVal Matrix(,) As Double, ByVal RowCount As Integer, ByVal ColCount As Integer, ByRef ResultVector() As Double)
            ReDim ResultVector(RowCount - 1)

            Dim i As Integer, j As Integer
            Dim s As Double

            For i = 0 To RowCount - 1
                s = 0
                For j = 0 To ColCount - 1
                    s = s + Matrix(i, j)
                Next
                ResultVector(i) = s
            Next
        End Sub

        ''' <summary>
        ''' Calculates sums of elements in each column of a square Matrix and puts it in ResultVector
        ''' </summary>
        ''' <param name="Matrix">Matrix</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <remarks>In ResultVector each element holds the sum of elements of corresponding row of a Matrix</remarks>
        Public Sub ColSumInSquareMatrix(ByVal Matrix(,) As Double, ByVal MatrixSize As Integer, ByRef ResultVector() As Double)
            ReDim ResultVector(MatrixSize - 1)

            Dim i As Integer, j As Integer
            Dim s As Double

            For j = 0 To MatrixSize - 1
                s = 0
                For i = 0 To MatrixSize - 1
                    s = s + Matrix(i, j)
                Next
                ResultVector(j) = s
            Next
        End Sub

        ''' <summary>
        ''' Calculates sums of elements in each column of a Matrix and puts it in ResultVector
        ''' </summary>
        ''' <param name="Matrix">Matrix</param>
        ''' <param name="RowCount">Row count in the matrix</param>
        ''' <param name="ColCount">Column count in the matrix</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <remarks>In ResultVector each element holds the sum of elements of corresponding row of a Matrix</remarks>
        Public Sub ColSumInMatrix(ByVal Matrix(,) As Double, ByVal RowCount As Integer, ByVal ColCount As Integer, ByRef ResultVector() As Double)
            ReDim ResultVector(ColCount - 1)

            Dim i As Integer, j As Integer
            Dim s As Double

            For j = 0 To ColCount - 1
                s = 0
                For i = 0 To RowCount - 1
                    s = s + Matrix(i, j)
                Next
                ResultVector(j) = s
            Next
        End Sub

        ''' <summary>
        ''' Returns the sum of all elements of the vector V
        ''' </summary>
        ''' <param name="V">Vector</param>
        ''' <param name="VectorSize">Vector size</param>
        ''' <returns>Returns the sum of all elements of the vector V</returns>
        ''' <remarks></remarks>
        Public Function VectorSum(ByVal V() As Double, ByVal VectorSize As Integer) As Double
            Dim i As Integer
            Dim s As Double = 0
            For i = 0 To VectorSize - 1
                s = s + V(i)
            Next
            Return s
        End Function

        ''' <summary>
        ''' Normalizes vector V
        ''' </summary>
        ''' <param name="V">Vector</param>
        ''' <param name="VectorSize">Vector size</param>
        ''' <remarks>This routine calculates the sum of all elements of the vector and after that divides each element by that sum. 
        ''' So, after normalization the sum of all elements will be equals 1.</remarks>
        Public Sub NormalizeVector(ByRef V() As Double, ByVal VectorSize As Integer)
            Dim i As Integer
            Dim s As Double = VectorSum(V, VectorSize)
            For i = 0 To VectorSize - 1
                V(i) /= s
            Next
        End Sub

        ''' <summary>
        ''' Calculates a vector each element of which is an absolute value of subtraction of corresponding elements of 2 input vectors
        ''' </summary>
        ''' <param name="V1">First vector</param>
        ''' <param name="V2">Second vector</param>
        ''' <param name="ResultVector">Result vector</param>
        ''' <param name="VectorSize">Vector size</param>
        ''' <remarks>Because the result vector holds the absolute values of subtraction the order of parameters V1 and V2 doesn't matter.</remarks>
        Public Sub VectorSubAbs(ByVal V1() As Double, ByVal V2() As Double, ByRef ResultVector() As Double, ByVal VectorSize As Integer)
            ReDim ResultVector(VectorSize - 1)

            Dim i As Integer
            For i = 0 To VectorSize - 1
                ResultVector(i) = Math.Abs(V1(i) - V2(i))
            Next
        End Sub

        ''' <summary>
        ''' Creates a copy of a square matrix
        ''' </summary>
        ''' <param name="SourceMatrix">Source matrix to copy</param>
        ''' <param name="DestMatrix">Destination matrix</param>
        ''' <param name="MatrixSize">Matrix size</param>
        ''' <remarks>It is not necessary to initialize destination matrix before passing it to this function</remarks>
        Public Sub CopySquareMatrix(ByVal SourceMatrix(,) As Double, ByRef DestMatrix(,) As Double, ByVal MatrixSize As Integer)
            ReDim DestMatrix(MatrixSize - 1, MatrixSize - 1)

            Dim i As Integer, j As Integer
            For i = 0 To MatrixSize - 1
                For j = 0 To MatrixSize - 1
                    DestMatrix(i, j) = SourceMatrix(i, j)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Creates a copy of a matrix
        ''' </summary>
        ''' <param name="SourceMatrix">Source matrix to copy</param>
        ''' <param name="DestMatrix">Destination matrix</param>
        ''' <param name="RowCount">Row count in the matrix</param>
        ''' <param name="ColCount">Column count in the matrix</param>
        ''' <remarks>It is not necessary to initialize destination matrix before passing it to this function</remarks>
        Public Sub CopyMatrix(ByVal SourceMatrix(,) As Double, ByRef DestMatrix(,) As Double, ByVal RowCount As Integer, ByVal ColCount As Integer)
            ReDim DestMatrix(RowCount - 1, ColCount - 1)

            Dim i As Integer, j As Integer
            For i = 0 To RowCount - 1
                For j = 0 To ColCount - 1
                    DestMatrix(i, j) = SourceMatrix(i, j)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Creates a copy of a vector
        ''' </summary>
        ''' <param name="SourceVector">Source vector to copy</param>
        ''' <param name="DestVector">Destination vector</param>
        ''' <param name="VectorSize">Vector size</param>
        ''' <remarks>It is not necessary to initialize destination vector before passing it to this function</remarks>
        Public Sub CopyVector(ByVal SourceVector() As Double, ByRef DestVector() As Double, ByVal VectorSize As Integer)
            ReDim DestVector(VectorSize - 1)

            Dim i As Integer
            For i = 0 To VectorSize - 1
                DestVector(i) = SourceVector(i)
            Next
        End Sub

        Public Function CalculateVarianceAndStandardDeviation(ByVal SetOfDoubleValues As List(Of Double), ByRef Variance As Double, ByRef StandardDeviation As Double, ByVal StdDeviationMode As StandardDeviationMode) As Boolean 'C0631 'C0633
            If SetOfDoubleValues Is Nothing Then
                Return False
            End If

            If (StdDeviationMode = StandardDeviationMode.sdmUnbiased And SetOfDoubleValues.Count <= 1) Or
                (StdDeviationMode = StandardDeviationMode.sdmBiased And SetOfDoubleValues.Count = 0) Then 'C0633
                Return False
            End If

            Dim count As Integer = SetOfDoubleValues.Count

            Dim sum As Double = 0
            For Each value As Double In SetOfDoubleValues
                sum += value
            Next

            Dim mean As Double = sum / count

            Variance = 0
            For Each value As Double In SetOfDoubleValues
                Variance += Math.Pow(value - mean, 2)
            Next

            'Variance /= count - 1 'C0633

            'C0633===
            Select Case StdDeviationMode
                Case StandardDeviationMode.sdmUnbiased
                    Variance /= count - 1
                Case StandardDeviationMode.sdmBiased
                    Variance /= count
            End Select
            'C0633==

            StandardDeviation = Math.Sqrt(Variance) 'C0632

            Return True
        End Function
    End Module
End Namespace