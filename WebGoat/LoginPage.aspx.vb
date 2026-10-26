Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
'Imports TechInfoSystems.Data.SQLite
Imports System.Web.Security
Imports System.Web.Configuration
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Public Partial Class LoginPage
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ' Federated "log in with WebGoat Coins" hand-off: a partner site redirects the visitor
            ' here with an asserted identity and which completion contract to use.
            Dim ssoEmail As String = Request.QueryString("ssoEmail")
            Dim ssoMode As String = Request.QueryString("ssoMode")
            If Not String.IsNullOrEmpty(ssoEmail) Then
                Dim strategy As LoginCompletionStrategy
                If String.Equals(ssoMode, "partner", StringComparison.OrdinalIgnoreCase) Then
                    strategy = New PartnerSsoLoginCompletion()
                Else
                    strategy = New StandardLoginCompletion()
                End If
                Dim ssoCookie As HttpCookie = strategy.CompleteLogin(ssoEmail)
                Response.Cookies.Add(ssoCookie)
            End If
        End Sub

        Protected Sub ButtonLogOn_Click(ByVal sender As Object, ByVal e As EventArgs)
            Response.Redirect("/WebGoatCoins/CustomerLogin.aspx")

            'If Membership.ValidateUser(txtUserName.Value.Trim(), txtPassword.Value.Trim()) Then
            '    FormsAuthentication.RedirectFromLoginPage(txtUserName.Value, True)
            'Else
            '    labelMessage.Text = "invalid username"
            'End If
        End Sub

        Protected Sub ButtonAdminLogOn_Click(ByVal sender As Object, ByVal e As EventArgs)
        End Sub
    End Class
End Namespace