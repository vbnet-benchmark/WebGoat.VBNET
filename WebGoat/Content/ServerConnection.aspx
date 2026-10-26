<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true"
	CodeBehind="ServerConnection.aspx.vb" Inherits="OWASP.WebGoat.NET.ServerConnection" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HelpContentPlaceholder" runat="server">
	WebGoat.NET's partner-integration tools let you point several internal "connectivity check"
	utilities at any host and port you like, with no restriction on what that target is. A real
	attacker could use a tool like this to make the SERVER connect to an arbitrary internal or
	external endpoint on their behalf (port-scanning the internal network, reaching a service that
	was never meant to be exposed, etc.) -- this is CWE-941, Arbitrary Server Connection. Try
	pointing the tools below at a host/port combination you weren't given!
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
	<h1 class="title-regular-4 clearfix">Arbitrary Server Connection</h1>
	<div class="ContentArea">

		<h2>1. Direct Connectivity Check</h2>
		Enter a host and port for WebGoat.NET's server to connect to directly.<br />
		Host: <asp:TextBox ID="txtDirectHost" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtDirectPort" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnDirectConnect" runat="server" Text="Connect" OnClick="btnDirectConnect_Click" />
		<p />
		<asp:Label ID="lblDirectResult" runat="server"></asp:Label>
		<p />
		<em>Hardened version (validated against an approved allow-list):</em><br />
		Host: <asp:TextBox ID="txtDirectHostSafe" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtDirectPortSafe" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnDirectConnectSafe" runat="server" Text="Connect" OnClick="btnDirectConnectSafe_Click" />
		<p />
		<asp:Label ID="lblDirectResultSafe" runat="server"></asp:Label>

		<hr />

		<h2>2. Quick Health Check</h2>
		A one-click health check against a partner host/port of your choosing.<br />
		Host: <asp:TextBox ID="txtHealthCheckHost" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtHealthCheckPort" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnQuickHealthCheck" runat="server" Text="Run Health Check" OnClick="btnQuickHealthCheck_Click" />
		<p />
		<asp:Label ID="lblHealthCheckResult" runat="server"></asp:Label>
		<p />
		<em>Hardened version (validated against an approved allow-list):</em><br />
		Host: <asp:TextBox ID="txtHealthCheckHostSafe" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtHealthCheckPortSafe" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnQuickHealthCheckSafe" runat="server" Text="Run Health Check" OnClick="btnQuickHealthCheckSafe_Click" />
		<p />
		<asp:Label ID="lblHealthCheckResultSafe" runat="server"></asp:Label>

		<hr />

		<h2>3. Partner Endpoint Connectivity Probe</h2>
		Probes a partner integration endpoint (routed through the server's network-diagnostics
		service).<br />
		Host: <asp:TextBox ID="txtPartnerHost" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtPartnerPort" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnPartnerProbe" runat="server" Text="Probe Endpoint" OnClick="btnPartnerProbe_Click" />
		<p />
		<asp:Label ID="lblPartnerProbeResult" runat="server"></asp:Label>
		<p />
		<em>Hardened version (validated against an approved allow-list):</em><br />
		Host: <asp:TextBox ID="txtPartnerHostSafe" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtPartnerPortSafe" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnPartnerProbeSafe" runat="server" Text="Probe Endpoint" OnClick="btnPartnerProbeSafe_Click" />
		<p />
		<asp:Label ID="lblPartnerProbeResultSafe" runat="server"></asp:Label>

		<hr />

		<h2>4. Connection Mode</h2>
		Choose a connector implementation, then connect to any host/port -- notice the result
		changes only because of WHICH connector object gets built, not because of any visible
		branch at the connection call site itself.<br />
		Mode:
		<asp:DropDownList ID="ddlConnectionMode" runat="server">
			<asp:ListItem Text="Legacy" Value="legacy" />
			<asp:ListItem Text="Validated" Value="validated" />
		</asp:DropDownList>
		Host: <asp:TextBox ID="txtModeHost" runat="server" Width="250px"></asp:TextBox>
		Port: <asp:TextBox ID="txtModePort" runat="server" Width="60px"></asp:TextBox>
		<asp:Button ID="btnModeConnect" runat="server" Text="Connect" OnClick="btnModeConnect_Click" />
		<p />
		<asp:Label ID="lblModeConnectResult" runat="server"></asp:Label>

		<hr />

		<h2>5. Batch Connectivity Sweep</h2>
		Enter one <code>host:port</code> pair per line to connect to each of them in turn.<br />
		<asp:TextBox ID="txtBatchTargets" runat="server" TextMode="MultiLine" Rows="5" Columns="40"></asp:TextBox>
		<p />
		<asp:Button ID="btnBatchProbe" runat="server" Text="Run Sweep" OnClick="btnBatchProbe_Click" />
		<p />
		<asp:Label ID="lblBatchResult" runat="server"></asp:Label>
		<p />
		<em>Hardened version (every target validated against an approved allow-list):</em><br />
		<asp:TextBox ID="txtBatchTargetsSafe" runat="server" TextMode="MultiLine" Rows="5" Columns="40"></asp:TextBox>
		<p />
		<asp:Button ID="btnBatchProbeSafe" runat="server" Text="Run Sweep" OnClick="btnBatchProbeSafe_Click" />
		<p />
		<asp:Label ID="lblBatchResultSafe" runat="server"></asp:Label>

	</div>
</asp:Content>
