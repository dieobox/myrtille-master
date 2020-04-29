<%--
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2020 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

	    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
--%>

<%@ Page Language="C#" Inherits="Myrtille.Web.EnterpriseChangePassword" Codebehind="EnterpriseChangePassword.aspx.cs" AutoEventWireup="true" Culture="auto" UICulture="auto" %>
<%@ OutputCache Location="None" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
	
    <head>
        <title>Myrtille</title>
        <link rel="stylesheet" type="text/css" href="../css/Default.css"/>
	</head>

    <body onload="onChangeSuccess();">
        
        <form method="post" runat="server">
            
            <div id="changePasswordPopupInner">
                <span id="changePasswordPopupTitle">
                    <strong>Change Password</strong>
                </span>
                <br/>
                <p class="changePasswordMessage">Your password has expired and must be changed</p>
               
                <div class="changePasswordPopupInput">
                    <h5><label id="userNameLabel" for="hostName">User name</label></h5>
                    <input type="text" runat="server" id="userName" title="user name" readonly="readonly" />
                </div>
                <div class="changePasswordPopupInput">
                    <h5><label id="oldPasswordLabel" for="oldPassword">Old Password</label></h5>
                    <input type="password" runat="server" id="oldPassword" title="Old Password" />
                </div>
                <div class="changePasswordPopupInput">
                    <h5><label id="newPasswordLabel" for="newPassword">New Password</label></h5>
                    <input type="password" runat="server" id="newPassword" title="new password"/>
                </div>
                <div class="changePasswordPopupInput">
                    <h5><label id="confirmPasswordLabel" for="confirmPassword">Confirm Password</label></h5>
                    <input type="password" runat="server" id="confirmPassword" title="confirm password"/>
                </div>
                <label runat="server" id="changeError" class="changeError"></label>
                <br/>
                <br />
                <div class="changePasswordPopupInput">
                    <input type="submit" runat="server" id="changePassword" value="Change Password" onserverclick="ChangePasswordButtonClick"/>
                    <input type="button" id="closePopupButton" value="Close" onclick="parent.closePopup();"/>
                </div>
            </div>

        </form>

		<script type="text/javascript" language="javascript" defer="defer">

            // change password success
            function onChangeSuccess()
            {
                var idx = window.location.search.indexOf('change=success');
                if (idx != -1)
                {
                    parent.location.href = parent.location.href;
                }
            }

		</script>

	</body>

</html>