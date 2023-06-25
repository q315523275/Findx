using System.Data.Common;
using System.Threading.Tasks;
using Findx.Events;
using Findx.Exceptions;
using Findx.Utils;

namespace Findx.Data;

/// <summary>
/// 工作单元基类
/// </summary>
public abstract class UnitOfWorkBase: IUnitOfWork
{
    
    private readonly Stack<string> _transactionStack = new();

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    protected UnitOfWorkBase(IServiceProvider serviceProvider)
    {
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        Id = Guid.NewGuid();
        ServiceProvider = serviceProvider;
        UnitOfWorkEventDispatcher = new UnitOfWorkEventDispatcher(serviceProvider);
    }

    /// <summary>
    /// 事物事件调度者
    /// </summary>
    protected IUnitOfWorkEventDispatcher UnitOfWorkEventDispatcher { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    protected ILogger Logger { get; }
    
    /// <summary>
    /// 工作单元编号
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 服务提供器
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    /// 事物是否已提交
    /// </summary>
    public bool HasCommitted { get; private set; }
    
    /// <summary>
    /// 是否已启用事务
    /// </summary>
    public bool IsEnabledTransaction => _transactionStack.Count > 0;

    /// <summary>
    /// 连接信息
    /// </summary>
    public DbConnection Connection => Transaction?.Connection;
    
    /// <summary>
    /// 事物信息
    /// </summary>
    public DbTransaction Transaction { get; set; }
    
    /// <summary>
    /// 允许事物操作
    /// </summary>
    public void EnableTransaction()
    {
        var token = Guid.NewGuid().ToString();
        _transactionStack.Push(token);
        Logger.LogDebug("允许事务提交，标识：{Token}，当前总标识数：{TransactionStackCount}", token, _transactionStack.Count);
    }

    /// <summary>
    /// 开启或使用事物
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsEnabledTransaction || Transaction != null) return;
        
        await BeginTransactionAsync(cancellationToken);
        
        HasCommitted = false;
    }

    /// <summary>
    /// 开启具体事物
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存变更数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return InternalSaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// 内部保存变更数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task InternalSaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交事物
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (HasCommitted || Transaction?.Connection == null) return;

        string token;
        if (_transactionStack.Count > 1)
        {
            token = _transactionStack.Pop();
            Logger.LogDebug("跳过事务提交，标识：{Token}，当前剩余标识数：{TransactionStackCount}", token, _transactionStack.Count);
            return;
        }

        if (!IsEnabledTransaction)
            throw new FindxException("500", "执行 IUnitOfWork.CommitAsync() 之前，需要在事务开始时调用 IUnitOfWork.EnableTransaction()");

        token = _transactionStack.Pop();
        var transactionCode = Transaction.GetHashCode();

        await UnitOfWorkEventDispatcher.PublishEventsAsync(cancellationToken);

        await InternalCommitAsync(cancellationToken);

        await UnitOfWorkEventDispatcher.PublishAsyncEventsAsync(cancellationToken);
        
        Logger.LogDebug("提交事务，标识：{Token}，事务标识：{TransactionCode}", token, transactionCode);
  
        HasCommitted = true;
    }

    /// <summary>
    /// 内部事物提交
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task InternalCommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 事物回滚
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var transactionCode = Transaction?.GetHashCode();
        await InternalRollbackAsync(cancellationToken); 
        Logger.LogDebug("回滚事务，事务标识：{TransactionCode}", transactionCode);
        HasCommitted = true;
    }

    /// <summary>
    /// 事物回滚
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task InternalRollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加事件缓冲
    /// </summary>
    /// <param name="eventData"></param>
    /// <typeparam name="T"></typeparam>
    public void AddEventToBuffer<T>(T eventData) where T : IEvent
    {
        UnitOfWorkEventDispatcher.AddEventToBuffer(eventData);
    }
    
    private readonly AtomicInteger _disposeCounter = new();

    /// <summary>
    /// 资源释放
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposeCounter.IncrementAndGet() != 1) return;
        
        try
        {
            await InternalRollbackAsync();

            _transactionStack.Clear();
            UnitOfWorkEventDispatcher.ClearAllEvents();
            UnitOfWorkEventDispatcher = null;

            Logger.LogDebug("工作单元生命周期结束，单元标识：{Id}，释放计量：{DisposeCounter}", Id, _disposeCounter.Value);
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}