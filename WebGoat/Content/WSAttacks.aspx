<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="WSAttacks.aspx.vb" Inherits="OWASP.WebGoat.NET.WSAttacks" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HelpContentPlaceholder" runat="server">
This lesson covers attacks against the web services WebGoatCoins uses to exchange data with partner
marketplaces. The "Partner Price Feed Import" tool below simulates the server-side handler for a
partner marketplace's daily coin price-feed upload -- try feeding it an XML document that declares an
external entity.
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
<h1 class="title-regular-4 clearfix">Partner Price Feed Import</h1>

<p>Marketplace partners push their daily coin price updates to us as an XML document. Paste or upload
a feed below to import it.</p>

<h3>Legacy Price Feed</h3>
<p class="tiny">For partners whose integration predates our current export format (their files may
still declare a DOCTYPE for a currency-symbol entity).</p>
<p>
    <asp:TextBox ID="txtLegacyFeed" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnProcessLegacy" runat="server" Text="Import Legacy Feed"
        CssClass="button medium blue" onclick="btnProcessLegacy_Click" />
    <asp:Label ID="lblLegacyResult" runat="server"></asp:Label>
</p>
<p>
    <asp:TextBox ID="txtLegacyFeedSafe" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnProcessLegacySafe" runat="server" Text="Import Legacy Feed (Hardened)"
        CssClass="button medium blue" onclick="btnProcessLegacySafe_Click" />
    <asp:Label ID="lblLegacyResultSafe" runat="server"></asp:Label>
</p>

<hr />
<h3>Modern Price Feed</h3>
<p class="tiny">Our current export format. DOCTYPE-bearing files are still accepted for now so a
partner's transition period isn't blocked outright.</p>
<p>
    <asp:TextBox ID="txtModernFeed" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnProcessModern" runat="server" Text="Import Modern Feed"
        CssClass="button medium blue" onclick="btnProcessModern_Click" />
    <asp:Label ID="lblModernResult" runat="server"></asp:Label>
</p>
<p>
    <asp:TextBox ID="txtModernFeedSafe" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnProcessModernSafe" runat="server" Text="Import Modern Feed (Strict)"
        CssClass="button medium blue" onclick="btnProcessModernSafe_Click" />
    <asp:Label ID="lblModernResultSafe" runat="server"></asp:Label>
</p>

<hr />
<h3>Bulk Partner Feed Import</h3>
<p class="tiny">Some partners send their daily update as several smaller per-category files in one
upload instead of a single combined document.</p>
<p>
    <asp:FileUpload ID="filePartnerFeeds" runat="server" AllowMultiple="true" />
    <asp:Button ID="btnImportBatch" runat="server" Text="Import Batch"
        CssClass="button medium blue" onclick="btnImportBatch_Click" />
    <asp:Label ID="lblBatchResult" runat="server"></asp:Label>
</p>
<p>
    <asp:FileUpload ID="filePartnerFeedsSafe" runat="server" AllowMultiple="true" />
    <asp:Button ID="btnImportBatchSafe" runat="server" Text="Import Batch (Hardened)"
        CssClass="button medium blue" onclick="btnImportBatchSafe_Click" />
    <asp:Label ID="lblBatchResultSafe" runat="server"></asp:Label>
</p>

<hr />
<h3>Auto-Detect Format</h3>
<p class="tiny">Paste a feed in either format -- the importer inspects the document's own root
element to decide which parser to use, so you don't have to say which one it is.</p>
<p>
    <asp:TextBox ID="txtAutoFeed" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnAutoDetect" runat="server" Text="Import (Auto-Detect)"
        CssClass="button medium blue" onclick="btnAutoDetect_Click" />
    <asp:Label ID="lblAutoResult" runat="server"></asp:Label>
</p>
<p>
    <asp:TextBox ID="txtAutoFeedSafe" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnAutoDetectSafe" runat="server" Text="Import (Auto-Detect)"
        CssClass="button medium blue" onclick="btnAutoDetectSafe_Click" />
    <asp:Label ID="lblAutoResultSafe" runat="server"></asp:Label>
</p>

<hr />
<h3>Preview Partner Feed</h3>
<p class="tiny">Stage a feed here, then finish the import from the Confirm Import step on the WSDL
Exploits page -- mirrors how our WSDL-advertised partner endpoint splits preview and confirm into two
separate calls.</p>
<p>
    <asp:TextBox ID="txtPreviewFeed" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnPreviewFeed" runat="server" Text="Preview Feed"
        CssClass="button medium blue" onclick="btnPreviewFeed_Click" />
    <asp:Label ID="lblPreviewResult" runat="server"></asp:Label>
</p>
<p>
    <asp:TextBox ID="txtPreviewFeedSafe" runat="server" TextMode="MultiLine" Rows="6" Columns="70"></asp:TextBox>
    <br />
    <asp:Button ID="btnPreviewFeedSafe" runat="server" Text="Preview Feed (Hardened path)"
        CssClass="button medium blue" onclick="btnPreviewFeedSafe_Click" />
    <asp:Label ID="lblPreviewResultSafe" runat="server"></asp:Label>
</p>
</asp:Content>
