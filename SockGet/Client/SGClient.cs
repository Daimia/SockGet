﻿using SockGet.Serialization;
using SockGet.Core;
using SockGet.Core.Enums;
using SockGet.Data;
using SockGet.Events;
using SockGet.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SockGet.Core.Extensions;
using SockGet.Exceptions;

namespace SockGet.Client
{
    public class SgClient : SgClient<object>
    {

    }

    public class SgClient<T> : SgSocket<T>
    {
        public int Port { get; protected set; }
        public string Address { get; protected set; }
        public SgClient()
        {

        }

        public bool Check(string address, int port)
        {
            //var ipAddress = IPAddress.Parse(address);
            //Address = address;
            //Port = port;

            //socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            CloseReason = null;

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //socket.Connect(localEndPoint);
                socket.Connect(address, port);
                socket.Close();
                socket = null;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CheckAsync(string address, int port, int timeout)
        {
            var t = Task.Run(() => Check(address, port));
            var finsihed = await Task.WhenAny(t, Task.Delay(timeout));
            if (finsihed == t)
            {
                if (t.IsFaulted)
                    throw t.Exception.InnerException ?? t.Exception;
                return t.Result;
            }
            else
            {
                Close();
                throw new ConnectionTimeoutException("Connection timed out.");
            }
        }
        public bool Connect(int port)
        {
            return Connect("127.0.0.1", port);
        }
        public bool Connect(string address, int port)
        {
            Address = address;
            Port = port;

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(address, port);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Listen();
            return Authenticate();
        }
        public bool Reconnect()
        {
            return Connect(Address, Port);
        }
        public async Task<bool> ReconnectAsync(int timeout)
        {
            return await ConnectAsync(Address, Port, timeout);
        }
        public async Task<bool> ConnectAsync(int port, int timeout = 2000)
        {
            return await ConnectAsync("127.0.0.1", port, timeout);
        }
        public async Task<bool> ConnectAsync(string address, int port, int timeout = 2000)
        {
            if (timeout == 0)
                return await Task.Run(() => Connect(address, port));

            var t = Task.Run(() => Connect(address, port));
            var finished = await Task.WhenAny(t, Task.Delay(timeout));
            if (finished == t)
            {
                if (t.IsFaulted)
                    throw t.Exception.InnerException ?? t.Exception;

                return t.Result;
            }
            else
            {
                socket?.Close();
                throw new ConnectionTimeoutException("Connection timed out.");
            }
        }

        internal SgClient(Socket socket)
        {
            this.socket = socket;
            IsAuthorised = true;

            Address = socket.RemoteEndPoint.ToString();
        }
    }
}
