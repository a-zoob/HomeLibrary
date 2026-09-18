//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using HomeLibrary.Domain.Entities;
//using HomeLibrary.Domain.Repositories;
//using HomeLibrary.Infrastructure.Data;

//namespace HomeLibrary.Infrastructure.Repositories
//{
//    public class BookRepository : IBookRepository
//    {
//        private readonly AppDbContext _context;

//        public BookRepository(AppDbContext context)
//        {
//            _context = context;
//        }

//        // Реализация постраничного вывода
//        public async Task<IEnumerable<Book>> GetPagedAsync(int pageNumber, int pageSize)
//        {
//            // Защита от некорректных параметров
//            if (pageNumber < 1) pageNumber = 1;
//            if (pageSize < 1) pageSize = 10;

//            return await _context.Books
//                .AsNoTracking()
//                .OrderBy(b => b.Title) // Сортировка обязательна перед использованием Skip/Take
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();
//        }

//        // Получение общего количества для пагинатора на фронтенде
//        public async Task<int> GetTotalCountAsync()
//        {
//            return await _context.Books.CountAsync();
//        }

//        public async Task<Book?> GetByIdAsync(int id)
//        {
//            return await _context.Books.FindAsync(id);
//        }

//        public async Task AddAsync(Book book)
//        {
//            // если с фронтенда пришел 0, сбрасываем состояние ключа,
//            // чтобы EF Core не пытался трекать дефолтный ноль как готовый первичный ключ
//            if (book.Id == 0)
//            {
//                // Сообщаем EF Core, что ключ будет сгенерирован СУБД (IDENTITY)
//                _context.Entry(book).Property(b => b.Id).IsModified = false;
//            }

//            _context.Books.Add(book);

//            await _context.SaveChangesAsync();
//        }
//        public async Task UpdateAsync(Book book)
//        {
//            _context.Entry(book).State = EntityState.Modified;
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteAsync(int id)
//        {
//            var book = await _context.Books.FindAsync(id);
//            if (book != null)
//            {
//                _context.Books.Remove(book);
//                await _context.SaveChangesAsync();
//            }
//        }
//    }
//}

using HomeLibrary.Domain.Entities;
using HomeLibrary.Domain.Repositories;
using HomeLibrary.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using System.Xml;

namespace HomeLibrary.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Получение пагинированного списка через хранимую процедуру
        public async Task<IEnumerable<Book>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var paramPageNumber = new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber };
            var paramPageSize = new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize };

            return await _context.Books
                .FromSqlRaw("EXEC [dbo].[sp_GetPagedBooks] @PageNumber, @PageSize", paramPageNumber, paramPageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        // 2. Получение общего количества
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Books.CountAsync();
        }

        // 3. Получение книги по ID
        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        // 4. Добавление новой книги с получением сгенерированного ID из процедуры
        public async Task AddAsync(Book book)
        {
            // 1. переводим строку в SqlXml (уже валидированную в API)
            var sqlXmlValue = new SqlXml(new XmlTextReader(new StringReader(book.TableOfContentsXml)));

            // 2. защита от инъекций
            var paramTitle = new SqlParameter("@Title", SqlDbType.NVarChar, 250) { Value = book.Title ?? (object)DBNull.Value };
            var paramAuthor = new SqlParameter("@Author", SqlDbType.NVarChar, 150) { Value = book.Author ?? (object)DBNull.Value };
            var paramPublishingYear = new SqlParameter("@PublishingYear", SqlDbType.Int) { Value = book.PublishingYear };
            var paramXml = new SqlParameter("@TableOfContentsXml", SqlDbType.Xml) { Value = sqlXmlValue };

            var paramId = new SqlParameter("@Id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_InsertBook] @Title, @Author, @PublishingYear, @TableOfContentsXml, @Id OUTPUT",
                paramTitle, paramAuthor, paramPublishingYear, paramXml, paramId);

            if (paramId.Value != null && paramId.Value != DBNull.Value)
            {
                book.Id = (int)paramId.Value;
            }
        }





        // 5. Редактирование книги через хранимую процедуру
        public async Task UpdateAsync(Book book)
        {
            var sqlXmlValue = new SqlXml(new XmlTextReader(new StringReader(book.TableOfContentsXml)));

            var paramId = new SqlParameter("@Id", SqlDbType.Int) { Value = book.Id };
            var paramTitle = new SqlParameter("@Title", SqlDbType.NVarChar, 250) { Value = book.Title ?? (object)DBNull.Value };
            var paramAuthor = new SqlParameter("@Author", SqlDbType.NVarChar, 150) { Value = book.Author ?? (object)DBNull.Value };
            var paramPublishingYear = new SqlParameter("@PublishingYear", SqlDbType.Int) { Value = book.PublishingYear };
            var paramXml = new SqlParameter("@TableOfContentsXml", SqlDbType.Xml) { Value = sqlXmlValue };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_UpdateBook] @Id, @Title, @Author, @PublishingYear, @TableOfContentsXml",
                paramId, paramTitle, paramAuthor, paramPublishingYear, paramXml);
        }

        // 6. Удаление книги через хранимую процедуру
        public async Task DeleteAsync(int id)
        {
            var paramId = new SqlParameter("@Id", SqlDbType.Int) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_DeleteBook] @Id",
                paramId);
        }
    }
}
