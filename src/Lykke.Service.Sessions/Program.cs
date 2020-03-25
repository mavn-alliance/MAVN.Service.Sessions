using Lykke.Sdk;
using System.Threading.Tasks;

namespace Lykke.Service.Sessions
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
#if DEBUG
            await LykkeStarter.Start<Startup>(true);
#else
            await LykkeStarter.Start<Startup>(false);
#endif
        }
    }
}