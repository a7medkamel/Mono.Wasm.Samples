using System;
using System.Runtime.InteropServices;
using System.Text;
using WebAssembly;
using WebAssembly.Core;

namespace WasmString
{
    public class Program
    {
        static Function pointerAlert;
        static Function stringAlert;
        static Function mallocAlert;
        static Function arrayAlert;
        static Uint8Array byteArrayBuffer;
        public static void Main(string greeting) {
            Console.WriteLine($"Hello {greeting}");

            // Obtain a reference to the javascript function
            pointerAlert = (Function)Runtime.GetGlobalObject("pointer_alert");
            // Obtain a reference to the javascript function
            stringAlert = (Function)Runtime.GetGlobalObject("string_alert");
            // Obtain a reference to the javascript function
            mallocAlert = (Function)Runtime.GetGlobalObject("malloc_alert");
            // Obtain a reference to the javascript function
            arrayAlert = (Function)Runtime.GetGlobalObject("array_alert");
            // Obtain a reference to the javascript function
            byteArrayBuffer = (Uint8Array)Runtime.GetGlobalObject("MyWorkBuffer");
        }
        public static void Ptr()
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hello from Ptr");

            GCHandle gch = GCHandle.Alloc(helloBuffer, GCHandleType.Pinned);
            var gchAddress = gch.AddrOfPinnedObject();

            try
            {
                pointerAlert.Call(null, helloBuffer.Length, gchAddress.ToInt32());
            }
            finally
            {
                gch.Free();
            }
        }

        public static void PtrEval()
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hello from PtrEval");

            GCHandle gch = GCHandle.Alloc(helloBuffer, GCHandleType.Pinned);
            var gchAddress = gch.AddrOfPinnedObject();

            try
            {
                Runtime.InvokeJS($"pointer_alert({helloBuffer.Length}, {gchAddress})");
            }
            finally
            {
                gch.Free();
            }
        }
        public static void PtrRawEval()
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hello from PtrRawEval");

            GCHandle gch = GCHandle.Alloc(helloBuffer, GCHandleType.Pinned);
            var gchAddress = gch.AddrOfPinnedObject();

            try
            {
                Runtime.InvokeJS($"pointer_alert({helloBuffer.Length}, {gchAddress})");
            }
            finally
            {
                gch.Free();
            }
        }

        public static void MarshalString()
        {
            stringAlert.Call(null, "Hello from MarshalString");
        }
        public static void Malloc(int ptr)
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hic sunt Dracones!!!!");
            Marshal.Copy(helloBuffer, 0, (IntPtr)ptr, helloBuffer.Length);
            mallocAlert.Call(null, helloBuffer.Length, ptr);
        }
        public static void MallocEval(int ptr)
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hic sunt Dracones!!!!");
            Marshal.Copy(helloBuffer, 0, (IntPtr)ptr, helloBuffer.Length);
            Runtime.InvokeJS($"malloc_alert({helloBuffer.Length}, {ptr})");
        }
        public static void MallocRawEval(string ptr)
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hic sunt Dracones!!!!");
            var intPtr = Int32.Parse(ptr);
            Marshal.Copy(helloBuffer, 0, (IntPtr)intPtr, helloBuffer.Length);
            Runtime.InvokeJS($"malloc_alert({helloBuffer.Length}, {ptr})");
        }
        public static void ByteArray ()
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hello from Uint8Array");
            byteArrayBuffer.CopyFrom(helloBuffer);
            arrayAlert.Call(null, helloBuffer.Length);
        }
        public static void LargeString(string engorged)
        {
            Console.WriteLine(engorged.Length);
        }

    }
}
