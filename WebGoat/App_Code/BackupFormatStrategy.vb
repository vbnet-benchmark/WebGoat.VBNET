Imports System
Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Web.Script.Serialization

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Restores a session snapshot for Content/Viewstate.aspx's "resume where I left off"
    ''' feature. Two concrete implementations exist, chosen at runtime by Viewstate.aspx.vb based
    ''' on the "format" query parameter: one restores via the legacy BinaryFormatter-based
    ''' snapshot format this feature originally shipped with, the other via the newer, JSON-based
    ''' format.
    ''' </summary>
    Public MustInherit Class BackupFormatStrategy
        Public MustOverride Function Restore(ByVal rawSnapshot As String) As Object
    End Class

    ''' <summary>
    ''' The original snapshot format: a Base64-encoded BinaryFormatter stream. Retained for
    ''' visitors whose bookmarked "resume" link predates the JSON format below.
    ''' </summary>
    Public Class LegacyBinaryBackupStrategy
        Inherits BackupFormatStrategy

        Public Overrides Function Restore(ByVal rawSnapshot As String) As Object
            Dim bytes As Byte() = Convert.FromBase64String(rawSnapshot)
            Dim ms As New MemoryStream(bytes)
            ' SINK: PLANTED-VB-HR-124
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function
    End Class

    ''' <summary>
    ''' The current snapshot format: a Base64-encoded JSON document, restored with a plain
    ''' JavaScriptSerializer (no type resolver) so only the declared Dictionary(Of String,
    ''' String) shape can ever be reconstructed.
    ''' </summary>
    Public Class JsonBackupStrategy
        Inherits BackupFormatStrategy

        Public Overrides Function Restore(ByVal rawSnapshot As String) As Object
            Dim bytes As Byte() = Convert.FromBase64String(rawSnapshot)
            Dim json As String = Encoding.UTF8.GetString(bytes)
            ' SAFE_SINK: PLANTED-VB-HR-124-safe
            Return (New JavaScriptSerializer()).Deserialize(Of Dictionary(Of String, String))(json)
        End Function
    End Class
End Namespace
