using BasicAPI.Features;
using CQRSToolkit.DependencyInjection;

namespace CQRSToolkit;

public static class CQRSToolkit
{
    public static WebApplication CreateCQRSToolkitApp()
            => WebApplication
                .CreateBuilder(args)
                .RegisterServices()
                .UseAuthorization()
                .MapControllers()
                .Build()
  //TODO: Swagger
                .Run();

    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddControllers();
        webApplicationBuilder.Services.AddEndpointsApiExplorer();
        webApplicationBuilder.Services.AddSwaggerGen();
        webApplicationBuilder.Services.AddThemBois();
        return webApplicationBuilder;
    }


    //.UseSwagger
    //.UseSwaggerUI
/*    public static WebApplication ConfigureEnvironment(this WebApplicationBuilder webApplicationBuilder)
    {

    }
*/

}

