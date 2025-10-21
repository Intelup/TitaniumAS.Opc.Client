using System;

namespace TitaniumAS.Opc.Client.Logging
{
    /// <summary>
    /// Interface para provedor de logging que será implementada pelo CollectorOPC
    /// </summary>
    public interface ILoggingProvider
    {
        void Log(string level, string loggerName, string message);
    }
}
