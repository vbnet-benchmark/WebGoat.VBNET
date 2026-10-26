Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class Logout
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            ' A logout link embedded in an email or a partner banner can carry a "ReturnUrl" so
            ' following it both signs the visitor out and sends them back to the page they were
            ' reading, instead of always landing on the home page.
            Dim returnUrl As String = Request.QueryString("ReturnUrl")
            If Not String.IsNullOrEmpty(returnUrl) Then
                FormsAuthentication.SignOut()
                ' SINK: PLANTED-VB-HR-341
                Response.Redirect(returnUrl)
            End If

            ' Hardened twin: only a genuine application-relative path is accepted -- a single
            ' leading "/", never "//", which several browsers resolve as protocol-relative and
            ' would send the visitor off-site using whatever scheme this page itself was loaded
            ' over.
            Dim returnUrlSafe As String = Request.QueryString("ReturnUrlSafe")
            If Not String.IsNullOrEmpty(returnUrlSafe) Then
                FormsAuthentication.SignOut()
                If returnUrlSafe.StartsWith("/") AndAlso Not returnUrlSafe.StartsWith("//") Then
                    ' SAFE_SINK: PLANTED-VB-HR-341-safe
                    Response.Redirect(returnUrlSafe)
                Else
                    Response.Redirect("/Default.aspx")
                End If
            End If
        End Sub

        Protected Sub btnLogout_Click(sender As Object, e As EventArgs)
            FormsAuthentication.SignOut()
            Response.Redirect("/Default.aspx")
        End Sub
    End Class
End Namespace