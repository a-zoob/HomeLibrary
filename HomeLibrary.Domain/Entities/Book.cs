using System;

namespace HomeLibrary.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublishingYear { get; set; }

        // Оглавление будет храниться в БД в виде XML-строки
        public string TableOfContentsXml { get; set; } = string.Empty;
    }
}
