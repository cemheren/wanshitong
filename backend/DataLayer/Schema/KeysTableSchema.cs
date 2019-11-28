using Microsoft.Azure.Cosmos.Table;

namespace wanshitong.backend.datalayer.schema
{
    public class KeysTableEntity : TableEntity
    {
        public string Key {get; set;}

        public string Email {get; set;}

        public bool IsValid {get; set;}
    }
}