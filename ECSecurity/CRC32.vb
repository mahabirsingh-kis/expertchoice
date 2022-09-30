Imports System.Collections.Generic
Imports System.Linq
Imports System.Security.Cryptography

Namespace ECSecurity

    Public Module CRC

        ' Copyright (c) Damien Guard.  All rights reserved.
        ' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
        ' You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
        ' Originally published at http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net

        ''' <summary>
        ''' Implements a 32-bit CRC hash algorithm compatible with Zip etc.
        ''' </summary>
        ''' <remarks>
        ''' Crc32 should only be used for backward compatibility with older file formats
        ''' and algorithms. It is not secure enough for new applications.
        ''' If you need to call multiple times for the same data either use the HashAlgorithm
        ''' interface or remember that the result of one Compute call needs to be ~ (XOR) before
        ''' being passed in as the seed for the next Compute call.
        ''' </remarks>
        Public NotInheritable Class CRC32
            Inherits HashAlgorithm
            Public Const DefaultPolynomial As UInt32 = &HEDB88320UI
            Public Const DefaultSeed As UInt32 = &HFFFFFFFFUI

            Shared defaultTable As UInt32()

            ReadOnly seed As UInt32
            ReadOnly table As UInt32()
            Private _hash As UInt32

            Public Sub New()
                Me.New(DefaultPolynomial, DefaultSeed)
            End Sub

            Public Sub New(polynomial As UInt32, seed As UInt32)
                table = InitializeTable(polynomial)
                Me.seed = InlineAssignHelper(_hash, seed)
            End Sub

            Public Overrides Sub Initialize()
                _hash = seed
            End Sub

            Protected Overrides Sub HashCore(array As Byte(), ibStart As Integer, cbSize As Integer)
                _hash = CalculateHash(table, _hash, array, ibStart, cbSize)
            End Sub

            Protected Overrides Function HashFinal() As Byte()
                Dim hashBuffer = UInt32ToBigEndianBytes(Not _hash)
                HashValue = hashBuffer
                Return hashBuffer
            End Function

            Public Overrides ReadOnly Property HashSize() As Integer
                Get
                    Return 32
                End Get
            End Property

            ' D4079 ===
            Public Shared Function ComputeAsInt(sValue As String) As Integer
                Return Convert.ToInt32(Compute(sValue).ToString("X"), 16)
            End Function
            ' D4079 ==

            Public Shared Function Compute(sValue As String) As UInt32
                Return Compute(DefaultSeed, System.Text.Encoding.ASCII.GetBytes(sValue))
            End Function

            Public Shared Function Compute(buffer As Byte()) As UInt32
                Return Compute(DefaultSeed, buffer)
            End Function

            Public Shared Function Compute(seed As UInt32, buffer As Byte()) As UInt32
                Return Compute(DefaultPolynomial, seed, buffer)
            End Function

            Public Shared Function Compute(polynomial As UInt32, seed As UInt32, buffer As Byte()) As UInt32
                Return Not CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length)
            End Function

            Private Shared Function InitializeTable(polynomial As UInt32) As UInt32()
                If polynomial = DefaultPolynomial AndAlso defaultTable IsNot Nothing Then
                    Return defaultTable
                End If

                Dim createTable = New UInt32(255) {}
                For i As Integer = 0 To 255
                    Dim entry As UInteger = CType(i, UInteger)
                    For j As Integer = 0 To 7
                        If (entry And 1) = 1 Then
                            entry = (entry >> 1) Xor polynomial
                        Else
                            entry = entry >> 1
                        End If
                    Next
                    createTable(i) = entry
                Next

                If polynomial = DefaultPolynomial Then
                    defaultTable = createTable
                End If

                Return createTable
            End Function

            Private Shared Function CalculateHash(table As UInt32(), seed As UInt32, buffer As IList(Of Byte), start As Integer, size As Integer) As UInt32
                Dim hash = seed
                For i As Integer = start To start + (size - 1)
                    hash = (hash >> 8) Xor table(CInt(buffer(i) Xor hash And &HFF))
                Next
                Return hash
            End Function

            Private Shared Function UInt32ToBigEndianBytes(uint32 As UInt32) As Byte()
                Dim result = BitConverter.GetBytes(uint32)

                If BitConverter.IsLittleEndian Then
                    Array.Reverse(result)
                End If

                Return result
            End Function

            Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function

        End Class

    End Module

End Namespace

