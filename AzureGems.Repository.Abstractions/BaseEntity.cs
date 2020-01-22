using Newtonsoft.Json;

namespace AzureGems.Repository.Abstractions
{
	public abstract class BaseEntity
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		public string Discriminator { get; set; }
	}
}
