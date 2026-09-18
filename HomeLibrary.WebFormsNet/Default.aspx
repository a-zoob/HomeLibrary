<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card shadow-sm border-0 mb-4">
        <div class="card-body p-4">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <div>
                    <h2 class="card-title h3 mb-1 text-dark fw-bold">Список книг в библиотеке</h2>
                    <p class="text-muted small mb-0">Управление книгами и оглавлениями (WebForms Legacy Client)</p>
                </div>
                <!-- Кнопка открытия модального окна Bootstrap -->
                <button type="button" class="btn btn-primary shadow-sm px-4" id="btnCreateBook" data-bs-toggle="modal" data-bs-target="#bookModal">
                    Добавить книгу
                </button>
            </div>

            <!-- Таблица списка книг -->
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0" id="booksTable">
                    <thead class="table-light">
                        <tr>
                           <%-- <th style="width: 8%">ID</th>--%>
                            <th style="width: 35%">Название</th>
                            <th style="width: 25%">Автор</th>
                            <th style="width: 12%">Год издания</th>
                            <th style="width: 20%" class="text-end">Действия</th>
                        </tr>
                    </thead>
                    <tbody id="booksTableBody">
                        <tr id="tableLoader">
                            <td colspan="5" class="text-center py-5">
                                <div class="spinner-border text-primary spinner-border-sm" role="status"></div>
                                <span class="ms-2 text-muted">Загрузка данных из API...</span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Блок пагинации -->
            <div class="d-flex justify-content-between align-items-center mt-4">
                <div class="text-muted small" id="paginationInfo">
                    Показано 0 из 0 записей
                </div>
                <nav aria-label="Навигация по страницам книг">
                    <ul class="pagination pagination-sm mb-0" id="paginationControls">
                        <!-- Кнопки пагинатора генерируются динамически -->
                    </ul>
                </nav>
            </div>
        </div>
    </div>
    <!-- Модальное окно добавления/редактирования книги -->
    <div class="modal fade" id="bookModal" tabindex="-1" aria-labelledby="bookModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title fw-bold" id="bookModalLabel">Добавить новую книгу</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Закрыть"></button>
                </div>
               
                <div id="bookFormContainer">
                    <div class="modal-body p-4">
                        <div class="row g-3">
                            <div class="col-md-8">
                                <label for="bookTitle" class="form-label text-secondary small fw-bold">Название книги</label>
                                <input type="text" class="form-control" id="bookTitle" required placeholder="Например: Песнь льда и пламени" />
                            </div>
                            <div class="col-md-4">
                                <label for="bookPublishingYear" class="form-label text-secondary small fw-bold">Год издания</label>
                                <input type="number" class="form-control" id="bookPublishingYear" required min="1" max="2026" placeholder="2026" />
                            </div>
                            <div class="col-12">
                                <label for="bookAuthor" class="form-label text-secondary small fw-bold">Автор</label>
                                <input type="text" class="form-control" id="bookAuthor" required placeholder="Роберт Мартин" />
                            </div>
                            <div class="col-12">
                                <label for="bookTocHtml" class="form-label text-secondary small fw-bold">Оглавление (Структура глав и разделов)</label>
                                <!-- Контрол HTML-редактора CKEditor -->
                                <textarea id="bookTocHtml" class="form-control"></textarea>
                                <div class="form-text text-muted small mt-2">
                                    Используйте маркеры списков на панели инструментов для формирования иерархии глав.
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer bg-light p-3">
                        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Отмена</button>
                        <button type="button" class="btn btn-primary px-4 shadow-sm" id="btnSaveBook">Сохранить</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
    <script>
        let currentPage = 1;
        const pageSize = 5;
        let editingBookId = null;
        let htmlEditor;

        function escapeHtml(str) {
            if (!str) return '';
            return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
        }

        function escapeXmlAttribute(str) {
            if (!str) return '';
            return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&apos;');
        }

        // Конвертер: HTML-списки -> XML-строка для API
        function convertHtmlToXml(html) {
            if (!html || html.trim() === '') {
                return '<?xml version="1.0"?><TableOfContents />';
            }
            const parser = new DOMParser();
            const doc = parser.parseFromString(`<div>${html}</div>`, 'text/html');
            let xmlResult = '<?xml version="1.0"?><TableOfContents>';
            const listItems = doc.querySelectorAll('div > ul > li, div > ol > li');
            
            listItems.forEach((li, index) => {
                const linkEl = li.querySelector('a');
                const urlAttr = linkEl ? ` url="${escapeXmlAttribute(linkEl.getAttribute('href'))}"` : '';
                let title = '';
                if (linkEl) {
                    title = linkEl.textContent.trim();
                } else {
                    const nodes = Array.from(li.childNodes);
                    let textParts = [];
                    for (let node of nodes) {
                        if (node.nodeName === 'UL' || node.nodeName === 'OL') break;
                        textParts.push(node.textContent);
                    }
                    title = textParts.join('').trim();
                }
                if (!title) title = `Глава ${index + 1}`;
                
                xmlResult += `<Chapter id="${index + 1}" title="${escapeXmlAttribute(title)}"${urlAttr} page="0">`;
                const subItems = li.querySelectorAll('ul > li, ol > li');
                subItems.forEach((subLi, subIndex) => {
                    const subLinkEl = subLi.querySelector('a');
                    const subUrlAttr = subLinkEl ? ` url="${escapeXmlAttribute(subLinkEl.getAttribute('href'))}"` : '';
                    const subTitle = subLinkEl ? subLinkEl.textContent.trim() : subLi.textContent.trim();
                    xmlResult += `<Section id="${index + 1}.${subIndex + 1}" title="${escapeXmlAttribute(subTitle)}"${subUrlAttr} page="0" />`;
                });
                xmlResult += '</Chapter>';
            });
            xmlResult += '</TableOfContents>';
            return xmlResult;
        }

        // Деконвертер: XML -> HTML-списки для редактора карточки
        function convertXmlToHtml(xmlString) {
            if (!xmlString || xmlString.includes('<TableOfContents />') || !xmlString.includes('<Chapter')) {
                return '<ul><li>Глава 1</li></ul>';
            }
            const parser = new DOMParser();
            const xmlDoc = parser.parseFromString(xmlString, 'text/xml');
            if (xmlDoc.getElementsByTagName('parsererror').length > 0) {
                return '<ul><li>Ошибка чтения структуры оглавления</li></ul>';
            }
            const chapters = xmlDoc.getElementsByTagName('Chapter');
            let htmlResult = '<ul>';
            Array.from(chapters).forEach(chapter => {
                const title = chapter.getAttribute('title') || '';
                const url = chapter.getAttribute('url');
                const content = url ? `<a href="${escapeHtml(url)}">${escapeHtml(title)}</a>` : escapeHtml(title);
                htmlResult += `<li><strong>${content}</strong>`;
                
                const sections = chapter.getElementsByTagName('Section');
                if (sections.length > 0) {
                    htmlResult += '<ul>';
                    Array.from(sections).forEach(section => {
                        const subTitle = section.getAttribute('title') || '';
                        const subUrl = section.getAttribute('url');
                        const subContent = subUrl ? `<a href="${escapeHtml(subUrl)}">${escapeHtml(subTitle)}</a>` : escapeHtml(subTitle);
                        htmlResult += `<li>${subContent}</li>`;
                    });
                    htmlResult += '</ul>';
                }
                htmlResult += '</li>';
            });
            htmlResult += '</ul>';
            return htmlResult;
        }

        // Загрузка данных из API
        async function loadBooks(page) {
            currentPage = page;
            const tableBody = document.getElementById('booksTableBody');
            tableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center py-5">
                        <div class="spinner-border text-primary spinner-border-sm" role="status"></div>
                        <span class="ms-2 text-muted">Загрузка данных из API...</span>
                    </td>
                </tr>`;

            try {
                const result = await HomeLibraryApi.getPaged(currentPage, pageSize);
                tableBody.innerHTML = ''; 

                if (!result.data || result.data.length === 0) {
                    tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">Библиотека пуста.</td></tr>`;
                    updatePaginationControls(0, 0);
                    return;
                }

                result.data.forEach(book => {
                    const row = document.createElement('tr');
                    // row.innerHTML = `
                    //     <td class="fw-bold text-secondary">${book.id}</td>
                    //     <td class="text-dark fw-semibold">${escapeHtml(book.title)}</td>
                    //     <td>${escapeHtml(book.author)}</td>
                    //     <td><span class="badge bg-light text-dark border p-2">${book.publishingYear}</span></td>
                    //     <td class="text-end">
                    //         <div class="btn-group btn-group-sm">
                    //             <button type="button" class="btn btn-outline-secondary" onclick="HomeLibraryApi.downloadToc(${book.id})">Скачать XML</button>
                    //             <button type="button" class="btn btn-outline-secondary" onclick="editBook(${book.id})">Изменить</button>
                    //             <button type="button" class="btn btn-outline-danger" onclick="deleteBook(${book.id})">Удалить</button>
                    //         </div>
                    //     </td>`;
                    row.innerHTML = `
    <td class="text-dark fw-semibold">${escapeHtml(book.title)}</td>
    <td>${escapeHtml(book.author)}</td>
    <td><span class="badge bg-light text-dark border p-2">${book.publishingYear}</span></td>
    <td class="text-end">
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-outline-secondary" onclick="HomeLibraryApi.downloadToc(${book.id})">Скачать XML</button>
            <button type="button" class="btn btn-outline-secondary" onclick="editBook(${book.id})">Изменить</button>
            <button type="button" class="btn btn-outline-danger" onclick="deleteBook(${book.id})">Удалить</button>
        </div>
    </td>`;
                    tableBody.appendChild(row);
                });

                updatePaginationControls(result.totalCount, result.totalPages);
            } catch (error) {
                tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-danger">Ошибка получения данных от бэкенда.</td></tr>`;
            }
        }
                function updatePaginationControls(totalCount, totalPages) {
            const info = document.getElementById('paginationInfo');
            const controls = document.getElementById('paginationControls');
            if (totalCount === 0) {
                info.textContent = 'Показано 0 из 0 записей';
                controls.innerHTML = '';
                return;
            }
            const startIdx = (currentPage - 1) * pageSize + 1;
            const endIdx = Math.min(currentPage * pageSize, totalCount);
            info.textContent = `Показано с ${startIdx} по ${endIdx} из ${totalCount} записей`;

            let html = `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}"><a class="page-link" href="#" onclick="${currentPage > 1 ? `loadBooks(\${currentPage - 1})` : ''}">Назад</a></li>`;
            for (let i = 1; i <= totalPages; i++) {
                html += `<li class="page-item ${i === currentPage ? 'active' : ''}"><a class="page-link" href="#" onclick="loadBooks(${i})">${i}</a></li>`;
            }
            html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}"><a class="page-link" href="#" onclick="${currentPage < totalPages ? `loadBooks(\${currentPage + 1})` : ''}">Вперед</a></li>`;
            controls.innerHTML = html;
        }

        // Сохранение и обновление данных карточки книги
        document.getElementById('btnSaveBook').addEventListener('click', async () => {
            const titleInput = document.getElementById('bookTitle');
            const authorInput = document.getElementById('bookAuthor');
            const yearInput = document.getElementById('bookPublishingYear');

            if (!titleInput.value.trim() || !authorInput.value.trim() || !yearInput.value) {
                alert('Пожалуйста, заполните все обязательные поля карточки!');
                return;
            }

            const btnSave = document.getElementById('btnSaveBook');
            btnSave.disabled = true;
            btnSave.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span>...';

            const bookData = {
                title: titleInput.value.trim(),
                author: authorInput.value.trim(),
                publishingYear: parseInt(yearInput.value),
                tableOfContentsXml: convertHtmlToXml(htmlEditor.getData())
            };

            try {
                if (editingBookId) {
                    await HomeLibraryApi.update(editingBookId, bookData);
                } else {
                    await HomeLibraryApi.create(bookData);
                }
                bootstrap.Modal.getInstance(document.getElementById('bookModal')).hide();
                loadBooks(currentPage);
            } catch (error) {
                alert('Ошибка взаимодействия с API бэкенда.');
            } finally {
                btnSave.disabled = false;
                btnSave.textContent = 'Сохранить';
            }
        });

        async function editBook(id) {
            try {
                const book = await HomeLibraryApi.getById(id);
                editingBookId = id;
                document.getElementById('bookModalLabel').textContent = 'Редактировать книгу #' + id;
                document.getElementById('bookTitle').value = book.title;
                document.getElementById('bookAuthor').value = book.author;
                document.getElementById('bookPublishingYear').value = book.publishingYear;
                htmlEditor.setData(convertXmlToHtml(book.tableOfContentsXml));
                new bootstrap.Modal(document.getElementById('bookModal')).show();
            } catch (error) {
                alert('Не удалось загрузить сведения о книге.');
            }
        }

        async function deleteBook(id) {
            if (!confirm(`Вы действительно хотите удалить книгу #${id}?`)) return;
            try {
                await HomeLibraryApi.delete(id);
                loadBooks(currentPage);
            } catch (error) {
                alert('Ошибка удаления книги.');
            }
        }

        // Инициализация CKEditor и запуск первой страницы
        document.addEventListener('DOMContentLoaded', () => {
            ClassicEditor
                .create(document.querySelector('#bookTocHtml'), {
                    toolbar: ['bold', 'italic', 'bulletedList', 'numberedList', 'undo', 'redo'],
                    language: 'ru'
                })
                .then(editor => {
                    htmlEditor = editor;
                    loadBooks(currentPage);
                })
                .catch(error => {
                    console.error('Ошибка инициализации CKEditor:', error);
                    loadBooks(currentPage);
                });

            // Сброс полей при закрытии модального окна
            document.getElementById('bookModal').addEventListener('hidden.bs.modal', () => {
                document.getElementById('bookTitle').value = '';
                document.getElementById('bookAuthor').value = '';
                document.getElementById('bookPublishingYear').value = '';
                if (htmlEditor) htmlEditor.setData('<ul><li>Глава 1</li></ul>');
                editingBookId = null;
                document.getElementById('bookModalLabel').textContent = 'Добавить новую книгу';
            });
        });
    </script>
</asp:Content>
