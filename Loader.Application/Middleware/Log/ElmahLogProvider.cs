using Hangfire.Logging;
using Loader.Service.Services.Analytics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Loader.Application.Middleware.Log
{
    public class ElmahLog : ILog
    {
        public string Name { get; set; }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            if (messageFunc == null)
            {
                // Before calling a method with an actual message, LogLib first probes
                // whether the corresponding log level is enabled by passing a `null`
                // messageFunc instance.
                return logLevel > LogLevel.Info;
            }

            // Writing a message somewhere, make sure you also include the exception parameter,
            // because it usually contain valuable information, but it can be `null` for regular
            // messages.
            //Console.WriteLine(String.Format("{0}: {1} {2} {3}", logLevel, Name, messageFunc(), exception));
            Exception ExceptionLog = new Exception(String.Format("{0}: {1} {2} {3}", logLevel, Name, messageFunc(), exception));
            ElmahCore.ElmahExtensions.RiseError(ExceptionLog);
            
            // Telling LibLog the message was successfully logged.
            return true;
        }
    }

    public class ElmahLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            // Logger name usually contains the full name of a type that uses it,
            // e.g. "Hangfire.Server.RecurringJobScheduler". It's used to know the
            // context of this or that message and for filtering purposes.
            return new ElmahLog () { Name = name };
        }
    }

}


