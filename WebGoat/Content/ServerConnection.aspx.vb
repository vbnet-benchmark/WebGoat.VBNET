Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Net.Sockets
Imports System.Text

Namespace OWASP.WebGoat.NET
    Partial Public Class ServerConnection
        Inherits System.Web.UI.Page

        ' ===================================================================
        ' Tier 1: Direct -- straight-line, no helper, no branch.
        ' ===================================================================

        Protected Sub btnDirectConnect_Click(sender As Object, e As EventArgs)
            Dim portNumber As Integer
            If Not Integer.TryParse(txtDirectPort.Text, portNumber) Then
                lblDirectResult.Text = "Invalid port."
                Return
            End If

            Try
                Using client As New TcpClient(txtDirectHost.Text, portNumber) ' SINK: PLANTED-VB-HR-706
                    lblDirectResult.Text = "Connected to " & txtDirectHost.Text & ":" & portNumber
                End Using
            Catch ex As Exception
                lblDirectResult.Text = "Connection failed: " & ex.Message
            End Try
        End Sub

        Protected Sub btnDirectConnectSafe_Click(sender As Object, e As EventArgs)
            Dim portNumber As Integer
            If Not Integer.TryParse(txtDirectPortSafe.Text, portNumber) Then
                lblDirectResultSafe.Text = "Invalid port."
                Return
            End If

            If Not ConnectionAllowlist.IsAllowed(txtDirectHostSafe.Text, portNumber) Then
                lblDirectResultSafe.Text = "Rejected: target is not on the approved connectivity allow-list."
                Return
            End If

            Try
                Using client As New TcpClient(txtDirectHostSafe.Text, portNumber) ' SAFE_SINK: PLANTED-VB-HR-706-safe
                    lblDirectResultSafe.Text = "Connected to " & txtDirectHostSafe.Text & ":" & portNumber
                End Using
            Catch ex As Exception
                lblDirectResultSafe.Text = "Connection failed: " & ex.Message
            End Try
        End Sub

        ' ===================================================================
        ' Tier 2: Indirect -- same-class private helper, one hop.
        ' ===================================================================

        Protected Sub btnQuickHealthCheck_Click(sender As Object, e As EventArgs)
            lblHealthCheckResult.Text = OpenHealthCheckConnection(txtHealthCheckHost.Text, txtHealthCheckPort.Text)
        End Sub

        Private Function OpenHealthCheckConnection(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            Try
                Using client As New TcpClient(hostName, portNumber) ' SINK: PLANTED-VB-HR-707
                    Return "Health check succeeded for " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Health check failed: " & ex.Message
            End Try
        End Function

        Protected Sub btnQuickHealthCheckSafe_Click(sender As Object, e As EventArgs)
            lblHealthCheckResultSafe.Text = OpenHealthCheckConnectionSafe(txtHealthCheckHostSafe.Text, txtHealthCheckPortSafe.Text)
        End Sub

        Private Function OpenHealthCheckConnectionSafe(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            If Not ConnectionAllowlist.IsAllowed(hostName, portNumber) Then
                Return "Rejected: target is not on the approved connectivity allow-list."
            End If

            Try
                Using client As New TcpClient(hostName, portNumber) ' SAFE_SINK: PLANTED-VB-HR-707-safe
                    Return "Health check succeeded for " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Health check failed: " & ex.Message
            End Try
        End Function

        ' ===================================================================
        ' Tier 3: Interprocedural -- 2-hop cross-file chain: this page ->
        ' App_Code/NetworkProbeService.vb -> App_Code/RawConnectionProbe.vb.
        ' ===================================================================

        Protected Sub btnPartnerProbe_Click(sender As Object, e As EventArgs)
            lblPartnerProbeResult.Text = NetworkProbeService.ProbePartnerEndpoint(txtPartnerHost.Text, txtPartnerPort.Text)
        End Sub

        Protected Sub btnPartnerProbeSafe_Click(sender As Object, e As EventArgs)
            lblPartnerProbeResultSafe.Text = NetworkProbeService.ProbePartnerEndpointSafe(txtPartnerHostSafe.Text, txtPartnerPortSafe.Text)
        End Sub

        ' ===================================================================
        ' Tier 4: Type/polymorphism-dependent -- a genuine MustInherit
        ' hierarchy; the dropdown only picks WHICH concrete resolver is
        ' constructed, the Connect(...) call site itself never branches.
        ' ===================================================================

        Protected Sub btnModeConnect_Click(sender As Object, e As EventArgs)
            Dim resolver As ServerConnectionResolverBase

            If String.Equals(ddlConnectionMode.SelectedValue, "validated", StringComparison.OrdinalIgnoreCase) Then
                resolver = New ValidatedConnectionResolver()
            Else
                resolver = New LegacyConnectionResolver()
            End If

            lblModeConnectResult.Text = resolver.Connect(txtModeHost.Text, txtModePort.Text)
        End Sub

        ' ===================================================================
        ' Tier 5: Diversity -- batch/loop construct: one connection attempt
        ' per host:port pair entered, every result consumed in the rendered
        ' summary rather than left as a dead-end local.
        ' ===================================================================

        Protected Sub btnBatchProbe_Click(sender As Object, e As EventArgs)
            Dim resultsBuilder As New StringBuilder()
            Dim targetLines() As String = txtBatchTargets.Text.Split(New Char() {ControlChars.Lf})

            For Each rawLine As String In targetLines
                Dim trimmedLine As String = rawLine.Trim(ControlChars.Cr).Trim()
                If trimmedLine.Length = 0 Then
                    Continue For
                End If

                Dim parts() As String = trimmedLine.Split(":"c)
                If parts.Length <> 2 Then
                    resultsBuilder.AppendLine(trimmedLine & " -> skipped (expected host:port)")
                    Continue For
                End If

                Dim targetHost As String = parts(0)
                Dim targetPort As Integer
                If Not Integer.TryParse(parts(1), targetPort) Then
                    resultsBuilder.AppendLine(trimmedLine & " -> skipped (invalid port)")
                    Continue For
                End If

                Try
                    Using client As New TcpClient(targetHost, targetPort) ' SINK: PLANTED-VB-HR-710
                        resultsBuilder.AppendLine(targetHost & ":" & targetPort & " -> reachable")
                    End Using
                Catch ex As Exception
                    resultsBuilder.AppendLine(targetHost & ":" & targetPort & " -> unreachable (" & ex.Message & ")")
                End Try
            Next

            lblBatchResult.Text = resultsBuilder.ToString()
        End Sub

        Protected Sub btnBatchProbeSafe_Click(sender As Object, e As EventArgs)
            Dim resultsBuilder As New StringBuilder()
            Dim targetLines() As String = txtBatchTargetsSafe.Text.Split(New Char() {ControlChars.Lf})

            For Each rawLine As String In targetLines
                Dim trimmedLine As String = rawLine.Trim(ControlChars.Cr).Trim()
                If trimmedLine.Length = 0 Then
                    Continue For
                End If

                Dim parts() As String = trimmedLine.Split(":"c)
                If parts.Length <> 2 Then
                    resultsBuilder.AppendLine(trimmedLine & " -> skipped (expected host:port)")
                    Continue For
                End If

                Dim targetHost As String = parts(0)
                Dim targetPort As Integer
                If Not Integer.TryParse(parts(1), targetPort) Then
                    resultsBuilder.AppendLine(trimmedLine & " -> skipped (invalid port)")
                    Continue For
                End If

                If Not ConnectionAllowlist.IsAllowed(targetHost, targetPort) Then
                    resultsBuilder.AppendLine(targetHost & ":" & targetPort & " -> rejected (not allow-listed)")
                    Continue For
                End If

                Try
                    Using client As New TcpClient(targetHost, targetPort) ' SAFE_SINK: PLANTED-VB-HR-710-safe
                        resultsBuilder.AppendLine(targetHost & ":" & targetPort & " -> reachable")
                    End Using
                Catch ex As Exception
                    resultsBuilder.AppendLine(targetHost & ":" & targetPort & " -> unreachable (" & ex.Message & ")")
                End Try
            Next

            lblBatchResultSafe.Text = resultsBuilder.ToString()
        End Sub

    End Class
End Namespace
