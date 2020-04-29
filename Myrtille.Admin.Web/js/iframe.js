/*
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
*/

function switchIframe(iframeId)
{
	try
    {
		//alert('leaving iframe: ' + iframeId);

        var iframe = document.getElementById(iframeId);
        var info = iframe.id.split('_');

        var nextIframeId = info[0] + '_' + (parseInt(info[1]) + 1);
		//alert('next iframe: ' + nextIframeId);
				
		var nextIframe = document.getElementById(nextIframeId);
		if (nextIframe == null)
		{
			//alert('last iframe is reached, cycling to first');
            nextIframe = document.getElementById(info[0] + '_1');
		}
		if (nextIframe != null)
		{
			//alert('active iframe: ' + nextIframe.id);
            nextIframe.contentWindow.setKeyCombination();
            nextIframe.contentWindow.focus();
		}
	}
	catch (exc)
	{
		alert('switchIframe error: ' + exc.message);
	}
}

var overlayTimeout = new Array();

function hideIframeContent(iframeId, width, height)
{
    try
    {
        //alert('hiding iframe: ' + iframeId);

        var iframe = document.getElementById(iframeId);
        var info = iframe.id.split('_');

        var timeout = overlayTimeout[parseInt(info[1])];
        if (timeout != null)
        {
            window.clearTimeout(timeout);
            overlayTimeout[parseInt(info[1])] = null;
        }

        // duplicate the iframe content and draw it over (scaled + color border width)
        var overlay = document.getElementById(iframeId + '_overlay');
        overlay.width = width + 3;
        overlay.height = height + 3;
        var context = overlay.getContext('2d');
        var canvas = iframe.contentWindow.getMyrtille().getDisplay().getCanvas().getCanvasObject();
        context.drawImage(canvas, 0, 0, width + 3, height + 3);

        // remove it after a small delay (time for reconnection)
        overlayTimeout[parseInt(info[1])] = window.setTimeout(function()
        {
            overlay.width = 0;
            overlay.height = 0;
            overlayTimeout[parseInt(info[1])] = null;
            //alert('iframe ' + iframeId + ' is now visible');
        },
        5000);
    }
    catch (exc)
    {
        alert('hideIframeContent error: ' + exc.message);
    }
}

function checkIframeFocus()
{
    try
    {
        var iframes = document.getElementsByTagName('iframe');
        for (var i = 0; i < iframes.length; i++)
        {
            var iframe = document.getElementById(iframes[i].id);
            if (iframe.id != document.activeElement.id)
            {
                iframe.className = 'iframeNoBorder';
            }
            else
            {
                iframe.className = 'iframeColorBorder';
            }
        }
    }
	catch (exc)
	{
        alert('checkIframeFocus error: ' + exc.message);
	}
}

function updateIframeUI(iframeId, connected)
{
    // iframe cookies are set/cleared on session connect/disconnect
    switch (iframeId)
    {
        case 'myrtille_1':
            document.getElementById('AddGuestButton').disabled = !connected;
            document.getElementById('GetGuestsButton').disabled = !connected;
            document.getElementById('GetGuestButton').disabled = !connected;
            document.getElementById('UpdateGuestButton').disabled = !connected;
            document.getElementById('RemoveGuestButton').disabled = !connected;

            document.getElementById('SetScreenshotConfigButton').disabled = !connected;
            document.getElementById('StartTakingScreenshotsButton').disabled = !connected;
            document.getElementById('StopTakingScreenshotsButton').disabled = !connected;
            document.getElementById('TakeScreenshotButton').disabled = !connected;

            document.getElementById('myrtille_1_disconnect').disabled = !connected;
            document.getElementById('Logout').disabled = !connected && document.getElementById('myrtille_2_disconnect').disabled;
            break;

        case 'myrtille_2':
            document.getElementById('myrtille_2_disconnect').disabled = !connected;
            document.getElementById('Logout').disabled = !connected && document.getElementById('myrtille_1_disconnect').disabled;
            break;
    }
}

// http://www.quirksmode.org/js/cookies.html
function setCookie(name,value,days) {
	var expires = "";
	if (days) {
		var date = new Date();
		date.setTime(date.getTime() + (days*24*60*60*1000));
		expires = "; expires=" + date.toUTCString();
	}
    document.cookie = name + "=" + (value || "") + expires + "; path=/";

    // connected
    if (name == 'myrtille_1' || name == 'myrtille_2')
    {
        updateIframeUI(name, true);
    }
}
function getCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for(var i=0;i < ca.length;i++) {
		var c = ca[i];
		while (c.charAt(0)==' ') c = c.substring(1,c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
	}
	return null;
}
function eraseCookie(name) {   
    document.cookie = name + '=; Max-Age=-99999999; path=/';

    // disconnected
    if (name == 'myrtille_1' || name == 'myrtille_2')
    {
        updateIframeUI(name, false);
    }
}