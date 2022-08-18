using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Point.Azure_Functions;

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
