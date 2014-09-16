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

        static ConfigMapper()
        {
            ExistingConfiguration = new ConcurrentDictionary<Type, object>();
        }
 
        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the appSettings config section.
        /// </summary>
        public static T Map<T>()
        {
            var type = typeof (T);

            return (T)ExistingConfiguration.GetOrAdd(type, t =>
            {
                if (ExistingConfiguration.ContainsKey(type))
                    return (T) ExistingConfiguration[type];

                var typeBuilder = GetTypeBuilder<T>();

                GenerateConstructor(typeBuilder);

                foreach (var property in typeof (T).GetProperties())
                    GenerateProperty<T>(typeBuilder, property);

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
        /// </summary>
        public static void RefreshConfiguration<T>()
        {
            RefreshConfigurationManager();

            var type = typeof (T);
            if (ExistingConfiguration.ContainsKey(type))
                ((Configuration)ExistingConfiguration[type]).___InitialiseFieldValues();
            else
                Map<T>();
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

            var initialiseMethod = typeof(Configuration).GetMethod("___InitialiseFieldValues", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

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
                returnType: typeof(string),
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

        private static TypeBuilder GetTypeBuilder<T>()
        {
            var assemblyName = new AssemblyName("DynamicConfigMapping");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            return moduleBuilder.DefineType(
                name: "ConfigMapping", 
                attr: TypeAttributes.Public | TypeAttributes.Sealed, 
                parent: typeof(Configuration), 
                interfaces: new[] { typeof(T) });
        }
    }
}
