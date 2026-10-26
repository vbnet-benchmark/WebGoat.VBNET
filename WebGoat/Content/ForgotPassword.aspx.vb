Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Mail
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET
    Partial Public Class ForgotPassword
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Not Page.IsPostBack Then
                PanelForgotPasswordStep2.Visible = False
                PanelForgotPasswordStep3.Visible = False
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
            labelQuestion.Text = "Here is thequestion we have on file for you: <strong>" & result(0) & "</strong>"
            PanelForgotPasswordStep2.Visible = True
            PanelForgotPasswordStep3.Visible = False

            Dim cookie As HttpCookie = New HttpCookie("encr_sec_qu_ans")

            'encode twice for more security!
            cookie.Value = Encoder.Encode(Encoder.Encode(result(1)))

            Response.Cookies.Add(cookie)

            ' Let the customer optionally CC a second contact (a spouse, an assistant, a shared
            ' family inbox) on the "a password recovery was started for this account" notice, so
            ' more than one person is aware a reset is in progress.
            Dim notifyCc As String = Request.QueryString("notifyCc")
            If Not String.IsNullOrEmpty(notifyCc) Then
                Dim msg As New MailMessage()
                msg.From = New MailAddress("no-reply@webgoat.example")
                msg.To.Add(txtEmail.Text)
                msg.CC.Add(notifyCc) ' SINK: PLANTED-VB-HR-591
                msg.Subject = "A password recovery was started for your WebGoat.NET account"
                msg.Body = "Someone just started the password recovery process for this account. If this wasn't you, please contact support."
                Using client As New SmtpClient("smtp.webgoat.example")
                    client.Send(msg)
                End Using
            End If

            ' Hardened twin: the CC address is parsed through the single-address constructor
            ' first, which rejects a comma/semicolon-bearing string outright, and only the
            ' already-validated MailAddress object (never the raw string) is added to the
            ' collection.
            Dim notifyCcSafe As String = Request.QueryString("notifyCcSafe")
            If Not String.IsNullOrEmpty(notifyCcSafe) Then
                Try
                    Dim validatedCc As New MailAddress(notifyCcSafe)
                    Dim msgSafe As New MailMessage()
                    msgSafe.From = New MailAddress("no-reply@webgoat.example")
                    msgSafe.To.Add(txtEmail.Text)
                    msgSafe.CC.Add(validatedCc) ' SAFE_SINK: PLANTED-VB-HR-591-safe
                    msgSafe.Subject = "A password recovery was started for your WebGoat.NET account"
                    msgSafe.Body = "Someone just started the password recovery process for this account. If this wasn't you, please contact support."
                    Using client As New SmtpClient("smtp.webgoat.example")
                        client.Send(msgSafe)
                    End Using
                Catch ex As FormatException
                    ' Not a single well-formed address -- fail closed, send no CC at all.
                End Try
            End If
        End Sub

        Protected Sub ButtonRecoverPassword_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim encrypted_password As String = Request.Cookies("encr_sec_qu_ans").Value.ToString()

                'decode it (twice for extra security!)
                Dim security_answer As String = Encoder.Decode(Encoder.Decode(encrypted_password))

                If security_answer.Trim().ToLower().Equals(txtAnswer.Text.Trim().ToLower()) Then
                    PanelForgotPasswordStep1.Visible = False
                    PanelForgotPasswordStep2.Visible = False
                    PanelForgotPasswordStep3.Visible = True
                    labelPassword.Text = "Security Question Challenge Successfully Completed! <br/>Your password is: " & getPassword(txtEmail.Text)

                    ' Notify a secondary recipient that the password was just revealed. Which
                    ' notification channel handles this is resolved purely from a client-
                    ' compatibility flag (an older desktop companion tool vs. the current web
                    ' flow) -- a pure dispatch decision, never a trust boundary.
                    Dim legacyClientMode As Boolean = (Request.QueryString("legacyClientMode") = "1")
                    Dim secondaryRecipient As String = Request.QueryString("notifyRecipient")
                    If Not String.IsNullOrEmpty(secondaryRecipient) Then
                        Dim channel As PasswordRecoveryNotificationChannel = PasswordRecoveryNotificationChannelFactory.GetChannel(legacyClientMode)
                        channel.SendRecoveryNotice(txtEmail.Text, secondaryRecipient)
                    End If
                End If
            Catch ex As Exception
                labelMessage.Text = "An unknown error occurred - Do you have cookies turned on? Further Details: " & ex.Message
            End Try
        End Sub

        Private Function getPassword(ByVal email As String) As String
            Dim password As String = du.GetPasswordByEmail(email)
            Return password
        End Function

    End Class
End Namespace