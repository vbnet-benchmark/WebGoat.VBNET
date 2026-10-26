Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Diagnostics
Imports System.Text
Imports System.Text.RegularExpressions
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Public Partial Class CommandInjection
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' "Ping a Host" -- direct tier. The submitted host is appended straight onto the ping
        ''' utility's argument string and the process is launched with no validation at all.
        ''' </summary>
        Protected Sub btnPing_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim startInfo As New ProcessStartInfo With {
                .FileName = "ping",
                .Arguments = "-n 4 " & txtPingHost.Text,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .CreateNoWindow = True
            }

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SINK: PLANTED-VB-HR-511
                proc.WaitForExit(5000)
            End Using

            lblPingResult.Text = "<div class='tiny'>Ping started for " & HttpUtility.HtmlEncode(txtPingHost.Text) & ".</div>"
        End Sub

        ''' <summary>
        ''' Safe twin of btnPing_Click: the host must look like a plain hostname or IP address
        ''' (letters, digits, dots and hyphens only) before the ping utility is ever launched.
        ''' </summary>
        Protected Sub btnPingSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim host As String = txtPingHostSafe.Text
            If host Is Nothing OrElse Not Regex.IsMatch(host, "^[A-Za-z0-9.-]+$") Then
                lblPingResultSafe.Text = "<div class='failure'>Enter a plain hostname or IP address (letters, digits, dots and hyphens only).</div>"
                Return
            End If

            Dim startInfo As New ProcessStartInfo With {
                .FileName = "ping",
                .Arguments = "-n 4 " & host,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .CreateNoWindow = True
            }

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SAFE_SINK: PLANTED-VB-HR-511-safe
                proc.WaitForExit(5000)
            End Using

            lblPingResultSafe.Text = "<div class='tiny'>Ping started for " & HttpUtility.HtmlEncode(host) & ".</div>"
        End Sub

        ''' <summary>
        ''' Indirect tier: the "Trace Route" button hands the submitted host to this same-class
        ''' helper, one hop away from the button handler, which is what actually launches the
        ''' trace-route utility with the host appended to its argument string unsanitized.
        ''' </summary>
        Private Sub RunTraceroute(ByVal host As String)
            Dim startInfo As New ProcessStartInfo With {
                .FileName = "tracert",
                .Arguments = "-h 15 " & host,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .CreateNoWindow = True
            }

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SINK: PLANTED-VB-HR-512
                proc.WaitForExit(5000)
            End Using
        End Sub

        Protected Sub btnTrace_Click(ByVal sender As Object, ByVal e As EventArgs)
            RunTraceroute(txtTraceHost.Text)
            lblTraceResult.Text = "<div class='tiny'>Trace route started for " & HttpUtility.HtmlEncode(txtTraceHost.Text) & ".</div>"
        End Sub

        ''' <summary>
        ''' Safe twin of RunTraceroute: the host must look like a plain hostname or IP address
        ''' before the trace-route utility is ever launched.
        ''' </summary>
        Private Sub RunTracerouteSafe(ByVal host As String)
            If host Is Nothing OrElse Not Regex.IsMatch(host, "^[A-Za-z0-9.-]+$") Then
                Return
            End If

            Dim startInfo As New ProcessStartInfo With {
                .FileName = "tracert",
                .Arguments = "-h 15 " & host,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .CreateNoWindow = True
            }

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SAFE_SINK: PLANTED-VB-HR-512-safe
                proc.WaitForExit(5000)
            End Using
        End Sub

        Protected Sub btnTraceSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim host As String = txtTraceHostSafe.Text
            If host Is Nothing OrElse Not Regex.IsMatch(host, "^[A-Za-z0-9.-]+$") Then
                lblTraceResultSafe.Text = "<div class='failure'>Enter a plain hostname or IP address (letters, digits, dots and hyphens only).</div>"
                Return
            End If
            RunTracerouteSafe(host)
            lblTraceResultSafe.Text = "<div class='tiny'>Trace route started for " & HttpUtility.HtmlEncode(host) & ".</div>"
        End Sub

        ''' <summary>
        ''' Type/polymorphism-dependent tier (genuine): "Resolve Host" picks which
        ''' App_Code/DiagnosticToolInvoker.vb backend builds the resolver's argument string based
        ''' on the "Lookup Backend" dropdown. LegacyLookupInvoker forwards the host unmodified;
        ''' ValidatedLookupInvoker is this same feature's hardened replacement.
        ''' </summary>
        Protected Sub btnLookup_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim host As String = txtLookupHost.Text

            If String.Equals(ddlLookupBackend.Text, "Validated", StringComparison.OrdinalIgnoreCase) Then
                Dim invoker As DiagnosticToolInvoker = New ValidatedLookupInvoker()
                Dim startInfo As New ProcessStartInfo With {
                    .FileName = "nslookup",
                    .Arguments = invoker.BuildLookupArguments(host),
                    .UseShellExecute = False,
                    .RedirectStandardOutput = True,
                    .CreateNoWindow = True
                }

                Using proc As New Process()
                    proc.StartInfo = startInfo
                    proc.Start() ' SAFE_SINK: PLANTED-VB-HR-514-safe
                    proc.WaitForExit(3000)
                End Using
            Else
                Dim invoker As DiagnosticToolInvoker = New LegacyLookupInvoker()
                Dim startInfo As New ProcessStartInfo With {
                    .FileName = "nslookup",
                    .Arguments = invoker.BuildLookupArguments(host),
                    .UseShellExecute = False,
                    .RedirectStandardOutput = True,
                    .CreateNoWindow = True
                }

                Using proc As New Process()
                    proc.StartInfo = startInfo
                    proc.Start() ' SINK: PLANTED-VB-HR-514
                    proc.WaitForExit(3000)
                End Using
            End If

            lblLookupResult.Text = "<div class='tiny'>Lookup started for " & HttpUtility.HtmlEncode(host) & ".</div>"
        End Sub

        ''' <summary>
        ''' Diversity tier: "Ping Multiple Hosts" uses a genuinely different shape from the single-
        ''' host tools above -- a batch loop that launches one ping process per entry in a
        ''' submitted list, each one built from that entry with no validation.
        ''' </summary>
        Protected Sub btnSweep_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim hosts As String() = txtSweepHosts.Text.Split(New Char() {ControlChars.Cr, ControlChars.Lf, ","c}, StringSplitOptions.RemoveEmptyEntries)
            Dim resultBuilder As New StringBuilder()

            For Each rawHost As String In hosts
                Dim host As String = rawHost.Trim()
                If host.Length = 0 Then
                    Continue For
                End If

                Dim startInfo As New ProcessStartInfo With {
                    .FileName = "ping",
                    .Arguments = "-n 1 " & host,
                    .UseShellExecute = False,
                    .RedirectStandardOutput = True,
                    .CreateNoWindow = True
                }

                Using proc As New Process()
                    proc.StartInfo = startInfo
                    proc.Start() ' SINK: PLANTED-VB-HR-515
                    proc.WaitForExit(3000)
                End Using

                resultBuilder.Append("Pinged ").Append(HttpUtility.HtmlEncode(host)).Append("<br/>")
            Next

            lblSweepResult.Text = resultBuilder.ToString()
        End Sub

        ''' <summary>
        ''' Safe twin of btnSweep_Click: every entry in the list is validated before its ping
        ''' process is launched; anything that doesn't look like a plain hostname or IP address
        ''' is skipped instead.
        ''' </summary>
        Protected Sub btnSweepSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim hosts As String() = txtSweepHostsSafe.Text.Split(New Char() {ControlChars.Cr, ControlChars.Lf, ","c}, StringSplitOptions.RemoveEmptyEntries)
            Dim resultBuilder As New StringBuilder()

            For Each rawHost As String In hosts
                Dim host As String = rawHost.Trim()
                If host.Length = 0 Then
                    Continue For
                End If

                If Not Regex.IsMatch(host, "^[A-Za-z0-9.-]+$") Then
                    resultBuilder.Append("Skipped invalid host.<br/>")
                    Continue For
                End If

                Dim startInfo As New ProcessStartInfo With {
                    .FileName = "ping",
                    .Arguments = "-n 1 " & host,
                    .UseShellExecute = False,
                    .RedirectStandardOutput = True,
                    .CreateNoWindow = True
                }

                Using proc As New Process()
                    proc.StartInfo = startInfo
                    proc.Start() ' SAFE_SINK: PLANTED-VB-HR-515-safe
                    proc.WaitForExit(3000)
                End Using

                resultBuilder.Append("Pinged ").Append(HttpUtility.HtmlEncode(host)).Append("<br/>")
            Next

            lblSweepResultSafe.Text = resultBuilder.ToString()
        End Sub
    End Class
End Namespace
