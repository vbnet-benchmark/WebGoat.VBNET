Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Builds the argument string passed to the external name-resolution tool for the "Network
    ''' Diagnostics" lesson's "Resolve Host" feature. Dispatch is chosen at runtime (the lesson's
    ''' "Lookup Backend" dropdown) so the lesson can show both the original behavior and the
    ''' hardened replacement side by side without duplicating the process-launch plumbing.
    ''' </summary>
    Public MustInherit Class DiagnosticToolInvoker
        Public MustOverride Function BuildLookupArguments(ByVal target As String) As String
    End Class

    ''' <summary>
    ''' The original lookup backend this lesson shipped with: forwards the submitted host
    ''' straight through to the resolver tool's command line.
    ''' </summary>
    Public Class LegacyLookupInvoker
        Inherits DiagnosticToolInvoker

        Public Overrides Function BuildLookupArguments(ByVal target As String) As String
            Return target
        End Function
    End Class

    ''' <summary>
    ''' The current lookup backend: only forwards a value that looks like a plain hostname or IP
    ''' address (letters, digits, dots and hyphens), so nothing else the resolver's command line
    ''' would otherwise interpret can ever reach it.
    ''' </summary>
    Public Class ValidatedLookupInvoker
        Inherits DiagnosticToolInvoker

        Public Overrides Function BuildLookupArguments(ByVal target As String) As String
            If target Is Nothing OrElse Not Regex.IsMatch(target, "^[A-Za-z0-9.-]+$") Then
                Return "invalid-host.example"
            End If
            Return target
        End Function
    End Class
End Namespace
