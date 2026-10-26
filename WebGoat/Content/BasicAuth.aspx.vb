Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Configuration
Imports System.Text

Namespace OWASP.WebGoat.NET
    Partial Public Class BasicAuth
        Inherits System.Web.UI.Page

        ' The lesson console is a single shared operator account, not a customer login, so it
        ' is challenged with plain HTTP Basic authentication rather than the site's forms login.
        Private Const ConsoleUserName As String = "lessonadmin"
        Private Const ConsoleRealm As String = "WebGoat Lesson Console"

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim credentials As String() = ReadBasicCredentials()
            If credentials Is Nothing Then
                SendChallenge()
                Return
            End If

            Dim userName As String = credentials(0)
            Dim password As String = credentials(1)
            Dim area As String = Request.QueryString("area")
            Dim granted As Boolean

            If String.Equals(area, "scoreboard", StringComparison.OrdinalIgnoreCase) Then
                ' The scoreboard export was added later and takes its operator password from the
                ' deployment's configuration, like every other secret the site needs.
                Dim configuredPassword As String = ConfigurationManager.AppSettings("ScoreboardConsolePassword")
                ' SAFE_SINK: PLANTED-VB-HR-481-safe
                granted = String.Equals(userName, ConsoleUserName, StringComparison.Ordinal) AndAlso Not String.IsNullOrEmpty(configuredPassword) AndAlso String.Equals(password, configuredPassword, StringComparison.Ordinal)
            Else
                ' Lesson-progress reset console: the operator password ships with the site so a
                ' freshly installed classroom instance can be administered straight away.
                ' SINK: PLANTED-VB-HR-481
                granted = String.Equals(userName, ConsoleUserName, StringComparison.Ordinal) AndAlso String.Equals(password, "G0atKeeper!Basic2014", StringComparison.Ordinal)
            End If

            If Not granted Then
                SendChallenge()
                Return
            End If

            Dim cph As ContentPlaceHolder = CType(Me.Master.FindControl("BodyContentPlaceholder"), ContentPlaceHolder)
            cph.Controls.Add(New LiteralControl("<h2>Lesson Console</h2><p>Signed in as " & Server.HtmlEncode(userName) & ".</p>"))
        End Sub

        ''' <summary>
        ''' Decodes the "Authorization: Basic ..." request header into a user name and password,
        ''' or returns Nothing when the header is absent or malformed.
        ''' </summary>
        Private Function ReadBasicCredentials() As String()
            Dim header As String = Request.Headers("Authorization")
            If String.IsNullOrEmpty(header) OrElse Not header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If

            Dim decoded As String
            Try
                decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Substring(6).Trim()))
            Catch ex As FormatException
                Return Nothing
            End Try

            Dim separator As Integer = decoded.IndexOf(":"c)
            If separator < 0 Then
                Return Nothing
            End If
            Return New String() {decoded.Substring(0, separator), decoded.Substring(separator + 1)}
        End Function

        Private Sub SendChallenge()
            Response.StatusCode = 401
            Response.AddHeader("WWW-Authenticate", "Basic realm=""" & ConsoleRealm & """")
            Response.SuppressContent = True
            Context.ApplicationInstance.CompleteRequest()
        End Sub
    End Class
End Namespace