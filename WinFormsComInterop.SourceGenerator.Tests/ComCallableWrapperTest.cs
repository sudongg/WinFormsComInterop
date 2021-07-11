using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace WinFormsComInterop.SourceGenerator.Tests
{
    [TestClass]
    public class ComCallableWrapperTest : CodeGenerationTestBase
    {
        protected override SyntaxTree GetValidatedSyntaxTree(Compilation outputCompilation)
        {
            return outputCompilation.SyntaxTrees
                .First(_ => _.FilePath.Contains("Foo") && !_.FilePath.Contains("comwrappers"));
        }

        [TestMethod]
        public void DeclarationOfProxy()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    public interface IStr
    {
        void Read(byte* pv, uint cb, uint* pcbRead);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Read(System.IntPtr thisPtr, byte* pv, uint cb, uint* pcbRead)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.Read(pv, cb, pcbRead);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MethodWithoutParameters()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    public interface ICloneable
    {
        void Clone();
    }

    [ComCallableWrapper(typeof(ICloneable))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class ICloneableProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Clone(System.IntPtr thisPtr)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.ICloneable>((ComInterfaceDispatch*)thisPtr);
                inst.Clone();
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ExternIntefaceType()
        {
            string source = @"
extern alias drawing;
namespace Foo
{
    [ComCallableWrapper(typeof(drawing::ICloneable))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
extern alias drawing;
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class DrawingICloneableProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Clone(System.IntPtr thisPtr)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<drawing::ICloneable>((ComInterfaceDispatch*)thisPtr);
                inst.Clone();
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void EnumParameters()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum EnumValue {}

    public interface IStr
    {
        void Read(byte* pv, EnumValue cb, uint* pcbRead);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Read(System.IntPtr thisPtr, byte* pv, int cb, uint* pcbRead)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.Read(pv, (global::Foo.EnumValue)cb, pcbRead);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ComInterfaceParameter()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum EnumValue {}

    public interface IStr
    {
        void CopyTo(IStr pstm, ulong cb);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int CopyTo(System.IntPtr thisPtr, System.IntPtr pstm, ulong cb)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var local_0 = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)pstm);
                inst.CopyTo(local_0, cb);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ObjectParameter()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum EnumValue {}

    public interface IStr
    {
        void CopyTo([MarshalAs(UnmanagedType.Interface)]object data, ulong cb);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int CopyTo(System.IntPtr thisPtr, System.IntPtr data, ulong cb)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var local_0 = ComInterfaceDispatch.GetInstance<object>((ComInterfaceDispatch*)data);
                inst.CopyTo(local_0, cb);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void OutParameter()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum STATFLAG {}
    struct STATSTG { int Dummy; }

    public interface IStr
    {
        void Stat(out STATSTG pstatstg, STATFLAG grfStatFlag);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Stat(System.IntPtr thisPtr, global::Foo.STATSTG* pstatstg, int grfStatFlag)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.Stat(out *pstatstg, (global::Foo.STATFLAG)grfStatFlag);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void RefParameter()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum STATFLAG {}
    struct STATSTG { int Dummy; }

    public interface IStr
    {
        void Stat(ref STATSTG pstatstg, STATFLAG grfStatFlag);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Stat(System.IntPtr thisPtr, global::Foo.STATSTG* pstatstg, int grfStatFlag)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.Stat(ref *pstatstg, (global::Foo.STATFLAG)grfStatFlag);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void InParameter()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum STATFLAG {}
    struct STATSTG { int Dummy; }

    public interface IStr
    {
        void Stat(in STATSTG pstatstg, STATFLAG grfStatFlag);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Stat(System.IntPtr thisPtr, global::Foo.STATSTG* pstatstg, int grfStatFlag)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.Stat(in *pstatstg, (global::Foo.STATFLAG)grfStatFlag);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void PreserveSig()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT: int {}

    public interface IStr
    {
        [PreserveSig]
        HRESULT LockRegion(ulong libOffset);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int LockRegion(System.IntPtr thisPtr, ulong libOffset)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                return (int)inst.LockRegion(libOffset);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void PreserveSigVoid()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT: int {}

    public interface IStr
    {
        [PreserveSig]
        void LockRegion(ulong libOffset);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static void LockRegion(System.IntPtr thisPtr, ulong libOffset)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                inst.LockRegion(libOffset);
            }
            catch (System.Exception __e)
            {
                throw;
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void PreserveSigFloat()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT: int {}

    public interface IStr
    {
        [PreserveSig]
        float LockRegion(ulong libOffset);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static float LockRegion(System.IntPtr thisPtr, ulong libOffset)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                return (float)inst.LockRegion(libOffset);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ReturnValue()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT {}

    public interface IStr
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object Clone();
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Clone(System.IntPtr thisPtr, System.IntPtr* retVal)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                *retVal = MarshalSupport.GetIUnknownForObject(inst.Clone());
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ReturnInterfaceValue()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT {}

    [Guid(""D6DD68D1-86FD-4332-8666-9ABEDEA2D24C"")]
    public interface IStr
    {
        IStr Clone();
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Clone(System.IntPtr thisPtr, System.IntPtr* retVal)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var retValManaged = inst.Clone();
                if (retValManaged != null)
                {
                    var retValLocal = MarshalSupport.GetIUnknownForObject(retValManaged);
                    var targetInterface = new System.Guid(""D6DD68D1-86FD-4332-8666-9ABEDEA2D24C"");
                    try
                    {
                        var hrResult = Marshal.QueryInterface(retValLocal, ref targetInterface, out *retVal);
                        if (hrResult < 0)
                        {
                            Marshal.ThrowExceptionForHR(hrResult);
                        }
                    }
                    finally
                    {
                        Marshal.Release(retValLocal);
                    }
                }
                else
                {
                    *retVal = System.IntPtr.Zero;
                }
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void GetProperty()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT {}

    public interface IStr
    {
        object Parent { get; }
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int get_Parent(System.IntPtr thisPtr, System.IntPtr* retVal)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                *retVal = MarshalSupport.GetIUnknownForObject(inst.Parent);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void GetInterfaceProperty()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT {}

    [Guid(""22DD68D1-86FD-4332-8666-9ABEDEA2D24C"")]
    public interface IStr
    {
        IStr Parent { get; }
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int get_Parent(System.IntPtr thisPtr, System.IntPtr* retVal)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var retValManaged = inst.Parent;
                if (retValManaged != null)
                {
                    var retValLocal = MarshalSupport.GetIUnknownForObject(retValManaged);
                    var targetInterface = new System.Guid(""22DD68D1-86FD-4332-8666-9ABEDEA2D24C"");
                    try
                    {
                        var hrResult = Marshal.QueryInterface(retValLocal, ref targetInterface, out *retVal);
                        if (hrResult < 0)
                        {
                            Marshal.ThrowExceptionForHR(hrResult);
                        }
                    }
                    finally
                    {
                        Marshal.Release(retValLocal);
                    }
                }
                else
                {
                    *retVal = System.IntPtr.Zero;
                }
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void GetEnumProperty()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    enum HRESULT {}

    [Guid(""22DD68D1-86FD-4332-8666-9ABEDEA2D24C"")]
    public interface IStr
    {
        HRESULT Parent { get; }
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int get_Parent(System.IntPtr thisPtr, int* retVal)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                *retVal = (int)inst.Parent;
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void StringParameters()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    public interface IStr
    {
        void Read(byte* pv, [MarshalAs(UnmanagedType.LPWStr)] string pwcsName, uint* pcbRead);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Read(System.IntPtr thisPtr, byte* pv, System.IntPtr pwcsName, uint* pcbRead)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var local_1 = Marshal.PtrToStringUni(pwcsName);
                inst.Read(pv, local_1, pcbRead);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ExternIntefaceTypeWithPointer()
        {
            string source = @"
extern alias drawing;
namespace Foo
{
    [ComCallableWrapper(typeof(drawing::IPtrMethod))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
extern alias drawing;
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class DrawingIPtrMethodProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int MethodPtr(System.IntPtr thisPtr, drawing::EnumValue* val)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<drawing::IPtrMethod>((ComInterfaceDispatch*)thisPtr);
                inst.MethodPtr(val);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void BlittableArrayParameters()
        {
            string source = @"
namespace Foo
{
    using System.Runtime.InteropServices;

    public interface IStr
    {
        void Read(byte* pv, System.Guid[] pIIDExclude, uint* pcbRead);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int Read(System.IntPtr thisPtr, byte* pv, global::System.Guid* pIIDExclude, uint* pcbRead)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                var local_1 = new System.Span<global::System.Guid>(pIIDExclude, 1).ToArray();
                inst.Read(pv, local_1, pcbRead);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ComInterfaceOutParameter()
        {
            string source = @"
namespace Foo
{
    using System.Threading;

    enum EnumValue {}

    public interface IStr
    {
        void CopyTo(out IStr pstm, ulong cb);
    }

    [ComCallableWrapper(typeof(IStr))]
    partial class C
    {
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by COM Proxy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
using ComInterfaceDispatch = System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Foo
{
    [System.Runtime.Versioning.SupportedOSPlatform(""windows"")]
    unsafe partial class IStrProxy
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly]
        public static int CopyTo(System.IntPtr thisPtr, System.IntPtr* pstm, ulong cb)
        {
            try
            {
                var inst = ComInterfaceDispatch.GetInstance<global::Foo.IStr>((ComInterfaceDispatch*)thisPtr);
                global::Foo.IStr local_0;
                inst.CopyTo(out local_0, cb);
                *pstm = local_0 == null ? System.IntPtr.Zero : MarshalSupport.GetIUnknownForObject(local_0);
            }
            catch (System.Exception __e)
            {
                return __e.HResult;
            }

            return 0; // S_OK;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }
    }
}
