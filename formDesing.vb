<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Importador
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstVisor = New System.Windows.Forms.ListBox()
        Me.btnImportar = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lstVisor
        '
        Me.lstVisor.FormattingEnabled = True
        Me.lstVisor.HorizontalScrollbar = True
        Me.lstVisor.Location = New System.Drawing.Point(12, 10)
        Me.lstVisor.Name = "lstVisor"
        Me.lstVisor.Size = New System.Drawing.Size(563, 238)
        Me.lstVisor.TabIndex = 2
        '
        'btnImportar
        '
        Me.btnImportar.Location = New System.Drawing.Point(440, 254)
        Me.btnImportar.Name = "btnImportar"
        Me.btnImportar.Size = New System.Drawing.Size(97, 22)
        Me.btnImportar.TabIndex = 3
        Me.btnImportar.Text = "Importar"
        Me.btnImportar.UseVisualStyleBackColor = True
        '
        'Importador
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(587, 288)
        Me.Controls.Add(Me.btnImportar)
        Me.Controls.Add(Me.lstVisor)
        Me.Name = "Importador"
        Me.Text = "Importar Archivos FTP"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstVisor As System.Windows.Forms.ListBox
    Friend WithEvents btnImportar As System.Windows.Forms.Button

End Class