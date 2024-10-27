namespace JSeekerBot;

[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public void InitializeEnvironment()
    {
        //string env = Environment.GetEnvironmentVariable("NHA_ENV");

        //Loads the .env environment variable file.  
        DotNetEnv.Env
            .TraversePath()
            .Load();

    }
}
