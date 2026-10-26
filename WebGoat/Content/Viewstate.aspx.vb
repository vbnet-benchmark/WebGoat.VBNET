Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class Viewstate
        Inherits System.Web.UI.Page
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ' Let a returning visitor resume a previously-saved session snapshot via a "resume"
            ' link that names which snapshot format to use -- "format=json" is the current
            ' format; any other (or missing) value falls back to the original BinaryFormatter-
            ' based format this feature shipped with first.
            Dim snapshot As String = Request.QueryString("snapshot")
            If Not String.IsNullOrEmpty(snapshot) Then
                Dim format As String = Request.QueryString("format")
                Dim strategy As BackupFormatStrategy

                If String.Equals(format, "json", StringComparison.OrdinalIgnoreCase) Then
                    strategy = New JsonBackupStrategy()
                Else
                    strategy = New LegacyBinaryBackupStrategy()
                End If

                Dim restored As Dictionary(Of String, String) = TryCast(strategy.Restore(snapshot), Dictionary(Of String, String))
                If restored IsNot Nothing Then
                    Response.Write("<div class='snapshot'>Restored snapshot with " & restored.Count.ToString() & " entries</div>")
                End If
            End If
        End Sub
    End Class
End Namespace