Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Restores a customer's exported backup for WebGoatCoins/CustomerSettings.aspx's "restore
    ''' from backup code" feature and for WebGoatCoins/Orders.aspx's "restore saved order draft
    ''' notes" feature. A backup code is produced by a prior "export" action (not modeled
    ''' separately here) as a Base64-encoded, serialized payload, optionally paired with an HMAC
    ''' signature over the same bytes.
    ''' </summary>
    Public Class PreferenceBackupService

        ' Demo signing key for the MAC-protected restore paths below -- in a real deployment this
        ' would come from a secrets store, mirroring how ASP.NET's own EnableViewStateMac relies on
        ' a server-only machineKey. The mitigation SHAPE under test here (verify-before-deserialize,
        ' fail closed on any mismatch) is independent of how the key itself is managed.
        Private Shared ReadOnly SigningKey As Byte() = Encoding.UTF8.GetBytes("WebGoatCoins-Backup-Signing-Key-Demo")

        ''' <summary>
        ''' Restores a single exported backup value (e.g. a preferences dictionary). Used by
        ''' CustomerSettings.aspx's "restore from backup code" feature.
        ''' </summary>
        Public Shared Function RestoreBackup(ByVal rawBackup As String) As Object
            If String.IsNullOrEmpty(rawBackup) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(rawBackup)
            Return DeserializePayload(bytes)
        End Function

        ''' <summary>
        ''' Same as RestoreBackup, but requires a matching HMAC signature over the payload bytes
        ''' before ever attempting to deserialize them -- an attacker without SigningKey cannot
        ''' forge a bytes+signature pair that passes, so the bytes reaching DeserializePayloadSafe
        ''' are provably unmodified since this server's own earlier "export" step produced them.
        ''' </summary>
        Public Shared Function RestoreBackupSafe(ByVal rawBackup As String, ByVal signatureBase64 As String) As Object
            If String.IsNullOrEmpty(rawBackup) OrElse String.IsNullOrEmpty(signatureBase64) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(rawBackup)
            Dim providedSignature As Byte() = Convert.FromBase64String(signatureBase64)

            Using hmac As New HMACSHA256(SigningKey)
                Dim expectedSignature As Byte() = hmac.ComputeHash(bytes)
                If Not SignaturesMatch(expectedSignature, providedSignature) Then Return Nothing
            End Using

            Return DeserializePayloadSafe(bytes)
        End Function

        ''' <summary>
        ''' Restores one item out of a multi-item backup (e.g. one order's draft note out of a
        ''' batch). A separate entry point from RestoreBackup/RestoreBackupSafe so
        ''' Orders.aspx.vb's per-item loop has its own call site, distinct from
        ''' CustomerSettings.aspx.vb's single-value restore.
        ''' </summary>
        Public Shared Function RestoreBackupItem(ByVal rawBackup As String) As Object
            If String.IsNullOrEmpty(rawBackup) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(rawBackup)
            Dim ms As New MemoryStream(bytes)
            ' SINK: PLANTED-VB-HR-125
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function

        ''' <summary>
        ''' Same as RestoreBackupItem, but requires a matching HMAC signature over the item's
        ''' payload bytes before ever attempting to deserialize them (see RestoreBackupSafe).
        ''' </summary>
        Public Shared Function RestoreBackupItemSafe(ByVal rawBackup As String, ByVal signatureBase64 As String) As Object
            If String.IsNullOrEmpty(rawBackup) OrElse String.IsNullOrEmpty(signatureBase64) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(rawBackup)
            Dim providedSignature As Byte() = Convert.FromBase64String(signatureBase64)

            Using hmac As New HMACSHA256(SigningKey)
                Dim expectedSignature As Byte() = hmac.ComputeHash(bytes)
                If Not SignaturesMatch(expectedSignature, providedSignature) Then Return Nothing
            End Using

            Dim ms As New MemoryStream(bytes)
            ' SAFE_SINK: PLANTED-VB-HR-125-safe
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function

        Private Shared Function SignaturesMatch(ByVal a As Byte(), ByVal b As Byte()) As Boolean
            If a.Length <> b.Length Then Return False
            Dim diff As Integer = 0

            For i As Integer = 0 To a.Length - 1
                diff = diff Or (a(i) Xor b(i))
            Next

            Return diff = 0
        End Function

        Private Shared Function DeserializePayload(ByVal bytes As Byte()) As Object
            Dim ms As New MemoryStream(bytes)
            ' SINK: PLANTED-VB-HR-123
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function

        ''' <summary>
        ''' The MAC verified by the caller (RestoreBackupSafe) proves these exact bytes were
        ''' produced by this server's own earlier "export" step and have not been modified since --
        ''' deserializing here is safe even though BinaryFormatter itself is still capable of
        ''' reconstructing any type, because an attacker can never reach this call with bytes of
        ''' their own choosing. Mirrors ASP.NET's own EnableViewStateMac verify-before-deserialize
        ''' shape.
        ''' </summary>
        Private Shared Function DeserializePayloadSafe(ByVal bytes As Byte()) As Object
            Dim ms As New MemoryStream(bytes)
            ' SAFE_SINK: PLANTED-VB-HR-123-safe
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function
    End Class
End Namespace
