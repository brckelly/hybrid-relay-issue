namespace Listener
{
	using Microsoft.Azure.Relay;
	using System;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using System.Threading.Tasks;
	using System.Net.Http.Json;

	internal class Program
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine(".NET Version: " + Environment.Version);
			var hostBuilder = Host.CreateDefaultBuilder();
			var host = hostBuilder.Build();

			var configuration = host.Services.GetRequiredService<IConfiguration>();

			var keyName = configuration["keyName"];
			var key = configuration["key"];
			var relayNamespace = configuration["relayNamespace"];
			var connectionName = configuration["connectionName"];
			var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, key);
			var listener = new HybridConnectionListener(new Uri(string.Format("sb://{0}/{1}", relayNamespace, connectionName)), tokenProvider);

			// Subscribe to the status events.
			listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
			listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
			listener.Online += (o, e) => { Console.WriteLine("Online"); };

			listener.RequestHandler += HandleConnection;

			// Opening the listener establishes the control channel to
			// the Azure Relay service. The control channel is continuously 
			// maintained, and is reestablished when connectivity is disrupted.
			await listener.OpenAsync();
			Console.WriteLine("Server listening");

			await host.RunAsync();
		}

		private static async void HandleConnection(RelayedHttpListenerContext context)
		{
			Console.WriteLine("Handling request...");

			var obj = new
			{
				token = "abunchofjunk",
				timeout = TimeSpan.FromMinutes(10)
			};
			var content = JsonContent.Create(obj);

			context.Response.StatusCode = System.Net.HttpStatusCode.OK;
			await content.CopyToAsync(context.Response.OutputStream);
			await context.Response.CloseAsync();
		}
	}
}
