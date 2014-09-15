using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoConfiguration
{
    public class Configuration
    {
        /*private static readonly Dictionary<Type, Action<ILGenerator, string>> TypeConvertMappings = new Dictionary<Type, Action<ILGenerator, string>>() {
            { typeof(bool), (generator, s) => generator.Emit(Convert.ToBoolean(s) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0)},
            { typeof(byte), (generator, s) => generator.Emit(OpCodes.Ldc_I4, Convert.ToByte(s))},
            { typeof(int), (generator, s) => generator.Emit(OpCodes.Ldc_I4, Convert.().ToInt32(s))},
            { typeof(string), (generator, s) => generator.Emit(OpCodes.Ldstr, s)}, 
        };
        */

        /*internal static void SetValue(object obj, string fieldname, string value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Instance);

            var fieldType = field.FieldType;

            var convertMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Single(m => m.Name.StartsWith("To") && m.ReturnType == fieldType && m.GetParameters().Last().ParameterType == typeof(string));

            field.SetValue(obj, convertMethod.Invoke(null, new object[] { value }));
        }*/

        protected internal void SetValue(string fieldname, string value)
        {
            var type = this.GetType();
            var field = type.GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Instance);

            var fieldType = field.FieldType;
            
            var convertMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name.StartsWith("To") && m.ReturnType == fieldType && m.GetParameters().Last().ParameterType == typeof(string));

            field.SetValue(this, convertMethod.Invoke(null, new object[] { value }));
        }
    }

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
            
            // The first argument to the constructor is the instance
            var constructorIL = constructorBuilder.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);



            var interfaceProperties = typeof (T).GetProperties();

            foreach (var property in interfaceProperties)
            {
                GenerateProperty<T>(typeBuilder, constructorIL, property);
            }

            // Return the instance
            constructorIL.Emit(OpCodes.Ret);
            
            //_assemblyBuilder.Save("DynamicAutoConfiguration.dll");

            var generatedType = typeBuilder.CreateType();

            var instance = Activator.CreateInstance(generatedType);


            return (T) instance;
        }

        /*//TODO: decimal, unsigned forms http://en.wikipedia.org/wiki/List_of_CIL_instructions
         ;*/

        static readonly MethodInfo SetValueMethod = typeof(Configuration).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
        
        private static void GenerateProperty<T>(TypeBuilder typeBuilder, ILGenerator constructorIL, PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            var fieldName = "_" + property.Name;
            var fieldBuilder = typeBuilder.DefineField(
                fieldName,
                propertyType,
                FieldAttributes.Private);


            
            constructorIL.Emit(OpCodes.Ldstr, fieldName);
            constructorIL.Emit(OpCodes.Ldstr, ConfigurationManager.AppSettings[property.Name]);
            constructorIL.EmitCall(OpCodes.Call, SetValueMethod, null);
            
            

            /*constructorIL.Emit(OpCodes.Ldstr, ConfigurationManager.AppSettings[property.Name]);
            constructorIL.Emit(OpCodes.Stfld, fieldBuilder);*/


            
            /*constructorIL.Emit(OpCodes.Ldstr, fieldBuilder.Name);
            constructorIL.Emit(OpCodes.Ldstr, ConfigurationManager.AppSettings[property.Name]);
            constructorIL.Emit(OpCodes.Call, SetValueMethod);*/

            // Tell the constructor to initialise this field with the value from AppSettings
            //TypeStackMappings[propertyType](constructorIL, ConfigurationManager.AppSettings[property.Name]);
            
            /*constructorIL.Emit(OpCodes.Ldc_I4_1);
            
            if (propertyType == typeof(string))
                constructorIL.Emit(OpCodes.Ldstr, ConfigurationManager.AppSettings[property.Name]);

            if (propertyType == typeof(int))
                constructorIL.Emit(OpCodes.Ldc_I4_S, Convert.ToInt32(ConfigurationManager.AppSettings[property.Name]));*/
            
            

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
