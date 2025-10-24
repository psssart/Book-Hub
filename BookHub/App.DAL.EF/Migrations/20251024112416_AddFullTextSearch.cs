using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace App.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS unaccent;");
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""Books""
                ADD COLUMN IF NOT EXISTS ""search_vector"" tsvector NOT NULL DEFAULT ''::tsvector;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Authors""
                ADD COLUMN IF NOT EXISTS ""search_vector"" tsvector NOT NULL DEFAULT ''::tsvector;
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Books""
                SET ""search_vector"" =
                    setweight(to_tsvector('simple', unaccent(coalesce(""Tittle"", ''))), 'A') ||
                    setweight(to_tsvector('simple', unaccent(coalesce(""Description"", ''))), 'B');
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Authors""
                SET ""search_vector"" =
                    setweight(to_tsvector('simple', unaccent(coalesce(""Name"", ''))), 'A') ||
                    setweight(to_tsvector('simple', unaccent(coalesce(""Biography"", ''))), 'B');
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION books_search_vector_update() RETURNS trigger AS $$
                BEGIN
                    NEW.""search_vector"" :=
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Tittle"", ''))), 'A') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Description"", ''))), 'B');
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION authors_search_vector_update() RETURNS trigger AS $$
                BEGIN
                    NEW.""search_vector"" :=
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Name"", ''))), 'A') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Biography"", ''))), 'B');
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS books_search_vector_update_trg ON ""Books"";
                CREATE TRIGGER books_search_vector_update_trg
                BEFORE INSERT OR UPDATE OF ""Tittle"", ""Description""
                ON ""Books""
                FOR EACH ROW
                EXECUTE FUNCTION books_search_vector_update();
            ");

            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS authors_search_vector_update_trg ON ""Authors"";
                CREATE TRIGGER authors_search_vector_update_trg
                BEFORE INSERT OR UPDATE OF ""Name"", ""Biography""
                ON ""Authors""
                FOR EACH ROW
                EXECUTE FUNCTION authors_search_vector_update();
            ");
            
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_books_search_vector
                ON ""Books"" USING GIN (""search_vector"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_authors_search_vector
                ON ""Authors"" USING GIN (""search_vector"");
            ");
            
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_books_title_trgm
                ON ""Books"" USING GIN (""Tittle"" gin_trgm_ops);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_authors_name_trgm
                ON ""Authors"" USING GIN (""Name"" gin_trgm_ops);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_books_search_vector;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_authors_search_vector;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_books_title_trgm;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_authors_name_trgm;");
            
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS books_search_vector_update_trg ON ""Books"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS authors_search_vector_update_trg ON ""Authors"";");
            
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS books_search_vector_update();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS authors_search_vector_update();");
            
            migrationBuilder.Sql(@"ALTER TABLE ""Books"" DROP COLUMN IF EXISTS ""search_vector"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Authors"" DROP COLUMN IF EXISTS ""search_vector"";");
        }
    }
}
