Imports System.IO
Imports System.IO.Compression

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim SelectedSP As String
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            SelectedSP = OpenFileDialog1.FileName
        Else
            If Not SelectedSP.ToLower().EndsWith(".sp") Then
                MessageBox.Show($"Input file is not a SP.. Ensure it ends in .sp")
                Exit Sub
            End If
        End If

        'Create Output DIR
        Dim ExportDir = "Export/SP"
        Directory.CreateDirectory(ExportDir)

        Using br As New BinaryReader(File.Open(SelectedSP, FileMode.Open))
            br.BaseStream.Position = 4
            Dim HasFFHeader As Boolean

            If br.ReadByte() = Byte.MaxValue Then
                HasFFHeader = True
                br.BaseStream.Position = 64
            Else
                HasFFHeader = False
                br.BaseStream.Position = 0
            End If

            Try
                Dim StartTablePOS = &H2800
                Dim StartDataPOS = &H287C
                If HasFFHeader = True Then
                    StartDataPOS += 64
                    StartTablePOS += 64
                End If

                br.BaseStream.Position = StartTablePOS
                'Start getting Data
                For i As Integer = 1 To 31
                    'Get Length
                    Dim FileLength As UInt32 = ReadUInt32BigEndian(br)

                    'Set Last Table POS
                    Dim LastTablePOS = br.BaseStream.Position

                    'Go to DataStart
                    br.BaseStream.Position = StartDataPOS

                    'Extract Data
                    Dim filePath As String = Path.Combine(ExportDir, $"{i}.bin")
                    Dim bytes As Byte() = br.ReadBytes(FileLength)
                    File.WriteAllBytes(Path.Combine(ExportDir, $"{i}.bin"), bytes)

                    ' Check if the file is a ZIP (starts with "PK")
                    If bytes.Length >= 2 AndAlso bytes(0) = &H50 AndAlso bytes(1) = &H4B Then
                        Dim extractDir As String = Path.Combine(ExportDir, "Extracted")

                        ' Ensure Extracted directory exists
                        If Not Directory.Exists(extractDir) Then
                            Directory.CreateDirectory(extractDir)
                        End If

                        ' Define the extraction path using the original file name
                        Dim zipExtractPath As String = Path.Combine(extractDir, $"Extracted_{i}")

                        ' Extract ZIP contents
                        ExtractZip(filePath, zipExtractPath)
                    End If

                    'Set new Data Start
                    StartDataPOS = br.BaseStream.Position

                    'Go Back to Table
                    br.BaseStream.Position = LastTablePOS
                Next
            Finally
                ' Cleanup if necessary
            End Try
        End Using
    End Sub
    Public Sub ExtractZip(zipFilePath As String, extractPath As String)
        Try
            ' Ensure extraction directory exists
            If Not Directory.Exists(extractPath) Then
                Directory.CreateDirectory(extractPath)
            End If

            ' Extract ZIP contents
            ZipFile.ExtractToDirectory(zipFilePath, extractPath)
            Console.WriteLine($"Extracted: {zipFilePath} -> {extractPath}")

        Catch ex As Exception
            Console.WriteLine($"Error extracting {zipFilePath}: {ex.Message}")
        End Try
    End Sub

    Public Function ReadUInt32BigEndian(reader As BinaryReader) As UInteger
        Dim bytes As Byte() = reader.ReadBytes(4)

        If bytes.Length < 4 Then
            Throw New EndOfStreamException("Not enough bytes to read a UInt32.")
        End If

        ' Convert big-endian bytes to UInt32 explicitly
        Return (CUInt(bytes(0)) << 24) Or
           (CUInt(bytes(1)) << 16) Or
           (CUInt(bytes(2)) << 8) Or
           CUInt(bytes(3))
    End Function
End Class
