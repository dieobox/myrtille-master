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

using System.Collections;

namespace Myrtille.Web
{
    public class RdpScanCode
    {
        public int Value { get; private set; }
        public bool Extend { get; private set; }

        /// <summary>
        /// rdp scancode
        /// </summary>
        /// <remarks>
        /// rdp scancodes are directly mapped to a physical keyboard layout
        /// </remarks>
        /// <param name="value">decimal value</param>
        /// <param name="extend">non alphanumeric characters must be extended</param>
        public RdpScanCode(int value, bool extend)
        {
            Value = value;
            Extend = extend;
        }
    }

    public static class JsKeyCodeToRdpScanCodeMapping
    {
        public static Hashtable MapTable { get; private set; }

        static JsKeyCodeToRdpScanCodeMapping()
        {
            // this mapping is not complete (latin characters only); fill it as needed (below values are those for qwerty (US) and azerty (FR) layouts)
            // keys (javascript keycodes) and values (rdp scancodes) are decimals (scancodes are often expressed in hexadecimal). scancode value 0 means there is no available translation (at least for now)
            // see http://protocolsofmatrix.blogspot.com/2007/09/javascript-keycode-reference-table-for.html or http://www.webonweboff.com/tips/js/event_key_codes.aspx for js keycodes
            // see http://www.vmware.com/support/ws55/doc/ws_devices_keymap_vscan.html or https://github.com/neutrinolabs/xrdp/blob/devel/xrdp/rdp-scan-codes.txt for rdp scancodes

            // the left and right location distinction for key modifiers is not currently done
            // while this is fine for most applications, some of them may need this distinction
            // also, some keys are a combination of key modifiers (i.e.: alt+gr = left ctrl + right alt)

            // TODO: make that distinction, by passing the key location from javascript (in addition to the key code),
            // then use the appropriate rdp scancodes (currently disabled below)

            MapTable = new Hashtable();

            MapTable.Add(8, new RdpScanCode(14, false));            // backspace
            MapTable.Add(9, new RdpScanCode(15, false));            // tab
            MapTable.Add(13, new RdpScanCode(28, false));           // enter
            MapTable.Add(16, new RdpScanCode(42, false));           // shift (left)
            //MapTable.Add(16, new RdpScanCode(54, false));         // shift (right)
            MapTable.Add(17, new RdpScanCode(29, false));           // ctrl (left)
            //MapTable.Add(17, new RdpScanCode(29, true));          // ctrl (right)
            //MapTable.Add(18, new RdpScanCode(56, false));         // alt (left) * disabled to prevent interfering with the browser alt+key menu *
            MapTable.Add(18, new RdpScanCode(56, true));            // alt (right)
            MapTable.Add(19, new RdpScanCode(256, false));          // pause/break
            MapTable.Add(20, new RdpScanCode(58, false));           // caps lock
            MapTable.Add(27, new RdpScanCode(1, false));            // escape
            MapTable.Add(32, new RdpScanCode(57, false));           // space
            MapTable.Add(33, new RdpScanCode(73, true));            // page up
            MapTable.Add(34, new RdpScanCode(81, true));            // page down
            MapTable.Add(35, new RdpScanCode(79, true));            // end
            MapTable.Add(36, new RdpScanCode(71, true));            // home
            MapTable.Add(37, new RdpScanCode(75, true));            // left arrow
            MapTable.Add(38, new RdpScanCode(72, true));            // up arrow
            MapTable.Add(39, new RdpScanCode(77, true));            // right arrow
            MapTable.Add(40, new RdpScanCode(80, true));            // down arrow
            MapTable.Add(45, new RdpScanCode(82, true));            // insert
            MapTable.Add(46, new RdpScanCode(83, true));            // delete
            MapTable.Add(48, new RdpScanCode(11, false));           // 0
            MapTable.Add(49, new RdpScanCode(2, false));            // 1
            MapTable.Add(50, new RdpScanCode(3, false));            // 2
            MapTable.Add(51, new RdpScanCode(4, false));            // 3
            MapTable.Add(52, new RdpScanCode(5, false));            // 4
            MapTable.Add(53, new RdpScanCode(6, false));            // 5
            MapTable.Add(54, new RdpScanCode(7, false));            // 6
            MapTable.Add(55, new RdpScanCode(8, false));            // 7
            MapTable.Add(56, new RdpScanCode(9, false));            // 8
            MapTable.Add(57, new RdpScanCode(10, false));           // 9
            MapTable.Add(65, new RdpScanCode(30, false));           // a qwerty
            //MapTable.Add(65, new RdpScanCode(16, false));         // a azerty
            MapTable.Add(66, new RdpScanCode(48, false));           // b
            MapTable.Add(67, new RdpScanCode(46, false));           // c
            MapTable.Add(68, new RdpScanCode(32, false));           // d
            MapTable.Add(69, new RdpScanCode(18, false));           // e
            MapTable.Add(70, new RdpScanCode(33, false));           // f
            MapTable.Add(71, new RdpScanCode(34, false));           // g
            MapTable.Add(72, new RdpScanCode(35, false));           // h
            MapTable.Add(73, new RdpScanCode(23, false));           // i
            MapTable.Add(74, new RdpScanCode(36, false));           // j
            MapTable.Add(75, new RdpScanCode(37, false));           // k
            MapTable.Add(76, new RdpScanCode(38, false));           // l
            MapTable.Add(77, new RdpScanCode(50, false));           // m qwerty
            //MapTable.Add(77, new RdpScanCode(39, false));         // m azerty
            MapTable.Add(78, new RdpScanCode(49, false));           // n
            MapTable.Add(79, new RdpScanCode(24, false));           // o
            MapTable.Add(80, new RdpScanCode(25, false));           // p
            MapTable.Add(81, new RdpScanCode(16, false));           // q qwerty
            //MapTable.Add(81, new RdpScanCode(30, false));         // q azerty
            MapTable.Add(82, new RdpScanCode(19, false));           // r
            MapTable.Add(83, new RdpScanCode(31, false));           // s
            MapTable.Add(84, new RdpScanCode(20, false));           // t
            MapTable.Add(85, new RdpScanCode(22, false));           // u
            MapTable.Add(86, new RdpScanCode(47, false));           // v
            MapTable.Add(87, new RdpScanCode(17, false));           // w qwerty
            //MapTable.Add(87, new RdpScanCode(44, false));         // w azerty
            MapTable.Add(88, new RdpScanCode(45, false));           // x
            MapTable.Add(89, new RdpScanCode(21, false));           // y
            MapTable.Add(90, new RdpScanCode(44, false));           // z qwerty
            //MapTable.Add(90, new RdpScanCode(17, false));         // z azerty
            MapTable.Add(91, new RdpScanCode(347, true));           // left window key
            MapTable.Add(92, new RdpScanCode(348, true));           // right window key
            MapTable.Add(93, new RdpScanCode(0, true));             // select key
            MapTable.Add(96, new RdpScanCode(82, false));           // numpad 0
            MapTable.Add(97, new RdpScanCode(79, false));           // numpad 1
            MapTable.Add(98, new RdpScanCode(80, false));           // numpad 2
            MapTable.Add(99, new RdpScanCode(81, false));           // numpad 3
            MapTable.Add(100, new RdpScanCode(75, false));          // numpad 4
            MapTable.Add(101, new RdpScanCode(76, false));          // numpad 5
            MapTable.Add(102, new RdpScanCode(77, false));          // numpad 6
            MapTable.Add(103, new RdpScanCode(71, false));          // numpad 7
            MapTable.Add(104, new RdpScanCode(72, false));          // numpad 8
            MapTable.Add(105, new RdpScanCode(73, false));          // numpad 9
            MapTable.Add(106, new RdpScanCode(55, false));          // multiply
            MapTable.Add(107, new RdpScanCode(78, false));          // add
            MapTable.Add(109, new RdpScanCode(74, false));          // subtract
            MapTable.Add(110, new RdpScanCode(52, false));          // decimal point
            MapTable.Add(111, new RdpScanCode(53, true));           // divide
            MapTable.Add(112, new RdpScanCode(59, false));          // f1
            MapTable.Add(113, new RdpScanCode(60, false));          // f2
            MapTable.Add(114, new RdpScanCode(61, false));          // f3
            MapTable.Add(115, new RdpScanCode(62, false));          // f4
            MapTable.Add(116, new RdpScanCode(63, false));          // f5
            MapTable.Add(117, new RdpScanCode(64, false));          // f6
            MapTable.Add(118, new RdpScanCode(65, false));          // f7
            MapTable.Add(119, new RdpScanCode(66, false));          // f8
            MapTable.Add(120, new RdpScanCode(67, false));          // f9
            MapTable.Add(121, new RdpScanCode(68, false));          // f10
            MapTable.Add(122, new RdpScanCode(87, false));          // f11
            MapTable.Add(123, new RdpScanCode(88, false));          // f12
            MapTable.Add(144, new RdpScanCode(69, false));          // num lock
            MapTable.Add(145, new RdpScanCode(70, false));          // scroll lock
            MapTable.Add(186, new RdpScanCode(39, false));          // semi-colon qwerty
            //MapTable.Add(186, new RdpScanCode(50, false));        // semi-colon azerty
            MapTable.Add(187, new RdpScanCode(13, false));          // equal sign
            MapTable.Add(188, new RdpScanCode(51, false));          // comma
            MapTable.Add(189, new RdpScanCode(12, false));          // dash
            MapTable.Add(190, new RdpScanCode(52, false));          // period
            MapTable.Add(191, new RdpScanCode(53, false));          // forward slash
            MapTable.Add(192, new RdpScanCode(41, false));          // grave accent qwerty
            //MapTable.Add(192, new RdpScanCode(40, false));        // grave accent azerty
            MapTable.Add(219, new RdpScanCode(26, false));          // open bracket
            MapTable.Add(220, new RdpScanCode(43, false));          // back slash
            MapTable.Add(221, new RdpScanCode(27, false));          // close braket
            MapTable.Add(222, new RdpScanCode(40, false));          // single quote qwerty
            //MapTable.Add(222, new RdpScanCode(41, false));        // single quote azerty
            MapTable.Add(223, new RdpScanCode(0, false));           // ?
            MapTable.Add(226, new RdpScanCode(86, false));          // lesser than
        }
    }
}