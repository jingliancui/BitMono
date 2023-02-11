using AsmResolver.DotNet.Signatures.Types;

namespace BitMono.Protections;

public class MethodArgsToArglist : IProtection
{
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        Console.WriteLine("FLAG 1");

        var factory = context.Module.CorLibTypeFactory;
        var systemObject = factory.Object.ToTypeDefOrRef();
        var systemObjectArray = factory.Object.MakeSzArrayType();
        var systemInt32 = factory.Int32;
        var argIterator = context.Importer.ImportType(typeof(ArgIterator)).ToTypeSignature(isValueType: true);
        var argIteratorCtor = context.Importer.ImportMethod(typeof(ArgIterator).GetConstructor(new Type[]
        {
            typeof(RuntimeArgumentHandle)
        }));
        var getRemainingCount = context.Importer.ImportMethod(typeof(ArgIterator).GetMethod(nameof(ArgIterator.GetRemainingCount)));
        var getNextArg =
            context.Importer.ImportMethod(typeof(ArgIterator).GetMethod(nameof(ArgIterator.GetNextArg),
                Array.Empty<Type>()));
        var typedReferenceToObject =
            context.Importer.ImportMethod(typeof(TypedReference).GetMethod(nameof(TypedReference.ToObject)));

        var method1 = parameters.Members.OfType<MethodDefinition>().FirstOrDefault(m => m.Name == "DrawArgList");
        Console.WriteLine(Helper.ReflectObject(method1.Signature.ParameterTypes.GetType(), method1.Signature.ParameterTypes,
            "parameterTypes"));
        var methods = parameters.Members.OfType<MethodDefinition>().Where(methodsFilter);
        Console.WriteLine("FLAG 2: " + methods.Count());
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                var instructions = body.Instructions;
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.OpCode == CilOpCodes.Call && instruction.Operand is IMethodDescriptor methodDescriptor)
                    {
                        var callingMethod = methodDescriptor.Resolve();
                        if (callingMethod != null)
                        {
                            callingMethod.Signature.ParameterTypes.Clear();
                            var memberRef =
                                callingMethod.CreateMemberReference(callingMethod.Name, callingMethod.Signature).ImportWith(context.Importer);

                            //body.ComputeMaxStackOnBuild = false;
                            //body.InitializeLocals = true;
                            method.Signature.IncludeSentinel = true;
                            method.Signature.IsSentinel = true;
                            method.Signature.Attributes = CallingConventionAttributes.VarArg;
                            method.Signature.SentinelParameterTypes.Add(factory.Boolean);
                            instruction.Operand = memberRef;
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;

        foreach (var method in methods)
        {
            //var paramsCount = method.ParameterDefinitions.Count;
            //method.ParameterDefinitions.Clear();

            var body = method.CilMethodBody;
            //body.ComputeMaxStackOnBuild = false;

            var iteratorLocalVarible = new CilLocalVariable(argIterator);
            var paramListLocalVarible = new CilLocalVariable(systemObjectArray);
            var indexLocalVarible = new CilLocalVariable(systemInt32);
            body.LocalVariables.Insert(0, iteratorLocalVarible);
            body.LocalVariables.Insert(1, paramListLocalVarible);
            body.LocalVariables.Insert(2, indexLocalVarible);

            IList<CilInstruction> instructions = new List<CilInstruction>();
            var add = new Action<CilInstruction>((inst) => instructions.Add(inst));

            add(new CilInstruction(CilOpCodes.Ldloca_S, iteratorLocalVarible));
            add(new CilInstruction(CilOpCodes.Arglist));
            add(new CilInstruction(CilOpCodes.Call, argIteratorCtor));
            add(new CilInstruction(CilOpCodes.Ldloca_S, iteratorLocalVarible));
            add(new CilInstruction(CilOpCodes.Call, getRemainingCount));
            add(new CilInstruction(CilOpCodes.Newarr, systemObject));
            add(new CilInstruction(CilOpCodes.Stloc_S, paramListLocalVarible));
            add(new CilInstruction(CilOpCodes.Ldc_I4_0));
            add(new CilInstruction(CilOpCodes.Stloc_S, indexLocalVarible));

            add(new CilInstruction(CilOpCodes.Br_S, null));

            add(new CilInstruction(CilOpCodes.Ldloc_S, paramListLocalVarible));
            add(new CilInstruction(CilOpCodes.Ldloc_S, indexLocalVarible));
            add(new CilInstruction(CilOpCodes.Dup));
            add(new CilInstruction(CilOpCodes.Ldc_I4_1));
            add(new CilInstruction(CilOpCodes.Add));
            add(new CilInstruction(CilOpCodes.Stloc_S, indexLocalVarible));
            add(new CilInstruction(CilOpCodes.Ldloca_S, iteratorLocalVarible));
            add(new CilInstruction(CilOpCodes.Call, getNextArg));
            add(new CilInstruction(CilOpCodes.Call, typedReferenceToObject));
            add(new CilInstruction(CilOpCodes.Stelem_Ref, typedReferenceToObject));

            add(new CilInstruction(CilOpCodes.Ldloca_S, iteratorLocalVarible));
            add(new CilInstruction(CilOpCodes.Call, getRemainingCount));
            add(new CilInstruction(CilOpCodes.Ldc_I4_0));
            add(new CilInstruction(CilOpCodes.Bgt_S, null));

            var ldlocal_sLabel = instructions[20].CreateLabel();
            var ldloc_1Label = instructions[10].CreateLabel();

            instructions[9].Operand = ldlocal_sLabel;
            instructions[23].Operand = ldloc_1Label;

            Console.WriteLine("INSTRUCTUINS:\n" + string.Join(", ", body.Instructions.Select(i => i.OpCode)));

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                if (instruction.OpCode == CilOpCodes.Ldarg_S || instruction.OpCode == CilOpCodes.Ldarg)
                {
                    var operand = instruction.Operand;

                    Console.WriteLine("FLAG 55: OPERAND: " + (operand?.ToString() ?? "NULL") + " | " +
                                      (operand?.GetType()?.FullName ?? "NULL"));

                    if (operand is Parameter parameter)
                    {
                        Console.WriteLine("FLAG 44: " + parameter.Name + " " + parameter.Index + " " +
                                          parameter.Sequence);
                        body.Instructions[i].ReplaceWith(CilOpCodes.Ldloc_S, paramListLocalVarible);
                        body.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Ldc_I4, parameter.Index));
                        body.Instructions.Insert(i + 2, new CilInstruction(CilOpCodes.Ldelem_Ref));
                    }
                }
            }

            method.Signature.ParameterTypes.Clear();
            body.ComputeMaxStackOnBuild = false;
            body.InitializeLocals = true;
            method.Signature.IncludeSentinel = true;
            method.Signature.IsSentinel = true;
            method.Signature.Attributes = CallingConventionAttributes.VarArg;
            method.Signature.SentinelParameterTypes.Add(factory.Boolean);
            body.Instructions.InsertRange(0, instructions);
            var methodNew = new MethodDefinition("Hello world", MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));
        }

        Console.WriteLine("FLAG 100: FINISH");
        return Task.CompletedTask;
    }


    private bool methodsFilter(MethodDefinition method) =>
        method is { CilMethodBody: { }, IsConstructor: false }
        && method.DeclaringType.IsModuleType == false
        && method.ParameterDefinitions.Count > 0
        && method.Signature.Attributes.HasFlag(CallingConventionAttributes.VarArg) == false;


}

