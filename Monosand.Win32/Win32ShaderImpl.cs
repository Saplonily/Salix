using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Monosand.Win32;

internal class Win32ShaderImpl : ShaderImpl
{
    private IntPtr winHandle;
    private IntPtr nativeHandle;

    private Win32ShaderImpl(IntPtr winHandle) { this.winHandle = winHandle; }

    internal unsafe static Win32ShaderImpl FromGlsl(Win32RenderContext context, byte* vshSource, byte* fshSource)
    {
        Win32ShaderImpl impl = new(context.GetHandle());
        impl.nativeHandle = Interop.MsdgCreateShaderFromGlsl(impl.winHandle, vshSource, fshSource);
        return impl;
    }

    internal override int GetParameterLocation(string name)
    {
        EnsureState();
        return Interop.MsdgGetShaderParamLocation(winHandle, nativeHandle, name);
    }

    internal override int GetParameterLocation(ReadOnlySpan<byte> nameUtf8)
    {
        EnsureState();
        unsafe
        {
            fixed (byte* ptr = nameUtf8)
            {
                return Interop.MsdgGetShaderParamLocation(winHandle, nativeHandle, ptr);
            }
        }
    }

    internal override void SetParameter<T>(int location, T value)
    {
        EnsureState();
        // our jit god will do the optimization

        if (typeof(T) == typeof(int))
        {
            Interop.MsdgSetShaderParamInt(winHandle, nativeHandle, location, (int)(object)value);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            Interop.MsdgSetShaderParamFloat(winHandle, nativeHandle, location, (float)(object)value);
            return;
        }

        throw new NotSupportedException($"Type of {typeof(T)} is not supported in shader parameter.");
    }

    internal override void Use()
    {
        EnsureState();
        Interop.MsdgUseShader(winHandle, nativeHandle);
    }

    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
    }
}