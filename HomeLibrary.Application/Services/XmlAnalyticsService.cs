using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml; 
using System.Xml.Linq;
using HomeLibrary.Domain.Services;

namespace HomeLibrary.Application.Services
{
    public class XmlAnalyticsService : IXmlAnalyticsService
    {
        public IEnumerable<string> ExtractAllTitles(string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent)) return Enumerable.Empty<string>();

            try
            {
                var doc = XDocument.Parse(xmlContent);

                var chapterTitles = doc.Descendants("Chapter").Select(x => x.Attribute("title")?.Value);
                var sectionTitles = doc.Descendants("Section").Select(x => x.Attribute("title")?.Value);

                return chapterTitles.Concat(sectionTitles).Where(t => t != null)!;
            }
            catch (XmlException) 
            {
                return Enumerable.Empty<string>();
            }
        }

        public int? FindPageByTitle(string xmlContent, string title)
        {
            if (string.IsNullOrWhiteSpace(xmlContent) || string.IsNullOrWhiteSpace(title)) return null;

            try
            {
                var doc = XDocument.Parse(xmlContent);

                var element = doc.Descendants()
                    .FirstOrDefault(x => string.Equals(x.Attribute("title")?.Value, title, StringComparison.OrdinalIgnoreCase));

                if (element != null && int.TryParse(element.Attribute("page")?.Value, out int page))
                {
                    return page;
                }
                return null;
            }
            catch (XmlException) 
            {
                return null;
            }
        }

        public byte[] GenerateXmlFileBytes(string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                xmlContent = "<?xml version=\"1.0\"?><TableOfContents />";
            }

            try
            {
                var doc = XDocument.Parse(xmlContent);
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, Encoding.UTF8);
                doc.Save(writer);
                writer.Flush();
                return ms.ToArray();
            }
            catch (XmlException) 
            {
                return Encoding.UTF8.GetBytes(xmlContent);
            }
        }
    }
}
