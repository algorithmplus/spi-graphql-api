﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.GraphQlApi.Domain.Common;
using Dfe.Spi.GraphQlApi.Domain.Configuration;
using Dfe.Spi.GraphQlApi.Domain.Registry;
using Newtonsoft.Json;
using RestSharp;

namespace Dfe.Spi.GraphQlApi.Infrastructure.RegistryApi
{
    public class RegistryApiRegistryProvider : IRegistryProvider
    {
        private readonly IRestClient _restClient;
        private readonly ILoggerWrapper _logger;

        public RegistryApiRegistryProvider(IRestClient restClient, RegistryConfiguration configuration, ILoggerWrapper logger)
        {
            _restClient = restClient;
            _restClient.BaseUrl = new Uri(configuration.RegistryApiBaseUrl, UriKind.Absolute);
            if (!string.IsNullOrEmpty(configuration.RegistryApiFunctionKey))
            {
                _restClient.DefaultParameters.Add(new Parameter("x-functions-key", configuration.RegistryApiFunctionKey,
                    ParameterType.HttpHeader));
            }

            _logger = logger;
        }
        public async Task<EntityReference[]> GetSynonymsAsync(string entityType, string sourceSystem, string sourceSystemId,
            CancellationToken cancellationToken)
        {
            var resource = $"{entityType}/{sourceSystem}/{sourceSystemId}/synonyms";
            _logger.Debug($"Looking up synonyms at {resource}");
            
            var httpRequest = new RestRequest(resource, Method.GET);
            
            var response = await _restClient.ExecuteTaskAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessful)
            {
                throw new RegistryApiException(resource, response.StatusCode, response.Content);
            }
            _logger.Debug($"Synonyms response json from {resource} is ${response.Content}");

            var results = JsonConvert.DeserializeObject<GetSynonymsResult>(response.Content);
            _logger.Debug($"Deserialized response from {resource} to {JsonConvert.SerializeObject(results)}");

            return results.Synonyms;
        }
    }
}