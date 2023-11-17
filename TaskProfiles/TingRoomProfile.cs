using System;
using System.Collections.ObjectModel;
using System.Linq;
using CreeperX.Tasks;
using CreeperX.Utils;
using HtmlAgilityPack;

namespace CreeperX.TaskProfiles;

public class TingRoomProfile : CreeperProfile
{
    private static readonly string INDEX_URI = "http://jp.tingroom.com/rumen/zary/";
    private static readonly string PAGES_DICT_NAME = "pages";

    private static readonly Action<CreeperProfile, SearchPageTask, HtmlNode>
            navPageSearchRule = (profile, task, documentNode) =>
    {
        var navDiv = documentNode.Descendants("div").Where(
                node => node.IncludesClass("pages")).First();

        var visitedPages = profile.VisitedUris[PAGES_DICT_NAME];

        // Get pages which are not recoreded (not stored in the index dictionary)
        var newLinks = navDiv.GetChildrenLinks().Select(x => x.Attributes["href"].Value)
                .Distinct().Where(x => !visitedPages.ContainsKey(x)).ToList();

        foreach (var newLink in newLinks)
        {
            visitedPages.Add(newLink, false);
        }

        var newPageTasks = newLinks.Select(link => (CreeperTask) new SearchPageTask()
                {
                    PageUri = link,
                    Title = $"[Page] {link[link.LastIndexOf('/')..]}",
                    SearchRule = navPageSearchRule
                }).ToList();

        foreach (var newPageTask in newPageTasks)
        {
            task.AddSibling(profile, newPageTask);
        }
    };


    public TingRoomProfile(string workDirectory) : base(workDirectory)
    {
        // Initialize pages dictionary
        VisitedUris.Add(PAGES_DICT_NAME, new()
        {
            [INDEX_URI] = false
        });
    }

    protected override ObservableCollection<CreeperTask> GetInitialTasks()
    {
        var treeData = new ObservableCollection<CreeperTask>();

        var indexTask = new SearchPageTask()
        {
            PageUri = INDEX_URI,
            Title = "[Page] index",
            SearchRule = navPageSearchRule
        };

        treeData.Add(indexTask);

        return treeData;
    }
}
