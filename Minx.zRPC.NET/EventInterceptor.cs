using System;
using System.Reflection.Emit;
using System.Reflection;

namespace Minx.zRPC.NET
{
    public class EventInterceptor
    {
        private readonly Action<object[]> handler;

        private EventInterceptor(Action<object[]> handler)
        {
            this.handler = handler;
        }

        public static EventInterceptor Create(object targetInstance, Action<object[]> handler)
        {
            var interceptor = new EventInterceptor(handler);

            AttachDynamicHandlersToAllEvents(targetInstance, interceptor);

            return interceptor;
        }

        private static Delegate CreateDynamicHandler(EventInfo eventInfo, EventInterceptor logger)
        {
            var eventParams = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();

            Type[] handlerParameters = new Type[eventParams.Length + 1];
            handlerParameters[0] = typeof(EventInterceptor);
            for (int i = 0; i < eventParams.Length; i++)
            {
                handlerParameters[i + 1] = eventParams[i].ParameterType;
            }

            DynamicMethod method = new DynamicMethod("", null, handlerParameters, typeof(EventInterceptor).Module);

            ILGenerator ilGen = method.GetILGenerator();

            // Load the logger reference onto the stack.
            ilGen.Emit(OpCodes.Ldarg_0);

            // Create an object array.
            ilGen.Emit(OpCodes.Ldc_I4, eventParams.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));

            // Fill the array with the event's parameters.
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

            // Call the helper method.
            ilGen.Emit(OpCodes.Call, typeof(EventInterceptor).GetMethod("DynamicHandlerHelper",
                BindingFlags.Static | BindingFlags.NonPublic));

            ilGen.Emit(OpCodes.Ret);

            return method.CreateDelegate(eventInfo.EventHandlerType, logger);
        }

        private static void AttachDynamicHandlersToAllEvents(object instance, EventInterceptor interceptor)
        {
            foreach (EventInfo eventInfo in instance.GetType().GetEvents())
            {
                var dynamicHandler = CreateDynamicHandler(eventInfo, interceptor);

                if (dynamicHandler != null)
                {
                    eventInfo.AddEventHandler(instance, dynamicHandler);
                }
            }
        }

        private static void DynamicHandlerHelper(EventInterceptor loggerInstance, params object[] args)
        {
            loggerInstance.handler?.Invoke(args);
            // You can now use the eventName within this method for additional logic or logging.
            Console.WriteLine(loggerInstance);
        }
    }
}
