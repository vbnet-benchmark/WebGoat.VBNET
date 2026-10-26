Imports System

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Formats the "which notification channel to expect a confirmation on" detail that
    ''' Content/ResponseSplitting.aspx's redirect-notice feature attaches as a custom response
    ''' header. Two concrete implementations exist, chosen at runtime by
    ''' ResponseSplitting.aspx.vb based on the "channelMode" query parameter: the original
    ''' formatter this feature shipped with returns the caller-supplied detail unchanged, while
    ''' the current formatter strips any line-break characters from it first.
    ''' </summary>
    Public MustInherit Class NotificationChannelHeaderFormatter
        Public MustOverride Function FormatHeaderValue(ByVal rawDetail As String) As String
    End Class

    ''' <summary>
    ''' The original formatter: returns the caller-supplied channel detail exactly as submitted.
    ''' </summary>
    Public Class LegacyChannelHeaderFormatter
        Inherits NotificationChannelHeaderFormatter

        Public Overrides Function FormatHeaderValue(ByVal rawDetail As String) As String
            Return rawDetail
        End Function
    End Class

    ''' <summary>
    ''' The current formatter: strips every CR and LF character from the channel detail before it
    ''' is used as a header value, so no obs-fold continuation (a lone CR or LF, not just the
    ''' two-character CRLF sequence) can survive into the response either.
    ''' </summary>
    Public Class ModernChannelHeaderFormatter
        Inherits NotificationChannelHeaderFormatter

        Public Overrides Function FormatHeaderValue(ByVal rawDetail As String) As String
            Return rawDetail.Replace(Chr(13), "").Replace(Chr(10), "")
        End Function
    End Class
End Namespace
