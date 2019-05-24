Public Class IQ_P0002
    Dim Ticket_Response As String
    Public Sub New()

        ' Llamada necesaria para el diseñador.
        InitializeComponent()
        Me.Label2.Text = ""
        Me.Label2.Visible = False
        Me.Label1.Text = "NUEVO TICKET ASIGNADO: " & IQ_P0001.LblTicket.Text
    End Sub

    Private Sub ButtonAtender_Click(sender As Object, e As EventArgs) Handles ButtonAtender.Click
        Me.Label2.Text = "A"
        Me.Dispose()
    End Sub

    Private Sub ButtonNonShow_Click(sender As Object, e As EventArgs)
        Me.Label2.Text = "N"
        Me.Dispose()
    End Sub
End Class