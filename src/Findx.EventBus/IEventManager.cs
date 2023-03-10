using System;
using System.Collections.Generic;
using Findx.EventBus.Attribute;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public interface IEventManager
    {
        /// <summary>
        /// 将事件源与事件处理器添加到存储
        /// </summary>
        /// <typeparam name="TEventData">事件源数据</typeparam>
        /// <typeparam name="TEventHandler">数据处理器</typeparam>
        void Add<TEventData, TEventHandler>() where TEventData : IEventData where TEventHandler : IEventHandler, new();
        
        /// <summary>
        /// 移除指定事件与事件处理器
        /// </summary>
        /// <typeparam name="TEventData"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        void Remove<TEventData, TEventHandler>() where TEventData : IEventData where TEventHandler : IEventHandler, new();
        
        /// <summary>
        /// 移除指定事件的所有处理器
        /// </summary>
        /// <typeparam name="TEventData"></typeparam>
        void RemoveAll<TEventData>() where TEventData : IEventData;

        /// <summary>
        /// 根据事件类型查询对应所有事件执行器
        /// </summary>
        /// <typeparam name="T">事件源数据</typeparam>
        /// <returns></returns>
        IEnumerable<IEventHandler> GetHandlersForEvent<T>() where T : IEventData;

        /// <summary>
        /// 根据事件名称查询对应所有事件执行器
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        IEnumerable<IEventHandler> GetHandlersForEvent(string eventName);

        /// <summary>
        /// 判断事件类型是否存在对应执行器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasSubscribeForEvent<T>() where T : IEventData;

        /// <summary>
        /// 判断事件名称是否存在对应执行器
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        bool HasSubscribeForEvent(string eventName);

        /// <summary>
        /// 根据名称查询事件类型
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        Type GetEventTypeByName(string eventName);
    }
}