[assembly: WebJobsStartup(typeof(Startup))]
namespace Point.Azure_Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            
        }
    }
}
