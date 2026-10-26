Imports System
Imports System.Web
Imports System.Web.Security

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Completes the "sign me in" step of the security-question password-recovery flow once
    ''' ForgotPassword.aspx has verified the customer's identity: ChangePassword.aspx.vb calls this
    ''' shared helper after the new password is saved, rather than sending the customer back to
    ''' CustomerLogin.aspx to re-enter credentials they have just effectively proven they know.
    ''' </summary>
    Public Class AccountRecoveryLoginCompletion

        Public Shared Function CompleteLogin(ByVal email As String) As HttpCookie
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                FormsAuthentication.FormsCookiePath)
            Return CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
        End Function

        ''' <summary>
        ''' Hardened twin: abandons the session that was active during the recovery flow before
        ''' building the now-authenticated cookie, so a session ID an attacker pre-set on the
        ''' customer before the recovery flow began can never carry the recovered identity.
        ''' </summary>
        Public Shared Function CompleteLoginSafe(ByVal email As String) As HttpCookie
            HttpContext.Current.Session.Abandon()
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                FormsAuthentication.FormsCookiePath)
            Return CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
        End Function

    End Class
End Namespace
