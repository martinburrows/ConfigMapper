using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ConfigMapping
{
    internal class TypeGenerator<TI, TP>
    {
        public object Generate()
        {
            var typeBuilder = GetTypeBuilder();

            GenerateConstructor(typeBuilder);

            foreach (var property in typeof (TI).GetProperties())
                GenerateProperty(typeBuilder, property);

            return Activator.CreateInstance(typeBuilder.CreateType());
        }

        private static void GenerateConstructor(TypeBuilder typeBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            var initialiseMethod = typeBuilder.BaseType.GetMethod("___InitialiseFieldValues",
                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

            var constructorIL = constructorBuilder.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.EmitCall(OpCodes.Call, initialiseMethod, null);
            constructorIL.Emit(OpCodes.Ret);
        }

        private static void GenerateProperty(TypeBuilder typeBuilder, PropertyInfo interfaceProperty)
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

            typeBuilder.DefineMethodOverride(getAccessorMethodBuilder, typeof (TI).GetMethod(getAccessorName));

            var interfaceGetAccessor = typeof (TI).GetMethod(getAccessorName);
            if (interfaceGetAccessor != null)
                typeBuilder.DefineMethodOverride(getAccessorMethodBuilder, interfaceGetAccessor);

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
                parameterTypes: new[] {propertyType});

            var interfaceSetAccessor = typeof (TI).GetMethod(setAccessorName);
            if (interfaceSetAccessor != null)
                typeBuilder.DefineMethodOverride(setAccessorMethodBuiler, interfaceSetAccessor);

            var setAccessorILGenerator = setAccessorMethodBuiler.GetILGenerator();
            setAccessorILGenerator.Emit(OpCodes.Ldarg_0);
            setAccessorILGenerator.Emit(OpCodes.Ldarg_1);
            setAccessorILGenerator.Emit(OpCodes.Stfld, fieldBuilder);

            setAccessorILGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setAccessorMethodBuiler);
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var assemblyName = new AssemblyName("DynamicConfigMapping");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            return moduleBuilder.DefineType(
                name: "ConfigMapping",
                attr: TypeAttributes.Public | TypeAttributes.Sealed,
                parent: typeof (TP),
                interfaces: new[] {typeof (TI)});
        }
    }
}
