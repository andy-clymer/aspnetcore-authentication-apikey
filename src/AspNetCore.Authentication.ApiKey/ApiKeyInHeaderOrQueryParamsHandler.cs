﻿// Copyright (c) Mihir Dilip. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.ApiKey
{
	public class ApiKeyInHeaderOrQueryParamsHandler : ApiKeyHandlerBase
	{
#if NET8_0_OR_GREATER
        protected ApiKeyInHeaderOrQueryParamsHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        [Obsolete("ISystemClock is obsolete, use TimeProvider on AuthenticationSchemeOptions instead.")]
#endif
        public ApiKeyInHeaderOrQueryParamsHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}

		protected override Task<string> ParseApiKeyAsync()
		{
			// Try query parameter
			if (Request.Query.TryGetValue(Options.KeyName, out var value))
			{
				return Task.FromResult(value.FirstOrDefault());
			}

			// No ApiKey query parameter found try headers
			if (Request.Headers.TryGetValue(Options.KeyName, out var headerValue))
			{
				return Task.FromResult(headerValue.FirstOrDefault());
			}

			// No ApiKey query parameter or header found then try Authorization header
			if (Request.Headers.TryGetValue(HeaderNames.Authorization, out Microsoft.Extensions.Primitives.StringValues authHeaderStringValue) 
					&& AuthenticationHeaderValue.TryParse(authHeaderStringValue, out var authHeaderValue)
					&& authHeaderValue.Scheme.Equals(Options.KeyName, StringComparison.OrdinalIgnoreCase)
			)
			{
				return Task.FromResult(authHeaderValue.Parameter);
			}

			// No ApiKey found
			return Task.FromResult(string.Empty);
		}
	}
}