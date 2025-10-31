using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Common.Logging;
using IConnectionPoint = System.Runtime.InteropServices.ComTypes.IConnectionPoint;

namespace TitaniumAS.Opc.Client.Common.Internal
{
    internal class ConnectionPoint<T> : IDisposable where T : class
    {
        private static readonly ILog Log = LogManager.GetLogger<ConnectionPoint<T>>();
        private readonly T _sink;
        private IConnectionPoint _connectionPoint;
        private int? _cookie;

        public ConnectionPoint(T sink)
        {
            _sink = sink;
        }

        public bool IsConnected
        {
            get { return _connectionPoint != null && _cookie != null; }
        }

        public T Sink
        {
            get { return _sink; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Disconnect();
        }

        ~ConnectionPoint()
        {
            Dispose(false);
        }

        public void TryConnect(object comServer)
        {
            try
            {
                Connect(comServer);
            }
            catch (InvalidCastException)
            {
                Log.TraceFormat("Server does not support '{0}' connection point. This is optional and the connection will continue.", typeof(T).Name);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Failed to connect to '{0}' connection point: {1}", typeof(T).Name, ex.Message);
            }
        }

        /// <exception cref="InvalidOperationException">Already attached to IOPCShutdown connection point.</exception>
        public void Connect(object comServer)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already attached to the connection point.");

            IConnectionPointContainer connectionPointContainer;
            try
            {
                connectionPointContainer = (IConnectionPointContainer) comServer;
            }
            catch (InvalidCastException)
            {
                Log.TraceFormat("Server does not support IConnectionPointContainer interface. Connection point '{0}' will not be available.", typeof(T).Name);
                throw;
            }

            var riid = typeof (T).GUID;
            connectionPointContainer.FindConnectionPoint(ref riid, out _connectionPoint);
            int cookie;
            _connectionPoint.Advise(_sink, out cookie);
            _cookie = cookie;
        }

        public void Disconnect()
        {
            try
            {
                if (_connectionPoint == null)
                    return;

                if (_cookie.HasValue)
                {
                    // TODO: Fix: hangs when disposing.
                    _connectionPoint.Unadvise(_cookie.Value);
                    _cookie = null;
                }

                Marshal.ReleaseComObject(_connectionPoint);
                _connectionPoint = null;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to unsubscribe callback.", ex);
            }
        }
    }
}