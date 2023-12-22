# ChaosLib

ChaosLib is a collection of utility classes used for developing diagnostic applications. It is primarily designed to store all components that are shared between [DebugTools](https://github.com/lordmilko/DebugTools) and [ChaosDbg](https://github.com/lordmilko/ChaosDbg]

Useful utilities contained in ChaosLib include

* PInvoke wrappers that return `HRESULT` values and/or throw exceptions
* A custom `Dispatcher` / `DispatcherThread` modelled on `System.Windows.Threading` that acts as a producer/consumer for executing certain operations on a dedicated thread
* Duck typing (`new Foo().As<IBar>()`)
* Kernel handle enumeration
* `Stream` implementations that serve as building blocks for reading in-process memory
* A mockable PE Reader based on this [sample](https://github.com/lordmilko/ClrDebug/tree/master/Samples/PEReader) from ClrDebug
* A mockable runner for executing command line executables and streaming their output back to your process
* A mockable SigBlob reader for reading CLR signature blobs
* A runner for programmatically capturing Time Travel Debugging traces. Provides a means to get at secret functionality that is normally locked down in normal builds of `ttd.exe` / `tttracer.exe`
* A typed data data model. Allows exploring and interacting with arbitrary data structures defined in debug symbols. Essentially an enhanced `ExtRemoteTyped` from `EngExtCpp`
* A mechanism for programmatically attaching or detaching the Visual Studio debugger debugging *your process* to *other processes* (such as child processes your program delivers that host out-of-proc services)
* Resolvers capable of locating DbgEng and DbgShim
* and more

As mentioned, this library is primarily designed to be a shared component that is used by my other projects. Absolutely no guarantee is provided as to the robustness or backwards compatibility of any of the releases provided by this repo.