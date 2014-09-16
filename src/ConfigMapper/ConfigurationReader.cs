using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ConfigMapper
{
    public class ConfigurationReader
    {
        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the appSettings config section.
        /// </summary>
        public static T Read<T>()
        {
            var typeBuilder = GetTypeBuilder<T>();

            GenerateConstructor(typeBuilder);

            foreach (var property in typeof (T).GetProperties())
                GenerateProperty<T>(typeBuilder, property);

            return (T)Activator.CreateInstance(typeBuilder.CreateType());
        }

        private static void GenerateConstructor(TypeBuilder typeBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            var initialiseMethod = typeof(Configuration).GetMethod("InitialiseFieldValues", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

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

            // For an instance property, argument zero is the instance. Load the  
            // instance, then load the private field and return, leaving the 
            // field value on the stack.
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
            // Load the instance and then the argument, then store the 
            // argument in the field.
            setAccessorILGenerator.Emit(OpCodes.Ldarg_0);
            setAccessorILGenerator.Emit(OpCodes.Ldarg_1);
            setAccessorILGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setAccessorILGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setAccessorMethodBuiler);
        }

        private static TypeBuilder GetTypeBuilder<T>()
        {
            var assemblyName = new AssemblyName("DynamicConfigMapper");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            return moduleBuilder.DefineType(
                name: "ConfigMapper", 
                attr: TypeAttributes.Public, 
                parent: typeof(Configuration), 
                interfaces: new[] { typeof(T) });
        }
    }
}
