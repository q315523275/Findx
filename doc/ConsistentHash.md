## 一致性哈希

ConsistentHash，主要用来解决分布式系统的数据分区问题；在服务发现的IpHash服务选择器中进行了使用。

> 使用示例

```js
// 一致哈希
var nodes = new ConsistentHash<string>();
nodes.Add("192.168.1.101");
nodes.Add("192.168.1.102");
nodes.Add("192.168.1.103");
nodes.Add("192.168.1.104");
nodes.Add("192.168.1.105");
nodes.Add("192.168.1.106");
var dict = new ConcurrentDictionary<string, int>();
for (var i = 0; i < 100000; i++)
{
    var node = nodes.GetItemNode("127.0.0.1"); // 指定固定内容
    dict.AddOrUpdate(node, 1, (_, value) => value + 1);
}
foreach (var item in dict)
{
    Console.WriteLine($"{item.Key}:{item.Value}");
}
```
```js
// 打印结果
Hello, World!
192.168.1.101:100000
```

