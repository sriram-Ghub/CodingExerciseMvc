using System.Text;
using Application.Web.ConfigFileOperations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Application.ConfigFileOperations.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> GetServerNames()
        {
            var serverNames = new List<string>();
            var httpClient = _httpClientFactory.CreateClient("configFileOperation");
            var url = "servers";
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                serverNames = JsonConvert.DeserializeObject<List<string>>(jsonResponse);
            }

            return View("Settings", serverNames);
        }

        public async Task<IActionResult> GetConfigs(string selectedItem)
        {
            if (string.IsNullOrEmpty(selectedItem))
            {
                selectedItem = "default";
            }
            var configs = new Dictionary<string, string>();
            var httpClient = _httpClientFactory.CreateClient("configFileOperation");
            var url = $"servers/{selectedItem}/configs";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var configModel = JsonConvert.DeserializeObject<ConfigModel>(jsonResponse);

                configs = configModel?.Configs;
            }

            return View("ConfigSettings", configs);
        }

        public async Task<IActionResult> UpdateConfigs(string key, string value)
        {
            var updatedConfig = new Dictionary<string,string>
            {
                [key] = value
            };

            var httpClient = _httpClientFactory.CreateClient("configFileOperation");
            var url = $"servers/default/configs";
            var request = new StringContent(JsonConvert.SerializeObject(updatedConfig), Encoding.UTF8,
                "application/json");
            var response = await httpClient.PatchAsync(url, request);
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetConfigs");
            }

            return BadRequest();
        }
    }
}