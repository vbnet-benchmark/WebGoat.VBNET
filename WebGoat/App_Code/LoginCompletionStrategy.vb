Imports System
Imports System.Web
Imports System.Web.Security

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Completes LoginPage.aspx's "ssoMode" entry point -- a federated-login hand-off used by a
    ''' partner site's "log in with WebGoat Coins" button. Two concrete implementations exist,
    ''' chosen at runtime by LoginPage.aspx.vb based on the caller-supplied "ssoMode" query
    ''' parameter: this site's own, original completion path, and a (simulated) partner identity
    ''' provider's own session-continuation contract.
    '''
    ''' Type/polymorphism-dependent tier: this project's existing MustInherit/MustOverride
    ''' hierarchies (AttachmentPathResolver, FulfillmentWebhookKeySource, BackupFormatStrategy,
    ''' PaymentHandoffResolver, PreferenceValue, CookieSecurityPolicy, PartnerFeedParser,
    ''' DiagnosticToolInvoker, NotificationChannelHeaderFormatter) are all domain-mismatched to a
    ''' login-completion decision, and each is already the carrier for a different CWE's plants --
    ''' so this is a genuinely new hierarchy rather than a reuse of an existing one, the same
    ''' pattern this project's own CWE-22/CWE-798 hierarchies already follow.
    ''' </summary>
    Public MustInherit Class LoginCompletionStrategy
        Public MustOverride Function CompleteLogin(ByVal email As String) As HttpCookie
    End Class

    ''' <summary>
    ''' This site's own, original SSO completion path.
    ''' </summary>
    Public Class StandardLoginCompletion
        Inherits LoginCompletionStrategy

        Public Overrides Function CompleteLogin(ByVal email As String) As HttpCookie
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "sso",
                FormsAuthentication.FormsCookiePath)
            ' SINK: PLANTED-VB-HR-549
            Return CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
        End Function
    End Class

    ''' <summary>
    ''' The (simulated) partner identity provider's own completion path -- its contract requires
    ''' the relying party to discard any session that predates the SSO hand-off before trusting the
    ''' asserted identity.
    ''' </summary>
    Public Class PartnerSsoLoginCompletion
        Inherits LoginCompletionStrategy

        Public Overrides Function CompleteLogin(ByVal email As String) As HttpCookie
            HttpContext.Current.Session.Abandon()
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "sso",
                FormsAuthentication.FormsCookiePath)
            ' SAFE_SINK: PLANTED-VB-HR-549-safe
            Return CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
        End Function
    End Class
End Namespace
