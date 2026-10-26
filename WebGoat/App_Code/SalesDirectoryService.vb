Imports System
Imports System.Collections.Generic
Imports System.Xml

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Cross-cutting sales-directory lookups for XPathInjection.aspx's "Our Offices" lesson that
    ''' go beyond a single-field search: looking a salesperson up by more than one field at once,
    ''' and a bulk, multi-state search. The page loads the in-memory sales directory document and
    ''' hands it here so the query-building logic for these two lookups lives in one place instead
    ''' of being duplicated per caller, the same factoring OrderAttachmentService.vb already uses
    ''' for order-attachment lookups.
    ''' </summary>
    Public Class SalesDirectoryService

        ''' <summary>
        ''' Looks a salesperson up by name AND state at once -- both fields are spliced into a
        ''' single 'and'-joined XPath predicate, so either field alone is enough to inject if the
        ''' other happens to be benign.
        ''' </summary>
        Public Shared Function FindByNameAndState(ByVal doc As XmlDocument, ByVal name As String, ByVal state As String) As XmlNodeList
            Dim xpath As String = "//salesperson[name='" & name & "' and state='" & state & "']"
            ' SINK: PLANTED-VB-HR-363
            Return doc.SelectNodes(xpath)
        End Function

        ''' <summary>
        ''' Safe twin of FindByNameAndState: both combined sources are escaped as XPath string
        ''' literals before being spliced in -- escaping only one of the two would still leave the
        ''' predicate injectable through the other.
        ''' </summary>
        Public Shared Function FindByNameAndStateSafe(ByVal doc As XmlDocument, ByVal name As String, ByVal state As String) As XmlNodeList
            Dim xpath As String = "//salesperson[name=" & XPathSafetyHelpers.XPathStringLiteral(name) & " and state=" & XPathSafetyHelpers.XPathStringLiteral(state) & "]"
            ' SAFE_SINK: PLANTED-VB-HR-363-safe
            Return doc.SelectNodes(xpath)
        End Function

        ''' <summary>
        ''' Bulk lookup across several states at once (comma-separated): one 'state=...' clause per
        ''' entry, OR-joined into a single predicate. No genuine reusable polymorphic hierarchy in
        ''' this project plausibly gates an XPath-query-construction decision (the existing
        ''' MustInherit/MustOverride hierarchies -- CookieSecurityPolicy, PartnerFeedParser,
        ''' BackupFormatStrategy -- are all domain-mismatched to a sales-directory lookup and are
        ''' already consumed by other CWEs' plants), so this tier is substituted with a loop/batch
        ''' construction shape instead.
        ''' </summary>
        Public Shared Function FindByStates(ByVal doc As XmlDocument, ByVal statesCsv As String) As XmlNodeList
            Dim clauses As New List(Of String)()
            If statesCsv IsNot Nothing Then
                For Each entry As String In statesCsv.Split(","c)
                    Dim trimmed As String = entry.Trim()
                    If trimmed.Length > 0 Then
                        clauses.Add("state='" & trimmed & "'")
                    End If
                Next
            End If
            If clauses.Count = 0 Then
                Return Nothing
            End If
            Dim xpath As String = "//salesperson[" & String.Join(" or ", clauses) & "]"
            ' SINK: PLANTED-VB-HR-364
            Return doc.SelectNodes(xpath)
        End Function

        ''' <summary>
        ''' Safe twin of FindByStates: each entry is validated against the shared allow-list before
        ''' it is folded into the OR-joined predicate; an entry that fails validation is dropped
        ''' rather than ever reaching the expression string.
        ''' </summary>
        Public Shared Function FindByStatesSafe(ByVal doc As XmlDocument, ByVal statesCsv As String) As XmlNodeList
            Dim clauses As New List(Of String)()
            If statesCsv IsNot Nothing Then
                For Each entry As String In statesCsv.Split(","c)
                    Dim trimmed As String = entry.Trim()
                    If XPathSafetyHelpers.IsValidIdentifier(trimmed) Then
                        clauses.Add("state='" & trimmed & "'")
                    End If
                Next
            End If
            If clauses.Count = 0 Then
                Return Nothing
            End If
            Dim xpath As String = "//salesperson[" & String.Join(" or ", clauses) & "]"
            ' SAFE_SINK: PLANTED-VB-HR-364-safe
            Return doc.SelectNodes(xpath)
        End Function
    End Class
End Namespace
