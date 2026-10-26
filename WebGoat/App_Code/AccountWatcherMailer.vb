Imports System
Imports System.Net.Mail

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Notifies every "account watcher" address a customer has registered (e.g. a shared family
    ''' inbox, a trusted assistant) when a security-relevant account change happens --
    ''' WebGoatCoins/ChangePassword.aspx.vb's NotifyPasswordChanged calls this once per request,
    ''' for every watcher address supplied via the repeated "watchEmail"/"watchEmailSafe"
    ''' query-string key.
    ''' </summary>
    Friend Module AccountWatcherMailer

        Friend Sub NotifyWatchers(ByVal customerEmail As String, ByVal watchers As String())
            Dim msg As New MailMessage()
            msg.From = New MailAddress("no-reply@webgoatcoins.example")
            msg.To.Add(customerEmail)
            For Each watcher As String In watchers
                If Not String.IsNullOrEmpty(watcher) Then
                    msg.CC.Add(watcher) ' SINK: PLANTED-VB-HR-595
                End If
            Next
            msg.Subject = "Your WebGoat Coins account password was changed"
            msg.Body = "The password on this WebGoat Coins account was just changed. If this wasn't you, contact support immediately."
            Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                client.Send(msg)
            End Using
        End Sub

        ''' <summary>
        ''' Hardened twin: each watcher address is parsed through the single-address constructor
        ''' first; a malformed element (including one carrying a comma/semicolon-smuggled extra
        ''' address) is skipped rather than letting it reach the collection unchecked.
        ''' </summary>
        Friend Sub NotifyWatchersSafe(ByVal customerEmail As String, ByVal watchers As String())
            Dim msg As New MailMessage()
            msg.From = New MailAddress("no-reply@webgoatcoins.example")
            msg.To.Add(customerEmail)
            For Each watcher As String In watchers
                If Not String.IsNullOrEmpty(watcher) Then
                    Try
                        Dim validatedWatcher As New MailAddress(watcher)
                        msg.CC.Add(validatedWatcher) ' SAFE_SINK: PLANTED-VB-HR-595-safe
                    Catch ex As FormatException
                        ' Not a single well-formed address -- skip this watcher, keep the rest.
                    End Try
                End If
            Next
            msg.Subject = "Your WebGoat Coins account password was changed"
            msg.Body = "The password on this WebGoat Coins account was just changed. If this wasn't you, contact support immediately."
            Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                client.Send(msg)
            End Using
        End Sub

    End Module
End Namespace
