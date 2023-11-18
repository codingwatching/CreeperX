using System;
using System.Collections.ObjectModel;
using System.Linq;
using CreeperX.Tasks;
using CreeperX.Utils;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace CreeperX.Profiles;

public class TingRoomProfile : CreeperProfile
{
    private static readonly string INDEX_URI = "http://jp.tingroom.com/rumen/zary/";

    private static readonly string PAGES_DICT_NAME = "pages";
    private static readonly string ARTICLES_DICT_NAME = "articles";

    private static readonly Func<CreeperProfile, SearchPageTask, HtmlNode, List<CreeperTask>>
            pageListSearchRule = (profile, task, documentNode) =>
            {
                var recordedIndices = profile.TempEntryItems[PAGES_DICT_NAME];
                // Gather newly found index pages as siblings of this task
                var navDiv = documentNode.Descendants("div").Where(
                        node => node.IncludesClass("pages")).First();
                // Get pages which are not recoreded (not stored in the index dictionary)
                var newIndices = navDiv.GetChildrenLinks().Select(x => x.Attributes["href"].Value)
                        .Distinct().Where(x => !recordedIndices.ContainsKey(x)).ToList();

                foreach (var newIndex in newIndices)
                {
                    recordedIndices.Add(newIndex, null);
                }

                return newIndices.Select(link => (CreeperTask) new SearchPageTask()
                {
                    PageUri = link,
                    Title = $"[PageList] {link[link.LastIndexOf('/')..]}",
                    SearchSiblingsRule = pageListSearchRule,
                    SearchChildrenRule = articleSearchRule
                }).ToList();
            };

    private static readonly Func<CreeperProfile, SearchPageTask, HtmlNode, List<CreeperTask>>
            articleSearchRule = (profile, task, documentNode) =>
            {
                var recordedArticles = profile.EntryItems[ARTICLES_DICT_NAME];
                // Gather article pages on this page as children of this task
                var listElem = documentNode.Descendants("ul").Where(
                         x => x.IncludesClass("e2")).First();

                // Get pages which are not recoreded (not stored in the article dictionary)
                var newArticles = listElem.GetListItems().Select(x =>
                {
                    var linkElem = x.GetChildLink();

                    return (link: linkElem.Attributes["href"].Value, name: linkElem.InnerHtml);
                }).ToList();

                newArticles.ForEach(x => recordedArticles.TryAdd(x.link, x.name));

                return new(); // Don't return any child task
            };

    public TingRoomProfile(string workDirectory) : base(workDirectory)
    {
        // Initialize pages dictionary
        TempEntryItems.Add(PAGES_DICT_NAME, new()
        {
            [INDEX_URI] = null
        });

        // Initialize articles dictionary
        EntryItems.TryAdd(ARTICLES_DICT_NAME, new());
    }

    protected override ObservableCollection<CreeperTask> GetInitialTasks()
    {
        var treeData = new ObservableCollection<CreeperTask>();

        var indexTask = new SearchPageTask()
        {
            PageUri = INDEX_URI,
            Title = "[Page] index",
            SearchSiblingsRule = pageListSearchRule,
            SearchChildrenRule = articleSearchRule
        };

        treeData.Add(indexTask);

        return treeData;
    }
}
