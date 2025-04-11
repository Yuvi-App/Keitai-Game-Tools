Imports System.IO
Imports System.Text
Imports System.Windows.Forms.VisualStyles
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
                Dim ExportDIR = Path.Join("Extracted_ARCS", Path.GetFileNameWithoutExtension(inputArc))
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
    Private Sub btnRepackARCSingle_Click(sender As Object, e As EventArgs) Handles btnRepackARCSingle.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            RepackARC(FolderBrowserDialog1.SelectedPath)
        End If
    End Sub
    Function RepackARC(InputFolder)
        Dim NewARCFolder = $"Created/ARC/"
        Dim NewARCFile = $"{NewARCFolder}{Path.GetFileNameWithoutExtension(InputFolder)}.dat"
        Directory.CreateDirectory(NewARCFolder)

        If File.Exists(NewARCFile) = True Then
            File.Delete(NewARCFile)
        End If
        Using bw As BinaryWriter = New BinaryWriter(File.Open(NewARCFile, FileMode.Create))
            bw.Write({&H41, &H52, &H43, &H0}) 'ARC Header

            ' Total File Size UINT16, come back to this later
            Dim TotalFileSizePOS = bw.BaseStream.Position
            bw.Write({&H0, &H0})

            ' Total File Count Uint16
            Dim FileCount = Directory.GetFiles(InputFolder).Count
            WriteUInt16BigEndian(bw, FileCount)

            ' Write File Table
            Dim CurrentFileCount = 0
            Dim CurrentFileOffset = 0
            For Each F In Directory.GetFiles(InputFolder)
                WriteUInt16BigEndian(bw, CurrentFileCount) ' Current File Number Starting at 0
                WriteUInt16BigEndian(bw, CurrentFileOffset) ' Current file offset
                Dim fileSize As Long = New FileInfo(F).Length
                WriteUInt16BigEndian(bw, fileSize) ' File Size

                CurrentFileOffset += fileSize
                CurrentFileCount += 1
            Next

            ' Write File Data
            For Each F In Directory.GetFiles(InputFolder)
                bw.Write(File.ReadAllBytes(F))
            Next

            ' Update Total Size
            Dim TotalARCSize = bw.BaseStream.Position
            bw.BaseStream.Position = TotalFileSizePOS
            WriteUInt16BigEndian(bw, TotalARCSize) ' Total Arc Size
        End Using

    End Function
    Public Sub WriteUInt16BigEndian(writer As BinaryWriter, value As UShort)
        ' Extract the high and low bytes and write them in big-endian order
        writer.Write(CByte((value >> 8) And &HFF)) ' MSB (High Byte)
        writer.Write(CByte(value And &HFF))        ' LSB (Low Byte)
    End Sub
    Function FixScriptPointers()
        'first two bytes look to be standard for script?
        'next two are the legth of table
        'read that chunk then you get a another two byets of a footer
        ''001b = new scene
        '541b = continue scene switch textbox

        Using br As BinaryReader = New BinaryReader(File.Open("HitTestCode", FileMode.Open))
            Dim u1 = br.ReadUInt16
            Dim HeaderSize = br.ReadUInt16
            Dim u2 = br.ReadUInt16
        End Using
    End Function

    Private Sub btnExtractText_Click(sender As Object, e As EventArgs) Handles btnExtractText.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            Dim selectedFile As String = OpenFileDialog1.FileName
            Dim folderPath As String = Path.GetDirectoryName(selectedFile)

            Using Br As BinaryReader = New BinaryReader(File.Open(OpenFileDialog1.FileName, FileMode.Open))
                Dim lastpos
                Dim LinePOSList As New List(Of Integer)
                Dim StringTextBoxList As New List(Of String)
                Dim PointerPOSList As New List(Of Integer)

                'We First need to get the First TextStringLocation to Mark end of PointerTable
                Dim EndofPT As Integer
                Dim U1 = Br.ReadBytes(2)
                Dim PointerOffset = ReadUInt16BigEndian(Br)
                While True
                    Dim checkbyte = Br.ReadByte
                    If checkbyte = 0 Then
                        Dim nextcheckbyte = Br.ReadByte
                        If nextcheckbyte = &H1B Then
                            EndofPT = Br.BaseStream.Position + PointerOffset
                            Exit While
                        End If
                    End If
                End While

                'Now Loop again to get all pointers
                Br.BaseStream.Position = 0
                While Br.BaseStream.Position <= EndofPT - 2
                    Dim pos As Long = Br.BaseStream.Position
                    Dim b1 As Byte = Br.ReadByte()

                    ' Peek ahead without advancing permanently
                    If Br.BaseStream.Position > EndofPT Then Exit While ' avoid overflow
                    Dim b2 As Byte = Br.ReadByte()

                    ' Combine to word (little-endian: b1 is low byte, b2 is high byte)
                    Dim word As UShort = CUShort(b1) Or (CUShort(b2) << 8)

                    ' Check for scene start marker
                    If word = &H1B00 OrElse word = &H1B54 Then
                        ' Rewind to the start of the pattern
                        Br.BaseStream.Position = pos

                        ' Start Getting Pointers (start of scene pointers)
                        Dim TypeofScene = Br.ReadBytes(2)
                        Dim Scene_u1 = Br.ReadBytes(2)
                        PointerPOSList.Add(Br.BaseStream.Position)
                        LinePOSList.Add(Br.ReadUInt16) 'TextBox Line 1

                        ' Lopp inside the Pointers to get text pointers
                        While True
                            Dim validPatterns As List(Of Byte()) = New List(Of Byte()) From {
                                New Byte() {&H1B, &HB5, &H0},
                                New Byte() {&H7, &HB6, &H0},
                                New Byte() {&H7, &HC3, &H0},
                                New Byte() {&H7, &HD0, &H0},
                                New Byte() {&H7, &HDD, &H0}
                            }

                            Dim posBefore = Br.BaseStream.Position
                            Dim next3Bytes() As Byte = Br.ReadBytes(3)
                            If next3Bytes.Length < 3 Then Exit While

                            Dim matchFound As Boolean = validPatterns.Any(Function(p) p.SequenceEqual(next3Bytes))

                            If matchFound Then
                                ' Add to PointerPOS List
                                PointerPOSList.Add(Br.BaseStream.Position)
                                ' Matched a valid 3-byte pointer indicator
                                LinePOSList.Add(Br.ReadUInt16()) ' Read the pointer that follows
                            Else
                                ' Rewind if no match
                                Br.BaseStream.Position = posBefore
                                Exit While
                            End If
                        End While
                        PointerPOSList.Add(0) ' This is so i know the end of scene textbox
                        LinePOSList.Add(0) ' This is so i know the end of scene textbox
                    Else
                        ' Rewind back one byte so we move forward by only 1 byte
                        Br.BaseStream.Position = pos + 1
                    End If
                End While

                ' Go get the text at the pointers
                For Each L In LinePOSList
                    If L = 0 OrElse L >= Br.BaseStream.Length Then
                        StringTextBoxList.Add("\n")
                    Else
                        Br.BaseStream.Position = L
                        Dim bytes As New List(Of Byte)

                        While Br.BaseStream.Position < Br.BaseStream.Length
                            Dim b As Byte = Br.ReadByte()
                            If b = 0 Then Exit While ' null terminator
                            bytes.Add(b)
                        End While

                        If bytes.Count = 0 Then
                            StringTextBoxList.Add("\n")
                        Else
                            StringTextBoxList.Add(Encoding.GetEncoding(932).GetString(bytes.ToArray()))
                        End If
                    End If
                Next

                Dim ExportFile = ""

                'Export to PO
                Directory.CreateDirectory("Script_Export")
                Dim folderName As String = New DirectoryInfo(Path.GetDirectoryName(selectedFile)).Name
                Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(selectedFile)
                Dim poFile As String = folderName & "_" & fileNameWithoutExt & ".po"
                If File.Exists($"Script_Export/{poFile}") Then File.Delete($"Script_Export/{poFile}")
                Dim FormattedString As String = ""
                Dim OutputFormattedStrings As New List(Of String)
                For Each ST In StringTextBoxList
                    If ST = "\n" Then
                        OutputFormattedStrings.Add(FormattedString)
                        FormattedString = ""
                    Else
                        FormattedString += ST & "\n"
                    End If
                Next
                Dim OutPutPointerString = ""
                Dim Count = 0
                For Each Point In PointerPOSList
                    If Point = 0 Then
                        WriteToPOFile(poFile, OutputFormattedStrings(Count), OutPutPointerString)
                        OutPutPointerString = ""
                        Count += 1
                    Else
                        OutPutPointerString += Point.ToString & ","
                    End If
                Next
            End Using
        End If
    End Sub

    Public Function WriteToPOFile(pofile, stringtowrite, PointerPOSs)
        Using sw As StreamWriter = New StreamWriter(File.Open($"Script_Export/{pofile}", FileMode.Append))
            sw.WriteLine($"# POS:{PointerPOSs}")
            sw.WriteLine($"msgid ""{stringtowrite}""")
            sw.WriteLine("msgstr """"")
            sw.WriteLine("")
        End Using
    End Function
    Public Function ReadUntilPattern(br As BinaryReader) As Byte()
        ' Define the patterns to search for
        Dim patterns As Byte()() = {
        New Byte() {&H0, &H1D},
        New Byte() {&H0, &H1B},
        New Byte() {&H0, &H1C},
        New Byte() {&H54, &H1B},
        New Byte() {&H54, &H3},
        New Byte() {&H54, &H35}
    }

        ' Store read bytes
        Dim buffer As New List(Of Byte)
        Dim foundPattern As Boolean = False

        While br.BaseStream.Position < br.BaseStream.Length
            ' Read the next byte
            Dim b As Byte = br.ReadByte()
            buffer.Add(b)

            ' Check if the last two bytes form one of the patterns
            If buffer.Count >= 2 Then
                Dim lastTwo As Byte() = {buffer(buffer.Count - 2), buffer(buffer.Count - 1)}
                For Each pattern In patterns
                    If lastTwo.SequenceEqual(pattern) Then
                        ' Remove the pattern from the buffer
                        buffer.RemoveAt(buffer.Count - 1)
                        buffer.RemoveAt(buffer.Count - 1)
                        foundPattern = True
                        br.BaseStream.Position -= 2
                        Exit For
                    End If
                Next
            End If

            ' If a pattern was found, break the loop
            If foundPattern Then Exit While
        End While

        ' Return the collected bytes as an array
        Return buffer.ToArray()
    End Function
    Function FoundNextScene(br As BinaryReader) As Boolean
        ' Store the initial position
        Dim initialPosition As Long = br.BaseStream.Position

        ' Ensure there are at least 2 more bytes to read
        If br.BaseStream.Length - br.BaseStream.Position < 2 Then
            Return False
        End If

        ' Read the next two bytes
        Dim byte1 As Byte = br.ReadByte()
        Dim byte2 As Byte = br.ReadByte()

        ' Check if they match either 0x54 0x1B or 0x00 0x1B
        If (byte1 = &H54 AndAlso byte2 = &H1B) OrElse (byte1 = &H0 AndAlso byte2 = &H1B) Then
            br.BaseStream.Position = initialPosition
            Return True
        End If

        ' If neither match, reset position
        br.BaseStream.Position = initialPosition
        Return False
    End Function
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    End Sub
End Class
