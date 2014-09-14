﻿using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoConfiguration
{
    public class ConfigurationReader
    {
        public static T Read<T>()
        {
            AssemblyName aName = new AssemblyName("DynamicAutoConfiguration");
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);

            // For a single-module assembly, the module name is usually 
            // the assembly name plus an extension.
            ModuleBuilder mb =
                ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            TypeBuilder tb = mb.DefineType(
            "AutoConfiguration",
             TypeAttributes.Public, typeof(object), new[] { typeof(T) });

            // Add a private field of type int (Int32).
            FieldBuilder fbString = tb.DefineField(
                "_test",
                typeof(string),
                FieldAttributes.Private);

            // Define a default constructor that supplies a default value 
            // for the private field. For parameter types, pass the empty 
            // array of types or pass null.
            ConstructorBuilder ctor0 = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            ILGenerator ctor0IL = ctor0.GetILGenerator();
            // For a constructor, argument zero is a reference to the new 
            // instance. Push it on the stack before pushing the default 
            // value on the stack, then call constructor ctor1.
            ctor0IL.Emit(OpCodes.Ldarg_0);
            //ctor0IL.Emit(OpCodes.Ldc_I4_S, 42);
            ctor0IL.Emit(OpCodes.Ldstr, "Hardcoded Test");
            ctor0IL.Emit(OpCodes.Stfld, fbString);
            ctor0IL.Emit(OpCodes.Ret);

            PropertyBuilder pbNumber = tb.DefineProperty(
            "Test",
            PropertyAttributes.HasDefault,
            typeof(string),
            null);

            // The property "set" and property "get" methods require a special
            // set of attributes.
            MethodAttributes getSetAttr = MethodAttributes.Public |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            // Define the "get" accessor method for Number. The method returns
            // an integer and has no arguments. (Note that null could be  
            // used instead of Types.EmptyTypes)
            MethodBuilder mbNumberGetAccessor = tb.DefineMethod(
                "get_Test",
                getSetAttr | MethodAttributes.Virtual,
                typeof(string),
                Type.EmptyTypes);

            tb.DefineMethodOverride(mbNumberGetAccessor, typeof(T).GetMethod("get_Test"));

            ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
            // For an instance property, argument zero is the instance. Load the  
            // instance, then load the private field and return, leaving the 
            // field value on the stack.
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fbString);
            numberGetIL.Emit(OpCodes.Ret);

            pbNumber.SetGetMethod(mbNumberGetAccessor);

            // Define the "set" accessor method for Number, which has no return 
            // type and takes one argument of type int (Int32).
            MethodBuilder mbNumberSetAccessor = tb.DefineMethod(
                "set_Test",
                getSetAttr | MethodAttributes.Virtual,
                null,
                new Type[] { typeof(string) });
            
            tb.DefineMethodOverride(mbNumberSetAccessor, typeof(T).GetMethod("set_Test"));

            ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
            // Load the instance and then the numeric argument, then store the 
            // argument in the field.
            numberSetIL.Emit(OpCodes.Ldarg_0);
            numberSetIL.Emit(OpCodes.Ldarg_1);
            numberSetIL.Emit(OpCodes.Stfld, fbString);
            numberSetIL.Emit(OpCodes.Ret);

            pbNumber.SetSetMethod(mbNumberSetAccessor);


            //            ab.Save(aName.Name + ".dll");

            Type t = tb.CreateType();



            // Create an instance of MyDynamicType using the default  
            // constructor.  
            T o1 = (T)Activator.CreateInstance(t);
            return o1;
        }
    }
}
