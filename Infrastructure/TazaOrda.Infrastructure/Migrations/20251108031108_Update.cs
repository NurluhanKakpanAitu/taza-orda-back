using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TazaOrda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventParticipants_CoinTransactions_CoinTransactionId",
                table: "EventParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Districts_DistrictId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_OrganizerId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_Category",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_IsCompleted",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_OrganizerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MinParticipants",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Report",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "OrganizerNotes",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "ParticipantNotes",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "EventParticipants");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Events",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DistrictId",
                table: "Events",
                newName: "district_id");

            migrationBuilder.RenameColumn(
                name: "CoordinatesLongitude",
                table: "Events",
                newName: "lng");

            migrationBuilder.RenameColumn(
                name: "CoordinatesLatitude",
                table: "Events",
                newName: "lat");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "Events",
                newName: "start_at");

            migrationBuilder.RenameColumn(
                name: "RewardCoins",
                table: "Events",
                newName: "coin_reward");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Events",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Events",
                newName: "cover_url");

            migrationBuilder.RenameColumn(
                name: "EndDateTime",
                table: "Events",
                newName: "end_at");

            migrationBuilder.RenameIndex(
                name: "IX_Events_StartDateTime",
                table: "Events",
                newName: "IX_Events_start_at");

            migrationBuilder.RenameIndex(
                name: "IX_Events_Name",
                table: "Events",
                newName: "IX_Events_Title");

            migrationBuilder.RenameIndex(
                name: "IX_Events_IsActive_StartDateTime",
                table: "Events",
                newName: "IX_Events_is_active_start_at");

            migrationBuilder.RenameIndex(
                name: "IX_Events_IsActive",
                table: "Events",
                newName: "IX_Events_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_Events_DistrictId",
                table: "Events",
                newName: "IX_Events_district_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "EventParticipants",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CoinsAwarded",
                table: "EventParticipants",
                newName: "coins_awarded");

            migrationBuilder.RenameColumn(
                name: "CoinTransactionId",
                table: "EventParticipants",
                newName: "coin_transaction_id");

            migrationBuilder.RenameColumn(
                name: "RegisteredAt",
                table: "EventParticipants",
                newName: "joined_at");

            migrationBuilder.RenameColumn(
                name: "ParticipatedAt",
                table: "EventParticipants",
                newName: "coins_awarded_at");

            migrationBuilder.RenameColumn(
                name: "ConfirmedAt",
                table: "EventParticipants",
                newName: "checked_in_at");

            migrationBuilder.RenameIndex(
                name: "IX_EventParticipants_Status",
                table: "EventParticipants",
                newName: "IX_EventParticipants_status");

            migrationBuilder.RenameIndex(
                name: "IX_EventParticipants_CoinTransactionId",
                table: "EventParticipants",
                newName: "IX_EventParticipants_coin_transaction_id");

            migrationBuilder.AddColumn<string>(
                name: "PhotoAfterUrl",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_joined_at",
                table: "EventParticipants",
                column: "joined_at");

            migrationBuilder.AddForeignKey(
                name: "FK_EventParticipants_CoinTransactions_coin_transaction_id",
                table: "EventParticipants",
                column: "coin_transaction_id",
                principalTable: "CoinTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Districts_district_id",
                table: "Events",
                column: "district_id",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventParticipants_CoinTransactions_coin_transaction_id",
                table: "EventParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Districts_district_id",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventParticipants_joined_at",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "PhotoAfterUrl",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Events",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "district_id",
                table: "Events",
                newName: "DistrictId");

            migrationBuilder.RenameColumn(
                name: "lng",
                table: "Events",
                newName: "CoordinatesLongitude");

            migrationBuilder.RenameColumn(
                name: "lat",
                table: "Events",
                newName: "CoordinatesLatitude");

            migrationBuilder.RenameColumn(
                name: "start_at",
                table: "Events",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "end_at",
                table: "Events",
                newName: "EndDateTime");

            migrationBuilder.RenameColumn(
                name: "cover_url",
                table: "Events",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "coin_reward",
                table: "Events",
                newName: "RewardCoins");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Events",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Events_Title",
                table: "Events",
                newName: "IX_Events_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Events_start_at",
                table: "Events",
                newName: "IX_Events_StartDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Events_is_active_start_at",
                table: "Events",
                newName: "IX_Events_IsActive_StartDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Events_is_active",
                table: "Events",
                newName: "IX_Events_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Events_district_id",
                table: "Events",
                newName: "IX_Events_DistrictId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "EventParticipants",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "coins_awarded",
                table: "EventParticipants",
                newName: "CoinsAwarded");

            migrationBuilder.RenameColumn(
                name: "coin_transaction_id",
                table: "EventParticipants",
                newName: "CoinTransactionId");

            migrationBuilder.RenameColumn(
                name: "joined_at",
                table: "EventParticipants",
                newName: "RegisteredAt");

            migrationBuilder.RenameColumn(
                name: "coins_awarded_at",
                table: "EventParticipants",
                newName: "ParticipatedAt");

            migrationBuilder.RenameColumn(
                name: "checked_in_at",
                table: "EventParticipants",
                newName: "ConfirmedAt");

            migrationBuilder.RenameIndex(
                name: "IX_EventParticipants_status",
                table: "EventParticipants",
                newName: "IX_EventParticipants_Status");

            migrationBuilder.RenameIndex(
                name: "IX_EventParticipants_coin_transaction_id",
                table: "EventParticipants",
                newName: "IX_EventParticipants_CoinTransactionId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinParticipants",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerId",
                table: "Events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Report",
                table: "Events",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Events",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "EventParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizerNotes",
                table: "EventParticipants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantNotes",
                table: "EventParticipants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "EventParticipants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "EventParticipants",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Category",
                table: "Events",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsCompleted",
                table: "Events",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerId",
                table: "Events",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventParticipants_CoinTransactions_CoinTransactionId",
                table: "EventParticipants",
                column: "CoinTransactionId",
                principalTable: "CoinTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Districts_DistrictId",
                table: "Events",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_OrganizerId",
                table: "Events",
                column: "OrganizerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
