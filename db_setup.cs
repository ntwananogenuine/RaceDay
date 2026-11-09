using Microsoft.EntityFrameworkCore;

namespace raceday_api.database
{//start of namespace
    //links the api to the RaceDayDB database made by RaceDay.sql
    public static class db_setup
    {//start of class

        public static void link_database(AppDbContext db, ILogger logger)
        {//start of method

            //the unit tests use an in-memory database, only do this for SQL Server
            if (!db.Database.IsSqlServer()) return;

            //if RaceDayDB does not exist yet, create it with all the tables
            //if it already exists (made by RaceDay.sql) this does nothing
            db.Database.EnsureCreated();

            //RaceDay.sql does not have the columns for event type, distance and the two image names
            //(Azure Blob Storage) so add them when they are missing
            db.Database.ExecuteSqlRaw(@"
                IF COL_LENGTH('dbo.Events', 'EventType') IS NULL
                    ALTER TABLE dbo.Events ADD EventType VARCHAR(20) NOT NULL
                        CONSTRAINT DF_Events_EventType DEFAULT 'Run';
                IF COL_LENGTH('dbo.Events', 'DistanceKm') IS NULL
                    ALTER TABLE dbo.Events ADD DistanceKm DECIMAL(5,2) NULL;
                IF COL_LENGTH('dbo.Events', 'BannerImage') IS NULL
                    ALTER TABLE dbo.Events ADD BannerImage VARCHAR(255) NULL;
                IF COL_LENGTH('dbo.Users', 'ProfilePicture') IS NULL
                    ALTER TABLE dbo.Users ADD ProfilePicture VARCHAR(255) NULL;");

            //show in the console which database the api is connected to
            var connection = db.Database.GetDbConnection();
            logger.LogInformation("Connected to {server} / {database}", connection.DataSource, connection.Database);

        }//end of method

    }//end of class
}//end of namespace
