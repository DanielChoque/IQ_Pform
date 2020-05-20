Public Class Registros
    Dim dt As New DataTable
    Dim inte As New Integer
    Dim i As Integer
    Dim Cnn_Central_Server2 As String
    Dim Computer_Code2 As String

    Public Sub recarga(Cnn_Central_Server As String, Computer_Code As String)
        Cnn_Central_Server2 = Cnn_Central_Server
        Computer_Code2 = Computer_Code
        'Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
        'Carga_Coneccion_O2.Open()

        'Dim Carga_Comando_O2 As New OleDb.OleDbCommand("SELECT * FROM IQ_Tickets WHERE IQ_Tickets.IQTicket_Emision >= '" & Format(Me.DateDesde.Value, "dd/MM/yyyy") & " 00:00:00' AND  IQ_Tickets.IQTicket_Emision <= '" & Format(Me.DateHasta.Value, "dd/MM/yyyy") & " 23:59:59' AND IQ_Tickets.IQTicket_Punto = '" & Computer_Code & "'  ORDER BY IQ_Tickets.IQTicket_Emision", Carga_Coneccion_O2)
        'Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)

        'dt.Columns.Clear()

        'dt.Columns.Add("Fecha", GetType(String))
        'dt.Columns.Add("Nro Ticket", GetType(String))
        'dt.Columns.Add("Descripción", GetType(String))
        'dt.Columns.Add("NIT", GetType(String))
        'dt.Columns.Add("Nombre", GetType(String))
        'dt.Columns.Add("Codigo Punto", GetType(String))
        'i = -35
        'grid.DataSource = dt
        'grid.Columns(0).Width = 200 + i + i
        'grid.Columns(1).Width = 100 + i
        'grid.Columns(2).Width = 200 + i + i
        'grid.Columns(3).Width = 200 + i + i
        'dt.Rows.Clear()
        'Dim da = Carga_Reader_O2.FieldCount

        'While Carga_Reader_O2.Read
        '    dt.Rows.Add(Carga_Reader_O2.GetValue(5), Carga_Reader_O2.GetValue(1), Carga_Reader_O2.GetValue(13), Carga_Reader_O2.GetValue(16), Carga_Reader_O2.GetValue(15), Carga_Reader_O2.GetValue(11))
        'End While
        'Carga_Coneccion_O2.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)


        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server2)
        Carga_Coneccion_O2.Open()

        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("", Carga_Coneccion_O2)
        Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)


        Carga_Coneccion_O2.Close()
    End Sub

    Private Sub btnCall_Click(sender As Object, e As EventArgs) Handles btnLLamada.Click
        recargaCall(Cnn_Central_Server2, Computer_Code2)
        btnTicket.Enabled = True
        btnLLamada.Enabled = False
    End Sub

    Private Sub btnTicket_Click(sender As Object, e As EventArgs) Handles btnTicket.Click
        btnTicket.Enabled = False
        btnLLamada.Enabled = True
        recargaTicket(Cnn_Central_Server2, Computer_Code2)
    End Sub


    Public Sub recargaTicket(Cnn_Central_Server As String, Computer_Code As String)
        Cnn_Central_Server2 = Cnn_Central_Server
        Computer_Code2 = Computer_Code
        Dim fFecha As String
        fFecha = GetSetting("I-Queue", "Appl", "ServerCollation", "")
        Select Case fFecha
            Case "ymd"
                fFecha = "yyyy/MM/dd"
            Case "ydm"
                fFecha = "yyyy/dd/MM"
            Case "mdy"
                fFecha = "MM/dd/yyyy"
            Case "dmy"
                fFecha = "dd/MM/yyyy"
        End Select
        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O2.Open()
        'Dim Carga_Comando_O2 As New OleDb.OleDbCommand("SELECT * FROM IQ_Tickets WHERE IQ_Tickets.IQTicket_Emision >= '" & Format(Me.DateDesde.Value, "dd/MM/yyyy") & " 00:00:00' AND  IQ_Tickets.IQTicket_Emision <= '" & Format(Me.DateHasta.Value, "dd/MM/yyyy") & " 23:59:59' AND IQ_Tickets.IQTicket_Punto = '" & Computer_Code & "'  ORDER BY IQ_Tickets.IQTicket_Emision", Carga_Coneccion_O2)
        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("SELECT * FROM IQ_Tickets WHERE IQ_Tickets.IQTicket_Emision >= '" & Format(Me.DateDesde.Value, fFecha) & " 00:00:00' AND  IQ_Tickets.IQTicket_Emision <= '" & Format(Me.DateHasta.Value, fFecha) & " 23:59:59' AND IQ_Tickets.IQTicket_Punto = '" & Computer_Code & "'  ORDER BY IQ_Tickets.IQTicket_Emision", Carga_Coneccion_O2)
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
        grid.Columns(3).Width = 100 + i
        grid.Columns(4).Width = 200 + i
        grid.Columns(5).Width = 200 + i
        dt.Rows.Clear()
        Dim da = Carga_Reader_O2.FieldCount

        While Carga_Reader_O2.Read
            dt.Rows.Add(Carga_Reader_O2.GetValue(5), Carga_Reader_O2.GetValue(1), Carga_Reader_O2.GetValue(13), Carga_Reader_O2.GetValue(16), Carga_Reader_O2.GetValue(15), Carga_Reader_O2.GetValue(11))
        End While
        Carga_Coneccion_O2.Close()
    End Sub
    Public Sub recargaCall(Cnn_Central_Server As String, Computer_Code As String)
        Cnn_Central_Server2 = Cnn_Central_Server
        Computer_Code2 = Computer_Code
        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim fFecha As String
        fFecha = GetSetting("I-Queue", "Appl", "ServerCollation", "")
        Select Case fFecha
            Case "ymd"
                fFecha = "yyyy/MM/dd"
            Case "ydm"
                fFecha = "yyyy/dd/MM"
            Case "mdy"
                fFecha = "MM/dd/yyyy"
            Case "dmy"
                fFecha = "dd/MM/yyyy"
        End Select
        Carga_Coneccion_O2.Open()
        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("SELECT * FROM IQ_Ausencias WHERE IQ_Ausencias.IQAusencias_Justificativo LIKE '6c7afada99e4%' AND IQ_Ausencias.IQAusencias_Fecha >='" & Format(Me.DateDesde.Value, fFecha) & " 00:00:00' AND IQ_Ausencias.IQAusencias_Fecha <= '" & Format(Me.DateHasta.Value, fFecha) & " 23:59:59' AND IQ_Ausencias.IQAusencias_Punto ='" & Computer_Code & "' ORDER BY IQ_Ausencias.IQAusencias_Fecha", Carga_Coneccion_O2)
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
        grid.Columns(3).Width = 100 + i
        grid.Columns(4).Width = 200 + i
        grid.Columns(5).Width = 200 + i
        dt.Rows.Clear()
        Dim da = Carga_Reader_O2.FieldCount
        Dim numC As Integer
        Dim LineOfText As String
        Dim ii As Integer
        Dim aryTextFile() As String
        Dim nitCi As String
        Dim razonNit As String
        numC = 0
        nitCi = ""
        razonNit = ""
        While Carga_Reader_O2.Read
            numC += 1

            LineOfText = Carga_Reader_O2.GetValue(3)
            aryTextFile = LineOfText.Split("|")
            If UBound(aryTextFile) > 0 Then
                For ii = 0 To UBound(aryTextFile)
                    'MessageBox.Show(aryTextFile(i))
                    If ii = 1 Then
                        nitCi = aryTextFile(1)
                    End If
                    If ii = 2 Then
                        razonNit = aryTextFile(2)
                    End If
                Next ii
            End If
            dt.Rows.Add(Carga_Reader_O2.GetValue(2), "CALL-" & numC, "Llamada Atendida", nitCi, razonNit, Carga_Reader_O2.GetValue(0))
        End While
        Carga_Coneccion_O2.Close()
    End Sub
End Class