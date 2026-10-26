Imports System
Imports System.Configuration
Imports System.Text

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Supplies the shared secret used to sign the order notifications sent to a fulfillment
    ''' warehouse, so the warehouse can tell a genuine notification from a forged one. Each
    ''' warehouse integration has its own secret and its own way of obtaining it.
    ''' </summary>
    Public MustInherit Class FulfillmentWebhookKeySource

        ''' <summary>Name of the warehouse this key source signs for, sent alongside the signature.</summary>
        Public MustOverride ReadOnly Property WarehouseName As String

        ''' <summary>Returns the HMAC key bytes for this warehouse's webhook.</summary>
        Public MustOverride Function GetSigningKey() As Byte()

    End Class

    ''' <summary>
    ''' The original domestic warehouse integration. Its secret was agreed with the warehouse
    ''' once and compiled into the integration.
    ''' </summary>
    Public Class DomesticWarehouseKeySource
        Inherits FulfillmentWebhookKeySource

        Public Overrides ReadOnly Property WarehouseName As String
            Get
                Return "domestic"
            End Get
        End Property

        Public Overrides Function GetSigningKey() As Byte()
            Return Encoding.UTF8.GetBytes("whsec_dom_4f9a1c7e2b8d6035aa71")
        End Function
    End Class

    ''' <summary>
    ''' The overseas warehouse integration, whose secret is supplied per deployment.
    ''' </summary>
    Public Class OverseasWarehouseKeySource
        Inherits FulfillmentWebhookKeySource

        Public Overrides ReadOnly Property WarehouseName As String
            Get
                Return "overseas"
            End Get
        End Property

        Public Overrides Function GetSigningKey() As Byte()
            Dim configured As String = ConfigurationManager.AppSettings("OverseasWarehouseWebhookSecret")
            If String.IsNullOrEmpty(configured) Then
                Throw New ConfigurationErrorsException("OverseasWarehouseWebhookSecret is not configured.")
            End If
            Return Encoding.UTF8.GetBytes(configured)
        End Function
    End Class
End Namespace
