using Current.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260723154500_BackfillNotificationRelatedEntityId")]
    public partial class BackfillNotificationRelatedEntityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Notifications" AS notification
                SET "RelatedEntityId" = matched_transaction."Id"
                FROM (
                    SELECT
                        notification_row."Id" AS notification_id,
                        transaction_row."Id"
                    FROM "Notifications" AS notification_row
                    INNER JOIN LATERAL (
                        SELECT transaction_candidate."Id"
                        FROM "Transactions" AS transaction_candidate
                        WHERE transaction_candidate."Category" = 'Transfer'
                          AND ABS(EXTRACT(EPOCH FROM (transaction_candidate."CreatedAt" - notification_row."CreatedAt"))) < 5
                        ORDER BY ABS(EXTRACT(EPOCH FROM (transaction_candidate."CreatedAt" - notification_row."CreatedAt")))
                        LIMIT 1
                    ) AS transaction_row ON TRUE
                    WHERE notification_row."RelatedEntityId" IS NULL
                      AND notification_row."NotificationType" IN ('PaymentSent', 'PaymentReceived')
                ) AS matched_transaction
                WHERE notification."Id" = matched_transaction.notification_id;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Notifications"
                SET "RelatedEntityId" = NULL
                WHERE "NotificationType" IN ('PaymentSent', 'PaymentReceived');
                """);
        }
    }
}
