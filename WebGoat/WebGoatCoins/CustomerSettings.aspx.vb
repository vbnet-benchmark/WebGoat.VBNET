Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.Security
Imports System.Data
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class CustomerSettings
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            ' Display a single preference value back to the customer for confirmation.
            ' prefMode=encoded uses the HTML-encoding renderer; any other value (or none)
            ' uses the plain echo renderer.
            Dim prefValue As String = Request.QueryString("prefValue")
            If Not String.IsNullOrEmpty(prefValue) Then
                Dim mode As String = Request.QueryString("prefMode")
                If String.Equals(mode, "encoded", StringComparison.OrdinalIgnoreCase) Then
                    Dim safeRenderer As PreferenceValue = New EncodedPreferenceValue(prefValue)
                    ' SAFE_SINK: PLANTED-VB-HR-9-safe
                    Response.Write("<div class='pref'>Preference: " & safeRenderer.Render() & "</div>")
                Else
                    Dim rawRenderer As PreferenceValue = New EchoPreferenceValue(prefValue)
                    ' SINK: PLANTED-VB-HR-9
                    Response.Write("<div class='pref'>Preference: " & rawRenderer.Render() & "</div>")
                End If

                ' Optionally remember this preference across visits via a long-lived cookie,
                ' built through CookieManager's shared cookie-construction helper.
                If Request.QueryString("rememberPref") = "1" Then
                    Dim themeCookie As HttpCookie = CookieManager.CreatePreferenceCookie("themePref", prefValue, 30)
                    ' SINK: PLANTED-VB-HR-82
                    Response.Cookies.Add(themeCookie)
                End If

                If Request.QueryString("rememberPrefSafe") = "1" Then
                    Dim themeCookieSafe As HttpCookie = CookieManager.CreatePreferenceCookieSafe("themePrefSafe", prefValue, 30)
                    ' SAFE_SINK: PLANTED-VB-HR-82-safe
                    Response.Cookies.Add(themeCookieSafe)
                End If
            End If

            ' Let a returning customer pick which "cookie policy" implementation handles their
            ' session-tracker cookie -- a historical opt-out for script-heavy themes that read it
            ' back via JS, retained alongside the modern strict default.
            Dim policyMode As String = Request.QueryString("cookiePolicy")
            If Not String.IsNullOrEmpty(policyMode) Then
                Dim trackerCookie As New HttpCookie("sessionTrackerToken", Guid.NewGuid().ToString("N"))

                If String.Equals(policyMode, "strict", StringComparison.OrdinalIgnoreCase) Then
                    Dim policy As CookieSecurityPolicy = New StrictCookiePolicy()
                    ' SAFE_SINK: PLANTED-VB-HR-84-safe
                    Response.Cookies.Add(policy.Apply(trackerCookie))
                Else
                    Dim policy As CookieSecurityPolicy = New LegacyCookiePolicy()
                    ' SINK: PLANTED-VB-HR-84
                    Response.Cookies.Add(policy.Apply(trackerCookie))
                End If
            End If

            ' Let a returning customer restore every preference at once from a backup code they
            ' saved from a prior "export my preferences" action, routed through the shared
            ' PreferenceBackupService so the restore logic lives in one place instead of being
            ' duplicated across pages.
            Dim prefBackup As String = Request.QueryString("prefBackup")
            If Not String.IsNullOrEmpty(prefBackup) Then
                Dim restored As Dictionary(Of String, String) = TryCast(PreferenceBackupService.RestoreBackup(prefBackup), Dictionary(Of String, String))
                If restored IsNot Nothing Then
                    Response.Write("<div class='pref-backup'>Restored " & restored.Count.ToString() & " preference(s)</div>")
                End If
            End If

            Dim prefBackupSafe As String = Request.QueryString("prefBackupSafe")
            Dim prefBackupMac As String = Request.QueryString("prefBackupMac")
            If Not String.IsNullOrEmpty(prefBackupSafe) AndAlso Not String.IsNullOrEmpty(prefBackupMac) Then
                Dim restoredSafe As Dictionary(Of String, String) = TryCast(PreferenceBackupService.RestoreBackupSafe(prefBackupSafe, prefBackupMac), Dictionary(Of String, String))
                If restoredSafe IsNot Nothing Then
                    Response.Write("<div class='pref-backup-safe'>Restored " & restoredSafe.Count.ToString() & " preference(s)</div>")
                End If
            End If

            ' "View/sign in as any of my family's accounts" convenience: looks up every account
            ' sharing this household's email-alias prefix (e.g. family@example.com,
            ' family+kid1@example.com) and signs the customer into each one in turn, so they can
            ' review every linked account's settings in one visit instead of logging out and back
            ' in once per account.
            Dim linkedEmailPrefix As String = Request.QueryString("linkedEmailPrefix")
            If Not String.IsNullOrEmpty(linkedEmailPrefix) Then
                Dim linkedAccounts As DataSet = du.GetCustomerEmails(linkedEmailPrefix)
                If linkedAccounts IsNot Nothing Then
                    For Each row As DataRow In linkedAccounts.Tables(0).Rows
                        Dim linkedEmail As String = row("email").ToString()
                        Dim linkedTicket As New FormsAuthenticationTicket(
                            1, linkedEmail, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                            FormsAuthentication.FormsCookiePath)
                        Dim linkedCookie As HttpCookie = CookieManager.SetCookie(linkedTicket, FormsAuthentication.FormsCookieName, linkedEmail)
                        ' SINK: PLANTED-VB-HR-550
                        Response.Cookies.Add(linkedCookie)
                    Next
                End If
            End If

            ' Hardened twin: the session is abandoned once, before the loop begins, so none of the
            ' linked accounts' now-authenticated cookies can ever be carried by a session an
            ' attacker pre-set on this visitor.
            Dim linkedEmailPrefixSafe As String = Request.QueryString("linkedEmailPrefixSafe")
            If Not String.IsNullOrEmpty(linkedEmailPrefixSafe) Then
                Dim linkedAccountsSafe As DataSet = du.GetCustomerEmails(linkedEmailPrefixSafe)
                If linkedAccountsSafe IsNot Nothing Then
                    Session.Abandon()
                    For Each row As DataRow In linkedAccountsSafe.Tables(0).Rows
                        Dim linkedEmailSafe As String = row("email").ToString()
                        Dim linkedTicketSafe As New FormsAuthenticationTicket(
                            1, linkedEmailSafe, DateTime.Now, DateTime.Now.AddDays(14), False, "customer",
                            FormsAuthentication.FormsCookiePath)
                        Dim linkedCookieSafe As HttpCookie = CookieManager.SetCookie(linkedTicketSafe, FormsAuthentication.FormsCookieName, linkedEmailSafe)
                        ' SAFE_SINK: PLANTED-VB-HR-550-safe
                        Response.Cookies.Add(linkedCookieSafe)
                    Next
                End If
            End If
        End Sub
    End Class
End Namespace