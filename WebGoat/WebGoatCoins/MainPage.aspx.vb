Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Data
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Public Partial Class MainPage
        Inherits System.Web.UI.Page

        'TODO: Add "welcome back " + name;
        'TODO: pending orders?
        'TODO: Add "WebGoat Coins Info Center"
        'TODO: Take out monthly special, add "hear what our customers are saying" - with the latest comments.  Add date field to comments??

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            labelUpload.Visible = False

            ' Promote the per-visit tracking token stashed in Session by Default.aspx.vb into a
            ' cookie, so it survives across the customer's whole visit, not just this one
            ' request.
            Dim visitorTrackingId As Object = Session("visitorTrackingId")
            If visitorTrackingId IsNot Nothing Then
                Dim trackingCookie As New HttpCookie("visitorTrackingId", visitorTrackingId.ToString())
                trackingCookie.Expires = DateTime.Now.AddDays(1)
                ' SINK: PLANTED-VB-HR-83
                Response.Cookies.Add(trackingCookie)
            End If

            Dim visitorTrackingIdSafe As Object = Session("visitorTrackingIdSafe")
            If visitorTrackingIdSafe IsNot Nothing Then
                Dim trackingCookieSafe As New HttpCookie("visitorTrackingIdSafe", visitorTrackingIdSafe.ToString())
                trackingCookieSafe.Expires = DateTime.Now.AddDays(1)
                trackingCookieSafe.HttpOnly = True
                ' SAFE_SINK: PLANTED-VB-HR-83-safe
                Response.Cookies.Add(trackingCookieSafe)
            End If

            ' Recently-viewed-products strip: each product ID the customer browsed this session
            ' gets its own small cookie so the storefront can render a "continue browsing" rail
            ' without a DB round trip.
            Dim recentProductsRaw As String = Request.QueryString("recentProducts")
            If Not String.IsNullOrEmpty(recentProductsRaw) Then
                Dim recentProductIds As New List(Of String)(recentProductsRaw.Split(","c))
                Dim recentCookies As List(Of HttpCookie) = CookieManager.CreateTrackingCookies("recentProduct", recentProductIds)
                For Each recentCookie As HttpCookie In recentCookies
                    ' SINK: PLANTED-VB-HR-85
                    Response.Cookies.Add(recentCookie)
                Next
            End If

            Dim recentProductsSafeRaw As String = Request.QueryString("recentProductsSafe")
            If Not String.IsNullOrEmpty(recentProductsSafeRaw) Then
                Dim recentProductIdsSafe As New List(Of String)(recentProductsSafeRaw.Split(","c))
                Dim recentCookiesSafe As List(Of HttpCookie) = CookieManager.CreateTrackingCookiesSafe("recentProductSafe", recentProductIdsSafe)
                For Each recentCookieSafe As HttpCookie In recentCookiesSafe
                    ' SAFE_SINK: PLANTED-VB-HR-85-safe
                    Response.Cookies.Add(recentCookieSafe)
                Next
            End If

            If Request.Cookies("customerNumber") IsNot Nothing Then
                Dim customerNumber As String = Request.Cookies("customerNumber").Value

                Dim ds As DataSet = du.GetCustomerDetails(customerNumber)
                Dim row As DataRow = ds.Tables(0).Rows(0) 'customer row

                Image1.ImageUrl = "images/logos/" + row("logoFileName").ToString()

                For Each col As DataColumn In ds.Tables(0).Columns
                    Dim tablerow As New TableRow()
                    tablerow.ID = col.ColumnName.ToString()

                    Dim cell1 As New TableCell()
                    Dim cell2 As New TableCell()
                    cell1.Text = col.ColumnName.ToString()
                    cell2.Text = row(col).ToString()
                    
                    tablerow.Cells.Add(cell1)
                    tablerow.Cells.Add(cell2)
                    
                    CustomerTable.Rows.Add(tablerow)
                Next
            End If

            ' Personalized welcome banner (fulfills the "welcome back " + name TODO above) driven
            ' by an optional display-name query parameter.
            Dim displayName As String = Request.QueryString("displayName")
            ' SINK: PLANTED-VB-HR-7
            labelWelcome.Text = BuildWelcomeBanner(displayName)

            Dim nickname As String = Request.QueryString("nickname")
            ' SAFE_SINK: PLANTED-VB-HR-7-safe
            labelMessage.Text = BuildNicknameBanner(nickname)

            ' Greet the customer based on which page referred them here (set by
            ' CustomerLogin.aspx.vb's Page_Load when the customer logs in).
            Dim lastPage As Object = Session("lastVisitedPage")
            If lastPage IsNot Nothing Then
                ' SINK: PLANTED-VB-HR-8
                labelOutput.Text &= "You came from: " & lastPage.ToString() & "<br/>"
            End If

            Dim lastPageSafe As Object = Session("lastVisitedPageSafe")
            If lastPageSafe IsNot Nothing Then
                ' SAFE_SINK: PLANTED-VB-HR-8-safe
                labelOutput.Text &= "You came from (safe): " & Server.HtmlEncode(lastPageSafe.ToString()) & "<br/>"
            End If

            ' Attach the customer's own requested banner text (queued during login by
            ' CustomerLogin.aspx.vb's Page_Load, above) as a custom response header once they land
            ' here, then clear it so it is only ever attached once.
            Dim pendingBannerHeaderText As Object = Session("pendingBannerHeaderText")
            If pendingBannerHeaderText IsNot Nothing Then
                ' SINK: PLANTED-VB-HR-443
                Response.AppendHeader("X-Goat-Banner", pendingBannerHeaderText.ToString())
                Session("pendingBannerHeaderText") = Nothing
            End If

            Dim pendingBannerHeaderTextSafe As Object = Session("pendingBannerHeaderTextSafe")
            If pendingBannerHeaderTextSafe IsNot Nothing Then
                Dim strippedBanner As String = pendingBannerHeaderTextSafe.ToString().Replace(Chr(13), "").Replace(Chr(10), "")
                ' SAFE_SINK: PLANTED-VB-HR-443-safe
                Response.AppendHeader("X-Goat-Banner-Safe", strippedBanner)
                Session("pendingBannerHeaderTextSafe") = Nothing
            End If

            ' Customers managing their uploaded documents (ID scans, proof-of-address) from the
            ' portal home page can remove one they no longer want kept on file. Routed through
            ' the shared CustomerDocumentService, which itself resolves the physical path one
            ' further hop away -- a genuine cross-file, multi-hop call chain.
            Dim removeDocument As String = Request.QueryString("removeDocument")
            If Not String.IsNullOrEmpty(removeDocument) Then
                CustomerDocumentService.RemoveUploadedDocument(removeDocument)
                labelOutput.Text &= "<div class='document-removed'>Removed document: " & HttpUtility.HtmlEncode(removeDocument) & "</div>"
            End If

            Dim removeDocumentSafe As String = Request.QueryString("removeDocumentSafe")
            If Not String.IsNullOrEmpty(removeDocumentSafe) Then
                CustomerDocumentService.RemoveUploadedDocumentSafe(removeDocumentSafe)
                labelOutput.Text &= "<div class='document-removed-safe'>Removed document: " & HttpUtility.HtmlEncode(removeDocumentSafe) & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' Builds the "welcome back" banner for an optional display name. Returns a generic
        ''' greeting when no name was supplied.
        ''' </summary>
        Private Function BuildWelcomeBanner(ByVal displayName As String) As String
            If String.IsNullOrEmpty(displayName) Then
                Return "Welcome back to WebGoat Coins!"
            End If
            Return String.Format("Welcome back, {0}!", displayName)
        End Function

        ''' <summary>
        ''' Builds the "nickname on file" banner for an optional nickname, HTML-encoding it
        ''' first since it's rendered straight into the page.
        ''' </summary>
        Private Function BuildNicknameBanner(ByVal nickname As String) As String
            If String.IsNullOrEmpty(nickname) Then
                Return String.Empty
            End If
            Return String.Format("Nickname on file: {0}", HttpUtility.HtmlEncode(nickname))
        End Function

        Protected Sub btnUpload_Click(sender As Object, e As EventArgs)
            If FileUpload1.HasFile Then
                Try
                    Dim filename As String = Path.GetFileName(FileUpload1.FileName)
                    FileUpload1.SaveAs(Server.MapPath("~/WebGoatCoins/uploads/") + filename)
                    
                Catch ex As Exception
                    labelUpload.Text = "Upload Failed: " + ex.Message
                Finally
                    labelUpload.Visible = True
                End Try
            End If
        End Sub
    End Class
End Namespace