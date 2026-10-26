<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="AdminTools.aspx.vb" Inherits="OWASP.WebGoat.NET.WebGoatCoins.AdminTools" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HelpContentPlaceholder" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h1 class="title-regular-4 clearfix">Legacy Admin Tools</h1>
    <p class="tiny">A small back-office toolbox, carried over from the original VB6 admin console,
    for maintenance tasks the current admin UI never got around to replacing: opening an archived
    customer document in the legacy viewer, kicking off the overnight export batch by hand, and
    running a named account "Quick Action" or diagnostic check without waiting for its own
    dedicated button.</p>

    <hr />
    <h3>Open Customer Document</h3>
    <p class="tiny">Opens the named document from the archive in the legacy document viewer.</p>
    <p>
        Document name: <asp:TextBox ID="txtDocName" runat="server"></asp:TextBox>
        <asp:Button ID="btnOpenDocument" runat="server" Text="Open"
            CssClass="button medium blue" OnClick="btnOpenDocument_Click" />
    </p>
    <asp:Label ID="lblDocumentResult" runat="server"></asp:Label>
    <p>
        Document name: <asp:TextBox ID="txtDocNameSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnOpenDocumentSafe" runat="server" Text="Open (Hardened)"
            CssClass="button medium blue" OnClick="btnOpenDocumentSafe_Click" />
    </p>
    <asp:Label ID="lblDocumentResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Run Scheduled Export Batch</h3>
    <p class="tiny">Runs the overnight export batch script immediately, tagging the output with
    the given label.</p>
    <p>
        Label: <asp:TextBox ID="txtExportLabel" runat="server"></asp:TextBox>
        <asp:Button ID="btnRunExport" runat="server" Text="Run Export"
            CssClass="button medium blue" OnClick="btnRunExport_Click" />
    </p>
    <asp:Label ID="lblExportResult" runat="server"></asp:Label>
    <p>
        Label: <asp:TextBox ID="txtExportLabelSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnRunExportSafe" runat="server" Text="Run Export (Hardened)"
            CssClass="button medium blue" OnClick="btnRunExportSafe_Click" />
    </p>
    <asp:Label ID="lblExportResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Quick Actions</h3>
    <p class="tiny">Runs a single named account Quick Action (unlock, resend, purgecart) against
    the current customer record.</p>
    <p>
        Action name: <asp:TextBox ID="txtQuickAction" runat="server"></asp:TextBox>
        <asp:Button ID="btnQuickAction" runat="server" Text="Run Action"
            CssClass="button medium blue" OnClick="btnQuickAction_Click" />
    </p>
    <asp:Label ID="lblQuickActionResult" runat="server"></asp:Label>
    <p>
        Action name: <asp:TextBox ID="txtQuickActionSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnQuickActionSafe" runat="server" Text="Run Action (Hardened)"
            CssClass="button medium blue" OnClick="btnQuickActionSafe_Click" />
    </p>
    <asp:Label ID="lblQuickActionResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Batch Quick Actions</h3>
    <p class="tiny">Runs a comma-separated list of account Quick Actions against the current
    customer record in one request.</p>
    <p>
        Action names (comma-separated): <asp:TextBox ID="txtBatchActions" runat="server"></asp:TextBox>
        <asp:Button ID="btnBatchActions" runat="server" Text="Run Batch"
            CssClass="button medium blue" OnClick="btnBatchActions_Click" />
    </p>
    <asp:Label ID="lblBatchActionsResult" runat="server"></asp:Label>
    <p>
        Action names (comma-separated): <asp:TextBox ID="txtBatchActionsSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnBatchActionsSafe" runat="server" Text="Run Batch (Hardened)"
            CssClass="button medium blue" OnClick="btnBatchActionsSafe_Click" />
    </p>
    <asp:Label ID="lblBatchActionsResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Run Named Diagnostic</h3>
    <p class="tiny">Administrators only. Runs a single named diagnostic check (diskspace,
    dbconnectivity, flushcache) immediately.</p>
    <p>
        Diagnostic name: <asp:TextBox ID="txtDiagnosticName" runat="server"></asp:TextBox>
        <asp:Button ID="btnDiagnostic" runat="server" Text="Run Diagnostic"
            CssClass="button medium blue" OnClick="btnDiagnostic_Click" />
    </p>
    <asp:Label ID="lblDiagnosticResult" runat="server"></asp:Label>
    <p>
        Diagnostic name: <asp:TextBox ID="txtDiagnosticNameSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnDiagnosticSafe" runat="server" Text="Run Diagnostic (Hardened)"
            CssClass="button medium blue" OnClick="btnDiagnosticSafe_Click" />
    </p>
    <asp:Label ID="lblDiagnosticResultSafe" runat="server"></asp:Label>
</asp:Content>
