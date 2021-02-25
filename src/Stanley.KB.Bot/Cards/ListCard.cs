using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Cards
{
    public class ListCard
    {
        public const string ContentType = "application/vnd.microsoft.teams.card.list";

        public Content content { get; set; }

        public ListCard(string title = null, IList<Item> items = null, IList<CardAction> buttons = null)
        {
            content = new Content
            {
                title = title,
                items = items,
                buttons = buttons
            };
        }
    }

    public class Content
    {
        public string title { get; set; }
        public IList<Item> items { get; set; }
        public IList<CardAction> buttons { get; set; }
    }

    public class Item
    {
        public string type { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public string subtitle { get; set; }
        public CardAction tap { get; set; }
        public string icon { get; set; }

        public Item(string id = null, string type = null, string title = null, string subtitle = null, string icon = null, CardAction tap = null)
        {
            this.id = id;
            this.type = type;
            this.title = title;
            this.subtitle = subtitle;
            this.icon = icon;
            this.tap = tap;
        }
    }

    public class ItemTypes
    {
        public const string File = "file";
        public const string ResultItem = "resultItem";
        public const string Section = "section";
    }

    public static class ListCardExtensions
    {
        public static Attachment ToAttachment(this ListCard card)
        {
            return new Attachment
            {
                ContentType = ListCard.ContentType,
                Content = card.content
            };
        }
    }
}
