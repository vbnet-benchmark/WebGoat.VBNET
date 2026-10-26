Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace OWASP.WebGoat.NET
    Partial Public Class LogoutIssues
        Inherits System.Web.UI.Page

        ''' <summary>
        ''' A handful of session-timeout/forced-logout flows (an idle-session kick, an admin-forced
        ''' logout) land here with a "next" query parameter naming where the visitor should be sent
        ''' once the logout completes, so they return to the page that triggered the logout rather
        ''' than always landing on the home page.
        ''' </summary>
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            Dim nextPage As String = Request.QueryString("next")
            If Not String.IsNullOrEmpty(nextPage) Then
                Dim destination As String = ResolveLogoutDestination(nextPage)
                ' SINK: PLANTED-VB-HR-342
                Response.Redirect(destination)
            End If

            Dim nextPageSafe As String = Request.QueryString("nextSafe")
            If Not String.IsNullOrEmpty(nextPageSafe) Then
                Dim destinationSafe As String = ResolveLogoutDestinationSafe(nextPageSafe)
                ' SAFE_SINK: PLANTED-VB-HR-342-safe
                Response.Redirect(destinationSafe)
            End If
        End Sub

        ''' <summary>
        ''' Forwards the requested post-logout destination unchanged -- the "next" value is taken
        ''' as-is, on the assumption that only this site's own pages would ever be named here.
        ''' </summary>
        Private Function ResolveLogoutDestination(ByVal raw As String) As String
            Return raw
        End Function

        ''' <summary>
        ''' Hardened twin of ResolveLogoutDestination: only a known, fixed set of post-logout
        ''' destinations is permitted.
        ''' </summary>
        Private Shared ReadOnly AllowedLogoutDestinations As String() = {"/Default.aspx", "/LoginPage.aspx"}

        Private Function ResolveLogoutDestinationSafe(ByVal raw As String) As String
            For Each allowed As String In AllowedLogoutDestinations
                If String.Equals(raw, allowed, StringComparison.OrdinalIgnoreCase) Then
                    Return allowed
                End If
            Next
            Return "/Default.aspx"
        End Function
    End Class
End Namespace