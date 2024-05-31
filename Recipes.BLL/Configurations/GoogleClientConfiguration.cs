using Microsoft.Extensions.Configuration;

namespace Recipes.BLL.Configurations
{
    public class GoogleClientConfiguration
    {
        private readonly IConfiguration configuration;

        public string GoogleClientID => configuration["GoogleClientID"];
        public string GoogleClientSecret => configuration["GoogleClientSecret"];
        public GoogleClientConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
