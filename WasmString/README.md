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