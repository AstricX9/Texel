using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace Texel.Models
{
    [Serializable]
    public class MinecraftPack
    {
        public string Name { get; set; } = "New Resource Pack";
        public string Description { get; set; } = "Made with Texel Editor";
        public string Version { get; set; } = "1.20.1";
        public int Resolution { get; set; } = 16;
        public int PackFormat { get; set; } = 15; // Auto-calculated based on version
        public string ProjectPath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public string IconPath { get; set; } // Path to pack.png
        public bool AutoSave { get; set; } = false;
        public int AutoSaveInterval { get; set; } = 5; // Minutes
        
        [JsonIgnore]
        public bool IsDirty { get; set; } = false;
        
        // For storing the current editing state
        [JsonIgnore] 
        public List<EditorAction> UndoStack { get; set; } = new List<EditorAction>();
        [JsonIgnore]
        public List<EditorAction> RedoStack { get; set; } = new List<EditorAction>();
        
        public string GetAssetsPath()
        {
            return Path.Combine(ProjectPath, "assets", "minecraft");
        }
        
        public string GetTexturesPath()
        {
            return Path.Combine(GetAssetsPath(), "textures");
        }
        
        public string GetPackMetaPath()
        {
            return Path.Combine(ProjectPath, "pack.mcmeta");
        }
        
        public string GetPackIconPath()
        {
            return Path.Combine(ProjectPath, "pack.png");
        }
        
        public void UpdatePackFormat()
        {
            // Based on Minecraft version, update pack_format
            // Reference: https://minecraft.fandom.com/wiki/Resource_pack#Pack_format
            if (Version.StartsWith("1.20.2") || Version.StartsWith("1.20.3")) PackFormat = 18;
            else if (Version.StartsWith("1.20")) PackFormat = 15;
            else if (Version.StartsWith("1.19.4")) PackFormat = 13;
            else if (Version.StartsWith("1.19")) PackFormat = 9;
            else if (Version.StartsWith("1.18")) PackFormat = 8;
            else if (Version.StartsWith("1.17")) PackFormat = 7;
            else if (Version.StartsWith("1.16")) PackFormat = 6;
            else if (Version.StartsWith("1.15")) PackFormat = 5;
            else if (Version.StartsWith("1.14")) PackFormat = 4;
            else if (Version.StartsWith("1.13")) PackFormat = 4;
            else if (Version.StartsWith("1.12")) PackFormat = 3;
            else if (Version.StartsWith("1.11")) PackFormat = 3;
            else if (Version.StartsWith("1.10")) PackFormat = 2;
            else if (Version.StartsWith("1.9")) PackFormat = 2;
            else if (Version.StartsWith("1.8")) PackFormat = 1;
            else PackFormat = 1;
        }
        
        public void GeneratePackMcmeta()
        {
            UpdatePackFormat();
            
            var json = new
            {
                pack = new
                {
                    pack_format = PackFormat,
                    description = Description
                }
            };
            
            string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(GetPackMetaPath(), jsonString);
        }
    }
    
    [Serializable]
    public class EditorAction
    {
        public string TexturePath { get; set; }
        public Color[,] Before { get; set; }
        public Color[,] After { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Description { get; set; }
    }
}
