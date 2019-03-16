using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Log4Net.Webhook.ElasticSearch.Models;
using Log4Net.Webhook.Extensions;

namespace Log4Net.Webhook.ElasticSearch
{
    public class ElasticSearchAppender : BufferingAppenderSkeleton
    {
        static readonly string AppenderType = typeof(ElasticSearchAppender).Name;

        const int DefaultOnCloseTimeout = 30000;

        public ElasticSearchAppender()
        {
            OnCloseTimeout = DefaultOnCloseTimeout;
        }

        public string ConnectionString { get; set; }
        public int TimeBuffer { get; set; }
        public int OnCloseTimeout { get; set; }
        private BackgroundQueue _logQueue;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            ServicePointManager.Expect100Continue = false;

            try
            {
                Validate(ConnectionString);
            }
            catch (Exception ex)
            {
                HandleError("Failed to validate ConnectionString in ActivateOptions", ex);
                return;
            }

            // Artificially add the buffer size to the connection string so it can be parsed
            // later to decide if we should send a _bulk API call
            ConnectionString += $";BufferSize={BufferSize}";
            _logQueue = new BackgroundQueue(BufferSize, TimeBuffer, ConnectionString, _cts.Token);
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            try
            {
                new Task(() => _logQueue.QueueEvents(LogEvent.CreateMany(events), _cts.Token)).Start();
                //Task.Run(() => _logQueue.QueueEvents(logEvent.CreateMany(events), _cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                HandleError("Error during adding logs", ex);
            }
        }

        protected override void OnClose()
        {
            Task.WaitAll(_logQueue.Flush(_cts.Token));
            //_cts.Cancel();
            base.OnClose();
        }


        void HandleError(string message, Exception ex)
        {
            ErrorHandler.Error("{0} [{1}]: {2}.".With(AppenderType, Name, message), ex, ErrorCode.GenericFailure);
        }

        private static void Validate(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            if (connectionString.Length == 0)
                throw new ArgumentException("connectionString is empty", nameof(connectionString));
        }
    }
}
