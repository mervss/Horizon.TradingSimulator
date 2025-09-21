Horizon Trading Simulator
A simple trading data simulator that updates five symbols (AAPL, MSFT, GOOGL, TSLA, AMZN) every 5 seconds (±2% movement, last 10 ticks retained). Data is exposed via REST, SignalR, and a TCP stream. Output formatting is extensible through dynamically loaded plugins (CSV/JSON) using AssemblyLoadContext.
Requirements
•	.NET 8 SDK
https://dotnet.microsoft.com/download/dotnet/8.0
•	IDE: Visual Studio 2022 (17.8+) / Rider / VS Code
Visual Studio workloads: ASP.NET and web development, .NET desktop development.
•	(Windows + HTTPS) Trust dev certificate:
•	dotnet dev-certs https --trust
Project Layout
src/
  Horizon.Core/            # Domain abstractions (IPriceGenerator, IPriceStore, IClock, etc.)
  Horizon.PriceEngine/     # Generator, in-memory store, background publisher
  Horizon.Host/            # Web host: REST + SignalR hub + TCP server + plugin loader
    Plugins/               # plugin.json + formatter DLLs (copied to output at build)
  Horizon.JsonFormatter/   # IDataFormatter -> JSON line
  Horizon.CsvFormatter/    # IDataFormatter -> CSV line (invariant culture)
  Horizon.SignalRClient/   # Console client for SignalR
  Horizon.TcpClient/       # Console client for TCP stream
tests/
  Horizon.Tests/           # Unit + integration + plugin-loading tests
Defaults (from launchSettings.json):
HTTP http://localhost:5176 · HTTPS https://localhost:7228 · TCP localhost:5055 · Hub /hubs/prices
Setup
git clone https://github.com/mervss/Horizon.TradingSimulator.git
cd Horizon.TradingSimulator
dotnet restore
Ensure Horizon.JsonFormatter and Horizon.CsvFormatter build; their DLLs are copied into src/Horizon.Host/Plugins/ at build and loaded at runtime.
Build & Run
Visual Studio
1.	Open Horizon.TradingSimulator.sln.
2.	Set Horizon.Host as Startup Project.
3.	Run (F5).
o	Swagger: http://localhost:5176/swagger or https://localhost:7228/swagger
o	Host console should log Plugin loaded: ...Horizon.CsvFormatter.dll.
Optional console clients:
•	SignalR: set Horizon.SignalRClient as Startup and run.
•	TCP: set Horizon.TcpClient as Startup and run.
Or use Multiple startup projects (Host + one client).
Command Line
Host
dotnet run --project src/Horizon.Host/Horizon.Host.csproj
SignalR client
dotnet run --project src/Horizon.SignalRClient/Horizon.SignalRClient.csproj -- "http://localhost:5176/hubs/prices"
# or if Host is on HTTPS:
# dotnet run --project src/Horizon.SignalRClient/Horizon.SignalRClient.csproj -- "https://localhost:7228/hubs/prices"
TCP client
dotnet run --project src/Horizon.TcpClient/Horizon.TcpClient.csproj
REST Quick Reference
•	GET /api/prices/{symbol} – latest tick
•	GET /api/prices/{symbol}/history – last 10 ticks
•	GET /health – liveness probe
Try them in Swagger at /swagger.
Plugins (Dynamic Loading)
•	Contract:
•	public interface IDataFormatter {
•	    string FormatPrice(string symbol, decimal price, DateTime timestamp);
•	}
•	Place DLLs + plugin.json in Horizon.Host/Plugins/ (copied to output).
•	Loaded with AssemblyLoadContext; resources are disposed on shutdown.
Run Tests
dotnet test
Included:
•	Unit: generator behavior (±2%, determinism, metadata), store keeps last 10.
•	Integration: REST via WebApplicationFactory (latest/history/health).
•	Plugin loading: dynamic discovery + invocation through AssemblyLoadContext.
Troubleshooting
•	Swagger 404: verify Host is running and port matches the selected profile.
•	SignalR cannot connect: use the correct hub URL/profile; for HTTPS trust dev certs; behind strict proxies enable LongPolling:
•	.WithUrl(url, o => o.Transports =
•	    Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling)
•	No TCP output: check Host log for Plugin loaded and confirm client connects to localhost:5055.
•	File lock during plugin tests: unload uses AssemblyLoadContext; tests already retry after GC.Collect()—rebuild if a lock persists.
