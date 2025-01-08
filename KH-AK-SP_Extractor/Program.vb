Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.IO

Module Program
    Private KHSP As New List(Of Tuple(Of Integer, Integer)) From {
        Tuple.Create(0, 5120),
        Tuple.Create(5120, 5120),
        Tuple.Create(10240, 81920),
        Tuple.Create(92160, 10240),
        Tuple.Create(102400, 20480),
        Tuple.Create(122880, 296960),
        Tuple.Create(419840, 92160),
        Tuple.Create(512000, 20480)
    }

    <STAThread>
    Sub Main(args As String())
        If args.Length = 0 Then
            Console.WriteLine("Usage: KH-AK-SP_Extractor.exe <file-path>")
        Else
            Dim path1 As String = "Export"
            If Not Directory.Exists(path1) Then
                Directory.CreateDirectory(path1)
            End If

            Dim path2 As String = args(0)

            If Not path2.ToLower().EndsWith(".sp") Then
                Console.WriteLine("Input file is not a SP.. Ensure it ends in .sp")
            ElseIf File.Exists(path2) Then
                Using binaryReader As New BinaryReader(File.Open(path2, FileMode.Open))
                    binaryReader.BaseStream.Position = 4
                    Dim flag As Boolean

                    If binaryReader.ReadByte() = Byte.MaxValue Then
                        flag = True
                        binaryReader.BaseStream.Position = 64
                    Else
                        flag = True
                        binaryReader.BaseStream.Position = 0
                    End If

                    Try
                        For Each tuple As Tuple(Of Integer, Integer) In KHSP
                            Dim num As Integer = tuple.Item1
                            Dim count As Integer = tuple.Item2
                            Dim str As String = ""

                            Select Case num
                                Case 0
                                    str = "0_profiledata.bin"
                                Case 5120
                                    str = "1_item.khar"
                                Case 10240
                                    str = "2_fixed.khar"
                                Case 92160
                                    str = "3_avatar.khar"
                                Case 102400
                                    str = "4_title.khar"
                                Case 122880
                                    str = "5_land.khar"
                                Case 419840
                                    str = "6_MANYTYPES.khar"
                                Case 512000
                                    str = "7_level.khar"
                            End Select

                            If flag Then
                                num = num + 64
                            End If

                            binaryReader.BaseStream.Position = num
                            Dim bytes As Byte() = binaryReader.ReadBytes(count)
                            File.WriteAllBytes(Path.Combine(path1, str), bytes)
                        Next
                    Finally
                        ' Cleanup if necessary
                    End Try
                End Using
            Else
                Console.WriteLine(String.Format("Error: File not found at '{0}'", path2))
            End If
        End If
    End Sub
End Module
