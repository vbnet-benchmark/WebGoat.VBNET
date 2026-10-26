<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="CommandInjection.aspx.vb" Inherits="OWASP.WebGoat.NET.CommandInjection" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h1 class="title-regular-4 clearfix">Network Diagnostics</h1>
    <p class="tiny">A small toolbox the support team uses to check connectivity from the server
    itself -- ping a host, trace the route to it, resolve its name, or sweep a short list of
    hosts at once. Each tool shells out to the matching platform utility and streams its output
    back to the page.</p>

    <hr />
    <h3>Ping a Host</h3>
    <p class="tiny">Sends four ICMP echo requests to the given host or IP address.</p>
    <p>
        Host: <asp:TextBox ID="txtPingHost" runat="server"></asp:TextBox>
        <asp:Button ID="btnPing" runat="server" Text="Ping"
            CssClass="button medium blue" onclick="btnPing_Click" />
    </p>
    <asp:Label ID="lblPingResult" runat="server"></asp:Label>
    <p>
        Host: <asp:TextBox ID="txtPingHostSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnPingSafe" runat="server" Text="Ping (Hardened)"
            CssClass="button medium blue" onclick="btnPingSafe_Click" />
    </p>
    <asp:Label ID="lblPingResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Trace Route</h3>
    <p class="tiny">Shows the network path (up to 15 hops) to the given host or IP address.</p>
    <p>
        Host: <asp:TextBox ID="txtTraceHost" runat="server"></asp:TextBox>
        <asp:Button ID="btnTrace" runat="server" Text="Trace Route"
            CssClass="button medium blue" onclick="btnTrace_Click" />
    </p>
    <asp:Label ID="lblTraceResult" runat="server"></asp:Label>
    <p>
        Host: <asp:TextBox ID="txtTraceHostSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnTraceSafe" runat="server" Text="Trace Route (Hardened)"
            CssClass="button medium blue" onclick="btnTraceSafe_Click" />
    </p>
    <asp:Label ID="lblTraceResultSafe" runat="server"></asp:Label>

    <hr />
    <h3>Resolve Host</h3>
    <p class="tiny">Looks up the given host's address record using the selected lookup backend.</p>
    <p>
        Host: <asp:TextBox ID="txtLookupHost" runat="server"></asp:TextBox>
        Backend:
        <asp:DropDownList ID="ddlLookupBackend" runat="server">
            <asp:ListItem>Legacy</asp:ListItem>
            <asp:ListItem>Validated</asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="btnLookup" runat="server" Text="Resolve"
            CssClass="button medium blue" onclick="btnLookup_Click" />
    </p>
    <asp:Label ID="lblLookupResult" runat="server"></asp:Label>

    <hr />
    <h3>Ping Multiple Hosts</h3>
    <p class="tiny">Enter one host or IP address per line (or comma-separated) to ping each of
    them in turn.</p>
    <p>
        <asp:TextBox ID="txtSweepHosts" runat="server" TextMode="MultiLine" Rows="4" Columns="40"></asp:TextBox><br />
        <asp:Button ID="btnSweep" runat="server" Text="Ping All"
            CssClass="button medium blue" onclick="btnSweep_Click" />
    </p>
    <asp:Label ID="lblSweepResult" runat="server"></asp:Label>
    <p>
        <asp:TextBox ID="txtSweepHostsSafe" runat="server" TextMode="MultiLine" Rows="4" Columns="40"></asp:TextBox><br />
        <asp:Button ID="btnSweepSafe" runat="server" Text="Ping All (Hardened)"
            CssClass="button medium blue" onclick="btnSweepSafe_Click" />
    </p>
    <asp:Label ID="lblSweepResultSafe" runat="server"></asp:Label>
</asp:Content>
