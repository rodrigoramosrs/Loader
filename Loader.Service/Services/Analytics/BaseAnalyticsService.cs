using Loader.Domain.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.Analytics
{
    public abstract class BaseAnalyticsService
    {
        private readonly string _AnalyticsID;
        private readonly string _CustomerName;
        private readonly string _CustomerID;

        protected string AnalyticsID { get { return this._AnalyticsID; } }
        protected string CustomerName { get { return this._CustomerName; } }
        protected string CustomerID { get { return this._CustomerID; } }

        protected BaseAnalyticsService(string AnalyticsID, string CustomerName, string CustomerID)
        {
            _AnalyticsID = AnalyticsID;
            _CustomerName = CustomerName;
            _CustomerID = CustomerID;
        }

        /// <summary>
        /// Metodo responsável por registrar o estatísticas uso do sistema
        /// </summary>
        /// <param name="Category">Normalmente, é o objeto que participou da interação(por exemplo, 'Video'</param>
        /// <param name="Action">O tipo de interação (por exemplo, 'play'</param>
        /// <param name="Label">Útil para classificar eventos (por exemplo, 'Fall Campaign').</param>
        /// <param name="Value">Um valor numérico associado ao evento (por exemplo, 42)</param>
        /// <returns></returns>
        public abstract Task<bool> Send(AnalyticsData AnalyticsData);
    }
}
