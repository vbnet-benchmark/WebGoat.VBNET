<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="PwdComplexity.aspx.vb" Inherits="OWASP.WebGoat.NET.PwdComplexity" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h1 class="title-regular-4 clearfix">Password Complexity</h1>
    <p class="tiny">This lesson used to enforce password complexity with client-side JavaScript
    only (easy to bypass by disabling script). These exercises add a server-side backstop --
    but a server-side check can introduce its own problems if the regular expression behind it
    isn't written carefully.</p>

    <hr />
    <h3>Create Account</h3>
    <p class="tiny">Password must contain only letters and digits.</p>
    <p>
        Username: <asp:TextBox ID="txtNewUsername" runat="server"></asp:TextBox>
        Password: <asp:TextBox ID="txtNewPassword" runat="server"></asp:TextBox>
        <asp:Button ID="btnCreateAccount" runat="server" Text="Create Account"
            CssClass="button medium blue" onclick="btnCreateAccount_Click" />
        <asp:Label ID="lblCreateResult" runat="server"></asp:Label>
    </p>
    <p>
        Username: <asp:TextBox ID="txtNewUsernameSafe" runat="server"></asp:TextBox>
        Password: <asp:TextBox ID="txtNewPasswordSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnCreateAccountSafe" runat="server" Text="Create Account (Hardened)"
            CssClass="button medium blue" onclick="btnCreateAccountSafe_Click" />
        <asp:Label ID="lblCreateResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Security Answer Format Hint</h3>
    <p class="tiny">Just a hint while you type your recovery security answer -- it's informational
    only and never stops you from saving whatever you type.</p>
    <p>
        Security answer: <asp:TextBox ID="txtSecurityAnswer" runat="server"></asp:TextBox>
        <asp:Button ID="btnCheckAnswerFormat" runat="server" Text="Check Format"
            CssClass="button medium blue" onclick="btnCheckAnswerFormat_Click" />
        <asp:Label ID="lblAnswerFormatHint" runat="server"></asp:Label>
    </p>
    <p>
        Security answer: <asp:TextBox ID="txtSecurityAnswerSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnCheckAnswerFormatSafe" runat="server" Text="Check Format (Hardened)"
            CssClass="button medium blue" onclick="btnCheckAnswerFormatSafe_Click" />
        <asp:Label ID="lblAnswerFormatHintSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Update Password Policy</h3>
    <p class="tiny">Set a new password under the site's complexity policy (letters, digits and
    underscore only). The policy is enforced by App_Code/PasswordPolicyValidator.vb.</p>
    <p>
        New password: <asp:TextBox ID="txtPolicyPassword" runat="server"></asp:TextBox>
        <asp:Button ID="btnUpdatePolicy" runat="server" Text="Update Password"
            CssClass="button medium blue" onclick="btnUpdatePolicy_Click" />
        <asp:Label ID="lblPolicyResult" runat="server"></asp:Label>
    </p>
    <p>
        New password: <asp:TextBox ID="txtPolicyPasswordSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnUpdatePolicySafe" runat="server" Text="Update Password (Hardened)"
            CssClass="button medium blue" onclick="btnUpdatePolicySafe_Click" />
        <asp:Label ID="lblPolicyResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Preview Field Format (Promo Code)</h3>
    <p class="tiny">Try a sample promo code against the configured "promoCode" format rule before
    rolling it out to customers -- this only previews whether the value would pass; it does not
    save or redeem anything.</p>
    <p>
        Sample promo code: <asp:TextBox ID="txtPromoPreview" runat="server"></asp:TextBox>
        <asp:Button ID="btnPreviewPromo" runat="server" Text="Preview"
            CssClass="button medium blue" onclick="btnPreviewPromo_Click" />
        <asp:Label ID="lblPromoPreviewResult" runat="server"></asp:Label>
    </p>
    <p>
        Sample promo code: <asp:TextBox ID="txtPromoPreviewSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnPreviewPromoSafe" runat="server" Text="Preview (Hardened)"
            CssClass="button medium blue" onclick="btnPreviewPromoSafe_Click" />
        <asp:Label ID="lblPromoPreviewResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Submit Account Recovery Code</h3>
    <p class="tiny">Enter the recovery code we emailed you (format: digits-ABCD, e.g. 48213-QXTR).</p>
    <p>
        Recovery code: <asp:TextBox ID="txtRecoveryCode" runat="server"></asp:TextBox>
        <asp:Button ID="btnSubmitRecoveryCode" runat="server" Text="Submit"
            CssClass="button medium blue" onclick="btnSubmitRecoveryCode_Click" />
        <asp:Label ID="lblRecoveryResult" runat="server"></asp:Label>
    </p>
    <p>
        Recovery code: <asp:TextBox ID="txtRecoveryCodeSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnSubmitRecoveryCodeSafe" runat="server" Text="Submit (Hardened)"
            CssClass="button medium blue" onclick="btnSubmitRecoveryCodeSafe_Click" />
        <asp:Label ID="lblRecoveryResultSafe" runat="server"></asp:Label>
    </p>
</asp:Content>
