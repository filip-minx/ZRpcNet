using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace Minx.ZRpcNet
{
    public class EventInterceptor
    {
        private readonly Action<Type, EventInfo, object[]> handler;
        private readonly Type interceptedType;
        private readonly EventInfo eventInfo;

        private EventInterceptor(Action<Type, EventInfo, object[]> handler, Type interceptedType, EventInfo eventInfo)
        {
            this.handler = handler;
            this.interceptedType = interceptedType;
            this.eventInfo = eventInfo;
        }

        public static EventInterceptor Create(Type interceptedType, object targetInstance, EventInfo eventInfo, Action<Type, EventInfo, object[]> handler)
        {
            var interceptor = new EventInterceptor(handler, interceptedType, eventInfo);

            var dynamicHandler = CreateDynamicHandler(eventInfo, interceptor);

            if (dynamicHandler != null)
            {
                eventInfo.AddEventHandler(targetInstance, dynamicHandler);
            }

            return interceptor;
        }

        public static EventInterceptor[] CreateForAllEvents(Type interceptedType, object targetInstance, Action<Type, EventInfo, object[]> handler)
        {
            return targetInstance
                .GetType()
                .GetEvents()
                .Select(eventInfo => Create(interceptedType, targetInstance, eventInfo, handler))
                .ToArray();
        }

        private static Delegate CreateDynamicHandler(EventInfo eventInfo, EventInterceptor interceptor)
        {
            var eventParams = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();

            var handlerParameters = new Type[eventParams.Length + 1];

            handlerParameters[0] = typeof(EventInterceptor);

            for (int i = 0; i < eventParams.Length; i++)
            {
                handlerParameters[i + 1] = eventParams[i].ParameterType;
            }

            DynamicMethod method = new DynamicMethod("", null, handlerParameters, typeof(EventInterceptor).Module);

            ILGenerator ilGen = method.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldc_I4, eventParams.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));

            for (int i = 0; i < eventParams.Length; i++)
            {
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldarg, i + 1);

                if (eventParams[i].ParameterType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, eventParams[i].ParameterType);
                }

                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            ilGen.Emit(OpCodes.Call, typeof(EventInterceptor).GetMethod("InternalInvokeHandler",
                BindingFlags.Static | BindingFlags.NonPublic));

            ilGen.Emit(OpCodes.Ret);

            return method.CreateDelegate(eventInfo.EventHandlerType, interceptor);
        }

        internal static void InternalInvokeHandler(EventInterceptor interceptor, params object[] args)
        {
            interceptor.handler?.Invoke(interceptor.interceptedType, interceptor.eventInfo, args);
        }
    }
}
