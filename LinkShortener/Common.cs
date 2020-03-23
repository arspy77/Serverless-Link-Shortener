using System;
using System.Collections.Generic;
using System.Text;

namespace LinkShortener
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.Documents;
    using LinkShortener.Model;

    public class Common
    {
        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
        public static async Task<CloudTable> CreateOrGetCloudTableAsync(string tableName)
        {
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=linkshortener-db;AccountKey=Xy2up5vqpihu9IA4jR2neJ7QWDksWMRGJrS7R9WaNnXpRvs5kIB2MBMQnrBzUUjVbomBSpjgyhg6rFSsclNbcg==;TableEndpoint=https://linkshortener-db.table.cosmos.azure.com:443/;";
            // string storageUri = System.UriBuilder(AppSettings.LoadAppSettings().StorageUri);
            // string storageCredentials = AppSettings.LoadAppSettings().StorageCredentials;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            {

            }

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }

        public static async Task<LinkMap> InsertOrMergeEntityAsync(CloudTable table, LinkMap entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                LinkMap insertedLinkMap = result.Result as LinkMap;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedLinkMap;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task<LinkMap> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<LinkMap>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                LinkMap linkMap = result.Result as LinkMap;
                if (linkMap != null)
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}", linkMap.PartitionKey, linkMap.RowKey, linkMap.LongLink);
                }

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return linkMap;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task DeleteEntityAsync(CloudTable table, LinkMap deleteEntity)
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                }

            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}
