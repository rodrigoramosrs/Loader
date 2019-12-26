using Hangfire;
using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Loader.Infra.Data.Repository;
using Loader.Domain.Models.Job;

namespace Loader.Service.Services.Job
{
    public class JobService
    {
        private readonly Analytics.BaseAnalyticsService _AnalyticsService;
        private readonly Loader.Infra.Data.Repository.JobRepository _JobRepository;
        
        public JobService(Analytics.BaseAnalyticsService AnalyticsService, JobRepository JobRepository)
        {
            _AnalyticsService = AnalyticsService;
            _JobRepository = JobRepository;
        }
        public bool ClearAllJobStatus()
        {
            return this._JobRepository.ClearAllJobStatus();
        }

        public void CreateUpdateJobStatus(JobStatus JobStatus)
        {
            this._JobRepository.CreateUpdateJobStatus(JobStatus);
        }

        public JobStatus GetQueuePosition(JobStatus JobStatus)
        {
           return this._JobRepository.GetQueuePosition(JobStatus);
        }

        public string Enqueue([InstantHandle][NotNull] Expression<Action> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public string Enqueue<T>([InstantHandle][NotNull] Expression<Func<T, Task>> methodCall)
        {
            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public string Enqueue<T>([InstantHandle][NotNull] Expression<Action<T>> methodCall)
        {
            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public string Enqueue([InstantHandle][NotNull] Expression<Func<Task>> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        private async void SendAnalyticsData(string ActionName, string Description)
        {
            Task.Run(() => _AnalyticsService.SendInformation(ActionName, Description));
        }
    }
}
