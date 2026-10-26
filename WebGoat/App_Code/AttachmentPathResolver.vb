Imports System
Imports System.IO
Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Resolves a raw, caller-supplied attachment name into a physical path for
    ''' WebGoatCoins/OrderAttachments.aspx's "polyAttachment" preview feature. Two concrete
    ''' implementations exist, chosen at runtime by OrderAttachments.aspx.vb based on the
    ''' "resolverMode" query parameter: one resolves against the "public intake" queue (anonymous,
    ''' customer-submitted documents pending triage -- nothing about the name can be assumed), the
    ''' other against the ordinary, already-validated customer attachment area.
    ''' </summary>
    Public MustInherit Class AttachmentPathResolver
        Public MustOverride Function Resolve(ByVal rawName As String) As String
    End Class

    ''' <summary>
    ''' Resolves against the public intake queue. Names arriving here come straight from an
    ''' anonymous upload form, so nothing about them can be trusted -- but this resolver does not
    ''' account for that, and simply appends the raw name as-is.
    ''' </summary>
    Public Class PublicAttachmentPathResolver
        Inherits AttachmentPathResolver

        Public Overrides Function Resolve(ByVal rawName As String) As String
            Return HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/intake/" & rawName)
        End Function
    End Class

    ''' <summary>
    ''' Resolves against the ordinary customer attachment area. Reduces the name to its bare
    ''' file-name component first, so the result can never resolve outside that fixed directory.
    ''' </summary>
    Public Class TrustedAttachmentPathResolver
        Inherits AttachmentPathResolver

        Public Overrides Function Resolve(ByVal rawName As String) As String
            Return HttpContext.Current.Server.MapPath("~/WebGoatCoins/attachments/trusted/" & Path.GetFileName(rawName))
        End Function
    End Class
End Namespace
