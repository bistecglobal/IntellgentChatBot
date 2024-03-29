﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Extensions.Configuration;

namespace QnABot.Storages
{
    public class UserStateStorage : IStorage
    {

        private readonly CosmosDbStorage cosmosDbStorage;

        public UserStateStorage(IConfiguration configuration)
        {
            cosmosDbStorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = configuration["CosmosDBKey"],
                CollectionId = configuration["UserStateCollectionName"],
                CosmosDBEndpoint = new Uri(configuration["CosmosServiceEndpoint"]),
                DatabaseId = configuration["CosmosDBDatabaseName"]
            });
        }

        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await cosmosDbStorage.ReadAsync(keys, cancellationToken);
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            return cosmosDbStorage.WriteAsync(changes, cancellationToken);
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            return cosmosDbStorage.DeleteAsync(keys, cancellationToken);
        }
    }
}
