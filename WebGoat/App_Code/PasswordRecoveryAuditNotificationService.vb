Imports System
Imports System.Net.Mail

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Sends the "your password was just revealed via the security-question recovery flow"
    ''' audit notice that WebGoatCoins/ForgotPassword.aspx.vb's ButtonRecoverPassword_Click
    ''' raises once the security-question challenge succeeds, optionally CC'ing a secondary
    ''' address the customer names (e.g. a shared family inbox or a trusted assistant).
    ''' </summary>
    Public Class PasswordRecoveryAuditNotificationService

        Public Shared Sub NotifyPasswordRevealed(ByVal primaryEmail As String, ByVal ccEmail As String)
            If String.IsNullOrEmpty(ccEmail) Then
                Return
            End If
            Dim msg As New MailMessage()
            msg.From = New MailAddress("no-reply@webgoatcoins.example")
            msg.To.Add(primaryEmail)
            msg.CC.Add(ccEmail) ' SINK: PLANTED-VB-HR-593
            msg.Subject = "Your WebGoat Coins password was just revealed via account recovery"
            msg.Body = "The password-recovery security question for this account was just answered correctly and the password was shown. If this wasn't you, contact support immediately."
            Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                client.Send(msg)
            End Using
        End Sub

        ''' <summary>
        ''' Hardened twin: the CC address is parsed through the single-address constructor first,
        ''' which rejects a comma/semicolon-bearing string outright, and only the already-
        ''' validated MailAddress object (never the raw string) is added to the collection.
        ''' </summary>
        Public Shared Sub NotifyPasswordRevealedSafe(ByVal primaryEmail As String, ByVal ccEmail As String)
            If String.IsNullOrEmpty(ccEmail) Then
                Return
            End If
            Try
                Dim validatedCc As New MailAddress(ccEmail)
                Dim msg As New MailMessage()
                msg.From = New MailAddress("no-reply@webgoatcoins.example")
                msg.To.Add(primaryEmail)
                msg.CC.Add(validatedCc) ' SAFE_SINK: PLANTED-VB-HR-593-safe
                msg.Subject = "Your WebGoat Coins password was just revealed via account recovery"
                msg.Body = "The password-recovery security question for this account was just answered correctly and the password was shown. If this wasn't you, contact support immediately."
                Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                    client.Send(msg)
                End Using
            Catch ex As FormatException
                ' Not a single well-formed address -- fail closed, send no CC at all.
            End Try
        End Sub

    End Class
End Namespace
