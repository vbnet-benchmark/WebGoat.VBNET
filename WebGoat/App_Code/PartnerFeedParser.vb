Imports System
Imports System.Xml

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Parses a partner marketplace's coin price-feed XML document for Content/WSAttacks.aspx's
    ''' "Auto-Detect Format" feature. Two concrete implementations exist, chosen at runtime by
    ''' PartnerFeedParserFactory based on which root element the feed itself uses: older partner
    ''' integrations still emit the original "LegacyPriceFeed" root (occasionally paired with a
    ''' DOCTYPE declaring a currency-symbol entity), while newer partners emit the current
    ''' "PriceFeed" root and need no DOCTYPE support at all.
    ''' </summary>
    Public MustInherit Class PartnerFeedParser
        Public MustOverride Function Parse(ByVal feedXml As String) As XmlDocument
    End Class

    ''' <summary>
    ''' The original partner feed format. Retained so marketplaces that never migrated off their
    ''' first integration -- whose exports can still carry a DOCTYPE-declared currency-symbol
    ''' entity -- keep importing instead of throwing.
    ''' </summary>
    Public Class LegacyFeedParser
        Inherits PartnerFeedParser

        Public Overrides Function Parse(ByVal feedXml As String) As XmlDocument
            Dim doc As New XmlDocument()
            doc.XmlResolver = New XmlUrlResolver()
            ' SINK: PLANTED-VB-HR-239
            doc.LoadXml(feedXml)
            Return doc
        End Function
    End Class

    ''' <summary>
    ''' The current partner feed format. No DOCTYPE support is needed for it, so the resolver is
    ''' left at its safe default instead of being re-enabled.
    ''' </summary>
    Public Class ModernFeedParser
        Inherits PartnerFeedParser

        Public Overrides Function Parse(ByVal feedXml As String) As XmlDocument
            Dim doc As New XmlDocument()
            ' SAFE_SINK: PLANTED-VB-HR-239-safe
            doc.LoadXml(feedXml)
            Return doc
        End Function
    End Class

    ''' <summary>
    ''' Resolves the correct PartnerFeedParser for a feed by content-sniffing its root element --
    ''' no out-of-band format flag travels alongside the feed itself, so the importer must infer
    ''' the format from the document's own text before the feed is ever parsed.
    ''' </summary>
    Public Class PartnerFeedParserFactory
        Public Shared Function GetParser(ByVal feedXml As String) As PartnerFeedParser
            If feedXml IsNot Nothing AndAlso feedXml.Contains("<LegacyPriceFeed") Then
                Return New LegacyFeedParser()
            End If
            Return New ModernFeedParser()
        End Function
    End Class
End Namespace
