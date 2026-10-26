Imports System
Imports System.Net.Sockets

Namespace OWASP.WebGoat.NET

    ''' <summary>
    ''' Base type for the "Connection Mode" dropdown in the Arbitrary Server Connection lesson.
    ''' The lesson page constructs one of the two concrete resolvers below based purely on the
    ''' selected mode, holds the result as the base type, and calls <see cref="Connect"/> at a
    ''' single shared call site -- which concrete connection behavior runs depends entirely on
    ''' virtual dispatch, not on any branching visible at that call site.
    ''' </summary>
    Public MustInherit Class ServerConnectionResolverBase
        Public MustOverride Function Connect(hostName As String, portText As String) As String
    End Class

    ''' <summary>
    ''' "Legacy" connection mode: opens a raw TCP connection to whatever host/port it is given,
    ''' with no validation -- preserved for backward compatibility with older monitoring scripts.
    ''' </summary>
    Public Class LegacyConnectionResolver
        Inherits ServerConnectionResolverBase

        Public Overrides Function Connect(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            Try
                Using client As New TcpClient(hostName, portNumber) ' SINK: PLANTED-VB-HR-709
                    Return "Legacy connector reached " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Legacy connector failed: " & ex.Message
            End Try
        End Function
    End Class

    ''' <summary>
    ''' "Validated" connection mode: the same raw TCP connection, but only after the target
    ''' host/port pair is confirmed to be on the approved connectivity allow-list.
    ''' </summary>
    Public Class ValidatedConnectionResolver
        Inherits ServerConnectionResolverBase

        Public Overrides Function Connect(hostName As String, portText As String) As String
            Dim portNumber As Integer
            If Not Integer.TryParse(portText, portNumber) Then
                Return "Invalid port."
            End If

            If Not ConnectionAllowlist.IsAllowed(hostName, portNumber) Then
                Return "Rejected: target is not on the approved connectivity allow-list."
            End If

            Try
                Using client As New TcpClient(hostName, portNumber) ' SAFE_SINK: PLANTED-VB-HR-709-safe
                    Return "Validated connector reached " & hostName & ":" & portNumber
                End Using
            Catch ex As Exception
                Return "Validated connector failed: " & ex.Message
            End Try
        End Function
    End Class

End Namespace
