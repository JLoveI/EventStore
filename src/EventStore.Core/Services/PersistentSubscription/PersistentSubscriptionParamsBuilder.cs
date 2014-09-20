using System;
using EventStore.Common.Utils;

namespace EventStore.Core.Services.PersistentSubscription
{
    /// <summary>
    /// Builds a <see cref="PersistentSubscriptionParams"/> object.
    /// </summary>
    public class PersistentSubscriptionParamsBuilder
    {
        private bool _resolveLinkTos;
        private int _startFrom;
        private bool _latencyStatistics;
        private TimeSpan _timeout;
        private int _readBatchSize;
        private int _maxRetryCount;
        private int _liveBufferSize;
        private bool _preferRoundRobin;
        private int _historyBufferSize;
        private string _subscriptionId;
        private string _eventStreamId;
        private string _groupName;
        private IPersistentSubscriptionEventLoader _eventLoader;
        private IPersistentSubscriptionCheckpointReader _checkpointReader;
        private IPersistentSubscriptionCheckpointWriter _checkpointWriter;

        /// <summary>
        /// Creates a new <see cref="PersistentSubscriptionParamsBuilder"></see> object
        /// </summary>
        /// <param name="streamName">The name of the stream for the subscription</param>
        /// <param name="groupName">The name of the group of the subscription</param>
        /// <returns>a new <see cref="PersistentSubscriptionParamsBuilder"></see> object</returns>
        public static PersistentSubscriptionParamsBuilder CreateFor(string streamName, string groupName)
        {
            return new PersistentSubscriptionParamsBuilder(streamName + ":" + groupName, 
                streamName, 
                groupName, 
                false,
                0,
                false,
                TimeSpan.FromSeconds(30),
                500,
                500,
                10,
                20,
                true);
        }


        private PersistentSubscriptionParamsBuilder(string subscriptionId, string streamName, string groupName, bool resolveLinkTos, int startFrom, bool latencyStatistics, TimeSpan timeout,
            int historyBufferSize, int liveBufferSize, int maxRetryCount, int readBatchSize, bool preferRoundRobin)
        {
            _resolveLinkTos = resolveLinkTos;
            _startFrom = startFrom;
            _latencyStatistics = latencyStatistics;
            _timeout = timeout;
            _historyBufferSize = historyBufferSize;
            _liveBufferSize = liveBufferSize;
            _maxRetryCount = maxRetryCount;
            _readBatchSize = readBatchSize;
            _preferRoundRobin = preferRoundRobin;
            _eventStreamId = streamName;
            _subscriptionId = subscriptionId;
            _groupName = groupName;
        }

        /// <summary>
        /// Sets the checkpoint reader for the instance
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public PersistentSubscriptionParamsBuilder WithCheckpointReader(IPersistentSubscriptionCheckpointReader reader)
        {
            _checkpointReader = reader;
            return this;
        }

        /// <summary>
        /// Sets the check point reader for the subscription
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public PersistentSubscriptionParamsBuilder WithCheckpointWriter(IPersistentSubscriptionCheckpointWriter writer)
        {
            _checkpointWriter = writer;
            return this;
        }

        /// <summary>
        /// Sets the event loader for the subscription
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public PersistentSubscriptionParamsBuilder WithEventLoader(IPersistentSubscriptionEventLoader loader)
        {
            _eventLoader = loader;
            return this;
        }
        
        /// <summary>
        /// Sets the option to include further latency statistics. These statistics have a cost and should not be used
        /// in high performance situations.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithExtraLatencyStatistics()
        {
            _latencyStatistics = true;
            return this;
        }

        /// <summary>
        /// Sets the option to resolve linktos on events that are found for this subscription.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder ResolveLinkTos()
        {
            _resolveLinkTos = true;
            return this;
        }

        /// <summary>
        /// Sets the option to not resolve linktos on events that are found for this subscription.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder DoNotResolveLinkTos()
        {
            _resolveLinkTos = false;
            return this;
        }

        /// <summary>
        /// If set the subscription will prefer if possible to round robin between the clients that
        /// are connected.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder PreferRoundRobin()
        {
            _preferRoundRobin = true;
            return this;
        }

        /// <summary>
        /// If set the subscription will prefer if possible to dispatch only to a single of the connected
        /// clients. If however the buffer limits are reached on that client it will begin sending to other 
        /// clients.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder PreferDispatchToSingle()
        {
            _preferRoundRobin = false;
            return this;
        }

        /// <summary>
        /// Sets that the subscription should start from the beginning of the stream.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder StartFromBeginning()
        {
            _startFrom = 0;
            return this;
        }

        /// <summary>
        /// Sets that the subscription should start from a specified location of the stream.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder StartFrom(int position)
        {
            _startFrom = position;
            return this;
        }

        /// <summary>
        /// Sets the timeout for a message (will be retried if an ack is not received within this timespan)
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithMessageTimeoutOf(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the number of times a message should be retried before being considered a bad message
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithMaxRetriesOf(int count)
        {
            Ensure.Nonnegative(count, "count");
            _maxRetryCount = count;
            return this;
        }

        /// <summary>
        /// Sets the size of the live buffer for the subscription. This is the buffer used 
        /// to cache messages while sending messages as they happen. The count is
        /// in terms of the number of messages to cache.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithLiveBufferSizeOf(int count)
        {
            Ensure.Nonnegative(count, "count");
            _liveBufferSize = count;
            return this;
        }


        /// <summary>
        /// Sets the size of the read batch used when paging in history for the subscription
        /// sizes should not be too big ...
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithReadBatchOf(int count)
        {
            Ensure.Nonnegative(count, "count");
            _readBatchSize = count;
            return this;
        }


        /// <summary>
        /// Sets the size of the read batch used when paging in history for the subscription
        /// sizes should not be too big ...
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder WithHistoryBufferSizeOf(int count)
        {
            Ensure.Nonnegative(count, "count");
            _historyBufferSize = count;
            return this;
        }

        /// <summary>
        /// Sets that the subscription should start from where the stream is when the subscription is first connected.
        /// </summary>
        /// <returns>A new <see cref="PersistentSubscriptionParamsBuilder"></see></returns>
        public PersistentSubscriptionParamsBuilder StartFromCurrent()
        {
            _startFrom = -1;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="PersistentSubscriptionParams"/> object from a <see cref="PersistentSubscriptionParamsBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="PersistentSubscriptionParamsBuilder"/> from which to build a <see cref="PersistentSubscriptionParamsBuilder"/></param>
        /// <returns></returns>
        public static implicit operator PersistentSubscriptionParams(PersistentSubscriptionParamsBuilder builder)
        {
            return new PersistentSubscriptionParams(builder._resolveLinkTos,
                builder._subscriptionId,
                builder._eventStreamId,
                builder._groupName,
                builder._startFrom,
                builder._latencyStatistics,
                builder._timeout,
                builder._preferRoundRobin,
                builder._maxRetryCount,
                builder._liveBufferSize,
                builder._historyBufferSize,
                builder._readBatchSize,
                builder._eventLoader,
                builder._checkpointReader,
                builder._checkpointWriter);
        }
    }
}