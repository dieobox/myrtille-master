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

/*****************************************************************************************************************************************************************************************************/
/*** Convert                                                                                                                                                                                       ***/
/*****************************************************************************************************************************************************************************************************/

// convert a byte array to base64 data
function bytesToBase64(bytes)
{
    return window.btoa(bytesToStr(bytes));
}

// convert a text to a byte array
function strToBytes(str)
{
    var bytes = new ArrayBuffer(str.length);
    var arr = new Uint8Array(bytes);

    for (var i = 0; i < str.length; i++)
    {
        arr[i] = str.charCodeAt(i);
    }

    return bytes;
}

// convert a byte array to a text
// this function only works with ascii characters
// use a text decoder (i.e.: https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder, or the one below if not supported) for UTF-8 buffers
function bytesToStr(bytes)
{
    var str = '';
    var arr = new Uint8Array(bytes);

    for (var i = 0; i < bytes.byteLength; i++)
    {
        str += String.fromCharCode(arr[i]);
    }

    return str;
}

// https://weblog.rogueamoeba.com/2017/02/27/javascript-correctly-converting-a-byte-array-to-a-utf-8-string/
function decodeUtf8(data)
{
    const extraByteMap = [ 1, 1, 1, 1, 2, 2, 3, 0 ];
    var count = data.length;
    var str = "";
    
    for (var index = 0;index < count;)
    {
        var ch = data[index++];
        if (ch & 0x80)
        {
            var extra = extraByteMap[(ch >> 3) & 0x07];
            if (!(ch & 0x40) || !extra || ((index + extra) > count))
                return null;
        
            ch = ch & (0x3F >> extra);
            for (;extra > 0;extra -= 1)
            {
                var chx = data[index++];
                if ((chx & 0xC0) != 0x80)
                return null;
          
                ch = (ch << 6) | (chx & 0x3F);
            }
        }
      
        str += String.fromCharCode(ch);
    }
    
    return str;
}

// convert a text to unicode code points
function strToUnicode(str)
{
    var charsCodes = new Array();

    for (var i = 0; i < str.length; i++)
    {
        var charCode = str.charCodeAt(i);
        charsCodes.push(charCode);
    }

    return charsCodes.join('-');
}