Imports System

Namespace OWASP.WebGoat.NET

    ''' <summary>
    ''' First hop of the interprocedural "Partner Endpoint Connectivity Probe" chain: validates
    ''' the port text and hands the raw host/port pair off to <see cref="RawConnectionProbe"/>
    ''' (a second, separate App_Code file) to actually open the connection.
    ''' </summary>
    Public Class NetworkProbeService

        Public Shared Function ProbePartnerEndpoint(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            Return RawConnectionProbe.Connect(hostName, portNumber)
        End Function

        Public Shared Function ProbePartnerEndpointSafe(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            Return RawConnectionProbe.ConnectSafe(hostName, portNumber)
        End Function

    End Class

End Namespace
