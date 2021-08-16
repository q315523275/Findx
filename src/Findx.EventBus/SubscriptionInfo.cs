using System;

namespace Findx.EventBus
{
    public class SubscribeInfo
    {
        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        private SubscribeInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public static SubscribeInfo Dynamic(Type handlerType)
        {
            return new SubscribeInfo(true, handlerType);
        }
        public static SubscribeInfo Typed(Type handlerType)
        {
            return new SubscribeInfo(false, handlerType);
        }
    }
}
