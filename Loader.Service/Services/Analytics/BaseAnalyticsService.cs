using Loader.Domain.Models.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.Analytics
{
    public abstract class BaseAnalyticsService
    {
        private readonly string _DefaultAnalyticsID;
        private readonly string _CustomerName;
        private readonly string _CustomerID;
        private readonly string _ExceptionAnalyticsID;

        protected string DefaultAnalyticsID { get { return this._DefaultAnalyticsID; } }
        protected string ExceptionAnalyticsID { get { return this._ExceptionAnalyticsID; } }
        
        protected string CustomerName { get { return this._CustomerName; } }
        protected string CustomerID { get { return this._CustomerID; } }

        protected BaseAnalyticsService(string DefaultAnalyticsID, string ExceptionAnalyticsID, string CustomerName, string CustomerID)
        {
            _DefaultAnalyticsID = DefaultAnalyticsID;
            _CustomerName = CustomerName;
            _CustomerID = CustomerID;

            if (string.IsNullOrEmpty(ExceptionAnalyticsID))
                this._ExceptionAnalyticsID = DefaultAnalyticsID;
        }

        /// <summary>
        /// Metodo responsável por registrar o estatísticas uso do sistema
        /// </summary>
        /// <param name="Category">Normalmente, é o objeto que participou da interação(por exemplo, 'Video'</param>
        /// <param name="Action">O tipo de interação (por exemplo, 'play'</param>
        /// <param name="Label">Útil para classificar eventos (por exemplo, 'Fall Campaign').</param>
        /// <param name="Value">Um valor numérico associado ao evento (por exemplo, 42)</param>
        /// <returns></returns>
        public abstract Task<bool> SendInformation(string ActionName, string Description);// AnalyticsInformationData AnalyticsData);
        public abstract Task<bool> SendPageView(string HostName, string PageName, string Title = null);// AnalyticsInformationData AnalyticsData);
        public abstract Task<bool> SendException(string ActionName, Exception ex);//AnalyticsExceptionData ExceptionData);

        public String FormatExceptionMessage(Exception exception, int innerDepthCount = 0)
        {
            try
            {
                if (exception == null)
                {
                    return String.Empty;
                }

                var indent = innerDepthCount > 0 ? new String(' ', 4 * innerDepthCount) : String.Empty;

                // Type and message
                StringBuilder exceptionMessage = new StringBuilder();
                exceptionMessage.Append(indent);
                exceptionMessage.AppendLine(exception.GetType().FullName);
                exceptionMessage.AppendLine();
                exceptionMessage.Append(indent);
                exceptionMessage.Append(" Message: ");
                exceptionMessage.Append(exception.Message);

                // Reflected properties
                var exType = exception.GetType();
                var properties = exType.GetProperties();
                //List<String> handled = new List<string>(new String[] { "Data", "HelpLink", "HResult", "InnerException", "Message", "Source", "StackTrace", "TargetSite" });
                List<String> handled = new List<string>(new String[] { "Data", "InnerException", "Message", "StackTrace" });
                var propertiesToDisplay = properties.Where(prop => !handled.Contains(prop.Name));
                if (propertiesToDisplay.Count() > 0)
                {
                    exceptionMessage.AppendLine();
                    foreach (var property in propertiesToDisplay)
                    {
                        var value = property.GetValue(exception, null);
                        exceptionMessage.AppendFormat("\n{2} {0}: {1}", property.Name, value, indent);
                    }
                }

                // Data
                String exDataString = null;
                if (exception.Data != null && exception.Data.Count > 0)
                {
                    var exData = new StringBuilder();
                    bool first = true;
                    foreach (DictionaryEntry keyPair in exception.Data)
                    {
                        if (first) { exData.AppendLine(); }
                        exData.Append(String.Format("{2}    {0} - {1}", keyPair.Key, keyPair.Value, indent));
                    }
                    exDataString = exData.ToString();

                    exceptionMessage.AppendFormat("\n\n{1} Data: {0}", exDataString, indent);
                }

                // Stack trace
                var tabbedStackTrace = exception.StackTrace != null
                           ? ((innerDepthCount > 0)
                                ? indent + exception.StackTrace.Replace(Environment.NewLine, Environment.NewLine + indent)
                                : exception.StackTrace)
                           : null;

                if (!String.IsNullOrEmpty(tabbedStackTrace))
                {
                    exceptionMessage.AppendFormat("\n\n{1} Stack:\n{1} [\n{0}\n{1} ]", tabbedStackTrace, indent);
                }

                // Inner exception
                if (exception.InnerException != null)
                {
                    String innerExceptionString = FormatExceptionMessage(exception.InnerException, innerDepthCount + 1);

                    exceptionMessage.AppendFormat("\n\n{2} Inner-{0}:\n{2} (\n{1}\n{2} )", innerDepthCount, innerExceptionString, indent);
                }

                return exceptionMessage.ToString();
            }
            // This method will be used in exception handling and must not error.
            catch (Exception ex)
            {
                return "UNABLE TO GET FULL EXCEPTION OUTPUT.\n\nDefault output:\n" + ex.ToString();
            }
        }
    }
}
