using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoConfiguration
{
    public class ConfigurationReader
    {
        private static AssemblyBuilder _assemblyBuilder;
        public static T Read<T>()
        {
            var typeBuilder = GetTypeBuilder<T>();

            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);
            
            var initialiseMethod =  typeof(Configuration).GetMethod("InitialiseFieldValues", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);

            var constructorIL = constructorBuilder.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.EmitCall(OpCodes.Call, initialiseMethod, null);
            constructorIL.Emit(OpCodes.Ret);

            var interfaceProperties = typeof (T).GetProperties();

            foreach (var property in interfaceProperties)
            {
                GenerateProperty<T>(typeBuilder, constructorIL, property);
            }

            var generatedType = typeBuilder.CreateType();
            return (T)Activator.CreateInstance(generatedType);
        }

        private static void GenerateProperty<T>(TypeBuilder typeBuilder, ILGenerator constructorIL, PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            var fieldName = "_" + property.Name;
            var fieldBuilder = typeBuilder.DefineField(
                fieldName,
                propertyType,
                FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                property.Name,
                PropertyAttributes.HasDefault,
                typeof(string),
                null);

            const MethodAttributes getSetVirtualAttributes = MethodAttributes.Public |
                                                             MethodAttributes.SpecialName | 
                                                             MethodAttributes.HideBySig | 
                                                             MethodAttributes.Virtual;

            var getAccessorName = "get_" + property.Name;

            var getAccessorMethodBuilder = typeBuilder.DefineMethod(
                getAccessorName,
                getSetVirtualAttributes,
                propertyType,
                Type.EmptyTypes);

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

            var setAccessorName = "set_" + property.Name;
            var setAccessorMethodBuiler = typeBuilder.DefineMethod(
                setAccessorName,
                getSetVirtualAttributes,
                null,
                new [] { propertyType });

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
            var assemblyName = new AssemblyName("DynamicAutoConfiguration");
            _assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);

            var moduleBuilder =
                _assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var typeBuilder = moduleBuilder.DefineType(
            "AutoConfiguration",
             TypeAttributes.Public, typeof(Configuration), new[] { typeof(T) });

            return typeBuilder;
        }
    }
}
