using BasicAPI.Features.Commands;
using BasicAPI.Features.Queries;
using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI;

public static class BasicApiServiceExtensions
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddTransient<IQuery<GetAllDudes, Dude>, GetAllDudesQuery>();
        services.AddTransient<IQuery<GetDude, Dude>, GetDudeQuery>();
        services.AddTransient<ICommand<SetDude>, SetDudeCommand>();

        return services;
    }
}