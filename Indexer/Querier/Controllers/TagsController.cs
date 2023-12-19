using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using Microsoft.AspNetCore.Mvc;
using wanshitong.Common.Lucene;

namespace Indexer.Querier.Controllers
{
    public class TagsController : ApiController
    {
        private readonly Telemetry telemetry;
        private readonly LuceneClient luceneClient;

        public TagsController(Telemetry telemetry, LuceneClient luceneClient)
        {
            this.telemetry = telemetry;
            this.luceneClient = luceneClient;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<TagModel> Search(string text)
        {
            this.telemetry.client.TrackEvent("TagsController.Search");
            
            var searchResults = this.luceneClient.SearchTag(text);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public void AddTag(string tag, string color)
        {
            this.telemetry.client.TrackEvent("TagsController.AddTag");

            this.luceneClient.AddTag(tag, color);
        }
    } 
}