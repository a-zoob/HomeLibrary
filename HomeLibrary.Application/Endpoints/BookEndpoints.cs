using System.Text.RegularExpressions;
using HomeLibrary.Domain.Entities;
using HomeLibrary.Domain.Repositories;
using HomeLibrary.Domain.Services;

namespace HomeLibrary.Application.Endpoints
{
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/books");

            // 1. Постраничный вывод книг
            group.MapGet("/", async (int? page, int? pageSize, IBookRepository repository) =>
            {
                int actualPage = page ?? 1;
                int actualPageSize = pageSize ?? 10;

                var books = await repository.GetPagedAsync(actualPage, actualPageSize);
                var totalCount = await repository.GetTotalCountAsync();

                return Results.Ok(new
                {
                    Data = books,
                    TotalCount = totalCount,
                    Page = actualPage,
                    PageSize = actualPageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / actualPageSize)
                });
            });

            // 2. Получение книги по ID
            group.MapGet("/{id:int}", async (int id, IBookRepository repository) =>
            {
                var book = await repository.GetByIdAsync(id);
                return book is not null ? Results.Ok(book) : Results.NotFound($"Книга с ID {id} не найдена.");
            });

            // 3. Создание новой книги
            group.MapPost("/", async (Book book, IBookRepository repository) =>
            {
                // Защита от передачи "0" во внутренний трекер
                book.Id = 0;

                if (string.IsNullOrWhiteSpace(book.TableOfContentsXml))
                {
                    book.TableOfContentsXml = "<?xml version=\"1.0\"?><TableOfContents />";
                }
                else
                {
                    // Удаляем декларацию кодировки (encoding="..."), если пользователь вставил её вручную
                    book.TableOfContentsXml = Regex.Replace(book.TableOfContentsXml, @"encoding=[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
                }

                await repository.AddAsync(book);
                return Results.Created($"/api/books/{book.Id}", book);
            });

            // 4. Редактирование книги
            group.MapPut("/{id:int}", async (int id, Book updatedBook, IBookRepository repository) =>
            {
                var existingBook = await repository.GetByIdAsync(id);
                if (existingBook is null) return Results.NotFound($"Книга с ID {id} не найдена.");

                existingBook.Title = updatedBook.Title;
                existingBook.Author = updatedBook.Author;
                existingBook.PublishingYear = updatedBook.PublishingYear;
                if (string.IsNullOrWhiteSpace(updatedBook.TableOfContentsXml))
                {
                    existingBook.TableOfContentsXml = "<?xml version=\"1.0\"?><TableOfContents />";
                }
                else
                {
                    // Точно так же защищаем операцию обновления от ошибок ручного ввода XML
                    existingBook.TableOfContentsXml = Regex.Replace(updatedBook.TableOfContentsXml, @"encoding=[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
                }
               
                await repository.UpdateAsync(existingBook);
                return Results.NoContent();
            });

            // 5. Удаление книги
            group.MapDelete("/{id:int}", async (int id, IBookRepository repository) =>
            {
                var existingBook = await repository.GetByIdAsync(id);
                if (existingBook is null) return Results.NotFound($"Книга с ID {id} не найдена.");

                await repository.DeleteAsync(id);
                return Results.NoContent();
            });

            // 6. Скачивание оглавления книги в виде файла XML
            group.MapGet("/{id:int}/download-toc", async (int id, IBookRepository repository, IXmlAnalyticsService xmlService, HttpContext context) =>
            {
                var book = await repository.GetByIdAsync(id);
                if (book is null) return Results.NotFound($"Книга с ID {id} не найдена.");

                byte[] fileBytes = xmlService.GenerateXmlFileBytes(book.TableOfContentsXml);
                string fileName = $"{book.Author}_{book.Title}.xml";

                context.Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");

                return Results.File(fileBytes, "application/xml", fileName);
            });

        }
    }
}
