using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace HomeLibrary.Application.Helpers
{
    

    public static class XmlExtensions
    {
        private static readonly Regex EncodingRegex = new(@"encoding=[""'][^""']*[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const string DefaultXml = "<?xml version=\"1.0\"?><TableOfContents />";

        /// <summary>
        /// Очищает XML от деклараций кодировки, защищает от XXE-атак и гарантирует валидность структуры.
        /// </summary>
        public static string SanitizeAndValidateXml(this string? inputXml)
        {
            if (string.IsNullOrWhiteSpace(inputXml))
            {
                return DefaultXml;
            }

            // 1. Очищаем от декларации кодировки
            string cleanedXml = EncodingRegex.Replace(inputXml, "").Trim();

            if (string.IsNullOrWhiteSpace(cleanedXml) || cleanedXml == "<?xml version=\"1.0\"?>")
            {
                return DefaultXml;
            }

            try
            {
                // 2. защита от XML-бомб
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    IgnoreComments = true
                };

                using (var stringReader = new StringReader(cleanedXml))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    while (xmlReader.Read()) { } // Проверяем структуру документа
                }

                return cleanedXml;
            }
            catch (XmlException)
            {
                // В случае любой ошибки парсинга возвращаем дефолт
                return DefaultXml;
            }
        }
    }

}
