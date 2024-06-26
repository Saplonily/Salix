using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salix;

public sealed class Shader : GraphicsResource
{
    private IntPtr nativeHandle;
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    [CLSCompliant(false)]
    public unsafe Shader(RenderContext context, byte* vertSource, byte* fragSource)
        : base(context)
    {
        nativeHandle = Interop.SLX_CreateShaderFromGlsl(vertSource, fragSource);
        if (nativeHandle == IntPtr.Zero) Interop.Throw();
    }

    public unsafe Shader(RenderContext context, ReadOnlySpan<byte> vertSource, ReadOnlySpan<byte> fragSource)
        : base(context)
    {
        fixed (byte* vptr = vertSource)
        fixed (byte* fptr = fragSource)
        {
            nativeHandle = Interop.SLX_CreateShaderFromGlsl(vptr, fptr);
            if (nativeHandle == IntPtr.Zero) Interop.Throw();
        }
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
        if (Interop.SLX_DeleteShader(nativeHandle))
            Interop.Throw();
        nativeHandle = IntPtr.Zero;
    }

    private int GetParameterLocation(string name)
    {
        EnsureState();
        EnsureCurrentState();
#if NETSTANDARD2_1_OR_GREATER
        return Interop.SLX_GetShaderParamLocation(nativeHandle, name);
#else
        byte[] utf8NameBytes = Encoding.UTF8.GetBytes(name);
        unsafe
        {
            fixed (byte* ptr = utf8NameBytes)
            {
                return Interop.SLX_GetShaderParamLocation(nativeHandle, ptr);
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
                return Interop.SLX_GetShaderParamLocation(nativeHandle, ptr);
            }
        }
    }

    public unsafe void SetParameter<T>(ShaderParameter param, ref T value) where T : unmanaged
    {
        if (!ReferenceEquals(param.Shader, this))
            throw new ArgumentException(SR.UnmatchedShaderParamOwner, nameof(param));
        EnsureState();
        EnsureCurrentState();
        int location = param.Location;

        // our jit god will do the optimization

        if (typeof(T) == typeof(int))
        {
            if (Interop.SLX_SetShaderParamInt(location, Unsafe.As<T, int>(ref value)))
                Interop.Throw();
            return;
        }

        if (typeof(T) == typeof(bool))
        {
            if (Interop.SLX_SetShaderParamInt(location, Unsafe.As<T, bool>(ref value) ? 1 : 0))
                Interop.Throw();
            return;
        }

        if (typeof(T) == typeof(float))
        {
            if (Interop.SLX_SetShaderParamFloat(location, Unsafe.As<T, float>(ref value)))
                Interop.Throw();
            return;
        }

        if (typeof(T) == typeof(Vector4))
        {
            ref var vec = ref Unsafe.As<T, Vector4>(ref value);
            if (Interop.SLX_SetShaderParamVec4(location, (float*)Unsafe.AsPointer(ref vec)))
                Interop.Throw();
            return;
        }

        if (typeof(T) == typeof(Matrix3x2))
        {
            ref var mat = ref Unsafe.As<T, Matrix3x2>(ref value);
            if (Interop.SLX_SetShaderParamMat3x2(location, (float*)Unsafe.AsPointer(ref mat)))
                Interop.Throw();
            return;
        }

        if (typeof(T) == typeof(Matrix4x4))
        {
            ref var mat = ref Unsafe.As<T, Matrix4x4>(ref value);
            if (Interop.SLX_SetShaderParamMat4(location, (float*)Unsafe.AsPointer(ref mat)))
                Interop.Throw();
            return;
        }

        throw new NotSupportedException(string.Format(SR.TypeNotSupportedInShader, typeof(T)));
    }

    private void EnsureCurrentState()
        => ThrowHelper.ThrowIfInvalid(RenderContext.Shader != this, SR.ShaderRequiredCurrent);
}