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
        btnExtractGIFData = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        btnExtractLocalise = New Button()
        OpenFileDialog1 = New OpenFileDialog()
        SuspendLayout()
        ' 
        ' btnExtractGIFData
        ' 
        btnExtractGIFData.Location = New Point(33, 54)
        btnExtractGIFData.Name = "btnExtractGIFData"
        btnExtractGIFData.Size = New Size(174, 29)
        btnExtractGIFData.TabIndex = 0
        btnExtractGIFData.Text = "Extract GIF Data"
        btnExtractGIFData.UseVisualStyleBackColor = True
        ' 
        ' btnExtractLocalise
        ' 
        btnExtractLocalise.Location = New Point(33, 98)
        btnExtractLocalise.Name = "btnExtractLocalise"
        btnExtractLocalise.Size = New Size(174, 29)
        btnExtractLocalise.TabIndex = 1
        btnExtractLocalise.Text = "Extract Localise Text"
        btnExtractLocalise.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog1
        ' 
        OpenFileDialog1.FileName = "OpenFileDialog1"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnExtractLocalise)
        Controls.Add(btnExtractGIFData)
        Name = "Form1"
        Text = "Form1"
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnExtractGIFData As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents btnExtractLocalise As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog

End Class
