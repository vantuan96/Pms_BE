using System.Data.Entity.Migrations;

namespace DataAccess.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<PMSContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            //AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(PMSContext context)
        {
        }
    }
}
