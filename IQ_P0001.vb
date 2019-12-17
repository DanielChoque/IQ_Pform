Imports System.Data.OleDb
Imports System.Threading
' NORMAL

Public Class IQ_P0001
    Dim Computer_Ip As String
    Dim Computer_Ofic As String
    Dim Disco_Appl As String
    Dim Server_Name As String
    Dim Server_Ip As String
    Dim Server_User As String
    Dim Server_Pwd As String
    Dim Server_Collation As String
    Dim Icon_Folder As String
    Dim Cnn_Central_Server As String
    Private DictDestinos As New ColeccionDestinos
    Dim Computer_Code As String
    Dim Computer_Area As String
    Dim Computer_Sigla As String
    Dim Computer_Descrip As String
    Dim Computer_TipoAtt As String
    Dim listaTickets As New System.Windows.Forms.ListBox
    Dim listaDestinos As New System.Windows.Forms.ListBox
    Dim desfase_segundos As Integer
    Dim counter_lista As Integer
    Dim Tipo_Combo As String
    Dim Ticket_Actual As String
    Dim Huellas(100) As String
    Dim Areas_Espera(100) As String
    Dim Area_Ticket As String
    Dim Alt_F4 As Boolean
    Private DsTramites As New DataSet
    Private DbTramites As System.Data.OleDb.OleDbDataAdapter = New System.Data.OleDb.OleDbDataAdapter
    Dim contadorAux As Integer = 0
    Dim arrayValues() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    Dim arrayValuesAux() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    Dim arrayCheck() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    Dim posArray As SByte = 0
    Dim Cnn_Central_Server_1 As String
    Dim toogle As SByte = 0
    Dim toogleCall As SByte = 0
    'Dim indice_primario As Integer
    Public Sub New()

        ' Llamada necesaria para el diseñador.
        InitializeComponent()
        If Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1 Then
            MessageBox.Show("IQ_PForm NO SE PUEDE EJECUTAR MAS DE UNA VEZ AL MISMO TIEMPO", "IQ_Pform", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End If
        Me.TimerEspera.Enabled = False
        Me.TimerEspera.Stop()
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        Me.Timer1.Enabled = True
        Me.Timer1.Start()
    End Sub
    Private Class ColeccionDestinos
        Inherits System.Collections.DictionaryBase

        Public Sub Add_ColeccionDestinos(ByVal Key As String, ByVal Item As String)
            Dictionary.Add(Key, Item)
        End Sub

        Public Function Valor(ByVal Key As String) As String
            Valor = Dictionary.Item(Key)
        End Function
    End Class
    Private Function CodigoDestino(ByVal Codigo As String) As String
        CodigoDestino = ""
        If Me.listaDestinos.Items.Count = 0 Then
            CodigoDestino = ""
        Else
            For Me.counter_lista = 1 To Me.listaDestinos.Items.Count - 1 Step 2
                If Trim(Codigo) = Trim(Me.listaDestinos.Items.Item(Me.counter_lista)) Then
                    CodigoDestino = Me.listaDestinos.Items.Item(Me.counter_lista - 1)
                End If
            Next
        End If
    End Function
    Private Sub IQ_P0001_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Alt_F4 = True Then
            e.Cancel = True
            Alt_F4 = False
            Exit Sub
        End If
        If Computer_Ip <> "" And Computer_Ip <> Nothing Then
            Proceso_Salir()
        End If
    End Sub
    Private Sub Synchronize_Date_Server()
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpGetServerDate"
        CmmCentral.Parameters.Add("Fecha", OleDbType.Date).Direction = ParameterDirection.Output
        Dim Fecha_Sistema As Date
        Dim Fecha_Maquina As Date
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            Try
                Fecha_Sistema = CmmCentral.Parameters("Fecha").Value
            Catch ex As Exception
                Fecha_Sistema = DateTime.Now
                Exit Try
            End Try
            Fecha_Maquina = DateTime.Now
            desfase_segundos = DateDiff(DateInterval.Second, Fecha_Maquina, Fecha_Sistema)
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
    End Sub
    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonLibre.Click, ButtonAusente.Click, ButtonEspera.Click, ButtonRetorno.Click, ButtonRedirect.Click, ButtonNonShow.Click, ButtonSalir.Click, ButtonAtender.Click, BtnBell.Click
        Select Case UCase(sender.name)
            Case "BTNBELL"
                Proceso_Bell("S")
            Case "BUTTONLIBRE"
                Proceso_Libre()
            Case "BUTTONAUSENTE"
                Proceso_Ausente()
            Case "BUTTONESPERA"
                Proceso_Espera()
            Case "BUTTONRETORNO"
                Proceso_Retorno()
            Case "BUTTONREDIRECT"
                Proceso_Derivacion()
            Case "BUTTONNONSHOW"
                Proceso_NonShow()
            Case "BUTTONATENDER"
                Proceso_Atender()
            Case "BUTTONSALIR"
                Proceso_Salir()
        End Select
    End Sub
    Private Sub Proceso_Salir()
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        If Me.PnlPrimario.Visible = True Then
            If Verifica_Tramites2() = False Then
                Exit Sub
            End If
            Graba_Tramites()
        End If
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "X"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        End
    End Sub
    Private Sub Proceso_Bell(Numero As String)
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "B"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text & "|" & Numero
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        Dim Ips_Prov(50) As String
        Dim Indice_Ips As Integer = 0
        Timer1.Stop()
        Timer1.Enabled = False
        Computer_Ip = Nothing
        Computer_Code = Nothing
        Me.LblVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString
        Dim version_ok As Boolean = False
        Do Until version_ok = True
            For indice_version = Len(Me.LblVersion.Text) To 1 Step -1
                If Mid(Me.LblVersion.Text, indice_version, 1) = "." Then
                    Me.LblVersion.Text = Mid(Me.LblVersion.Text, 1, Len(Me.LblVersion.Text) - 1)
                    version_ok = True
                    Exit For
                Else
                    Me.LblVersion.Text = Mid(Me.LblVersion.Text, 1, Len(Me.LblVersion.Text) - 1)
                End If
            Next
        Loop
        Dim strHostName As String = System.Net.Dns.GetHostName()
        Dim iphe As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(strHostName)
        For Each ipheal As System.Net.IPAddress In iphe.AddressList
            If ipheal.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                Ips_Prov(Indice_Ips) = ipheal.ToString()
                Indice_Ips += 1
            End If
        Next
        Indice_Ips = 1
        Disco_Appl = GetSetting("I-Queue", "Appl", "Disco", "D")
        If Disco_Appl Is Nothing Then
            Disco_Appl = "C"
        End If
        Icon_Folder = Disco_Appl & ":\I-Q\Iconos\"
        Server_Name = GetSetting("I-Queue", "Appl", "ServerName", "")
        Server_Ip = GetSetting("I-Queue", "Appl", "ServerIp", "")
        Server_User = GetSetting("I-Queue", "Appl", "ServerUser", "")
        Server_Pwd = GetSetting("I-Queue", "Appl", "ServerPwd", "")
        Server_Collation = GetSetting("I-Queue", "Appl", "ServerCollation", "ymd")
        If Server_Name = "" Or Server_Ip = "" Or Server_User = "" Then
            MessageBox.Show("EQUIPO NO CONFIGURADO PARA I-Q", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
        End If
        Computer_Ip = ""
        Dim instruccion As String
        For Indice_Ips = 0 To 49
            If Ips_Prov(Indice_Ips) = Nothing Then
                Exit For
            End If
            Cnn_Central_Server = "Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" & Server_User & ";Password=" & Server_Pwd & ";Data Source=" & Server_Name & ";Initial Catalog=IQData;Use Procedure for Prepare=1;Auto Translate=True;Packet Size=4096;Workstation ID=RIESGO2;Use Encryption for Data=False;Tag with column collation when possible=False"
            instruccion = "Select IQAreas_Oficina, IQ_PuntosAtencion.IqPuntos_Codigo, IQ_PuntosAtencion.IqPuntos_Area, IQ_PuntosAtencion.IqPuntos_Sigla, IQ_PuntosAtencion.IqPuntos_Descripcion, IQ_PuntosAtencion.IqPuntos_TipoAtencion from IQ_PuntosAtencion join IQ_Areas on IqPuntos_Area = IQAreas_Codigo where Iq_PuntosAtencion.IqPuntos_Ip = '" & Ips_Prov(Indice_Ips) & "'"
            Dim Carga_Coneccion_M2b As New OleDb.OleDbConnection(Cnn_Central_Server)
            Carga_Coneccion_M2b.Open()
            Dim Carga_Comando_M2b As New OleDb.OleDbCommand(instruccion, Carga_Coneccion_M2b)
            Dim Carga_Reader_M2b As OleDb.OleDbDataReader = Carga_Comando_M2b.ExecuteReader(CommandBehavior.CloseConnection)
            If Carga_Reader_M2b.HasRows = True Then
                While Carga_Reader_M2b.Read
                    If IsDBNull(Carga_Reader_M2b.GetValue(0)) = False Then
                        Computer_Ip = Ips_Prov(Indice_Ips)
                        Computer_Ofic = Carga_Reader_M2b.GetValue(0)
                        Computer_Code = Carga_Reader_M2b.GetValue(1)
                        Computer_Area = Carga_Reader_M2b.GetValue(2)
                        Computer_Sigla = Carga_Reader_M2b.GetValue(3)
                        Computer_Descrip = Carga_Reader_M2b.GetValue(4)
                        Computer_TipoAtt = Carga_Reader_M2b.GetValue(5)
                    End If
                End While
                Carga_Coneccion_M2b.Dispose()
                If Computer_Ip <> "" Then
                    Exit For
                End If
            End If
        Next
        If Computer_Ip = "" Then
            MessageBox.Show("ESTACION NO CONFIGURADA EN EL SERVIDOR DE I-Q", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End If
        Synchronize_Date_Server()
        Me.LblTicket.Text = ""
        Area_Ticket = ""
        Me.Text = Computer_Descrip & " (" & Computer_Code & ")"
        Me.ComboDestino.Visible = False
        Me.LblDestino.Visible = False
        Me.Rojo.Visible = True
        Me.LblRojo.Visible = True
        Me.Amarillo.Visible = False
        Me.LblAmarillo.Visible = False
        Me.Verde.Visible = False
        Me.BtnBell.Visible = False
        Me.ButtonAtender.Visible = False
        Me.ButtonAusente.Visible = True
        Me.ButtonEspera.Visible = False
        Me.ButtonNonShow.Visible = False
        Me.ButtonRedirect.Visible = False
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = True
        Me.LabelEspera.Visible = False
        Me.LabelNonShow.Visible = False
        Me.LabelRedirect.Visible = False
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.PnlPrimario.Visible = False
        nitDisable()
        Me.PnlSecundario.Visible = False
        Dim cn As System.Data.OleDb.OleDbConnection = New System.Data.OleDb.OleDbConnection(Cnn_Central_Server)
        cn.Open()
        DsTramites.Clear()
        With DbTramites
            Dim SQLStr As String = "Select  * from Iq_TipTram Order by IQTipTram_Codigo"
            .TableMappings.Add("Table", "Iq_TipTram")
            Dim cmd As System.Data.OleDb.OleDbCommand = New System.Data.OleDb.OleDbCommand(SQLStr, cn)
            cmd.CommandType = CommandType.Text
            .SelectCommand = cmd
            .Fill(DsTramites)
            .Dispose()
            cmd.Cancel()
        End With
        cn.Close()
        Me.LstEspera.Items.Clear()
        instruccion = "Select IQEspera_Ticket, IQEspera_Hora, IQEspera_Area from Iq_TktEspera where IQEspera_Punto = '" & Computer_Code & "' order by IqEspera_Hora"
        Dim Carga_Coneccion_O0 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O0.Open()
        Dim Carga_Comando_O0 As New OleDb.OleDbCommand(instruccion, Carga_Coneccion_O0)
        Dim Carga_Reader_O0 As OleDb.OleDbDataReader = Carga_Comando_O0.ExecuteReader(CommandBehavior.CloseConnection)
        While Carga_Reader_O0.Read
            Me.LstEspera.Items.Add(Carga_Reader_O0.GetValue(0) & "(" & Format(Carga_Reader_O0.GetValue(1), "HH:mm:ss") & ")")
            If Me.TimerEspera.Enabled = False Then
                Me.TimerEspera.Enabled = True
                Me.TimerEspera.Start()
            End If
        End While
        Carga_Coneccion_O0.Dispose()
        '     Me.LabelTicketAbajo.Visible = False
        '    Me.LblTicket.Visible = False
        Me.TimerIdle.Enabled = False
        Me.TimerIdle.Stop()
        Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O2.Open()
        Dim Carga_Comando_O2 As New OleDb.OleDbCommand("Select IQTicket_Area, IQTicket_Ticket, IQTIcket_Atencion from IQ_Tickets where IQTicket_Punto = '" & Computer_Code & "' And IQTicket_Estado = 'P' And IQTicket_Fecha = convert(varchar(10), getdate(), 111) Order by IQTicket_Atencion", Carga_Coneccion_O2)
        Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)
        While Carga_Reader_O2.Read
            If IsDBNull(Carga_Reader_O2.GetValue(0)) = False Then
                MessageBox.Show("ESTE PUNTO DE ATENCION TIENE EL TICKET " & Carga_Reader_O2.GetValue(1) & " EN PROCESO DE ATENCION DESDE HORAS " & Format(CDate(Carga_Reader_O2.GetValue(2)), "HH:mm:ss"), Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.Rojo.Visible = False
                Me.LblRojo.Visible = False
                Me.Amarillo.Visible = True
                Me.LblAmarillo.Visible = True
                Me.Verde.Visible = False
                Me.Lblverde.Visible = False
                Me.ButtonAtender.Visible = False
                Me.BtnBell.Visible = False
                Me.ButtonAusente.Visible = True
                Me.TimerIdle.Interval = 2400000
                Me.TimerIdle.Tag = "Atencion"
                Me.TimerIdle.Enabled = True
                Me.TimerIdle.Start()
                Me.TimerSearch.Enabled = False
                Me.TimerSearch.Stop()
                Me.ButtonEspera.Visible = True
                Me.ButtonLibre.Visible = True
                Me.ButtonNonShow.Visible = False
                Me.ButtonRedirect.Visible = True
                Me.ButtonRetorno.Visible = True
                Me.ButtonSalir.Visible = True
                Me.LabelAtender.Visible = False
                Me.LabelAusente.Visible = True
                Me.LabelEspera.Visible = True
                Me.LabelLibre.Visible = True
                Me.LabelNonShow.Visible = False
                Me.LabelRedirect.Visible = True
                Me.LabelRetorno.Visible = True
                'Carga_Tramites(Mid(Carga_Reader_O2.GetValue(1), 1, 3))

                Carga_Tramites("SAC")
                Me.LblTicket.Text = Carga_Reader_O2.GetValue(1)
                Me.LblTicket.Visible = True
                Area_Ticket = Carga_Reader_O2.GetValue(0)
                Me.LabelTicketAbajo.Visible = True
                Me.LabelSalir.Visible = True
                disablePhone()

                Exit Sub
            End If
        End While
        Carga_Coneccion_O2.Dispose()
        Me.TimerSearch.Enabled = True
        Me.TimerSearch.Start()
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        Me.Lblverde.Visible = False
    End Sub
    Private Sub Carga_Tramites(Ticket_Carga As String)
        enableCall()
        Dim indice_primario As Integer = 0
        Dim indice_secundario As Integer = 0
        Me.ChkPrim01.Visible = False
        Me.ChkPrim02.Visible = False
        Me.ChkPrim03.Visible = False
        Me.ChkPrim04.Visible = False
        Me.ChkPrim05.Visible = False
        Me.ChkPrim06.Visible = False
        Me.ChkPrim07.Visible = False
        Me.ChkPrim08.Visible = False
        Me.ChkPrim09.Visible = False
        Me.ChkPrim10.Visible = False
        Me.ChkPrim11.Visible = False
        Me.ChkPrim12.Visible = False
        Me.ChkPrim13.Visible = False
        Me.ChkPrim14.Visible = False
        Me.ChkPrim15.Visible = False
        Me.ChkPrim16.Visible = False
        Me.ChkSec01.Visible = False
        Me.ChkSec02.Visible = False
        Me.ChkSec03.Visible = False
        Me.ChkSec04.Visible = False
        Me.ChkSec05.Visible = False
        Me.ChkSec06.Visible = False
        Me.ChkSec07.Visible = False
        Me.ChkSec08.Visible = False
        Me.ChkSec09.Visible = False
        Me.ChkSec10.Visible = False
        Me.ChkSec11.Visible = False
        Me.ChkSec12.Visible = False
        Me.ChkSec13.Visible = False
        Me.ChkSec14.Visible = False
        Me.ChkSec15.Visible = False
        Me.ChkSec16.Visible = False
        Me.ChkSec17.Visible = False
        Me.ChkSec18.Visible = False
        Me.ChkSec19.Visible = False
        Me.ChkSec20.Visible = False
        Me.ChkSec21.Visible = False
        Me.ChkSec22.Visible = False
        Me.ChkSec23.Visible = False
        Me.ChkSec24.Visible = False
        Me.ChkSec25.Visible = False
        Me.ChkSec26.Visible = False
        Me.ChkSec27.Visible = False
        Me.ChkSec28.Visible = False
        Me.ChkSec29.Visible = False
        Me.ChkSec30.Visible = False
        Me.ChkSec31.Visible = False
        Me.ChkSec32.Visible = False
        For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
            If DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Ticket") = Ticket_Carga Then
                indice_primario += 1
                Select Case indice_primario
                    Case 1
                        Me.ChkPrim01.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim01.Visible = True
                    Case 2
                        Me.ChkPrim02.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim02.Visible = True
                    Case 3
                        Me.ChkPrim03.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim03.Visible = True
                    Case 4
                        Me.ChkPrim04.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim04.Visible = True
                    Case 5
                        Me.ChkPrim05.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim05.Visible = True
                    Case 6
                        Me.ChkPrim06.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim06.Visible = True
                    Case 7
                        Me.ChkPrim07.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim07.Visible = True
                    Case 8
                        Me.ChkPrim08.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim08.Visible = True
                    Case 9
                        Me.ChkPrim09.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim09.Visible = True
                    Case 10
                        Me.ChkPrim10.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim10.Visible = True
                    Case 11
                        Me.ChkPrim11.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim11.Visible = True
                    Case 12
                        Me.ChkPrim12.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim12.Visible = True
                    Case 13
                        Me.ChkPrim13.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim13.Visible = True
                    Case 14
                        Me.ChkPrim14.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim14.Visible = True
                    Case 15
                        Me.ChkPrim15.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim15.Visible = True
                    Case 16
                        Me.ChkPrim16.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkPrim16.Visible = True
                End Select
            End If
            indice_secundario += 1 + 100
            Select Case indice_secundario
                Case 1
                    Me.ChkSec01.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec01.Visible = True
                Case 2
                    Me.ChkSec02.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec02.Visible = True
                Case 3
                    Me.ChkSec03.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec03.Visible = True
                Case 4
                    Me.ChkSec04.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec04.Visible = True
                Case 5
                    Me.ChkSec05.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec05.Visible = True
                Case 6
                    Me.ChkSec06.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec06.Visible = True
                Case 7
                    Me.ChkSec07.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec07.Visible = True
                Case 8
                    Me.ChkSec08.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec08.Visible = True
                Case 9
                    Me.ChkSec09.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec09.Visible = True
                Case 10
                    Me.ChkSec10.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec10.Visible = True
                Case 11
                    Me.ChkSec11.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec11.Visible = True
                Case 12
                    Me.ChkSec12.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec12.Visible = True
                Case 13
                    Me.ChkSec13.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec13.Visible = True
                Case 14
                    Me.ChkSec14.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec14.Visible = True
                Case 15
                    Me.ChkSec15.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec15.Visible = True
                Case 16
                    Me.ChkSec16.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec16.Visible = True
                Case 17
                    Me.ChkSec17.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec17.Visible = True
                Case 18
                    Me.ChkSec18.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec18.Visible = True
                Case 19
                    Me.ChkSec19.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec19.Visible = True
                Case 20
                    Me.ChkSec20.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec20.Visible = True
                Case 21
                    Me.ChkSec21.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec21.Visible = True
                Case 22
                    Me.ChkSec22.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec22.Visible = True
                Case 23
                    Me.ChkSec23.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec23.Visible = True
                Case 24
                    Me.ChkSec24.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec24.Visible = True
                Case 25
                    Me.ChkSec25.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec25.Visible = True
                Case 26
                    Me.ChkSec26.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec26.Visible = True
                Case 27
                    Me.ChkSec27.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec27.Visible = True
                Case 28
                    Me.ChkSec28.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec28.Visible = True
                Case 29
                    Me.ChkSec29.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec29.Visible = True
                Case 30
                    Me.ChkSec30.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec30.Visible = True
                Case 31
                    Me.ChkSec31.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec31.Visible = True
                Case 32
                    Me.ChkSec32.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                    Me.ChkSec32.Visible = True
            End Select
        Next
        Me.ChkPrim01.Checked = False
        Me.ChkPrim02.Checked = False
        Me.ChkPrim03.Checked = False
        Me.ChkPrim04.Checked = False
        Me.ChkPrim05.Checked = False
        Me.ChkPrim06.Checked = False
        Me.ChkPrim07.Checked = False
        Me.ChkPrim08.Checked = False
        Me.ChkPrim09.Checked = False
        Me.ChkPrim10.Checked = False
        Me.ChkPrim11.Checked = False
        Me.ChkPrim12.Checked = False
        Me.ChkPrim13.Checked = False
        Me.ChkPrim14.Checked = False
        Me.ChkPrim15.Checked = False
        Me.ChkPrim16.Checked = False
        Me.ChkSec01.Checked = False
        Me.ChkSec02.Checked = False
        Me.ChkSec03.Checked = False
        Me.ChkSec04.Checked = False
        Me.ChkSec05.Checked = False
        Me.ChkSec06.Checked = False
        Me.ChkSec07.Checked = False
        Me.ChkSec08.Checked = False
        Me.ChkSec09.Checked = False
        Me.ChkSec10.Checked = False
        Me.ChkSec11.Checked = False
        Me.ChkSec12.Checked = False
        Me.ChkSec13.Checked = False
        Me.ChkSec14.Checked = False
        Me.ChkSec15.Checked = False
        Me.ChkSec16.Checked = False
        Me.ChkSec17.Checked = False
        Me.ChkSec18.Checked = False
        Me.ChkSec19.Checked = False
        Me.ChkSec20.Checked = False
        Me.ChkSec21.Checked = False
        Me.ChkSec22.Checked = False
        Me.ChkSec23.Checked = False
        Me.ChkSec24.Checked = False
        Me.ChkSec25.Checked = False
        Me.ChkSec26.Checked = False
        Me.ChkSec27.Checked = False
        Me.ChkSec28.Checked = False
        Me.ChkSec29.Checked = False
        Me.ChkSec30.Checked = False
        Me.ChkSec31.Checked = False
        Me.ChkSec32.Checked = False
        Me.PnlPrimario.Visible = True
        nitEnable()
        Me.PnlSecundario.Visible = True
        Me.lblNit.Visible = True
        Me.lblName.Visible = True
        Me.txtNit1.Visible = True
        Me.txtName1.Visible = True
        Me.Button1.Visible = True
    End Sub
    Private Sub Graba_Tramites()
        Dim indice_tramites As Integer = 0
        Dim tramite_a_grabar As String = ""
        For indice = 1 To 16
            tramite_a_grabar = ""
            Select Case indice
                Case 1
                    If Me.ChkPrim01.Visible = True Then
                        If Me.ChkPrim01.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim01.Text
                            Exit For
                        End If
                    End If
                Case 2
                    If Me.ChkPrim02.Visible = True Then
                        If Me.ChkPrim02.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim02.Text
                            Exit For
                        End If
                    End If
                Case 3
                    If Me.ChkPrim03.Visible = True Then
                        If Me.ChkPrim03.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim03.Text
                            Exit For
                        End If
                    End If
                Case 4
                    If Me.ChkPrim04.Visible = True Then
                        If Me.ChkPrim04.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim04.Text
                            Exit For
                        End If
                    End If
                Case 5
                    If Me.ChkPrim05.Visible = True Then
                        If Me.ChkPrim05.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim05.Text
                            Exit For
                        End If
                    End If
                Case 6
                    If Me.ChkPrim06.Visible = True Then
                        If Me.ChkPrim06.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim06.Text
                            Exit For
                        End If
                    End If
                Case 7
                    If Me.ChkPrim07.Visible = True Then
                        If Me.ChkPrim07.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim07.Text
                            Exit For
                        End If
                    End If
                Case 8
                    If Me.ChkPrim08.Visible = True Then
                        If Me.ChkPrim08.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim08.Text
                            Exit For
                        End If
                    End If
                Case 9
                    If Me.ChkPrim09.Visible = True Then
                        If Me.ChkPrim09.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim09.Text
                            Exit For
                        End If
                    End If
                Case 10
                    If Me.ChkPrim10.Visible = True Then
                        If Me.ChkPrim10.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim10.Text
                            Exit For
                        End If
                    End If
                Case 11
                    If Me.ChkPrim11.Visible = True Then
                        If Me.ChkPrim11.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim11.Text
                            Exit For
                        End If
                    End If
                Case 12
                    If Me.ChkPrim12.Visible = True Then
                        If Me.ChkPrim12.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim12.Text
                            Exit For
                        End If
                    End If
                Case 13
                    If Me.ChkPrim13.Visible = True Then
                        If Me.ChkPrim13.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim13.Text
                            Exit For
                        End If
                    End If
                Case 14
                    If Me.ChkPrim14.Visible = True Then
                        If Me.ChkPrim14.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim14.Text
                            Exit For
                        End If
                    End If
                Case 15
                    If Me.ChkPrim15.Visible = True Then
                        If Me.ChkPrim15.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim15.Text
                            Exit For
                        End If
                    End If
                Case 16
                    If Me.ChkPrim16.Visible = True Then
                        If Me.ChkPrim16.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim16.Text
                            Exit For
                        End If
                    End If
            End Select
        Next
        Dim instruccion_insert As String
        instruccion_insert = "insert into IQ_TickTram values ("
        instruccion_insert = instruccion_insert & "'" & Area_Ticket & "', "
        instruccion_insert = instruccion_insert & "'" & Me.LblTicket.Text & "', "
        instruccion_insert = instruccion_insert & " '" & Format(DateTime.Today, "yyyy/MM/dd") & "', "
        instruccion_insert = instruccion_insert & CStr(indice_tramites) & ", "
        For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
            If Trim(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")) = Trim(tramite_a_grabar) Then
                instruccion_insert = instruccion_insert & CStr(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Codigo")) & ", "
            End If
        Next
        instruccion_insert = instruccion_insert & " '" & Computer_Code & "')"
        Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Try
            IQ_Cnn.Open()
            Dim IQ_Cmm2 As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
            Dim RegistrosInsertados As Long = IQ_Cmm2.ExecuteNonQuery()
            IQ_Cnn.Close()
        Catch ex As Exception
            IQ_Cnn.Close()
            ' MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Try
        End Try
        For indice = 1 To 32
            tramite_a_grabar = ""
            Select Case indice
                Case 1
                    If Me.ChkSec01.Visible = True Then
                        If Me.ChkSec01.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec01.Text
                        End If
                    End If
                Case 2
                    If Me.ChkSec02.Visible = True Then
                        If Me.ChkSec02.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec02.Text
                        End If
                    End If
                Case 3
                    If Me.ChkSec03.Visible = True Then
                        If Me.ChkSec03.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec03.Text
                        End If
                    End If
                Case 4
                    If Me.ChkSec04.Visible = True Then
                        If Me.ChkSec04.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec04.Text
                        End If
                    End If
                Case 5
                    If Me.ChkSec05.Visible = True Then
                        If Me.ChkSec05.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec05.Text
                        End If
                    End If
                Case 6
                    If Me.ChkSec06.Visible = True Then
                        If Me.ChkSec06.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec06.Text
                        End If
                    End If
                Case 7
                    If Me.ChkSec07.Visible = True Then
                        If Me.ChkSec07.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec07.Text
                        End If
                    End If
                Case 8
                    If Me.ChkSec08.Visible = True Then
                        If Me.ChkSec08.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec08.Text
                        End If
                    End If
                Case 9
                    If Me.ChkSec09.Visible = True Then
                        If Me.ChkSec09.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec09.Text
                        End If
                    End If
                Case 10
                    If Me.ChkSec10.Visible = True Then
                        If Me.ChkSec10.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec10.Text
                        End If
                    End If
                Case 11
                    If Me.ChkSec11.Visible = True Then
                        If Me.ChkSec11.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec11.Text
                        End If
                    End If
                Case 12
                    If Me.ChkSec12.Visible = True Then
                        If Me.ChkSec12.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec12.Text
                        End If
                    End If
                Case 13
                    If Me.ChkSec13.Visible = True Then
                        If Me.ChkSec13.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec13.Text
                        End If
                    End If
                Case 14
                    If Me.ChkSec14.Visible = True Then
                        If Me.ChkSec14.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec14.Text
                        End If
                    End If
                Case 15
                    If Me.ChkSec15.Visible = True Then
                        If Me.ChkSec15.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec15.Text
                        End If
                    End If
                Case 16
                    If Me.ChkSec16.Visible = True Then
                        If Me.ChkSec16.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec16.Text
                        End If
                    End If
                Case 17
                    If Me.ChkSec17.Visible = True Then
                        If Me.ChkSec17.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec17.Text
                        End If
                    End If
                Case 18
                    If Me.ChkSec18.Visible = True Then
                        If Me.ChkSec18.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec18.Text
                        End If
                    End If
                Case 19
                    If Me.ChkSec19.Visible = True Then
                        If Me.ChkSec19.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec19.Text
                        End If
                    End If
                Case 20
                    If Me.ChkSec20.Visible = True Then
                        If Me.ChkSec20.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec20.Text
                        End If
                    End If
                Case 21
                    If Me.ChkSec21.Visible = True Then
                        If Me.ChkSec21.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec21.Text
                        End If
                    End If
                Case 22
                    If Me.ChkSec22.Visible = True Then
                        If Me.ChkSec22.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec22.Text
                        End If
                    End If
                Case 23
                    If Me.ChkSec23.Visible = True Then
                        If Me.ChkSec23.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec23.Text
                        End If
                    End If
                Case 24
                    If Me.ChkSec24.Visible = True Then
                        If Me.ChkSec24.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec24.Text
                        End If
                    End If
                Case 25
                    If Me.ChkSec25.Visible = True Then
                        If Me.ChkSec25.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec25.Text
                        End If
                    End If
                Case 26
                    If Me.ChkSec26.Visible = True Then
                        If Me.ChkSec26.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec26.Text
                        End If
                    End If
                Case 27
                    If Me.ChkSec27.Visible = True Then
                        If Me.ChkSec27.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec27.Text
                        End If
                    End If
                Case 28
                    If Me.ChkSec28.Visible = True Then
                        If Me.ChkSec28.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec28.Text
                        End If
                    End If
                Case 29
                    If Me.ChkSec29.Visible = True Then
                        If Me.ChkSec29.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec29.Text
                        End If
                    End If
                Case 30
                    If Me.ChkSec30.Visible = True Then
                        If Me.ChkSec30.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec30.Text
                        End If
                    End If
                Case 31
                    If Me.ChkSec31.Visible = True Then
                        If Me.ChkSec31.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec31.Text
                        End If
                    End If
                Case 32
                    If Me.ChkSec32.Visible = True Then
                        If Me.ChkSec32.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec32.Text
                        End If
                    End If
            End Select
            If tramite_a_grabar <> "" Then
                instruccion_insert = "insert into IQ_TickTram values ("
                instruccion_insert = instruccion_insert & "'" & Area_Ticket & "', "
                instruccion_insert = instruccion_insert & "'" & Me.LblTicket.Text & "', "
                instruccion_insert = instruccion_insert & " '" & Format(DateTime.Today, "yyyy/MM/dd") & "', "
                instruccion_insert = instruccion_insert & CStr(indice_tramites) & ", "
                For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
                    If Trim(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")) = Trim(tramite_a_grabar) Then
                        instruccion_insert = instruccion_insert & CStr(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Codigo")) & ", "
                    End If
                Next
                instruccion_insert = instruccion_insert & " '" & Computer_Code & "')"
                Try
                    IQ_Cnn.Open()
                    Dim IQ_Cmm2 As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
                    Dim RegistrosInsertados As Long = IQ_Cmm2.ExecuteNonQuery()
                    IQ_Cnn.Close()
                Catch ex As Exception
                    IQ_Cnn.Close()
                    '   MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                    Exit Try
                End Try
            End If
        Next
    End Sub
    Private Sub Proceso_Libre()
        Dim nitName As String
        nitName = ""
        enableCall()
        If Me.PnlPrimario.Visible = True Then
            If Verifica_Tramites2() = False Then
                Exit Sub
            End If
            If toogleCall = 1 Then
                ' Proceso_AusenteLL()
                Graba_Tramites2()
                toogleCall = 0
                nitName = "|6c7afada99e4|" & txtNit1.Text & "|" & txtName1.Text
            Else
                toogleCall = 0
                nitName = ""
                Graba_Tramites()
            End If
            saveNitNameNoMessage()
        End If
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "L"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text & nitName
        'CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text & "|9070532|Daniel choque"
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        If InStr(resultado, "-") > 0 Then
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = True
            Me.LblAmarillo.Visible = True
            Me.Verde.Visible = False
            Me.Lblverde.Visible = False
            Me.LblTicket.Text = Mid(resultado, 1, InStr(resultado, "|") - 1)
            Area_Ticket = Mid(resultado, InStr(resultado, "|") + 1, Len(resultado) - InStr(resultado, "|"))
            Me.LblTicket.Visible = True
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = True
            Me.BtnBell.Visible = True
            Me.ButtonAusente.Visible = False
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = True
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = False
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = True
            Me.LabelAusente.Visible = False
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelNonShow.Visible = True
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = False
            Me.LabelSalir.Visible = True
            Me.TimerIdle.Interval = 120000
            Me.TimerIdle.Tag = "Espera"
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerWait.Enabled = True
            Me.TimerWait.Start()
            Me.TimerSearch.Enabled = False
            Me.TimerSearch.Stop()
            Me.txtName1.Visible = True
            Me.txtNit1.Visible = True
            Me.Button1.Visible = True
            Me.lblNit.Visible = True
            Me.lblName.Visible = True
        Else
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = False
            Me.LblAmarillo.Visible = False
            Me.Verde.Visible = True
            Me.Lblverde.Visible = True
            Me.LblTicket.Text = "LIBRE"
            Area_Ticket = ""
            Me.LblTicket.Visible = True
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = False
            Me.PnlSecundario.Visible = False
            Me.PnlPrimario.Visible = False
            nitDisable()
            Me.BtnBell.Visible = False
            Me.ButtonAusente.Visible = True
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = False
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = True
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = False
            Me.LabelAusente.Visible = True
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelNonShow.Visible = False
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = True
            Me.LabelSalir.Visible = True
            Me.TimerSearch.Enabled = True
            Me.TimerSearch.Start()
            Me.TimerIdle.Enabled = False
            Me.TimerIdle.Stop()
            Me.txtName1.Visible = False
            Me.txtNit1.Visible = False
            Me.Button1.Visible = False
            Me.lblNit.Visible = False
            Me.lblName.Visible = False
        End If
        clear()
    End Sub
    Private Sub Proceso_Espera()
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "E"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        If Me.TimerEspera.Enabled = False Then
            Me.TimerEspera.Enabled = True
            Me.TimerEspera.Start()
        End If
        Me.LstEspera.Items.Add(Me.LblTicket.Text & "(" & Format(DateTime.Now, "HH:mm:ss") & ")")
        If InStr(resultado, "-") > 0 Then
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = True
            Me.LblAmarillo.Visible = True
            Me.Verde.Visible = False
            Me.Lblverde.Visible = False
            Me.LblTicket.Text = Mid(resultado, 1, InStr(resultado, "|") - 1)
            Area_Ticket = Mid(resultado, InStr(resultado, "|") + 1, Len(resultado) - InStr(resultado, "|"))
            Me.LblTicket.Visible = True
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = True
            Me.BtnBell.Visible = True
            Me.ButtonAusente.Visible = False
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = True
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = False
            Me.TimerSearch.Enabled = False
            Me.TimerSearch.Stop()
            Me.TimerIdle.Interval = 120000
            Me.TimerIdle.Tag = "Espera"
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerWait.Enabled = True
            Me.TimerWait.Start()
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = True
            Me.LabelAusente.Visible = False
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LabelNonShow.Visible = True
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = False
            Me.LabelSalir.Visible = True
        Else
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = False
            Me.LblAmarillo.Visible = False
            Me.Verde.Visible = True
            Me.Lblverde.Visible = True
            Me.LblTicket.Text = "LIBRE"
            Me.LblTicket.Visible = True
            Area_Ticket = ""
            Me.TimerSearch.Enabled = True
            Me.TimerSearch.Start()
            Me.TimerIdle.Enabled = False
            Me.TimerIdle.Stop()
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = False
            Me.BtnBell.Visible = False
            Me.ButtonAusente.Visible = True
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = False
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = True
            Me.ButtonSalir.Visible = True
            Me.PnlPrimario.Visible = False
            nitDisable()
            Me.PnlSecundario.Visible = False
            Me.LabelAtender.Visible = False
            Me.LabelAusente.Visible = True
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LabelNonShow.Visible = False
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = True
            Me.LabelSalir.Visible = True
        End If
    End Sub
    Private Sub Proceso_Atender2()
        disablePhone()
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "O"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        Me.Rojo.Visible = False
        Me.LblRojo.Visible = False
        Me.Amarillo.Visible = True
        Me.LblAmarillo.Visible = True
        Me.Verde.Visible = False
        Me.Lblverde.Visible = False
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.ButtonAusente.Visible = True
        Me.TimerIdle.Interval = 2400000
        Me.TimerIdle.Tag = "Atencion"
        Me.TimerWait.Enabled = False
        Me.TimerIdle.Stop()
        Me.TimerIdle.Enabled = True
        Me.TimerIdle.Start()
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        'Carga_Tramites(Mid(Me.LblTicket.Text, 1, 3))
        Carga_Tramites("SAC")
        Me.ButtonEspera.Visible = True
        Me.ButtonLibre.Visible = True
        Me.ButtonNonShow.Visible = False
        Me.ButtonRedirect.Visible = True
        Me.ButtonRetorno.Visible = True
        Me.ButtonSalir.Visible = True
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = True
        Me.LabelEspera.Visible = True
        Me.LabelLibre.Visible = True
        Me.LabelNonShow.Visible = False
        Me.LabelRedirect.Visible = True
        Me.LabelRetorno.Visible = True
        Me.LabelSalir.Visible = True
    End Sub
    Private Sub Proceso_NonShow()
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "N"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        If InStr(resultado, "-") > 0 Then
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = True
            Me.LblAmarillo.Visible = True
            Me.Verde.Visible = False
            Me.Lblverde.Visible = False
            Me.LblTicket.Text = Mid(resultado, 1, InStr(resultado, "|") - 1)
            Area_Ticket = Mid(resultado, InStr(resultado, "|") + 1, Len(resultado) - InStr(resultado, "|"))
            Me.LblTicket.Visible = True
            Me.LabelTicketAbajo.Visible = True
            Me.BtnBell.Visible = True
            Me.ButtonAtender.Visible = True
            Me.ButtonAusente.Visible = False
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = True
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = False
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = True
            Me.LabelAusente.Visible = False
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LabelNonShow.Visible = True
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = False
            Me.TimerWait.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerIdle.Interval = 120000
            Me.TimerIdle.Tag = "Espera"
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerSearch.Enabled = False
            Me.TimerSearch.Stop()
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelSalir.Visible = True
        Else
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = False
            Me.LblAmarillo.Visible = False
            Me.Verde.Visible = True
            Me.Lblverde.Visible = True
            Me.LblTicket.Text = "LIBRE"
            Me.LblTicket.Visible = True
            Area_Ticket = ""
            Me.PnlPrimario.Visible = False
            nitDisable()
            Me.PnlSecundario.Visible = False
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = False
            Me.BtnBell.Visible = False
            Me.ButtonAusente.Visible = True
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = False
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = True
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = False
            Me.LabelAusente.Visible = True
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LabelNonShow.Visible = False
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = True
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelSalir.Visible = True
            Me.TimerIdle.Enabled = False
            Me.TimerIdle.Stop()
            Me.TimerSearch.Enabled = True
            Me.TimerSearch.Start()
        End If
    End Sub
    Private Function Verifica_Tramites() As Boolean
        Verifica_Tramites = True
        Dim num_primarios As Integer
        If Me.ChkPrim01.Visible = True Then
            If Me.ChkPrim01.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim02.Visible = True Then
            If Me.ChkPrim02.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim03.Visible = True Then
            If Me.ChkPrim03.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim04.Visible = True Then
            If Me.ChkPrim04.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim05.Visible = True Then
            If Me.ChkPrim05.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim06.Visible = True Then
            If Me.ChkPrim06.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim07.Visible = True Then
            If Me.ChkPrim07.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim08.Visible = True Then
            If Me.ChkPrim08.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim09.Visible = True Then
            If Me.ChkPrim09.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim10.Visible = True Then
            If Me.ChkPrim10.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim11.Visible = True Then
            If Me.ChkPrim11.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim12.Visible = True Then
            If Me.ChkPrim12.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim13.Visible = True Then
            If Me.ChkPrim13.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim14.Visible = True Then
            If Me.ChkPrim14.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim15.Visible = True Then
            If Me.ChkPrim15.Checked = True Then
                num_primarios += 1
            End If
        End If
        If Me.ChkPrim16.Visible = True Then
            If Me.ChkPrim16.Checked = True Then
                num_primarios += 1
            End If
        End If
        If num_primarios = 0 Then
            MessageBox.Show("DEBE SELECCIONAR POR LO MENOS UN TRAMITE PRIMARIO EFECTUADO POR EL TICKET", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Verifica_Tramites = False
            Exit Function
        End If
        If num_primarios > 3 Then
            MessageBox.Show("NO PUEDE SELECCIONAR MAS DE 3 TRAMITES PRIMARIOS EFECTUADOS POR EL TICKET", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Verifica_Tramites = False
            Exit Function
        End If
    End Function
    Private Sub Proceso_Retorno()
        Tipo_Combo = "R"
        Dim ind_huellas As Integer
        For ind_huellas = 0 To 99
            Huellas(ind_huellas) = Nothing
        Next
        If Me.ButtonLibre.Visible = True Then
            Huellas(0) = "S"
        Else
            Huellas(0) = "N"
        End If
        If Me.ButtonAtender.Visible = True Then
            Huellas(1) = "S"
        Else
            Huellas(1) = "N"
        End If
        If Me.ButtonAusente.Visible = True Then
            Huellas(2) = "S"
        Else
            Huellas(2) = "N"
        End If
        If Me.ButtonNonShow.Visible = True Then
            Huellas(3) = "S"
        Else
            Huellas(3) = "N"
        End If
        If Me.ButtonRedirect.Visible = True Then
            Huellas(4) = "S"
        Else
            Huellas(4) = "N"
        End If
        If Me.ButtonEspera.Visible = True Then
            Huellas(5) = "S"
        Else
            Huellas(5) = "N"
        End If
        If Me.ButtonRetorno.Visible = True Then
            Huellas(6) = "S"
        Else
            Huellas(6) = "N"
        End If
        If Me.ButtonSalir.Visible = True Then
            Huellas(7) = "S"
        Else
            Huellas(7) = "N"
        End If
        If Me.LblRojo.Visible = True Then
            Huellas(8) = "S"
        Else
            Huellas(8) = "N"
        End If
        If Me.LblAmarillo.Visible = True Then
            Huellas(9) = "S"
        Else
            Huellas(9) = "N"
        End If
        If Me.Lblverde.Visible = True Then
            Huellas(10) = "S"
        Else
            Huellas(10) = "N"
        End If
        Huellas(11) = Me.LblTicket.Text
        If Me.TimerIdle.Enabled = True Then
            Huellas(12) = "S"
        Else
            Huellas(12) = "N"
        End If
        If Me.TimerSearch.Enabled = True Then
            Huellas(13) = "S"
        Else
            Huellas(13) = "N"
        End If
        Huellas(14) = TimerIdle.Interval.ToString
        Huellas(15) = TimerIdle.Tag
        If Me.LblTicket.Visible = True Then
            Huellas(16) = "S"
        Else
            Huellas(16) = "N"
        End If
        If Me.LabelTicketAbajo.Visible = True Then
            Huellas(17) = "S"
        Else
            Huellas(17) = "N"
        End If
        Huellas(18) = Area_Ticket
        Me.ComboDestino.Visible = True
        Me.LblDestino.Text = "TICKET RETORNANTE"
        Me.LblDestino.Visible = True
        Me.ButtonLibre.Visible = False
        Me.ButtonAusente.Visible = False
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.ButtonEspera.Visible = False
        Me.ButtonRetorno.Visible = False
        Me.ButtonRedirect.Visible = False
        Me.ButtonNonShow.Visible = False
        Me.LabelLibre.Visible = False
        Me.LabelAusente.Visible = False
        Me.LabelAtender.Visible = False
        Me.LabelEspera.Visible = False
        Me.LabelRetorno.Visible = False
        Me.LabelRedirect.Visible = False
        Me.ButtonSalir.Visible = False
        Me.LabelSalir.Visible = False
        Me.LabelNonShow.Visible = False
        Me.Rojo.Visible = False
        Me.LblRojo.Visible = False
        Me.Amarillo.Visible = False
        Me.LblAmarillo.Visible = False
        Me.Verde.Visible = True
        Me.Lblverde.Visible = True
        Me.listaTickets.MultiColumn = True
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        Me.listaTickets.SelectionMode = SelectionMode.One
        Me.ComboDestino.Items.Clear()
        Me.listaTickets.Items.Clear()
        Me.ComboDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple
        Me.ComboDestino.Sorted = True
        Me.listaTickets.BeginUpdate()
        Dim instruccion As String
        Dim indic_ae As Integer
        For indic_ae = 0 To 100
            Areas_Espera(indic_ae) = ""
        Next
        instruccion = "Select IQEspera_Ticket, IQEspera_Hora, IQEspera_Area from Iq_TktEspera where IQEspera_Punto = '" & Computer_Code & "' order by IqEspera_Hora"
        Dim Carga_Coneccion_O0 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O0.Open()
        Dim Carga_Comando_O0 As New OleDb.OleDbCommand(instruccion, Carga_Coneccion_O0)
        Dim Carga_Reader_O0 As OleDb.OleDbDataReader = Carga_Comando_O0.ExecuteReader(CommandBehavior.CloseConnection)
        indic_ae = 0
        While Carga_Reader_O0.Read
            Me.listaTickets.Items.Add(Carga_Reader_O0.GetValue(0))
            Me.listaTickets.Items.Add(Carga_Reader_O0.GetValue(0))
            Me.ComboDestino.Items.Add(Carga_Reader_O0.GetValue(0))
            Areas_Espera(indic_ae) = Carga_Reader_O0.GetValue(2)
            indic_ae += 1
        End While
        Carga_Coneccion_O0.Dispose()
        Me.ComboDestino.Items.Add("")
        Me.listaTickets.EndUpdate()
        Me.ComboDestino.EndUpdate()
        If Me.listaTickets.Items.Count = 0 Then
            MessageBox.Show("NO EXISTEN TICKETS EN ESPERA", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.ComboDestino.Visible = False
            Me.LblDestino.Visible = False
            If Huellas(0) = "S" Then
                Me.ButtonLibre.Visible = True
                Me.LabelLibre.Visible = True
            Else
                Me.ButtonLibre.Visible = False
                Me.LabelLibre.Visible = False
            End If
            If Huellas(1) = "S" Then
                Me.ButtonAtender.Visible = True
                Me.LabelAtender.Visible = True
                Me.BtnBell.Visible = True
            Else
                Me.ButtonAtender.Visible = False
                Me.LabelAtender.Visible = False
                Me.BtnBell.Visible = False
            End If
            If Huellas(2) = "S" Then
                Me.ButtonAusente.Visible = True
                Me.LabelAusente.Visible = True
            Else
                Me.ButtonAusente.Visible = False
                Me.LabelAusente.Visible = False
            End If
            If Huellas(3) = "S" Then
                Me.ButtonNonShow.Visible = True
                Me.LabelNonShow.Visible = True
            Else
                Me.ButtonNonShow.Visible = False
                Me.LabelNonShow.Visible = False
            End If
            If Huellas(4) = "S" Then
                Me.ButtonRedirect.Visible = True
                Me.LabelRedirect.Visible = True
            Else
                Me.ButtonRedirect.Visible = False
                Me.LabelRedirect.Visible = False
            End If
            If Huellas(5) = "S" Then
                Me.ButtonEspera.Visible = True
                Me.LabelEspera.Visible = True
            Else
                Me.ButtonEspera.Visible = False
                Me.LabelEspera.Visible = False
            End If
            If Huellas(6) = "S" Then
                Me.ButtonRetorno.Visible = True
                Me.LabelRetorno.Visible = True
            Else
                Me.ButtonRetorno.Visible = False
                Me.LabelRetorno.Visible = False
            End If
            If Huellas(7) = "S" Then
                Me.ButtonSalir.Visible = True
                Me.LabelSalir.Visible = True
            Else
                Me.ButtonSalir.Visible = False
                Me.LabelSalir.Visible = False
            End If
            If Huellas(8) = "S" Then
                Me.LblRojo.Visible = True
                Me.Rojo.Visible = True
            Else
                Me.LblRojo.Visible = False
                Me.Rojo.Visible = False
            End If
            If Huellas(9) = "S" Then
                Me.LblAmarillo.Visible = True
                Me.Amarillo.Visible = True
            Else
                Me.LblAmarillo.Visible = False
                Me.Amarillo.Visible = False
            End If
            If Huellas(10) = "S" Then
                Me.Lblverde.Visible = True
                Me.Verde.Visible = True
            Else
                Me.Lblverde.Visible = False
                Me.Verde.Visible = False
            End If
            Me.LblTicket.Text = Huellas(11)
            If Huellas(16) = "S" Then
                Me.LblTicket.Visible = True
            Else
                Me.LblTicket.Visible = False
            End If
            If Huellas(17) = "S" Then
                Me.LabelTicketAbajo.Visible = True
            Else
                Me.LabelTicketAbajo.Visible = False
            End If
            If Huellas(12) = "S" Then
                Me.TimerIdle.Interval = CInt(Huellas(14))
                Me.TimerIdle.Tag = Huellas(15)
                Me.TimerIdle.Enabled = True
                Me.TimerIdle.Start()
            Else
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
            End If
            If Huellas(13) = "S" Then
                Me.TimerSearch.Enabled = True
                Me.TimerSearch.Start()
            Else
                Me.TimerSearch.Enabled = False
                Me.TimerSearch.Stop()
            End If
            Area_Ticket = Huellas(18)
            Exit Sub
        End If
        Try
            Me.ComboDestino.Text = Me.listaTickets.Items.Item(1)
        Catch ex As Exception
            Exit Try
        End Try
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.TimerIdle.Interval = 20000
        Me.TimerIdle.Tag = "Combo"
        Me.TimerIdle.Enabled = True
        Me.TimerIdle.Start()
        Me.ComboDestino.Focus()
    End Sub
    Private Sub Proceso_Abandono_Espera(Numero_ticket As String)
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "Y"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Numero_ticket
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = ""
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
    End Sub
    Private Sub ComboDestino_Click(sender As Object, e As EventArgs) Handles ComboDestino.Click
        Dim resultado As String = ""
        If Trim(Me.ComboDestino.SelectedItem) <> "" Then
            If Tipo_Combo = "R" And Me.LblTicket.Text <> "" And Me.LblTicket.Text <> "LIBRE" Then
                Dim Central_Cnn2 As New OleDb.OleDbConnection(Cnn_Central_Server)
                Dim CmmCentral2 As New OleDb.OleDbCommand("", Central_Cnn2)
                CmmCentral2.CommandTimeout = 0
                CmmCentral2.CommandType = CommandType.StoredProcedure
                CmmCentral2.CommandText = "IQ_SpPlataforma"
                CmmCentral2.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
                CmmCentral2.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
                CmmCentral2.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
                CmmCentral2.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "C"
                CmmCentral2.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
                CmmCentral2.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
                CmmCentral2.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
                Try
                    Central_Cnn2.Open()
                    CmmCentral2.ExecuteNonQuery()
                    resultado = CmmCentral2.Parameters("Resultado").Value
                    Central_Cnn2.Close()
                Catch exc As Exception
                    Dim Mensaje_Excepcion As String
                    Mensaje_Excepcion = exc.Message
                    MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End Try
            End If
            Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
            Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
            CmmCentral.CommandTimeout = 0
            CmmCentral.CommandType = CommandType.StoredProcedure
            CmmCentral.CommandText = "IQ_SpPlataforma"
            CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
            CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
            CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
            CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = Tipo_Combo
            Select Case Tipo_Combo
                Case "R"
                    CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Trim(Me.ComboDestino.SelectedItem)
                    CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Areas_Espera(Me.ComboDestino.SelectedIndex - 1)
                Case "D"
                    CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = CodigoDestino(Me.ComboDestino.SelectedItem) & "|" & Trim(Me.LblTicket.Text)
                    CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
            End Select
            CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
            Try
                Central_Cnn.Open()
                CmmCentral.ExecuteNonQuery()
                resultado = CmmCentral.Parameters("Resultado").Value
                Central_Cnn.Close()
            Catch exc As Exception
                Dim Mensaje_Excepcion As String
                Mensaje_Excepcion = exc.Message
                MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End Try
        End If
        If Tipo_Combo = "R" Then
            Dim ind_tkt As Integer
            For ind_tkt = 0 To Me.LstEspera.Items.Count
                Dim tkt_lista As String = Mid(Trim(Me.LstEspera.Items(ind_tkt)), 1, InStr(Me.LstEspera.Items(ind_tkt), "(") - 1)
                If Trim(Me.ComboDestino.SelectedItem) = tkt_lista Then
                    Me.LstEspera.Items.Remove(Me.LstEspera.Items(ind_tkt))
                    Exit For
                End If
            Next
        End If
        Me.ComboDestino.Visible = False
        Me.LblDestino.Visible = False
        If Tipo_Combo = "D" Then
            If InStr(resultado, "-") > 0 Then
                Me.Rojo.Visible = False
                Me.LblRojo.Visible = False
                Me.Amarillo.Visible = True
                Me.LblAmarillo.Visible = True
                Me.Verde.Visible = False
                Me.Lblverde.Visible = False
                Me.LblTicket.Text = Mid(resultado, 1, InStr(resultado, "|") - 1)
                Area_Ticket = Mid(resultado, InStr(resultado, "|") + 1, Len(resultado) - InStr(resultado, "|"))
                'Carga_Tramites(Mid(Me.LblTicket.Text, 1, 3))
                Carga_Tramites("SAC")
                Me.LblTicket.Visible = True
                Me.LabelTicketAbajo.Visible = True
                Me.ButtonAtender.Visible = True
                Me.BtnBell.Visible = True
                Me.ButtonAusente.Visible = False
                Me.ButtonEspera.Visible = False
                Me.ButtonLibre.Visible = False
                Me.ButtonNonShow.Visible = True
                Me.ButtonRedirect.Visible = False
                Me.ButtonRetorno.Visible = False
                Me.ButtonSalir.Visible = True
                Me.LabelAtender.Visible = True
                Me.LabelAusente.Visible = False
                Me.LabelEspera.Visible = False
                Me.LabelLibre.Visible = False
                Me.LabelNonShow.Visible = True
                Me.LabelRedirect.Visible = False
                Me.LabelRetorno.Visible = False
                Me.LabelSalir.Visible = True
                Me.TimerSearch.Enabled = False
                Me.TimerSearch.Stop()
                Me.TimerIdle.Interval = 2400000
                Me.TimerIdle.Tag = "Atencion"
                Me.TimerWait.Enabled = True
                Me.TimerWait.Start()
                Me.TimerIdle.Enabled = True
                Me.TimerIdle.Start()
            Else
                Me.Rojo.Visible = False
                Me.LblRojo.Visible = False
                Me.Amarillo.Visible = False
                Me.LblAmarillo.Visible = False
                Me.Verde.Visible = True
                Me.Lblverde.Visible = True
                Me.LblTicket.Text = "LIBRE"
                Area_Ticket = ""
                Me.LblTicket.Visible = True
                Me.LabelTicketAbajo.Visible = True
                Me.ButtonAtender.Visible = False
                Me.PnlPrimario.Visible = False
                nitDisable()
                Me.PnlSecundario.Visible = False
                Me.BtnBell.Visible = False
                Me.ButtonAusente.Visible = True
                Me.ButtonEspera.Visible = False
                Me.ButtonLibre.Visible = False
                Me.ButtonNonShow.Visible = False
                Me.ButtonRedirect.Visible = False
                Me.ButtonRetorno.Visible = True
                Me.ButtonSalir.Visible = True
                Me.LabelAtender.Visible = False
                Me.LabelAusente.Visible = True
                Me.LabelEspera.Visible = False
                Me.LabelLibre.Visible = False
                Me.LabelNonShow.Visible = False
                Me.LabelRedirect.Visible = False
                Me.LabelRetorno.Visible = True
                Me.LstEspera.Visible = True
                Me.LstEspera.Enabled = True
                Me.LabelSalir.Visible = True
                Me.TimerSearch.Enabled = True
                Me.TimerSearch.Start()
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
            End If
        Else
            Me.LblTicket.Text = Trim(Me.ComboDestino.SelectedItem)
            Area_Ticket = Areas_Espera(Me.ComboDestino.SelectedIndex - 1)
            Me.LblTicket.Visible = True
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = True
            Me.LblAmarillo.Visible = True
            Me.Verde.Visible = False
            Me.Lblverde.Visible = False
            Me.LblTicket.Visible = True
            Me.LabelTicketAbajo.Visible = True
            Me.ButtonAtender.Visible = True
            Me.BtnBell.Visible = True
            Me.PnlSecundario.Visible = False
            Me.PnlPrimario.Visible = False
            nitDisable()
            Me.ButtonAusente.Visible = False
            Me.ButtonEspera.Visible = False
            Me.ButtonLibre.Visible = False
            Me.ButtonNonShow.Visible = True
            Me.ButtonRedirect.Visible = False
            Me.ButtonRetorno.Visible = False
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = True
            Me.LabelAusente.Visible = False
            Me.LabelEspera.Visible = False
            Me.LabelLibre.Visible = False
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelNonShow.Visible = True
            Me.LabelRedirect.Visible = False
            Me.LabelRetorno.Visible = False
            Me.LabelSalir.Visible = True
            Me.TimerIdle.Interval = 120000
            Me.TimerIdle.Tag = "Espera"
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerWait.Enabled = True
            Me.TimerWait.Start()
            Me.TimerSearch.Enabled = False
            Me.TimerSearch.Stop()
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
        End If
    End Sub
    Private Sub ComboDestino_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles ComboDestino.KeyPress
        Select Case e.KeyChar
            Case Chr(13)
                Dim resultado As String = ""
                If Trim(Me.ComboDestino.Text) <> "" Then
                    If Tipo_Combo = "R" And Me.LblTicket.Text <> "" And Me.LblTicket.Text <> "LIBRE" Then
                        Dim Central_Cnn2 As New OleDb.OleDbConnection(Cnn_Central_Server)
                        Dim CmmCentral2 As New OleDb.OleDbCommand("", Central_Cnn2)
                        CmmCentral2.CommandTimeout = 0
                        CmmCentral2.CommandType = CommandType.StoredProcedure
                        CmmCentral2.CommandText = "IQ_SpPlataforma"
                        CmmCentral2.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
                        CmmCentral2.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
                        CmmCentral2.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
                        CmmCentral2.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "C"
                        CmmCentral2.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
                        CmmCentral2.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
                        CmmCentral2.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
                        Try
                            Central_Cnn2.Open()
                            CmmCentral2.ExecuteNonQuery()
                            resultado = CmmCentral2.Parameters("Resultado").Value
                            Central_Cnn2.Close()
                        Catch exc As Exception
                            Dim Mensaje_Excepcion As String
                            Mensaje_Excepcion = exc.Message
                            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End Try
                    End If
                    Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
                    Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
                    CmmCentral.CommandTimeout = 0
                    CmmCentral.CommandType = CommandType.StoredProcedure
                    CmmCentral.CommandText = "IQ_SpPlataforma"
                    CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
                    CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
                    CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
                    CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = Tipo_Combo
                    Select Case Tipo_Combo
                        Case "R"
                            CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Trim(Me.ComboDestino.Text)
                            For ind_cmb = 0 To Me.ComboDestino.Items.Count
                                If Me.ComboDestino.Items(ind_cmb) = Me.ComboDestino.Text Then
                                    Area_Ticket = Areas_Espera(ind_cmb - 1)
                                End If
                            Next
                        Case "D"
                            CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = CodigoDestino(Me.ComboDestino.Text) & "|" & Trim(Me.LblTicket.Text)
                            CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
                    End Select
                    CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
                    Try
                        Central_Cnn.Open()
                        CmmCentral.ExecuteNonQuery()
                        resultado = CmmCentral.Parameters("Resultado").Value
                        Central_Cnn.Close()
                    Catch exc As Exception
                        Dim Mensaje_Excepcion As String
                        Mensaje_Excepcion = exc.Message
                        MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit Sub
                    End Try
                End If
                If Tipo_Combo = "R" Then
                    Dim ind_tkt As Integer
                    For ind_tkt = 0 To Me.LstEspera.Items.Count
                        Dim tkt_lista As String = Mid(Trim(Me.LstEspera.Items(ind_tkt)), 1, InStr(Me.LstEspera.Items(ind_tkt), "(") - 1)
                        If Trim(Me.ComboDestino.SelectedItem) = tkt_lista Then
                            Me.LstEspera.Items.Remove(Me.LstEspera.Items(ind_tkt))
                            Exit For
                        End If
                    Next
                End If
                Me.ComboDestino.Visible = False
                Me.LblDestino.Visible = False
                If Tipo_Combo = "D" Then
                    If InStr(resultado, "-") > 0 Then
                        Me.Rojo.Visible = False
                        Me.LblRojo.Visible = False
                        Me.Amarillo.Visible = True
                        Me.LblAmarillo.Visible = True
                        Me.Verde.Visible = False
                        Me.Lblverde.Visible = False
                        Me.LblTicket.Text = Mid(resultado, 1, InStr(resultado, "|") - 1)
                        Area_Ticket = Mid(resultado, InStr(resultado, "|") + 1, Len(resultado) - InStr(resultado, "|"))
                        'Carga_Tramites(Mid(Me.LblTicket.Text, 1, 3))
                        Carga_Tramites("SAC")
                        Me.LblTicket.Visible = True
                        Me.LabelTicketAbajo.Visible = True
                        Me.ButtonAtender.Visible = True
                        Me.BtnBell.Visible = True
                        Me.ButtonAusente.Visible = False
                        Me.ButtonEspera.Visible = False
                        Me.ButtonLibre.Visible = False
                        Me.ButtonNonShow.Visible = True
                        Me.ButtonRedirect.Visible = False
                        Me.ButtonRetorno.Visible = False
                        Me.ButtonSalir.Visible = True
                        Me.LstEspera.Visible = True
                        Me.LstEspera.Enabled = True
                        Me.LabelAtender.Visible = True
                        Me.LabelAusente.Visible = False
                        Me.LabelEspera.Visible = False
                        Me.LabelLibre.Visible = False
                        Me.LabelNonShow.Visible = True
                        Me.LabelRedirect.Visible = False
                        Me.LabelRetorno.Visible = False
                        Me.LabelSalir.Visible = True
                        Me.TimerSearch.Enabled = False
                        Me.TimerSearch.Stop()
                        Me.TimerIdle.Interval = 2400000
                        Me.TimerIdle.Tag = "Atencion"
                        Me.TimerWait.Enabled = True
                        Me.TimerWait.Start()
                        Me.TimerIdle.Enabled = True
                        Me.TimerIdle.Start()
                    Else
                        Me.Rojo.Visible = False
                        Me.LblRojo.Visible = False
                        Me.Amarillo.Visible = False
                        Me.LblAmarillo.Visible = False
                        Me.Verde.Visible = True
                        Me.Lblverde.Visible = True
                        Me.LblTicket.Text = "LIBRE"
                        Area_Ticket = ""
                        Me.LblTicket.Visible = True
                        Me.LabelTicketAbajo.Visible = True
                        Me.ButtonAtender.Visible = False
                        Me.BtnBell.Visible = False
                        Me.ButtonAusente.Visible = True
                        Me.ButtonEspera.Visible = False
                        Me.ButtonLibre.Visible = False
                        Me.PnlPrimario.Visible = False
                        nitDisable()
                        Me.PnlSecundario.Visible = False
                        Me.ButtonNonShow.Visible = False
                        Me.ButtonRedirect.Visible = False
                        Me.ButtonRetorno.Visible = True
                        Me.ButtonSalir.Visible = True
                        Me.LstEspera.Visible = True
                        Me.LstEspera.Enabled = True
                        Me.LabelAtender.Visible = False
                        Me.LabelAusente.Visible = True
                        Me.LabelEspera.Visible = False
                        Me.LabelLibre.Visible = False
                        Me.LabelNonShow.Visible = False
                        Me.LabelRedirect.Visible = False
                        Me.LabelRetorno.Visible = True
                        Me.LabelSalir.Visible = True
                        Me.TimerSearch.Enabled = True
                        Me.TimerSearch.Start()
                        Me.TimerIdle.Enabled = False
                        Me.TimerIdle.Stop()
                    End If
                Else
                    Me.Rojo.Visible = False
                    Me.LblRojo.Visible = False
                    Me.Amarillo.Visible = True
                    Me.LblAmarillo.Visible = True
                    Me.LblTicket.Text = Trim(Me.ComboDestino.Text)
                    For ind_cmb = 0 To Me.ComboDestino.Items.Count
                        If Me.ComboDestino.Items(ind_cmb) = Me.ComboDestino.Text Then
                            Area_Ticket = Areas_Espera(ind_cmb - 1)
                        End If
                    Next
                    'Carga_Tramites(Mid(Me.LblTicket.Text, 1, 3))
                    Carga_Tramites("SAC")
                    Me.LblTicket.Visible = True
                    Me.LabelTicketAbajo.Visible = True
                    Me.Verde.Visible = False
                    Me.Lblverde.Visible = False
                    Me.ButtonAtender.Visible = False
                    Me.BtnBell.Visible = False
                    Me.ButtonAusente.Visible = True
                    Me.ButtonEspera.Visible = True
                    Me.ButtonLibre.Visible = True
                    Me.ButtonNonShow.Visible = False
                    Me.ButtonRedirect.Visible = True
                    Me.ButtonRetorno.Visible = True
                    Me.ButtonSalir.Visible = True
                    Me.LabelAtender.Visible = False
                    Me.LabelAusente.Visible = True
                    Me.LabelEspera.Visible = True
                    Me.LabelLibre.Visible = True
                    Me.LabelNonShow.Visible = False
                    Me.LabelRedirect.Visible = True
                    Me.LabelRetorno.Visible = True
                    Me.LabelSalir.Visible = True
                    Me.TimerSearch.Enabled = False
                    Me.TimerSearch.Stop()
                    Me.TimerIdle.Interval = 2400000
                    Me.TimerIdle.Tag = "Atencion"
                    Me.TimerIdle.Enabled = True
                    Me.TimerIdle.Start()
                End If
        End Select
    End Sub
    Private Sub Proceso_Derivacion()
        Tipo_Combo = "D"
        Me.ComboDestino.Visible = True
        Me.LblDestino.Text = "AREA/PUNTO DE DESTINO"
        Me.LblDestino.Visible = True
        Me.ButtonLibre.Visible = False
        Me.ButtonAusente.Visible = False
        Me.ButtonEspera.Visible = False
        Me.ButtonRetorno.Visible = False
        Me.ButtonRedirect.Visible = False
        Me.ButtonNonShow.Visible = False
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.LabelLibre.Visible = False
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = False
        Me.LabelEspera.Visible = False
        Me.LabelRetorno.Visible = False
        Me.LabelRedirect.Visible = False
        Me.LabelNonShow.Visible = False
        Me.ButtonSalir.Visible = False
        Me.LabelSalir.Visible = False
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        Me.listaDestinos.MultiColumn = True
        Me.listaDestinos.SelectionMode = SelectionMode.One
        Me.DictDestinos.Clear()
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.ComboDestino.Items.Clear()
        Me.listaDestinos.Items.Clear()
        Me.ComboDestino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple
        Me.ComboDestino.Sorted = True
        Me.listaDestinos.BeginUpdate()
        Dim instruccion As String
        instruccion = "Select IQAreas_Codigo, max(IQAreas_Descripcion) from IQ_PuntosAtencion Join IQ_Areas on IQPuntos_Area = IQAreas_Codigo Join IQ_Oficinas on IQAreas_Oficina = IQOficinas_Codigo Join IQ_Workstations on IQPuntos_Codigo = IQWS_CodPunto where IQAreas_Oficina = '" & Computer_Ofic & "' and (iqws_status = 'l' or iqws_status = 'b' or iqws_status = 't')  and iqpuntos_Codigo <> '" & Computer_Code & "' group by IQAreas_Codigo order by IqAreas_Codigo"
        Dim Carga_Coneccion_O0 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O0.Open()
        Dim Carga_Comando_O0 As New OleDb.OleDbCommand(instruccion, Carga_Coneccion_O0)
        Dim Carga_Reader_O0 As OleDb.OleDbDataReader = Carga_Comando_O0.ExecuteReader(CommandBehavior.CloseConnection)
        While Carga_Reader_O0.Read
            Me.listaDestinos.Items.Add("AREA:" & Carga_Reader_O0.GetValue(0))
            Me.listaDestinos.Items.Add("AREA:" & Carga_Reader_O0.GetValue(1))
            Me.ComboDestino.Items.Add("AREA:" & Carga_Reader_O0.GetValue(1))
        End While
        Carga_Coneccion_O0.Dispose()
        instruccion = "Select IQPuntos_Codigo, IQPuntos_Descripcion from IQ_PuntosAtencion Join IQ_Areas on IQPuntos_Area = IQAreas_Codigo Join IQ_Oficinas on IQAreas_Oficina = IQOficinas_Codigo Join IQ_Workstations on IQPuntos_Codigo = IQWS_CodPunto where IQAreas_Oficina = '" & Computer_Ofic & "' and (iqws_status = 'l' or iqws_status = 'b' or iqws_status = 't')  and iqpuntos_Codigo <> '" & Computer_Code & "' order by IqPuntos_Descripcion"
        Dim Carga_Coneccion_O1 As New OleDb.OleDbConnection(Cnn_Central_Server)
        Carga_Coneccion_O1.Open()
        Dim Carga_Comando_O1 As New OleDb.OleDbCommand(instruccion, Carga_Coneccion_O1)
        Dim Carga_Reader_O1 As OleDb.OleDbDataReader = Carga_Comando_O1.ExecuteReader(CommandBehavior.CloseConnection)
        While Carga_Reader_O1.Read
            Me.listaDestinos.Items.Add(Carga_Reader_O1.GetValue(0))
            Me.listaDestinos.Items.Add(Carga_Reader_O1.GetValue(1))
            Me.ComboDestino.Items.Add(Carga_Reader_O1.GetValue(1))
        End While
        Carga_Coneccion_O1.Dispose()
        Me.listaDestinos.EndUpdate()
        Me.ComboDestino.EndUpdate()
        If Me.listaDestinos.Items.Count = 0 Then
            MessageBox.Show("NO EXISTEN PUNTOS O AREAS DE ATENCION PARA REDIRECCIONAMIENTO", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.ComboDestino.Visible = False
            Me.LblDestino.Visible = False
            Me.Rojo.Visible = False
            Me.LblRojo.Visible = False
            Me.Amarillo.Visible = True
            Me.LblAmarillo.Visible = True
            Me.Verde.Visible = False
            Me.Lblverde.Visible = False
            Me.ButtonAtender.Visible = False
            Me.BtnBell.Visible = False
            Me.ButtonAusente.Visible = True
            Me.TimerIdle.Interval = 2400000
            Me.TimerIdle.Tag = "Atencion"
            Me.TimerIdle.Enabled = True
            Me.TimerIdle.Start()
            Me.TimerSearch.Enabled = False
            Me.TimerSearch.Stop()
            Me.ButtonEspera.Visible = True
            Me.ButtonLibre.Visible = True
            Me.ButtonNonShow.Visible = False
            Me.ButtonRedirect.Visible = True
            Me.ButtonRetorno.Visible = True
            Me.ButtonSalir.Visible = True
            Me.LabelAtender.Visible = False
            Me.LabelAusente.Visible = True
            Me.LabelEspera.Visible = True
            Me.LabelLibre.Visible = True
            Me.LstEspera.Visible = True
            Me.LstEspera.Enabled = True
            Me.LabelNonShow.Visible = False
            Me.LabelRedirect.Visible = True
            Me.LabelRetorno.Visible = True
            Me.LabelSalir.Visible = True
        End If
        If Me.listaDestinos.Items.Count > 0 Then
            For Me.counter_lista = 0 To Me.listaDestinos.Items.Count - 2 Step 2
                Me.DictDestinos.Add_ColeccionDestinos(Me.listaDestinos.Items.Item(counter_lista), Me.listaDestinos.Items.Item(counter_lista + 1))
            Next
        End If
        Me.ComboDestino.Enabled = True
        Try
            Me.ComboDestino.Text = Me.listaDestinos.Items.Item(1)
        Catch ex As Exception
            Exit Try
        End Try
        Me.TimerIdle.Interval = 20000
        Me.TimerIdle.Tag = "Combo"
        Me.TimerIdle.Enabled = True
        Me.TimerIdle.Start()
        Me.ComboDestino.Focus()
    End Sub
    Private Sub IQ_P0001_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown, ButtonLibre.KeyDown, ButtonAusente.KeyDown, ButtonEspera.KeyDown, ButtonRetorno.KeyDown, ButtonRedirect.KeyDown, ButtonNonShow.KeyDown, ButtonSalir.KeyDown, ComboDestino.KeyDown
        Alt_F4 = False
        If e.Alt And e.KeyCode = Keys.F4 Then
            Alt_F4 = True
        End If
        If e.Control And e.KeyCode.ToString = "L" Then
            If Me.ButtonLibre.Visible = True Then
                Proceso_Libre()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "N" Then
            If Me.ButtonNonShow.Visible = True Then
                Proceso_NonShow()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "A" Then
            If Me.ButtonAusente.Visible = True Then
                Proceso_Ausente()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "E" Then
            If Me.ButtonEspera.Visible = True Then
                Proceso_Espera()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "R" Then
            If Me.ButtonRetorno.Visible = True Then
                Proceso_Retorno()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "D" Then
            If Me.ButtonRedirect.Visible = True Then
                Proceso_Derivacion()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "O" Then
            If Me.ButtonAtender.Visible = True Then
                Proceso_Atender()
            End If
        ElseIf e.Control And e.KeyCode.ToString = "X" Then
            If Me.ButtonSalir.Visible = True Then
                Proceso_Salir()
            End If
        End If
        If sender.NAME = "ComboDestino" And e.KeyCode = Keys.Escape Then
            If Tipo_Combo = "R" Then
                Me.ComboDestino.Visible = False
                Me.LblDestino.Visible = False
                If Huellas(0) = "S" Then
                    Me.ButtonLibre.Visible = True
                    Me.LabelLibre.Visible = True
                Else
                    Me.ButtonLibre.Visible = False
                    Me.LabelLibre.Visible = False
                End If
                If Huellas(1) = "S" Then
                    Me.ButtonAtender.Visible = True
                    Me.LabelAtender.Visible = True
                    Me.BtnBell.Visible = True
                Else
                    Me.ButtonAtender.Visible = False
                    Me.LabelAtender.Visible = False
                    Me.BtnBell.Visible = False
                End If
                If Huellas(2) = "S" Then
                    Me.ButtonAusente.Visible = True
                    Me.LabelAusente.Visible = True
                Else
                    Me.ButtonAusente.Visible = False
                    Me.LabelAusente.Visible = False
                End If
                If Huellas(3) = "S" Then
                    Me.ButtonNonShow.Visible = True
                    Me.LabelNonShow.Visible = True
                Else
                    Me.ButtonNonShow.Visible = False
                    Me.LabelNonShow.Visible = False
                End If
                If Huellas(4) = "S" Then
                    Me.ButtonRedirect.Visible = True
                    Me.LabelRedirect.Visible = True
                Else
                    Me.ButtonRedirect.Visible = False
                    Me.LabelRedirect.Visible = False
                End If
                If Huellas(5) = "S" Then
                    Me.ButtonEspera.Visible = True
                    Me.LabelEspera.Visible = True
                Else
                    Me.ButtonEspera.Visible = False
                    Me.LabelEspera.Visible = False
                End If
                If Huellas(6) = "S" Then
                    Me.ButtonRetorno.Visible = True
                    Me.LabelRetorno.Visible = True
                Else
                    Me.ButtonRetorno.Visible = False
                    Me.LabelRetorno.Visible = False
                End If
                If Huellas(7) = "S" Then
                    Me.ButtonSalir.Visible = True
                    Me.LabelSalir.Visible = True
                Else
                    Me.ButtonSalir.Visible = False
                    Me.LabelSalir.Visible = False
                End If
                If Huellas(8) = "S" Then
                    Me.LblRojo.Visible = True
                    Me.Rojo.Visible = True
                Else
                    Me.LblRojo.Visible = False
                    Me.Rojo.Visible = False
                End If
                If Huellas(9) = "S" Then
                    Me.LblAmarillo.Visible = True
                    Me.Amarillo.Visible = True
                Else
                    Me.LblAmarillo.Visible = False
                    Me.Amarillo.Visible = False
                End If
                If Huellas(10) = "S" Then
                    Me.Lblverde.Visible = True
                    Me.Verde.Visible = True
                Else
                    Me.Lblverde.Visible = False
                    Me.Verde.Visible = False
                End If
                Me.LblTicket.Text = Huellas(11)
                If Huellas(16) = "S" Then
                    Me.LblTicket.Visible = True
                Else
                    Me.LblTicket.Visible = False
                End If
                If Huellas(17) = "S" Then
                    Me.LabelTicketAbajo.Visible = True
                Else
                    Me.LabelTicketAbajo.Visible = False
                End If
                If Huellas(12) = "S" Then
                    Me.TimerIdle.Interval = CInt(Huellas(14))
                    Me.TimerIdle.Tag = Huellas(15)
                    Me.TimerIdle.Enabled = True
                    Me.TimerIdle.Start()
                Else
                    Me.TimerIdle.Enabled = False
                    Me.TimerIdle.Stop()
                End If
                If Huellas(13) = "S" Then
                    Me.TimerSearch.Enabled = True
                    Me.TimerSearch.Start()
                Else
                    Me.TimerSearch.Enabled = False
                    Me.TimerSearch.Stop()
                End If
                Area_Ticket = Huellas(18)
                Me.LstEspera.Visible = True
                Me.LstEspera.Enabled = True
            Else
                Me.ComboDestino.Visible = False
                Me.LblDestino.Visible = False
                Me.Rojo.Visible = False
                Me.LblRojo.Visible = False
                Me.Amarillo.Visible = True
                Me.LblAmarillo.Visible = True
                Me.Verde.Visible = False
                Me.Lblverde.Visible = False
                Me.ButtonAtender.Visible = False
                Me.BtnBell.Visible = False
                Me.ButtonAusente.Visible = True
                Me.TimerIdle.Interval = 2400000
                Me.TimerIdle.Tag = "Atencion"
                Me.TimerIdle.Enabled = True
                Me.TimerIdle.Start()
                Me.TimerSearch.Enabled = False
                Me.TimerSearch.Stop()
                Me.ButtonEspera.Visible = True
                Me.ButtonLibre.Visible = True
                Me.ButtonNonShow.Visible = False
                Me.ButtonRedirect.Visible = True
                Me.ButtonRetorno.Visible = True
                Me.ButtonSalir.Visible = True
                Me.LabelAtender.Visible = False
                Me.LabelAusente.Visible = True
                Me.LabelEspera.Visible = True
                Me.LabelLibre.Visible = True
                Me.LabelNonShow.Visible = False
                Me.LabelRedirect.Visible = True
                Me.LabelRetorno.Visible = True
                Me.LabelSalir.Visible = True
                Me.LstEspera.Visible = True
                Me.LstEspera.Enabled = True
            End If
        End If
    End Sub
    Private Sub TimerEspera_Tick(sender As Object, e As EventArgs) Handles TimerEspera.Tick
        Me.TimerEspera.Enabled = False
        Me.TimerEspera.Stop()
nuevamente:
        If Me.LstEspera.Items.Count > 0 Then
            For Me.counter_lista = 0 To Me.LstEspera.Items.Count - 1 Step 1
                Dim codigo_ticket As String
                Dim hora_ticket As Integer
                Dim min_ticket As Integer
                Dim seg_ticket As Integer
                Dim pos_hora As Integer
                pos_hora = InStr(Me.LstEspera.Items.Item(Me.counter_lista), "(")
                codigo_ticket = Mid(Me.LstEspera.Items.Item(Me.counter_lista), 1, pos_hora - 1)
                hora_ticket = CInt(Mid(Me.LstEspera.Items.Item(Me.counter_lista), pos_hora + 1, 2))
                min_ticket = CInt(Mid(Me.LstEspera.Items.Item(Me.counter_lista), pos_hora + 4, 2))
                seg_ticket = CInt(Mid(Me.LstEspera.Items.Item(Me.counter_lista), pos_hora + 7, 2))
                seg_ticket = seg_ticket + (min_ticket * 60) + (hora_ticket * 3600)
                Dim hora_now As Integer
                Dim min_now As Integer
                Dim seg_now As Integer
                Dim time_now As String = Format(DateTime.Now, "HH:mm:ss")
                hora_now = CInt(Mid(time_now, 1, 2))
                min_now = CInt(Mid(time_now, 4, 2))
                seg_now = CInt(Mid(time_now, 7, 2))
                seg_now = seg_now + (min_now * 60) + (hora_now * 3600)
                seg_ticket = seg_now - seg_ticket
                If seg_ticket > 60 Then
                    Me.LstEspera.Items.Remove(Me.LstEspera.Items(counter_lista))
                    Proceso_Abandono_Espera(codigo_ticket)
                    GoTo nuevamente
                End If
            Next
            Me.TimerEspera.Enabled = True
            Me.TimerEspera.Start()
        End If
    End Sub
    Private Sub TimerSearch_Tick(sender As Object, e As EventArgs) Handles TimerSearch.Tick
        If Trim(Me.LblTicket.Text) = "" Or Trim(Me.LblTicket.Text) = "LIBRE" Then
            If Me.LblTicket.Visible = True Then
                TimerSearch.Enabled = False
                TimerSearch.Stop()
                Dim Carga_Coneccion_O2 As New OleDb.OleDbConnection(Cnn_Central_Server)
                Carga_Coneccion_O2.Open()
                Dim Carga_Comando_O2 As New OleDb.OleDbCommand("Select * from IQ_Pending where IQPending_CodPunto = '" & Computer_Code & "' Order by IQPending_Hora Desc", Carga_Coneccion_O2)
                Dim Carga_Reader_O2 As OleDb.OleDbDataReader = Carga_Comando_O2.ExecuteReader(CommandBehavior.CloseConnection)
                While Carga_Reader_O2.Read
                    If IsDBNull(Carga_Reader_O2.GetValue(0)) = False Then
                        Me.LblTicket.Text = Carga_Reader_O2.GetValue(1)
                        Me.LblTicket.Visible = True
                        Area_Ticket = Carga_Reader_O2.GetValue(0)
                        Me.LabelTicketAbajo.Visible = True
                        Me.Rojo.Visible = False
                        Me.LblRojo.Visible = False
                        Me.Amarillo.Visible = True
                        Me.LblAmarillo.Visible = True
                        Me.Verde.Visible = False
                        Me.Lblverde.Visible = False
                        Me.ButtonAtender.Visible = True
                        Me.BtnBell.Visible = True
                        Me.ButtonAusente.Visible = False
                        Me.ButtonEspera.Visible = False
                        Me.ButtonLibre.Visible = False
                        Me.ButtonNonShow.Visible = True
                        Me.ButtonRedirect.Visible = False
                        Me.ButtonRetorno.Visible = False
                        Me.ButtonSalir.Visible = False
                        Me.LabelAtender.Visible = True
                        Me.LabelAusente.Visible = False
                        Me.LabelEspera.Visible = False
                        Me.LstEspera.Visible = True
                        Me.LstEspera.Enabled = True
                        Me.LabelLibre.Visible = False
                        Me.LabelNonShow.Visible = True
                        Me.LabelRedirect.Visible = False
                        Me.LabelRetorno.Visible = False
                        Me.LabelSalir.Visible = False
                        Me.TimerIdle.Enabled = False
                        Me.TimerIdle.Stop()
                        TimerSearch.Enabled = False
                        TimerSearch.Stop()
                        Dim doc As IQ_P0002 = New IQ_P0002
                        doc.TopMost = True
                        doc.ShowDialog()
                        'If IQ_P0002.Label2.Text = "A" Then
                        ' Proceso_Atender()
                        ' Exit Sub
                        'Else
                        '   Proceso_NonShow()
                        '  Exit Sub
                        'End If
                        Me.TimerIdle.Interval = 120000
                        Me.TimerIdle.Tag = "Espera"
                        Me.TimerIdle.Enabled = True
                        Me.TimerIdle.Start()
                        Me.TimerWait.Enabled = True
                        Me.TimerWait.Start()
                        Exit Sub
                    Else
                        Me.TimerIdle.Enabled = False
                        Me.TimerIdle.Stop()
                        Me.TimerSearch.Enabled = True
                        Me.TimerSearch.Start()
                        Exit Sub
                    End If
                End While
                Carga_Coneccion_O2.Dispose()
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
                Me.TimerSearch.Enabled = True
                Me.TimerSearch.Start()
            Else
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
                Me.TimerSearch.Enabled = True
                Me.TimerSearch.Start()
            End If
        End If
    End Sub
    Private Sub TimerWait_Tick(sender As Object, e As EventArgs) Handles TimerWait.Tick
        TimerWait.Enabled = False
        TimerWait.Stop()
        Proceso_NonShow()
    End Sub
    Private Sub Proceso_Ausente()
        disableCall()
        'Me.btnPhone.Enabled = True
        ' Me.LblRojo.Text = "AUSENTE"
        Me.btnPhone.Enabled = False
        Dim justificativo As String = ""
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        Do Until justificativo <> ""
            justificativo = InputBox("Ingrese por favor el Justificativo de su Ausencia (X CANCELA):", "")
        Loop
        If UCase(justificativo) = "X" Then
            Me.TimerWait.Enabled = True
            Me.TimerWait.Start()
            enableCall()
            Exit Sub
        End If
        If Me.PnlPrimario.Visible = True Then
            If Verifica_Tramites2() = False Then
                Exit Sub
            End If
            Graba_Tramites()
        End If
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "A"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text & "|" & justificativo
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        Me.Rojo.Visible = True
        Me.LblRojo.Visible = True
        Me.Amarillo.Visible = False
        Me.LblAmarillo.Visible = False
        Me.Verde.Visible = False
        Me.Lblverde.Visible = False
        Me.ButtonLibre.Visible = True
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.PnlPrimario.Visible = False
        nitDisable()
        Me.PnlSecundario.Visible = False
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        Me.TimerIdle.Enabled = False
        Me.TimerIdle.Stop()
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.ButtonAusente.Visible = False
        Me.ButtonEspera.Visible = False
        Me.ButtonNonShow.Visible = False
        Me.ButtonRedirect.Visible = False
        Me.LabelLibre.Visible = True
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = False
        Me.LabelEspera.Visible = False
        Me.LabelNonShow.Visible = False
        Me.LabelRedirect.Visible = False
        Me.LabelTicketAbajo.Visible = False
        Me.LblTicket.Visible = False
        Area_Ticket = ""
        Me.Lblverde.Visible = False
    End Sub
    Private Sub TimerIdle_Tick(sender As Object, e As EventArgs) Handles TimerIdle.Tick
        Me.TimerIdle.Enabled = False
        Me.TimerIdle.Stop()
        Select Case TimerIdle.Tag
            Case "Atencion"
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
                Dim respuesta
                respuesta = MessageBox.Show("¿Precisa Ud. más tiempo en la atención del ticket actual?", "TICKET EN PROCESO DE ATENCION", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If respuesta = Windows.Forms.DialogResult.Yes Then
                    Dim justificativo As String = ""
                    Do Until justificativo <> ""
                        justificativo = InputBox("Ingrese por favor el Justificativo del exceso de tiempo en la atención:", "")
                    Loop
                    Dim instruccion_insert As String = ""
                    instruccion_insert = "Update Iq_Tickets Set IQTicket_Delay = IQTicket_Delay + '" & justificativo & " ' where IQTicket_Area = '" & Area_Ticket & "' and IQTicket_Ticket = '" & Me.LblTicket.Text & "' And IQTicket_Estado = 'P' and IQTicket_Fecha = CONVERT(varchar(10), getdate(), 111)"
                    Try
                        Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
                        IQ_Cnn.Open()
                        Dim IQ_Cmm As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
                        Dim RegistrosInsertados As Long = IQ_Cmm.ExecuteNonQuery()
                        IQ_Cnn.Close()
                    Catch exc As Exception
                        Dim Mensaje_Excepcion As String
                        Mensaje_Excepcion = exc.Message
                        MessageBox.Show(Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                        Exit Sub
                    End Try
                    Me.TimerIdle.Enabled = True
                    Me.TimerIdle.Start()
                Else
                    Proceso_Libre()
                    Exit Sub
                End If
            Case "Espera"
                Me.TimerIdle.Enabled = False
                Me.TimerIdle.Stop()
                '              Dim respuesta
                '              respuesta = MessageBox.Show("¿El Ticket asignado se presentó o no?", "TICKET PENDIENTE", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
                '              If respuesta = Windows.Forms.DialogResult.Cancel Then
                ' Me.TimerIdle.Enabled = True
                ' Me.TimerIdle.Start()
                ' Exit Sub
                ' ElseIf respuesta = Windows.Forms.DialogResult.Yes Then
                ' Proceso_Atender()
                ' Exit Sub
                ' Else
                ' Proceso_NonShow()
                ' Exit Sub
                ' End If
            Case "Combo"
                If Tipo_Combo = "R" Then
                    Me.TimerWait.Enabled = False
                    Me.TimerWait.Stop()
                    Me.ComboDestino.Visible = False
                    Me.LblDestino.Visible = False
                    If Huellas(0) = "S" Then
                        Me.ButtonLibre.Visible = True
                        Me.LabelLibre.Visible = True
                    Else
                        Me.ButtonLibre.Visible = False
                        Me.LabelLibre.Visible = False
                    End If
                    If Huellas(1) = "S" Then
                        Me.ButtonAtender.Visible = True
                        Me.LabelAtender.Visible = True
                        Me.BtnBell.Visible = True
                    Else
                        Me.ButtonAtender.Visible = False
                        Me.LabelAtender.Visible = False
                        Me.BtnBell.Visible = False
                    End If
                    If Huellas(2) = "S" Then
                        Me.ButtonAusente.Visible = True
                        Me.LabelAusente.Visible = True
                    Else
                        Me.ButtonAusente.Visible = False
                        Me.LabelAusente.Visible = False
                    End If
                    If Huellas(3) = "S" Then
                        Me.ButtonNonShow.Visible = True
                        Me.LabelNonShow.Visible = True
                    Else
                        Me.ButtonNonShow.Visible = False
                        Me.LabelNonShow.Visible = False
                    End If
                    If Huellas(4) = "S" Then
                        Me.ButtonRedirect.Visible = True
                        Me.LabelRedirect.Visible = True
                    Else
                        Me.ButtonRedirect.Visible = False
                        Me.LabelRedirect.Visible = False
                    End If
                    If Huellas(5) = "S" Then
                        Me.ButtonEspera.Visible = True
                        Me.LabelEspera.Visible = True
                    Else
                        Me.ButtonEspera.Visible = False
                        Me.LabelEspera.Visible = False
                    End If
                    If Huellas(6) = "S" Then
                        Me.ButtonRetorno.Visible = True
                        Me.LabelRetorno.Visible = True
                    Else
                        Me.ButtonRetorno.Visible = False
                        Me.LabelRetorno.Visible = False
                    End If
                    If Huellas(7) = "S" Then
                        Me.ButtonSalir.Visible = True
                        Me.LabelSalir.Visible = True
                    Else
                        Me.ButtonSalir.Visible = False
                        Me.LabelSalir.Visible = False
                    End If
                    If Huellas(8) = "S" Then
                        Me.LblRojo.Visible = True
                        Me.Rojo.Visible = True
                    Else
                        Me.LblRojo.Visible = False
                        Me.Rojo.Visible = False
                    End If
                    If Huellas(9) = "S" Then
                        Me.LblAmarillo.Visible = True
                        Me.Amarillo.Visible = True
                    Else
                        Me.LblAmarillo.Visible = False
                        Me.Amarillo.Visible = False
                    End If
                    If Huellas(10) = "S" Then
                        Me.Lblverde.Visible = True
                        Me.Verde.Visible = True
                    Else
                        Me.Lblverde.Visible = False
                        Me.Verde.Visible = False
                    End If
                    Me.LblTicket.Text = Huellas(11)
                    If Huellas(16) = "S" Then
                        Me.LblTicket.Visible = True
                    Else
                        Me.LblTicket.Visible = False
                    End If
                    If Huellas(17) = "S" Then
                        Me.LabelTicketAbajo.Visible = True
                    Else
                        Me.LabelTicketAbajo.Visible = False
                    End If
                    If Huellas(12) = "S" Then
                        Me.TimerIdle.Interval = CInt(Huellas(14))
                        Me.TimerIdle.Tag = Huellas(15)
                        Me.TimerIdle.Enabled = True
                        Me.TimerIdle.Start()
                    Else
                        Me.TimerIdle.Enabled = False
                        Me.TimerIdle.Stop()
                    End If
                    If Huellas(13) = "S" Then
                        Me.TimerSearch.Enabled = True
                        Me.TimerSearch.Start()
                    Else
                        Me.TimerSearch.Enabled = False
                        Me.TimerSearch.Stop()
                    End If
                    Me.LstEspera.Visible = True
                    Me.LstEspera.Enabled = True
                    Area_Ticket = Huellas(18)
                Else
                    Me.TimerWait.Enabled = False
                    Me.TimerWait.Stop()
                    Me.ComboDestino.Visible = False
                    Me.LblDestino.Visible = False
                    Me.Rojo.Visible = False
                    Me.LblRojo.Visible = False
                    Me.Amarillo.Visible = True
                    Me.LblAmarillo.Visible = True
                    Me.Verde.Visible = False
                    Me.Lblverde.Visible = False
                    Me.ButtonAtender.Visible = False
                    Me.BtnBell.Visible = False
                    Me.ButtonAusente.Visible = True
                    Me.TimerIdle.Interval = 2400000
                    Me.TimerIdle.Tag = "Atencion"
                    Me.TimerIdle.Enabled = True
                    Me.TimerIdle.Start()
                    Me.TimerSearch.Enabled = False
                    Me.TimerSearch.Stop()
                    Me.PnlPrimario.Visible = True
                    nitEnable()
                    Me.PnlSecundario.Visible = True
                    Me.ButtonEspera.Visible = True
                    Me.ButtonLibre.Visible = True
                    Me.ButtonNonShow.Visible = False
                    Me.ButtonRedirect.Visible = True
                    Me.ButtonRetorno.Visible = True
                    Me.ButtonSalir.Visible = True
                    Me.LabelAtender.Visible = False
                    Me.LabelAusente.Visible = True
                    Me.LabelEspera.Visible = True
                    Me.LabelLibre.Visible = True
                    Me.LabelNonShow.Visible = False
                    Me.LabelRedirect.Visible = True
                    Me.LstEspera.Visible = True
                    Me.LstEspera.Enabled = True
                    Me.LabelRetorno.Visible = True
                    Me.LabelSalir.Visible = True
                End If
            Case "Nuevo"
        End Select
    End Sub

    Private Sub ChkPrim01_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim01.CheckedChanged
        'If checkAllCell() Then
        If Me.ChkPrim01.Checked Then
            aux("PBD")
            Me.arrayValues(posArray) = "PBD"
            posArray += 1
        Else
            'auxErase("PBD")
            'setArrayCheck()
            uncheckFunc("PBD")


        End If
    End Sub
    Private Sub ChkPrim02_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim02.CheckedChanged
        If Me.ChkPrim02.Checked Then
            aux("ST")
            Me.arrayValues(posArray) = "ST"
            posArray += 1
        Else
            'auxErase("ST")
            'setArrayCheck()
            uncheckFunc("ST")
        End If


    End Sub
    Private Sub ChkPrim03_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim03.CheckStateChanged
        If Me.ChkPrim03.Checked Then
            aux("SF")
            Me.arrayValues(posArray) = "SF"
            posArray += 1
        Else
            'auxErase("SF")
            'setArrayCheck()
            uncheckFunc("SF")
        End If
    End Sub
    Private Sub ChkPrim04_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim04.CheckStateChanged
        If Me.ChkPrim04.Checked Then
            aux("ERO")
            Me.arrayValues(posArray) = "ERO"
            posArray += 1
        Else
            'auxErase("ERO")
            'setArrayCheck()
            uncheckFunc("ERO")
        End If
    End Sub
    Private Sub ChkPrim05_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim05.CheckStateChanged
        If Me.ChkPrim05.Checked Then
            aux("DJ")
            Me.arrayValues(posArray) = "DJ"
            posArray += 1
        Else
            'auxErase("DJ")
            'setArrayCheck()
            uncheckFunc("DJ")
        End If
    End Sub
    Private Sub ChkPrim06_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim06.CheckedChanged
        If Me.ChkPrim06.Checked Then
            aux("NRT")
            Me.arrayValues(posArray) = "NRT"
            posArray += 1
        Else
            'auxErase("NRT")
            setArrayCheck()
            uncheckFunc("NRT")
        End If
    End Sub
    Private Sub ChkPrim07_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPrim07.CheckedChanged
        If Me.ChkPrim07.Checked Then
            aux("PTE")
            Me.arrayValues(posArray) = "PTE"
            posArray += 1
        Else
            'auxErase("PTE")
            setArrayCheck()
            uncheckFunc("PTE")
        End If
    End Sub

    Private Sub aux(ByVal Codigo As String)

        If toogle = 1 Then
            toogle = 0
        Else
            toogle = 1
        End If
        Dim indice_primario As Integer = contadorAux
        'Dim indice_secundario As Integer = contadorAux
        Dim backColor As Color
        If toogle = 0 Then
            backColor = Color.FromArgb(160, 160, 255)
        Else
            backColor = Color.FromArgb(192, 192, 255)
        End If



        For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
            If DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Ticket") = Codigo Then
                indice_primario += 1
                contadorAux += 1
                Select Case indice_primario
                    Case 1
                        Me.ChkSec01.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec01.Visible = True
                        Me.ChkSec01.BackColor = backColor
                    Case 2
                        Me.ChkSec02.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec02.Visible = True
                        Me.ChkSec02.BackColor = backColor
                    Case 3
                        Me.ChkSec03.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec03.Visible = True
                        Me.ChkSec03.BackColor = backColor
                    Case 4
                        Me.ChkSec04.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec04.Visible = True
                        Me.ChkSec04.BackColor = backColor
                    Case 5
                        Me.ChkSec05.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec05.Visible = True
                        Me.ChkSec05.BackColor = backColor
                    Case 6
                        Me.ChkSec06.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec06.Visible = True
                        Me.ChkSec06.BackColor = backColor
                    Case 7
                        Me.ChkSec07.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec07.Visible = True
                        Me.ChkSec07.BackColor = backColor
                    Case 8
                        Me.ChkSec08.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec08.Visible = True
                        Me.ChkSec08.BackColor = backColor
                    Case 9
                        Me.ChkSec09.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec09.Visible = True
                        Me.ChkSec09.BackColor = backColor
                    Case 10
                        Me.ChkSec10.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec10.Visible = True
                        Me.ChkSec10.BackColor = backColor
                    Case 11
                        Me.ChkSec11.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec11.Visible = True
                        Me.ChkSec11.BackColor = backColor
                    Case 12
                        Me.ChkSec12.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec12.Visible = True
                        Me.ChkSec12.BackColor = backColor
                    Case 13
                        Me.ChkSec13.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec13.Visible = True
                        Me.ChkSec13.BackColor = backColor
                    Case 14
                        Me.ChkSec14.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec14.Visible = True
                        Me.ChkSec14.BackColor = backColor
                    Case 15
                        Me.ChkSec15.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec15.Visible = True
                        Me.ChkSec15.BackColor = backColor
                    Case 16
                        Me.ChkSec16.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec16.Visible = True
                        Me.ChkSec16.BackColor = backColor
                    Case 17
                        Me.ChkSec17.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec17.Visible = True
                        Me.ChkSec17.BackColor = backColor
                    Case 18
                        Me.ChkSec18.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec18.Visible = True
                        Me.ChkSec18.BackColor = backColor
                    Case 19
                        Me.ChkSec19.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec19.Visible = True
                        Me.ChkSec19.BackColor = backColor
                    Case 20
                        Me.ChkSec20.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec20.Visible = True
                        Me.ChkSec20.BackColor = backColor
                    Case 21
                        Me.ChkSec21.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec21.Visible = True
                        Me.ChkSec21.BackColor = backColor
                    Case 22
                        Me.ChkSec22.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec22.Visible = True
                        Me.ChkSec22.BackColor = backColor
                    Case 23
                        Me.ChkSec23.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec23.Visible = True
                        Me.ChkSec23.BackColor = backColor
                    Case 24
                        Me.ChkSec24.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec24.Visible = True
                        Me.ChkSec24.BackColor = backColor
                    Case 25
                        Me.ChkSec25.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec25.Visible = True
                        Me.ChkSec25.BackColor = backColor
                    Case 26
                        Me.ChkSec26.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec26.Visible = True
                        Me.ChkSec26.BackColor = backColor
                    Case 27
                        Me.ChkSec27.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec27.Visible = True
                        Me.ChkSec27.BackColor = backColor
                    Case 28
                        Me.ChkSec28.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec28.Visible = True
                        Me.ChkSec28.BackColor = backColor
                    Case 29
                        Me.ChkSec29.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec29.Visible = True
                        Me.ChkSec29.BackColor = backColor
                    Case 30
                        Me.ChkSec30.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec30.Visible = True
                        Me.ChkSec30.BackColor = backColor
                    Case 31
                        Me.ChkSec31.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec31.Visible = True
                        Me.ChkSec31.BackColor = backColor
                    Case 32
                        Me.ChkSec32.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec32.Visible = True
                        Me.ChkSec32.BackColor = backColor
                End Select
            End If

        Next
    End Sub
    Private Sub cleanSecon()
        contadorAux = 0
        'Me.ChkPrim01.CheckState = 0
        'Me.ChkPrim02.CheckState = 0
        'Me.ChkPrim03.CheckState = 0
        'Me.ChkPrim04.CheckState = 0
        'Me.ChkPrim05.CheckState = 0
        'Me.ChkPrim06.CheckState = 0
        'Me.ChkPrim07.CheckState = 0
        'Me.ChkPrim08.CheckState = 0
        Me.ChkSec01.Visible = False
        Me.ChkSec02.Visible = False
        Me.ChkSec03.Visible = False
        Me.ChkSec04.Visible = False
        Me.ChkSec05.Visible = False
        Me.ChkSec06.Visible = False
        Me.ChkSec07.Visible = False
        Me.ChkSec08.Visible = False
        Me.ChkSec09.Visible = False
        Me.ChkSec10.Visible = False
        Me.ChkSec11.Visible = False
        Me.ChkSec12.Visible = False
        Me.ChkSec13.Visible = False
        Me.ChkSec14.Visible = False
        Me.ChkSec15.Visible = False
        Me.ChkSec16.Visible = False
        Me.ChkSec17.Visible = False
        Me.ChkSec18.Visible = False
        Me.ChkSec19.Visible = False
        Me.ChkSec20.Visible = False
        Me.ChkSec21.Visible = False
        Me.ChkSec22.Visible = False
        Me.ChkSec23.Visible = False
        Me.ChkSec24.Visible = False
        Me.ChkSec25.Visible = False
        Me.ChkSec26.Visible = False
        Me.ChkSec27.Visible = False
        Me.ChkSec28.Visible = False
        Me.ChkSec29.Visible = False
        Me.ChkSec30.Visible = False
        Me.ChkSec31.Visible = False
        Me.ChkSec32.Visible = False

        Me.ChkSec01.CheckState = 0
        Me.ChkSec02.CheckState = 0
        Me.ChkSec03.CheckState = 0
        Me.ChkSec04.CheckState = 0
        Me.ChkSec05.CheckState = 0
        Me.ChkSec06.CheckState = 0
        Me.ChkSec07.CheckState = 0
        Me.ChkSec08.CheckState = 0
        Me.ChkSec09.CheckState = 0
        Me.ChkSec10.CheckState = 0
        Me.ChkSec11.CheckState = 0
        Me.ChkSec12.CheckState = 0
        Me.ChkSec13.CheckState = 0
        Me.ChkSec14.CheckState = 0
        Me.ChkSec15.CheckState = 0
        Me.ChkSec16.CheckState = 0
        Me.ChkSec17.CheckState = 0
        Me.ChkSec18.CheckState = 0
        Me.ChkSec19.CheckState = 0
        Me.ChkSec20.CheckState = 0
        Me.ChkSec21.CheckState = 0
        Me.ChkSec22.CheckState = 0
        Me.ChkSec23.CheckState = 0
        Me.ChkSec24.CheckState = 0
        Me.ChkSec25.CheckState = 0
        Me.ChkSec26.CheckState = 0
        Me.ChkSec27.CheckState = 0
        Me.ChkSec28.CheckState = 0
        Me.ChkSec29.CheckState = 0
        Me.ChkSec30.CheckState = 0
        Me.ChkSec31.CheckState = 0
        Me.ChkSec32.CheckState = 0

        Me.ChkSec01.Text = ""
        Me.ChkSec02.Text = ""
        Me.ChkSec03.Text = ""
        Me.ChkSec04.Text = ""
        Me.ChkSec05.Text = ""
        Me.ChkSec06.Text = ""
        Me.ChkSec07.Text = ""
        Me.ChkSec08.Text = ""
        Me.ChkSec09.Text = ""
        Me.ChkSec10.Text = ""
        Me.ChkSec11.Text = ""
        Me.ChkSec12.Text = ""
        Me.ChkSec13.Text = ""
        Me.ChkSec14.Text = ""
        Me.ChkSec15.Text = ""
        Me.ChkSec16.Text = ""
        Me.ChkSec17.Text = ""
        Me.ChkSec18.Text = ""
        Me.ChkSec19.Text = ""
        Me.ChkSec20.Text = ""
        Me.ChkSec21.Text = ""
        Me.ChkSec22.Text = ""
        Me.ChkSec23.Text = ""
        Me.ChkSec24.Text = ""
        Me.ChkSec25.Text = ""
        Me.ChkSec26.Text = ""
        Me.ChkSec27.Text = ""
        Me.ChkSec28.Text = ""
        Me.ChkSec29.Text = ""
        Me.ChkSec30.Text = ""
        Me.ChkSec31.Text = ""
        Me.ChkSec32.Text = ""
    End Sub

    Private Sub ChkPrim01_CheckedChanged_1(sender As Object, e As EventArgs) Handles ChkPrim01.CheckedChanged

        'Me.ChkSec32.Text = Me.ChkSec01.Checked
        'Me.ChkSec32.Visible = True
    End Sub
    Function checkAllCell() As Boolean
        If Me.ChkPrim01.CheckState Then

        Else
            contadorAux = 0
        End If

        checkAllCell = False
    End Function




    Private Sub auxErase(ByVal Codigo As String)
        Dim indice_primario As Integer = contadorAux
        'Dim indice_secundario As Integer = contadorAux
        For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
            If DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Ticket") = Codigo Then

                Select Case indice_primario
                    Case 1
                        Me.ChkSec01.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec01.Visible = False
                    Case 2
                        Me.ChkSec02.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec02.Visible = False
                    Case 3
                        Me.ChkSec03.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec03.Visible = False
                    Case 4
                        Me.ChkSec04.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec04.Visible = False
                    Case 5
                        Me.ChkSec05.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec05.Visible = False
                    Case 6
                        Me.ChkSec06.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec06.Visible = False
                    Case 7
                        Me.ChkSec07.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec07.Visible = False
                    Case 8
                        Me.ChkSec08.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec08.Visible = False
                    Case 9
                        Me.ChkSec09.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec09.Visible = False
                    Case 10
                        Me.ChkSec10.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec10.Visible = False
                    Case 11
                        Me.ChkSec11.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec11.Visible = False
                    Case 12
                        Me.ChkSec12.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec12.Visible = False
                    Case 13
                        Me.ChkSec13.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec13.Visible = False
                    Case 14
                        Me.ChkSec14.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec14.Visible = False
                    Case 15
                        Me.ChkSec15.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec15.Visible = False
                    Case 16
                        Me.ChkSec16.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec16.Visible = False
                    Case 17
                        Me.ChkSec17.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec17.Visible = False
                    Case 18
                        Me.ChkSec18.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec18.Visible = False
                    Case 19
                        Me.ChkSec19.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec19.Visible = False
                    Case 20
                        Me.ChkSec20.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec20.Visible = False
                    Case 21
                        Me.ChkSec21.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec21.Visible = False
                    Case 22
                        Me.ChkSec22.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec22.Visible = False
                    Case 23
                        Me.ChkSec23.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec23.Visible = False
                    Case 24
                        Me.ChkSec24.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec24.Visible = False
                    Case 25
                        Me.ChkSec25.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec25.Visible = False
                    Case 26
                        Me.ChkSec26.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec26.Visible = False
                    Case 27
                        Me.ChkSec27.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec27.Visible = False
                    Case 28
                        Me.ChkSec28.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec28.Visible = False
                    Case 29
                        Me.ChkSec29.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec29.Visible = False
                    Case 30
                        Me.ChkSec30.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec30.Visible = False
                    Case 31
                        Me.ChkSec31.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec31.Visible = False
                    Case 32
                        Me.ChkSec32.Text = DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")
                        Me.ChkSec32.Visible = False
                End Select
                indice_primario -= 1
                contadorAux -= 1
            End If

        Next
    End Sub
    Private Sub uncheckFunc(p1 As String)
        posArray -= 1
        For Indice_Ips = 0 To 15
            If Me.arrayValues(Indice_Ips) = p1 Then
                Me.arrayValues(Indice_Ips) = ""
                setArrayChkSecond()
                'ChkSec10
            End If
        Next

    End Sub

    Private Sub setArrayChkSecond()
        Dim countAux As Integer = 0
        For Indice_Ips = 0 To 15
            If Me.arrayValues(Indice_Ips) = "" Then
            Else
                Me.arrayValuesAux(countAux) = Me.arrayValues(Indice_Ips)
                countAux += 1
            End If
            Me.arrayValues(Indice_Ips) = ""
        Next
        countAux = 0
        saveChckSec()
        cleanSecon()
        For Indice_Ips = 0 To 15
            If Me.arrayValuesAux(Indice_Ips) = "" Then
            Else
                Me.arrayValues(countAux) = Me.arrayValuesAux(Indice_Ips)
                aux(Me.arrayValues(countAux))
                countAux += 1
            End If
            Me.arrayValuesAux(Indice_Ips) = ""
        Next
        setCheckSecArray()
        For indice = 1 To 32
            Me.arrayCheck(indice) = ""
        Next
        countAux = 0
    End Sub

    Private Sub setArrayCheck()

        Dim indice_tramites As Integer = 0

        For indice = 1 To 32
            Select Case indice
                Case 1
                    If Me.ChkSec01.Visible = True Then
                        If Me.ChkSec01.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec01.Text
                        End If
                    End If
                Case 2
                    If Me.ChkSec02.Visible = True Then
                        If Me.ChkSec02.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec02.Text
                        End If
                    End If
                Case 3
                    If Me.ChkSec03.Visible = True Then
                        If Me.ChkSec03.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec03.Text
                        End If
                    End If
                Case 4
                    If Me.ChkSec04.Visible = True Then
                        If Me.ChkSec04.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec04.Text
                        End If
                    End If
                Case 5
                    If Me.ChkSec05.Visible = True Then
                        If Me.ChkSec05.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec05.Text
                        End If
                    End If
                Case 6
                    If Me.ChkSec06.Visible = True Then
                        If Me.ChkSec06.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec06.Text
                        End If
                    End If
                Case 7
                    If Me.ChkSec07.Visible = True Then
                        If Me.ChkSec07.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec07.Text
                        End If
                    End If
                Case 8
                    If Me.ChkSec08.Visible = True Then
                        If Me.ChkSec08.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec08.Text
                        End If
                    End If
                Case 9
                    If Me.ChkSec09.Visible = True Then
                        If Me.ChkSec09.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec09.Text
                        End If
                    End If
                Case 10
                    If Me.ChkSec10.Visible = True Then
                        If Me.ChkSec10.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec10.Text
                        End If
                    End If
                Case 11
                    If Me.ChkSec11.Visible = True Then
                        If Me.ChkSec11.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec11.Text
                        End If
                    End If
                Case 12
                    If Me.ChkSec12.Visible = True Then
                        If Me.ChkSec12.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec12.Text
                        End If
                    End If
                Case 13
                    If Me.ChkSec13.Visible = True Then
                        If Me.ChkSec13.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec13.Text
                        End If
                    End If
                Case 14
                    If Me.ChkSec14.Visible = True Then
                        If Me.ChkSec14.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec14.Text
                        End If
                    End If
                Case 15
                    If Me.ChkSec15.Visible = True Then
                        If Me.ChkSec15.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec15.Text
                        End If
                    End If
                Case 16
                    If Me.ChkSec16.Visible = True Then
                        If Me.ChkSec16.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec16.Text
                        End If
                    End If
                Case 17
                    If Me.ChkSec17.Visible = True Then
                        If Me.ChkSec17.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec17.Text
                        End If
                    End If
                Case 18
                    If Me.ChkSec18.Visible = True Then
                        If Me.ChkSec18.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec18.Text
                        End If
                    End If
                Case 19
                    If Me.ChkSec19.Visible = True Then
                        If Me.ChkSec19.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec19.Text
                        End If
                    End If
                Case 20
                    If Me.ChkSec20.Visible = True Then
                        If Me.ChkSec20.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec20.Text
                        End If
                    End If
                Case 21
                    If Me.ChkSec21.Visible = True Then
                        If Me.ChkSec21.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec21.Text
                        End If
                    End If
                Case 22
                    If Me.ChkSec22.Visible = True Then
                        If Me.ChkSec22.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec22.Text
                        End If
                    End If
                Case 23
                    If Me.ChkSec23.Visible = True Then
                        If Me.ChkSec23.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec23.Text
                        End If
                    End If
                Case 24
                    If Me.ChkSec24.Visible = True Then
                        If Me.ChkSec24.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec24.Text
                        End If
                    End If
                Case 25
                    If Me.ChkSec25.Visible = True Then
                        If Me.ChkSec25.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec25.Text
                        End If
                    End If
                Case 26
                    If Me.ChkSec26.Visible = True Then
                        If Me.ChkSec26.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec26.Text
                        End If
                    End If
                Case 27
                    If Me.ChkSec27.Visible = True Then
                        If Me.ChkSec27.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec27.Text
                        End If
                    End If
                Case 28
                    If Me.ChkSec28.Visible = True Then
                        If Me.ChkSec28.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec28.Text
                        End If
                    End If
                Case 29
                    If Me.ChkSec29.Visible = True Then
                        If Me.ChkSec29.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec29.Text
                        End If
                    End If
                Case 30
                    If Me.ChkSec30.Visible = True Then
                        If Me.ChkSec30.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec30.Text
                        End If
                    End If
                Case 31
                    If Me.ChkSec31.Visible = True Then
                        If Me.ChkSec31.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec31.Text
                        End If
                    End If
                Case 32
                    If Me.ChkSec32.Visible = True Then
                        If Me.ChkSec32.Checked = True Then
                            indice_tramites += 1
                            arrayCheck(indice_tramites) = Me.ChkSec32.Text
                        End If
                    End If
            End Select

        Next
    End Sub
    Private Function setCheck(p1 As String) As Boolean
        Dim vert As Boolean = False
        For indice = 1 To 32
            If arrayCheck(indice) = p1 Then
                vert = True
                Exit For
            End If
        Next
        Return vert
    End Function

    Private Sub saveChckSec()

        For indice = 1 To 32
            Me.arrayCheck(indice) = ""
        Next
        Dim indice_array As Integer = 0
        For indice = 1 To 32
            Select Case indice
                Case 1
                    If Me.ChkSec01.Visible = True Then
                        If Me.ChkSec01.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec01.Text
                        End If
                    End If
                Case 2
                    If Me.ChkSec02.Visible = True Then
                        If Me.ChkSec02.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec02.Text
                        End If
                    End If
                Case 3
                    If Me.ChkSec03.Visible = True Then
                        If Me.ChkSec03.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec03.Text
                        End If
                    End If
                Case 4
                    If Me.ChkSec04.Visible = True Then
                        If Me.ChkSec04.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec04.Text
                        End If
                    End If
                Case 5
                    If Me.ChkSec05.Visible = True Then
                        If Me.ChkSec05.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec05.Text
                        End If
                    End If
                Case 6
                    If Me.ChkSec06.Visible = True Then
                        If Me.ChkSec06.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec06.Text
                        End If
                    End If
                Case 7
                    If Me.ChkSec07.Visible = True Then
                        If Me.ChkSec07.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec07.Text
                        End If
                    End If
                Case 8
                    If Me.ChkSec08.Visible = True Then
                        If Me.ChkSec08.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec08.Text
                        End If
                    End If
                Case 9
                    If Me.ChkSec09.Visible = True Then
                        If Me.ChkSec09.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec09.Text
                        End If
                    End If
                Case 10
                    If Me.ChkSec10.Visible = True Then
                        If Me.ChkSec10.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec10.Text
                        End If
                    End If
                Case 11
                    If Me.ChkSec11.Visible = True Then
                        If Me.ChkSec11.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec11.Text
                        End If
                    End If
                Case 12
                    If Me.ChkSec12.Visible = True Then
                        If Me.ChkSec12.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec12.Text
                        End If
                    End If
                Case 13
                    If Me.ChkSec13.Visible = True Then
                        If Me.ChkSec13.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec13.Text
                        End If
                    End If
                Case 14
                    If Me.ChkSec14.Visible = True Then
                        If Me.ChkSec14.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec14.Text
                        End If
                    End If
                Case 15
                    If Me.ChkSec15.Visible = True Then
                        If Me.ChkSec15.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec15.Text
                        End If
                    End If
                Case 16
                    If Me.ChkSec16.Visible = True Then
                        If Me.ChkSec16.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec16.Text
                        End If
                    End If
                Case 17
                    If Me.ChkSec17.Visible = True Then
                        If Me.ChkSec17.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec17.Text
                        End If
                    End If
                Case 18
                    If Me.ChkSec18.Visible = True Then
                        If Me.ChkSec18.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec18.Text
                        End If
                    End If
                Case 19
                    If Me.ChkSec19.Visible = True Then
                        If Me.ChkSec19.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec19.Text
                        End If
                    End If
                Case 20
                    If Me.ChkSec20.Visible = True Then
                        If Me.ChkSec20.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec20.Text
                        End If
                    End If
                Case 21
                    If Me.ChkSec21.Visible = True Then
                        If Me.ChkSec21.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec21.Text
                        End If
                    End If
                Case 22
                    If Me.ChkSec22.Visible = True Then
                        If Me.ChkSec22.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec22.Text
                        End If
                    End If
                Case 23
                    If Me.ChkSec23.Visible = True Then
                        If Me.ChkSec23.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec23.Text
                        End If
                    End If
                Case 24
                    If Me.ChkSec24.Visible = True Then
                        If Me.ChkSec24.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec24.Text
                        End If
                    End If
                Case 25
                    If Me.ChkSec25.Visible = True Then
                        If Me.ChkSec25.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec25.Text
                        End If
                    End If
                Case 26
                    If Me.ChkSec26.Visible = True Then
                        If Me.ChkSec26.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec26.Text
                        End If
                    End If
                Case 27
                    If Me.ChkSec27.Visible = True Then
                        If Me.ChkSec27.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec27.Text
                        End If
                    End If
                Case 28
                    If Me.ChkSec28.Visible = True Then
                        If Me.ChkSec28.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec28.Text
                        End If
                    End If
                Case 29
                    If Me.ChkSec29.Visible = True Then
                        If Me.ChkSec29.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec29.Text
                        End If
                    End If
                Case 30
                    If Me.ChkSec30.Visible = True Then
                        If Me.ChkSec30.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec30.Text
                        End If
                    End If
                Case 31
                    If Me.ChkSec31.Visible = True Then
                        If Me.ChkSec31.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec31.Text
                        End If
                    End If
                Case 32
                    If Me.ChkSec32.Visible = True Then
                        If Me.ChkSec32.Checked = True Then
                            indice_array += 1
                            arrayCheck(indice_array) = Me.ChkSec32.Text
                        End If
                    End If
            End Select

        Next
    End Sub

    Private Sub setCheckSecArray()
        For indice = 1 To 32
            Select Case indice
                Case 1
                    If revisarCheck(Me.ChkSec01.Text) Then
                        Me.ChkSec01.CheckState = 1
                    End If
                Case 2
                    If revisarCheck(Me.ChkSec02.Text) Then
                        Me.ChkSec02.CheckState = 1
                    End If
                Case 3
                    If revisarCheck(Me.ChkSec03.Text) Then
                        Me.ChkSec03.CheckState = 1
                    End If
                Case 4
                    If revisarCheck(Me.ChkSec04.Text) Then
                        Me.ChkSec04.CheckState = 1
                    End If
                Case 5
                    If revisarCheck(Me.ChkSec05.Text) Then
                        Me.ChkSec05.CheckState = 1
                    End If
                Case 6
                    If revisarCheck(Me.ChkSec06.Text) Then
                        Me.ChkSec06.CheckState = 1
                    End If
                Case 7
                    If revisarCheck(Me.ChkSec07.Text) Then
                        Me.ChkSec07.CheckState = 1
                    End If
                Case 8
                    If revisarCheck(Me.ChkSec08.Text) Then
                        Me.ChkSec08.CheckState = 1
                    End If
                Case 9
                    If revisarCheck(Me.ChkSec09.Text) Then
                        Me.ChkSec09.CheckState = 1
                    End If
                Case 10
                    If revisarCheck(Me.ChkSec10.Text) Then
                        Me.ChkSec10.CheckState = 1
                    End If
                Case 11
                    If revisarCheck(Me.ChkSec11.Text) Then
                        Me.ChkSec11.CheckState = 1
                    End If
                Case 12
                    If revisarCheck(Me.ChkSec12.Text) Then
                        Me.ChkSec12.CheckState = 1
                    End If
                Case 13
                    If revisarCheck(Me.ChkSec13.Text) Then
                        Me.ChkSec13.CheckState = 1
                    End If
                Case 14
                    If revisarCheck(Me.ChkSec14.Text) Then
                        Me.ChkSec14.CheckState = 1
                    End If
                Case 15
                    If revisarCheck(Me.ChkSec15.Text) Then
                        Me.ChkSec15.CheckState = 1
                    End If
                Case 16
                    If revisarCheck(Me.ChkSec16.Text) Then
                        Me.ChkSec16.CheckState = 1
                    End If
                Case 17
                    If revisarCheck(Me.ChkSec17.Text) Then
                        Me.ChkSec17.CheckState = 1
                    End If
                Case 18
                    If revisarCheck(Me.ChkSec18.Text) Then
                        Me.ChkSec18.CheckState = 1
                    End If
                Case 19
                    If revisarCheck(Me.ChkSec19.Text) Then
                        Me.ChkSec19.CheckState = 1
                    End If
                Case 20
                    If revisarCheck(Me.ChkSec20.Text) Then
                        Me.ChkSec20.CheckState = 1
                    End If
                Case 21
                    If revisarCheck(Me.ChkSec21.Text) Then
                        Me.ChkSec21.CheckState = 1
                    End If
                Case 22
                    If revisarCheck(Me.ChkSec22.Text) Then
                        Me.ChkSec22.CheckState = 1
                    End If
                Case 23
                    If revisarCheck(Me.ChkSec23.Text) Then
                        Me.ChkSec23.CheckState = 1
                    End If
                Case 24
                    If revisarCheck(Me.ChkSec24.Text) Then
                        Me.ChkSec24.CheckState = 1
                    End If
                Case 25
                    If revisarCheck(Me.ChkSec25.Text) Then
                        Me.ChkSec25.CheckState = 1
                    End If
                Case 26
                    If revisarCheck(Me.ChkSec26.Text) Then
                        Me.ChkSec26.CheckState = 1
                    End If
                Case 27
                    If revisarCheck(Me.ChkSec27.Text) Then
                        Me.ChkSec27.CheckState = 1
                    End If
                Case 28
                    If revisarCheck(Me.ChkSec28.Text) Then
                        Me.ChkSec28.CheckState = 1
                    End If
                Case 29
                    If revisarCheck(Me.ChkSec29.Text) Then
                        Me.ChkSec29.CheckState = 1
                    End If
                Case 30
                    If revisarCheck(Me.ChkSec30.Text) Then
                        Me.ChkSec30.CheckState = 1
                    End If
                Case 31
                    If revisarCheck(Me.ChkSec31.Text) Then
                        Me.ChkSec31.CheckState = 1
                    End If
                Case 32
                    If revisarCheck(Me.ChkSec32.Text) Then
                        Me.ChkSec32.CheckState = 1
                    End If
            End Select

        Next
    End Sub

    Private Function revisarCheck(p1 As String) As Boolean
        Dim vert As Boolean = False
        For indice = 1 To 32
            If arrayCheck(indice) = p1 And arrayCheck(indice) <> "" Then
                vert = True
                Exit For
            End If
        Next
        Return vert
    End Function

    Private Sub btnGName_Click(sender As Object, e As EventArgs)
        Dim instruccion_insert As String = ""
        instruccion_insert = "Update Iq_Tickets Set IQTicket_NIT =   '" & txtNit1.Text & "' , IQTicket_Nombre = '" & txtName1.Text & "' where IQTicket_Area = '" & Area_Ticket & "' and IQTicket_Ticket = '" & Me.LblTicket.Text & "' And IQTicket_Estado = 'P' and IQTicket_Fecha = CONVERT(varchar(10), getdate(), 111)"
        Try
            Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
            IQ_Cnn.Open()
            Dim IQ_Cmm As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
            Dim RegistrosInsertados As Long = IQ_Cmm.ExecuteNonQuery()
            IQ_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show(Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        End Try
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        saveNitName()
    End Sub
    Private Sub saveNitName()
        If txtNit1.Text = "" And txtName1.Text = "" Then
            MessageBox.Show("No hay datos para guardar")
        Else
            Dim instruccion_insert As String = ""
            instruccion_insert = "Update Iq_Tickets Set IQTicket_NIT =   '" & txtNit1.Text & "' , IQTicket_Nombre = '" & txtName1.Text & "' where IQTicket_Area = '" & Area_Ticket & "' and IQTicket_Ticket = '" & Me.LblTicket.Text & "' And IQTicket_Estado = 'P' and IQTicket_Fecha = CONVERT(varchar(10), getdate(), 111)"
            Try
                Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
                IQ_Cnn.Open()
                Dim IQ_Cmm As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
                Dim RegistrosInsertados As Long = IQ_Cmm.ExecuteNonQuery()
                IQ_Cnn.Close()
                MessageBox.Show("Se Guardó de Manera Correcta el" + vbCr + "NIT:" + txtNit1.Text + vbCr + "Nombre:" + txtName1.Text)
            Catch exc As Exception
                Dim Mensaje_Excepcion As String
                Mensaje_Excepcion = exc.Message
                MessageBox.Show(Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Exit Sub
            End Try
        End If
    End Sub
    Private Sub saveNitNameNoMessage()
        If txtNit1.Text = "" And txtName1.Text = "" Then
            ' MessageBox.Show("No hay datos para guardar")
        Else
            Dim instruccion_insert As String = ""
            instruccion_insert = "Update Iq_Tickets Set IQTicket_NIT =   '" & txtNit1.Text & "' , IQTicket_Nombre = '" & txtName1.Text & "' where IQTicket_Area = '" & Area_Ticket & "' and IQTicket_Ticket = '" & Me.LblTicket.Text & "' And IQTicket_Estado = 'P' and IQTicket_Fecha = CONVERT(varchar(10), getdate(), 111)"
            Try
                Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
                IQ_Cnn.Open()
                Dim IQ_Cmm As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
                Dim RegistrosInsertados As Long = IQ_Cmm.ExecuteNonQuery()
                IQ_Cnn.Close()
                'MessageBox.Show("Se Guardó de Manera Correcta el" + vbCr + "NIT:" + txtNit1.Text + vbCr + "Nombre:" + txtName1.Text)
            Catch exc As Exception
                Dim Mensaje_Excepcion As String
                Mensaje_Excepcion = exc.Message
                MessageBox.Show(Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Exit Sub
            End Try
        End If
    End Sub
    Private Sub clear()
        txtNit1.Text = ""
        txtName1.Text = ""
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim s As New Registros
        s.recarga(Cnn_Central_Server, Computer_Code)
        s.Show()
    End Sub


    Private Sub btnPhone_Click(sender As Object, e As EventArgs) Handles btnPhone.Click
        Dim result As Integer = MessageBox.Show("¿Registrar Tramites de llamada?", "Registrar", MessageBoxButtons.YesNo)
        If result = DialogResult.Cancel Then
            MessageBox.Show("Cancel pressed")
        ElseIf result = DialogResult.No Then
        ElseIf result = DialogResult.Yes Then
            Proceso_AusenteLL()
            Carga_Tramites("SAC")
            toogleCall = 1
            disableCall()
        End If
       
    End Sub
    Private Sub Proceso_AusenteLL()
        Dim justificativo As String = ""
        Me.TimerWait.Enabled = False
        Me.TimerWait.Stop()
        ' justificativo = "6c7afada99e4|" & txtNit1.Text & "|" & txtName1.Text
        Do Until justificativo <> ""
            'justificativo = MsgBox("This information is on the first line. " & vbCrLf & "This information is on the 2nd line. " & vbCrLf & _ "Do you wish to continue?", vbYesNo + vbInformation, "Message Box")
            'InputBox("Ingrese por favor el Justificativo de su Ausencia (X CANCELA):", "", "")
            'InputBox("Ingrese por favor el Justificativo de su Ausencia (X CANCELA):", "", "")
            justificativo = "6c7afada99e4|" & txtNit1.Text & "|" & txtName1.Text
        Loop
        If UCase(justificativo) = "X" Then
            Me.TimerWait.Enabled = True
            Me.TimerWait.Start()
            Exit Sub
        End If
        If Me.PnlPrimario.Visible = True Then
            If Verifica_Tramites2() = False Then
                Exit Sub
            End If
            Graba_Tramites()
        End If
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "A"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text & "|" & justificativo
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        Me.Rojo.Visible = True
        Me.LblRojo.Visible = True
        Me.Amarillo.Visible = False
        Me.LblAmarillo.Visible = False
        Me.Verde.Visible = False
        Me.Lblverde.Visible = False
        Me.ButtonLibre.Visible = True
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.PnlPrimario.Visible = False
        nitDisable()
        Me.PnlSecundario.Visible = False
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        Me.TimerIdle.Enabled = False
        Me.TimerIdle.Stop()
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.ButtonAusente.Visible = False
        Me.ButtonEspera.Visible = False
        Me.ButtonNonShow.Visible = False
        Me.ButtonRedirect.Visible = False
        Me.LabelLibre.Visible = True
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = False
        Me.LabelEspera.Visible = False
        Me.LabelNonShow.Visible = False
        Me.LabelRedirect.Visible = False
        Me.LabelTicketAbajo.Visible = False
        Me.LblTicket.Visible = False
        Area_Ticket = ""
        Me.Lblverde.Visible = False
        Me.txtName1.Visible = False
        Me.txtNit1.Visible = False
        Me.Button1.Visible = False
        Me.lblNit.Visible = False
        Me.lblName.Visible = False
    End Sub

    Private Sub Graba_Tramites2()
        Dim indice_tramites As Integer = 0
        Dim tramite_a_grabar As String = ""
        Dim timeCall = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
        For indice = 1 To 16
            tramite_a_grabar = ""
            Select Case indice
                Case 1
                    If Me.ChkPrim01.Visible = True Then
                        If Me.ChkPrim01.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim01.Text
                            Exit For
                        End If
                    End If
                Case 2
                    If Me.ChkPrim02.Visible = True Then
                        If Me.ChkPrim02.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim02.Text
                            Exit For
                        End If
                    End If
                Case 3
                    If Me.ChkPrim03.Visible = True Then
                        If Me.ChkPrim03.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim03.Text
                            Exit For
                        End If
                    End If
                Case 4
                    If Me.ChkPrim04.Visible = True Then
                        If Me.ChkPrim04.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim04.Text
                            Exit For
                        End If
                    End If
                Case 5
                    If Me.ChkPrim05.Visible = True Then
                        If Me.ChkPrim05.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim05.Text
                            Exit For
                        End If
                    End If
                Case 6
                    If Me.ChkPrim06.Visible = True Then
                        If Me.ChkPrim06.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim06.Text
                            Exit For
                        End If
                    End If
                Case 7
                    If Me.ChkPrim07.Visible = True Then
                        If Me.ChkPrim07.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim07.Text
                            Exit For
                        End If
                    End If
                Case 8
                    If Me.ChkPrim08.Visible = True Then
                        If Me.ChkPrim08.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim08.Text
                            Exit For
                        End If
                    End If
                Case 9
                    If Me.ChkPrim09.Visible = True Then
                        If Me.ChkPrim09.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim09.Text
                            Exit For
                        End If
                    End If
                Case 10
                    If Me.ChkPrim10.Visible = True Then
                        If Me.ChkPrim10.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim10.Text
                            Exit For
                        End If
                    End If
                Case 11
                    If Me.ChkPrim11.Visible = True Then
                        If Me.ChkPrim11.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim11.Text
                            Exit For
                        End If
                    End If
                Case 12
                    If Me.ChkPrim12.Visible = True Then
                        If Me.ChkPrim12.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim12.Text
                            Exit For
                        End If
                    End If
                Case 13
                    If Me.ChkPrim13.Visible = True Then
                        If Me.ChkPrim13.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim13.Text
                            Exit For
                        End If
                    End If
                Case 14
                    If Me.ChkPrim14.Visible = True Then
                        If Me.ChkPrim14.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim14.Text
                            Exit For
                        End If
                    End If
                Case 15
                    If Me.ChkPrim15.Visible = True Then
                        If Me.ChkPrim15.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim15.Text
                            Exit For
                        End If
                    End If
                Case 16
                    If Me.ChkPrim16.Visible = True Then
                        If Me.ChkPrim16.Checked = True Then
                            indice_tramites = 1
                            tramite_a_grabar = Me.ChkPrim16.Text
                            Exit For
                        End If
                    End If
            End Select
        Next
        Dim instruccion_insert As String
        instruccion_insert = "insert into IQ_TickTram values ("
        instruccion_insert = instruccion_insert & "'" & Computer_Area & "', "
        instruccion_insert = instruccion_insert & "'" & "C-" & timeCall & "', "
        instruccion_insert = instruccion_insert & " '" & Format(DateTime.Today, "yyyy/MM/dd") & "', "
        instruccion_insert = instruccion_insert & CStr(indice_tramites) & ", "
        For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
            If Trim(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")) = Trim(tramite_a_grabar) Then
                instruccion_insert = instruccion_insert & CStr(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Codigo")) & ", "
            End If
        Next
        instruccion_insert = instruccion_insert & " '" & Computer_Code & "')"
        Dim IQ_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Try
            IQ_Cnn.Open()
            Dim IQ_Cmm2 As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
            Dim RegistrosInsertados As Long = IQ_Cmm2.ExecuteNonQuery()
            IQ_Cnn.Close()
        Catch ex As Exception
            IQ_Cnn.Close()
            ' MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Try
        End Try
        For indice = 1 To 32
            tramite_a_grabar = ""
            Select Case indice
                Case 1
                    If Me.ChkSec01.Visible = True Then
                        If Me.ChkSec01.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec01.Text
                        End If
                    End If
                Case 2
                    If Me.ChkSec02.Visible = True Then
                        If Me.ChkSec02.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec02.Text
                        End If
                    End If
                Case 3
                    If Me.ChkSec03.Visible = True Then
                        If Me.ChkSec03.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec03.Text
                        End If
                    End If
                Case 4
                    If Me.ChkSec04.Visible = True Then
                        If Me.ChkSec04.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec04.Text
                        End If
                    End If
                Case 5
                    If Me.ChkSec05.Visible = True Then
                        If Me.ChkSec05.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec05.Text
                        End If
                    End If
                Case 6
                    If Me.ChkSec06.Visible = True Then
                        If Me.ChkSec06.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec06.Text
                        End If
                    End If
                Case 7
                    If Me.ChkSec07.Visible = True Then
                        If Me.ChkSec07.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec07.Text
                        End If
                    End If
                Case 8
                    If Me.ChkSec08.Visible = True Then
                        If Me.ChkSec08.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec08.Text
                        End If
                    End If
                Case 9
                    If Me.ChkSec09.Visible = True Then
                        If Me.ChkSec09.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec09.Text
                        End If
                    End If
                Case 10
                    If Me.ChkSec10.Visible = True Then
                        If Me.ChkSec10.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec10.Text
                        End If
                    End If
                Case 11
                    If Me.ChkSec11.Visible = True Then
                        If Me.ChkSec11.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec11.Text
                        End If
                    End If
                Case 12
                    If Me.ChkSec12.Visible = True Then
                        If Me.ChkSec12.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec12.Text
                        End If
                    End If
                Case 13
                    If Me.ChkSec13.Visible = True Then
                        If Me.ChkSec13.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec13.Text
                        End If
                    End If
                Case 14
                    If Me.ChkSec14.Visible = True Then
                        If Me.ChkSec14.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec14.Text
                        End If
                    End If
                Case 15
                    If Me.ChkSec15.Visible = True Then
                        If Me.ChkSec15.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec15.Text
                        End If
                    End If
                Case 16
                    If Me.ChkSec16.Visible = True Then
                        If Me.ChkSec16.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec16.Text
                        End If
                    End If
                Case 17
                    If Me.ChkSec17.Visible = True Then
                        If Me.ChkSec17.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec17.Text
                        End If
                    End If
                Case 18
                    If Me.ChkSec18.Visible = True Then
                        If Me.ChkSec18.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec18.Text
                        End If
                    End If
                Case 19
                    If Me.ChkSec19.Visible = True Then
                        If Me.ChkSec19.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec19.Text
                        End If
                    End If
                Case 20
                    If Me.ChkSec20.Visible = True Then
                        If Me.ChkSec20.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec20.Text
                        End If
                    End If
                Case 21
                    If Me.ChkSec21.Visible = True Then
                        If Me.ChkSec21.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec21.Text
                        End If
                    End If
                Case 22
                    If Me.ChkSec22.Visible = True Then
                        If Me.ChkSec22.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec22.Text
                        End If
                    End If
                Case 23
                    If Me.ChkSec23.Visible = True Then
                        If Me.ChkSec23.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec23.Text
                        End If
                    End If
                Case 24
                    If Me.ChkSec24.Visible = True Then
                        If Me.ChkSec24.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec24.Text
                        End If
                    End If
                Case 25
                    If Me.ChkSec25.Visible = True Then
                        If Me.ChkSec25.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec25.Text
                        End If
                    End If
                Case 26
                    If Me.ChkSec26.Visible = True Then
                        If Me.ChkSec26.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec26.Text
                        End If
                    End If
                Case 27
                    If Me.ChkSec27.Visible = True Then
                        If Me.ChkSec27.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec27.Text
                        End If
                    End If
                Case 28
                    If Me.ChkSec28.Visible = True Then
                        If Me.ChkSec28.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec28.Text
                        End If
                    End If
                Case 29
                    If Me.ChkSec29.Visible = True Then
                        If Me.ChkSec29.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec29.Text
                        End If
                    End If
                Case 30
                    If Me.ChkSec30.Visible = True Then
                        If Me.ChkSec30.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec30.Text
                        End If
                    End If
                Case 31
                    If Me.ChkSec31.Visible = True Then
                        If Me.ChkSec31.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec31.Text
                        End If
                    End If
                Case 32
                    If Me.ChkSec32.Visible = True Then
                        If Me.ChkSec32.Checked = True Then
                            indice_tramites += 1
                            tramite_a_grabar = Me.ChkSec32.Text
                        End If
                    End If
            End Select
            If tramite_a_grabar <> "" Then
                instruccion_insert = "insert into IQ_TickTram values ("
                instruccion_insert = instruccion_insert & "'" & Computer_Area & "', "
                instruccion_insert = instruccion_insert & "'" & "C-" & timeCall & "', "
                instruccion_insert = instruccion_insert & " '" & Format(DateTime.Today, "yyyy/MM/dd") & "', "
                instruccion_insert = instruccion_insert & CStr(indice_tramites) & ", "
                For indice_busqueda = 0 To DsTramites.Tables("Iq_TipTram").Rows.Count - 1
                    If Trim(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Descripcion")) = Trim(tramite_a_grabar) Then
                        instruccion_insert = instruccion_insert & CStr(DsTramites.Tables("Iq_TipTram").Rows(indice_busqueda).Item("IqTipTram_Codigo")) & ", "
                    End If
                Next
                instruccion_insert = instruccion_insert & " '" & Computer_Code & "')"
                Try
                    IQ_Cnn.Open()
                    Dim IQ_Cmm2 As New OleDb.OleDbCommand(instruccion_insert, IQ_Cnn)
                    Dim RegistrosInsertados As Long = IQ_Cmm2.ExecuteNonQuery()
                    IQ_Cnn.Close()
                Catch ex As Exception
                    IQ_Cnn.Close()
                    '   MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                    Exit Try
                End Try
            End If
        Next
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs)
        Graba_Tramites2()
    End Sub
    Private Sub enableCall()
        Me.btnPhone.Enabled = True
        Me.LblRojo.Text = "AUSENTE"
    End Sub
    Private Sub disableCall()
        Me.btnPhone.Enabled = False
        Me.LblRojo.Text = "LLAMADA"
    End Sub
    Private Function Verifica_Tramites2() As Boolean
        Verifica_Tramites2 = True
        Dim num_primarios As Integer
        If Me.ChkSec01.Visible = True Then
            If Me.ChkSec01.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec02.Visible = True Then
            If Me.ChkSec02.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec03.Visible = True Then
            If Me.ChkSec03.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec04.Visible = True Then
            If Me.ChkSec04.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec05.Visible = True Then
            If Me.ChkSec05.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec06.Visible = True Then
            If Me.ChkSec06.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec07.Visible = True Then
            If Me.ChkSec07.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec08.Visible = True Then
            If Me.ChkSec08.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec09.Visible = True Then
            If Me.ChkSec09.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec10.Visible = True Then
            If Me.ChkSec10.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec11.Visible = True Then
            If Me.ChkSec11.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec12.Visible = True Then
            If Me.ChkSec12.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec13.Visible = True Then
            If Me.ChkSec13.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec14.Visible = True Then
            If Me.ChkSec14.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec15.Visible = True Then
            If Me.ChkSec15.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec16.Visible = True Then
            If Me.ChkSec16.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec17.Visible = True Then
            If Me.ChkSec17.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec18.Visible = True Then
            If Me.ChkSec18.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec19.Visible = True Then
            If Me.ChkSec19.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec20.Visible = True Then
            If Me.ChkSec20.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec21.Visible = True Then
            If Me.ChkSec21.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec22.Visible = True Then
            If Me.ChkSec22.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec23.Visible = True Then
            If Me.ChkSec23.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec24.Visible = True Then
            If Me.ChkSec24.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec25.Visible = True Then
            If Me.ChkSec25.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec26.Visible = True Then
            If Me.ChkSec26.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec27.Visible = True Then
            If Me.ChkSec27.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec28.Visible = True Then
            If Me.ChkSec28.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec29.Visible = True Then
            If Me.ChkSec29.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec30.Visible = True Then
            If Me.ChkSec30.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec31.Visible = True Then
            If Me.ChkSec31.Checked = True Then
                num_primarios += 1
            End If
        End If

        If Me.ChkSec32.Visible = True Then
            If Me.ChkSec32.Checked = True Then
                num_primarios += 1
            End If
        End If
        If num_primarios = 0 Then
            MessageBox.Show("DEBE SELECCIONAR POR LO MENOS UN TRAMITE SECUNDARIO EFECTUADO POR EL TICKET", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Verifica_Tramites2 = False
            Exit Function
        End If
        If num_primarios > 32 Then
            MessageBox.Show("NO PUEDE SELECCIONAR MAS DE 3 TRAMITES SECUNDARIO EFECTUADOS POR EL TICKET", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Verifica_Tramites2 = False
            Exit Function
        End If
    End Function
    Private Sub disablePhone()
        Me.btnPhone.Enabled = False
    End Sub
    Private Sub enablePhone()
        Me.btnPhone.Enabled = True
    End Sub

    Private Sub nitDisable()
        txtName1.Visible = False
        txtNit1.Visible = False
        lblNit.Visible = False
        lblName.Visible = False

    End Sub

    Private Sub nitEnable()
        txtName1.Visible = True
        txtNit1.Visible = True
        lblNit.Visible = True
        lblName.Visible = True
    End Sub
    Private Sub Proceso_Atender()
        disablePhone()
        Dim Central_Cnn As New OleDb.OleDbConnection(Cnn_Central_Server)
        Dim CmmCentral As New OleDb.OleDbCommand("", Central_Cnn)
        CmmCentral.CommandTimeout = 0
        CmmCentral.CommandType = CommandType.StoredProcedure
        CmmCentral.CommandText = "IQ_SpPlataforma"
        CmmCentral.Parameters.Add("CodStation", OleDbType.VarChar, 19).Value = Computer_Code
        CmmCentral.Parameters.Add("Station", OleDbType.VarChar, 6).Value = Computer_Sigla
        CmmCentral.Parameters.Add("Area", OleDbType.VarChar, 19).Value = Computer_Area
        CmmCentral.Parameters.Add("Action", OleDbType.VarChar, 1).Value = "O"
        CmmCentral.Parameters.Add("Parameter", OleDbType.VarChar, 100).Value = Me.LblTicket.Text
        CmmCentral.Parameters.Add("Area_Ticket", OleDbType.VarChar, 19).Value = Area_Ticket
        CmmCentral.Parameters.Add("Resultado", OleDbType.VarChar, 100).Direction = ParameterDirection.Output
        Dim resultado As String = ""
        Try
            Central_Cnn.Open()
            CmmCentral.ExecuteNonQuery()
            resultado = CmmCentral.Parameters("Resultado").Value
            Central_Cnn.Close()
        Catch exc As Exception
            Dim Mensaje_Excepcion As String
            Mensaje_Excepcion = exc.Message
            MessageBox.Show("Error Integrado: " + Mensaje_Excepcion, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End Try
        Me.Rojo.Visible = False
        Me.LblRojo.Visible = False
        Me.Amarillo.Visible = True
        Me.LblAmarillo.Visible = True
        Me.Verde.Visible = False
        Me.Lblverde.Visible = False
        Me.ButtonAtender.Visible = False
        Me.BtnBell.Visible = False
        Me.ButtonAusente.Visible = True
        Me.TimerIdle.Interval = 2400000
        Me.TimerIdle.Tag = "Atencion"
        Me.TimerWait.Enabled = False
        Me.TimerIdle.Stop()
        Me.TimerIdle.Enabled = True
        Me.TimerIdle.Start()
        Me.TimerSearch.Enabled = False
        Me.TimerSearch.Stop()
        'Carga_Tramites(Mid(Me.LblTicket.Text, 1, 3))
        Carga_Tramites("SAC")
        Me.ButtonEspera.Visible = True
        Me.ButtonLibre.Visible = True
        Me.ButtonNonShow.Visible = False
        Me.ButtonRedirect.Visible = True
        Me.ButtonRetorno.Visible = True
        Me.ButtonSalir.Visible = True
        Me.LstEspera.Visible = True
        Me.LstEspera.Enabled = True
        Me.LabelAtender.Visible = False
        Me.LabelAusente.Visible = True
        Me.LabelEspera.Visible = True
        Me.LabelLibre.Visible = True
        Me.LabelNonShow.Visible = False
        Me.LabelRedirect.Visible = True
        Me.LabelRetorno.Visible = True
        Me.LabelSalir.Visible = True
        disablePhone()
    End Sub

End Class




