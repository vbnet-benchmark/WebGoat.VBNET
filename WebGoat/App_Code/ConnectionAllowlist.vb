Imports System

Namespace OWASP.WebGoat.NET

    ''' <summary>
    ''' Shared allow-list used by the "Arbitrary Server Connection" lesson's safe/hardened
    ''' connectivity-check paths: a raw outbound connection is only permitted when BOTH the
    ''' target host and the target port are on a short, fixed, server-side roster. Anything
    ''' request-derived that does not match is rejected rather than attempted.
    ''' </summary>
    Public Class ConnectionAllowlist

        Private Shared ReadOnly AllowedHosts() As String = {
            "partner-api.internal.example.com",
            "metrics.internal.example.com",
            "payments-gateway.internal.example.com"
        }

        Private Shared ReadOnly AllowedPorts() As Integer = {443, 8443}

        Public Shared Function IsAllowed(hostName As String, portNumber As Integer) As Boolean
            If String.IsNullOrWhiteSpace(hostName) Then
                Return False
            End If

            Dim hostMatches As Boolean = False
            For Each candidateHost As String In AllowedHosts
                If String.Equals(candidateHost, hostName, StringComparison.OrdinalIgnoreCase) Then
                    hostMatches = True
                    Exit For
                End If
            Next

            If Not hostMatches Then
                Return False
            End If

            Dim portMatches As Boolean = False
            For Each candidatePort As Integer In AllowedPorts
                If candidatePort = portNumber Then
                    portMatches = True
                    Exit For
                End If
            Next

            Return portMatches
        End Function

    End Class

End Namespace
