Imports System
Imports System.IO
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Decides how a staged checkout receipt file is disposed of for WebGoatCoins/Buy.aspx's
    ''' checkout wizard -- a shopper who abandons checkout partway through, or who later asks to
    ''' "forget" a receipt, can have the staged file the wizard wrote removed. Which concrete
    ''' policy runs is chosen per request via "disposalMode", mirroring
    ''' AttachmentPathResolver.vb's own resolverMode-driven dispatch shape elsewhere in this
    ''' project. None of this project's existing MustInherit/MustOverride hierarchies
    ''' (AttachmentPathResolver, FulfillmentWebhookKeySource, BackupFormatStrategy,
    ''' PaymentHandoffResolver, PartnerFeedParser, NotificationChannelHeaderFormatter,
    ''' CookieSecurityPolicy, PreferenceValue, LoginCompletionStrategy, DiagnosticToolInvoker --
    ''' see AdminQuickActionDispatcher.vb's own class comment for the full roster) is
    ''' domain-relevant to a file-disposal decision, and each already carries a different CWE's
    ''' plants, so this is a dedicated, new hierarchy rather than a substitution.
    ''' </summary>
    Public MustInherit Class AttachmentDisposalPolicy
        Public MustOverride Sub Dispose(ByVal rawReceiptName As String)
    End Class

    ''' <summary>
    ''' Selected whenever "disposalMode" is anything other than "verified" -- disposes of the
    ''' staged receipt purely by the name the checkout wizard was handed, with no check that the
    ''' name is actually one of this checkout session's own staged receipts.
    ''' </summary>
    Public Class UnverifiedDisposalPolicy
        Inherits AttachmentDisposalPolicy

        Public Overrides Sub Dispose(ByVal rawReceiptName As String)
            Dim receiptPath As String = HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/staging/" & rawReceiptName)
            ' SINK: PLANTED-VB-HR-609
            File.Delete(receiptPath)
        End Sub
    End Class

    ''' <summary>
    ''' Selected when "disposalMode" is "verified" -- the requested name is reduced to its bare
    ''' file name and checked against the fixed, server-controlled list of staged receipts this
    ''' checkout area is actually known to hold before anything is deleted.
    ''' </summary>
    Public Class VerifiedDisposalPolicy
        Inherits AttachmentDisposalPolicy

        Public Overrides Sub Dispose(ByVal rawReceiptName As String)
            Dim safeReceiptName As String = Path.GetFileName(rawReceiptName)
            If safeReceiptName.Length = 0 OrElse safeReceiptName = "." OrElse safeReceiptName = ".." OrElse
                Not AttachmentRegistry.IsKnownStagedReceipt(safeReceiptName) Then
                Return
            End If

            Dim receiptPathSafe As String = HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/staging/" & safeReceiptName)
            ' SAFE_SINK: PLANTED-VB-HR-609-safe
            File.Delete(receiptPathSafe)
        End Sub
    End Class
End Namespace
