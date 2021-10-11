namespace sport_telegram_bot.Persistence
{
    public static class PostgresConnectionStringFactory
    {
        public static string GetConnectionStringFromUrl(string url)
        {
            url = url.Remove(0, 11);
            var userPass = url.Split("@")[0];
            var dbConf = url.Split("@")[1];
            var host = dbConf.Split(":")[0];
            var port = dbConf.Split(":")[1].Split("/")[0];
            var user = userPass.Split(":")[0];
            var password = userPass.Split(":")[1];
            var dbName = dbConf.Split(":")[1].Split("/")[1];
            return $"Host={host}; Port={port}; User Id={user}; Password={password}; Database={dbName}; SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}