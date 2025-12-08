using Mediator;

namespace MusicStreamingService.Commands;

public static class MediatorPipelineConfiguration
{
    public static IServiceCollection ConfigureMediatorPipelines(
        this IServiceCollection services) =>
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionalPipelineBehavior<,>));
}