Imports System
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Invoice
        Inherits Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ' Some customers choose to settle this invoice through an external payment provider
            ' (e.g. a partner checkout button) instead of the site's own "pay on account" flow --
            ' PaymentProvider names which one, PaymentReturnUrl where its confirmation page should
            ' send the customer back to once they're done there.
            Dim paymentReturnUrl As String = Request.QueryString("PaymentReturnUrl")
            If Not String.IsNullOrEmpty(paymentReturnUrl) Then
                Dim providerKind As String = Request.QueryString("PaymentProvider")
                Dim resolver As PaymentHandoffResolver

                If String.Equals(providerKind, "inhouse", StringComparison.OrdinalIgnoreCase) Then
                    resolver = New InHouseAccountHandoffResolver()
                Else
                    resolver = New ExternalProviderHandoffResolver()
                End If

                resolver.PerformHandoff(Response, paymentReturnUrl)
                Return
            End If

            lblFullName.Text = Session("buy_full_name").ToString()
            lblCountry.Text = Session("buy_country").ToString()
            lblAddress.Text = Session("buy_address").ToString()
            lblPartNo.Text = Session("buy_product_id").ToString()
            lblDescription.Text = Session("buy_product_description").ToString()
            lblPrice.Text = Session("buy_product_price").ToString()
            lblSubTotal.Text = Session("buy_product_price").ToString()
            lblShippingRate.Text = Session("buy_shipping_price").ToString()
            lblTotal.Text = Session("buy_total_price").ToString()

            ' Hand the warehouse a signed summary of this order (simulated as response headers,
            ' since this goat app has no real warehouse to call). Orders shipping abroad go to the
            ' overseas warehouse, everything else to the domestic one.
            Dim orderSummary As String = lblPartNo.Text & "|" & lblCountry.Text & "|" & lblTotal.Text
            Dim summaryBytes As Byte() = Encoding.UTF8.GetBytes(orderSummary)
            Dim signatureHex As String
            Dim warehouse As String

            If String.Equals(lblCountry.Text, "Australia", StringComparison.OrdinalIgnoreCase) Then
                Dim overseasKeys As FulfillmentWebhookKeySource = New OverseasWarehouseKeySource()
                warehouse = overseasKeys.WarehouseName
                Using mac As New HMACSHA256(overseasKeys.GetSigningKey()) ' SAFE_SINK: PLANTED-VB-HR-484-safe
                    signatureHex = BitConverter.ToString(mac.ComputeHash(summaryBytes)).Replace("-", "").ToLowerInvariant()
                End Using
            Else
                Dim domesticKeys As FulfillmentWebhookKeySource = New DomesticWarehouseKeySource()
                warehouse = domesticKeys.WarehouseName
                Using mac As New HMACSHA256(domesticKeys.GetSigningKey()) ' SINK: PLANTED-VB-HR-484
                    signatureHex = BitConverter.ToString(mac.ComputeHash(summaryBytes)).Replace("-", "").ToLowerInvariant()
                End Using
            End If

            Response.AppendHeader("X-Fulfillment-Warehouse", warehouse)
            Response.AppendHeader("X-Fulfillment-Signature", signatureHex)
        End Sub
    End Class
End Namespace