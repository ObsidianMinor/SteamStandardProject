using Steam.Common;
using Steam.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    internal class JobManager
    {
        private const int jobTimeout = 10000;
        ConcurrentDictionary<SteamGuid, (Timer, object, Func<Exception, bool>)> _runningJobs = new ConcurrentDictionary<SteamGuid, (Timer, object, Func<Exception, bool>)>();
        LogManager _jobLog;
        private int _currentJobId = 0;
        private DateTime _startTime;

        internal JobManager(LogManager manager)
        {
            _jobLog = manager;
            _startTime = Process.GetCurrentProcess().StartTime;
        }

        internal (Task<T>, SteamGuid) AddJob<T>()
        {
            SteamGuid jobId = new SteamGuid((uint)Interlocked.Increment(ref _currentJobId), _startTime, 0, 0);
            return (AddJob<T>(jobId), jobId);
        }

        internal async Task<T> AddJob<T>(SteamGuid jobId)
        {
            if (_runningJobs.ContainsKey(jobId))
                throw new InvalidOperationException();

            TaskCompletionSource<T> jobCompletionSource = new TaskCompletionSource<T>();
            Timer jobLifetime = new Timer(TimeoutTimer, jobId, jobTimeout, -1);
            _runningJobs.TryAdd(jobId, (jobLifetime, jobCompletionSource, jobCompletionSource.TrySetException));
            await _jobLog.LogDebugAsync($"Added job {jobId.SequentialCount}");

            return await jobCompletionSource.Task;
        }

        internal async Task SetJobResult<T>(SteamGuid jobId, T result)
        {
            if (!_runningJobs.TryGetValue(jobId, out (Timer timer, object source, Func<Exception, bool> _) jobTaskTimer) || !(jobTaskTimer.source is TaskCompletionSource<T> completionSource))
            {
                await _jobLog.LogErrorAsync($"Could not set job result: Requested job {jobId.SequentialCount} does not exist in the list of running jobs").ConfigureAwait(false);
                return;
            }
            
            completionSource.TrySetResult(result);
            _runningJobs.TryRemove(jobId, out _);
            jobTaskTimer.timer.Dispose();
            await _jobLog.LogDebugAsync($"Removed job {jobId.SequentialCount} after setting successful result");
        }

        internal async Task SetJobFail(SteamGuid jobId, Exception exception)
        {
            if(!_runningJobs.TryGetValue(jobId, out (Timer timer, object completionSource, Func<Exception, bool> exceptionSetter) job))
            {
                await _jobLog.LogErrorAsync($"Could not set job failure: Requested job {jobId.SequentialCount} does not exist in the list of running jobs").ConfigureAwait(false);
                return;
            }

            if (!job.exceptionSetter(exception))
                await _jobLog.LogInfoAsync("Could not set exception on job task, the task may have already been completed or the task was disposed").ConfigureAwait(false);

            _runningJobs.TryRemove(jobId, out _);
            job.timer.Dispose();
        }   

        internal async Task HeartbeatJob(SteamGuid jobId)
        {
            if (!_runningJobs.TryGetValue(jobId, out (Timer timer, object completionSource, Func<Exception, bool> _) job))
            {
                await _jobLog.LogErrorAsync($"Could not heartbeat job: Requested job {jobId.SequentialCount} does not exist in the list of running jobs").ConfigureAwait(false);
                return;
            }

            job.timer.Change(jobTimeout, -1);
            await _jobLog.LogDebugAsync($"Heartbeat job {jobId.SequentialCount}").ConfigureAwait(false);
        }

        private void TimeoutTimer(object state)
        {
            SteamGuid jobId = (SteamGuid)state;
            _runningJobs.TryRemove(jobId, out (Timer timer, object completionSource, Func<Exception, bool> exceptionSetter) job);
            job.exceptionSetter(new TimeoutException("The destination job timed out"));
            job.timer.Dispose();
        }
    }
}
