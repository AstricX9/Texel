using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using Texel.Models;
using Texel.Classes;

namespace Texel.Services
{
    public class MinecraftAssetsService
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/InventivetalentDev/minecraft-assets/branches";
        private const string GITHUB_TREE_API_URL = "https://api.github.com/repos/InventivetalentDev/minecraft-assets/git/trees/{0}?recursive=1";
        private const string RAW_CONTENT_URL = "https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{0}/assets/minecraft/";

        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Texel Editor",
            "Cache");

        private readonly HttpClient _httpClient;

        public MinecraftAssetsService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Texel-Editor");

            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }
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
                    // Include stable 1.x releases and ignore pre/rc branches. We no longer
                    // filter by a minimum minor version because older 1.x releases are
                    // considered supported by the mapping in MinecraftPacks.UpdatePackFormat.
                    if (!branch.Name.StartsWith("1."))
                        continue;

                    if (branch.Name.Contains("-pre") || branch.Name.Contains("-rc"))
                        continue;

                    versions.Add(branch.Name);
                }

                // Merge with central store to ensure all supported versions are present
                var baseline = MinecraftVersionStore.GetSupportedVersions();
                var merged = new HashSet<string>(versions, StringComparer.OrdinalIgnoreCase);
                foreach (var v in baseline) merged.Add(v);
                versions = merged.ToList();

                // Determine the minimum supported version from the baseline (e.g., 1.8)
                var minSupported = baseline
                    .OrderBy(v => v, Comparer<string>.Create(CompareVersions))
                    .FirstOrDefault() ?? "1.8";

                // Filter out anything older than the minimum supported version
                versions = versions
                    .Where(v => CompareVersions(v, minSupported) >= 0)
                    .ToList();

                // Sort newest-first
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
            Directory.CreateDirectory(versionCachePath);

            var texturePaths = await GetAllTexturePathsAsync(version);
            int totalTextures = texturePaths.Count;
            int downloadedTextures = 0;

            foreach (var texturePath in texturePaths)
            {
                string localPath = Path.Combine(versionCachePath, texturePath.Replace('/', Path.DirectorySeparatorChar));
                string directory = Path.GetDirectoryName(localPath);
                Directory.CreateDirectory(directory);

                if (!File.Exists(localPath))
                {
                    string textureUrl = string.Format(RAW_CONTENT_URL, version) + texturePath;
                    try
                    {
                        await DownloadFileAsync(textureUrl, localPath);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"Failed to download {textureUrl}: {ex.Message}");
                        continue;
                    }
                }

                downloadedTextures++;
                progress?.Report((int)(downloadedTextures * 100.0 / totalTextures));
            }
        }

        private async Task<List<string>> GetAllTexturePathsAsync(string version)
        {
            var url = string.Format(GITHUB_TREE_API_URL, version);
            var json = await _httpClient.GetStringAsync(url);
            var treeResponse = JsonConvert.DeserializeObject<GitTreeResponse>(json);

            var texturePaths = new List<string>();

            foreach (var item in treeResponse.Tree)
            {
                if (item.Type == "blob" &&
                    item.Path.StartsWith("assets/minecraft/textures/") &&
                    (item.Path.EndsWith(".png") || item.Path.EndsWith(".mcmeta")))
                {
                    // Remove "assets/minecraft/" prefix
                    string relativePath = item.Path.Substring("assets/minecraft/".Length);
                    texturePaths.Add(relativePath);
                }
            }

            return texturePaths;
        }

        private async Task DownloadFileAsync(string url, string outputPath)
        {
            byte[] fileData = await _httpClient.GetByteArrayAsync(url);

            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fileStream.WriteAsync(fileData, 0, fileData.Length);
            }
        }

        public string GetCachedTexturePath(string version, string texturePath)
        {
            return Path.Combine(CachePath, version, texturePath.Replace('/', Path.DirectorySeparatorChar));
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
                    if (comparison != 0) return comparison;
                }
                else
                {
                    int comparison = string.Compare(partsA[i], partsB[i], StringComparison.Ordinal);
                    if (comparison != 0) return comparison;
                }
            }

            return partsA.Length.CompareTo(partsB.Length);
        }

        private class BranchInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        private class GitTreeResponse
        {
            [JsonProperty("tree")]
            public List<GitTreeItem> Tree { get; set; }
        }

        private class GitTreeItem
        {
            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}
