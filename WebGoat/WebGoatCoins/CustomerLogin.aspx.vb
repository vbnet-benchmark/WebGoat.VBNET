Imports System
Imports System.Web.Security
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code
Imports log4net
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class CustomerLogin
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        Private log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            PanelError.Visible = False

            Dim returnUrl As String = Request.QueryString("ReturnUrl")
            If returnUrl IsNot Nothing Then
                PanelError.Visible = True
            End If

            ' Record where the customer came from so MainPage.aspx can greet them appropriately
            ' once they're logged in.
            Dim fromPage As String = Request.QueryString("fromPage")
            If fromPage IsNot Nothing Then
                Session("lastVisitedPage") = fromPage
            End If

            Dim fromPageSafe As String = Request.QueryString("fromPageSafe")
            If fromPageSafe IsNot Nothing Then
                Session("lastVisitedPageSafe") = fromPageSafe
            End If

            ' Queue the customer's own requested banner text so MainPage.aspx can attach it as a
            ' custom response header once they're logged in and redirected there -- the text rides
            ' across the login redirect via Session rather than being passed directly, the same
            ' mechanism "lastVisitedPage" above already established for carrying state across
            ' this same redirect.
            Dim bannerHeaderText As String = Request.QueryString("bannerHeaderText")
            If bannerHeaderText IsNot Nothing Then
                Session("pendingBannerHeaderText") = bannerHeaderText
            End If

            Dim bannerHeaderTextSafe As String = Request.QueryString("bannerHeaderTextSafe")
            If bannerHeaderTextSafe IsNot Nothing Then
                Session("pendingBannerHeaderTextSafe") = bannerHeaderTextSafe
            End If

            ' "Sign up and go" landing-page link: a marketing/referral link that pre-fills a brand
            ' new account and signs the visitor straight into it, skipping the usual "register,
            ' then separately log in" two-step so a referred visitor never has to type their new
            ' password a second time.
            Dim newAccountEmail As String = Request.QueryString("newAccountEmail")
            Dim newAccountPassword As String = Request.QueryString("newAccountPassword")
            If Not String.IsNullOrEmpty(newAccountEmail) AndAlso Not String.IsNullOrEmpty(newAccountPassword) Then
                If du.CreateCustomer(newAccountEmail, newAccountEmail, newAccountPassword, False, 0, "") Then
                    Dim newTicket As New FormsAuthenticationTicket(
                        1, newAccountEmail, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                        FormsAuthentication.FormsCookiePath)
                    Dim newCookie As HttpCookie = CookieManager.SetCookie(newTicket, FormsAuthentication.FormsCookieName, newAccountEmail)
                    ' SINK: PLANTED-VB-HR-546
                    Response.Cookies.Add(newCookie)
                End If
            End If

            ' Hardened twin: the brand new account's session is abandoned before the
            ' now-authenticated cookie is issued, so a session ID an attacker pre-set on this
            ' visitor before they signed up can never carry the new account's identity.
            Dim newAccountEmailSafe As String = Request.QueryString("newAccountEmailSafe")
            Dim newAccountPasswordSafe As String = Request.QueryString("newAccountPasswordSafe")
            If Not String.IsNullOrEmpty(newAccountEmailSafe) AndAlso Not String.IsNullOrEmpty(newAccountPasswordSafe) Then
                If du.CreateCustomer(newAccountEmailSafe, newAccountEmailSafe, newAccountPasswordSafe, False, 0, "") Then
                    Session.Abandon()
                    Dim newTicketSafe As New FormsAuthenticationTicket(
                        1, newAccountEmailSafe, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                        FormsAuthentication.FormsCookiePath)
                    Dim newCookieSafe As HttpCookie = CookieManager.SetCookie(newTicketSafe, FormsAuthentication.FormsCookieName, newAccountEmailSafe)
                    ' SAFE_SINK: PLANTED-VB-HR-546-safe
                    Response.Cookies.Add(newCookieSafe)
                End If
            End If

            ' Confirmation-link consumer for AddNewCustomer.aspx's "X-Account-Confirmation-Link"
            ' (and hardened "-Safe") headers: clicking the signed link both confirms the new
            ' account and signs the customer straight in, so they land in their new account
            ' instead of back at a bare login form. The server re-checks the signature against the
            ' identity/event/issued-at fields before honoring the click, exactly as
            ' AddNewCustomer.aspx.vb's own comment says it is meant to.
            Dim confirmEmailLink As String = Request.QueryString("email")
            Dim confirmTsLink As String = Request.QueryString("ts")
            Dim confirmSigLink As String = Request.QueryString("sig")
            If Request.QueryString("confirmAccount") = "1" AndAlso Not String.IsNullOrEmpty(confirmEmailLink) AndAlso Not String.IsNullOrEmpty(confirmTsLink) AndAlso Not String.IsNullOrEmpty(confirmSigLink) Then
                Dim issuedAt As DateTime
                If DateTime.TryParse(confirmTsLink, issuedAt) Then
                    Dim expectedSig As String = AccountNotificationSignatureUtil.ComputeNotificationSignature(confirmEmailLink, "AccountCreated", issuedAt)
                    If String.Equals(expectedSig, confirmSigLink, StringComparison.OrdinalIgnoreCase) Then
                        CompleteQuickLogin(confirmEmailLink)
                    End If
                End If
            ElseIf Request.QueryString("confirmAccountSafe") = "1" AndAlso Not String.IsNullOrEmpty(confirmEmailLink) AndAlso Not String.IsNullOrEmpty(confirmTsLink) AndAlso Not String.IsNullOrEmpty(confirmSigLink) Then
                Dim issuedAtSafe As DateTime
                If DateTime.TryParse(confirmTsLink, issuedAtSafe) Then
                    If VerifyConfirmationSignatureSafe(confirmEmailLink, "AccountCreated", issuedAtSafe, confirmSigLink) Then
                        CompleteQuickLoginSafe(confirmEmailLink)
                    End If
                End If
            End If
        End Sub

        ''' <summary>
        ''' Re-derives AddNewCustomer.aspx's hardened confirmation-link signature (salted SHA-256,
        ''' "saltHex:hash") using the salt carried in the link itself, and compares it to the
        ''' link's own hash half.
        ''' </summary>
        Private Function VerifyConfirmationSignatureSafe(ByVal accountIdentifier As String, ByVal eventKind As String, ByVal issuedAtUtc As DateTime, ByVal saltedSig As String) As Boolean
            Dim parts() As String = saltedSig.Split(":"c)
            If parts.Length <> 2 Then
                Return False
            End If
            Dim saltHex As String = parts(0)
            Dim expectedHash As String = parts(1)
            Using sha256 As SHA256 = SHA256.Create()
                Dim input As String = accountIdentifier & "|" & eventKind & "|" & issuedAtUtc.ToString("O") & "|" & saltHex
                Dim hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input))
                Dim actualHash As String = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
                Return String.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase)
            End Using
        End Function

        ''' <summary>
        ''' Signs the customer straight in once their account-confirmation link has verified.
        ''' </summary>
        Private Sub CompleteQuickLogin(ByVal email As String)
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                FormsAuthentication.FormsCookiePath)
            Dim cookie As HttpCookie = CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
            ' SINK: PLANTED-VB-HR-547
            Response.Cookies.Add(cookie)
        End Sub

        ''' <summary>
        ''' Hardened twin of CompleteQuickLogin: abandons whatever session preceded the
        ''' confirmation click before issuing the now-authenticated cookie.
        ''' </summary>
        Private Sub CompleteQuickLoginSafe(ByVal email As String)
            Session.Abandon()
            Dim ticket As New FormsAuthenticationTicket(
                1, email, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                FormsAuthentication.FormsCookiePath)
            Dim cookie As HttpCookie = CookieManager.SetCookie(ticket, FormsAuthentication.FormsCookieName, email)
            ' SAFE_SINK: PLANTED-VB-HR-547-safe
            Response.Cookies.Add(cookie)
        End Sub

        Protected Sub ButtonLogOn_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim email As String = txtUserName.Text
            Dim pwd As String = txtPassword.Text

            log.Info("User " & email & " attempted to log in with password " & pwd)

            If Not du.IsValidCustomerLogin(email, pwd) Then
                labelError.Text = "Incorrect username/password"
                PanelError.Visible = True
                Return
            End If

            ' put ticket into the cookie
            Dim ticket As New FormsAuthenticationTicket(
                            1, 'version
                            email, 'name
                            DateTime.Now, ' issueDate
                            DateTime.Now.AddDays(14), ' expireDate
                            True, ' isPersistent
                            "customer", ' userData (customer role)
                            FormsAuthentication.FormsCookiePath ' cookiePath
            )

            Dim encrypted_ticket As String = FormsAuthentication.Encrypt(ticket) ' encrypt the ticket

            ' put ticket into the cookie
            Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encrypted_ticket)

            ' set expiration date
            If ticket.IsPersistent Then
                cookie.Expires = ticket.Expiration
            End If

            Response.Cookies.Add(cookie)

            ' "Remember this device" convenience cookie: a separate, longer-lived token from the
            ' FormsAuth ticket above, so a returning customer skips re-entering their password on
            ' this browser. Opt-in via a "remember" link/query param on the login form.
            If Request.QueryString("remember") = "1" Then
                Dim rememberCookie As New HttpCookie("rememberDeviceToken", Guid.NewGuid().ToString("N"))
                rememberCookie.Expires = DateTime.Now.AddDays(30)
                ' SINK: PLANTED-VB-HR-81
                Response.Cookies.Add(rememberCookie)
            End If

            If Request.QueryString("rememberSafe") = "1" Then
                Dim rememberCookieSafe As New HttpCookie("rememberDeviceTokenSafe", Guid.NewGuid().ToString("N"))
                rememberCookieSafe.Expires = DateTime.Now.AddDays(30)
                rememberCookieSafe.HttpOnly = True
                ' SAFE_SINK: PLANTED-VB-HR-81-safe
                Response.Cookies.Add(rememberCookieSafe)
            End If

            Dim returnUrl As String = Request.QueryString("ReturnUrl")

            If returnUrl Is Nothing Then
                returnUrl = "/WebGoatCoins/MainPage.aspx"
            End If

            Response.Redirect(returnUrl)
        End Sub
    End Class
End Namespace
