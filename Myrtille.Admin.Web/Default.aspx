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

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Myrtille.Admin.Web.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    <link rel="stylesheet" type="text/css" href="css/iframe.css" />
    <script language="javascript" type="text/javascript" src="js/iframe.js"></script>
<body>

</body>

    <form method="post" runat="server" id="mainForm">

        <div align="left">
            <input type="hidden" runat="server" id="_guestId" />
            <input type="hidden" runat="server" id="_allowControl" />
            <input type="button" runat="server" id="AddGuestButton" value="Add a guest" data-fid="myrtille_1" onclick="_allowControl.value = prompt('Grant control to guest?', 'true');" onserverclick="AddGuestButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="GetGuestsButton" value="Show guests list" data-fid="myrtille_1" onserverclick="GetGuestsButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="GetGuestButton" value="Show guest info" data-fid="myrtille_1" onclick="_guestId.value = prompt('Enter guest id', 'guid');" onserverclick="GetGuestButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="UpdateGuestButton" value="Update a guest" data-fid="myrtille_1" onclick="_guestId.value = prompt('Enter guest id', 'guid'); if (_guestId.value) { _allowControl.value = prompt('Grant control to guest?', 'true'); }" onserverclick="UpdateGuestButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="RemoveGuestButton" value="Remove a guest" data-fid="myrtille_1" onclick="_guestId.value = prompt('Enter guest id', 'guid');" onserverclick="RemoveGuestButtonClick" disabled="disabled"/><br />
        </div>
        
        <br />

	    <div align="left">
            <div class="iframeOverlayOuter">
                <canvas id="myrtille_1_overlay" class="iframeOverlayInner" width="0" height="0"></canvas>
                <!-- relative iframe size: session is reconnected on browser resize (adjusting to screen) -->
                <iframe runat="server" id="myrtille_1" name="myrtille_1"></iframe><br />
            </div>
            <span>^ relative iframe size (resized on browser resize)</span><br />
            <input type="button" runat="server" id="SetScreenshotConfigButton" value="screenshot config (click first)" data-fid="myrtille_1" onserverclick="SetScreenshotConfigButtonClick" disabled="disabled"/><br />&nbsp;Screenshots will be saved into C:\path\to\screenshots on this machine<br />
            <input type="button" runat="server" id="StartTakingScreenshotsButton" value="start taking screenshots" data-fid="myrtille_1" onserverclick="StartTakingScreenshotsButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="StopTakingScreenshotsButton" value="stop taking screenshots" data-fid="myrtille_1" onserverclick="StopTakingScreenshotsButtonClick" disabled="disabled"/><br />
            <input type="button" runat="server" id="TakeScreenshotButton" value="take screenshot" data-fid="myrtille_1" onserverclick="TakeScreenshotButtonClick" disabled="disabled"/>
        </div>

	    <div align="left">
            <input type="button" runat="server" id="myrtille_1_disconnect" value="Disconnect" data-fid="myrtille_1" onserverclick="DisconnectButtonClick" disabled="disabled"/>
        </div>

	    <hr/>
	
        <div align="left">
            <!-- fixed iframe size (no need for an overlay) -->
            <iframe runat="server" id="myrtille_2" name="myrtille_2"></iframe><br />
            <span>^ fixed iframe size</span>
        </div>

	    <div align="left">
            <input type="button" runat="server" id="myrtille_2_disconnect" value="Disconnect" data-fid="myrtille_2" onserverclick="DisconnectButtonClick" disabled="disabled"/>
        </div>

	    <hr/>

	    <div align="left">
            <input type="button" runat="server" id="Logout" value="Logout" onserverclick="LogoutButtonClick" disabled="disabled"/>
        </div>
	
    </form>

	<script type="text/javascript" language="javascript" defer="defer">

        // TODO: there is probably a better way to handle the focused iframe than having to poll the page iframes,
        // but, sadly, the iframe element doesn't play nice with events (...)
        window.setInterval(function() { checkIframeFocus(); }, 1000);

	</script>

</html>