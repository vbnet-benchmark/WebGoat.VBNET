Imports System

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Second hop for Content/ForcedBrowsing.aspx's "Access Denied, continuing you on to where you
    ''' asked to go" fallback, reached only through NavigationHelper. Resolve is the raw, unchecked
    ''' form -- the destination you tried to force-browse to is handed straight back so you still
    ''' end up there. ResolveSafe only ever returns one of a small, fixed set of known-internal
    ''' destinations.
    ''' </summary>
    Public Class RedirectTargetResolver
        Private Shared ReadOnly AllowedDestinations As String() = {"/Default.aspx", "/Content/SamplePage.aspx"}

        Public Shared Function Resolve(ByVal raw As String) As String
            Return raw
        End Function

        Public Shared Function ResolveSafe(ByVal raw As String) As String
            For Each allowed As String In AllowedDestinations
                If String.Equals(raw, allowed, StringComparison.OrdinalIgnoreCase) Then
                    Return allowed
                End If
            Next
            Return "/Default.aspx"
        End Function
    End Class
End Namespace
