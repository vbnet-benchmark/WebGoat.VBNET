Imports System
Imports System.Configuration

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Central place for the API keys the checkout uses to talk to outside shipping services,
    ''' so the pages that call those services never have to know where a key comes from.
    ''' </summary>
    Public NotInheritable Class ShippingServiceCredentials

        Private Sub New()
        End Sub

        ''' <summary>
        ''' API key for the parcel carrier's shipment-booking endpoint. The carrier issued one key
        ''' for the store when the integration was set up, and it has been bundled with the code
        ''' ever since.
        ''' </summary>
        Public Shared Function GetCarrierBookingApiKey() As String
            Return "cbk_live_7Hq2Lw9Zr4Tx8Vn3Pe6Ys1Jd"
        End Function

        ''' <summary>
        ''' API key for the address-verification service, supplied per deployment.
        ''' </summary>
        Public Shared Function GetAddressVerificationApiKey() As String
            Return ConfigurationManager.AppSettings("AddressVerificationApiKey")
        End Function

    End Class
End Namespace
