Imports System.IO
Imports System.Text
Imports FF7DoCLE_Toolkit.localise

Public Class Form1
    Private Sub btnExtractGIFData_Click(sender As Object, e As EventArgs) Handles btnExtractGIFData.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            Dim SelectedPath = FolderBrowserDialog1.SelectedPath
            ExtractAndCombineGifComments(Directory.GetFiles(SelectedPath, "*.gif"), $"Export/gifdata", $"Export/gifdata/combined.zip")
            'CombineBinFiles("Export/gifdata/", "Export/gifdata/Combined.bin")
        End If
    End Sub
    Private Sub btnExtractLocalise_Click(sender As Object, e As EventArgs) Handles btnExtractLocalise.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            localise.LoadlocaliseFile(OpenFileDialog1.FileName)
        End If
    End Sub
    Public Sub ExtractAndCombineGifComments(gifPaths As String(), outputFolder As String, combinedOutputPath As String)
        Dim combinedOutput As New List(Of Byte)
        Dim fileIndex As Integer = 0

        Directory.CreateDirectory(outputFolder)

        For Each gifPath In gifPaths
            Dim gifBytes As Byte() = File.ReadAllBytes(gifPath)
            Dim commentData As New List(Of Byte)
            Dim i As Integer = 0

            While i < gifBytes.Length - 1
                If gifBytes(i) = &H21 AndAlso gifBytes(i + 1) = &HFE Then
                    i += 2

                    ' Read comment sub-blocks
                    While i < gifBytes.Length
                        Dim blockLen As Byte = gifBytes(i)
                        i += 1
                        If blockLen = 0 Then Exit While
                        If i + blockLen <= gifBytes.Length Then
                            commentData.AddRange(gifBytes.Skip(i).Take(blockLen))
                            i += blockLen
                        Else
                            Console.WriteLine($"Invalid block size in {gifPath}")
                            Exit While
                        End If
                    End While
                Else
                    i += 1
                End If
            End While

            ' Save individual comment file
            Dim fileNameOnly = Path.GetFileNameWithoutExtension(gifPath)
            Dim individualOutputPath = Path.Combine(outputFolder, fileNameOnly & ".bin")
            File.WriteAllBytes(individualOutputPath, commentData.ToArray())
            Console.WriteLine($"Saved comment data to: {individualOutputPath}")

            ' Add to combined output (strip 6 bytes from all but first)
            If fileIndex = 0 Then
                combinedOutput.AddRange(commentData)
            ElseIf commentData.Count > 6 Then
                combinedOutput.AddRange(commentData.Skip(6))
            End If

            fileIndex += 1
        Next

        ' Save combined file
        File.WriteAllBytes(combinedOutputPath, combinedOutput.ToArray())
        Console.WriteLine($"Combined comment data saved to: {combinedOutputPath}")
    End Sub
    Public Sub CombineBinFiles(inputFolder As String, outputFilePath As String)
        File.Delete(outputFilePath)
        Dim binFiles As String() = Directory.GetFiles(inputFolder, "*.bin")

        If binFiles.Length = 0 Then
            Console.WriteLine("No .bin files found in folder.")
            Return
        End If
        Using outputStream As FileStream = New FileStream(outputFilePath, FileMode.Create, FileAccess.Write)
            For Each binFile In binFiles
                Dim fileBytes As Byte() = File.ReadAllBytes(binFile)
                outputStream.Write(fileBytes, 0, fileBytes.Length)
            Next
        End Using

        Console.WriteLine("Combined " & binFiles.Length & " files into: " & outputFilePath)
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        Directory.CreateDirectory("Export/gifdata")
    End Sub
End Class
