Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Xml

Module Program
    Dim WhatTalesGame As New List(Of Tuple(Of String, Integer)) From {
        Tuple.Create("TOB", 10240),
        Tuple.Create("TOC", 5120),
        Tuple.Create("TOW", 5120),
        Tuple.Create("TOP", 0)
    }

    Private TOBSP As New List(Of Tuple(Of Integer, Integer)) From {
        Tuple.Create(0, 10240),
        Tuple.Create(10240, 35840),
        Tuple.Create(35840, 97280),
        Tuple.Create(97280, 128000),
        Tuple.Create(128000, 163480),
        Tuple.Create(163480, 245760),
        Tuple.Create(245760, 261120),
        Tuple.Create(261120, 276480),
        Tuple.Create(276480, 302080),
        Tuple.Create(302080, 317440),
        Tuple.Create(317440, 332800),
        Tuple.Create(332800, 363520),
        Tuple.Create(363520, 409600)
    }
    Private TOCSP As New List(Of Tuple(Of Integer, Integer)) From {
        Tuple.Create(0, 5120),
        Tuple.Create(5120, 35840),
        Tuple.Create(35840, 56320),
        Tuple.Create(56320, 87040),
        Tuple.Create(87040, 148480),
        Tuple.Create(148480, 250880),
        Tuple.Create(250880, 266240),
        Tuple.Create(266240, 286720),
        Tuple.Create(286720, 296960),
        Tuple.Create(296960, 322560),
        Tuple.Create(322560, 353280),
        Tuple.Create(353280, 373760),
        Tuple.Create(373760, 394240),
        Tuple.Create(394240, 409600)
    }
    Private TOWSP As New List(Of Tuple(Of Integer, Integer)) From {
        Tuple.Create(0, 5120),
        Tuple.Create(5120, 50176),
        Tuple.Create(50176, 65536),
        Tuple.Create(65536, 101376),
        Tuple.Create(101376, 152576),
        Tuple.Create(152576, 270336),
        Tuple.Create(270336, 280576),
        Tuple.Create(280576, 301056),
        Tuple.Create(301056, 311296),
        Tuple.Create(311296, 336896),
        Tuple.Create(336896, 367616),
        Tuple.Create(367616, 386048),
        Tuple.Create(386048, 404480),
        Tuple.Create(404480, 409600)
    }

    <STAThread>
    Sub Main(args As String())
        If args.Length = 0 Then
            Console.WriteLine("Usage: TalesOf-SP_Extractor.exe <file-path>")
        Else
            Dim path1 As String = "Export"
            If Not Directory.Exists(path1) Then
                Directory.CreateDirectory(path1)
            End If

            Dim path2 As String = args(0)

            If Not path2.ToLower().EndsWith(".sp") Then
                Console.WriteLine("Input file is not a SP.. Ensure it ends in .sp")
            ElseIf File.Exists(path2) Then
                Using br As New BinaryReader(File.Open(path2, FileMode.Open))
                    br.BaseStream.Position = 4
                    Dim flag As Boolean
                    If br.ReadByte() = Byte.MaxValue Then
                        flag = True
                        br.BaseStream.Position = 64
                    Else
                        flag = False
                        br.BaseStream.Position = 0
                    End If

                    'What Tales of Game
                    Dim FoundRightGame As Boolean
                    Dim FoundGame As String
                    For Each tuple As Tuple(Of String, Integer) In WhatTalesGame
                        Dim game As String = tuple.Item1
                        Dim pos As Integer = tuple.Item2
                        If flag = True Then
                            pos += 64
                        End If
                        br.BaseStream.Position = pos
                        Dim GameType = System.Text.Encoding.ASCII.GetString(br.ReadBytes(3))
                        If GameType.ToUpper = game.ToUpper Then
                            FoundRightGame = True
                            FoundGame = game.ToUpper
                            Exit For
                        Else
                            FoundRightGame = False
                        End If
                    Next

                    If FoundRightGame = True Then
                        Dim FileCount As Integer = 0
                        Select Case FoundGame
                            Case "TOB"
                                Try
                                    For Each tuple As Tuple(Of Integer, Integer) In TOBSP
                                        Dim num As Integer = tuple.Item1
                                        Dim count As Integer = tuple.Item2 - num
                                        Dim str As String = $"{FileCount.ToString}.bin"

                                        If flag Then
                                            num = num + 64
                                        End If

                                        br.BaseStream.Position = num
                                        Dim bytes As Byte() = br.ReadBytes(count)
                                        File.WriteAllBytes(Path.Combine(path1, str), bytes)
                                        FileCount += 1
                                    Next
                                Catch
                                End Try
                            Case "TOC"
                                Try
                                    For Each tuple As Tuple(Of Integer, Integer) In TOCSP
                                        Dim num As Integer = tuple.Item1
                                        Dim count As Integer = tuple.Item2 - num
                                        Dim str As String = $"{FileCount.ToString}.bin"

                                        If flag Then
                                            num = num + 64
                                        End If

                                        br.BaseStream.Position = num
                                        Dim bytes As Byte() = br.ReadBytes(count)
                                        File.WriteAllBytes(Path.Combine(path1, str), bytes)
                                        FileCount += 1
                                    Next
                                Catch
                                End Try
                            Case "TOW"
                                Try
                                    For Each tuple As Tuple(Of Integer, Integer) In TOWSP
                                        Dim num As Integer = tuple.Item1
                                        Dim count As Integer = tuple.Item2 - num
                                        Dim str As String = $"{FileCount.ToString}.bin"

                                        If flag Then
                                            num = num + 64
                                        End If

                                        br.BaseStream.Position = num
                                        Dim bytes As Byte() = br.ReadBytes(count)
                                        File.WriteAllBytes(Path.Combine(path1, str), bytes)
                                        FileCount += 1
                                    Next
                                Catch
                                End Try
                            Case "TOP"
                        End Select
                    ElseIf FoundRightGame = False Then
                        Console.WriteLine("SP does not match a known Tales of Keitai Game.")
                    End If
                End Using
            Else
                Console.WriteLine(String.Format("Error: File not found at '{0}'", path2))
            End If
        End If
    End Sub
End Module
