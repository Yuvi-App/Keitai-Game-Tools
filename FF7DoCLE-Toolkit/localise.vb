Imports System.IO
Imports System.Net.Http
Imports System.Text

Public Class localise
    Shared header 'first 4 bytes
    Shared u1  'next 4
    Shared TextOffset  'next 4
    Shared CountofPTEnteries  'next 4

    Public Shared Function LoadlocaliseFile(InputFile As String) As List(Of String)
        Dim ListofPT As New List(Of Integer)
        Dim ListofExportedStrings As New List(Of String)

        If Not File.Exists(InputFile) Then
            Console.WriteLine($"File not found: {InputFile}")
            Return ListofExportedStrings
        End If

        Try
            Using br As New BinaryReader(File.Open(InputFile, FileMode.Open, FileAccess.Read))
                ' Check file length to ensure it's long enough to contain header and 3 UInt32s
                If br.BaseStream.Length < 16 Then
                    Console.WriteLine("File too short to be a valid localise file.")
                    Return ListofExportedStrings
                End If

                Dim header As String = Encoding.UTF8.GetString(br.ReadBytes(4))
                If header <> "LOC0" Then
                    Console.WriteLine("Invalid localise file header.")
                    Return ListofExportedStrings
                End If

                Dim u1 As UInteger = br.ReadUInt32()
                Dim TextOffset As UInteger = br.ReadUInt32()
                Dim CountofPTEnteries As UInteger = br.ReadUInt32()

                ' Ensure the pointer table doesn't exceed file size
                If br.BaseStream.Position + (CountofPTEnteries * 4) > br.BaseStream.Length Then
                    Console.WriteLine("Pointer table goes beyond end of file.")
                    Return ListofExportedStrings
                End If

                Dim StartofTextBlock = br.BaseStream.Position + (CountofPTEnteries * 4)
                'For i As Integer = 1 To CountofPTEnteries
                '    Dim PTOffset As UInteger = br.ReadUInt32()
                '    Dim CompletedPTLocation As UInteger = StartofTextBlock + PTOffset

                '    If CompletedPTLocation < br.BaseStream.Length Then
                '        ListofPT.Add(CInt(CompletedPTLocation))
                '    Else
                '        Console.WriteLine($"Pointer {CompletedPTLocation:X} out of file bounds.")
                '    End If
                'Next

                br.BaseStream.Position = StartofTextBlock
                Dim bytes As New List(Of Byte)
                While br.BaseStream.Position < br.BaseStream.Length
                    Dim b As Byte = br.ReadByte()

                    If b = 0 Then
                        ' End of one string — add newline
                        If bytes.Count > 0 Then
                            bytes.Add(&HA) ' Append 0x0A (line feed) instead of 0x00
                        End If

                        ' Convert to string and store
                        Dim ExportString As String = Encoding.UTF8.GetString(bytes.ToArray())
                        ListofExportedStrings.Add(ExportString)
                        bytes.Clear()
                    Else
                        bytes.Add(b)
                    End If
                End While

                ' Ensure export directory exists
                Dim exportPath As String = Path.Combine("export", Path.GetFileNameWithoutExtension(InputFile) & ".txt")
                Directory.CreateDirectory(Path.GetDirectoryName(exportPath))

                Using sw As New StreamWriter(exportPath, False, Encoding.UTF8)
                    For Each L In ListofExportedStrings
                        sw.WriteLine(L)
                    Next
                End Using

            End Using
        Catch ex As Exception
            Console.WriteLine($"Error reading file: {ex.Message}")
        End Try

        Return ListofExportedStrings
    End Function

End Class
