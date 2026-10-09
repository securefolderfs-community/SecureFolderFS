using System;
using System.Collections.Generic;
using System.Threading;
using SecureFolderFS.Sdk.Api.Enums;

namespace SecureFolderFS.Sdk.Api.Helpers
{
    /// <summary>
    /// Enforces per-client request budgets using a token bucket per request class.
    /// </summary>
    public sealed class ApiRateLimiterHelper
    {
        private readonly Lock _lock = new();
        private readonly Dictionary<string, Dictionary<ApiRequestType, Bucket>> _clients = [];

        /// <summary>
        /// Consumes budget for a request, if any remains.
        /// </summary>
        /// <param name="clientKey">Identifies the caller across connections.</param>
        /// <param name="requestType">Determines which budget applies.</param>
        /// <returns>Whether the request is allowed, and how long to wait when it is not.</returns>
        public (bool IsAllowed, int RetryAfterMs) Check(string clientKey, ApiRequestType requestType)
        {
            var now = DateTimeOffset.UtcNow;
            var (capacity, refillInterval) = GetBudget(requestType);

            lock (_lock)
            {
                if (!_clients.TryGetValue(clientKey, out var buckets))
                    _clients[clientKey] = buckets = [];

                if (!buckets.TryGetValue(requestType, out var bucket))
                    buckets[requestType] = bucket = new Bucket(capacity, now);

                bucket.Refill(now, capacity, refillInterval);

                if (bucket.Tokens >= 1d)
                {
                    bucket.Tokens--;
                    return (true, 0);
                }

                var retryAfter = refillInterval * (1d - bucket.Tokens);
                return (false, (int)retryAfter.TotalMilliseconds);
            }
        }

        private static (double Capacity, TimeSpan RefillInterval) GetBudget(ApiRequestType requestType) => requestType switch
        {
            // Enumeration is invisible to the user, so the budget only needs to stop runaway polling.
            ApiRequestType.Read => (60d, TimeSpan.FromSeconds(1)),

            // Roughly five prompts a minute, with a short burst allowance for legitimate rapid use.
            ApiRequestType.Trigger => (5d, TimeSpan.FromSeconds(12)),

            // One consent prompt at a time, and no rapid retries after a refusal.
            ApiRequestType.Pairing => (1d, TimeSpan.FromSeconds(60)),

            _ => throw new ArgumentOutOfRangeException(nameof(requestType))
        };

        private sealed class Bucket(double tokens, DateTimeOffset lastRefill)
        {
            private DateTimeOffset _lastRefill = lastRefill;

            public double Tokens { get; set; } = tokens;

            public void Refill(DateTimeOffset now, double capacity, TimeSpan refillInterval)
            {
                var elapsed = now - _lastRefill;
                if (elapsed <= TimeSpan.Zero)
                    return;

                Tokens = Math.Min(capacity, Tokens + elapsed / refillInterval);
                _lastRefill = now;
            }
        }
    }
}