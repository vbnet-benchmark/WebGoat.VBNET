Imports System
Imports System.Text
Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Shared helpers for the "Our Offices" sales-directory lookup lessons (XPathInjection.aspx
    ''' and SalesDirectoryService.vb). XPath 1.0's .NET API (XmlDocument.SelectNodes/
    ''' XPathNavigator.Select/Evaluate) has no parameterized-query mechanism equivalent to
    ''' SqlCommand/SqlParameter, so a value spliced into an XPath expression string must either be
    ''' escaped as a safe XPath string literal, or validated against a strict allow-list, before
    ''' it is concatenated in.
    ''' </summary>
    Public Class XPathSafetyHelpers

        ''' <summary>
        ''' Renders value as a safe XPath 1.0 string literal. XPath string literals have no
        ''' backslash-escape sequence -- only a choice of delimiter -- so a value containing just
        ''' one of the two quote characters is wrapped in the other; a value containing both is
        ''' built via XPath's concat() function instead, splitting on every single quote and
        ''' rejoining the pieces with a "'" literal between them, so the result can never be
        ''' interpreted as anything other than a literal string value no matter what punctuation
        ''' it contains.
        ''' </summary>
        Public Shared Function XPathStringLiteral(ByVal value As String) As String
            If value Is Nothing Then
                value = String.Empty
            End If

            If Not value.Contains("'") Then
                Return "'" & value & "'"
            ElseIf Not value.Contains("""") Then
                Return """" & value & """"
            Else
                Dim parts As String() = value.Split("'"c)
                Dim sb As New StringBuilder()
                sb.Append("concat(")
                For i As Integer = 0 To parts.Length - 1
                    sb.Append("'").Append(parts(i)).Append("'")
                    If i < parts.Length - 1 Then
                        sb.Append(", ""'"", ")
                    End If
                Next
                sb.Append(")")
                Return sb.ToString()
            End If
        End Function

        ''' <summary>
        ''' Strict allow-list check for a short directory-lookup field (a city or state name): only
        ''' letters, digits, spaces, '-' and '_' are accepted, which excludes every character XPath
        ''' predicate syntax needs to break out of a literal ('\'', '"') or alter the predicate's
        ''' structure ('(', ')', '=').
        ''' </summary>
        Public Shared Function IsValidIdentifier(ByVal value As String) As Boolean
            If String.IsNullOrEmpty(value) Then
                Return False
            End If
            Return Regex.IsMatch(value, "^[A-Za-z0-9 _-]+$")
        End Function
    End Class
End Namespace
