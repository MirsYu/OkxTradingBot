using Microsoft.Extensions.Configuration;
using System.IO;

namespace OkxTradingBot.Core.Config
{
    /// <summary>
    /// 配置模块
    /// 加载API密钥和其他参数
    /// </summary>
    public static class AppConfig
    {
        private static readonly IConfigurationRoot Configuration;

        static AppConfig()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static string GetApiKey() => Configuration["Okx:ApiKey"] ?? "";
        public static string GetSecretKey() => Configuration["Okx:SecretKey"] ?? "";
        public static string GetPassphrase() => Configuration["Okx:Passphrase"] ?? "";
    }
}
