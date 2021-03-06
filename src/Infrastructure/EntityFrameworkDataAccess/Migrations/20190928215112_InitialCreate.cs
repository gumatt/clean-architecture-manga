// <copyright file="20190928215112_InitialCreate.cs" company="Ivan Paulovich">
// Copyright © Ivan Paulovich. All rights reserved.
// </copyright>

namespace Infrastructure.EntityFrameworkDataAccess.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Account",
                table => new {Id = table.Column<Guid>(), CustomerId = table.Column<Guid>()},
                constraints: table => { table.PrimaryKey("PK_Account", x => x.Id); });

            migrationBuilder.CreateTable(
                "Credit",
                table => new
                {
                    Id = table.Column<Guid>(),
                    Amount = table.Column<decimal>(),
                    TransactionDate = table.Column<DateTime>(),
                    AccountId = table.Column<Guid>()
                },
                constraints: table => { table.PrimaryKey("PK_Credit", x => x.Id); });

            migrationBuilder.CreateTable(
                "Customer",
                table => new {Id = table.Column<Guid>(), Name = table.Column<string>(), SSN = table.Column<string>()},
                constraints: table => { table.PrimaryKey("PK_Customer", x => x.Id); });

            migrationBuilder.CreateTable(
                "Debit",
                table => new
                {
                    Id = table.Column<Guid>(),
                    Amount = table.Column<decimal>(),
                    TransactionDate = table.Column<DateTime>(),
                    AccountId = table.Column<Guid>()
                },
                constraints: table => { table.PrimaryKey("PK_Debit", x => x.Id); });

            migrationBuilder.InsertData(
                "Account",
                new[] {"Id", "CustomerId"},
                new object[]
                {
                    new Guid("4c510cfe-5d61-4a46-a3d9-c4313426655f"),
                    new Guid("197d0438-e04b-453d-b5de-eca05960c6ae")
                });

            migrationBuilder.InsertData(
                "Credit",
                new[] {"Id", "AccountId", "Amount", "TransactionDate"},
                new object[]
                {
                    new Guid("f5117315-e789-491a-b662-958c37237f9b"),
                    new Guid("4c510cfe-5d61-4a46-a3d9-c4313426655f"), 400m,
                    new DateTime(2019, 9, 28, 21, 51, 12, 605, DateTimeKind.Utc).AddTicks(4990)
                });

            migrationBuilder.InsertData(
                "Customer",
                new[] {"Id", "Name", "SSN"},
                new object[] {new Guid("197d0438-e04b-453d-b5de-eca05960c6ae"), "Test User", "19860817-9999"});

            migrationBuilder.InsertData(
                "Debit",
                new[] {"Id", "AccountId", "Amount", "TransactionDate"},
                new object[]
                {
                    new Guid("3d6032df-7a3b-46e6-8706-be971e3d539f"),
                    new Guid("4c510cfe-5d61-4a46-a3d9-c4313426655f"), 400m,
                    new DateTime(2019, 9, 28, 21, 51, 12, 605, DateTimeKind.Utc).AddTicks(5890)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Account");

            migrationBuilder.DropTable(
                "Credit");

            migrationBuilder.DropTable(
                "Customer");

            migrationBuilder.DropTable(
                "Debit");
        }
    }
}
