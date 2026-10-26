Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class ResponseSplitting
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ' Let the page that redirected here hand the next page a short status banner to
            ' display, carried as a custom response header -- this goat app has no shared
            ' server-side flash-message store, so the redirecting page's own query string is the
            ' only place for it to come from.
            Dim notice As String = Request.QueryString("notice")
            If Not String.IsNullOrEmpty(notice) Then
                ' SINK: PLANTED-VB-HR-441
                Response.AddHeader("X-Goat-Notice", notice)
            End If

            Dim noticeSafe As String = Request.QueryString("noticeSafe")
            If Not String.IsNullOrEmpty(noticeSafe) Then
                Dim strippedNotice As String = noticeSafe.Replace(Chr(13), "").Replace(Chr(10), "")
                ' SAFE_SINK: PLANTED-VB-HR-441-safe
                Response.AddHeader("X-Goat-Notice-Safe", strippedNotice)
            End If

            ' The visitor's preferred locale, echoed back as its own custom header so a CDN edge
            ' rule can route follow-up requests to the right regional cache without re-parsing the
            ' query string itself. Routed through a private helper so Page_Load itself stays
            ' focused on which query parameters are in play.
            Dim locale As String = Request.QueryString("locale")
            If Not String.IsNullOrEmpty(locale) Then
                ApplyLocaleHeader(locale)
            End If

            Dim localeSafe As String = Request.QueryString("localeSafe")
            If Not String.IsNullOrEmpty(localeSafe) Then
                ApplyLocaleHeaderSafe(localeSafe)
            End If

            ' Some redirects also want to name which notification channel (the original channel
            ' this feature shipped with, or the current one) the customer should expect their
            ' confirmation on, attached as its own custom header. No genuine reusable polymorphic
            ' hierarchy in this project plausibly gates a response-header-value-formatting
            ' decision (the existing MustInherit/MustOverride hierarchies -- CookieSecurityPolicy,
            ' BackupFormatStrategy, PartnerFeedParser, PaymentHandoffResolver, PreferenceValue,
            ' AttachmentPathResolver -- are all domain-mismatched to header-value formatting and
            ' are already consumed by other CWEs' plants), so this tier is substituted with a
            ' small, purpose-built NotificationChannelHeaderFormatter hierarchy instead.
            Dim channelDetail As String = Request.QueryString("channelDetail")
            If Not String.IsNullOrEmpty(channelDetail) Then
                Dim channelMode As String = Request.QueryString("channelMode")

                If String.Equals(channelMode, "modern", StringComparison.OrdinalIgnoreCase) Then
                    Dim modernFormatter As NotificationChannelHeaderFormatter = New ModernChannelHeaderFormatter()
                    ' SAFE_SINK: PLANTED-VB-HR-444-safe
                    Response.AppendHeader("X-Goat-Channel-Notice", modernFormatter.FormatHeaderValue(channelDetail))
                Else
                    Dim legacyFormatter As NotificationChannelHeaderFormatter = New LegacyChannelHeaderFormatter()
                    ' SINK: PLANTED-VB-HR-444
                    Response.AppendHeader("X-Goat-Channel-Notice", legacyFormatter.FormatHeaderValue(channelDetail))
                End If
            End If
        End Sub

        ''' <summary>
        ''' Applies the visitor's preferred locale as a custom response header, unmodified.
        ''' </summary>
        Private Sub ApplyLocaleHeader(ByVal rawLocale As String)
            ' SINK: PLANTED-VB-HR-442
            Response.AppendHeader("X-Goat-Locale", rawLocale)
        End Sub

        ''' <summary>
        ''' Safe twin of ApplyLocaleHeader -- every CR and LF character is stripped from the
        ''' locale before it becomes a header value, so no obs-fold continuation (a lone CR or
        ''' LF, not just the two-character CRLF sequence) can survive into the response either.
        ''' </summary>
        Private Sub ApplyLocaleHeaderSafe(ByVal rawLocale As String)
            Dim strippedLocale As String = rawLocale.Replace(Chr(13), "").Replace(Chr(10), "")
            ' SAFE_SINK: PLANTED-VB-HR-442-safe
            Response.AppendHeader("X-Goat-Locale-Safe", strippedLocale)
        End Sub
    End Class
End Namespace
