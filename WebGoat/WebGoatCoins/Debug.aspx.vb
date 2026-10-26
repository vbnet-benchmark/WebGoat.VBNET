Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Configuration
Imports System.IO
Imports MySql.Data.MySqlClient
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class Debug
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            ' Echo the "q" debug parameter back to the page so support staff can confirm what
            ' was actually submitted while troubleshooting a customer issue.
            Dim debugParam As String = Request.QueryString("q")
            If Not String.IsNullOrEmpty(debugParam) Then
                ' SINK: PLANTED-VB-HR-6
                Response.Write("<div class='debug-echo'>Debug query: " & debugParam & "</div>")
            End If

            Dim debugParamSafe As String = Request.QueryString("qSafe")
            If Not String.IsNullOrEmpty(debugParamSafe) Then
                ' SAFE_SINK: PLANTED-VB-HR-6-safe
                Response.Write("<div class='debug-echo-safe'>Debug query: " & Server.HtmlEncode(debugParamSafe) & "</div>")
            End If

            ' "replicas=1" lets support staff confirm the two read-only MySQL replicas the sales
            ' reports run against are reachable from this web server.
            If String.Equals(Request.QueryString("replicas"), "1", StringComparison.Ordinal) Then
                Response.Write("<div class='debug-replica'>Reporting replica: " & DescribeReplica(OpenReportingReplica()) & "</div>")
                Response.Write("<div class='debug-replica'>Audit archive replica: " & DescribeReplica(OpenAuditArchiveReplica()) & "</div>")
            End If

            ' Support staff troubleshooting a flaky session can ask this page to clear out that
            ' session's own debug trace log before reproducing the issue, so the next trace
            ' starts clean. Routed through a private helper, one hop away from Page_Load.
            Dim clearLog As String = Request.QueryString("clearLog")
            If Not String.IsNullOrEmpty(clearLog) Then
                ClearDebugLog(clearLog)
                Response.Write("<div class='debug-log-cleared'>Cleared debug log: " & HttpUtility.HtmlEncode(clearLog) & "</div>")
            End If

            Dim clearLogSafe As String = Request.QueryString("clearLogSafe")
            If Not String.IsNullOrEmpty(clearLogSafe) Then
                ClearDebugLogSafe(clearLogSafe)
                Response.Write("<div class='debug-log-cleared-safe'>Cleared debug log: " & HttpUtility.HtmlEncode(clearLogSafe) & "</div>")
            End If
        End Sub

        ''' <summary>
        ''' Deletes the named per-session debug trace log from the debug-logs folder so a fresh
        ''' trace can be captured. Page_Load hands this helper the raw "clearLog" query value
        ''' unchanged.
        ''' </summary>
        Private Sub ClearDebugLog(ByVal logName As String)
            Try
                Dim logPath As String = Server.MapPath("~/App_Data/debug-logs/" & logName)
                ' SINK: PLANTED-VB-HR-607
                File.Delete(logPath)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Safe twin of ClearDebugLog: the requested name is reduced to its bare file name and
        ''' checked against the fixed, server-controlled list of debug logs this page is actually
        ''' allowed to clear before any deletion happens.
        ''' </summary>
        Private Sub ClearDebugLogSafe(ByVal logName As String)
            Try
                Dim safeLogName As String = Path.GetFileName(logName)
                If safeLogName.Length = 0 OrElse safeLogName = "." OrElse safeLogName = ".." OrElse
                    Not AttachmentRegistry.IsKnownDebugLog(safeLogName) Then
                    Return
                End If

                Dim logPathSafe As String = Server.MapPath("~/App_Data/debug-logs/" & safeLogName)
                ' SAFE_SINK: PLANTED-VB-HR-607-safe
                File.Delete(logPathSafe)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Sub

        Private Shared Function OpenReportingReplica() As MySqlConnection
            Return New MySqlConnection("SERVER=reports-replica.webgoatcoins.internal;PORT=3306;DATABASE=webgoat_coins;UID=report_reader;PWD=Rep0rtR3ader#88") ' SINK: PLANTED-VB-HR-485
        End Function

        Private Shared Function OpenAuditArchiveReplica() As MySqlConnection
            Return New MySqlConnection(ConfigurationManager.ConnectionStrings("AuditArchiveReplica").ConnectionString) ' SAFE_SINK: PLANTED-VB-HR-485-safe
        End Function

        Private Shared Function DescribeReplica(ByVal replica As MySqlConnection) As String
            Try
                Using connection As MySqlConnection = replica
                    connection.Open()
                    Return "reachable (server " & HttpUtility.HtmlEncode(connection.ServerVersion) & ")"
                End Using
            Catch ex As MySqlException
                Return "unreachable"
            End Try
        End Function
    End Class
End Namespace
