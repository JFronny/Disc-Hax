using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Shared
{
    public static class HtmlProcessor
    {
        private static readonly HashSet<string> InlineTags = new HashSet<string>
        {
            //from https://developer.mozilla.org/en-US/docs/Web/HTML/Inline_elemente
            "b", "big", "i", "small", "tt", "abbr", "acronym",
            "cite", "code", "dfn", "em", "kbd", "strong", "samp",
            "var", "a", "bdo", "br", "img", "map", "object", "q",
            "script", "span", "sub", "sup", "button", "input", "label",
            "select", "textarea"
        };

        private static readonly HashSet<string> NonVisibleTags = new HashSet<string>
        {
            "script", "style"
        };

        public static string ToPlainText(string htmlDoc)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlDoc);
            StringBuilder builder = new StringBuilder();
            ToPlainTextState state = ToPlainTextState.StartLine;
            Plain(builder, ref state, new[] {doc.DocumentNode});
            return builder.ToString();
        }

        private static void Plain(StringBuilder builder, ref ToPlainTextState state, IEnumerable<HtmlNode> nodes)
        {
            foreach (HtmlNode node in nodes)
                if (node is HtmlTextNode)
                {
                    HtmlTextNode text = (HtmlTextNode) node;
                    char[] chars = HtmlEntity.DeEntitize(text.Text).ToCharArray();
                    foreach (char ch in chars)
                        if (char.IsWhiteSpace(ch))
                        {
                            if (ch == 0xA0 || ch == 0x2007 || ch == 0x202F)
                            {
                                if (state == ToPlainTextState.WhiteSpace)
                                    builder.Append(' ');
                                builder.Append(' ');
                                state = ToPlainTextState.NotWhiteSpace;
                            }
                            else
                            {
                                if (state == ToPlainTextState.NotWhiteSpace)
                                    state = ToPlainTextState.WhiteSpace;
                            }
                        }
                        else
                        {
                            if (state == ToPlainTextState.WhiteSpace)
                                builder.Append(' ');
                            builder.Append(ch);
                            state = ToPlainTextState.NotWhiteSpace;
                        }
                }
                else
                {
                    string tag = node.Name.ToLower();

                    if (tag == "br")
                    {
                        builder.AppendLine();
                        state = ToPlainTextState.StartLine;
                    }
                    else if (NonVisibleTags.Contains(tag))
                    {
                    }
                    else if (InlineTags.Contains(tag))
                        Plain(builder, ref state, node.ChildNodes);
                    else
                    {
                        if (state != ToPlainTextState.StartLine)
                        {
                            builder.AppendLine();
                            state = ToPlainTextState.StartLine;
                        }
                        Plain(builder, ref state, node.ChildNodes);
                        if (state != ToPlainTextState.StartLine)
                        {
                            builder.AppendLine();
                            state = ToPlainTextState.StartLine;
                        }
                    }
                }
        }

        private enum ToPlainTextState
        {
            StartLine = 0,
            NotWhiteSpace,
            WhiteSpace
        }
    }
}