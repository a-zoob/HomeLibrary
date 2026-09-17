
const HomeLibraryApi = {
    // Читаем адрес из глобальной переменной
    // либо используем относительный путь по умолчанию, если клиент и api на одном хосте
    baseUrl: window.HOME_LIBRARY_API_URL || '/api/books',

    /**
     * Получить постраничный список книг
     */
    async getPaged(page = 1, pageSize = 10) {
        try {
            const response = await fetch(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
            if (!response.ok) throw new Error('Ошибка при получении списка книг');
            return await response.json();
        } catch (error) {
            console.error('API Error (getPaged):', error);
            throw error;
        }
    },

    /**
     * Получить книгу по ID
     * @param {number} id - Идентификатор книги
     */
    async getById(id) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}`);
            if (!response.ok) throw new Error(`Книга с ID ${id} не найдена`);
            return await response.json();
        } catch (error) {
            console.error('API Error (getById):', error);
            throw error;
        }
    },

    /**
     * Создать новую книгу
     * @param {Object} bookData - Объект книги { title, author, publishingYear, tableOfContentsXml }
     */
    async create(bookData) {
        try {
            const response = await fetch(this.baseUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(bookData)
            });
            if (!response.ok) throw new Error('Ошибка при создании книги');
            return await response.json();
        } catch (error) {
            console.error('API Error (create):', error);
            throw error;
        }
    },

    /**
     * Обновить данные книги
     * @param {number} id - Идентификатор книги
     * @param {Object} bookData - Обновленные данные книги
     */
    async update(id, bookData) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(bookData)
            });
            if (!response.ok) throw new Error(`Ошибка при обновлении книги с ID ${id}`);
            return true; // NoContent
        } catch (error) {
            console.error('API Error (update):', error);
            throw error;
        }
    },

    /**
     * Удалить книгу
     * @param {number} id - Идентификатор книги
     */
    async delete(id) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}`, {
                method: 'DELETE'
            });
            if (!response.ok) throw new Error(`Ошибка при удалении книги с ID ${id}`);
            return true; // NoContent
        } catch (error) {
            console.error('API Error (delete):', error);
            throw error;
        }
    },

    /**
     * Скачать оглавление в виде XML-файла
     * @param {number} id - Идентификатор книги
     * @param {string} bookTitle - Название книги для формирования имени файла (опционально)
     */
    /**
 * Скачать оглавление в виде XML-файла
 * @param {number} id - Идентификатор книги
 */
    /**
 * Скачать оглавление в виде XML-файла с автоматической расшифровкой кириллицы
 * @param {number} id - Идентификатор книги
 */
    async downloadToc(id) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/download-toc`);
            if (!response.ok) throw new Error('Не удалось скачать оглавление');

            let fileName = `book_${id}_toc.xml`; // Имя по умолчанию
            const disposition = response.headers.get('Content-Disposition');

            if (disposition) {

                const utf8FilenameRegex = /filename\*=\s*UTF-8''([^;\n]*)/i;
                const utf8Matches = utf8FilenameRegex.exec(disposition);

                if (utf8Matches && utf8Matches[1]) {
                    
                    fileName = decodeURIComponent(utf8Matches[1]);
                } else {
                    const normalFilenameRegex = /filename=\s*(?:(["'])(.*?)\1|([^;\n]*))/i;
                    const normalMatches = normalFilenameRegex.exec(disposition);
                    if (normalMatches) {
                        const rawName = normalMatches[2] || normalMatches[3];
                        if (rawName) {
                            fileName = rawName.trim();
                        }
                    }
                }
            }

            // Создаем Blob и скачиваем файл
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);

            const a = document.createElement('a');
            a.href = url;
            a.download = fileName; 

            document.body.appendChild(a);
            a.click();

            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('API Error (downloadToc):', error);
            alert('Произошла ошибка при скачивании файла оглавления.');
        }
    }


};
