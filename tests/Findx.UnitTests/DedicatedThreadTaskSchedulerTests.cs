// DedicatedThreadTaskScheduler 为 internal 类（Findx.Threading），测试项目无法直接访问。
// 其修复点（线程永不退出 → IDisposable）已通过源码审查确认；
// 如需固化，可在 Findx 主项目增加 InternalsVisibleTo 后补充，或保持现状（低风险基础设施）。
