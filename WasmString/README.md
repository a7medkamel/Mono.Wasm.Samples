# WasmString

Sample code demonstrating different ways to pass strings back and forth.

Live test can be found here: https://kjpou1.github.io/wasmstring/

## Requirements

- [WebAssembly SDK](https://github.com/mono/mono/blob/master/sdks/wasm/docs/getting-started/obtain-wasm-sdk.md) - Right now the WebAssembly sdk is packaged in a zip file and can be downloaded and unzipped ready to go from the build server.  We are working on distribution and easier setup in future steps.

## Create sample

1. Compile sample 

    This will compile into `./bin` directory so you may need to create first.

    ``` bash

    csc /nostdlib /noconfig /nologo /langversion:latest -target:library -out:./bin/WasmString.dll /r:$WASM_SDK/out/wasm-bcl/wasm/mscorlib.dll /r:$WASM_SDK/out/wasm-bcl/wasm/System.Core.dll /r:$WASM_SDK/out/wasm-bcl/wasm/System.dll /r:$WASM_SDK/out/wasm-bcl/wasm/Facades/netstandard.dll /r:$WASM_SDK/out/wasm-bcl/wasm/Facades/System.Memory.dll /r:$WASM_SDK/wasm/framework/WebAssembly.Bindings.dll /r:$WASM_SDK/wasm/framework/WebAssembly.Net.Http.dll Program.cs

    ```

1. Package and publish sample 

    ``` bash

    mono $WASM_SDK/packager.exe  --copy=ifnewer --out=publish --asset=index.html --asset=server.py  ./bin/WasmString.dll

    ```

## Serving sample

Follow the [sample instructions](https://github.com/mono/mono/blob/master/sdks/wasm/docs/getting-started/sample.md#serving-sample).

## Green buttons

The Green buttons use the WebAssembly bindings to marshal data types back and forth.

- **Pointer:** creates a string and using a pinned byte array, calls a javascript function with the pointer to the string to be displayed.

   ``` javascript

            onPtrClick: function () {
                BINDING.call_static_method("[WasmString]WasmString.Program:Ptr", []);
                console.log("Total Memory: " + Module.HEAPU8.byteLength);
            },

   ```

   ``` csharp

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

   ```

   ``` javascript

        function pointer_alert(length, ptrMessage) {
            // The ptrMessage is the address of the pinned bytes from the managed side
            var s = Module.UTF8ToString(ptrMessage, length);
            alert(s);
        }

   ```

- **String:** creates a string calls a javascript function marshalling the string to the function.

   ``` javascript
   
            onStringClick: function () {
                BINDING.call_static_method("[WasmString]WasmString.Program:MarshalString", []);
                console.log("Total Memory: " + Module.HEAPU8.byteLength);
            },

   ```

   ``` csharp

        public static void MarshalString()
        {
            stringAlert.Call(null, "Hello from MarshalString");
        }

   ```

   ``` javascript

        function string_alert(strMessage) {
            // The strMessage is the marshalled string value
            alert(strMessage);
        }

   ```   

- **Heap Malloc:** allocates memory on wasm heap and passes the pointer to the C# managed.  **Note** the developer is responsible for allocating the wasm heap AND FREEING that memory from javascript.

   ``` javascript
   
            onMallocClick: function () {
                // Warning:  When using the malloc from the javascript side you are resposible
                // for freeing memory on the javascript side.
                var ptr = Module._malloc(50000);
                BINDING.call_static_method("[WasmString]WasmString.Program:Malloc", [ptr]);
                Module._free(ptr);
                console.log("Total Memory: " + Module.HEAPU8.byteLength);
            },

   ```

   ``` csharp

        public static void Malloc(int ptr)
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hic sunt Dracones!!!!");
            Marshal.Copy(helloBuffer, 0, (IntPtr)ptr, helloBuffer.Length);
            mallocAlert.Call(null, helloBuffer.Length, ptr);
        }

   ```

   ``` javascript

        function malloc_alert(length, ptrMessage) {
            // The ptrMessage is the address of the heap allocated from javascript
            var s = Module.UTF8ToString(ptrMessage, length);
            alert(s);
        }

   ```   

- **Uint8Array:** copies the string bytes to a Uint8Array and then calls the javascript function.  **Note** Not necessarily the best way to do this but can be done.  This uses a lot of memory transferring to/from the heap.

   ``` javascript

             onBufferClick: function () {
                BINDING.call_static_method("[WasmString]WasmString.Program:ByteArray", []);
                console.log("Total Memory: " + Module.HEAPU8.byteLength);
            },

   ```

   ``` csharp

        public static void ByteArray ()
        {
            var helloBuffer = Encoding.UTF8.GetBytes("Hello from Uint8Array");
            byteArrayBuffer.CopyFrom(helloBuffer);
            arrayAlert.Call(null, helloBuffer.Length);
        }

   ```

   ``` javascript

        function array_alert(length) {
            var numBytes = length * MyWorkBuffer.BYTES_PER_ELEMENT;
            var ptr = Module._malloc(numBytes);
            var heapBytes = new Uint8Array(Module.HEAPU8.buffer, ptr, numBytes);
            heapBytes.set(new Uint8Array(MyWorkBuffer.buffer, MyWorkBuffer.byteOffset, numBytes));
            var s = Module.UTF8ToString(ptr, length);
            Module._free(heapBytes.byteOffset);
            alert(s);
        }

   ```      