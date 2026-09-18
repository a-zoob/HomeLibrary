using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeLibrary.Infrastructure.Migrations
{
    public partial class AddStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Получение пагинированного списка
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_GetPagedBooks]
                    @PageNumber INT,
                    @PageSize INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF @PageNumber < 1 SET @PageNumber = 1;
                    IF @PageSize < 1 SET @PageSize = 10;

                    SELECT [Id], [Title], [Author], [PublishingYear], [TableOfContentsXml]
                    FROM [dbo].[Books]
                    ORDER BY [Title]
                    OFFSET (@PageNumber - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END;");

            // 2. Создание книги
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_InsertBook]
                        @Title NVARCHAR(250),
                        @Author NVARCHAR(150),
                        @PublishingYear INT,
                        @TableOfContentsXml XML,
                        @Id INT OUTPUT
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        BEGIN TRANSACTION;

                        BEGIN TRY
       
                            INSERT INTO [dbo].[Books] ([Title], [Author], [PublishingYear], [TableOfContentsXml])
                            VALUES (@Title, @Author, @PublishingYear, @TableOfContentsXml);

                            SET @Id = SCOPE_IDENTITY();

                           COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0
                            BEGIN
                                ROLLBACK TRANSACTION;
                            END;

                            SET @Id = NULL;

                            THROW;
                        END CATCH;
                    END;
                ");

            // 3. Редактирование книги
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_UpdateBook]
                    @Id INT,
                    @Title NVARCHAR(250),
                    @Author NVARCHAR(150),
                    @PublishingYear INT,
                    @TableOfContentsXml XML
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE [dbo].[Books]
                    SET [Title] = @Title,
                        [Author] = @Author,
                        [PublishingYear] = @PublishingYear,
                        [TableOfContentsXml] = @TableOfContentsXml
                    WHERE [Id] = @Id;
                END;");

            // 4. Удаление книги
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_DeleteBook]
                    @Id INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DELETE FROM [dbo].[Books]
                    WHERE [Id] = @Id;
                END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPagedBooks];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_InsertBook];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_UpdateBook];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_DeleteBook];");
        }
    }
}
