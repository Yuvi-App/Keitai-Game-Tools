<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        bntExtractSP = New Button()
        OpenFileDialog1 = New OpenFileDialog()
        btnExtractARCSingle = New Button()
        Label1 = New Label()
        btnExtractARCBatch = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        Label2 = New Label()
        btnRepackARCSingle = New Button()
        btnExtractText = New Button()
        SuspendLayout()
        ' 
        ' bntExtractSP
        ' 
        bntExtractSP.Location = New Point(12, 12)
        bntExtractSP.Name = "bntExtractSP"
        bntExtractSP.Size = New Size(347, 29)
        bntExtractSP.TabIndex = 0
        bntExtractSP.Text = "Extract SP"
        bntExtractSP.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog1
        ' 
        OpenFileDialog1.FileName = "OpenFileDialog1"
        ' 
        ' btnExtractARCSingle
        ' 
        btnExtractARCSingle.Location = New Point(12, 85)
        btnExtractARCSingle.Name = "btnExtractARCSingle"
        btnExtractARCSingle.Size = New Size(165, 29)
        btnExtractARCSingle.TabIndex = 1
        btnExtractARCSingle.Text = "Single"
        btnExtractARCSingle.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.FlatStyle = FlatStyle.Flat
        Label1.Font = New Font("Segoe UI", 9F, FontStyle.Underline)
        Label1.Location = New Point(12, 57)
        Label1.Name = "Label1"
        Label1.Size = New Size(347, 25)
        Label1.TabIndex = 2
        Label1.Text = "Extract ARC"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' btnExtractARCBatch
        ' 
        btnExtractARCBatch.Location = New Point(193, 85)
        btnExtractARCBatch.Name = "btnExtractARCBatch"
        btnExtractARCBatch.Size = New Size(166, 29)
        btnExtractARCBatch.TabIndex = 3
        btnExtractARCBatch.Text = "Batch"
        btnExtractARCBatch.UseVisualStyleBackColor = True
        ' 
        ' Label2
        ' 
        Label2.FlatStyle = FlatStyle.Flat
        Label2.Font = New Font("Segoe UI", 9F, FontStyle.Underline)
        Label2.Location = New Point(12, 274)
        Label2.Name = "Label2"
        Label2.Size = New Size(347, 25)
        Label2.TabIndex = 4
        Label2.Text = "Repack ARC"
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' btnRepackARCSingle
        ' 
        btnRepackARCSingle.Location = New Point(12, 302)
        btnRepackARCSingle.Name = "btnRepackARCSingle"
        btnRepackARCSingle.Size = New Size(165, 29)
        btnRepackARCSingle.TabIndex = 5
        btnRepackARCSingle.Text = "Single"
        btnRepackARCSingle.UseVisualStyleBackColor = True
        ' 
        ' btnExtractText
        ' 
        btnExtractText.Location = New Point(12, 160)
        btnExtractText.Name = "btnExtractText"
        btnExtractText.Size = New Size(347, 29)
        btnExtractText.TabIndex = 6
        btnExtractText.Text = "Extract Text"
        btnExtractText.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(371, 450)
        Controls.Add(btnExtractText)
        Controls.Add(btnRepackARCSingle)
        Controls.Add(Label2)
        Controls.Add(btnExtractARCBatch)
        Controls.Add(Label1)
        Controls.Add(btnExtractARCSingle)
        Controls.Add(bntExtractSP)
        Name = "Form1"
        Text = "Xenosaga Pied Piper Toolkit"
        ResumeLayout(False)
    End Sub

    Friend WithEvents bntExtractSP As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents btnExtractARCSingle As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents btnExtractARCBatch As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents Label2 As Label
    Friend WithEvents btnRepackARCSingle As Button
    Friend WithEvents btnExtractText As Button

End Class
