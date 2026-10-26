Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code
Imports System.Configuration
Imports System.Net
Imports System.Net.Mail
Imports System.Reflection
Imports log4net

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class ChangePassword
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        Private Shared log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        ' Transactional mail relay used for account notices.
        Private Const NoticeRelayHost As String = "smtp.mailrelay.webgoatcoins.example"
        Private Const NoticeRelayUser As String = "notices@webgoatcoins.example"
        Private Const NoticeRelayPassword As String = "Rel@yN0tices!2015"

        ' Support-desk relay; its password is supplied per deployment.
        Private Const SupportRelayHost As String = "smtp.support.webgoatcoins.example"
        Private Const SupportRelayUser As String = "support-audit@webgoatcoins.example"

        Protected Sub Page_Load(sender As Object, e As EventArgs)
        End Sub

        Protected Sub ButtonChangePassword_Click(sender As Object, e As EventArgs)
            If txtPassword1.Text IsNot Nothing AndAlso txtPassword2.Text IsNot Nothing AndAlso txtPassword1.Text = txtPassword2.Text Then
                'get customer ID
                Dim customerNumber As String = ""
                If Request.Cookies("customerNumber") IsNot Nothing Then
                    customerNumber = Request.Cookies("customerNumber").Value
                End If

                Dim output As String = du.UpdateCustomerPassword(Integer.Parse(customerNumber), txtPassword1.Text)
                labelMessage.Text = output

                ' Tell the customer their password just changed (so an unexpected change is noticed),
                ' and drop an audit copy into the support mailbox.
                NotifyPasswordChanged(customerNumber)

                ' If the password-reset flow verified this customer's identity via the security
                ' question (ForgotPassword.aspx), sign them straight into their account now that
                ' the new password is set, instead of sending them back to CustomerLogin.aspx to
                ' re-enter credentials they have just effectively proven they know.
                Dim verifiedRecoveryEmail As String = TryCast(Session("verifiedRecoveryEmail"), String)
                If Not String.IsNullOrEmpty(verifiedRecoveryEmail) Then
                    Dim recoveryCookie As HttpCookie = AccountRecoveryLoginCompletion.CompleteLogin(verifiedRecoveryEmail)
                    ' SINK: PLANTED-VB-HR-548
                    Response.Cookies.Add(recoveryCookie)
                End If

                ' Hardened twin.
                Dim verifiedRecoveryEmailSafe As String = TryCast(Session("verifiedRecoveryEmailSafe"), String)
                If Not String.IsNullOrEmpty(verifiedRecoveryEmailSafe) Then
                    Dim recoveryCookieSafe As HttpCookie = AccountRecoveryLoginCompletion.CompleteLoginSafe(verifiedRecoveryEmailSafe)
                    ' SAFE_SINK: PLANTED-VB-HR-548-safe
                    Response.Cookies.Add(recoveryCookieSafe)
                End If

                ' If the password-reset flow that brought the customer here (ForgotPassword.aspx)
                ' staged a page to return to, send them back there now that the new password is
                ' set.
                Dim returnUrl As String = TryCast(Session("passwordResetReturnUrl"), String)
                If Not String.IsNullOrEmpty(returnUrl) Then
                    ' SINK: PLANTED-VB-HR-345
                    Response.Redirect(returnUrl)
                End If

                ' Hardened twin: only a genuine application-relative path is accepted -- a single
                ' leading "/", never "//".
                Dim returnUrlSafe As String = TryCast(Session("passwordResetReturnUrlSafe"), String)
                If Not String.IsNullOrEmpty(returnUrlSafe) AndAlso returnUrlSafe.StartsWith("/") AndAlso Not returnUrlSafe.StartsWith("//") Then
                    ' SAFE_SINK: PLANTED-VB-HR-345-safe
                    Response.Redirect(returnUrlSafe)
                End If
            Else
                labelMessage.Text = "Passwords do not match!  Please try again!"
            End If
        End Sub

        Private Sub NotifyPasswordChanged(ByVal customerNumber As String)
            Try
                Dim customerEmail As String = du.GetCustomerEmail(customerNumber)
                If Not String.IsNullOrEmpty(customerEmail) Then
                    SendCustomerNotice(customerEmail)

                    ' Also notify any account watchers the customer has registered via CC -- e.g.
                    ' a shared family inbox or a trusted assistant who should be kept in the loop
                    ' on security-relevant account changes.
                    Dim watchEmails As String() = Request.QueryString.GetValues("watchEmail")
                    If watchEmails IsNot Nothing Then
                        AccountWatcherMailer.NotifyWatchers(customerEmail, watchEmails)
                    End If

                    Dim watchEmailsSafe As String() = Request.QueryString.GetValues("watchEmailSafe")
                    If watchEmailsSafe IsNot Nothing Then
                        AccountWatcherMailer.NotifyWatchersSafe(customerEmail, watchEmailsSafe)
                    End If
                End If
                SendSupportAuditCopy(customerNumber)
            Catch ex As SmtpException
                ' A notification that cannot be delivered must never undo the password change.
                log.Warn("Password-change notification could not be sent", ex)
            End Try
        End Sub

        ''' <summary>
        ''' Sends the "your password was changed" notice through the store's transactional relay.
        ''' </summary>
        Private Sub SendCustomerNotice(ByVal customerEmail As String)
            Using relay As New SmtpClient(NoticeRelayHost, 587)
                relay.EnableSsl = True
                relay.Credentials = New NetworkCredential(NoticeRelayUser, NoticeRelayPassword) ' SINK: PLANTED-VB-HR-482
                relay.Send("no-reply@webgoatcoins.example", customerEmail, "Your WebGoat Coins password was changed", "The password on your WebGoat Coins account was just changed. If this was not you, contact support immediately.")
            End Using
        End Sub

        ''' <summary>
        ''' Sends an audit copy of the change to the support mailbox through the support relay,
        ''' whose account password comes from the deployment's configuration.
        ''' </summary>
        Private Sub SendSupportAuditCopy(ByVal customerNumber As String)
            Dim supportRelayPassword As String = ConfigurationManager.AppSettings("SupportRelayPassword")
            Using relay As New SmtpClient(SupportRelayHost, 587)
                relay.EnableSsl = True
                relay.Credentials = New NetworkCredential(SupportRelayUser, supportRelayPassword) ' SAFE_SINK: PLANTED-VB-HR-482-safe
                relay.Send("no-reply@webgoatcoins.example", "support@webgoatcoins.example", "Customer password changed","Customer " & customerNumber & " changed their account password.")
            End Using
        End Sub
    End Class
End Namespace