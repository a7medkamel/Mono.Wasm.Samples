# WasmRoslyn

Sample code demonstrating how to Compile and Run C# in Browser

Live test can be found here: https://kjpou1.github.io/WasmRoslyn/

## Requirements

- [WebAssembly SDK](https://github.com/mono/mono/blob/master/sdks/wasm/docs/getting-started/obtain-wasm-sdk.md) - Right now the WebAssembly sdk is packaged in a zip file and can be downloaded and unzipped ready to go from the build server.  We are working on distribution and easier setup in future steps.

## Create sample

1. Compile sample 

    This will compile into `./bin` directory so you may need to create first.

    ``` bash

    csc /nostdlib /noconfig /nologo /langversion:latest -target:library -out:./bin/WasmRoslyn.dll /r:$WASM_SDK/wasm-bcl/wasm/mscorlib.dll /r:$WASM_SDK/wasm-bcl/wasm/System.Core.dll /r:$WASM_SDK/wasm-bcl/wasm/System.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.Runtime.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.IO.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.Collections.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.Text.Encoding.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.Threading.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/System.Threading.Tasks.dll /r:$WASM_SDK/wasm-bcl/wasm/System.Net.Http.dll /r:$WASM_SDK/wasm-bcl/wasm/Facades/netstandard.dll  /r:$WASM_SDK/framework/WebAssembly.Bindings.dll /r:$WASM_SDK/framework/WebAssembly.Net.Http.dll /r:./managed/Microsoft.CodeAnalysis.CSharp.dll /r:./managed/Microsoft.CodeAnalysis.dll /r:./managed/System.Collections.Immutable.dll Program.cs CompileService.cs

    ```

1. Package and publish sample 

    ``` bash

    mono $WASM_SDK/packager.exe  --copy=ifnewer --out=publish --search-path=./managed/ --asset=index.html --asset=RunClass.txt  ./bin/WasmRoslyn.dll

    ```

## Serving sample

Follow the [sample instructions](https://github.com/mono/mono/blob/master/sdks/wasm/docs/getting-started/sample.md#serving-sample).