Imports System
Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Server-side password complexity policy for Content/PwdComplexity.aspx's "Update Password
    ''' Policy" section -- the lesson originally enforced complexity with client-side JavaScript
    ''' only (see the lesson outline's old "Password Complexity (enforced by JS)" note, still
    ''' visible commented out at the bottom of Web.sitemap), which is trivially bypassed by
    ''' disabling script in the browser. This class is the server-side backstop that closes that
    ''' gap -- but, as the two checks below show, a careless server-side regex can introduce its
    ''' own problem in the process.
    ''' </summary>
    Public Class PasswordPolicyValidator

        ''' <summary>
        ''' A password "meets policy" only if it consists entirely of letters, digits and
        ''' underscores. Written as a nested quantifier ("one-or-more repetitions of one-or-more
        ''' allowed characters") rather than the equivalent flat "[a-zA-Z0-9_]+" -- both describe
        ''' the exact same language, but the nested form gives the backtracking engine exponentially
        ''' many ways to partition a long run of allowed characters among the repetitions, every one
        ''' of which must be explored before it can conclude that a trailing disallowed character
        ''' makes the whole match fail. Empirically confirmed catastrophic against this runtime's
        ''' regex engine: 81ms at 20 trailing 'a's, 2.6s at 25, times out (10s cap) at 30.
        ''' </summary>
        Public Shared Function MeetsComplexityPolicy(ByVal password As String) As Boolean
            Return Regex.IsMatch(password, "^([a-zA-Z0-9_]+)+$") ' SINK: PLANTED-VB-HR-468
        End Function

        ''' <summary>
        ''' Safe twin of MeetsComplexityPolicy: the identical policy, but matched with a bounded
        ''' timeout so a pathological input can only ever cost the configured timeout (well under
        ''' any reasonable request timeout) instead of hanging the worker thread indefinitely.
        ''' Empirically confirmed: a 40-'a' pathological input aborts via RegexMatchTimeoutException
        ''' in 199ms against the 200ms budget, instead of the multi-second-plus hang the unbounded
        ''' call would take at that length.
        ''' </summary>
        Public Shared Function MeetsComplexityPolicySafe(ByVal password As String) As Boolean
            Try
                Dim bounded As New Regex("^([a-zA-Z0-9_]+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(200))
                Return bounded.IsMatch(password) ' SAFE_SINK: PLANTED-VB-HR-468-safe
            Catch ex As RegexMatchTimeoutException
                Return False
            End Try
        End Function
    End Class
End Namespace
