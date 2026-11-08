## 一致性哈希

ConsistentHash，主要用来解决分布式系统的数据分区问题；在服务发现的IpHash服务选择器中进行了使用。

> 使用示例

```csharp
var nodes = new ConsistentHash<string>();
nodes.Add("192.168.1.101");
nodes.Add("192.168.1.102");
nodes.Add("192.168.1.103");
nodes.Add("192.168.1.104");
nodes.Add("192.168.1.105");
nodes.Add("192.168.1.106");

// 节点集变化后必须重建虚拟节点环
nodes.RebuildKeys();

var dict = new ConcurrentDictionary<string, int>();
for (var i = 0; i < 100000; i++)
{
    var node = nodes.GetItemNode($"client-{i}");
    dict.AddOrUpdate(node, 1, (_, value) => value + 1);
}

foreach (var item in dict) Console.WriteLine($"{item.Key}:{item.Value}");
```

```text
// 冲突较少时结果
192.168.1.102:16119
192.168.1.103:16879
192.168.1.101:16543
192.168.1.104:16698
192.168.1.106:17201
192.168.1.105:16560
```

## 实现要点与并发语义

| 关注点 | 行为 |
|---|---|
| 节点维护 | 内部用 `HashSet<T>` 保存原始节点，`Add` / `Remove` 幂等，重复添加不会产生重复虚拟节点 |
| 环重建 | 节点集变化后调用 `RebuildKeys()` 全量重建虚拟节点环；重建代价与「节点数 × 副本数」成正比，只在节点集变化时触发，不要放进请求热路径 |
| 空环 | 环为空时 `GetItemNode` 抛 `InvalidOperationException`，明确失败而不是返回 null |
| 读性能 | 命中路径是「哈希 → 二分查找虚拟节点」，O(log n) |
| 并发 | 选择器（`IpHashSelector`）内部持有不可变 `RingSnapshot`（环 + 实例集 + 实例集合），节点集变化时双检锁重建并原子替换引用，稳态读路径**零锁** |

> 注意事项

- `GetItemNode` 是纯函数，同一 key 在环不变时永远映射到同一节点，可安全用于会话粘性/缓存分区。
- 环扩缩容会重排部分 key 的归属，客户端需容忍少量抖动；需要更均匀分布时增大虚拟节点副本数。
- 不要并发 `Add` / `Remove` 与 `GetItemNode`：一致性哈希本身不加锁，并发安全由调用方（快照替换）保证。

