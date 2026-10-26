Imports System
Imports System.Net.Mail

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Sends the "your password was just revealed" notice to a secondary recipient that
    ''' Content/ForgotPassword.aspx.vb's ButtonRecoverPassword_Click names, once the
    ''' security-question challenge succeeds. Two concrete implementations exist, chosen at
    ''' runtime by PasswordRecoveryNotificationChannelFactory based on the caller-supplied
    ''' "legacyClientMode" query parameter -- an older desktop companion tool vs. the current
    ''' web flow, a pure client-compatibility dispatch decision with no trust implication.
    '''
    ''' Type/polymorphism-dependent tier: this project's existing MustInherit/MustOverride
    ''' hierarchies (AttachmentPathResolver, FulfillmentWebhookKeySource, BackupFormatStrategy,
    ''' PaymentHandoffResolver, PreferenceValue, CookieSecurityPolicy, PartnerFeedParser,
    ''' DiagnosticToolInvoker, NotificationChannelHeaderFormatter, LoginCompletionStrategy) are
    ''' all domain-mismatched to a recovery-notification decision, and each is already the
    ''' carrier for a different CWE's plants -- so this is a genuinely new hierarchy, the same
    ''' pattern this project's own CWE-22/CWE-798/CWE-384 hierarchies already follow.
    ''' </summary>
    Public MustInherit Class PasswordRecoveryNotificationChannel
        Public MustOverride Sub SendRecoveryNotice(ByVal primaryEmail As String, ByVal secondaryRecipient As String)
    End Class

    ''' <summary>
    ''' The older desktop companion tool's notification path: adds the caller-supplied secondary
    ''' recipient straight to the message.
    ''' </summary>
    Public Class DirectRecoveryNotificationChannel
        Inherits PasswordRecoveryNotificationChannel

        Public Overrides Sub SendRecoveryNotice(ByVal primaryEmail As String, ByVal secondaryRecipient As String)
            Dim msg As New MailMessage()
            msg.From = New MailAddress("no-reply@webgoat.example")
            msg.To.Add(primaryEmail)
            msg.To.Add(secondaryRecipient) ' SINK: PLANTED-VB-HR-594
            msg.Subject = "Your WebGoat.NET password was just revealed via account recovery"
            msg.Body = "The password-recovery security question for this account was just answered correctly and the password was shown. If this wasn't you, contact support immediately."
            Using client As New SmtpClient("smtp.webgoat.example")
                client.Send(msg)
            End Using
        End Sub
    End Class

    ''' <summary>
    ''' The current web flow's notification path: the secondary recipient is parsed through the
    ''' single-address constructor first, which rejects a comma/semicolon-bearing string
    ''' outright, and only the already-validated MailAddress object (never the raw string) is
    ''' added to the message.
    ''' </summary>
    Public Class ValidatedRecoveryNotificationChannel
        Inherits PasswordRecoveryNotificationChannel

        Public Overrides Sub SendRecoveryNotice(ByVal primaryEmail As String, ByVal secondaryRecipient As String)
            Try
                Dim validatedRecipient As New MailAddress(secondaryRecipient)
                Dim msg As New MailMessage()
                msg.From = New MailAddress("no-reply@webgoat.example")
                msg.To.Add(primaryEmail)
                msg.To.Add(validatedRecipient) ' SAFE_SINK: PLANTED-VB-HR-594-safe
                msg.Subject = "Your WebGoat.NET password was just revealed via account recovery"
                msg.Body = "The password-recovery security question for this account was just answered correctly and the password was shown. If this wasn't you, contact support immediately."
                Using client As New SmtpClient("smtp.webgoat.example")
                    client.Send(msg)
                End Using
            Catch ex As FormatException
                ' Not a single well-formed address -- fail closed, send no notice at all.
            End Try
        End Sub
    End Class

    ''' <summary>
    ''' Resolves which notification channel handles a password-recovery notice, purely by client
    ''' compatibility -- never a trust decision.
    ''' </summary>
    Public Class PasswordRecoveryNotificationChannelFactory
        Public Shared Function GetChannel(ByVal legacyClientMode As Boolean) As PasswordRecoveryNotificationChannel
            If legacyClientMode Then
                Return New DirectRecoveryNotificationChannel()
            Else
                Return New ValidatedRecoveryNotificationChannel()
            End If
        End Function
    End Class
End Namespace
