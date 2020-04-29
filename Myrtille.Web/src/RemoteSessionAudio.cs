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

namespace Myrtille.Web
{
    // TODO: other formats support? (AAC, WMA, OGG, etc.)
    // actually, WAV (lossless) and MP3 (lossy) should be enough for most purpose (Myrtille's objective is not to be an audio player)
    public enum AudioFormat
    {
        NONE = 0,   // audio disabled
        WAV = 1,    // uncompressed PCM 44100 Hz, 16 bits stereo
        MP3 = 2     // compressed MPEG 3 (default)
    }

    public class RemoteSessionAudio
    {
        public int Idx;
        public AudioFormat Format;
        public int Bitrate;
        public byte[] Data;
    }
}