public static class Helper
{
    public static string ReflectObject(Type type, object instance, string name)
    {
        var builder = new System.Text.StringBuilder()
            .AppendLine($"#{name ?? "_"}: {type.Name} = {instance ?? "NULL"}");

        if (type != null && instance != null)
        {
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);

            foreach (var member in members)
            {
                if (member is PropertyInfo property && property.CanRead)
                {
                    object value = null;
                    try
                    {
                        value = property.GetValue(instance);
                    }
                    catch
                    {
                        value = "ERROR ON GETTING VALUE";
                    }

                    var outputValue = value?.ToString() ?? "NULL";
                    if (value is string)
                    {
                        outputValue = $"\"{value}\"";
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        outputValue = $"{value.GetType()}";

                        int count = 0;
                        string items = "";
                        IEnumerator enumerator = null;
                        try
                        {
                            enumerator = enumerable.GetEnumerator();
                        }
                        catch
                        {
                        }

                        if (enumerator != null)
                        {
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    items += enumerator.Current?.ToString() ?? "NULL" + ", ";
                                    count++;
                                }
                            }
                            catch
                            {
                            }
                        }

                        outputValue += $"({count}) [{items.Trim(' ').Trim(',')}]";
                    }

                    builder.AppendLine($"- {property.Name ?? "_"}: \t{property.PropertyType.Name} \t = {outputValue}");
                }
                else if (member is FieldInfo field)
                {
                    object value = null;
                    try
                    {
                        value = field.GetValue(instance);
                    }
                    catch
                    {
                        value = "ERROR ON GETTING VALUE";
                    }

                    var outputValue = value?.ToString() ?? "NULL";
                    if (value is string)
                    {
                        outputValue = $"\"{value}\"";
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        outputValue = $"{value.GetType()}";

                        int count = 0;
                        string items = "";
                        IEnumerator enumerator = null;
                        try
                        {
                            enumerator = enumerable.GetEnumerator();
                        }
                        catch
                        {
                        }

                        if (enumerator != null)
                        {
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    items += enumerator.Current?.ToString() ?? "NULL" + ", ";
                                    count++;
                                }
                            }
                            catch
                            {
                            }
                        }

                        outputValue += $"({count}) [{items.Trim(' ').Trim(',')}]";
                    }

                    builder.AppendLine($"- {field.Name ?? "_"}: \t{field.FieldType.Name} \t = {outputValue}");
                }
            }
        }

        return builder.ToString();
    }
}