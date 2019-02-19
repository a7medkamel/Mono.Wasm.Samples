
TOP=$(realpath $(CURDIR))

CSC_FLAGS := /nostdlib /noconfig /nologo /langversion:latest
CSC_FLAGS_DEBUG := /debug:portable /noconfig /nostdlib /nologo /langversion:latest

BCL_DEPS=/r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/Facades/netstandard.dll 

ASSETS = \
    --asset=index.html   \

$(TOP)/mono/:
	mkdir -p $@

.stamp-wasm-sdk: | $(TOP)/mono/

	curl -L 'https://jenkins.mono-project.com/job/test-mono-mainline-wasm/label=ubuntu-1804-amd64/lastSuccessfulBuild/Azure/processDownloadRequest/2109/ubuntu-1804-amd64/sdks/wasm/mono-wasm-66092e05712.zip' -o "mono-wasm.zip" -#
	unzip mono-wasm.zip -d $(TOP)/mono/sdks
	touch $@

.PHONY: wasmsdk
wasmsdk: .stamp-wasm-sdk

# build:
# 	msbuild /r WasmWebSockets.sln

# rebuild: clean
# 	msbuild /r WasmWebSockets.sln

# rebuild-release: clean
# 	msbuild /r /p:_DebugSymbolsProduced=false

clean:
	rm -rf ./bin && rm -rf ./obj rm -rf publish

bin/:
	mkdir -p $@

WasmClientWebSocketTest.dll: Client.cs | bin
	csc $(CSC_FLAGS) -target:library -out:./bin/$@ $(BCL_DEPS) /r:$(WASM_DIR_FRAMEWORK)/WebAssembly.Bindings.dll /r:$(WASM_DIR_FRAMEWORK)/WebAssembly.Net.WebSockets.dll Client.cs

gen-runtime:
	mono $(WASM_DIR_PACKAGER)/packager.exe ${DRIVER_CONF} --copy=ifnewer --out=publish --search-path=./managed/ ${ASSETS} ./bin/WasmClientWebSocketTest.dll

build-managed: WasmClientWebSocketTest.dll gen-runtime

build-csc: set_wasm_dir build-managed

build-sdks: wasmsdk set_wasm_sdk_dir build-managed

set_wasm_dir:
	$(eval WASM_DIR=$(WASM_SDK)/out/wasm-bcl/wasm)
	$(eval WASM_DIR_FRAMEWORK=$(WASM_SDK)/wasm/framework)
	$(eval WASM_DIR_PACKAGER=$(WASM_SDK)/wasm)
	$(eval WASM_BCL_DIR=$(WASM_SDK)/out/wasm-bcl/wasm)

set_wasm_sdk_dir:
	echo $(TOP)
	$(eval WASM_DIR=$(TOP)/mono/sdks/wasm-bcl/wasm)
	$(eval WASM_DIR_FRAMEWORK=$(TOP)/mono/sdks/framework)
	$(eval WASM_DIR_PACKAGER=$(TOP)/mono/sdks)
	$(eval WASM_BCL_DIR=$(TOP)/mono/sdks/wasm-bcl/wasm)

