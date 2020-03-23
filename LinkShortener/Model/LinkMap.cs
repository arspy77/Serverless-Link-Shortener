using System;
using System.Collections.Generic;
using System.Text;

namespace LinkShortener.Model
{
    using Microsoft.Azure.Cosmos.Table;

    public class LinkMap : TableEntity
    {
        public LinkMap()
        {
        }

        public LinkMap(string shortLink, string partitionKey = "Link")
        {
            PartitionKey = partitionKey;
            RowKey = shortLink;
        }

        public string LongLink { get; set; }
    }
}
