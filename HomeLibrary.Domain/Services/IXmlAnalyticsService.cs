using System.Collections.Generic;

namespace HomeLibrary.Domain.Services
{
    public interface IXmlAnalyticsService
    {
        /// <summary>
        /// Возвращает список всех заголовков глав и секций из XML-строки.
        /// </summary>
        IEnumerable<string> ExtractAllTitles(string xmlContent);

        /// <summary>
        /// Находит номер страницы для главы по ее названию.
        /// </summary>
        int? FindPageByTitle(string xmlContent, string title);

        /// <summary>
        /// Формирует байтовый массив XML-файла с добавлением заголовков для скачивания.
        /// </summary>
        byte[] GenerateXmlFileBytes(string xmlContent);
    }
}
