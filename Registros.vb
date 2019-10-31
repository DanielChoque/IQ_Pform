Public Class Registros
    Dim dt As New DataTable
    Dim inte As New Integer
    Dim i As Integer
    Dim Cnn_Central_Server2 As String

    Public Sub recarga(Cnn_Central_Server As String, Computer_Code As String)
        Cnn_Central_Server2 = Cnn_Central_Server

        ' MsgBox("" & Computer_Code & " : " & Format(Date.Now, "dd/MM/yyyy"))
        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O2.Open()

        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("SELECT * FROM IQ_Tickets WHERE IQ_Tickets.IQTicket_Emision >= '" & Format(Date.Now, "dd/MM/yyyy") & " 00:00:00' AND  IQ_Tickets.IQTicket_Emision <= '" & Format(Date.Now, "dd/MM/yyyy") & " 23:59:59' AND IQ_Tickets.IQTicket_Punto = '" & Computer_Code & "'  ORDER BY IQ_Tickets.IQTicket_Emision", Carga_Coneccion_O2)
        Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)

        dt.Columns.Clear()

        dt.Columns.Add("Fecha", GetType(String))
        dt.Columns.Add("Nro Ticket", GetType(String))
        dt.Columns.Add("Descripción", GetType(String))
        dt.Columns.Add("NIT", GetType(String))
        dt.Columns.Add("Nombre", GetType(String))
        dt.Columns.Add("Codigo Punto", GetType(String))
        i = -35
        grid.DataSource = dt
        grid.Columns(0).Width = 200 + i + i
        grid.Columns(1).Width = 100 + i
        grid.Columns(2).Width = 200 + i + i
        grid.Columns(3).Width = 200 + i + i
        dt.Rows.Clear()
        While Carga_Reader_O2.Read
            dt.Rows.Add(Carga_Reader_O2.GetValue(5), Carga_Reader_O2.GetValue(1), Carga_Reader_O2.GetValue(13), Carga_Reader_O2.GetValue(15), Carga_Reader_O2.GetValue(16), Carga_Reader_O2.GetValue(11))
        End While
        Carga_Coneccion_O2.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        ' MsgBox("" & Computer_Code & " : " & Format(Date.Now, "dd/MM/yyyy"))
        'Dim s As String

        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server2)
        Carga_Coneccion_O2.Open()

        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("", Carga_Coneccion_O2)
        Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)


        Carga_Coneccion_O2.Close()
    End Sub
End Class