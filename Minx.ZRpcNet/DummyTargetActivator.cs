using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Minx.ZRpcNet
{
    public static class DummyTargetActivator
    {
        private static AssemblyName AssemblyName { get; } = new AssemblyName($"ZRpcDynamicAssembly");

        private static AssemblyBuilder AssemblyBuilder;
        private static ModuleBuilder ModuleBuilder;

        private static Dictionary<Type, Type> DynamicTypes = new Dictionary<Type, Type>();

        public static T CreateInstance<T>() where T : class
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("Generic argument type T must be an interface.");
            }

            var backingType = GetDynamicType<T>();

            return (T)Activator.CreateInstance(backingType);
        }

        private static ModuleBuilder GetDynamicModule()
        {
            if (AssemblyBuilder == null)
            {
                AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
                ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name);
            }

            return ModuleBuilder;
        }

        private static Type GetDynamicType<T>() where T : class
        {
            lock (DynamicTypes)
            {
                if (DynamicTypes.TryGetValue(typeof(T), out var type))
                {
                    return type;
                }

                var moduleBuilder = GetDynamicModule();

                TypeBuilder typeBuilder = moduleBuilder.DefineType($"DynamicType_{typeof(T).GUID}", TypeAttributes.Public);
                typeBuilder.AddInterfaceImplementation(typeof(T));

                // Implement methods with empty bodies
                foreach (var method in typeof(T).GetMethods())
                {
                    // Check if the method is an event accessor
                    if (typeof(T).GetEvent(method.Name.Replace("add_", "").Replace("remove_", "")) != null
                        && (method.Name.StartsWith("add_") || method.Name.StartsWith("remove_")))
                    {
                        // Skip add and remove event methods.
                        continue;
                    }

                    var methodBuilder = typeBuilder.DefineMethod(
                        method.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        method.ReturnType,
                        GetParameterTypes(method));

                    var ilGen = methodBuilder.GetILGenerator();

                    if (method.ReturnType != typeof(void))
                    {
                        // For methods that return a value, we need to provide a default return.
                        // Here we are using default(T) for simplicity. For reference types and most primitives this results in null/zero.
                        EmitDefaultValueForType(ilGen, method.ReturnType);
                    }

                    ilGen.Emit(OpCodes.Ret);

                    typeBuilder.DefineMethodOverride(methodBuilder, method);
                }

                // Implement events with add/remove methods
                foreach (var evt in typeof(T).GetEvents())
                {
                    var eventBuilder = typeBuilder.DefineEvent(evt.Name, evt.Attributes, evt.EventHandlerType);

                    // Create the backing field for the event
                    var fieldBuilder = typeBuilder.DefineField($"{evt.Name}", evt.EventHandlerType, FieldAttributes.Private);

                    // Implement 'add' method for the event
                    MethodBuilder addMethodBuilder = typeBuilder.DefineMethod($"add_{evt.Name}",
                        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        null, new Type[] { evt.EventHandlerType });

                    ILGenerator ilAdd = addMethodBuilder.GetILGenerator();
                    ilAdd.Emit(OpCodes.Ldarg_0);                   // Load this onto the stack
                    ilAdd.Emit(OpCodes.Ldarg_0);                   // Load this again (for field access)
                    ilAdd.Emit(OpCodes.Ldfld, fieldBuilder);       // Load the current value of the field onto the stack
                    ilAdd.Emit(OpCodes.Ldarg_1);                   // Load the new delegate (argument) onto the stack
                    ilAdd.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Combine", new Type[] { typeof(Delegate), typeof(Delegate) }));
                    ilAdd.Emit(OpCodes.Castclass, evt.EventHandlerType);
                    ilAdd.Emit(OpCodes.Stfld, fieldBuilder);       // Store the combined delegate back into the field
                    ilAdd.Emit(OpCodes.Ret);

                    // Implement 'remove' method for the event
                    MethodBuilder removeMethodBuilder = typeBuilder.DefineMethod($"remove_{evt.Name}",
                        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        null, new Type[] { evt.EventHandlerType });

                    ILGenerator ilRemove = removeMethodBuilder.GetILGenerator();
                    ilRemove.Emit(OpCodes.Ldarg_0);                 // Load this onto the stack
                    ilRemove.Emit(OpCodes.Ldarg_0);                 // Load this again (for field access)
                    ilRemove.Emit(OpCodes.Ldfld, fieldBuilder);     // Load the current value of the field onto the stack
                    ilRemove.Emit(OpCodes.Ldarg_1);                 // Load the delegate to be removed (argument) onto the stack
                    ilRemove.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Remove", new Type[] { typeof(Delegate), typeof(Delegate) }));
                    ilRemove.Emit(OpCodes.Castclass, evt.EventHandlerType);
                    ilRemove.Emit(OpCodes.Stfld, fieldBuilder);     // Store the resulting delegate back into the field
                    ilRemove.Emit(OpCodes.Ret);

                    eventBuilder.SetAddOnMethod(addMethodBuilder);
                    eventBuilder.SetRemoveOnMethod(removeMethodBuilder);
                }

                var backingType = typeBuilder.CreateType();

                DynamicTypes.Add(typeof(T), backingType);

                return backingType;
            }
        }

        private static Type[] GetParameterTypes(MethodInfo method)
            => method.GetParameters().Select(p => p.ParameterType).ToArray();

        private static void EmitDefaultValueForType(ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                var local = ilGen.DeclareLocal(type);

                ilGen.Emit(OpCodes.Ldloca, local);
                ilGen.Emit(OpCodes.Initobj, type);
                ilGen.Emit(OpCodes.Ldloc, local);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldnull);
            }
        }
    }
}
