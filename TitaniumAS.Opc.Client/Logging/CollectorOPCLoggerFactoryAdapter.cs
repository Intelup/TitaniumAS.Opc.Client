using System;
using System.Collections;
using Common.Logging;
using Common.Logging.Configuration;

namespace TitaniumAS.Opc.Client.Logging
{
    /// <summary>
    /// Factory adapter customizada para redirecionar logs do Common.Logging para o CollectorOPC
    /// </summary>
    public class CollectorOPCLoggerFactoryAdapter : ILoggerFactoryAdapter
    {
        public ILog GetLogger(Type type)
        {
            return new CollectorOPCLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new CollectorOPCLogger(name);
        }
    }
}
