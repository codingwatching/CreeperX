using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CreeperX.Utils;

public static class HtmlNodeExtensions
{
    public static bool IncludesClass(this HtmlNode node, string className)
    {
        if (!node.Attributes.Contains("class"))
        {
            return false;
        }

        return node.Attributes["class"].Value.Split(' ').Contains(className);
    }

    public static HtmlNode GetChildLink(this HtmlNode node)
    {
        return node.Descendants("a").First();
    }

    public static List<HtmlNode> GetChildrenLinks(this HtmlNode node)
    {
        return node.Descendants("a").ToList();
    }

    public static List<HtmlNode> GetListItems(this HtmlNode node)
    {
        return node.Descendants("li").ToList();
    }
}
