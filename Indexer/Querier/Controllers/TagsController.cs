using System.Web.Http;
using System.Collections.Generic;
using wanshitong;
using System.Linq;
using Indexer.LuceneTools;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Indexer.Querier.Controllers
{
    public class TagsController : ApiController
    {
        private readonly Telemetry telemetry;


        public TagsController(Telemetry telemetry)
        {
            this.telemetry = telemetry;

        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public List<TagModel> Search(string text)
        {
            this.telemetry.client.TrackEvent("TagsController.Search");
            
            var searchResults = Program
                .m_luceneTools
                .SearchTag(text);

            return searchResults;            
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public void AddTag(string tag, string color)
        {
            this.telemetry.client.TrackEvent("TagsController.AddTag");

            Program
                .m_luceneTools
                .AddTag(tag, color);
        }
    } 
}