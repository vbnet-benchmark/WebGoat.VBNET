Imports System
Imports System.IO
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Removes a customer's own previously-uploaded document (an ID scan, a proof-of-address
    ''' file) from the shared uploads area. Used by WebGoat/MainPage.aspx's "remove one of my
    ''' uploaded documents" portal feature. The actual physical-path resolution is delegated one
    ''' further hop, to ResolveUploadedDocumentPath below, so this class's public entry points
    ''' stay thin, documented wrappers -- mirroring AttachmentPathResolver.vb's own
    ''' name-to-physical-path resolution shape elsewhere in this project.
    ''' </summary>
    Public Class CustomerDocumentService

        ''' <summary>
        ''' Deletes the named uploaded document outright. The raw "removeDocument" query value is
        ''' handed straight through from MainPage.aspx.vb's Page_Load with no validation of its
        ''' own.
        ''' </summary>
        Public Shared Sub RemoveUploadedDocument(ByVal documentName As String)
            Try
                Dim documentPath As String = ResolveUploadedDocumentPath(documentName)
                ' SINK: PLANTED-VB-HR-608
                File.Delete(documentPath)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Safe twin of RemoveUploadedDocument: the requested name is reduced to its bare file
        ''' name and checked against the fixed, server-controlled list of documents a customer's
        ''' own upload area is actually known to hold before the physical path is ever resolved.
        ''' </summary>
        Public Shared Sub RemoveUploadedDocumentSafe(ByVal documentName As String)
            Try
                Dim safeDocumentName As String = Path.GetFileName(documentName)
                If safeDocumentName.Length = 0 OrElse safeDocumentName = "." OrElse safeDocumentName = ".." OrElse
                    Not AttachmentRegistry.IsKnownUploadedDocument(safeDocumentName) Then
                    Return
                End If

                Dim documentPathSafe As String = ResolveUploadedDocumentPath(safeDocumentName)
                ' SAFE_SINK: PLANTED-VB-HR-608-safe
                File.Delete(documentPathSafe)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Resolves a document name to its physical path under the shared uploads directory --
        ''' one further hop away from the two public entry points above.
        ''' </summary>
        Private Shared Function ResolveUploadedDocumentPath(ByVal documentName As String) As String
            Return HttpContext.Current.Server.MapPath("~/WebGoatCoins/uploads/" & documentName)
        End Function
    End Class
End Namespace
