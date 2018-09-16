<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Vote.aspx.cs" Inherits="WebApplication1.Mobile.VotePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>MONOL EVALUATE SYSTEM</title>
    <link href="../Content/Site.css" rel="stylesheet" />
    <script src="../Scripts/jquery-3.3.1.min.js"></script>
    <script>
        $(function () {
            $('form').submit(function () {
                var names = [];

                $('input[type="radio"]').each(function() {
                    // Creates an array with the names of all the different checkbox group.
                    names[$(this).attr('name')] = true;
                });

                // Goes through all the names and make sure there's at least one checked.
                for (name in names) {
                    var radio_buttons = $("input[name='" + name + "']");
                    if (radio_buttons.filter(':checked').length == 0) {
                        alert('none checked in ' + name);
                        return false;
                    } 
                    else {
                        // If you need to use the result you can do so without
                        // another (costly) jQuery selector call:
                        var val = radio_buttons.val();
                    }
                }
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Panel ID="plContainer" runat="server">

            </asp:Panel>
            <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="BtnSubmit_Click" />

            <asp:Label ID="lbResult" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>
