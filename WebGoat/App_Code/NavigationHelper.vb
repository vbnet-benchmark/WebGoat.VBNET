Imports System

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' First hop for Content/ForcedBrowsing.aspx's "Access Denied, continuing you on to where you
    ''' asked to go" fallback: forwards the caller's requested destination on to
    ''' RedirectTargetResolver, which actually decides what comes back.
    ''' </summary>
    Public Class NavigationHelper
        Public Shared Function ResolveRequestedPage(ByVal raw As String) As String
            Return RedirectTargetResolver.Resolve(raw)
        End Function

        ''' <summary>
        ''' Safe twin of ResolveRequestedPage, routed through RedirectTargetResolver.ResolveSafe.
        ''' </summary>
        Public Shared Function ResolveRequestedPageSafe(ByVal raw As String) As String
            Return RedirectTargetResolver.ResolveSafe(raw)
        End Function
    End Class
End Namespace
