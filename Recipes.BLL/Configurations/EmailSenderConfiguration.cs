using Microsoft.Extensions.Configuration;

namespace Recipes.BLL.Configurations
{
    public class EmailSenderConfiguration
    {
        private readonly IConfiguration configuration;

        public string Name => configuration["EmailCompanyName"];
        public string Adress => configuration["EmailCompanyAdress"];
        public string Password => configuration["EmailCompanyPassword"];
        public string Host => configuration["EmailHost"];
        public int Port => int.TryParse(configuration["EmailHostPort"], out int port) ? port : throw new Exception("Invalid port configuration.");
        public bool UseSsl => bool.TryParse(configuration["EmailHostUseSsl"], out bool usessl) ? usessl : throw new Exception("Invalid port configuration.");



        public EmailSenderConfiguration(IConfiguration configuration) =>
            this.configuration = configuration;
    }
}
