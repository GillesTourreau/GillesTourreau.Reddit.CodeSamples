using Microsoft.Extensions.DependencyInjection;

namespace KeyedService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddKeyedSingleton<ICache, SmallCache>("Small");
            services.AddKeyedSingleton<ICache, BigCache>("Big");

            var sp = services.BuildServiceProvider();

            var s = sp.GetKeyedServices<ICache>();

            Console.WriteLine("Hello, World!");
        }
    }

    public class Service
    {

    }

    public interface ICache
    {
    }

    public interface BigCache : ICache
    {
    }

    public interface SmallCache : ICache
    {
    }
}
