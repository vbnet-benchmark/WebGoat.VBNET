Imports System
Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Per-field-kind format policies for Content/PwdComplexity.aspx's "Preview Field Format" tool,
    ''' which lets a site administrator try a sample value against the format rule configured for a
    ''' given field kind before rolling it out.
    '''
    ''' Type/polymorphism-dependent tier, SUBSTITUTED: this project's existing MustInherit/
    ''' MustOverride hierarchies (CookieSecurityPolicy, PartnerFeedParser, BackupFormatStrategy,
    ''' PaymentHandoffResolver, PreferenceValue, AttachmentPathResolver,
    ''' NotificationChannelHeaderFormatter) are all domain-mismatched to field-format dispatch, and
    ''' each is already the carrier for a different CWE's plants, so none can host this tier without
    ''' colliding with another CWE's planted lines. This class substitutes a data-driven dispatch
    ''' over a field-kind string key for a type hierarchy: which pattern Validate resolves and
    ''' applies for a given kind plays the same role a subclass's overridden method body would.
    ''' </summary>
    Public Class FieldFormatPolicyRegistry

        ''' <summary>
        ''' Resolves the configured format pattern for one field kind. "promoCode" is the one kind
        ''' whose configured pattern uses a nested-quantifier shape -- see Validate.
        ''' </summary>
        Public Shared Function GetPatternForFieldKind(ByVal fieldKind As String) As String
            Select Case fieldKind
                Case "username"
                    Return "^[a-zA-Z0-9._-]{3,32}$"
                Case "email"
                    Return "^[^@\s]+@[^@\s]+\.[^@\s]+$"
                Case "promoCode"
                    Return "^(([A-Z0-9])+-?)+$"
                Case Else
                    Return ".*"
            End Select
        End Function

        ''' <summary>
        ''' Resolves the field kind's pattern and applies it to the submitted value in one step --
        ''' the data-driven stand-in for a subclass's overridden method body. For "promoCode" this
        ''' wraps "(X+-?)+" around the allowed-character class instead of the flat "X+(-X+)*": a run
        ''' of allowed characters with no dashes gives the engine the same exponential partition
        ''' ambiguity as any other "(X+)+"-shaped pattern once a trailing disallowed character forces
        ''' the overall match to fail. Empirically confirmed catastrophic: 171ms at 20 trailing
        ''' uppercase letters, 5.4s at 25, times out (10s cap) at 30.
        ''' </summary>
        Public Shared Function Validate(ByVal fieldKind As String, ByVal value As String) As Boolean
            Dim pattern As String = GetPatternForFieldKind(fieldKind)
            Return Regex.IsMatch(value, pattern) ' SINK: PLANTED-VB-HR-469
        End Function

        ''' <summary>
        ''' Safe twin of GetPatternForFieldKind: "promoCode" is rewritten from the nested-quantifier
        ''' shape to "one or more allowed characters, then zero or more (dash, one or more allowed
        ''' characters)" -- the same general "alnum blocks separated by single dashes" policy, but
        ''' with exactly one way for the engine to consume any given input (a dash can only ever
        ''' start a new block, never be reconsidered as part of the previous one).
        ''' </summary>
        Public Shared Function GetSafePatternForFieldKind(ByVal fieldKind As String) As String
            Select Case fieldKind
                Case "username"
                    Return "^[a-zA-Z0-9._-]{3,32}$"
                Case "email"
                    Return "^[^@\s]+@[^@\s]+\.[^@\s]+$"
                Case "promoCode"
                    Return "^[A-Z0-9]+(-[A-Z0-9]+)*$"
                Case Else
                    Return ".*"
            End Select
        End Function

        Public Shared Function ValidateSafe(ByVal fieldKind As String, ByVal value As String) As Boolean
            Dim pattern As String = GetSafePatternForFieldKind(fieldKind)
            Return Regex.IsMatch(value, pattern) ' SAFE_SINK: PLANTED-VB-HR-469-safe
        End Function
    End Class
End Namespace
