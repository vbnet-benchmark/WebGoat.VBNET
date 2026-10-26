<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="XPathInjection.aspx.vb" Inherits="OWASP.WebGoat.NET.XPathInjection" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h1 class="title-regular-4 clearfix">Our Offices</h1>
    Need to find a sales person?&nbsp; Click on a city below and get the details.<br />
    <br />

    <a href="XPathInjection.aspx?state=ca">California</a> |
    <a href="XPathInjection.aspx?state=or">Oregon</a> |
    <a href="XPathInjection.aspx?state=ny">New York</a> |
    <a href="XPathInjection.aspx?state=tx">Texas</a>

    <br />
    <br />

    <hr />
    <h3>Find by State</h3>
    <p class="tiny">Don't see your state in the list above? Type it in directly.</p>
    <p>
        <asp:TextBox ID="txtStateSearch" runat="server"></asp:TextBox>
        <asp:Button ID="btnStateSearch" runat="server" Text="Search"
            CssClass="button medium blue" onclick="btnStateSearch_Click" />
        <asp:Label ID="lblStateResult" runat="server"></asp:Label>
    </p>
    <p>
        <asp:TextBox ID="txtStateSearchSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnStateSearchSafe" runat="server" Text="Search (Hardened)"
            CssClass="button medium blue" onclick="btnStateSearchSafe_Click" />
        <asp:Label ID="lblStateResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Find by City</h3>
    <p>
        <asp:TextBox ID="txtCitySearch" runat="server"></asp:TextBox>
        <asp:Button ID="btnCitySearch" runat="server" Text="Search"
            CssClass="button medium blue" onclick="btnCitySearch_Click" />
        <asp:Label ID="lblCityResult" runat="server"></asp:Label>
    </p>
    <p>
        <asp:TextBox ID="txtCitySearchSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnCitySearchSafe" runat="server" Text="Search (Hardened)"
            CssClass="button medium blue" onclick="btnCitySearchSafe_Click" />
        <asp:Label ID="lblCityResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Find by Name and State</h3>
    <p class="tiny">Know both the salesperson's name and their state? Narrow the search down.</p>
    <p>
        Name: <asp:TextBox ID="txtDualName" runat="server"></asp:TextBox>
        State: <asp:TextBox ID="txtDualState" runat="server"></asp:TextBox>
        <asp:Button ID="btnDualSearch" runat="server" Text="Search"
            CssClass="button medium blue" onclick="btnDualSearch_Click" />
        <asp:Label ID="lblDualResult" runat="server"></asp:Label>
    </p>
    <p>
        Name: <asp:TextBox ID="txtDualNameSafe" runat="server"></asp:TextBox>
        State: <asp:TextBox ID="txtDualStateSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnDualSearchSafe" runat="server" Text="Search (Hardened)"
            CssClass="button medium blue" onclick="btnDualSearchSafe_Click" />
        <asp:Label ID="lblDualResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Bulk Search</h3>
    <p class="tiny">Looking for salespeople across several states at once? Enter a comma-separated
    list (e.g. "ca,ny,tx").</p>
    <p>
        <asp:TextBox ID="txtBulkStates" runat="server"></asp:TextBox>
        <asp:Button ID="btnBulkSearch" runat="server" Text="Search"
            CssClass="button medium blue" onclick="btnBulkSearch_Click" />
        <asp:Label ID="lblBulkResult" runat="server"></asp:Label>
    </p>
    <p>
        <asp:TextBox ID="txtBulkStatesSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnBulkSearchSafe" runat="server" Text="Search (Hardened)"
            CssClass="button medium blue" onclick="btnBulkSearchSafe_Click" />
        <asp:Label ID="lblBulkResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Remember My Last Search</h3>
    <p class="tiny">Save a state here, then come back later and re-run it with one click.</p>
    <p>
        <asp:TextBox ID="txtRememberState" runat="server"></asp:TextBox>
        <asp:Button ID="btnSaveSearch" runat="server" Text="Save Search"
            CssClass="button medium blue" onclick="btnSaveSearch_Click" />
        <asp:Label ID="lblRememberStatus" runat="server"></asp:Label>
        <br />
        <asp:Button ID="btnRunRememberedSearch" runat="server" Text="Run My Last Search"
            CssClass="button medium blue" onclick="btnRunRememberedSearch_Click" />
        <asp:Label ID="lblRememberedResult" runat="server"></asp:Label>
    </p>
    <p>
        <asp:TextBox ID="txtRememberStateSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnSaveSearchSafe" runat="server" Text="Save Search (Hardened)"
            CssClass="button medium blue" onclick="btnSaveSearchSafe_Click" />
        <asp:Label ID="lblRememberStatusSafe" runat="server"></asp:Label>
        <br />
        <asp:Button ID="btnRunRememberedSearchSafe" runat="server" Text="Run My Last Search (Hardened)"
            CssClass="button medium blue" onclick="btnRunRememberedSearchSafe_Click" />
        <asp:Label ID="lblRememberedResultSafe" runat="server"></asp:Label>
    </p>
</asp:Content>
