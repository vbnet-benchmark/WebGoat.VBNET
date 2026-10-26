Imports System
Imports System.Net.Sockets

Namespace OWASP.WebGoat.NET

    ''' <summary>
    ''' The actual network hop of the "Partner Endpoint Connectivity Probe" feature: opens a raw
    ''' TCP connection to whatever host/port it is handed. Called only from
    ''' <see cref="NetworkProbeService"/>, one file removed from the lesson page itself.
    ''' </summary>
    Public Class RawConnectionProbe

        Public Shared Function Connect(hostName As String, portNumber As Integer) As String
            Try
                Using client As New TcpClient(hostName, portNumber) ' SINK: PLANTED-VB-HR-708
                    Return "Partner endpoint reachable: " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Partner endpoint probe failed: " & ex.Message
            End Try
        End Function

        Public Shared Function ConnectSafe(hostName As String, portNumber As Integer) As String
            If Not ConnectionAllowlist.IsAllowed(hostName, portNumber) Then
                Return "Rejected: target is not on the approved connectivity allow-list."
            End If

            Try
                Using client As New TcpClient(hostName, portNumber) ' SAFE_SINK: PLANTED-VB-HR-708-safe
                    Return "Partner endpoint reachable: " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Partner endpoint probe failed: " & ex.Message
            End Try
        End Function

    End Class

End Namespace
