Imports System.IO
Imports System.Text
Imports ICSharpCode.SharpZipLib.Zip

Public Class Form1
    Private Sub bntExtractSP_Click(sender As Object, e As EventArgs) Handles bntExtractSP.Click
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
        Dim ExportDir = $"Export/{Path.GetFileNameWithoutExtension(SelectedSP)}"
        Directory.CreateDirectory(ExportDir)
        ExtractZipFilesFromBinary(SelectedSP, ExportDir)
        Directory.CreateDirectory(Path.Join(ExportDir, "ExtractedFiles"))
        ExtractAllZips(ExportDir)
    End Sub

    Sub ExtractZipFilesFromBinary(ByVal binaryFilePath As String, ByVal outputFolder As String)
        If Not Directory.Exists(outputFolder) Then
            Directory.CreateDirectory(outputFolder)
        End If

        Dim zipSignature As Byte() = {&H50, &H4B, &H3, &H4} ' ZIP file magic number
        Dim binaryData As Byte() = File.ReadAllBytes(binaryFilePath)
        Dim zipFileCount As Integer = 0
        Dim index As Integer = 0

        While index < binaryData.Length - 4
            ' Search for ZIP signature
            If binaryData(index) = zipSignature(0) AndAlso
               binaryData(index + 1) = zipSignature(1) AndAlso
               binaryData(index + 2) = zipSignature(2) AndAlso
               binaryData(index + 3) = zipSignature(3) Then

                ' Found ZIP header
                Dim endIndex As Integer = FindNextZipOrEnd(binaryData, index + 4)
                If endIndex = -1 Then
                    endIndex = binaryData.Length
                End If

                ' Extract ZIP data
                Dim zipData As Byte() = binaryData.Skip(index).Take(endIndex - index).ToArray()
                Dim zipFilePath As String = Path.Combine(outputFolder, $"extracted_{zipFileCount}.zip")
                File.WriteAllBytes(zipFilePath, zipData)

                Console.WriteLine($"Extracted ZIP file: {zipFilePath}")

                zipFileCount += 1
                index = endIndex ' Move index to next potential ZIP header
            Else
                index += 1
            End If
        End While

        Console.WriteLine($"Extraction complete. {zipFileCount} ZIP files extracted.")
    End Sub
    Function FindNextZipOrEnd(ByVal data As Byte(), ByVal startIndex As Integer) As Integer
        Dim zipSignature As Byte() = {&H50, &H4B, &H3, &H4}

        For i As Integer = startIndex To data.Length - 4
            If data(i) = zipSignature(0) AndAlso
               data(i + 1) = zipSignature(1) AndAlso
               data(i + 2) = zipSignature(2) AndAlso
               data(i + 3) = zipSignature(3) Then
                Return i ' Next ZIP found
            End If
        Next

        Return -1 ' No more ZIPs found
    End Function
    Sub ExtractAllZips(ByVal outputFolder As String)
        ' Get all extracted ZIP files
        Dim zipFiles As String() = Directory.GetFiles(outputFolder, "*.zip")

        For Each zipFile In zipFiles
            Dim extractFolder As String = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(zipFile))

            ' Ensure the folder exists
            If Not Directory.Exists(extractFolder) Then
                Directory.CreateDirectory(extractFolder)
            End If

            Try
                ' Extract ZIP contents using SharpZipLib
                ExtractZipFile(zipFile, extractFolder)
                Console.WriteLine($"Extracted contents of {zipFile} to {extractFolder}")
            Catch ex As Exception
                Console.WriteLine($"Error extracting {zipFile}: {ex.Message}")
            End Try
        Next

        Console.WriteLine("All ZIP files have been extracted.")
    End Sub
    Sub ExtractZipFile(ByVal zipPath As String, ByVal extractPath As String)
        Using zipFile As New ZipFile(File.OpenRead(zipPath))
            For Each entry As ZipEntry In zipFile
                If Not entry.IsFile Then Continue For ' Skip directories

                Dim entryPath As String = Path.Combine(extractPath, entry.Name)
                Dim entryDirectory As String = Path.GetDirectoryName(entryPath)

                If Not Directory.Exists(entryDirectory) Then
                    Directory.CreateDirectory(entryDirectory)
                End If

                Using zipStream As Stream = zipFile.GetInputStream(entry),
                      fileStream As FileStream = File.Create(entryPath)
                    zipStream.CopyTo(fileStream)
                End Using
            Next
        End Using
    End Sub

    Function ExtractARCFiles(inputArc)
        Using br As BinaryReader = New BinaryReader(File.Open(inputArc, FileMode.Open))
            Dim Checkheader = Encoding.ASCII.GetString(br.ReadBytes(3))
            If Checkheader = "ARC" Then
                br.ReadByte()
                'Create DIR
                Dim ExportDIR = Path.Join("Extracted", Path.GetFileNameWithoutExtension(inputArc))
                Directory.CreateDirectory(ExportDIR)

                Dim TotalFileSize = ReadUInt16BigEndian(br)
                Dim FileCount = ReadUInt16BigEndian(br)

                Dim StartPointerTablePOS = br.BaseStream.Position
                For i As Integer = 1 To FileCount
                    br.ReadBytes(6)
                Next
                Dim StartOfFilesPOS = br.BaseStream.Position
                br.BaseStream.Position = StartPointerTablePOS

                For i As Integer = 1 To FileCount
                    Dim FileNum = ReadUInt16BigEndian(br)
                    Dim StartFilePOSWITHOffset = ReadUInt16BigEndian(br) + StartOfFilesPOS
                    Dim FileLength = ReadUInt16BigEndian(br)
                    Dim LastPOS = br.BaseStream.Position
                    br.BaseStream.Position = StartFilePOSWITHOffset
                    Dim FileDataBytes = br.ReadBytes(FileLength)

                    ' Check for known file signatures
                    Dim isGIF As Boolean = FileDataBytes.Length >= 3 AndAlso
                           FileDataBytes(0) = &H47 AndAlso  ' G
                           FileDataBytes(1) = &H49 AndAlso  ' I
                           FileDataBytes(2) = &H46          ' F

                    Dim isMLD As Boolean = FileDataBytes.Length >= 3 AndAlso
                           FileDataBytes(0) = &H6D AndAlso  ' m
                           FileDataBytes(1) = &H65 AndAlso  ' e
                           FileDataBytes(2) = &H6C          ' l

                    Dim isJPEG As Boolean = FileDataBytes.Length >= 3 AndAlso
                            FileDataBytes(0) = &HFF AndAlso
                            FileDataBytes(1) = &HD8 AndAlso
                            FileDataBytes(2) = &HFF

                    Dim isBMP As Boolean = FileDataBytes.Length >= 2 AndAlso
                           FileDataBytes(0) = &H42 AndAlso  ' B
                           FileDataBytes(1) = &H4D          ' M

                    ' Determine file extension
                    Dim FileExtension As String
                    If isGIF Then
                        FileExtension = ".gif"
                    ElseIf isMLD Then
                        FileExtension = ".mld"
                    ElseIf isJPEG Then
                        FileExtension = ".jpg"
                    ElseIf isBMP Then
                        FileExtension = ".bmp"
                    Else
                        FileExtension = ".bin"
                    End If

                    ' Save file
                    File.WriteAllBytes(Path.Join(ExportDIR, FileNum & FileExtension), FileDataBytes)

                    br.BaseStream.Position = LastPOS
                Next
                Return True
            Else
                Return False
            End If
        End Using
    End Function

    Private Sub btnExtractARCSingle_Click(sender As Object, e As EventArgs) Handles btnExtractARCSingle.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            ExtractARCFiles(OpenFileDialog1.FileName)
        End If
    End Sub
    Private Sub btnExtractARCBatch_Click(sender As Object, e As EventArgs) Handles btnExtractARCBatch.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            For Each F In Directory.GetFiles(FolderBrowserDialog1.SelectedPath, "*.*", SearchOption.AllDirectories)
                ExtractARCFiles(F)
            Next
        End If
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
    Public Function ReadUInt16BigEndian(reader As BinaryReader) As UShort
        Dim bytes As Byte() = reader.ReadBytes(2)

        If bytes.Length < 2 Then
            Throw New EndOfStreamException("Not enough bytes to read a UInt16.")
        End If

        ' Convert big-endian bytes to UInt16
        Return CUShort((CUShort(bytes(0)) << 8) Or CUShort(bytes(1)))
    End Function
End Class
