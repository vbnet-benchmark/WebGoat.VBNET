Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.IO
Imports System.Collections.Specialized
Imports System.Security.Cryptography
Imports System.Text
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Orders
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim id As Integer
            Dim ds As DataSet
            If Request.Cookies("customerNumber") Is Nothing OrElse Not Integer.TryParse(Request.Cookies("customerNumber").Value.ToString(), id) Then
                lblOutput.Text = "Sorry, an unspecified problem regarding your Customer ID has occurred.  Are your cookies enabled?"
            Else
                ds = du.GetOrders(id)

                If Not Page.IsPostBack Then 'generate the data grid
                    GridView1.DataSource = ds.Tables(0)

                    GridView1.AutoGenerateColumns = False

                    Dim BoundFieldOrderNumber As New BoundField()
                    Dim BoundFieldStatus As New BoundField()
                    Dim BoundFieldRequiredDate As New BoundField()
                    Dim BoundFieldShippedDate As New BoundField()

                    BoundFieldOrderNumber.DataField = "orderNumber"
                    BoundFieldStatus.DataField = "status"
                    BoundFieldRequiredDate.DataField = "requiredDate"
                    BoundFieldShippedDate.DataField = "shippedDate"

                    BoundFieldOrderNumber.HeaderText = "Order Number"
                    BoundFieldStatus.HeaderText = "Status"
                    BoundFieldRequiredDate.HeaderText = "Required Date"
                    BoundFieldShippedDate.HeaderText = "Shipped Date"

                    BoundFieldRequiredDate.DataFormatString = "{0:MM/dd/yyyy}"
                    BoundFieldShippedDate.DataFormatString = "{0:MM/dd/yyyy}"

                    GridView1.Columns.Add(BoundFieldOrderNumber)
                    GridView1.Columns.Add(BoundFieldStatus)
                    GridView1.Columns.Add(BoundFieldRequiredDate)
                    GridView1.Columns.Add(BoundFieldShippedDate)

                    GridView1.DataBind()
                End If
                'check if orderNumber exists
                Dim orderNumber As String = Request("orderNumber")
                If orderNumber IsNot Nothing Then
                    Try
                        'lblOutput.Text = orderNumber;
                        Dim dsOrderDetails As DataSet = du.GetOrderDetails(Integer.Parse(orderNumber))
                        DetailsView1.DataSource = dsOrderDetails.Tables(0)
                        DetailsView1.DataBind()
                        'litOrderDetails.Visible = true;
                        PanelShowDetailSuccess.Visible = True

                        'allow customer to download image of their product
                        Dim image As String = dsOrderDetails.Tables(0).Rows(0)("productImage").ToString()
                        HyperLink1.Text = "Download Product Image"
                        HyperLink1.NavigateUrl = Request.RawUrl & "&image=images/products/" & image
                    Catch ex As Exception
                        'litOrderDetails.Text = "Error finding order number " + orderNumber + ". Details: " + ex.Message;
                        PanelShowDetailFailure.Visible = True
                        litErrorDetailMessage.Text = "Error finding order number " & orderNumber & ". Details: " & ex.Message
                    End Try
                End If

                'check if they are trying to download the image
                Dim target_image As String = Request("image")
                If target_image IsNot Nothing Then
                    Dim fi As New FileInfo(Server.MapPath(target_image))
                    lblOutput.Text = fi.FullName

                    Dim imageExtensions As New NameValueCollection()
                    imageExtensions.Add(".jpg", "image/jpeg")
                    imageExtensions.Add(".gif", "image/gif")
                    imageExtensions.Add(".png", "image/png")

                    Response.ContentType = imageExtensions.Get(fi.Extension)
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" & fi.Name)
                    Response.TransmitFile(fi.FullName)
                    Response.[End]()
                End If

                ' Let a customer restore draft notes for several orders at once from a single
                ' backup link (one they might have emailed to themselves before clearing their
                ' browser) -- reuses the same generic backup-restore mechanism
                ' PreferenceBackupService already provides for CustomerSettings.aspx's
                ' preference-backup feature.
                Dim orderNotesBackup As String = Request.QueryString("orderNotesBackup")
                If Not String.IsNullOrEmpty(orderNotesBackup) Then
                    Dim restoredNotes As New List(Of String)()

                    For Each chunk As String In orderNotesBackup.Split(","c)
                        Dim note As String = TryCast(PreferenceBackupService.RestoreBackupItem(chunk), String)
                        If note IsNot Nothing Then restoredNotes.Add(note)
                    Next

                    If restoredNotes.Count > 0 Then
                        lblOutput.Text &= "<div class='order-notes'>Restored " & restoredNotes.Count.ToString() & " draft note(s): " & String.Join(" | ", restoredNotes) & "</div>"
                    End If
                End If

                Dim orderNotesBackupSafe As String = Request.QueryString("orderNotesBackupSafe")
                Dim orderNotesBackupMac As String = Request.QueryString("orderNotesBackupMac")
                If Not String.IsNullOrEmpty(orderNotesBackupSafe) AndAlso Not String.IsNullOrEmpty(orderNotesBackupMac) Then
                    Dim restoredNotesSafe As New List(Of String)()
                    Dim chunks As String() = orderNotesBackupSafe.Split(","c)
                    Dim macs As String() = orderNotesBackupMac.Split(","c)

                    If chunks.Length = macs.Length Then

                        For i As Integer = 0 To chunks.Length - 1
                            Dim note As String = TryCast(PreferenceBackupService.RestoreBackupItemSafe(chunks(i), macs(i)), String)
                            If note IsNot Nothing Then restoredNotesSafe.Add(note)
                        Next

                    End If

                    If restoredNotesSafe.Count > 0 Then
                        lblOutput.Text &= "<div class='order-notes-safe'>Restored " & restoredNotesSafe.Count.ToString() & " draft note(s): " & String.Join(" | ", restoredNotesSafe) & "</div>"
                    End If
                End If

                ' "Export my order history" -- lets a customer download a plain-text summary of
                ' every order row currently shown in the grid above, stamped with a single
                ' integrity fingerprint accumulated across every row, so a re-imported copy (e.g.
                ' from a saved export file) can be checked for tampering before any of its rows
                ' are trusted again.
                If Request.QueryString("exportHistory") = "1" Then
                    Dim sb As New StringBuilder()
                    For Each row As DataRow In ds.Tables(0).Rows
                        sb.Append(row("orderNumber").ToString())
                        sb.Append("|")
                        sb.Append(row("status").ToString())
                        sb.Append("|")
                        sb.Append(row("requiredDate").ToString())
                        sb.Append(";")
                    Next

                    Using md5 As MD5 = MD5.Create()
                        Dim fingerprintBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())) ' SINK: PLANTED-VB-HR-415
                        Dim fingerprint As String = BitConverter.ToString(fingerprintBytes).Replace("-", "").ToLowerInvariant()
                        lblOutput.Text &= "<div class='export-fingerprint'>Order history export fingerprint: " & fingerprint & "</div>"
                    End Using
                End If

                ' Hardened twin: the same per-row accumulation, but the final fingerprint is
                ' salted with a fresh random value per export and hashed with SHA-256.
                If Request.QueryString("exportHistorySafe") = "1" Then
                    Dim sbSafe As New StringBuilder()
                    For Each row As DataRow In ds.Tables(0).Rows
                        sbSafe.Append(row("orderNumber").ToString())
                        sbSafe.Append("|")
                        sbSafe.Append(row("status").ToString())
                        sbSafe.Append("|")
                        sbSafe.Append(row("requiredDate").ToString())
                        sbSafe.Append(";")
                    Next

                    Dim saltBytes(15) As Byte
                    Using rng As New RNGCryptoServiceProvider()
                        rng.GetBytes(saltBytes)
                    End Using
                    Dim saltHex As String = BitConverter.ToString(saltBytes).Replace("-", "").ToLowerInvariant()

                    Using sha256 As SHA256 = SHA256.Create()
                        Dim fingerprintBytesSafe = sha256.ComputeHash(Encoding.UTF8.GetBytes(sbSafe.ToString() & "|" & saltHex)) ' SAFE_SINK: PLANTED-VB-HR-415-safe
                        Dim fingerprintSafe As String = saltHex & ":" & BitConverter.ToString(fingerprintBytesSafe).Replace("-", "").ToLowerInvariant()
                        lblOutput.Text &= "<div class='export-fingerprint-safe'>Order history export fingerprint: " & fingerprintSafe & "</div>"
                    End Using
                End If

                ' A customer tidying up their own order history can remove an attachment (a
                ' scanned proof-of-payment note, a canceled shipping label) from the order
                ' folder directly from this page -- a single straight-line action, no helper
                ' function involved.
                Dim removeAttachment As String = Request.QueryString("removeAttachment")
                If Not String.IsNullOrEmpty(removeAttachment) Then
                    Try
                        Dim attachmentPath As String = Server.MapPath("~/WebGoatCoins/attachments/" & removeAttachment)
                        ' SINK: PLANTED-VB-HR-606
                        File.Delete(attachmentPath)
                        lblOutput.Text &= "<div class='attachment-removed'>Removed attachment: " & HttpUtility.HtmlEncode(removeAttachment) & "</div>"
                    Catch ex As Exception
                        Console.WriteLine(ex.Message)
                        lblOutput.Text &= "<div class='attachment-remove-error'>Could not remove attachment: " & HttpUtility.HtmlEncode(removeAttachment) & "</div>"
                    End Try
                End If

                ' Hardened twin: the requested name is reduced to its bare file-name component
                ' and then checked against the fixed, server-controlled list of attachments this
                ' customer's own orders are actually known to have -- a syntactically clean name
                ' that simply isn't one of this customer's own attachments is still rejected.
                Dim removeAttachmentSafe As String = Request.QueryString("removeAttachmentSafe")
                If Not String.IsNullOrEmpty(removeAttachmentSafe) Then
                    Dim safeAttachmentName As String = Path.GetFileName(removeAttachmentSafe)
                    If safeAttachmentName.Length = 0 OrElse safeAttachmentName = "." OrElse safeAttachmentName = ".." OrElse
                        Not AttachmentRegistry.IsKnownOrderAttachment(safeAttachmentName) Then
                        lblOutput.Text &= "<div class='attachment-remove-error-safe'>Attachment not recognized for this order.</div>"
                    Else
                        Try
                            Dim attachmentPathSafe As String = Server.MapPath("~/WebGoatCoins/attachments/" & safeAttachmentName)
                            ' SAFE_SINK: PLANTED-VB-HR-606-safe
                            File.Delete(attachmentPathSafe)
                            lblOutput.Text &= "<div class='attachment-removed-safe'>Removed attachment: " & HttpUtility.HtmlEncode(safeAttachmentName) & "</div>"
                        Catch ex As Exception
                            Console.WriteLine(ex.Message)
                            lblOutput.Text &= "<div class='attachment-remove-error-safe'>Could not remove attachment: " & HttpUtility.HtmlEncode(safeAttachmentName) & "</div>"
                        End Try
                    End If
                End If

            End If
        End Sub

        Protected Sub GridView1_RowDataBound(ByVal sender As Object, ByVal e As GridViewRowEventArgs)
            'make the first column a hyperlink
            If e.Row.RowType = DataControlRowType.DataRow Then
                Dim link As New HyperLink()
                link.Text = e.Row.Cells(0).Text
                link.NavigateUrl = "Orders.aspx?orderNumber=" & link.Text
                e.Row.Cells(0).Controls.Add(link)
            End If

        End Sub
    End Class
End Namespace