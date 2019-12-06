<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Registros
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Registros))
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.btnTicket = New System.Windows.Forms.Button()
        Me.btnLLamada = New System.Windows.Forms.Button()
        Me.DateDesde = New System.Windows.Forms.DateTimePicker()
        Me.DateHasta = New System.Windows.Forms.DateTimePicker()
        Me.lblDesde = New System.Windows.Forms.Label()
        Me.lblHasta = New System.Windows.Forms.Label()
        CType(Me.grid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grid
        '
        Me.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grid.Location = New System.Drawing.Point(12, 56)
        Me.grid.Name = "grid"
        Me.grid.Size = New System.Drawing.Size(736, 329)
        Me.grid.TabIndex = 0
        '
        'btnTicket
        '
        Me.btnTicket.Image = CType(resources.GetObject("btnTicket.Image"), System.Drawing.Image)
        Me.btnTicket.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.btnTicket.Location = New System.Drawing.Point(153, 4)
        Me.btnTicket.Name = "btnTicket"
        Me.btnTicket.Size = New System.Drawing.Size(52, 40)
        Me.btnTicket.TabIndex = 1
        Me.btnTicket.UseVisualStyleBackColor = True
        '
        'btnLLamada
        '
        Me.btnLLamada.Image = CType(resources.GetObject("btnLLamada.Image"), System.Drawing.Image)
        Me.btnLLamada.Location = New System.Drawing.Point(297, 4)
        Me.btnLLamada.Name = "btnLLamada"
        Me.btnLLamada.Size = New System.Drawing.Size(40, 40)
        Me.btnLLamada.TabIndex = 2
        Me.btnLLamada.UseVisualStyleBackColor = True
        '
        'DateDesde
        '
        Me.DateDesde.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.DateDesde.Location = New System.Drawing.Point(53, 4)
        Me.DateDesde.Name = "DateDesde"
        Me.DateDesde.Size = New System.Drawing.Size(78, 20)
        Me.DateDesde.TabIndex = 3
        '
        'DateHasta
        '
        Me.DateHasta.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.DateHasta.Location = New System.Drawing.Point(53, 28)
        Me.DateHasta.Name = "DateHasta"
        Me.DateHasta.Size = New System.Drawing.Size(78, 20)
        Me.DateHasta.TabIndex = 4
        '
        'lblDesde
        '
        Me.lblDesde.AutoSize = True
        Me.lblDesde.Location = New System.Drawing.Point(13, 4)
        Me.lblDesde.Name = "lblDesde"
        Me.lblDesde.Size = New System.Drawing.Size(38, 13)
        Me.lblDesde.TabIndex = 5
        Me.lblDesde.Text = "Desde"
        '
        'lblHasta
        '
        Me.lblHasta.AutoSize = True
        Me.lblHasta.Location = New System.Drawing.Point(16, 28)
        Me.lblHasta.Name = "lblHasta"
        Me.lblHasta.Size = New System.Drawing.Size(35, 13)
        Me.lblHasta.TabIndex = 6
        Me.lblHasta.Text = "Hasta"
        '
        'Registros
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(769, 397)
        Me.Controls.Add(Me.lblHasta)
        Me.Controls.Add(Me.lblDesde)
        Me.Controls.Add(Me.DateHasta)
        Me.Controls.Add(Me.DateDesde)
        Me.Controls.Add(Me.btnLLamada)
        Me.Controls.Add(Me.btnTicket)
        Me.Controls.Add(Me.grid)
        Me.Name = "Registros"
        Me.Text = "Registros"
        CType(Me.grid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents grid As System.Windows.Forms.DataGridView
    Friend WithEvents btnTicket As System.Windows.Forms.Button
    Friend WithEvents btnLLamada As System.Windows.Forms.Button
    Friend WithEvents DateDesde As System.Windows.Forms.DateTimePicker
    Friend WithEvents DateHasta As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblDesde As System.Windows.Forms.Label
    Friend WithEvents lblHasta As System.Windows.Forms.Label
End Class
