using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: CLSCompliant(true)]
#if !NETSTANDARD2_1
[assembly: SupportedOSPlatform("windows")]
#endif

// for some uncomplete api
[assembly: InternalsVisibleTo("Test.Win32")]