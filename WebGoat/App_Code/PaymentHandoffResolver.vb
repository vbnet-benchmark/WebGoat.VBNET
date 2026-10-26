Imports System
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Hands a customer off to wherever they should go after settling WebGoatCoins/Invoice.aspx,
    ''' chosen at runtime by Invoice.aspx.vb based on the "PaymentProvider" query parameter. An
    ''' external provider's own confirmation URL is necessarily off-site and cannot be checked
    ''' against this application's own page set, while the in-house "pay on account" flow only
    ''' ever returns to one of a few known internal pages.
    ''' </summary>
    Public MustInherit Class PaymentHandoffResolver
        Public MustOverride Sub PerformHandoff(ByVal response As HttpResponse, ByVal rawDestination As String)
    End Class

    ''' <summary>
    ''' External-provider hand-off: the caller-supplied destination is assumed to be the
    ''' provider's own confirmation URL and is passed through unchanged.
    ''' </summary>
    Public Class ExternalProviderHandoffResolver
        Inherits PaymentHandoffResolver

        Public Overrides Sub PerformHandoff(ByVal response As HttpResponse, ByVal rawDestination As String)
            ' SINK: PLANTED-VB-HR-344
            response.Redirect(rawDestination)
        End Sub
    End Class

    ''' <summary>
    ''' In-house "pay on account" hand-off: only ever returns to one of a small, fixed set of
    ''' known-internal destinations, regardless of what was requested.
    ''' </summary>
    Public Class InHouseAccountHandoffResolver
        Inherits PaymentHandoffResolver

        Private Shared ReadOnly AllowedDestinations As String() = {"/WebGoatCoins/MainPage.aspx", "/WebGoatCoins/Orders.aspx"}

        Public Overrides Sub PerformHandoff(ByVal response As HttpResponse, ByVal rawDestination As String)
            Dim target As String = "/WebGoatCoins/MainPage.aspx"
            For Each allowed As String In AllowedDestinations
                If String.Equals(rawDestination, allowed, StringComparison.OrdinalIgnoreCase) Then
                    target = allowed
                    Exit For
                End If
            Next
            ' SAFE_SINK: PLANTED-VB-HR-344-safe
            response.Redirect(target)
        End Sub
    End Class
End Namespace
