// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using CleanAspire.Application.Pipeline;
using CleanAspire.Application.Features.Activities.Services;
using CleanAspire.Application.Features.Segments.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAspire.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FusionCacheBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FusionCacheRefreshBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MessageValidatorBehaviour<,>));
        services.AddMediator(options=>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // Activity Reminder Services
        services.AddScoped<IReminderService, ReminderService>();

        // Segment Engine Services
        services.AddScoped<ISegmentRuleEngine, SegmentRuleEngine>();
        services.AddScoped<ISegmentRebuilderService, SegmentRebuilderService>();

        return services;
    }
}

