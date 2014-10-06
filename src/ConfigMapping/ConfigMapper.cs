using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Reflection;
using System.Reflection.Emit;

namespace ConfigMapping
{
    public static class ConfigMapper
    {
        private static readonly ConcurrentDictionary<Type, object> ExistingConfiguration;
        private static readonly ConcurrentDictionary<Type, object> ExistingConnectionStrings;

        static ConfigMapper()
        {
            ExistingConfiguration = new ConcurrentDictionary<Type, object>();
            ExistingConnectionStrings = new ConcurrentDictionary<Type, object>();
        }
 
        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the appSettings config section.
        /// </summary>
        public static T Map<T>()
        {
            return Lookup<T, Configuration>(ExistingConfiguration);
        }

        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the connectionStrings config section.
        /// </summary>
        public static T MapConnectionStrings<T>()
        {
            return Lookup<T, ConnectionStrings>(ExistingConnectionStrings);
        }

        private static TI Lookup<TI, TP>(ConcurrentDictionary<Type, object> collection)
        {
            var type = typeof(TI);

            return (TI)collection.GetOrAdd(type, t =>
            {
                var typeBuilder = GetTypeBuilder<TI, TP>();

                GenerateConstructor(typeBuilder);

                foreach (var property in typeof(TI).GetProperties())
                    GenerateProperty<TI>(typeBuilder, property);

                return Activator.CreateInstance(typeBuilder.CreateType());
            });
        }

        /// <summary>
        /// Repopulates all configuration objects with fresh values from the appSettings config section.
        /// </summary>
        public static void RefreshConfiguration()
        {
            RefreshConfigurationManager();

            foreach (var configuration in ExistingConfiguration)
                ((Configuration)configuration.Value).___InitialiseFieldValues();
        }

        /// <summary>
        /// Repopulates configuration objects of only the type provided with fresh values from the appSettings config section.
        /// <returns>
        /// The refreshed configuration of the given type
        /// </returns>
        /// </summary>
        public static T RefreshConfiguration<T>()
        {
            RefreshConfigurationManager();

            var type = typeof (T);

            if (!ExistingConfiguration.ContainsKey(type)) return Map<T>();

            var configuration = ExistingConfiguration[type];
            ((Configuration)configuration).___InitialiseFieldValues();
            return (T)configuration;
        }

        private static void RefreshConfigurationManager()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void GenerateConstructor(TypeBuilder typeBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            var initialiseMethod = typeBuilder.BaseType.GetMethod("___InitialiseFieldValues", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

            var constructorIL = constructorBuilder.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.EmitCall(OpCodes.Call, initialiseMethod, null);
            constructorIL.Emit(OpCodes.Ret);
        }

        private static void GenerateProperty<T>(TypeBuilder typeBuilder, PropertyInfo interfaceProperty)
        {
            const MethodAttributes getSetVirtualAttributes = MethodAttributes.Public |
                                                             MethodAttributes.SpecialName |
                                                             MethodAttributes.HideBySig |
                                                             MethodAttributes.Virtual;

            var propertyType = interfaceProperty.PropertyType;

            var fieldBuilder = typeBuilder.DefineField(
                fieldName: "_" + interfaceProperty.Name, 
                type: propertyType, 
                attributes: FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                name: interfaceProperty.Name, 
                attributes: PropertyAttributes.HasDefault,
                returnType: propertyType,
                parameterTypes: Type.EmptyTypes);

            var getAccessorName = "get_" + interfaceProperty.Name;

            var getAccessorMethodBuilder = typeBuilder.DefineMethod(
                name: getAccessorName, 
                attributes: getSetVirtualAttributes, 
                returnType: propertyType, 
                parameterTypes: Type.EmptyTypes);

            typeBuilder.DefineMethodOverride(getAccessorMethodBuilder, typeof(T).GetMethod(getAccessorName));

            var interfaceGetAccessor = typeof(T).GetMethod(getAccessorName);
            if (interfaceGetAccessor != null) typeBuilder.DefineMethodOverride(getAccessorMethodBuilder, interfaceGetAccessor);

            var getAccessorILGenerator = getAccessorMethodBuilder.GetILGenerator();
            getAccessorILGenerator.Emit(OpCodes.Ldarg_0);
            getAccessorILGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getAccessorILGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getAccessorMethodBuilder);

            var setAccessorName = "set_" + interfaceProperty.Name;
            var setAccessorMethodBuiler = typeBuilder.DefineMethod(
                name: setAccessorName, 
                attributes: getSetVirtualAttributes, 
                returnType: null, 
                parameterTypes: new [] { propertyType });

            var interfaceSetAccessor = typeof(T).GetMethod(setAccessorName);
            if (interfaceSetAccessor != null) typeBuilder.DefineMethodOverride(setAccessorMethodBuiler, interfaceSetAccessor);

            var setAccessorILGenerator = setAccessorMethodBuiler.GetILGenerator();
            setAccessorILGenerator.Emit(OpCodes.Ldarg_0);
            setAccessorILGenerator.Emit(OpCodes.Ldarg_1);
            setAccessorILGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            
            setAccessorILGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setAccessorMethodBuiler);
        }

        private static TypeBuilder GetTypeBuilder<TI,TP>()
        {
            var assemblyName = new AssemblyName("DynamicConfigMapping");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            return moduleBuilder.DefineType(
                name: "ConfigMapping", 
                attr: TypeAttributes.Public | TypeAttributes.Sealed, 
                parent: typeof(TP), 
                interfaces: new[] { typeof(TI) });
        }
    }
}
