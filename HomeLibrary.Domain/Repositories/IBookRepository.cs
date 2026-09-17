using System.Collections.Generic;
using System.Threading.Tasks;
using HomeLibrary.Domain.Entities;

namespace HomeLibrary.Domain.Repositories
{
    public interface IBookRepository
    {
        /// <summary>
        /// Возвращает постраничный список книг.
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Количество книг на одной странице.</param>
        Task<IEnumerable<Book>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Возвращает общее количество книг в базе данных.
        /// </summary>
        Task<int> GetTotalCountAsync();

       Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
    }
}
