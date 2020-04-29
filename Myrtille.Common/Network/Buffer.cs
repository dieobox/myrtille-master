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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Myrtille.Network
{
    public class DataBuffer<T>
    {
        public int Size { get; set; }
        public int Delay { get; set; }

        private List<T> _data;
        private object _dataLock;

        public delegate void FlushDelegate(List<T> data);
        public FlushDelegate SendBufferData { get; set; }

        private bool _enabled;
        private CancellationTokenSource _timeout;

        public DataBuffer(int size, int delay)
        {
            Size = size;
            Delay = delay;

            _data = new List<T>();
           _dataLock = new object();
        }

        public void Start()
        {
            if (_enabled)
                return;

            _enabled = true;

            _timeout = new CancellationTokenSource();
            Task.Delay(Delay > 0 ? Delay : 1, _timeout.Token).ContinueWith(task =>
            {
                Flush();
            }, TaskContinuationOptions.NotOnCanceled);
        }

        public void Stop()
        {
            _enabled = false;
        }

        public void AddItem(T item)
        {
            lock (_dataLock)
            {
                _data.Add(item);

                if (_data.Count == 1)
                {
                    if (_timeout != null)
                    {
                        _timeout.Cancel();
                        _timeout = null;
                    }

                    _timeout = new CancellationTokenSource();
                    Task.Delay(Delay > 0 ? Delay : 1, _timeout.Token).ContinueWith(task =>
                    {
                        Flush();
                    }, TaskContinuationOptions.NotOnCanceled);
                }
                else if (_data.Count >= Size)
                {
                    Flush();
                }
            }
        }

        public void Flush()
        {
            if (!_enabled)
                return;

            if (_timeout != null)
            {
                _timeout.Cancel();
                _timeout = null;
            }

            lock (_dataLock)
            {
                if (_data.Count > 0)
                {
                    SendBufferData?.Invoke(_data);
                    _data.Clear();
                }
            }

            _timeout = new CancellationTokenSource();
            Task.Delay(Delay > 0 ? Delay : 1, _timeout.Token).ContinueWith(task =>
            {
                Flush();
            }, TaskContinuationOptions.NotOnCanceled);
        }
    }
}