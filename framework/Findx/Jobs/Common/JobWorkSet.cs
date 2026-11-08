using System.Collections.Concurrent;

namespace Findx.Jobs.Common;

/// <summary>
///     调度任务工作集合
/// </summary>
public class JobWorkSet : ConcurrentDictionary<string, Type>;