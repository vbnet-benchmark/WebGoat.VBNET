Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.Security.Cryptography
Imports System.Text
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class ProductDetails
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            lblMessage.Visible = False
            txtEmail.Enabled = True
            If Not Page.IsPostBack Then
                LoadComments()
            End If

            'TODO: broken 
            If Not Page.IsPostBack Then
                Dim ds As DataSet = du.GetCatalogData()
                ddlItems.DataSource = ds.Tables(0)
                ddlItems.DataTextField = "productName"
                ddlItems.DataValueField = "productCode"
                ddlItems.DataBind()
            End If
        End Sub

        Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
            Try
                Dim error_message As String = du.AddComment(hiddenFieldProductID.Value, txtEmail.Text, txtComment.Text)
                txtComment.Text = error_message
                lblMessage.Visible = True
                LoadComments()

                ' "Verified Purchaser" badge attached to the posted comment. A reviewer can prove
                ' they're a genuine purchaser one of three ways -- an order number, their account
                ' email, or a guest receipt code -- and each method folds a different set of
                ' fields into the badge's approval signature, so a moderator reviewing a kept
                ' badge later can tell it still matches the method it claims to have used.
                Dim verifyMethod As String = Request.QueryString("verifyMethod")
                If Not String.IsNullOrEmpty(verifyMethod) Then
                    Dim badgeInput As String
                    Select Case verifyMethod.ToLowerInvariant()
                        Case "order"
                            badgeInput = "order:" & Request.QueryString("orderNumber") & "|" & txtEmail.Text
                        Case "account"
                            badgeInput = "account:" & txtEmail.Text & "|" & hiddenFieldProductID.Value
                        Case Else ' "guest"
                            badgeInput = "guest:" & Request.QueryString("receiptCode") & "|" & hiddenFieldProductID.Value
                    End Select

                    Using md5 As MD5 = MD5.Create()
                        Dim badgeBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(badgeInput)) ' SINK: PLANTED-VB-HR-414
                        lblMessage.Text &= "<div class='badge'>Verified Purchaser badge: " & BitConverter.ToString(badgeBytes).Replace("-", "").ToLowerInvariant() & "</div>"
                    End Using
                End If

                ' Hardened twin: the same three verification methods, but the badge is salted
                ' with a fresh random value per post and hashed with SHA-256.
                Dim verifyMethodSafe As String = Request.QueryString("verifyMethodSafe")
                If Not String.IsNullOrEmpty(verifyMethodSafe) Then
                    Dim badgeInputSafe As String
                    Select Case verifyMethodSafe.ToLowerInvariant()
                        Case "order"
                            badgeInputSafe = "order:" & Request.QueryString("orderNumberSafe") & "|" & txtEmail.Text
                        Case "account"
                            badgeInputSafe = "account:" & txtEmail.Text & "|" & hiddenFieldProductID.Value
                        Case Else ' "guest"
                            badgeInputSafe = "guest:" & Request.QueryString("receiptCodeSafe") & "|" & hiddenFieldProductID.Value
                    End Select

                    Dim saltBytes(15) As Byte
                    Using rng As New RNGCryptoServiceProvider()
                        rng.GetBytes(saltBytes)
                    End Using
                    Dim saltHex As String = BitConverter.ToString(saltBytes).Replace("-", "").ToLowerInvariant()

                    Using sha256 As SHA256 = SHA256.Create()
                        Dim badgeBytesSafe = sha256.ComputeHash(Encoding.UTF8.GetBytes(badgeInputSafe & "|" & saltHex)) ' SAFE_SINK: PLANTED-VB-HR-414-safe
                        lblMessage.Text &= "<div class='badge-safe'>Verified Purchaser badge: " & saltHex & ":" & BitConverter.ToString(badgeBytesSafe).Replace("-", "").ToLowerInvariant() & "</div>"
                    End Using
                End If
            Catch ex As Exception
                lblMessage.Text = ex.Message
                lblMessage.Visible = True
            End Try
        End Sub

        Private Sub LoadComments()
            Dim id As String = Request("productNumber")
            If id Is Nothing Then id = "S18_2795" 'this month's special    
            Dim ds As DataSet = du.GetProductDetails(id)
            Dim output As String = String.Empty
            Dim comments As String = String.Empty
            For Each prodRow As DataRow In ds.Tables("products").Rows
                output += "<div class='product2' align='center'>"
                output += "<img src='./images/products/" & prodRow("productImage") & "'/><br/>"
                output += "<strong>" & prodRow("productName").ToString() & "</strong><br/>"
                output += "<hr/>" & prodRow("productDescription").ToString() & "<br/>"
                output += "</div>"

                hiddenFieldProductID.Value = prodRow("productCode").ToString()

                Dim childrows As DataRow() = prodRow.GetChildRows("prod_comments")
                If childrows.Length > 0 Then
                    comments += "<h2 class='title-regular-2'>Comments:</h2>"
                End If

                For Each commentRow As DataRow In childrows
                    comments += "<strong>Email:</strong>" & commentRow("email") & "<span style='font-size: x-small;color: #E47911;'> (Email Address Verified!) </span><br/>"
                    comments += "<strong>Comment:</strong><br/>" & commentRow("comment") & "<br/><hr/>"
                Next

            Next

            lblOutput.Text = output
            lblComments.Text = comments

            'Fill in the email address of authenticated users
            If Request.Cookies("customerNumber") IsNot Nothing Then
                Dim customerNumber As String = Request.Cookies("customerNumber").Value

                Dim email As String = du.GetCustomerEmail(customerNumber)
                txtEmail.Text = email
                txtEmail.ReadOnly = True
            End If
        End Sub

        Protected Sub ddlItems_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlItems.SelectedIndexChanged
            Response.Redirect("ProductDetails.aspx?productNumber=" & ddlItems.SelectedItem.Value)
        End Sub

        Protected Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Button1.Click
            Response.Redirect("ProductDetails.aspx?productNumber=" & ddlItems.SelectedItem.Value)
        End Sub

    End Class
End Namespace