process.on('uncaughtException', (err) => {
  console.error(err);
});

process.on('unhandledRejection', (reason) => {
  console.error(reason);
});

function DOMElement() {
  this.text = '';
};

Object.defineProperty(DOMElement, 'value', {
  get: function () {
    let ret = this.text || this.innerText;
    console.log('get/text', ret);
    return ret;
  },
  set: function (text) {
    console.log('set/text', text);
    this.text = text; s
  }
});

Object.defineProperty(DOMElement, 'innerText', {
  get: function () {
    let ret = this.text || this.innerText;
    console.log('get/innerText', ret);
    return ret;
  },
  set: function (text) {
    console.log('called', text);
    this.text = text;
  }
});

const compileLog = new DOMElement();
const outputLog = new DOMElement();
const csharpcode = new DOMElement();

const self = this;

let ext = {
  onRuntimeInitialized: function () {
    global.MONO.mono_load_runtime_and_bcl(
      "managed" /* vfs_prefix */,
      "managed" /* deploy_prefix */,
      0 /* enable_debugging */,
      ["WasmRoslyn.dll", "mscorlib.dll", "System.Net.Http.dll", "System.dll", "Mono.Security.dll", "System.Xml.dll", "System.Numerics.dll", "System.Core.dll", "WebAssembly.Net.Http.dll", "netstandard.dll", "System.Data.dll", "System.Transactions.dll", "System.Data.DataSetExtensions.dll", "System.Drawing.Common.dll", "System.IO.Compression.dll", "System.IO.Compression.FileSystem.dll", "System.ComponentModel.Composition.dll", "System.Runtime.Serialization.dll", "System.ServiceModel.Internals.dll", "System.Xml.Linq.dll", "WebAssembly.Bindings.dll", "System.Memory.dll", "Microsoft.CodeAnalysis.dll", "System.Runtime.dll", "System.Diagnostics.Debug.dll", "System.Collections.Immutable.dll", "System.Collections.dll", "System.Reflection.Metadata.dll", "System.Globalization.dll", "System.Reflection.dll", "System.IO.dll", "System.IO.FileSystem.dll", "System.Runtime.Extensions.dll", "System.Collections.Concurrent.dll", "System.Text.Encoding.dll", "System.Linq.dll", "System.Threading.Tasks.dll", "System.Security.Cryptography.Algorithms.dll", "System.Threading.dll", "System.ValueTuple.dll", "System.Xml.XDocument.dll", "System.Security.Cryptography.Primitives.dll", "System.Runtime.InteropServices.dll", "System.Reflection.Primitives.dll", "System.Diagnostics.Tools.dll", "System.Resources.ResourceManager.dll", "System.IO.FileSystem.Primitives.dll", "System.Xml.ReaderWriter.dll", "System.Runtime.Numerics.dll", "System.Threading.Tasks.Extensions.dll", "System.Xml.XPath.XDocument.dll", "System.Reflection.Extensions.dll", "System.Text.Encoding.CodePages.dll", "System.Text.Encoding.Extensions.dll", "Microsoft.CodeAnalysis.CSharp.dll", "System.Linq.Expressions.dll", "System.Threading.Tasks.Parallel.dll", "WebAssembly.Net.WebSockets.dll"],
      function () {
        global.window = self;

        this.location = {
          origin: 'http://example.com',
          pathname: '/wasm_roslyn_node'
        }

        global.fetch = function() {
          console.log('fetch:', arguments);
        };
        // global.fetch = require('node-fetch');
        
        mono.mono_call_static_method("[WasmRoslyn]WasmRoslyn.Program:Main", [compileLog, outputLog, csharpcode]);

        // mono.mono_call_static_method("[WasmRoslyn]WasmRoslyn.Program:Run", [compileLog, outputLog, csharpcode]);
      }
    )
  },
  readBinary: function (filename) {
    return require('fs').readFileSync(filename);
  }
};

var mono = require('./publish/mono');
Object.assign(mono, ext);

setInterval(() => {
  console.log(compileLog.value, outputLog.value, csharpcode.value);
}, 1000);