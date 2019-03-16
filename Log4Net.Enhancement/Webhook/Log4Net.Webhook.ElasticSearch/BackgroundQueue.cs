using Log4Net.Webhook.ElasticSearch.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Log4Net.Webhook.ElasticSearch
{
    public class BackgroundQueue
    {
        private static Timer _dequeueTimer;
        private static readonly ConcurrentQueue<LogEvent> LogEvents = new ConcurrentQueue<LogEvent>();
        private readonly SemaphoreSlim _signalQueue = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _signalDequeue = new SemaphoreSlim(1);
        private readonly int _bufferSize;
        private readonly TimeSpan _timeBuffer;
        private readonly IEsHttpClient _esHttpClient;

        public BackgroundQueue(int bufferSize, int timeBuffer, string connectionString, CancellationToken cancellationToken)
        {
            _bufferSize = bufferSize;
            _timeBuffer = timeBuffer <= 0 ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(timeBuffer);
            _dequeueTimer = new Timer(Dequeue, cancellationToken, _timeBuffer, _timeBuffer);
            _esHttpClient = new EsHttpClient(connectionString);
        }

        public void QueueEvents(IEnumerable<LogEvent> events, CancellationToken cancellationToken)
        {
            foreach (var logEvent in events)
                LogEvents.Enqueue(logEvent);
            _signalQueue.Wait(cancellationToken);
            if (LogEvents.Count >= _bufferSize)
                Dequeue(cancellationToken);
            _signalQueue.Release();
        }

        public void Dequeue(object cancellationToken)
        {
            _dequeueTimer.Change(_timeBuffer, _timeBuffer);
            var token = (CancellationToken)cancellationToken;
            _signalDequeue.Wait(token);
            var buffer = new List<LogEvent>(LogEvents.Count);
            while (buffer.Count < buffer.Capacity)
            {
                LogEvents.TryDequeue(out var logEvent);
                buffer.Add(logEvent);
            }
            if (buffer.Count > 0)
                try
                {
                    var task = new Task(() => _esHttpClient.PostBulk(buffer, token));
                    task.ContinueWith((x) => _signalDequeue.Release(), token);
                    task.Start();
                }
                catch (Exception)
                {
                    // ignored
                }
            else
                _signalDequeue.Release();
        }

        public async Task Flush(object cancellationToken)
        {
            _dequeueTimer.Change(_timeBuffer, _timeBuffer);
            var token = (CancellationToken)cancellationToken;
            _signalDequeue.Wait(token);
            var buffer = new List<LogEvent>(LogEvents.Count);
            while (buffer.Count < buffer.Capacity)
            {
                LogEvents.TryDequeue(out var logEvent);
                buffer.Add(logEvent);
            }
            if (buffer.Count > 0)
            {
                try
                {
                    await Task.Run(() => _esHttpClient.PostBulk(buffer, token));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            _signalDequeue.Release();
        }
    }
}
