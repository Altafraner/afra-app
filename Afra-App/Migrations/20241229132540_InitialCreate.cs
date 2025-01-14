using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Afra_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtiaKategories",
                columns: table => new
                {
                    Designation = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaKategories", x => x.Designation);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Otia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsCataloged = table.Column<bool>(type: "INTEGER", nullable: false),
                    KategoryDesignation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Otia_OtiaKategories_KategoryDesignation",
                        column: x => x.KategoryDesignation,
                        principalTable: "OtiaKategories",
                        principalColumn: "Designation",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Appendix = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    MentorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClassId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_People_People_MentorId",
                        column: x => x.MentorId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtiaRegularities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OtiumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    Start = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    End = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaRegularities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaRegularities_Otia_OtiumId",
                        column: x => x.OtiumId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaRegularities_People_TutorId",
                        column: x => x.TutorId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiumPerson",
                columns: table => new
                {
                    ManagedOtiaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ManagersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiumPerson", x => new { x.ManagedOtiaId, x.ManagersId });
                    table.ForeignKey(
                        name: "FK_OtiumPerson_Otia_ManagedOtiaId",
                        column: x => x.ManagedOtiaId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiumPerson_People_ManagersId",
                        column: x => x.ManagersId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonRole",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RolesId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRole", x => new { x.PersonId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PersonRole_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonRole_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaInstallments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OtiumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RegularityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsCanceled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaInstallments_OtiaRegularities_RegularityId",
                        column: x => x.RegularityId,
                        principalTable: "OtiaRegularities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OtiaInstallments_Otia_OtiumId",
                        column: x => x.OtiumId,
                        principalTable: "Otia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaInstallments_People_TutorId",
                        column: x => x.TutorId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtiaEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstallmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Start = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    End = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    AttendanceVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    MayEdit = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtiaEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtiaEnrollments_OtiaInstallments_InstallmentId",
                        column: x => x.InstallmentId,
                        principalTable: "OtiaInstallments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtiaEnrollments_People_StudentId",
                        column: x => x.StudentId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TutorId",
                table: "Classes",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Otia_KategoryDesignation",
                table: "Otia",
                column: "KategoryDesignation");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEnrollments_InstallmentId",
                table: "OtiaEnrollments",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaEnrollments_StudentId",
                table: "OtiaEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaInstallments_OtiumId",
                table: "OtiaInstallments",
                column: "OtiumId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaInstallments_RegularityId",
                table: "OtiaInstallments",
                column: "RegularityId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaInstallments_TutorId",
                table: "OtiaInstallments",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaRegularities_OtiumId",
                table: "OtiaRegularities",
                column: "OtiumId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiaRegularities_TutorId",
                table: "OtiaRegularities",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_OtiumPerson_ManagersId",
                table: "OtiumPerson",
                column: "ManagersId");

            migrationBuilder.CreateIndex(
                name: "IX_People_ClassId",
                table: "People",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_People_MentorId",
                table: "People",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRole_RolesId",
                table: "PersonRole",
                column: "RolesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_People_TutorId",
                table: "Classes",
                column: "TutorId",
                principalTable: "People",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_People_TutorId",
                table: "Classes");

            migrationBuilder.DropTable(
                name: "OtiaEnrollments");

            migrationBuilder.DropTable(
                name: "OtiumPerson");

            migrationBuilder.DropTable(
                name: "PersonRole");

            migrationBuilder.DropTable(
                name: "OtiaInstallments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "OtiaRegularities");

            migrationBuilder.DropTable(
                name: "Otia");

            migrationBuilder.DropTable(
                name: "OtiaKategories");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Classes");
        }
    }
}
