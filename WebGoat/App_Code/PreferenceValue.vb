Imports System
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Renders a single customer preference value as an HTML fragment for
    ''' WebGoatCoins/CustomerSettings.aspx. Two concrete implementations exist: one echoes the
    ''' raw preference value, the other HTML-encodes it first. Which one runs is decided by
    ''' CustomerSettings.aspx.vb based on the "prefMode" query parameter.
    ''' </summary>
    Public MustInherit Class PreferenceValue
        Protected ReadOnly RawValue As String

        Public Sub New(ByVal rawValue As String)
            Me.RawValue = rawValue
        End Sub

        Public MustOverride Function Render() As String
    End Class

    ''' <summary>
    ''' Renders the preference value exactly as submitted, with no encoding.
    ''' </summary>
    Public Class EchoPreferenceValue
        Inherits PreferenceValue

        Public Sub New(ByVal rawValue As String)
            MyBase.New(rawValue)
        End Sub

        Public Overrides Function Render() As String
            Return RawValue
        End Function
    End Class

    ''' <summary>
    ''' Renders the preference value HTML-encoded, safe for direct inclusion in the response body.
    ''' </summary>
    Public Class EncodedPreferenceValue
        Inherits PreferenceValue

        Public Sub New(ByVal rawValue As String)
            MyBase.New(rawValue)
        End Sub

        Public Overrides Function Render() As String
            Return HttpUtility.HtmlEncode(RawValue)
        End Function
    End Class
End Namespace
