using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using Texel.Models;

namespace Texel.Services
{
    public class MinecraftAssetsService
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/InventivetalentDev/minecraft-assets/branches";
        private const string RAW_CONTENT_URL = "https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{0}/assets/minecraft/textures/";
        
        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Texel Editor", 
            "Cache");
            
        private readonly HttpClient _httpClient;
        
        private System.Windows.Forms.ToolStripMenuItem toggleGridToolStripMenuItem;

        public MinecraftAssetsService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Texel-Editor");
            
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            this.toggleGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleGridToolStripMenuItem.Name = "toggleGridToolStripMenuItem";
            this.toggleGridToolStripMenuItem.Text = "Toggle Grid";
            this.toggleGridToolStripMenuItem.CheckOnClick = true;
        }
        
        public async Task<List<string>> GetAvailableVersionsAsync()
        {
            var versions = new List<string>();
            
            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                var branches = JsonConvert.DeserializeObject<List<BranchInfo>>(response);
                
                foreach (var branch in branches)
                {
                    if (branch.Name.StartsWith("1.") && !branch.Name.Contains("-pre") && !branch.Name.Contains("-rc"))
                    {
                        versions.Add(branch.Name);
                    }
                }
                
                // Sort versions in descending order (newest first)
                versions.Sort((a, b) => CompareVersions(b, a));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch Minecraft versions: {ex.Message}", ex);
            }
            
            return versions;
        }
        
        public async Task DownloadTexturesAsync(string version, IProgress<int> progress = null)
        {
            var versionCachePath = Path.Combine(CachePath, version);
            
            if (!Directory.Exists(versionCachePath))
            {
                Directory.CreateDirectory(versionCachePath);
            }
            
            // Get the list of textures for this version
            var texturesList = await GetTexturesListAsync(version);
            
            int totalTextures = texturesList.Count;
            int downloadedTextures = 0;
            
            foreach (var texturePath in texturesList)
            {
                string localPath = Path.Combine(versionCachePath, texturePath);
                string directory = Path.GetDirectoryName(localPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                if (!File.Exists(localPath))
                {
                    string textureUrl = string.Format(RAW_CONTENT_URL, version) + texturePath;
                    await DownloadFileAsync(textureUrl, localPath);
                }
                
                downloadedTextures++;
                progress?.Report((int)(downloadedTextures * 100.0 / totalTextures));
            }
        }
        
        private async Task<List<string>> GetTexturesListAsync(string version)
        {
            // This would ideally crawl the repository to get all texture paths
            // For simplicity, we'll just return a predefined list of common textures
            // In a real implementation, you'd parse the repository structure
            
            var textures = new List<string>
            {
                "block/stone.png",
                "block/dirt.png",
                "block/grass_block_side.png",
                "block/grass_block_top.png",
                "item/diamond.png",
                "item/apple.png",
                // Add more textures as needed
            };
            
            return textures;
        }
        
        private async Task DownloadFileAsync(string url, string outputPath)
        {
            byte[] fileData = await _httpClient.GetByteArrayAsync(url);
            
            // Use FileStream with async methods instead of File.WriteAllBytesAsync
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 
                                                  bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(fileData, 0, fileData.Length);
            }
        }
        
        public string GetCachedTexturePath(string version, string texturePath)
        {
            return Path.Combine(CachePath, version, texturePath);
        }
        
        public bool IsTextureAvailableInCache(string version, string texturePath)
        {
            return File.Exists(GetCachedTexturePath(version, texturePath));
        }
        
        private int CompareVersions(string versionA, string versionB)
        {
            var partsA = versionA.Split('.');
            var partsB = versionB.Split('.');
            
            for (int i = 0; i < Math.Min(partsA.Length, partsB.Length); i++)
            {
                if (int.TryParse(partsA[i], out int numA) && int.TryParse(partsB[i], out int numB))
                {
                    int comparison = numA.CompareTo(numB);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
                else
                {
                    int comparison = string.Compare(partsA[i], partsB[i], StringComparison.Ordinal);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
            }
            
            return partsA.Length.CompareTo(partsB.Length);
        }
        
        private class BranchInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}