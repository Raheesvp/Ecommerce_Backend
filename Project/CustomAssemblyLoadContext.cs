using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Project.WebAPI
{
    public class CustomAssemblyLoadContext :AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            
            return NativeLibrary.Load(absolutePath);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            
            return IntPtr.Zero;
        }
    }
}
