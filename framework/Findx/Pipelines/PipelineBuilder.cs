using System.Threading.Tasks;

namespace Findx.Pipelines;

/// <summary>
///     管道构建者者
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class PipelineBuilder<TContext>
{
    private readonly PipelineInvokeDelegate<TContext> _fallbackHandler;
    private readonly List<Func<PipelineInvokeDelegate<TContext>, PipelineInvokeDelegate<TContext>>> _pipelines = [];

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="fallbackHandler"></param>
    public PipelineBuilder(IServiceProvider serviceProvider, PipelineInvokeDelegate<TContext> fallbackHandler)
    {
        ServiceProvider = serviceProvider;
        _fallbackHandler = fallbackHandler;
    }
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appServices"></param>
    public PipelineBuilder(IServiceProvider appServices) : this(appServices, _ => Task.CompletedTask)
    {
    }

    /// <summary>
    ///     获取服务提供者
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    ///     构建管道执行委托
    /// </summary>
    /// <returns></returns>
    public PipelineInvokeDelegate<TContext> Build()
    {
        var handler = _fallbackHandler;
        for (var i = _pipelines.Count - 1; i >= 0; i--)
        {
            handler = _pipelines[i](handler);
        }
        return handler;
    }
    
    
    /// <summary>
    ///     使用默认配制创建新的PipelineBuilder
    /// </summary>
    /// <returns></returns>
    public PipelineBuilder<TContext> New()
    {
        return new PipelineBuilder<TContext>(ServiceProvider, _fallbackHandler);
    }   
    
    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="handler"></param> 
    /// <returns></returns>
    public PipelineBuilder<TContext> When(Func<TContext, bool> predicate, PipelineInvokeDelegate<TContext> handler)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                await handler(context);
            }
            else
            {
                await next(context);
            }
        });
    }


    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
    public PipelineBuilder<TContext> When(Func<TContext, bool> predicate, Action<PipelineBuilder<TContext>> configureAction)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                var branchBuilder = New();
                configureAction(branchBuilder);
                await branchBuilder.Build().Invoke(context);
            }
            else
            {
                await next(context);
            }
        });
    }

    /// <summary>
    ///     使用管道中间件
    /// </summary>
    /// <typeparam name="TPipeline"></typeparam>
    /// <returns></returns>
    public PipelineBuilder<TContext> Use<TPipeline>() where TPipeline : IPipelineBehavior<TContext>
    {
        var middleware = ActivatorUtilities.GetServiceOrCreateInstance<TPipeline>(ServiceProvider);
        return Use(middleware);
    }

    /// <summary>
    /// 使用中间件
    /// </summary> 
    /// <typeparam name="TPipeline"></typeparam> 
    /// <param name="pipeline"></param>
    /// <returns></returns>
    public PipelineBuilder<TContext> Use<TPipeline>(TPipeline pipeline) where TPipeline : IPipelineBehavior<TContext>
    {
        return Use(pipeline.InvokeAsync);
    }

    /// <summary>
    /// 使用中间件
    /// </summary>  
    /// <param name="pipeline"></param>
    /// <returns></returns>
    public PipelineBuilder<TContext> Use(Func<TContext, PipelineInvokeDelegate<TContext>, Task> pipeline)
    {
        return Use(next => context => pipeline(context, next));
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <param name="middleware"></param>
    /// <returns></returns>
    public PipelineBuilder<TContext> Use(Func<PipelineInvokeDelegate<TContext>, PipelineInvokeDelegate<TContext>> middleware)
    {
        _pipelines.Add(middleware);
        return this;
    }
}