Imports System
Imports System.Threading

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Type/polymorphism-dependent tier for Content/SleepDoS.aspx's "Preview Throttle Policy"
    ''' section. Decides how a requested processing-delay duration is actually applied; which
    ''' concrete policy runs is chosen per request via a "throttleMode" value (see
    ''' ThrottleDelayPolicyFactory.ResolveFor below), mirroring the resolver-by-mode-string
    ''' dispatch shape this project already uses elsewhere (e.g. AttachmentDisposalPolicy.vb's
    ''' "disposalMode", PaymentHandoffResolver.vb). None of this project's existing
    ''' MustInherit/MustOverride hierarchies (AttachmentPathResolver, FulfillmentWebhookKeySource,
    ''' BackupFormatStrategy, PaymentHandoffResolver, PartnerFeedParser,
    ''' NotificationChannelHeaderFormatter, CookieSecurityPolicy, PreferenceValue,
    ''' LoginCompletionStrategy, DiagnosticToolInvoker, AttachmentDisposalPolicy,
    ''' PasswordRecoveryNotificationChannel -- see AdminQuickActionDispatcher.vb's own class comment
    ''' and AttachmentDisposalPolicy.vb's for the full roster) is domain-relevant to a
    ''' processing-delay decision, and each already carries a different CWE's plants (re-verified by
    ''' grepping for PLANTED markers at each hierarchy's own file and dispatch call site, not just
    ''' the base-class declaration), so this is a dedicated, new hierarchy rather than a substitution.
    ''' </summary>
    Public MustInherit Class ThrottleDelayPolicy
        Public MustOverride Sub ApplyDelay(ByVal requestedDelayMs As Integer)
    End Class

    ''' <summary>
    ''' Selected when "throttleMode" is "raw" -- blocks the calling thread for exactly the
    ''' requested duration, with no upper bound, regardless of how large the request asks for.
    ''' </summary>
    Public Class UnboundedDelayPolicy
        Inherits ThrottleDelayPolicy

        Public Overrides Sub ApplyDelay(ByVal requestedDelayMs As Integer)
            Thread.Sleep(requestedDelayMs) ' SINK: PLANTED-VB-HR-639
        End Sub
    End Class

    ''' <summary>
    ''' Selected for every other "throttleMode" value (including "capped" and anything
    ''' unrecognized -- fail closed) -- clamps the requested duration to a small, fixed ceiling
    ''' before blocking the thread.
    ''' </summary>
    Public Class BoundedDelayPolicy
        Inherits ThrottleDelayPolicy

        Public Overrides Sub ApplyDelay(ByVal requestedDelayMs As Integer)
            Thread.Sleep(Math.Min(Math.Max(requestedDelayMs, 0), 2000)) ' SAFE_SINK: PLANTED-VB-HR-639-safe
        End Sub
    End Class

    ''' <summary>
    ''' Resolves which concrete ThrottleDelayPolicy a request's "throttleMode" value selects. The
    ''' caller (SleepDoS.aspx.vb's btnPreviewPolicy_Click) performs no branching of its own at all
    ''' -- it simply calls ApplyDelay on whatever concrete instance this factory hands back, so
    ''' which of the two overrides above actually runs is determined entirely by the resolved
    ''' runtime type, not by a visible if/else at the call site.
    ''' </summary>
    Public Class ThrottleDelayPolicyFactory
        Public Shared Function ResolveFor(ByVal throttleMode As String) As ThrottleDelayPolicy
            Select Case If(throttleMode, String.Empty).Trim().ToLowerInvariant()
                Case "raw"
                    Return New UnboundedDelayPolicy()
                Case Else
                    Return New BoundedDelayPolicy()
            End Select
        End Function
    End Class
End Namespace
