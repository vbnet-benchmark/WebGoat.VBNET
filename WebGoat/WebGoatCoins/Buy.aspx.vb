Imports System
Imports System.Data
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class Buy
        Inherits Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

            ' A shopper who abandons checkout partway through, or who later asks to "forget" a
            ' completed purchase's receipt, can have the staged receipt file this wizard wrote
            ' cleaned up. Which concrete policy actually disposes of it is picked per request via
            ' "disposalMode".
            Dim disposeReceipt As String = Request.QueryString("disposeReceipt")
            If Not String.IsNullOrEmpty(disposeReceipt) Then
                Dim disposalMode As String = Request.QueryString("disposalMode")
                Dim policy As AttachmentDisposalPolicy

                If String.Equals(disposalMode, "verified", StringComparison.OrdinalIgnoreCase) Then
                    policy = New VerifiedDisposalPolicy()
                Else
                    policy = New UnverifiedDisposalPolicy()
                End If

                policy.Dispose(disposeReceipt)
            End If

            If Not Page.IsPostBack Then

                If Request("id") Is Nothing Then Throw New Exception("Not valid request")

                Dim id = Request("id")
                Dim price As Double
                Dim description As String
                Dim html = GetProductContent(id, description, price)
                Session("buy_product_id") = id
                Session("buy_product_description") = description
                Session("buy_product_price") = price
                Session("buy_product_html") = html
                lblOutputStep0.Text = html

            Else

                Select Case BuyWizard.ActiveStepIndex
                    Case 0
                        Exit Select
                    Case 1
                        Session("buy_full_name") = tbFullName.Text
                        Session("buy_address") = tbAddress.Text
                        UpdatePriceIncludeShipping()
                    Case 2
                        Exit Select
                    Case Else
                        Throw New NotImplementedException()
                End Select

            End If

        End Sub

        Protected Sub OnActiveStepChanged(ByVal sender As Object, ByVal e As EventArgs)
            
            Select Case BuyWizard.ActiveStepIndex
                Case 0
                    lblOutputStep0.Text = CType(Session("buy_product_html"), String)
                Case 1
                    tbFullName.Text = CType(Session("buy_full_name"), String)
                    tbAddress.Text = CType(Session("buy_address"), String)
                    ddlCountry.SelectedValue = CType(Session("buy_country"), String)
                    UpdatePriceIncludeShipping()
                Case 2
                    Exit Select
                Case Else
                    Throw New NotImplementedException()
            End Select

        End Sub

        Protected Sub OnFinishButtonClick(ByVal sender As Object, ByVal e As WizardNavigationEventArgs)
            ' Attach the signed order-confirmation link that would be emailed to the customer --
            ' simulated here as response headers, since this goat app has no real mail server.
            Dim customerIdentity As String = TryCast(Session("buy_full_name"), String)
            If Not String.IsNullOrEmpty(customerIdentity) Then
                Dim issuedAt As DateTime = DateTime.UtcNow
                Dim confirmSig As String = AccountNotificationSignatureUtil.ComputeNotificationSignature(customerIdentity, "OrderPlaced", issuedAt)
                Response.AppendHeader("X-Order-Confirmation-Link", "/WebGoatCoins/Invoice.aspx?confirmOrder=1&cust=" & Server.UrlEncode(customerIdentity) & "&ts=" & issuedAt.ToString("O") & "&sig=" & confirmSig)

                Dim confirmSigSafe As String = AccountNotificationSignatureUtil.ComputeNotificationSignatureSafe(customerIdentity, "OrderPlaced", issuedAt)
                Response.AppendHeader("X-Order-Confirmation-Link-Safe", "/WebGoatCoins/Invoice.aspx?confirmOrderSafe=1&cust=" & Server.UrlEncode(customerIdentity) & "&ts=" & issuedAt.ToString("O") & "&sig=" & confirmSigSafe)
            End If

            ' Check the delivery address with the address-verification service, then book the
            ' parcel with the carrier so the label is ready by the time the invoice is printed.
            Dim deliveryAddress As String = TryCast(Session("buy_address"), String)
            If Not String.IsNullOrEmpty(deliveryAddress) Then
                Try
                    VerifyDeliveryAddress(deliveryAddress)
                    BookCarrierShipment(deliveryAddress)
                Catch ex As WebException
                    ' Shipping can still be arranged by hand from the order list; the purchase
                    ' itself must not fail because an outside service is unavailable.
                End Try
            End If

            Response.Redirect("~/WebGoatCoins/Invoice.aspx")
        End Sub

        Private Const AddressVerificationEndpoint As String = "https://api.addresscheck.example/v2/verify"
        Private Const CarrierBookingEndpoint As String = "https://api.parcelcarrier.example/v1/shipments"

        Private Sub VerifyDeliveryAddress(ByVal deliveryAddress As String)
            Dim verifyRequest As HttpWebRequest = CType(WebRequest.Create(AddressVerificationEndpoint), HttpWebRequest)
            verifyRequest.Method = "POST"
            verifyRequest.ContentType = "application/x-www-form-urlencoded"
            verifyRequest.Headers.Add("X-Api-Key", ShippingServiceCredentials.GetAddressVerificationApiKey()) ' SAFE_SINK: PLANTED-VB-HR-483-safe
            PostForm(verifyRequest, "address=" & HttpUtility.UrlEncode(deliveryAddress) & "&country=" & HttpUtility.UrlEncode(TryCast(Session("buy_country"), String)))
        End Sub

        Private Sub BookCarrierShipment(ByVal deliveryAddress As String)
            Dim bookingRequest As HttpWebRequest = CType(WebRequest.Create(CarrierBookingEndpoint), HttpWebRequest)
            bookingRequest.Method = "POST"
            bookingRequest.ContentType = "application/x-www-form-urlencoded"
            bookingRequest.Headers.Add("X-Api-Key", ShippingServiceCredentials.GetCarrierBookingApiKey()) ' SINK: PLANTED-VB-HR-483
            PostForm(bookingRequest, "recipient=" & HttpUtility.UrlEncode(TryCast(Session("buy_full_name"), String)) & "&address=" & HttpUtility.UrlEncode(deliveryAddress) & "&reference=" & HttpUtility.UrlEncode(TryCast(Session("buy_product_id"), String)))
        End Sub

        Private Shared Sub PostForm(ByVal outbound As HttpWebRequest, ByVal body As String)
            Dim payload As Byte() = Encoding.UTF8.GetBytes(body)
            outbound.ContentLength = payload.Length
            Using requestStream As Stream = outbound.GetRequestStream()
                requestStream.Write(payload, 0, payload.Length)
            End Using
            Using outboundResponse As WebResponse = outbound.GetResponse()
            End Using
        End Sub

        Protected Sub OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
            UpdatePriceIncludeShipping()
        End Sub

        Private Sub UpdatePriceIncludeShipping()
            
            Session("buy_country") = ddlCountry.SelectedValue

            If ddlCountry.SelectedValue = "Australia" Then
                Session("buy_shipping_price") = 14.0
            Else
                Session("buy_shipping_price") = 10.0
            End If

            Session("buy_total_price") = (CType(Session("buy_product_price"), Double) + CType(Session("buy_shipping_price"), Double))
            lblShippingPrice.Text = " $" & Session("buy_shipping_price")
            lblTotalPrice.Text = " $" & Session("buy_total_price")

        End Sub

        Private Function GetProductContent(ByVal id As String, ByRef description As String, ByRef price As Double) As String

            price = 0.0
            description = String.Empty
            Dim output = String.Empty
            Dim ds = du.GetProductDetails(id)

            For Each prodRow As DataRow In ds.Tables("products").Rows
                output &= "<div class='product2' align='center'>"
                output &= "<img src='./images/products/" & prodRow("productImage") & "'/><br/>"
                output &= "<strong>" & prodRow("productName") & "</strong><br/>"
                output &= "<hr/>" & prodRow("productDescription") & "<br/>"
                output &= "<hr/><strong>Product price:</strong> $" & prodRow("buyPrice") & "<br/>"
                output &= "</div>"

                price = CDbl(prodRow("buyPrice"))
                description = CStr(prodRow("productName"))
            Next

            Return output

        End Function

    End Class

End Namespace