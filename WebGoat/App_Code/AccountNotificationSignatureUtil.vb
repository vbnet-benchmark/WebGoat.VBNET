Imports System
Imports System.Security.Cryptography
Imports System.Text

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Shared helpers for signing the confirmation links embedded in transactional account
    ''' emails across the site (new-account confirmation from AddNewCustomer.aspx, order
    ''' confirmation from Buy.aspx) so the server can detect whether the identity/event/issued-at
    ''' fields on the link were altered before the link is trusted.
    ''' </summary>
    Public Class AccountNotificationSignatureUtil

        ''' <summary>
        ''' SECURITY-SENSITIVE: a forgeable signature here lets an attacker craft a confirmation
        ''' link for an arbitrary identity/event pair that still "looks" signed -- e.g. to
        ''' silently confirm an action against someone else's account before it is ever looked up
        ''' in the database.
        ''' </summary>
        Public Shared Function ComputeNotificationSignature(ByVal accountIdentifier As String, ByVal eventKind As String, ByVal issuedAtUtc As DateTime) As String
            Using md5 As MD5 = MD5.Create()
                Dim input As String = accountIdentifier & "|" & eventKind & "|" & issuedAtUtc.ToString("O")
                Dim hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input)) ' SINK: PLANTED-VB-HR-413
                Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Function

        ''' <summary>
        ''' Safe twin of ComputeNotificationSignature -- salted with a fresh random value per
        ''' call and hashed with SHA-256, so the signature cannot be reproduced without also
        ''' knowing that call's salt.
        ''' </summary>
        Public Shared Function ComputeNotificationSignatureSafe(ByVal accountIdentifier As String, ByVal eventKind As String, ByVal issuedAtUtc As DateTime) As String
            Dim saltBytes(15) As Byte
            Using rng As New RNGCryptoServiceProvider()
                rng.GetBytes(saltBytes)
            End Using
            Dim saltHex As String = BitConverter.ToString(saltBytes).Replace("-", "").ToLowerInvariant()
            Using sha256 As SHA256 = SHA256.Create()
                Dim input As String = accountIdentifier & "|" & eventKind & "|" & issuedAtUtc.ToString("O") & "|" & saltHex
                Dim hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input)) ' SAFE_SINK: PLANTED-VB-HR-413-safe
                Return saltHex & ":" & BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Function

    End Class
End Namespace
