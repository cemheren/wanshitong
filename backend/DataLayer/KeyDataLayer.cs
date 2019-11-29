using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using wanshitong.backend.datalayer.schema;

namespace wanshitong.backend.datalayer
{
    public class KeyDataLayer
    {
        const string TableName = "keystable";
        private CloudTableClient tableClient;
        private CloudTable tableReference;

        public KeyDataLayer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=wanshitong;AccountKey=UdNcvY+yLzJ51TnbdCKLNKfdvR7FXWkuk55NS6IMGjkf2NGqCzAdFF7xoU7QPnXVibfeAQXdRJ7syBnXvn3Q2Q==;EndpointSuffix=core.windows.net");

            // Create a table client for interacting with the table service
            this.tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            this.tableReference = this.tableClient.GetTableReference(TableName);
        }

        /// Create a new Valid key, put it in the database 
        public async Task<string> CreateNewValidKey(string emailAddress, ILogger log)
        {
            try
            {
                var newkey = Guid.NewGuid().ToString();
                var keyEntity = new KeysTableEntity
                {
                    PartitionKey = newkey,
                    RowKey = emailAddress,
                    Key = newkey,
                    IsValid = true,
                    Email = emailAddress
                };

                var insertOrMergeOperation = TableOperation.InsertOrMerge(keyEntity);

                // Execute the operation.
                TableResult result = await this.tableReference.ExecuteAsync(insertOrMergeOperation);
        
                return newkey;
            }
            catch (System.Exception e)
            {
                log.LogError(e, "New key creation failed");
                throw;
            }
        }

        public async Task<KeysTableEntity> GetKey(string key, ILogger log)
        {
            try
            {
                var query = new TableQuery<KeysTableEntity>().Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        key
                    )
                );

                var results = this.tableReference.ExecuteQuery(query).ToList();

                if(results.Count != 1)
                {
                    throw new Exception("The key was not found");
                }

                return results[0];
            }
            catch (System.Exception e)
            {
                log.LogError(e, $"key validation failed {key}");
                throw;
            }
        }
    }
}