using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monosand;

public sealed class Shader : GraphicsResource
{
    private IntPtr nativeHandle;
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    [CLSCompliant(false)]
    public unsafe Shader(RenderContext context, byte* vertSource, byte* fragSource)
        : base(context)
    {
        nativeHandle = Interop.MsdgCreateShaderFromGlsl(vertSource, fragSource);
    }

    public unsafe Shader(RenderContext context, ReadOnlySpan<byte> vertSource, ReadOnlySpan<byte> fragSource)
        : base(context)
    {
        fixed (byte* vptr = vertSource)
        fixed (byte* fptr = fragSource)
            nativeHandle = Interop.MsdgCreateShaderFromGlsl(vptr, fptr);
    }

    public ShaderParameter GetParameter(string name)
        => new(this, GetParameterLocation(name));

    public ShaderParameter GetParameter(ReadOnlySpan<byte> nameUtf8)
        => new(this, GetParameterLocation(nameUtf8));

    public void Use()
        => RenderContext.Shader = this;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Interop.MsdgDeleteShader(nativeHandle);
        nativeHandle = IntPtr.Zero;
    }

    private int GetParameterLocation(string name)
    {
        EnsureState();
        EnsureCurrentState();
#if NETSTANDARD2_1_OR_GREATER
        return Interop.MsdgGetShaderParamLocation(nativeHandle, name);
#else
        byte[] utf8NameBytes = Encoding.UTF8.GetBytes(name);
        unsafe
        {
            fixed (byte* ptr = utf8NameBytes)
            {
                return Interop.MsdgGetShaderParamLocation(nativeHandle, ptr);
            }
        }
#endif
    }

    private int GetParameterLocation(ReadOnlySpan<byte> nameUtf8)
    {
        EnsureState();
        EnsureCurrentState();
        unsafe
        {
            fixed (byte* ptr = nameUtf8)
            {
                return Interop.MsdgGetShaderParamLocation(nativeHandle, ptr);
            }
        }
    }

    public unsafe void SetParameter<T>(ShaderParameter param, ref T value) where T : unmanaged
    {
        ThrowHelper.ThrowIfArgInvalid(param.Shader != this, nameof(param));
        EnsureState();
        EnsureCurrentState();
        int location = param.Location;

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

        if (typeof(T) == typeof(Matrix3x2))
        {
            ref var mat = ref Unsafe.As<T, Matrix3x2>(ref value);
            Interop.MsdgSetShaderParamMat3x2(location, (float*)Unsafe.AsPointer(ref mat));
            return;
        }

        if (typeof(T) == typeof(Matrix4x4))
        {
            ref var mat = ref Unsafe.As<T, Matrix4x4>(ref value);
            Interop.MsdgSetShaderParamMat4(location, (float*)Unsafe.AsPointer(ref mat));
            return;
        }

        throw new NotSupportedException($"Type of {typeof(T)} is not supported in shader parameter.");
    }

    private void EnsureCurrentState()
        => ThrowHelper.ThrowIfInvalid(RenderContext.Shader != this, "This operation required this shader to be current.");
}