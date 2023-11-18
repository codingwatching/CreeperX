using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreeperX.Requests;
using CreeperX.Profiles;
using HtmlAgilityPack;

namespace CreeperX.Tasks;

public class SearchPageTask : CreeperTask
{
    public string PageUri
    {
        get; set;
    }

    public Action<CreeperProfile, SearchPageTask, HtmlNode> SearchRule
    {
        get; set;
    }

    public override string GetInfo() => $"Search Page: {PageUri}";

    public async override Task<bool> Run(CreeperProfile profile)
    {
        if (Status != CreeperTaskStatus.Pending)
        {
            return false;
        }

        Status = CreeperTaskStatus.Running;

        var succeeded = true;

        var pageResp = await HttpRequestHelper.GetUri(PageUri);
        if (pageResp.IsSuccessStatusCode) // HTTP 200, OK
        {
            if (SearchRule is not null)
            {
                var document = new HtmlDocument();
                document.LoadHtml(await pageResp.Content.ReadAsStringAsync());

                SearchRule.Invoke(profile, this, document.DocumentNode);
            }
        }
        else // Failed to get a valid response
        {
            succeeded = false;
        }

        // Call base Run() method to run children tasks
        succeeded = succeeded && await base.Run(profile);

        if (succeeded)
        {
            Status = CreeperTaskStatus.Succeeded;
            return true;
        }
        else
        {
            Status = CreeperTaskStatus.Failed;
            return false;
        }
    }

    public override string ToString()
    {
        return $"{GetType().Name}\n{PageUri}";
    }
}
