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
        Button1 = New Button()
        OpenFileDialog1 = New OpenFileDialog()
        btnCreateITEM = New Button()
        Label1 = New Label()
        btnCreateTeki = New Button()
        btnCreateWaza = New Button()
        btnCreateSP = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        btnScriptCreate = New Button()
        btnExtractTextFromScript = New Button()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(12, 12)
        Button1.Name = "Button1"
        Button1.Size = New Size(94, 29)
        Button1.TabIndex = 0
        Button1.Text = "Extract SP"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog1
        ' 
        OpenFileDialog1.FileName = "OpenFileDialog1"
        ' 
        ' btnCreateITEM
        ' 
        btnCreateITEM.Location = New Point(6, 237)
        btnCreateITEM.Name = "btnCreateITEM"
        btnCreateITEM.Size = New Size(94, 29)
        btnCreateITEM.TabIndex = 1
        btnCreateITEM.Text = "Item_Name"
        btnCreateITEM.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(8, 207)
        Label1.Name = "Label1"
        Label1.Size = New Size(98, 20)
        Label1.TabIndex = 2
        Label1.Text = "CREATE FILES"
        ' 
        ' btnCreateTeki
        ' 
        btnCreateTeki.Location = New Point(106, 237)
        btnCreateTeki.Name = "btnCreateTeki"
        btnCreateTeki.Size = New Size(94, 29)
        btnCreateTeki.TabIndex = 3
        btnCreateTeki.Text = "Teki_Name"
        btnCreateTeki.UseVisualStyleBackColor = True
        ' 
        ' btnCreateWaza
        ' 
        btnCreateWaza.Location = New Point(206, 237)
        btnCreateWaza.Name = "btnCreateWaza"
        btnCreateWaza.Size = New Size(99, 29)
        btnCreateWaza.TabIndex = 4
        btnCreateWaza.Text = "Waza_Name"
        btnCreateWaza.UseVisualStyleBackColor = True
        ' 
        ' btnCreateSP
        ' 
        btnCreateSP.Location = New Point(6, 282)
        btnCreateSP.Name = "btnCreateSP"
        btnCreateSP.Size = New Size(94, 29)
        btnCreateSP.TabIndex = 5
        btnCreateSP.Text = "Create SP"
        btnCreateSP.UseVisualStyleBackColor = True
        ' 
        ' btnScriptCreate
        ' 
        btnScriptCreate.Location = New Point(418, 237)
        btnScriptCreate.Name = "btnScriptCreate"
        btnScriptCreate.Size = New Size(177, 29)
        btnScriptCreate.TabIndex = 6
        btnScriptCreate.Text = "ScriptFileCreate"
        btnScriptCreate.UseVisualStyleBackColor = True
        ' 
        ' btnExtractTextFromScript
        ' 
        btnExtractTextFromScript.Location = New Point(14, 47)
        btnExtractTextFromScript.Name = "btnExtractTextFromScript"
        btnExtractTextFromScript.Size = New Size(199, 29)
        btnExtractTextFromScript.TabIndex = 7
        btnExtractTextFromScript.Text = "Extract Script 17 Text"
        btnExtractTextFromScript.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnExtractTextFromScript)
        Controls.Add(btnScriptCreate)
        Controls.Add(btnCreateSP)
        Controls.Add(btnCreateWaza)
        Controls.Add(btnCreateTeki)
        Controls.Add(Label1)
        Controls.Add(btnCreateITEM)
        Controls.Add(Button1)
        Name = "Form1"
        Text = "Form1"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents btnCreateITEM As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents btnCreateTeki As Button
    Friend WithEvents btnCreateWaza As Button
    Friend WithEvents btnCreateSP As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents btnScriptCreate As Button
    Friend WithEvents btnExtractTextFromScript As Button

End Class
