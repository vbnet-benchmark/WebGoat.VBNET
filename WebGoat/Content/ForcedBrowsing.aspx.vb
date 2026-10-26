Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class ForcedBrowsing
        Inherits System.Web.UI.Page

        ''' <summary>
        ''' Demonstrates the "Access Denied, continuing you on to where you asked to go" pattern a
        ''' forced-browsing guard often falls back to: the page you tried to force-browse to may be
        ''' off-limits, but rather than just stop there, the guard still sends you on -- to the very
        ''' URL you tried to force-browse to in the first place.
        ''' </summary>
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            Dim target As String = Request.QueryString("target")
            If Not String.IsNullOrEmpty(target) Then
                Dim resolved As String = NavigationHelper.ResolveRequestedPage(target)
                ' SINK: PLANTED-VB-HR-343
                Response.Redirect(resolved)
            End If

            Dim targetSafe As String = Request.QueryString("targetSafe")
            If Not String.IsNullOrEmpty(targetSafe) Then
                Dim resolvedSafe As String = NavigationHelper.ResolveRequestedPageSafe(targetSafe)
                ' SAFE_SINK: PLANTED-VB-HR-343-safe
                Response.Redirect(resolvedSafe)
            End If
        End Sub
    End Class
End Namespace