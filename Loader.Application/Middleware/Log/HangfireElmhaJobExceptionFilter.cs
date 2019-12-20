using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
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
    public class HangfireElmhaJobExceptionFilter : JobFilterAttribute, IElectStateFilter
    {
        public HangfireElmhaJobExceptionFilter()
        {

        }
        public void OnStateElection(ElectStateContext context)
        {
            // the way Hangfire works is retrying a job X times (10 by default), so this wont be called directly with a 
            // failed state sometimes.
            // To solve this we should look into TraversedStates for a failed state

            var failed = context.CandidateState as FailedState ??
                         context.TraversedStates.FirstOrDefault(x => x is FailedState) as FailedState;

            if (failed == null)
                return;
            string message = failed.Exception.ToString();
            
            ElmahCore.ElmahExtensions.RiseError(failed.Exception);
            //here you have the failed.Exception and you can do anything with it
            //and also the job name context.Job.Type.Name
        }
    }

}


