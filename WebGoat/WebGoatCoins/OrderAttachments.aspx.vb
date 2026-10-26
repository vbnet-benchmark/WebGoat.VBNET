Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class OrderAttachments
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            ' Lets a customer (or the support agent handling their order) preview a small text
            ' attachment saved alongside an order -- a scanned proof-of-payment note, a shipping
            ' carrier's delivery confirmation, a support agent's follow-up memo -- by its file
            ' name, without first having to look the order up in the database.
            Dim attachmentName As String = Request.QueryString("attachment")
            If Not String.IsNullOrEmpty(attachmentName) Then
                Try
                    Dim attachmentPath As String = Server.MapPath("~/WebGoatCoins/attachments/" & attachmentName)
                    ' SINK: PLANTED-VB-HR-306
                    Using sr As New StreamReader(attachmentPath)
                        Response.Write("<div class='attachment-preview'>" & HttpUtility.HtmlEncode(sr.ReadToEnd()) & "</div>")
                    End Using
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Response.Write("<div class='attachment-error'>Attachment not found: " & HttpUtility.HtmlEncode(attachmentName) & "</div>")
                End Try
            End If

            Dim attachmentNameSafe As String = Request.QueryString("attachmentSafe")
            If Not String.IsNullOrEmpty(attachmentNameSafe) Then
                Try
                    Dim safeFileName As String = Path.GetFileName(attachmentNameSafe)
                    Dim attachmentPathSafe As String = Server.MapPath("~/WebGoatCoins/attachments/" & safeFileName)
                    ' SAFE_SINK: PLANTED-VB-HR-306-safe
                    Using srSafe As New StreamReader(attachmentPathSafe)
                        Response.Write("<div class='attachment-preview-safe'>" & HttpUtility.HtmlEncode(srSafe.ReadToEnd()) & "</div>")
                    End Using
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Response.Write("<div class='attachment-error-safe'>Attachment not found: " & HttpUtility.HtmlEncode(attachmentNameSafe) & "</div>")
                End Try
            End If

            ' A support agent reviewing an order's full history can also pull up that order's
            ' "receipt log" -- a running text log of every receipt-related event recorded for the
            ' order (resent confirmations, refund notes, etc.) -- by name. Routed through a private
            ' helper so Page_Load itself stays focused on which query parameters are in play.
            Dim receiptLogName As String = Request.QueryString("receiptLog")
            If Not String.IsNullOrEmpty(receiptLogName) Then
                Response.Write("<div class='receipt-log'>" & ReadReceiptLog(receiptLogName) & "</div>")
            End If

            Dim receiptLogNameSafe As String = Request.QueryString("receiptLogSafe")
            If Not String.IsNullOrEmpty(receiptLogNameSafe) Then
                Response.Write("<div class='receipt-log-safe'>" & ReadReceiptLogSafe(receiptLogNameSafe) & "</div>")
            End If

            ' Routed through the shared OrderAttachmentService so the same "load an attachment's
            ' text preview" logic used here is also available to WebGoatCoins/Invoice.aspx's own
            ' "preview the invoice's original attachment" link (not modeled separately here).
            Dim previewName As String = Request.QueryString("preview")
            If Not String.IsNullOrEmpty(previewName) Then
                Response.Write("<div class='service-preview'>" & OrderAttachmentService.LoadAttachmentPreview(previewName) & "</div>")
            End If

            Dim previewNameSafe As String = Request.QueryString("previewSafe")
            If Not String.IsNullOrEmpty(previewNameSafe) Then
                Response.Write("<div class='service-preview-safe'>" & OrderAttachmentService.LoadAttachmentPreviewSafe(previewNameSafe) & "</div>")
            End If

            ' Some attachments are only ever meant to be reachable by an authenticated support
            ' agent working the "public intake" queue (anonymous customer-submitted documents
            ' pending triage), while others come from the ordinary, already-validated customer
            ' attachment area. Which trust tier applies is chosen per request via "resolverMode",
            ' and the concrete AttachmentPathResolver this picks changes how the raw name is turned
            ' into a physical path.
            Dim resolverMode As String = Request.QueryString("resolverMode")
            Dim polyAttachmentName As String = Request.QueryString("polyAttachment")
            If Not String.IsNullOrEmpty(polyAttachmentName) Then
                Try
                    If String.Equals(resolverMode, "trusted", StringComparison.OrdinalIgnoreCase) Then
                        Dim resolver As AttachmentPathResolver = New TrustedAttachmentPathResolver()
                        Dim resolvedPathSafe As String = resolver.Resolve(polyAttachmentName)
                        ' SAFE_SINK: PLANTED-VB-HR-309-safe
                        Using srPolySafe As New StreamReader(resolvedPathSafe)
                            Response.Write("<div class='poly-preview-safe'>" & HttpUtility.HtmlEncode(srPolySafe.ReadToEnd()) & "</div>")
                        End Using
                    Else
                        Dim resolver As AttachmentPathResolver = New PublicAttachmentPathResolver()
                        Dim resolvedPath As String = resolver.Resolve(polyAttachmentName)
                        ' SINK: PLANTED-VB-HR-309
                        Using srPoly As New StreamReader(resolvedPath)
                            Response.Write("<div class='poly-preview'>" & HttpUtility.HtmlEncode(srPoly.ReadToEnd()) & "</div>")
                        End Using
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Response.Write("<div class='poly-error'>Attachment not found: " & HttpUtility.HtmlEncode(polyAttachmentName) & "</div>")
                End Try
            End If

            ' Attachments can also be organized into nested categories (year / document-type /
            ' file), submitted as a single comma-separated list of path segments rather than one
            ' flat name -- e.g. a receipt filed under "2024,receipts,order482.txt". Each segment is
            ' joined onto the running path in turn.
            Dim rawSegments As String = Request.QueryString("segments")
            If Not String.IsNullOrEmpty(rawSegments) Then
                Try
                    Dim basePath As String = Server.MapPath("~/WebGoatCoins/attachments/categorized")
                    Dim assembledPath As String = basePath
                    For Each seg As String In rawSegments.Split(","c)
                        assembledPath = Path.Combine(assembledPath, seg)
                    Next
                    ' SINK: PLANTED-VB-HR-310
                    Using srCat As New StreamReader(assembledPath)
                        Response.Write("<div class='categorized-preview'>" & HttpUtility.HtmlEncode(srCat.ReadToEnd()) & "</div>")
                    End Using
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Response.Write("<div class='categorized-error'>Attachment not found</div>")
                End Try
            End If

            Dim rawSegmentsSafe As String = Request.QueryString("segmentsSafe")
            If Not String.IsNullOrEmpty(rawSegmentsSafe) Then
                Try
                    Dim basePathSafe As String = Server.MapPath("~/WebGoatCoins/attachments/categorized")
                    Dim assembledPathSafe As String = basePathSafe
                    For Each seg As String In rawSegmentsSafe.Split(","c)
                        assembledPathSafe = Path.Combine(assembledPathSafe, seg)
                    Next
                    Dim canonicalPath As String = Path.GetFullPath(assembledPathSafe)
                    Dim canonicalBase As String = Path.GetFullPath(basePathSafe)
                    If canonicalPath.StartsWith(canonicalBase & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) Then
                        ' SAFE_SINK: PLANTED-VB-HR-310-safe
                        Using srCatSafe As New StreamReader(canonicalPath)
                            Response.Write("<div class='categorized-preview-safe'>" & HttpUtility.HtmlEncode(srCatSafe.ReadToEnd()) & "</div>")
                        End Using
                    Else
                        Response.Write("<div class='categorized-error-safe'>Invalid attachment path</div>")
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Response.Write("<div class='categorized-error-safe'>Attachment not found</div>")
                End Try
            End If
        End Sub

        ''' <summary>
        ''' Reads back an order's receipt-event log by name. Kept as a private helper (rather than
        ''' inlined in Page_Load) since Orders.aspx.vb's own "view receipt history" postback
        ''' handler will eventually call the same logic once that integration lands.
        ''' </summary>
        Private Function ReadReceiptLog(ByVal logName As String) As String
            Try
                Dim logPath As String = Server.MapPath("~/WebGoatCoins/attachments/logs/" & logName)
                ' SINK: PLANTED-VB-HR-307
                Using sr As New StreamReader(logPath)
                    Return HttpUtility.HtmlEncode(sr.ReadToEnd())
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return "Receipt log not found: " & HttpUtility.HtmlEncode(logName)
            End Try
        End Function

        ''' <summary>
        ''' Same as ReadReceiptLog, but the requested name is reduced to its bare file-name
        ''' component before it ever becomes part of the physical path, so it can never resolve
        ''' outside the fixed "attachments/logs" directory.
        ''' </summary>
        Private Function ReadReceiptLogSafe(ByVal logName As String) As String
            Try
                Dim safeLogName As String = Path.GetFileName(logName)
                Dim logPathSafe As String = Server.MapPath("~/WebGoatCoins/attachments/logs/" & safeLogName)
                ' SAFE_SINK: PLANTED-VB-HR-307-safe
                Using srSafe As New StreamReader(logPathSafe)
                    Return HttpUtility.HtmlEncode(srSafe.ReadToEnd())
                End Using
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return "Receipt log not found: " & HttpUtility.HtmlEncode(logName)
            End Try
        End Function
    End Class
End Namespace
