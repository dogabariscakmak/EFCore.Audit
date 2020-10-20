using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EFCore.Audit.IntegrationTest.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditMetaDatas",
                columns: table => new
                {
                    HashPrimaryKey = table.Column<Guid>(nullable: false),
                    SchemaTable = table.Column<string>(nullable: false),
                    ReadablePrimaryKey = table.Column<string>(nullable: true),
                    Schema = table.Column<string>(nullable: true),
                    Table = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditMetaDatas", x => new { x.HashPrimaryKey, x.SchemaTable });
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OldValues = table.Column<string>(nullable: true),
                    NewValues = table.Column<string>(nullable: true),
                    DateTimeOffset = table.Column<DateTimeOffset>(nullable: false),
                    EntityState = table.Column<int>(nullable: false),
                    ByUser = table.Column<string>(nullable: true),
                    AuditMetaDataHashPrimaryKey = table.Column<Guid>(nullable: true),
                    AuditMetaDataSchemaTable = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_AuditMetaDatas_AuditMetaDataHashPrimaryKey_AuditMetaDataSchemaTable",
                        columns: x => new { x.AuditMetaDataHashPrimaryKey, x.AuditMetaDataSchemaTable },
                        principalTable: "AuditMetaDatas",
                        principalColumns: new[] { "HashPrimaryKey", "SchemaTable" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    PostalCode = table.Column<int>(nullable: false),
                    Street = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => new { x.PersonId, x.Type });
                    table.ForeignKey(
                        name: "FK_Addresses_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonAttributes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attribute = table.Column<int>(nullable: false),
                    AttributeValue = table.Column<string>(nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    DummyString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonAttributes_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditMetaDataHashPrimaryKey_AuditMetaDataSchemaTable",
                table: "Audits",
                columns: new[] { "AuditMetaDataHashPrimaryKey", "AuditMetaDataSchemaTable" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonAttributes_PersonId",
                table: "PersonAttributes",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "PersonAttributes");

            migrationBuilder.DropTable(
                name: "AuditMetaDatas");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
