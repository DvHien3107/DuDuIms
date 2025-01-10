using Promotion.Model.Model.Comon;

namespace PosAPI.Builder
{
    public static class ConfigServiceBuilder
    {
        public static void AddConfigService(this IServiceCollection services, IConfiguration configuration)
        {
            Const.POS_URL = configuration["Config:PosUrl"];
            Const.CHECKIN_URL = configuration["Config:CheckinUrl"];
        }
    }
}
