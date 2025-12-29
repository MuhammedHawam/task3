using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.Synergy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOpportunityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

update Opportunities set OpportunityTypeId = 1 where OpportunityTypeId = 3;


If exists(select * from OpportunityTypes where Name = 'Sponsorship' and id = 1)
begin
update OpportunityTypes set Name = 'Commercial Partnership' where id = 1
end 


If exists(select * from OpportunityTypes where Name = 'Commercial Collaboration' and id = 2)
begin
update OpportunityTypes set Name = 'Strategic Partnership' where id = 2
end 


delete from OpportunityTypes where id = 3



");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
