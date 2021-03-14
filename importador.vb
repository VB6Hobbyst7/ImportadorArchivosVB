Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Mail

Public Class Importador

    Public host As String = "#####"
    Public pathDestino As String = "C:\carpeta o dirección de red Windows"

    Public pathDestinoBackup As String = "C:\backup o dirección de red Windows"
    

    Public username As String = "user"
    Public password As String = "password"

    Dim anio = Date.Now.Year.ToString
    Dim mesActual = Date.Now.Month
    Dim mesLetra As String = String.Empty
    Dim ejecutarPagos As Boolean = False
    Dim envioMail As Boolean = False
    Dim pMensajeMail As String = String.Empty
#Region "Métodos"

    Private Sub Importar()
        Dim arcsBk = System.IO.Directory.GetFiles(pathDestinoBackup)
        Dim archivosBackup As New ArrayList
        Dim archivosEliminar As New ArrayList

        For Each arBk As String In arcsBk
            archivosBackup.Add(Replace(arBk, pathDestinoBackup + "\", "/"))
        Next

        Dim Requiere As FtpWebRequest = WebRequest.Create(host)
        Requiere.Method = WebRequestMethods.Ftp.ListDirectory

        Dim cred As New Net.NetworkCredential(username, password)
        Requiere.Credentials = cred

        Dim respuesta As FtpWebResponse = Requiere.GetResponse

        Dim Sr As New StreamReader(respuesta.GetResponseStream)
        Dim lst = Sr.ReadToEnd

        Dim Archivos() As String

        'Desgloza la cadena y la almacena en un vector  
        Archivos = Split(lst, vbCrLf)
        lstVisor.Items.Add("Procesando.." + Archivos.Count.ToString)
        For Each nomArchivo As String In Archivos

            If nomArchivo <> "" Then
                'Elimino la palabra Credifiar (nombre de la carpeta) del nombre del archivo
                nomArchivo = Mid(nomArchivo, 10, Len(nomArchivo))
                lstVisor.Items.Add("Archivo: " + nomArchivo)

                If Not archivosBackup.Contains(nomArchivo) Then
                    Dim request As New WebClient()
                    request.Credentials = New NetworkCredential(username, password)
                    'Read the file data into a Byte array
                    Dim bytes() As Byte = request.DownloadData(host + nomArchivo)
                    'Create a FileStream to read the file into
                    Dim carpetaDestino As String = Me.BuscarCarpeta(nomArchivo)
                    lstVisor.Items.Add("Archivo no repetido: " + nomArchivo)
                    'Grabo archivo en carpeta Backup (Mientras no sea el archivo RUBRO y PAGPRE)
                    
                    If Not nomArchivo.ToUpper.Contains("RUBRO") And Not nomArchivo.ToUpper.Contains("PAGPRE") Then

                        Dim DownloadStreamBackup As FileStream = IO.File.Create(pathDestinoBackup + nomArchivo)
                        DownloadStreamBackup.Write(bytes, 0, bytes.Length)
                        DownloadStreamBackup.Close()

                        'Grabo archivo en carpeta destino
                        Dim DownloadStream As FileStream = IO.File.Create(pathDestino + carpetaDestino + nomArchivo)
                        DownloadStream.Write(bytes, 0, bytes.Length)
                        DownloadStream.Close()

                        'listo archivos en listbox
                        lstVisor.Items.Add(pathDestino + carpetaDestino + nomArchivo)
                        archivosEliminar.Add(nomArchivo)

                    End If

                End If

            End If
        Next
        lstVisor.Items.Add("Fin de guardado.")
        lstVisor.Items.Add("Eliminando " + archivosEliminar.Count.ToString + " archivos..")
        Me.EliminarArchivos(archivosEliminar)
        lstVisor.Items.Add("Fin de proceso!")

    End Sub

    Private Function BuscarCarpeta(nombreArc As String)
        Dim nomCarpeta As String = anio + "/otros"

        'Obtengo la posición del "."
        'Busco el mes al que corresponde el archivo
        Dim mes As String = String.Empty
        Dim hasta = 0

        If nombreArc.Contains(".") Then
            hasta = InStr(nombreArc, ".")
            mes = Mid(nombreArc, hasta - 1, 1)
        Else
            mes = "otros"
        End If

        If nombreArc.ToUpper.Contains("AJUPRE") Then
            mes = "otros"
        End If

        Select Case mes.ToUpper
            Case "E"
                mes = "enero"
            Case "F"
                mes = "febrero"
            Case "M"
                mes = "marzo"
            Case "A"
                mes = "abril"
            Case "Y"
                mes = "mayo"
            Case "J"
                mes = "junio"
            Case "L"
                mes = "julio"
            Case "G"
                mes = "agosto"
            Case "S"
                mes = "septiembre"
            Case "O"
                mes = "octubre"
            Case "N"
                mes = "noviembre"
            Case "D"
                mes = "diciembre"
            Case Else
                mes = "otros"
        End Select

        If mes = "otros" Then
            ' Sólo para archivos LIQ que no tienen el mes en letras
            If nombreArc.ToUpper.Contains("POSLIQ") Then

                nomCarpeta = anio + "/otros"

            ElseIf nombreArc.ToUpper.Contains("LIQ") Then

                nomCarpeta = Me.ObtenerCarpetaLiqComercios()
                envioMail = True
                pMensajeMail = "Liquidaciones de Comercio "
            ElseIf nombreArc.ToUpper.Contains("ESTADISTICAS CREDIFIAR") Then

                Dim anioArc As String = Mid(nombreArc, hasta - 6, 4)
                Dim mesArc As String = Me.ObtenerMesPorNRO(Mid(nombreArc, hasta - 2, 2))
                nomCarpeta = anioArc + "/" + mesArc

            ElseIf nombreArc.ToUpper.Contains("IBSF") Or nombreArc.ToUpper.Contains("SUJETOS") Or nombreArc.ToUpper.Contains("SIC") Or nombreArc.ToUpper.Contains("IB") Then

                Dim mesArc As String = Me.ObtenerMesPorNRO(Date.Now.Month)
                nomCarpeta = anio + "/" + mesArc + "/impuestos"

            ElseIf nombreArc.ToUpper.Contains(".ZIP") Then

                Dim mesArc As String = Me.ObtenerMesPorNRO(Date.Now.Month)
                nomCarpeta = anio + "/" + mesArc + "/liq usuarios"
                envioMail = True
                pMensajeMail = "Liquidaciones de Usuarios "

            ElseIf nombreArc.ToUpper.Contains("RUBRO") Then

                'creo carpeta del DÍA actual en que se graba el archivo
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/otros/" + Date.Now.Day.ToString)
                nomCarpeta = anio + "/otros/" + Date.Now.Day.ToString

            Else
                nomCarpeta = anio + "/otros"
            End If

        Else

            If nombreArc.ToUpper.Contains("ADE") Then
                Dim subC = Mid(nombreArc, 5, 1)
                Select Case CInt(subC)
                    Case 1
                        nomCarpeta = anio + "/" + mes + "/adelantos/total"
                    Case 2
                        nomCarpeta = anio + "/" + mes + "/adelantos/detalle"
                    Case 3
                        nomCarpeta = anio + "/" + mes + "/adelantos/pagadora"
                    Case Else
                        nomCarpeta = anio + "/otros"
                End Select

            ElseIf nombreArc.ToUpper.Contains("ATM") Then
                Dim subC = Mid(nombreArc, 5, 1)
                Select Case CInt(subC)
                    Case 1
                        nomCarpeta = anio + "/" + mes + "/adelantos cajero/total"
                    Case 2
                        nomCarpeta = anio + "/" + mes + "/adelantos cajero/resumen"
                    Case Else
                        nomCarpeta = anio + "/otros"
                End Select

            ElseIf nombreArc.ToUpper.Contains("AJU") Then
                Dim subC = Mid(nombreArc, 5, 1)
                Select Case CInt(subC)
                    Case 1
                        nomCarpeta = anio + "/" + mes + "/ajustes/totales"
                    Case 2
                        nomCarpeta = anio + "/" + mes + "/ajustes/detalle"
                    Case Else
                        nomCarpeta = anio + "/otros"
                End Select

            ElseIf nombreArc.ToUpper.Contains("BBAJA") Then

                nomCarpeta = anio + "/" + mes + "/boletín/baja automática"

            ElseIf nombreArc.ToUpper.Contains("BINC") Then

                nomCarpeta = anio + "/" + mes + "/boletín/inclusión automática"

            ElseIf nombreArc.ToUpper.Contains("IBSF") Or nombreArc.ToUpper.Contains("SUJETOS") Or nombreArc.ToUpper.Contains("SIC") Or nombreArc.ToUpper.Contains("IB") Then

                nomCarpeta = anio + "/" + mes + "/impuestos"

            ElseIf nombreArc.ToUpper.Contains("LINK") Then

                nomCarpeta = anio + "/" + mes + "/link"

            ElseIf nombreArc.ToUpper.Contains("LIQ") Or nombreArc.ToUpper.Contains("ECS") Or nombreArc.ToUpper.Contains("SL") Then

                nomCarpeta = Me.ObtenerCarpetaLiqComercios

            ElseIf nombreArc.ToUpper.Contains("PAGPRE") Then

                nomCarpeta = anio + "/otros"
                ejecutarPagos = False

            ElseIf nombreArc.ToUpper.Contains("PAG") Then

                nomCarpeta = anio + "/" + mes + "/pagos usuarios"

            ElseIf nombreArc.ToUpper.Contains("PRES") Then

                nomCarpeta = anio + "/" + mes + "/prestamos"

            ElseIf nombreArc.ToUpper.Contains("PRE") Then

                nomCarpeta = anio + "/" + mes + "/prestamos"

            ElseIf nombreArc.ToUpper.Contains(".ZIP") Then

                nomCarpeta = anio + "/" + mes + "/liq usuarios"

            Else

                nomCarpeta = anio + "/otros"

            End If
        End If

        Return nomCarpeta
    End Function

    Private Function ObtenerCarpetaLiqComercios()
        Dim diaNro As Integer = Date.Now.Day
        Dim mesNro As Integer = Date.Now.Month
        Dim anioNroStr As String = anio.ToString
        Dim carpeta As String = "/liq comercios"
        Dim nomCarpeta As String = anio.ToString + "/otros"

        If diaNro > 1 And diaNro < 7 Then

            If mesNro = 1 Then

                anioNroStr = CType(Date.Now.Year - 1, String)
                mesNro = 12

            Else

                mesNro = Date.Now.Month - 1

            End If

            diaNro = 30

        End If

        Dim mesEnLetras = Me.ObtenerMesPorNRO(mesNro)

        If String.IsNullOrEmpty(mesEnLetras) Then

            nomCarpeta = anioNroStr + "/otros"

        Else

            Dim carpetaMesLiqComercio As New DirectoryInfo(pathDestino + anioNroStr + "/" + mesEnLetras + "/liq comercios")
            Dim listaSubCarpetas As New List(Of String)
            Dim existeCarp As Boolean = False

            'listar las carpetas
            For Each dir As DirectoryInfo In carpetaMesLiqComercio.GetDirectories

                If dir.Name = Date.Now.Day.ToString Then
                    existeCarp = True
                End If

            Next

            If Not existeCarp Then
                'creo carpeta del DÍA actual en que se graba el archivo
                System.IO.Directory.CreateDirectory(pathDestino + anioNroStr + "/" + mesEnLetras + "/liq comercios/" + diaNro.ToString)
            End If

            nomCarpeta = anioNroStr + "/" + mesEnLetras + "/liq comercios/" + diaNro.ToString

        End If

        Return nomCarpeta
    End Function

    Private Function ObtenerMesPorNRO(mes)
        Select Case mes
            Case 1
                mesLetra = "enero"
            Case 2
                mesLetra = "febrero"
            Case 3
                mesLetra = "marzo"
            Case 4
                mesLetra = "abril"
            Case 5
                mesLetra = "mayo"
            Case 6
                mesLetra = "junio"
            Case 7
                mesLetra = "julio"
            Case 8
                mesLetra = "agosto"
            Case 9
                mesLetra = "septiembre"
            Case 10
                mesLetra = "octubre"
            Case 11
                mesLetra = "noviembre"
            Case 12
                mesLetra = "diciembre"
        End Select

        Return mesLetra
    End Function

    Private Sub VerificarCarpetas()

        Dim existeAnio = System.IO.Directory.Exists(pathDestino + anio)

        Me.ObtenerMesPorNRO(mesActual)

        If existeAnio Then

            Dim existeMes = System.IO.Directory.Exists(pathDestino + anio + "/" + mesLetra)

            If existeMes Then
                ' si existe la carpeta, controlo que existan todas las subcarpetas

                Dim carpetaMes As New DirectoryInfo(pathDestino + anio + "/" + mesLetra)

                Dim listaCarpetas As New List(Of String)

                'listar las carpetas
                For Each dir As DirectoryInfo In carpetaMes.GetDirectories
                    listaCarpetas.Add(dir.Name)
                Next

                If Not listaCarpetas.Contains("adelantos") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/total")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/detalle")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/pagadora")
                Else
                    Dim carpetaMesAdelantos As New DirectoryInfo(pathDestino + anio + "/" + mesLetra + "/adelantos")
                    Dim listaSubCarpetas As New List(Of String)

                    'listar las carpetas
                    For Each dir As DirectoryInfo In carpetaMesAdelantos.GetDirectories
                        listaSubCarpetas.Add(dir.Name)
                    Next

                    If Not listaSubCarpetas.Contains("total") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/total")
                    End If

                    If Not listaSubCarpetas.Contains("detalle") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/detalle")
                    End If

                    If Not listaSubCarpetas.Contains("pagadora") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/pagadora")
                    End If
                End If

                If Not listaCarpetas.Contains("adelantos cajero") Then

                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/total")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/resumen")

                Else
                    Dim carpetaMesAdelantosCajero As New DirectoryInfo(pathDestino + anio + "/" + mesLetra + "/adelantos cajero")
                    Dim listaSubCarpetas As New List(Of String)

                    'listar las carpetas
                    For Each dir As DirectoryInfo In carpetaMesAdelantosCajero.GetDirectories
                        listaSubCarpetas.Add(dir.Name)
                    Next

                    If Not listaSubCarpetas.Contains("total") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/total")
                    End If

                    If Not listaSubCarpetas.Contains("resumen") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/resumen")
                    End If

                End If

                If Not listaCarpetas.Contains("ajustes") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/detalle")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/totales")
                Else
                    Dim carpetaMesAjustes As New DirectoryInfo(pathDestino + anio + "/" + mesLetra + "/ajustes")
                    Dim listaSubCarpetas As New List(Of String)

                    'listar las carpetas
                    For Each dir As DirectoryInfo In carpetaMesAjustes.GetDirectories
                        listaSubCarpetas.Add(dir.Name)
                    Next

                    If Not listaSubCarpetas.Contains("detalle") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/detalle")
                    End If

                    If Not listaSubCarpetas.Contains("totales") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/totales")
                    End If
                End If

                If Not listaCarpetas.Contains("boletín") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/inclusión automática")
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/baja automática")
                Else
                    Dim carpetaMesBoletin As New DirectoryInfo(pathDestino + anio + "/" + mesLetra + "/boletín")
                    Dim listaSubCarpetas As New List(Of String)

                    'listar las carpetas
                    For Each dir As DirectoryInfo In carpetaMesBoletin.GetDirectories
                        listaSubCarpetas.Add(dir.Name)
                    Next

                    If Not listaSubCarpetas.Contains("inclusión automática") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/inclusión automática")
                    End If

                    If Not listaSubCarpetas.Contains("baja automática") Then
                        System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/baja automática")
                    End If
                End If

                If Not listaCarpetas.Contains("impuestos") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/impuestos")
                End If
                If Not listaCarpetas.Contains("link") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/link")
                End If

                If Not listaCarpetas.Contains("liq comercios") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq comercios")
                End If

                If Not listaCarpetas.Contains("liq usuarios") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq usuarios")
                End If

                If Not listaCarpetas.Contains("pagos usuarios") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/pagos usuarios")
                End If

                If Not listaCarpetas.Contains("prestamos") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/prestamos")
                End If

                If Not System.IO.Directory.Exists(pathDestino + anio + "/otros") Then
                    System.IO.Directory.CreateDirectory(pathDestino + anio + "/otros")
                End If
            Else
                ' si no existe la carpeta del mes, creo esa carpeta con las subcarpetas para los archivos
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra)

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/total")
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/detalle")
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/pagadora")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/resumen")
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/total")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/detalle")
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/totales")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/inclusión automática")
                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/baja automática")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/impuestos")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/link")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq comercios")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq usuarios")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/pagos usuarios")

                System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/prestamos")
            End If
        Else
            'si no existe el año, creo esa carpeta, la carpeta OTROS y la del mes con las subcarpetas del mes.
            System.IO.Directory.CreateDirectory(pathDestino + anio)

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + "otros")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra)

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/total")
            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/detalle")
            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos/pagadora")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/resumen")
            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/adelantos cajero/total")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/detalle")
            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/ajustes/totales")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/inclusión automática")
            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/boletín/baja automática")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/impuestos")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/link")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq comercios")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/liq usuarios")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/pagos usuarios")

            System.IO.Directory.CreateDirectory(pathDestino + anio + "/" + mesLetra + "/prestamos")

        End If
    End Sub

    Private Sub EliminarArchivos(listaArchivos As ArrayList)

        For Each nombreArchivo As String In listaArchivos
            Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create(host & nombreArchivo), System.Net.FtpWebRequest)

            FTPRequest.Credentials = New System.Net.NetworkCredential(username, password)
            FTPRequest.Method = System.Net.WebRequestMethods.Ftp.DeleteFile
            Dim FTPDelResp As FtpWebResponse = FTPRequest.GetResponse
        Next

    End Sub

    Public Sub EnviarMail()

        Dim correo As New MailMessage
        correo.From = New MailAddress("mail@credifiar.com.ar")
        correo.To.Add("mail@credifiar.com.ar")
        correo.Subject = pMensajeMail
        correo.Body = pMensajeMail + " disponibles para ser procesadas. "
        correo.IsBodyHtml = False
        correo.Priority = MailPriority.Normal

        Dim smtp As New SmtpClient()
        smtp.Host = "smtp.gmail.com"
        smtp.Port = 587
        smtp.Credentials = New System.Net.NetworkCredential("mail@credifiar.com.ar", "pasword")
        smtp.EnableSsl = True

        Try
            smtp.Send(correo)
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Eventos"

    Private Sub Importador_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Try
            Me.VerificarCarpetas()
            Me.Importar()

            'If ejecutarPagos Then
            '    lstVisor.Items.Add("Ejecutando Automatización de Pagos..")
            '    System.Diagnostics.Process.Start(pathAutomatizacionPagos)
            '    lstVisor.Items.Add("Fin de proceso de Automatización de Pagos!")
            'End If

            If envioMail Then
                Me.EnviarMail()
            End If

            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub btnImportar_Click(sender As System.Object, e As System.EventArgs) Handles btnImportar.Click
        Try
            Me.VerificarCarpetas()
            Me.Importar()

            'If Me.ejecutarPagos Then
            '    lstVisor.Items.Add("Ejecutando Automatización de Pagos..")
            '    System.Diagnostics.Process.Start(pathAutomatizacionPagos)
            '    lstVisor.Items.Add("Fin de proceso de Automatización de Pagos!")
            'End If

            If envioMail Then
                Me.EnviarMail()
            End If
            'Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

#End Region


End Class

