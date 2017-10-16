using Steam.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    internal class JobManager<T>
    {
        private const int jobTimeout = 10000;
        private const int period = Timeout.Infinite;

        private ConcurrentDictionary<SteamGid, (TaskCompletionSource<T>, Timer)> _runningJobs = new ConcurrentDictionary<SteamGid, (TaskCompletionSource<T>, Timer)>();
        private Logger _jobLog;
        private int _currentJobId = 0;
        private DateTime _startTime;
        private int _processId;

        internal JobManager(Logger manager)
        {
            _jobLog = manager;
            var process = Process.GetCurrentProcess();
            _startTime = process.StartTime;
            _processId = process.Id;
        }

        internal (Task<T> task, SteamGid id) AddJob()
        {
            Interlocked.Increment(ref _currentJobId);
            var gid = new SteamGid((uint)_currentJobId, _startTime, 0, 0);
            var task = AddJob(gid);
            return (task, gid);
        }

        internal async Task<T> AddJob(SteamGid job)
        {
            var timeout = new Timer(TimeoutTimer, job, jobTimeout, period);
            var task = new TaskCompletionSource<T>();

            _runningJobs[job] = (task, timeout);
            await _jobLog.DebugAsync($"Added job {job}").ConfigureAwait(false);

            return await task.Task.ConfigureAwait(false);
        }

        internal bool IsRunningJob(SteamGid job)
        {
            return _runningJobs.ContainsKey(job);
        }

        internal async Task HeartbeatJob(SteamGid job)
        {
            await _jobLog.DebugAsync($"Extending job ({job}) timeout by {jobTimeout} ms").ConfigureAwait(false);
            _runningJobs[job].Item2.Change(jobTimeout, period);
        }

        internal async Task SetJobResult(T message, SteamGid job)
        {
            await _jobLog.DebugAsync($"Setting successful job result for job {job}").ConfigureAwait(false);
            _runningJobs[job].Item1.SetResult(message);
        }

        internal async Task CancelAllJobs()
        {
            foreach (var job in _runningJobs)
                await SetJobFail(job.Key, new TaskCanceledException(job.Value.Item1.Task));
        }

        internal async Task SetJobFail(SteamGid job, Exception ex)
        {
            if (!_runningJobs.TryRemove(job, out (TaskCompletionSource<T>, Timer) jobTuple))
            {
                await _jobLog.WarningAsync($"Could not find a job by ID {job}, someone has set a job outside the manager").ConfigureAwait(false);
                return;
            }

            await _jobLog.DebugAsync($"Failed job {job}").ConfigureAwait(false);

            jobTuple.Item1.SetException(ex);
            jobTuple.Item2.Dispose();
        }
        
        private void TimeoutTimer(object state)
        {
#if !DEBUG  // if we're debugging, we don't need to timeout
            SteamGid jobId = (SteamGid)state;
            _runningJobs.TryRemove(jobId, out Job job);
            job.Item1.SetException(new TimeoutException("The destination job timed out"));
            job.Item2.Dispose();
#endif
        }
    }
}
