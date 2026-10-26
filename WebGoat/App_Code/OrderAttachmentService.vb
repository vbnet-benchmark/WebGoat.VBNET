Imports System
Imports System.IO
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Shared attachment-preview loading for WebGoatCoins/OrderAttachments.aspx's "preview" link
    ''' and, eventually, WebGoatCoins/Invoice.aspx's own "view the invoice's original attachment"
    ''' feature (not wired up separately yet) -- both want the same "read back a small text
    ''' attachment by name" behaviour, so it lives here once instead of being duplicated per page.
    ''' </summary>
    Public Class OrderAttachmentService

        ''' <summary>
        ''' Loads the full text of an order attachment by its file name, for inline preview.
        ''' </summary>
        Public Shared Function LoadAttachmentPreview(ByVal attachmentName As String) As String
            Try
                Dim attachmentPath As String = HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/previews/" & attachmentName)
                ' SINK: PLANTED-VB-HR-308
                Using sr As New StreamReader(attachmentPath)
                    Return HttpUtility.HtmlEncode(sr.ReadToEnd())
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return "Attachment not found: " & HttpUtility.HtmlEncode(attachmentName)
            End Try
        End Function

        ''' <summary>
        ''' Same as LoadAttachmentPreview, but the requested name is reduced to its bare file-name
        ''' component first, so the result can never resolve outside the fixed
        ''' "attachments/previews" directory regardless of what the caller passes in.
        ''' </summary>
        Public Shared Function LoadAttachmentPreviewSafe(ByVal attachmentName As String) As String
            Try
                Dim safeFileName As String = Path.GetFileName(attachmentName)
                Dim attachmentPathSafe As String = HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/previews/" & safeFileName)
                ' SAFE_SINK: PLANTED-VB-HR-308-safe
                Using srSafe As New StreamReader(attachmentPathSafe)
                    Return HttpUtility.HtmlEncode(srSafe.ReadToEnd())
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return "Attachment not found: " & HttpUtility.HtmlEncode(attachmentName)
            End Try
        End Function
    End Class
End Namespace
