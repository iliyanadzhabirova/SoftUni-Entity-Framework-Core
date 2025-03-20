using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductShop.DTOs.Import
{
    public class ImportUserDto
    {
        [JsonProperty("firstName")]
        public string? FirstName { get; set; }
        [Required]
        [JsonProperty("lastName")]   
        public string LastName { get; set; } = null!;
        [JsonProperty("age")]
        public string? Age { get; set; }
    }
}
