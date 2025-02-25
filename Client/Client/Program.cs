namespace Client
{
	using Microsoft.Azure.Relay;
	using System;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using System.Threading.Tasks;
	using System.Net.Http.Json;
	using System.Net;
	using System.Net.Http;

	internal class Program
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine(".NET Version: " + Environment.Version);
			var hostBuilder = Host.CreateDefaultBuilder();
			hostBuilder.ConfigureServices(services =>
			{
				services.AddHttpClient("hybrid");
			});

			var host = hostBuilder.Build();

			var configuration = host.Services.GetRequiredService<IConfiguration>();

			var keyName = configuration["keyName"];
			var key = configuration["key"];
			var relayNamespace = configuration["relayNamespace"];
			var connectionName = configuration["connectionName"];
			var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, key);

			var address = $"https://{relayNamespace}/{connectionName}/";
			var token = await tokenProvider.GetTokenAsync(address, TimeSpan.FromMinutes(20));

			var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
			var client = httpClientFactory.CreateClient("hybrid");
			client.BaseAddress = new Uri(address);

			client.DefaultRequestHeaders.Add("ServiceBusAuthorization", token.TokenString);

			try
			{
				var response = await client.PostAsync("test", JsonContent.Create("testing"));
				var str = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
				Console.WriteLine(str);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
