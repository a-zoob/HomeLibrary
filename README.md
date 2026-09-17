1. Настройте подключение к Sql server: проект HomeLibrary.Application - appsettings.json - секция "ConnectionStrings" - "SqlServer".
2. Если понадобится, перенастройте порты для подключения к Api:
      - проект HomeLibrary.MvcWeb - appsettings.json - секция "ApiSettings" - BaseUrl - изменить номер порта;
      - проект HomeLibrary.WebFormsNet - web.config - секция appSettings - ключ ApiSettings:BaseUrl - изменить номер порта.
