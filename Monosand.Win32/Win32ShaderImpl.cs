using System.Numerics;
using System.Runtime.CompilerServices;

namespace Monosand.Win32;

internal class Win32ShaderImpl : GraphicsImplBase, IShaderImpl
{
    private Win32RenderContext context;
    private IntPtr shaderHandle;

    private Win32ShaderImpl(Win32RenderContext context)
        : base(context.GetWinHandle())
    {
        this.context = context;
    }

    internal unsafe static Win32ShaderImpl FromGlsl(Win32RenderContext context, byte* vshSource, byte* fshSource)
    {
        Win32ShaderImpl impl = new(context);
        impl.shaderHandle = Interop.MsdgCreateShaderFromGlsl(impl.winHandle, vshSource, fshSource);
        return impl;
    }

    int IShaderImpl.GetParameterLocation(string name)
    {
        EnsureState();
        EnsureCurrentState();
        return Interop.MsdgGetShaderParamLocation(winHandle, shaderHandle, name);
    }

    int IShaderImpl.GetParameterLocation(ReadOnlySpan<byte> nameUtf8)
    {
        EnsureState();
        EnsureCurrentState();
        unsafe
        {
            fixed (byte* ptr = nameUtf8)
            {
                return Interop.MsdgGetShaderParamLocation(winHandle, shaderHandle, ptr);
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
            Interop.MsdgSetShaderParamInt(winHandle, location, Unsafe.As<T, int>(ref value));
            return;
        }

        if (typeof(T) == typeof(float))
        {
            Interop.MsdgSetShaderParamFloat(winHandle, location, Unsafe.As<T, float>(ref value));
            return;
        }

        if (typeof(T) == typeof(Vector4))
        {
            ref var vec = ref Unsafe.As<T, Vector4>(ref value);
            Interop.MsdgSetShaderParamVec4(winHandle, location, (float*)Unsafe.AsPointer(ref vec));
            return;
        }

        if (typeof(T) == typeof(Matrix4x4))
        {
            ref var mat = ref Unsafe.As<T, Matrix4x4>(ref value);
            Interop.MsdgSetShaderParamMat4(winHandle, location, (float*)Unsafe.AsPointer(ref mat), false);
            return;
        }

        throw new NotSupportedException($"Type of {typeof(T)} is not supported in shader parameter.");
    }

    internal IntPtr GetShaderHandle()
    {
        EnsureState();
        return shaderHandle;
    }

    private void EnsureCurrentState()
    {
        ThrowHelper.ThrowIfInvalid(context.GetCurrentShader()?.GetImpl() != this, "This operation required this shader to be current.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        throw new NotImplementedException();
    }
}