﻿using System;

using MongoDB.Driver;

namespace Serilog.Sinks.MongoDB
{
    public class MongoDBSinkConfiguration
    {
        public string CollectionName { get; private set; } = MongoDBSinkDefaults.CollectionName;

        public int BatchPostingLimit { get; private set; } = MongoDBSinkDefaults.BatchPostingLimit;

        public CreateCollectionOptions CollectionCreationOptions { get; private set; }

        public TimeSpan BatchPeriod { get; private set; } = MongoDBSinkDefaults.BatchPeriod;

        public MongoUrl MongoUrl { get; private set; }

        public IMongoDatabase MongoDatabase { get; private set; }

        public void Validate()
        {
            if (MongoDatabase == null && MongoUrl == null)
            {
                throw new ArgumentOutOfRangeException("Invalid Configuration: MongoDatabase or Mongo Connection String must be set");
            }
        }

        /// <summary>
        ///     Set the periodic batch timeout period. (Default: 2 seconds)
        /// </summary>
        /// <param name="period"></param>
        public void SetBatchPeriod(TimeSpan period)
        {
            this.BatchPeriod = period;
        }

        /// <summary>
        ///     Setup capped collections during collection creation
        /// </summary>
        /// <param name="cappedMaxSizeMb"></param>
        /// <param name="cappedMaxDocuments"></param>
        public void SetCreateCappedCollection(
            long cappedMaxSizeMb = 50,
            long? cappedMaxDocuments = null)
        {
            this.CollectionCreationOptions = new CreateCollectionOptions
                                             {
                                                 Capped = true,
                                                 MaxSize = cappedMaxSizeMb * 1024 * 1024
                                             };

            if (cappedMaxDocuments.HasValue)
                this.CollectionCreationOptions.MaxDocuments = cappedMaxDocuments.Value;
        }

        /// <summary>
        ///     Set the mongo database instance directly
        /// </summary>
        /// <param name="database"></param>
        public void SetMongoDatabase(IMongoDatabase database)
        {
            this.MongoDatabase = database ?? throw new ArgumentNullException(nameof(database));
            this.MongoUrl = null;
        }

        /// <summary>
        ///     Set the mongo url (connection string) -- e.g. mongodb://localhost
        /// </summary>
        /// <param name="mongoUrl"></param>
        public void SetMongoUrl(string mongoUrl)
        {
            if (string.IsNullOrWhiteSpace(mongoUrl))
                throw new ArgumentNullException(nameof(mongoUrl));

            this.MongoUrl = MongoUrl.Create(mongoUrl);
            this.MongoDatabase = null;
        }

        /// <summary>
        ///     Set the MongoDB collection name (Default: 'logs')
        /// </summary>
        /// <param name="collectionName"></param>
        public void SetCollectionName(string collectionName)
        {
            if (collectionName == string.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionName), "Must not be string.empty");
            }

            this.CollectionName = collectionName ?? MongoDBSinkDefaults.CollectionName;
        }

        /// <summary>
        ///     Set the batch posting limit (Default: 50)
        /// </summary>
        /// <param name="batchPostingLimit"></param>
        public void SetBatchPostingLimit(int batchPostingLimit)
        {
            this.BatchPostingLimit = batchPostingLimit;
        }

#if NET452
        /// <summary>
        ///     Tries to set the Mongo url from a connection string in the .config file.
        /// </summary>
        /// <returns>false if not found</returns>
        /// <param name="connectionStringName"></param>
        public bool TrySetMongoUrlFromConnectionStringNamed(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException(nameof(connectionStringName));

            var connectionString =
                System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionString == null)
                return false;

            this.SetMongoUrl(connectionString.ConnectionString);

            return true;
        }

        /// <summary>
        ///     Set the Mongo url (connection string) or Connection String Name -- e.g. mongodb://localhost
        /// </summary>
        /// <param name="connectionString"></param>
        public void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (!this.TrySetMongoUrlFromConnectionStringNamed(connectionString))
                this.SetMongoUrl(connectionString);
        }
#else
        /// <summary>
        ///     Set the Mongo url (connection string)
        /// </summary>
        /// <param name="connectionString"></param>
        public void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            this.SetMongoUrl(connectionString);
        }
#endif
    }
}