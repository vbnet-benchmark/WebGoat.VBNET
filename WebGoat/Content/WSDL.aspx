<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="WSDL.aspx.vb" Inherits="OWASP.WebGoat.NET.WSDL" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
<h1 class="title-regular-4 clearfix">Confirm Partner Feed Import</h1>

<p>Finishes a partner price-feed import staged by the "Preview Partner Feed" tool on the
<a href="WSAttacks.aspx">Web Service Attacks</a> page -- mirrors how our WSDL-advertised partner
endpoint splits a feed upload into a preview call and a separate confirm call.</p>

<p>
    <asp:Button ID="btnConfirmImport" runat="server" Text="Confirm Import"
        CssClass="button medium blue" onclick="btnConfirmImport_Click" />
    <asp:Label ID="lblConfirmResult" runat="server"></asp:Label>
</p>
<p>
    <asp:Button ID="btnValidateOnly" runat="server" Text="Validate Only (Hardened path)"
        CssClass="button medium blue" onclick="btnValidateOnly_Click" />
    <asp:Label ID="lblValidateResult" runat="server"></asp:Label>
</p>
</asp:Content>
