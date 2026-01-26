using System;
using Common.Logging;

namespace TitaniumAS.Opc.Client.Logging
{
    /// <summary>
    /// Logger customizado que redireciona logs do TitaniumAS para o sistema de logging do CollectorOPC
    /// </summary>
    public class CollectorOPCLogger : ILog
    {
        private readonly string _loggerName;
        private static ILoggingProvider _loggingProvider;

        public CollectorOPCLogger(string loggerName)
        {
            _loggerName = loggerName;
        }

        /// <summary>
        /// Configura o provedor de logging do CollectorOPC
        /// </summary>
        /// <param name="provider">Provedor de logging que implementa ILoggingProvider</param>
        public static void SetLoggingProvider(ILoggingProvider provider)
        {
            _loggingProvider = provider;
        }

        public bool IsTraceEnabled => true;
        public bool IsDebugEnabled => true;
        public bool IsInfoEnabled => true;
        public bool IsWarnEnabled => true;
        public bool IsErrorEnabled => true;
        public bool IsFatalEnabled => true;

        public IVariablesContext GlobalVariablesContext => new EmptyVariablesContext();
        public IVariablesContext ThreadVariablesContext => new EmptyVariablesContext();

        public INestedVariablesContext NestedThreadVariablesContext => throw new NotImplementedException();

        public void Trace(object message)
        {
            LogMessage("TRACE", message?.ToString());
        }

        public void Trace(object message, Exception exception)
        {
            LogMessage("TRACE", $"{message} - {exception}");
        }

        public void TraceFormat(string format, params object[] args)
        {
            LogMessage("TRACE", string.Format(format, args));
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("TRACE", string.Format(formatProvider, format, args));
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("TRACE", $"{string.Format(format, args)} - {exception}");
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("TRACE", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        public void Debug(object message)
        {
            LogMessage("DEBUG", message?.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            LogMessage("DEBUG", $"{message} - {exception}");
        }

        public void DebugFormat(string format, params object[] args)
        {
            LogMessage("DEBUG", string.Format(format, args));
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("DEBUG", string.Format(formatProvider, format, args));
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("DEBUG", $"{string.Format(format, args)} - {exception}");
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("DEBUG", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        public void Info(object message)
        {
            LogMessage("INFO", message?.ToString());
        }

        public void Info(object message, Exception exception)
        {
            LogMessage("INFO", $"{message} - {exception}");
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogMessage("INFO", string.Format(format, args));
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("INFO", string.Format(formatProvider, format, args));
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("INFO", $"{string.Format(format, args)} - {exception}");
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("INFO", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        public void Warn(object message)
        {
            LogMessage("WARN", message?.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            LogMessage("WARN", $"{message} - {exception}");
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogMessage("WARN", string.Format(format, args));
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("WARN", string.Format(formatProvider, format, args));
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("WARN", $"{string.Format(format, args)} - {exception}");
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("WARN", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        public void Error(object message)
        {
            LogMessage("ERROR", message?.ToString());
        }

        public void Error(object message, Exception exception)
        {
            LogMessage("ERROR", $"{message} - {exception}");
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogMessage("ERROR", string.Format(format, args));
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("ERROR", string.Format(formatProvider, format, args));
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("ERROR", $"{string.Format(format, args)} - {exception}");
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("ERROR", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        public void Fatal(object message)
        {
            LogMessage("FATAL", message?.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            LogMessage("FATAL", $"{message} - {exception}");
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogMessage("FATAL", string.Format(format, args));
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            LogMessage("FATAL", string.Format(formatProvider, format, args));
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
            LogMessage("FATAL", $"{string.Format(format, args)} - {exception}");
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            LogMessage("FATAL", $"{string.Format(formatProvider, format, args)} - {exception}");
        }

        // Métodos com Action<FormatMessageHandler>
        public void Trace(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("TRACE", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("TRACE", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("TRACE", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("TRACE", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("DEBUG", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("DEBUG", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("DEBUG", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("DEBUG", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("INFO", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("INFO", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("INFO", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("INFO", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("WARN", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("WARN", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("WARN", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("WARN", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("ERROR", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("ERROR", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("ERROR", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("ERROR", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("FATAL", string.Format(format, args)); return string.Empty; });
            }
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("FATAL", $"{string.Format(format, args)} - {exception}"); return string.Empty; });
            }
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("FATAL", string.Format(formatProvider, format, args)); return string.Empty; });
            }
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if (formatMessageCallback != null)
            {
                formatMessageCallback((format, args) => { LogMessage("FATAL", $"{string.Format(formatProvider, format, args)} - {exception}"); return string.Empty; });
            }
        }

        private void LogMessage(string level, string message)
        {
            if (_loggingProvider != null && !string.IsNullOrEmpty(message))
            {
                _loggingProvider.Log(level, _loggerName, message);
            }
        }
    }

    /// <summary>
    /// Implementação vazia de IVariablesContext
    /// </summary>
    public class EmptyVariablesContext : IVariablesContext
    {
        public object this[string key]
        {
            get => null;
            set { }
        }

        public void Set(string key, object value) { }
        public object Get(string key) => null;
        public bool Contains(string key) => false;
        public void Remove(string key) { }
        public void Clear() { }
    }
}
