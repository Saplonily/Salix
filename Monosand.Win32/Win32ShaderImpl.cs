using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monosand.Win32;

internal class Win32ShaderImpl : Win32GraphicsImplBase, IShaderImpl
{
    private IntPtr handle;
    internal IntPtr Handle { get { EnsureState(); return handle; } }

    private Win32ShaderImpl(Win32RenderContext context)
        : base(context)
    {
    }

    internal unsafe static Win32ShaderImpl FromGlsl(Win32RenderContext context, byte* vertSource, byte* fragSource)
    {
        Win32ShaderImpl impl = new(context);
        impl.handle = Interop.MsdgCreateShaderFromGlsl(vertSource, fragSource);
        return impl;
    }

    int IShaderImpl.GetParameterLocation(string name)
    {
        EnsureState();
        EnsureCurrentState();
#if NETSTANDARD2_1_OR_GREATER
        return Interop.MsdgGetShaderParamLocation(handle, name);
#else
        byte[] utf8NameBytes = Encoding.UTF8.GetBytes(name);
        unsafe
        {
            fixed (byte* ptr = utf8NameBytes)
            {
                return Interop.MsdgGetShaderParamLocation(handle, ptr);
            }
        }
#endif
    }

    int IShaderImpl.GetParameterLocation(ReadOnlySpan<byte> nameUtf8)
    {
        EnsureState();
        EnsureCurrentState();
        unsafe
        {
            fixed (byte* ptr = nameUtf8)
            {
                return Interop.MsdgGetShaderParamLocation(handle, ptr);
            }
        }
    }

    unsafe void IShaderImpl.SetParameter<T>(int location, ref T value)
    {
        EnsureState();
        EnsureCurrentState();
        // our jit god will do the optimization

        if (typeof(T) == typeof(int))
        {
            Interop.MsdgSetShaderParamInt(location, Unsafe.As<T, int>(ref value));
            return;
        }

        if (typeof(T) == typeof(bool))
        {
            Interop.MsdgSetShaderParamInt(location, Unsafe.As<T, bool>(ref value) ? 1 : 0);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            Interop.MsdgSetShaderParamFloat(location, Unsafe.As<T, float>(ref value));
            return;
        }

        if (typeof(T) == typeof(Vector4))
        {
            ref var vec = ref Unsafe.As<T, Vector4>(ref value);
            Interop.MsdgSetShaderParamVec4(location, (float*)Unsafe.AsPointer(ref vec));
            return;
        }

        if (typeof(T) == typeof(Matrix4x4))
        {
            ref var mat = ref Unsafe.As<T, Matrix4x4>(ref value);
            Interop.MsdgSetShaderParamMat4(location, (float*)Unsafe.AsPointer(ref mat), false);
            return;
        }

        throw new NotSupportedException($"Type of {typeof(T)} is not supported in shader parameter.");
    }

    private void EnsureCurrentState()
    {
        ThrowHelper.ThrowIfInvalid(RenderContext.Shader?.Impl != this, "This operation required this shader to be current.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (handle != IntPtr.Zero)
        {
            Interop.MsdgDeleteShader(handle);
            handle = IntPtr.Zero;
        }
    }
}