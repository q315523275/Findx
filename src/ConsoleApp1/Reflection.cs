using BenchmarkDotNet.Attributes;
using Findx.Extensions;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
namespace ConsoleApp1
{
    public class Reflection
    {
        // 将反射找到的方法创建一个委托。
        Func<int, int> func = InstanceMethodBuilder<int, int>.CreateInstanceMethod(new StubClass(), typeof(StubClass).GetMethod(nameof(StubClass.Test), new[] { typeof(int) }));
        Func<object, object[], object> func5 = Findx.Reflection.FastInvokeHandler.Create(typeof(StubClass).GetMethod(nameof(StubClass.Test), new[] { typeof(int) }));

        [Benchmark]
        public int OneCreateInstanceMethod()
        {
            return func(1);
        }
        [Benchmark]
        public int OneFastInvokeHandler()
        {
            return func5.Invoke(1, new object[] { }).CastTo<int>();
        }
        [Benchmark]
        public int CreateInstanceMethod()
        {
            // 调用的目标实例。
            StubClass instance2 = new StubClass();
            // 使用反射找到的方法。
            MethodInfo method2 = typeof(StubClass).GetMethod(nameof(StubClass.Test), new[] { typeof(int) });
            var func2 = InstanceMethodBuilder<int, int>.CreateInstanceMethod(instance2, method2);
            return func2(1);
        }
        [Benchmark]
        public int FastInvokeHandler()
        {
            // 调用的目标实例。
            StubClass instance3 = new StubClass();
            // 使用反射找到的方法。
            MethodInfo method3 = typeof(StubClass).GetMethod(nameof(StubClass.Test), new[] { typeof(int) });

            var func3 = Findx.Reflection.FastInvokeHandler.Create(method3);

            return func3.Invoke(1, new object[] { }).CastTo<int>();
        }
    }
    public class StubClass
    {
        public int Test(int i)
        {
            return i;
        }
    }
    public static class InstanceMethodBuilder<T, TReturnValue>
    {
        /// <summary>
        /// 调用时就像 var result = func(t)。
        /// </summary>
        [Pure]
        public static Func<T, TReturnValue> CreateInstanceMethod<TInstanceType>(TInstanceType instance, MethodInfo method)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (method == null) throw new ArgumentNullException(nameof(method));

            return (Func<T, TReturnValue>)method.CreateDelegate(typeof(Func<T, TReturnValue>), instance);
        }

        /// <summary>
        /// 调用时就像 var result = func(this, t)。
        /// </summary>
        [Pure]
        public static Func<TInstanceType, T, TReturnValue> CreateMethod<TInstanceType>(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return (Func<TInstanceType, T, TReturnValue>)method.CreateDelegate(typeof(Func<TInstanceType, T, TReturnValue>));
        }
    }
}
