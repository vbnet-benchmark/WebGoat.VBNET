Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Xml

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Bulk-imports a batch of partner marketplace price-feed files for Content/WSAttacks.aspx's
    ''' "Bulk Partner Feed Import" feature. Used when a marketplace partner sends its daily price
    ''' update as several smaller per-category files in one upload rather than a single combined
    ''' document.
    ''' </summary>
    Public Class PartnerFeedImportService

        ''' <summary>
        ''' Imports every posted file in the batch, re-enabling external-entity resolution so
        ''' older per-category export files that still declare a DOCTYPE keep importing -- carried
        ''' over unchanged from the single-file importer this batch feature replaced.
        ''' </summary>
        Public Shared Function ImportBatch(ByVal postedFiles As IList(Of HttpPostedFile)) As Integer
            Dim importedCount As Integer = 0
            For Each postedFile As HttpPostedFile In postedFiles
                If postedFile IsNot Nothing AndAlso postedFile.ContentLength > 0 Then
                    Dim doc As New XmlDocument()
                    doc.XmlResolver = New XmlUrlResolver()
                    ' SINK: PLANTED-VB-HR-238
                    doc.Load(postedFile.InputStream)
                    importedCount += 1
                End If
            Next
            Return importedCount
        End Function

        ''' <summary>
        ''' Same batch-import loop as ImportBatch, but the resolver is never touched, so an
        ''' attacker-supplied DOCTYPE anywhere in the batch is rejected rather than resolved.
        ''' </summary>
        Public Shared Function ImportBatchSafe(ByVal postedFiles As IList(Of HttpPostedFile)) As Integer
            Dim importedCount As Integer = 0
            For Each postedFile As HttpPostedFile In postedFiles
                If postedFile IsNot Nothing AndAlso postedFile.ContentLength > 0 Then
                    Dim doc As New XmlDocument()
                    ' SAFE_SINK: PLANTED-VB-HR-238-safe
                    doc.Load(postedFile.InputStream)
                    importedCount += 1
                End If
            Next
            Return importedCount
        End Function
    End Class
End Namespace
