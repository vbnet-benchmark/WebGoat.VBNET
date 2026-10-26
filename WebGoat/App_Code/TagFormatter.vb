Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Builds the "Filtered by: ..." banner shown above a customer's message list when they
    ''' filter by one or more tags (WebGoatCoins/Messages.aspx?tag=...&amp;tag=...).
    ''' </summary>
    Public Class TagFormatter
        ''' <summary>
        ''' Builds the banner with each tag included exactly as submitted (no encoding).
        ''' </summary>
        Public Shared Function BuildTagSummary(ByVal tags As IEnumerable(Of String)) As String
            Dim sb As New StringBuilder()
            sb.Append("Filtered by: ")
            For Each tag As String In tags
                sb.Append(tag)
                sb.Append(", ")
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Builds the banner with each tag HTML-encoded before inclusion.
        ''' </summary>
        Public Shared Function BuildTagSummarySafe(ByVal tags As IEnumerable(Of String)) As String
            Dim sb As New StringBuilder()
            sb.Append("Filtered by: ")
            For Each tag As String In tags
                sb.Append(HttpUtility.HtmlEncode(tag))
                sb.Append(", ")
            Next
            Return sb.ToString()
        End Function
    End Class
End Namespace
