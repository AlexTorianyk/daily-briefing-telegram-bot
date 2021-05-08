using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection
{
    public static class AutomaticDependencyInjectionExtension
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            var assemblies = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from referencedAssembly in assembly.GetReferencedAssemblies()
                where referencedAssembly.Name != null && referencedAssembly.Name.StartsWith("daily")
                select Assembly.Load(referencedAssembly)).ToList();

            assemblies.Add(Assembly.GetCallingAssembly());

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<ITransient>())
                .AsSelfWithInterfaces()
                .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<IScoped>())
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ISingleton>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());
        }
    }
}