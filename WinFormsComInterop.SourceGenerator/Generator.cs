﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WinFormsComInterop.SourceGenerator
{
    /// <summary>
    /// Generator for COM interface proxies, used by ComWrappers.
    /// </summary>
    [Generator]
    public class Generator : ISourceGenerator
    {
        private const string AttributeSource = @"// <auto-generated>
// Code generated by COM interface proxies Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable disable

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
internal sealed class ComCallableWrapperAttribute: System.Attribute
{
    public ComCallableWrapperAttribute(System.Type interfaceType, string alias = null)
    {
        this.InterfaceType = interfaceType;
        this.Alias = alias;
    }

    public System.Type InterfaceType { get; }

    public string Alias { get; }
}

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
internal sealed class RuntimeCallableWrapperAttribute: System.Attribute
{
    public RuntimeCallableWrapperAttribute(System.Type interfaceType, string alias = null)
    {
        this.InterfaceType = interfaceType;
        this.Alias = alias;
    }

    public System.Type InterfaceType { get; }

    public string Alias { get; }
}
";
        private const string StaticSupportLibrarySource = @"// <auto-generated>
// Code generated by COM interface proxies Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WinFormsComInterop.InteropServices
{
    [StructLayout(LayoutKind.Explicit)]
    [SupportedOSPlatform(""windows"")]
    internal partial struct Variant 
    {
        // Most of the data types in the Variant are carried in _typeUnion
        [FieldOffset(0)] private TypeUnion _typeUnion;

        // Decimal is the largest data type and it needs to use the space that is normally unused in TypeUnion._wReserved1, etc.
        // Hence, it is declared to completely overlap with TypeUnion. A Decimal does not use the first two bytes, and so
        // TypeUnion._vt can still be used to encode the type.
        [FieldOffset(0)] private decimal _decimal;

        [StructLayout(LayoutKind.Sequential)]
        private struct TypeUnion
        {
            public ushort _vt;
            public ushort _wReserved1;
            public ushort _wReserved2;
            public ushort _wReserved3;

            public UnionTypes _unionTypes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            public IntPtr _record;
            public IntPtr _recordInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UnionTypes
        {
            [FieldOffset(0)] public sbyte _i1;
            [FieldOffset(0)] public short _i2;
            [FieldOffset(0)] public int _i4;
            [FieldOffset(0)] public long _i8;
            [FieldOffset(0)] public byte _ui1;
            [FieldOffset(0)] public ushort _ui2;
            [FieldOffset(0)] public uint _ui4;
            [FieldOffset(0)] public ulong _ui8;
            [FieldOffset(0)] public int _int;
            [FieldOffset(0)] public uint _uint;
            [FieldOffset(0)] public short _bool;
            [FieldOffset(0)] public int _error;
            [FieldOffset(0)] public float _r4;
            [FieldOffset(0)] public double _r8;
            [FieldOffset(0)] public long _cy;
            [FieldOffset(0)] public double _date;
            [FieldOffset(0)] public IntPtr _bstr;
            [FieldOffset(0)] public IntPtr _unknown;
            [FieldOffset(0)] public IntPtr _dispatch;
            [FieldOffset(0)] public IntPtr _pvarVal;
            [FieldOffset(0)] public IntPtr _byref;
            [FieldOffset(0)] public Record _record;
        }
    }
}

partial class MarshalSupport
{
    public static object GetObjectForNativeVariant(IntPtr pVariant)
    {
        return Marshal.GetObjectForNativeVariant(pVariant);
    }
}
";

        private static string ComCallableWrapperAttributeName = "ComCallableWrapperAttribute";

        private static string RuntimeCallableWrapperAttributeName = "RuntimeCallableWrapperAttribute";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization((pi) =>
            {
                pi.AddSource("ComProxyAttribute.cs", AttributeSource);
                pi.AddSource("StaticSupportLibrary.cs", StaticSupportLibrarySource);
            });
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Retrieve the populated receiver
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var wrapperContext = new WrapperGenerationContext(context);
            GenerateSupportLibrary(context, receiver, wrapperContext);
            foreach (var classSymbol in receiver.CCWDeclarations)
            {
                ProcessCCWDeclaration(classSymbol, wrapperContext);
                ProcessCCWVtblDeclaration(classSymbol, wrapperContext);
            }

            foreach (var classSymbol in receiver.RCWDeclarations)
            {
                ProcessRCWDeclaration(classSymbol, wrapperContext);
            }

            // Uncomment line below to trace resulting codegen.
            // context.AddSource("DebugOutput.cs", wrapperContext.DebugOutput());
        }
        private void GenerateSupportLibrary(GeneratorExecutionContext context, SyntaxReceiver receiver, WrapperGenerationContext wrapperContext)
        {
            IndentedStringBuilder source = new IndentedStringBuilder($@"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
");

            source.AppendLine();
            source.AppendLine("partial class MarshalSupport");
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine("public static System.IntPtr GetIUnknownForObject(object value)");
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine("if (value == null)");
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine("return System.IntPtr.Zero;");
            source.PopIndent();
            source.AppendLine("}");

            foreach (var classSymbol in receiver.RCWDeclarations)
            {
                source.AppendLine($"if (value is {classSymbol.Type.FormatType(wrapperContext.GetAlias(classSymbol.Type))})");
                source.AppendLine("{");
                source.PushIndent();
                source.AppendLine($"var result = (({classSymbol.Type.FormatType(wrapperContext.GetAlias(classSymbol.Type))})value).instance;");
                source.AppendLine("System.Runtime.InteropServices.Marshal.AddRef(result);");
                source.AppendLine("return result;");
                source.PopIndent();
                source.AppendLine("}");
            }

            source.AppendLine("return System.Runtime.InteropServices.Marshal.GetIUnknownForObject(value);");
            source.PopIndent();
            source.AppendLine("}"); // GetIUnknownForObject

            source.PopIndent();
            source.AppendLine("}");
            context.AddSource("SupportLibrary.cs", source.ToString());
        }
        private string ProcessCCWDeclaration(ClassDeclaration classSymbol, INamedTypeSymbol interfaceTypeSymbol, WrapperGenerationContext context)
        {
            string namespaceName = classSymbol.Type.ContainingNamespace.ToDisplayString();
            IndentedStringBuilder source = new IndentedStringBuilder($@"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
");
            var aliasSymbol = context.GetAlias(interfaceTypeSymbol);
            if (!string.IsNullOrWhiteSpace(aliasSymbol))
            {
                source.AppendLine($"extern alias {aliasSymbol};");
            }

            source.AppendLine("using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;");
            source.AppendLine("using Marshal = System.Runtime.InteropServices.Marshal;");
            source.Append($@"
namespace {namespaceName}
{{
");
            source.PushIndent();
            source.AppendLine("[System.Runtime.Versioning.SupportedOSPlatform(\"windows\")]");
            var typeName = $"{interfaceTypeSymbol.Name}Proxy";
            if (!string.IsNullOrWhiteSpace(aliasSymbol))
            {
                typeName = aliasSymbol.Substring(0,1).ToUpperInvariant() + aliasSymbol.Substring(1) + typeName;
            }

            source.AppendLine($"unsafe partial class {typeName}");
            source.AppendLine("{");
            source.PushIndent();
            IndentedStringBuilder proxyMethods = new ();
            IndentedStringBuilder vtblMethod = new ();
            int slotNumber = 3; /* Starting with slot after IUnknown */
            vtblMethod.AppendLine($"internal static void Create{typeName}Vtbl(out System.IntPtr vtbl)");
            vtblMethod.AppendLine("{");
            vtblMethod.PushIndent();
            vtblMethod.AppendLine($"var vtblRaw = (System.IntPtr*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(global::{typeName}), sizeof(System.IntPtr) * 4);");
            vtblMethod.AppendLine("GetIUnknownImpl(out vtblRaw[0], out vtblRaw[1], out vtblRaw[2]);");
            vtblMethod.AppendLine();
            foreach (var member in interfaceTypeSymbol.GetMembers())
            {
                switch (member)
                {
                    case IMethodSymbol methodSymbol:
                        {
                            var methodContext = context.CreateMethodGenerationContext(classSymbol, methodSymbol, slotNumber);
                            GenerateCCWMethod(proxyMethods, interfaceTypeSymbol, methodContext);
                            vtblMethod.AppendLine($"vtblRaw[{slotNumber}] = (System.IntPtr)({methodContext.UnmanagedDelegateSignature})&{typeName}.{methodContext.Method.Name};");
                            slotNumber++;
                        }
                        break;
                }
            }

            vtblMethod.AppendLine();
            vtblMethod.AppendLine($"vtbl = (System.IntPtr)vtblRaw;");
            vtblMethod.PopIndent();
            vtblMethod.AppendLine("}");
            //source.Append(vtblMethod);
            //source.AppendLine();
            source.Append(proxyMethods);
            source.PopIndent();
            source.AppendLine("}");
            source.PopIndent();

            source.Append("}");
            return source.ToString();
        }
        private void ProcessCCWVtblDeclaration(ClassDeclaration classSymbol, WrapperGenerationContext context)
        {
            IndentedStringBuilder source = new IndentedStringBuilder($@"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
");
            var proxyDeclarations = classSymbol.Type.GetAttributes().Where(ad => ad.AttributeClass?.ToDisplayString() == ComCallableWrapperAttributeName);
            var key = (INamedTypeSymbol)classSymbol.Type;
            List<string> aliases = new();
            foreach (var proxyAttribute in proxyDeclarations)
            {
                var interfaceTypeSymbol = proxyAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                if (interfaceTypeSymbol == null)
                {
                    continue;
                }

                var aliasSymbol = context.GetAlias(interfaceTypeSymbol);
                if (!string.IsNullOrWhiteSpace(aliasSymbol))
                {
                    aliases.Add(aliasSymbol);
                }
            }

            foreach (var aliasSymbol in aliases.Distinct())
            {
                source.AppendLine($"extern alias {aliasSymbol};");
            }

            string namespaceName = classSymbol.Type.ContainingNamespace.ToDisplayString();
            source.AppendLine("using System.Runtime.CompilerServices;");
            source.AppendLine("using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;");
            source.AppendLine("using Marshal = System.Runtime.InteropServices.Marshal;");
            source.Append($@"
namespace {namespaceName}
{{
"); 
            source.PushIndent();
            source.AppendLine("[System.Runtime.Versioning.SupportedOSPlatform(\"windows\")]");
            var typeName = key.Name;

            source.AppendLine($"unsafe partial class {typeName}");
            source.AppendLine("{");
            source.PushIndent();
            foreach (var proxyAttribute in proxyDeclarations)
            {
                var interfaceTypeSymbol = proxyAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                if (interfaceTypeSymbol == null)
                {
                    continue;
                }

                ProcessCCWVblDeclaration(source, classSymbol, interfaceTypeSymbol, context);
            }

            source.AppendLine("}");
            source.PopIndent();

            source.Append("}");
            context.AddComWrapperSource(key, SourceText.From(source.ToString(), Encoding.UTF8));
        }

        private void ProcessCCWVblDeclaration(IndentedStringBuilder vtblMethod, ClassDeclaration classSymbol, INamedTypeSymbol interfaceTypeSymbol, WrapperGenerationContext context)
        {
            var aliasSymbol = context.GetAlias(interfaceTypeSymbol);
            var key = (INamedTypeSymbol)classSymbol.Type;
            var typeName = $"{interfaceTypeSymbol.Name}Proxy";
            if (!string.IsNullOrWhiteSpace(aliasSymbol))
            {
                typeName = aliasSymbol.Substring(0, 1).ToUpperInvariant() + aliasSymbol.Substring(1) + typeName;
            }

            var membersCount = interfaceTypeSymbol.GetMembers().OfType<IMethodSymbol>().Count() + 3;
            int slotNumber = 3; /* Starting with slot after IUnknown */
            vtblMethod.AppendLine($"internal static void Create{typeName}Vtbl(out System.IntPtr vtbl)");
            vtblMethod.AppendLine("{");
            vtblMethod.PushIndent();
            vtblMethod.AppendLine($"var vtblRaw = (System.IntPtr*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof({key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), sizeof(System.IntPtr) * {membersCount});");
            vtblMethod.AppendLine("GetIUnknownImpl(out vtblRaw[0], out vtblRaw[1], out vtblRaw[2]);");
            vtblMethod.AppendLine();
            foreach (var member in interfaceTypeSymbol.GetMembers())
            {
                switch (member)
                {
                    case IMethodSymbol methodSymbol:
                        {
                            var methodContext = context.CreateMethodGenerationContext(classSymbol, methodSymbol, slotNumber);
                            vtblMethod.AppendLine($"vtblRaw[{slotNumber}] = (System.IntPtr)({methodContext.UnmanagedDelegateSignature})&{typeName}.{methodContext.Method.Name};");
                            slotNumber++;
                        }
                        break;
                }
            }

            vtblMethod.AppendLine();
            vtblMethod.AppendLine($"vtbl = (System.IntPtr)vtblRaw;");
            vtblMethod.PopIndent();
            vtblMethod.AppendLine("}");
        }

        private IEnumerable<INamedTypeSymbol> FindCCWDeclarations(ClassDeclaration classSymbol)
        {
            var proxyDeclarations = classSymbol.Type
                .GetAttributes()
                .Where(ad => ad.AttributeClass?.ToDisplayString() == ComCallableWrapperAttributeName);
            foreach (var proxyAttribute in proxyDeclarations)
            {
                var interfaceTypeSymbol = proxyAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                if (interfaceTypeSymbol == null)
                {
                    continue;
                }

                foreach (var baseInterface in interfaceTypeSymbol.Interfaces)
                {
                    yield return baseInterface;
                }

                yield return interfaceTypeSymbol;
            }

        }

        private void ProcessCCWDeclaration(ClassDeclaration classSymbol, WrapperGenerationContext context)
        {
            foreach (var interfaceTypeSymbol in FindCCWDeclarations(classSymbol).Distinct())
            {
                var sourceCode = ProcessCCWDeclaration(classSymbol, interfaceTypeSymbol, context);
                var key = (INamedTypeSymbol)classSymbol.Type;
                context.AddCCWSource(key, interfaceTypeSymbol, SourceText.From(sourceCode, Encoding.UTF8));
            }
        }

        private string ProcessRCWDeclaration(ClassDeclaration classSymbol, INamedTypeSymbol interfaceTypeSymbol, WrapperGenerationContext context)
        {
            string namespaceName = classSymbol.Type.ContainingNamespace.ToDisplayString();
            IndentedStringBuilder source = new IndentedStringBuilder($@"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
");
            var aliasSymbol = context.GetAlias(interfaceTypeSymbol);
            if (!string.IsNullOrWhiteSpace(aliasSymbol))
            {
                source.AppendLine($"extern alias {aliasSymbol};");
            }

            source.AppendLine("using Marshal = System.Runtime.InteropServices.Marshal;");
            source.Append($@"
namespace {namespaceName}
{{
");
            source.PushIndent();
            source.AppendLine("[System.Runtime.Versioning.SupportedOSPlatform(\"windows\")]");
            var typeName = $"{classSymbol.Type.Name}";

            source.AppendLine($"unsafe partial class {classSymbol.Type.Name} : {interfaceTypeSymbol.FormatType(aliasSymbol)}");
            source.AppendLine("{");
            source.PushIndent();
            int slotNumber = 3; /* Starting with slot after IUnknown */
            foreach (var member in interfaceTypeSymbol.GetMembers())
            {
                switch (member)
                {
                    case IMethodSymbol methodSymbol:
                        {
                            if (methodSymbol.MethodKind == MethodKind.PropertyGet || methodSymbol.MethodKind == MethodKind.PropertySet)
                            {
                                continue;
                            }

                            var methodContext = context.CreateMethodGenerationContext(classSymbol, methodSymbol, slotNumber);
                            slotNumber++;

                            GenerateRCWMethodHeader(source, interfaceTypeSymbol, methodContext);
                            WriteRCWMethodBody(source, interfaceTypeSymbol, methodContext);
                        }
                        break;
                    case IPropertySymbol propertySymbol:
                        {
                            GenerateRCWPropertyHeader(source, interfaceTypeSymbol, propertySymbol, context);
                            source.AppendLine("{");
                            source.PushIndent();

                            if (propertySymbol.GetMethod != null)
                            {
                                var getterContext = context.CreateMethodGenerationContext(classSymbol, propertySymbol.GetMethod, slotNumber);
                                slotNumber++;

                                source.AppendLine("get");
                                WriteRCWMethodBody(source, interfaceTypeSymbol, getterContext);
                            }

                            if (propertySymbol.SetMethod != null)
                            {
                                var setterContext = context.CreateMethodGenerationContext(classSymbol, propertySymbol.SetMethod, slotNumber);
                                slotNumber++;

                                source.AppendLine("set");
                                WriteRCWMethodBody(source, interfaceTypeSymbol, setterContext);
                            }

                            source.PopIndent();
                            source.AppendLine("}");
                        }

                        break;
                }
            }

            source.PopIndent();
            source.AppendLine("}");
            source.PopIndent();

            source.Append("}");
            return source.ToString();
        }

        private IEnumerable<INamedTypeSymbol> FindRCWDeclarations(ClassDeclaration classSymbol)
        {
            var proxyDeclarations = classSymbol.Type
                .GetAttributes()
                .Where(ad => ad.AttributeClass?.ToDisplayString() == RuntimeCallableWrapperAttributeName);
            foreach (var proxyAttribute in proxyDeclarations)
            {
                var interfaceTypeSymbol = proxyAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                if (interfaceTypeSymbol == null)
                {
                    continue;
                }

                foreach (var baseInterface in interfaceTypeSymbol.Interfaces)
                {
                    yield return baseInterface;
                }

                yield return interfaceTypeSymbol;
            }

        }

        private void ProcessRCWDeclaration(ClassDeclaration classSymbol, WrapperGenerationContext context)
        {
            foreach (var interfaceTypeSymbol in FindRCWDeclarations(classSymbol).Distinct())
            {
                var sourceCode = ProcessRCWDeclaration(classSymbol, interfaceTypeSymbol, context);
                var key = (INamedTypeSymbol)classSymbol.Type;
                context.AddCCWSource(key, interfaceTypeSymbol, SourceText.From(sourceCode, Encoding.UTF8));
            }
        }

        private void GenerateCCWMethod(IndentedStringBuilder source, INamedTypeSymbol interfaceSymbol, MethodGenerationContext context)
        {
            source.AppendLine("[System.Runtime.InteropServices.UnmanagedCallersOnly]");
            var method = context.Method;
            var marshallers = context.Marshallers;
            var preserveSignature = context.PreserveSignature;
            var parametersList = marshallers.Select(_ => _.GetUnmanagedParameterDeclaration()).ToList();
            parametersList.Insert(0, "System.IntPtr thisPtr");
            var returnMarshaller = context.CreateReturnMarshaller();
            if (!preserveSignature)
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    parametersList.Add(returnMarshaller.GetReturnDeclaration());
                }
            }

            var parametersListString = string.Join(", ", parametersList);
            var returnType = "int";
            if (preserveSignature)
            {
                if (method.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    returnType = "void";
                }
                else
                {
                    returnType = method.ReturnType.FormatType(context.GetAlias(method.ReturnType));
                    if (method.ReturnType.TypeKind == TypeKind.Enum)
                    {
                        returnType = "int";
                    }
                }
            }

            source.AppendLine($"public static {returnType} {method.Name}({parametersListString})");
            source.AppendLine("{");
            source.PushIndent();

            source.AppendLine("try");
            source.AppendLine("{");
            source.PushIndent();

            source.AppendLine($"var inst = ComInterfaceDispatch.GetInstance<{interfaceSymbol.FormatType(context.GetAlias(interfaceSymbol))}>((ComInterfaceDispatch*)thisPtr);");
            foreach (var p in marshallers)
            {
                p.DeclareLocalParameter(source);
            }
            var parametersInvocationList = string.Join(", ", marshallers.Select(_ => _.GetParameterInvocation()));

            if (!preserveSignature)
            {
                if (method.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    source.AppendLine($"inst.{method.Name}({parametersInvocationList});");
                }
                else
                {
                    string invocationExpression;
                    if (method.MethodKind == MethodKind.PropertyGet)
                    {
                        invocationExpression = $"inst.{method.AssociatedSymbol!.Name}";
                    }
                    else
                    {
                        invocationExpression = $"inst.{method.Name}({parametersInvocationList})";
                    }

                    returnMarshaller.GetReturnValue(source, invocationExpression);
                }
            }
            else
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    source.AppendLine($"return ({returnType})inst.{method.Name}({parametersInvocationList});");
                }
                else
                {
                    source.AppendLine($"inst.{method.Name}({parametersInvocationList});");
                }
            }

            foreach (var p in marshallers)
            {
                p.MarshalOutputParameter(source);
            }
            
            source.PopIndent();
            source.AppendLine("}");
            source.AppendLine("catch (System.Exception __e)");
            source.AppendLine("{");
            source.PushIndent();
            if (preserveSignature && method.ReturnType.SpecialType == SpecialType.System_Void)
            {
                source.AppendLine("throw;");
            }
            else
            {
                source.AppendLine("return __e.HResult;");
            }

            source.PopIndent();
            source.AppendLine("}");
            if (!preserveSignature)
            {
                source.AppendLine();
                source.AppendLine("return 0; // S_OK;");
            }

            source.PopIndent();
            source.AppendLine("}");
        }

        private void GenerateRCWMethodHeader(IndentedStringBuilder source, INamedTypeSymbol interfaceSymbol, MethodGenerationContext context)
        {
            var method = context.Method;
            var parametersList = context.Marshallers.Select(_ => _.GetManagedParameterDeclaration()).ToList();

            var parametersListString = string.Join(", ", parametersList);
            var interfaceTypeName = interfaceSymbol.FormatType(context.GetAlias(interfaceSymbol));
            var returnTypeName = method.ReturnType.FormatType(context.GetAlias(method.ReturnType));
            source.AppendLine($"{returnTypeName} {interfaceTypeName}.{method.Name}({parametersListString})");
        }

        private void GenerateRCWPropertyHeader(IndentedStringBuilder source, INamedTypeSymbol interfaceSymbol, IPropertySymbol propertySymbol, WrapperGenerationContext context)
        {
            var interfaceTypeName = interfaceSymbol.FormatType(context.GetAlias(interfaceSymbol));
            var returnTypeName = propertySymbol.Type.FormatType(context.GetAlias(propertySymbol.Type));
            source.AppendLine($"{returnTypeName} {interfaceTypeName}.{propertySymbol.Name}");
        }

        private static void WriteRCWMethodBody(IndentedStringBuilder source, INamedTypeSymbol interfaceSymbol, MethodGenerationContext context)
        {
            var method = context.Method;
            var marshallers = context.Marshallers;
            var preserveSignature = context.PreserveSignature;
            var returnMarshaller = context.CreateReturnMarshaller();
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine($"var targetInterface = new System.Guid(\"{interfaceSymbol.GetTypeGuid()}\");");
            source.AppendLine("var result = Marshal.QueryInterface(this.instance, ref targetInterface, out var thisPtr);");
            source.AppendLine("if (result != 0)");
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine("throw new System.InvalidCastException();");
            source.PopIndent();
            source.AppendLine("}");
            source.AppendLine();

            source.AppendLine("try");
            source.AppendLine("{");
            source.PushIndent();

            source.AppendLine("var comDispatch = (System.IntPtr*)thisPtr;");
            source.AppendLine("var vtbl = (System.IntPtr*)comDispatch[0];");
            foreach (var m in marshallers)
            {
                m.ConvertToUnmanagedParameter(source);
            }

            if (!context.PreserveSignature)
            {
                returnMarshaller.ConvertToUnmanagedParameter(source);
            }
            else
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    source.AppendLine($"{returnMarshaller.UnmanagedTypeName} retVal;");
                }
            }

            foreach (var m in marshallers)
            {
                m.PinParameter(source);
            }

            var parametersCallList = marshallers.Select(_ => _.GetUnmanagedParameterInvocation()).ToList();
            if (!context.PreserveSignature)
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    parametersCallList.Add(returnMarshaller.GetUnmanagedReturnValue());
                }
            }

            parametersCallList.Insert(0, "thisPtr");
            var parametersCallListString = string.Join(", ", parametersCallList);
            if (!preserveSignature)
            {
                source.AppendLine($"result = (({context.UnmanagedDelegateSignature})vtbl[{context.ComSlotNumber}])({parametersCallListString});");
                source.AppendLine("if (result != 0)");
                source.AppendLine("{");
                source.PushIndent();
                source.AppendLine("Marshal.ThrowExceptionForHR(result);");
                source.PopIndent();
                source.AppendLine("}");
                source.AppendLine();
            }
            else
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    source.AppendLine($"retVal = (({context.UnmanagedDelegateSignature})vtbl[{context.ComSlotNumber}])({parametersCallListString});");
                }
                else
                {
                    source.AppendLine($"(({context.UnmanagedDelegateSignature})vtbl[{context.ComSlotNumber}])({parametersCallListString});");
                }
            }

            foreach (var m in marshallers)
            {
                m.UnmarshalParameter(source);
            }

            if (!context.PreserveSignature)
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    source.AppendLine($"return {returnMarshaller.GetParameterInvocation()};");
                }
            }
            else
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                {
                    source.AppendLine($"return ({returnMarshaller.TypeName})retVal;");
                }
            }

            source.PopIndent();
            source.AppendLine("}");
            source.AppendLine("finally");
            source.AppendLine("{");
            source.PushIndent();
            source.AppendLine("Marshal.Release(thisPtr);");
            source.PopIndent();
            source.AppendLine("}");

            source.PopIndent();
            source.AppendLine("}");
        }

        internal class ClassDeclaration
        {
            public ITypeSymbol Type { get; set; }

            public string Alias { get; set; }
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<ClassDeclaration> CCWDeclarations { get; } = new List<ClassDeclaration>();
            public List<ClassDeclaration> RCWDeclarations { get; } = new List<ClassDeclaration>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // any field with at least one attribute is a candidate for property generation
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    // Get the symbol being declared by the field, and keep it if its annotated
                    var classSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as ITypeSymbol;
                    if (classSymbol == null)
                    {
                        return;
                    }

                    if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == ComCallableWrapperAttributeName))
                    {
                        var argumentExpression = classDeclarationSyntax.AttributeLists[0].Attributes[0].ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax;
                        var typeNameSyntax = argumentExpression.Type as QualifiedNameSyntax;
                        var alias = "";
                        this.CCWDeclarations.Add(new ClassDeclaration { Type = classSymbol, Alias = alias });
                    }

                    if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == RuntimeCallableWrapperAttributeName))
                    {
                        var argumentExpression = classDeclarationSyntax.AttributeLists[0].Attributes[0].ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax;
                        var typeNameSyntax = argumentExpression.Type as QualifiedNameSyntax;
                        var alias = "";
                        this.RCWDeclarations.Add(new ClassDeclaration { Type = classSymbol, Alias = alias });
                    }
                }
            }
        }
    }
}
