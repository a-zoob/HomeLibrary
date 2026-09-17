using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeLibrary.Domain.Entities;
using HomeLibrary.Domain.Repositories;
using HomeLibrary.Infrastructure.Data;

namespace HomeLibrary.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        // Реализация постраничного вывода
        public async Task<IEnumerable<Book>> GetPagedAsync(int pageNumber, int pageSize)
        {
            // Защита от некорректных параметров
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            return await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title) // Сортировка обязательна перед использованием Skip/Take
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Получение общего количества для пагинатора на фронтенде
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Books.CountAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task AddAsync(Book book)
        {
            // если с фронтенда пришел 0, сбрасываем состояние ключа,
            // чтобы EF Core не пытался трекать дефолтный ноль как готовый первичный ключ
            if (book.Id == 0)
            {
                // Сообщаем EF Core, что ключ будет сгенерирован СУБД (IDENTITY)
                _context.Entry(book).Property(b => b.Id).IsModified = false;
            }

            _context.Books.Add(book);

            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Book book)
        {
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}
