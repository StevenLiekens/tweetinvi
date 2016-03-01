﻿using System;
using Tweetinvi.Core;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Core.Events.EventArguments;
using Tweetinvi.Core.Interfaces.Controllers;
using Tweetinvi.Core.Interfaces.Credentials;
using Tweetinvi.Core.Interfaces.RateLimit;

namespace Tweetinvi
{
    public static class RateLimit
    {
        [ThreadStatic]
        private static IHelpController _helpController;
        public static IHelpController HelpController
        {
            get
            {
                if (_helpController == null)
                {
                    Initialize();
                }

                return _helpController;
            }
        }

        [ThreadStatic]
        private static IRateLimitCacheManager _rateLimitCacheManager;

        private static IRateLimitCacheManager RateLimitCacheManager
        {
            get
            {
                if (_rateLimitCacheManager == null)
                {
                    Initialize();
                }

                return _rateLimitCacheManager;
            }
        }

        [ThreadStatic]
        private static IRateLimitAwaiter _rateLimitAwaiter;

        private static IRateLimitAwaiter RateLimitAwaiter
        {
            get
            {
                if (_rateLimitAwaiter == null)
                {
                    Initialize();
                }

                return _rateLimitAwaiter;
            }
        }

        private static readonly IRateLimitCache _rateLimitCache;

        static RateLimit()
        {
            Initialize();

            _rateLimitCache = TweetinviContainer.Resolve<IRateLimitCache>();
        }

        static void Initialize()
        {
            _helpController = TweetinviContainer.Resolve<IHelpController>();
            _rateLimitCacheManager = TweetinviContainer.Resolve<IRateLimitCacheManager>();
            _rateLimitAwaiter = TweetinviContainer.Resolve<IRateLimitAwaiter>();
        }

        /// <summary>
        /// Notify that a query is awaiting for RateLimits to become available in order to continue
        /// </summary>
        public static event EventHandler<QueryAwaitingEventArgs> QueryAwaitingForRateLimit
        {
            add { RateLimitAwaiter.QueryAwaitingForRateLimit += value; }
            remove { RateLimitAwaiter.QueryAwaitingForRateLimit -= value; }
        }

        /// <summary>
        /// Configure how to Tweetinvi will handle RateLimits
        /// </summary>
        public static RateLimitTrackerOptions RateLimitTrackerOption
        {
            get { return TweetinviConfig.CURRENT_RATELIMIT_TRACKER_OPTION; }
            set { TweetinviConfig.CURRENT_RATELIMIT_TRACKER_OPTION = value; }
        }

        /// <summary>
        /// Clear all the RateLimits information stored in the cache
        /// </summary>
        public static void ClearRateLimitCache()
        {
            _rateLimitCache.ClearAll();
        }

        /// <summary>
        /// Clear a specific set of credentials RateLimits information stored in the cache
        /// </summary>
        public static void ClearRateLimitCache(ITwitterCredentials credentials)
        {
            _rateLimitCache.Clear(credentials);
        }

        /// <summary>
        /// Wait for the rate limits to be available. This should be used before executing a query
        /// </summary>
        public static void AwaitForQueryRateLimit(string query)
        {
            AwaitForQueryRateLimit(query, Auth.Credentials);
        }

        /// <summary>
        /// Wait for the rate limits to be available. This should be used before executing a query
        /// </summary>
        public static void AwaitForQueryRateLimit(string query, ITwitterCredentials credentials)
        {
            RateLimitAwaiter.WaitForCredentialsRateLimit(query, credentials);
        }

        /// <summary>
        /// Wait for the rate limits to be available. This should be used before executing a query
        /// </summary>
        public static void AwaitForQueryRateLimit(ITokenRateLimit tokenRateLimit)
        {
            RateLimitAwaiter.WaitForCredentialsRateLimit(tokenRateLimit);
        }

        /// <summary>
        /// Get the rate limits information for an url
        /// </summary>
        public static ITokenRateLimit GetQueryRateLimit(string query)
        {
            return RateLimitCacheManager.GetQueryRateLimit(query, Auth.Credentials);
        }

        /// <summary>
        /// Get the rate limits information for an url
        /// </summary>
        public static ITokenRateLimit GetQueryRateLimit(string query, ITwitterCredentials credentials)
        {
            return RateLimitCacheManager.GetQueryRateLimit(query, credentials);
        }

        /// <summary>
        /// Get all the rate limits of all the Twitter endpoints
        /// </summary>
        public static ITokenRateLimits GetCurrentCredentialsRateLimits(bool useRateLimitCache = false)
        {
            ITokenRateLimits tokenRateLimits = null;
            if (!useRateLimitCache)
            {
                tokenRateLimits = HelpController.GetCurrentCredentialsRateLimits();
                RateLimitCacheManager.UpdateTokenRateLimits(Auth.Credentials, tokenRateLimits);
            }
            else
            {
                tokenRateLimits = RateLimitCacheManager.GetTokenRateLimits(Auth.Credentials);
            }

            return tokenRateLimits;
        }

        /// <summary>
        /// Get all the rate limits of all the Twitter endpoints
        /// </summary>
        public static ITokenRateLimits GetCredentialsRateLimits(ITwitterCredentials credentials, bool useRateLimitCache = false)
        {
            if (useRateLimitCache)
            {
                return RateLimitCacheManager.GetTokenRateLimits(credentials);
            }
            else
            {
                return HelpController.GetCredentialsRateLimits(credentials);
            }
        }
    }
}