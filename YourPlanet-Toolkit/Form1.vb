Imports System.Data.Common
Imports System.Diagnostics.Eventing.Reader
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Threading

Public Class Form1

    Shared JARPATH = ""
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim SelectedSP As String
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            SelectedSP = OpenFileDialog1.FileName
        Else
            Exit Sub
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
                    File.WriteAllBytes(filePath, bytes)


                    If i = 17 Then '17 is another packed archive file
                        Dim extractDir As String = Path.Combine(ExportDir, "Extracted")
                        Dim zipExtractPath As String = Path.Combine(extractDir, $"Extracted_{i}")
                        Directory.CreateDirectory(zipExtractPath)
                        Using br2 As BinaryReader = New BinaryReader(File.Open(filePath, FileMode.Open))
                            Dim StartPOS17 As UInt32 = &H0
                            Dim StartData17 As UInt32 = &H54
                            For n As Integer = 1 To 21
                                br2.BaseStream.Position = StartPOS17
                                Dim FileLength17 As UInt32 = ReadUInt32BigEndian(br2)
                                StartPOS17 = br2.BaseStream.Position
                                br2.BaseStream.Position = StartData17
                                'Extract Data
                                Dim filePath17 As String = Path.Combine(zipExtractPath, $"{n}.bin")
                                Dim bytes17 As Byte() = br2.ReadBytes(FileLength17)
                                File.WriteAllBytes(filePath17, bytes17)
                                ' Check if the file is a ZIP (starts with "PK")
                                If bytes17.Length >= 2 AndAlso bytes17(0) = &H50 AndAlso bytes17(1) = &H4B Then
                                    ' Define the extraction path using the original file name
                                    Dim zipExtractPath17 As String = Path.Combine(zipExtractPath, $"Extracted_{n}")

                                    ' Extract ZIP contents
                                    ExtractZip(filePath17, zipExtractPath17)
                                End If

                                StartData17 = br2.BaseStream.Position
                                br2.BaseStream.Position = StartPOS17
                            Next
                        End Using
                    Else
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

    Private Sub btnCreateITEM_Click(sender As Object, e As EventArgs) Handles btnCreateITEM.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            WriteTextLinesToBinary(OpenFileDialog1.FileName, "built/14item_name.dat")
        End If
    End Sub
    Private Sub btnCreateTeki_Click(sender As Object, e As EventArgs) Handles btnCreateTeki.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            WriteTextLinesToBinary(OpenFileDialog1.FileName, "built/27teki_name.dat")
        End If
    End Sub
    Private Sub btnCreateWaza_Click(sender As Object, e As EventArgs) Handles btnCreateWaza.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            WriteTextLinesToBinary(OpenFileDialog1.FileName, "built/12waza_name.dat")
        End If
    End Sub

    'Text Extract
    'Each Textblock is 90 bytes long, and each block is a msgblock id incremented.
    'Each line in the textblock must be 30 bytes long, padd with 00 if not long enough till 30 bytes minimum
    Public Function CreateNewScriptFile(InputTranslatedTextFile As String)
        Dim Outputfile = $"built/{Path.GetFileNameWithoutExtension(InputTranslatedTextFile)}.dat"
        Using bw As BinaryWriter = New BinaryWriter(File.Open(Outputfile, FileMode.Create))
            Dim ByteCount As Integer = 0
            For Each f In File.ReadAllLines(InputTranslatedTextFile)
                If Not String.IsNullOrEmpty(f) Then
                    Dim StringToWrite As Byte()
                    If f.ToLower = "{empty}" Then
                        Dim headerSize = 10 * 3
                        StringToWrite = New Byte(headerSize - 1) {}
                    Else
                        StringToWrite = Encoding.GetEncoding(932).GetBytes(f)
                        If StringToWrite.Length > 30 Then
                            MessageBox.Show("The string is too long to fit in 30 bytes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Else
                            ' Pad with 0x00 to make exactly 30 bytes
                            Array.Resize(StringToWrite, 30)
                        End If
                    End If
                    ByteCount += StringToWrite.Length
                    bw.Write(StringToWrite)
                Else
                    If ByteCount <> 90 Then
                        MessageBox.Show($"Something went wrong, as this textbox was not 90 bytes long.")
                        Return False
                    ElseIf ByteCount = 90 Then
                        ByteCount = 0
                    End If
                End If
            Next
        End Using
    End Function


    'Helpers
    Public Sub WriteTextLinesToBinary(inputFilePath As String, outputFilePath As String)
        Const BlockSize As Integer = 16

        Using writer As New BinaryWriter(File.Open(outputFilePath, FileMode.Create))
            For Each line As String In File.ReadLines(inputFilePath)
                ' Convert to bytes
                Dim lineBytes As Byte() = Encoding.ASCII.GetBytes(line)

                ' If longer than 16 bytes, truncate
                If lineBytes.Length > BlockSize Then
                    ReDim Preserve lineBytes(BlockSize - 1)
                End If

                ' Pad with 0x00 if shorter than 16 bytes
                Dim paddedBytes(BlockSize - 1) As Byte
                Array.Copy(lineBytes, paddedBytes, lineBytes.Length)

                ' Write padded block to binary file
                writer.Write(paddedBytes)
            Next
        End Using
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        Directory.CreateDirectory("built")
        Directory.CreateDirectory("built/sp")
        JARPATH = Path.Join(Application.StartupPath(), "jar.exe")
    End Sub
    Private Sub btnCreateSP_Click(sender As Object, e As EventArgs) Handles btnCreateSP.Click
        If FolderBrowserDialog1.ShowDialog() <> DialogResult.OK Then Exit Sub

        Dim selectedFolder = FolderBrowserDialog1.SelectedPath
        Dim outputFolder = Path.Combine("built", "sp")
        Directory.Delete(outputFolder, recursive:=True)
        Directory.CreateDirectory(outputFolder)

        For Each folderPath In Directory.GetDirectories(selectedFolder)
            Dim folderName = Path.GetFileName(folderPath)
            Dim parts = folderName.Split("_"c)
            If parts.Length < 2 Then Continue For

            Dim folderNumber = parts(1)
            Dim dataFilePath = Path.Combine(folderPath, "data.dat")
            Dim binFileName = $"{folderNumber}.bin"
            Dim binFilePath = Path.Combine(outputFolder, binFileName)

            If File.Exists(dataFilePath) Then
                ' Single File Archive
                CreateJAR($"{JARPATH} -cMf {binFileName} *", folderPath)
                Thread.Sleep(300) ' Give time for JAR to complete
                File.Move(Path.Combine(folderPath, binFileName), binFilePath)

            ElseIf File.Exists(Path.Combine(folderPath, binFileName)) Then
                ' Already has .bin
                File.Copy(Path.Combine(folderPath, binFileName), binFilePath, overwrite:=True)

            ElseIf folderNumber = "17" Then
                ' Build special 17.bin
                Dim headerSize = 21 * 4
                Dim tableData As Byte() = New Byte(headerSize - 1) {}
                Dim output17Path = Path.Combine(outputFolder, "17.bin")

                Using bw As New BinaryWriter(File.Open(output17Path, FileMode.Create))
                    bw.Write(tableData)
                    Dim lastTablePos As Long = 0
                    Dim lastDataPos As Long = bw.BaseStream.Position

                    Dim subDirs = Directory.GetDirectories(folderPath) _
                        .Select(Function(d) New With {
                            .Path = d,
                            .Number = Integer.Parse(Path.GetFileName(d).Split("_"c)(1))
                        }) _
                        .OrderBy(Function(x) x.Number) _
                        .Select(Function(x) x.Path) _
                        .ToList()

                    For Each subDir In subDirs
                        Dim folderNum = Path.GetFileName(subDir).Split("_")(1)
                        Dim NewBinFile = $"{folderNum}.bin"
                        Dim NewBinFilePath = Path.Combine(subDir, NewBinFile)
                        Dim subDataPath = Path.Combine(subDir, "data.dat")
                        If File.Exists(subDataPath) Then
                            CreateJAR($"{JARPATH} -cMf {NewBinFile} *", subDir)
                            Thread.Sleep(300)
                            Dim fileBytes = File.ReadAllBytes(NewBinFilePath)
                            Dim fileLength As UInt32 = CUInt(fileBytes.Length)
                            Dim lengthBytes As Byte() = BitConverter.GetBytes(fileLength)
                            If BitConverter.IsLittleEndian Then
                                Array.Reverse(lengthBytes)
                            End If

                            ' Update table entry
                            bw.BaseStream.Position = lastTablePos
                            bw.Write(lengthBytes)
                            lastTablePos = bw.BaseStream.Position

                            ' Write actual data
                            bw.BaseStream.Position = lastDataPos
                            bw.Write(fileBytes)
                            lastDataPos = bw.BaseStream.Position

                            File.Delete(NewBinFilePath)
                        End If
                    Next
                End Using
            Else
                MessageBox.Show($"{folderPath} is missing a file to make a SP")
            End If
        Next

        ' Let Create the Actually SP now
        Using bw As BinaryWriter = New BinaryWriter(File.Open("built/sp/Your_Planet.sp", FileMode.Create))
            Dim fileHeader = File.ReadAllBytes("SP_Header.bin")
            bw.Write(fileHeader)

            Dim binFiles = Directory.GetFiles(outputFolder, "*.bin") _
                .Select(Function(f) New With {
                    .Path = f,
                    .Number = Integer.Parse(Path.GetFileNameWithoutExtension(f))
                }) _
                .OrderBy(Function(f) f.Number) _
                .ToList()

            Dim StartTablePOS = bw.BaseStream.Position
            Dim lastTablePos As Long = bw.BaseStream.Position
            Dim headerSize = 31 * 4
            Dim tableData As Byte() = New Byte(headerSize - 1) {}
            bw.Write(tableData)
            Dim LastDataPOS = bw.BaseStream.Position

            For Each bin In binFiles
                Dim fileBytes As Byte() = File.ReadAllBytes(bin.Path)
                Dim fileLength As UInt32 = CUInt(fileBytes.Length)
                Dim lengthBytes As Byte() = BitConverter.GetBytes(fileLength)
                If BitConverter.IsLittleEndian Then
                    Array.Reverse(lengthBytes)
                End If

                ' Update table entry
                bw.BaseStream.Position = lastTablePos
                bw.Write(lengthBytes)

                lastTablePos = bw.BaseStream.Position

                ' Write actual data
                bw.BaseStream.Position = LastDataPOS
                bw.Write(fileBytes)

                LastDataPOS = bw.BaseStream.Position
            Next


            'Pad to final size
            Dim targetSize As Integer = 409664
            Dim currentSize As Long = bw.BaseStream.Position

            If currentSize < targetSize Then
                Dim padLength As Integer = targetSize - CInt(currentSize)
                Dim padding() As Byte = New Byte(padLength - 1) {} ' Creates a zero-filled array
                bw.Write(padding)
            End If

            ' Go back and Write top PointerTable
            bw.BaseStream.Position = StartTablePOS
            Dim PTBytes(31 * 4 - 1) As Byte
            bw.BaseStream.Read(PTBYtes, 0, PTBYtes.Length)
            bw.BaseStream.Position = 64
            bw.Write(PTBYtes)
        End Using
        MessageBox.Show("Completed Creating SP")
    End Sub
    Public Shared Async Function CreateJAR(InputCMD As String, WD As String) As Task
        Dim psi As New ProcessStartInfo() With {
        .FileName = "cmd.exe",
        .Arguments = "/c " & InputCMD,
        .WorkingDirectory = WD,
        .RedirectStandardOutput = True,
        .RedirectStandardError = True,
        .UseShellExecute = False,
        .CreateNoWindow = True
    }

        Dim process As New Process()
        process.StartInfo = psi
        process.Start()

        Dim outputTask As Task(Of String) = process.StandardOutput.ReadToEndAsync()
        Dim errorTask As Task(Of String) = process.StandardError.ReadToEndAsync()

        Await Task.WhenAll(outputTask, errorTask)

        Await process.WaitForExitAsync()

        Dim output As String = outputTask.Result
        Dim [error] As String = errorTask.Result

        Console.WriteLine("Output: " & output)
        Console.WriteLine("Error: " & [error])
    End Function
    Private Sub btnScriptCreate_Click(sender As Object, e As EventArgs) Handles btnScriptCreate.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            CreateNewScriptFile(OpenFileDialog1.FileName)
        End If
    End Sub
    Private Sub btnExtractTextFromScript_Click(sender As Object, e As EventArgs) Handles btnExtractTextFromScript.Click
        If FolderBrowserDialog1.ShowDialog() <> DialogResult.OK Then Exit Sub
        Dim selectedFolder = FolderBrowserDialog1.SelectedPath
        Dim Exportpath = Path.Combine("Export", "Script17")
        Directory.CreateDirectory(Exportpath)
        For Each fol In Directory.GetDirectories(selectedFolder)
            Dim DataFilePath = Path.Combine(fol, "data.dat")
            If File.Exists(DataFilePath) Then
                Dim inputFilePath As String = DataFilePath
                Dim folderNum = Path.GetFileName(fol).Split("_")(1)
                Dim outputFilePath As String = Path.Combine(Exportpath, folderNum & "_" & Path.GetFileNameWithoutExtension(DataFilePath) & ".txt")
                Dim sjisEncoding As Encoding = Encoding.GetEncoding("shift_jis")

                Using br As New BinaryReader(File.OpenRead(inputFilePath))
                    Using writer As New StreamWriter(outputFilePath, False, Encoding.UTF8)
                        While br.BaseStream.Position < br.BaseStream.Length
                            For i As Integer = 1 To 3 ' 3 blocks per group (90 bytes total)
                                If br.BaseStream.Position + 30 > br.BaseStream.Length Then Exit While

                                Dim blockBytes As Byte() = br.ReadBytes(30)
                                Dim isEmpty = blockBytes.All(Function(b) b = 0)

                                If isEmpty Then
                                    writer.WriteLine("{empty}")
                                Else
                                    ' Trim 0x00 and decode as Shift-JIS
                                    Dim trimmedBytes = blockBytes.TakeWhile(Function(b) b <> 0).ToArray()
                                    Dim decodedString = sjisEncoding.GetString(trimmedBytes)
                                    writer.WriteLine(decodedString)
                                End If
                            Next
                            writer.WriteLine() ' Blank line after 3 blocks
                        End While
                    End Using
                End Using
            End If
        Next
    End Sub
End Class


