Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Mail
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Security.Cryptography
Imports System.Text
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class ForgotPassword
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Not Page.IsPostBack Then
                PanelForgotPasswordStep2.Visible = False
                PanelForgotPasswordStep3.Visible = False

                ' A "Reset my password" link that starts this flow from a specific product/order
                ' page (e.g. "you must reset your password before completing checkout") carries a
                ' ReturnUrl so ChangePassword.aspx can send the customer back there once the new
                ' password is set.
                Dim returnUrl As String = Request.QueryString("ReturnUrl")
                If Not String.IsNullOrEmpty(returnUrl) Then
                    Session("passwordResetReturnUrl") = returnUrl
                End If

                Dim returnUrlSafe As String = Request.QueryString("ReturnUrlSafe")
                If Not String.IsNullOrEmpty(returnUrlSafe) Then
                    Session("passwordResetReturnUrlSafe") = returnUrlSafe
                End If

                ' ForgotPassword.aspx is also the landing page for the "Confirm this password
                ' reset request" link emailed after Step 1 completes. The link carries the
                ' customer's email, the reset token, and the token's expiry as plain query-string
                ' fields, plus a signature so the server can tell whether any of those three
                ' fields were altered in transit before the token is ever looked up.
                ' SECURITY-SENSITIVE: a forgeable signature here lets an attacker land on this
                ' page with an arbitrary email/expiry pair that still "looks" signed, and jump
                ' straight to the password-reveal step for that email.
                Dim confirmEmail As String = Request.QueryString("confirmEmail")
                Dim confirmToken As String = Request.QueryString("confirmToken")
                Dim confirmExpiry As String = Request.QueryString("confirmExpiry")
                Dim confirmSig As String = Request.QueryString("confirmSig")
                If Not String.IsNullOrEmpty(confirmEmail) AndAlso Not String.IsNullOrEmpty(confirmToken) AndAlso Not String.IsNullOrEmpty(confirmExpiry) AndAlso Not String.IsNullOrEmpty(confirmSig) Then
                    Dim expectedBytes As Byte()
                    Using md5 As MD5 = MD5.Create()
                        Dim input As String = confirmEmail & "|" & confirmToken & "|" & confirmExpiry
                        expectedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input)) ' SINK: PLANTED-VB-HR-411
                    End Using
                    Dim expectedHex As String = BitConverter.ToString(expectedBytes).Replace("-", "").ToLowerInvariant()

                    If String.Equals(expectedHex, confirmSig, StringComparison.OrdinalIgnoreCase) Then
                        PanelForgotPasswordStep1.Visible = False
                        PanelForgotPasswordStep2.Visible = False
                        PanelForgotPasswordStep3.Visible = True
                        labelPassword.Text = "Reset link confirmed! <br/>Your password is: " & getPassword(confirmEmail)
                    End If
                End If

                ' Hardened twin: the same three fields, verified with a salted SHA-256 signature
                ' (the salt travels alongside the signature, since this link has no server-side
                ' session to remember it in) instead of a bare MD5 digest.
                Dim confirmEmailSafe As String = Request.QueryString("confirmEmailSafe")
                Dim confirmTokenSafe As String = Request.QueryString("confirmTokenSafe")
                Dim confirmExpirySafe As String = Request.QueryString("confirmExpirySafe")
                Dim confirmSigSafe As String = Request.QueryString("confirmSigSafe")
                Dim confirmSaltSafe As String = Request.QueryString("confirmSaltSafe")
                If Not String.IsNullOrEmpty(confirmEmailSafe) AndAlso Not String.IsNullOrEmpty(confirmTokenSafe) AndAlso Not String.IsNullOrEmpty(confirmExpirySafe) AndAlso Not String.IsNullOrEmpty(confirmSigSafe) AndAlso Not String.IsNullOrEmpty(confirmSaltSafe) Then
                    Dim expectedBytesSafe As Byte()
                    Using sha256 As SHA256 = SHA256.Create()
                        Dim input As String = confirmEmailSafe & "|" & confirmTokenSafe & "|" & confirmExpirySafe & "|" & confirmSaltSafe
                        expectedBytesSafe = sha256.ComputeHash(Encoding.UTF8.GetBytes(input)) ' SAFE_SINK: PLANTED-VB-HR-411-safe
                    End Using
                    Dim expectedHexSafe As String = BitConverter.ToString(expectedBytesSafe).Replace("-", "").ToLowerInvariant()

                    If String.Equals(expectedHexSafe, confirmSigSafe, StringComparison.OrdinalIgnoreCase) Then
                        PanelForgotPasswordStep1.Visible = False
                        PanelForgotPasswordStep2.Visible = False
                        PanelForgotPasswordStep3.Visible = True
                        labelPassword.Text = "Reset link confirmed! <br/>Your password is: " & getPassword(confirmEmailSafe)
                    End If
                End If
            End If
        End Sub

        Protected Sub ButtonCheckEmail_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim result() As String = du.GetSecurityQuestionAndAnswer(txtEmail.Text)

            If String.IsNullOrEmpty(result(0)) Then
                labelQuestion.Text = "That email address was not found in our database!"
                PanelForgotPasswordStep2.Visible = False
                PanelForgotPasswordStep3.Visible = False

                Return
            End If
            labelQuestion.Text = "Here is the question we have on file for you: <strong>" & result(0) & "</strong>"
            PanelForgotPasswordStep2.Visible = True
            PanelForgotPasswordStep3.Visible = False

            Dim cookie As HttpCookie = New HttpCookie("encr_sec_qu_ans")

            'encode twice for more security!

            cookie.Value = Encoder.Encode(Encoder.Encode(result(1)))

            Response.Cookies.Add(cookie)

            ' Let the customer optionally CC a second contact on the "a password recovery was
            ' started" notice -- routed through a private helper that builds and sends it.
            Dim notifyCc As String = Request.QueryString("notifyCc")
            SendRecoveryStartedNotice(txtEmail.Text, notifyCc)

            Dim notifyCcSafe As String = Request.QueryString("notifyCcSafe")
            SendRecoveryStartedNoticeSafe(txtEmail.Text, notifyCcSafe)
        End Sub

        ''' <summary>
        ''' Sends the "a password recovery was started for your account" notice, optionally
        ''' CC'ing a second contact the customer names.
        ''' </summary>
        Private Sub SendRecoveryStartedNotice(ByVal primaryEmail As String, ByVal ccEmail As String)
            If String.IsNullOrEmpty(ccEmail) Then
                Return
            End If
            Dim msg As New MailMessage()
            msg.From = New MailAddress("no-reply@webgoatcoins.example")
            msg.To.Add(primaryEmail)
            msg.CC.Add(ccEmail) ' SINK: PLANTED-VB-HR-592
            msg.Subject = "A password recovery was started for your WebGoat Coins account"
            msg.Body = "Someone just started the password recovery process for this account. If this wasn't you, please contact support."
            Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                client.Send(msg)
            End Using
        End Sub

        ''' <summary>
        ''' Hardened twin: the CC address is parsed through the single-address constructor first,
        ''' which rejects a comma/semicolon-bearing string outright, and only the already-
        ''' validated MailAddress object (never the raw string) is added to the collection.
        ''' </summary>
        Private Sub SendRecoveryStartedNoticeSafe(ByVal primaryEmail As String, ByVal ccEmail As String)
            If String.IsNullOrEmpty(ccEmail) Then
                Return
            End If
            Try
                Dim validatedCc As New MailAddress(ccEmail)
                Dim msg As New MailMessage()
                msg.From = New MailAddress("no-reply@webgoatcoins.example")
                msg.To.Add(primaryEmail)
                msg.CC.Add(validatedCc) ' SAFE_SINK: PLANTED-VB-HR-592-safe
                msg.Subject = "A password recovery was started for your WebGoat Coins account"
                msg.Body = "Someone just started the password recovery process for this account. If this wasn't you, please contact support."
                Using client As New SmtpClient("smtp.mailrelay.webgoatcoins.example")
                    client.Send(msg)
                End Using
            Catch ex As FormatException
                ' Not a single well-formed address -- fail closed, send no CC at all.
            End Try
        End Sub

        Protected Sub ButtonRecoverPassword_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                'get the security question answer from the cookie
                Dim encrypted_password As String = Request.Cookies("encr_sec_qu_ans").Value.ToString()

                'decode it (twice for extra security!)
                Dim security_answer As String = Encoder.Decode(Encoder.Decode(encrypted_password))

                If security_answer.Trim().ToLower().Equals(txtAnswer.Text.Trim().ToLower()) Then
                    PanelForgotPasswordStep1.Visible = False
                    PanelForgotPasswordStep2.Visible = False
                    PanelForgotPasswordStep3.Visible = True
                    labelPassword.Text = "Security Question Challenge Successfully Completed! <br/>Your password is: " & getPassword(txtEmail.Text)

                    ' Having just proven their identity via the security question, the customer is
                    ' considered "recovery-verified" for the rest of this visit -- ChangePassword.aspx
                    ' uses this to sign them straight into their account once the new password is
                    ' set, instead of sending them back to CustomerLogin.aspx to re-enter
                    ' credentials they have just effectively demonstrated they control.
                    Session("verifiedRecoveryEmail") = txtEmail.Text

                    ' Hardened twin's own recovery-verified marker.
                    Session("verifiedRecoveryEmailSafe") = txtEmail.Text

                    ' Send an audit-copy notice to a secondary address when the customer names
                    ' one, so e.g. a shared family inbox is also told the password was revealed.
                    Dim auditCc As String = Request.QueryString("auditCc")
                    PasswordRecoveryAuditNotificationService.NotifyPasswordRevealed(txtEmail.Text, auditCc)

                    Dim auditCcSafe As String = Request.QueryString("auditCcSafe")
                    PasswordRecoveryAuditNotificationService.NotifyPasswordRevealedSafe(txtEmail.Text, auditCcSafe)
                End If
            Catch ex As Exception
                labelMessage.Text = "An unknown error occurred - Do you have cookies turned on? Further Details: " & ex.Message
            End Try
        End Sub

        Protected Sub ButtonGoToCustomerLogin_Click(ByVal sender As Object, ByVal e As EventArgs)
            Response.Redirect("CustomerLogin.aspx")
        End Sub

        Private Function getPassword(ByVal email As String) As String
            Dim password As String = du.GetPasswordByEmail(email)
            Return password
        End Function

    End Class
End Namespace