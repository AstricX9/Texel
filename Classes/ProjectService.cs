using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression; // Make sure this reference is included
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Texel.Models;

namespace Texel.Services
{
    public class ProjectService
    {
        private const string PROJECT_FILE_EXTENSION = ".mcpackproj";
        private const string RECENT_PROJECTS_FILE = "recent_projects.json";
        private const int MAX_RECENT_PROJECTS = 10;
        
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Texel Editor");
            
        private List<string> _recentProjects;
        private System.Timers.Timer _autoSaveTimer;
        
        public MinecraftPack CurrentPack { get; private set; }
        
        public event EventHandler<MinecraftPack> ProjectLoaded;
        public event EventHandler<MinecraftPack> ProjectSaved;
        public event EventHandler<string> ErrorOccurred;
        
        public ProjectService()
        {
            EnsureAppDataFolderExists();
            LoadRecentProjects();
            
            _autoSaveTimer = new System.Timers.Timer();
            _autoSaveTimer.Elapsed += (s, e) => 
            {
                if (CurrentPack != null && CurrentPack.AutoSave && CurrentPack.IsDirty)
                {
                    try 
                    {
                        SaveProject(CurrentPack);
                    }
                    catch (Exception ex)
                    {
                        OnErrorOccurred($"Auto-save failed: {ex.Message}");
                    }
                }
            };
        }
        
        private void EnsureAppDataFolderExists()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
        }
        
        private void LoadRecentProjects()
        {
            string filePath = Path.Combine(AppDataPath, RECENT_PROJECTS_FILE);
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _recentProjects = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }
                catch
                {
                    _recentProjects = new List<string>();
                }
            }
            else
            {
                _recentProjects = new List<string>();
            }
        }
        
        private void SaveRecentProjects()
        {
            string filePath = Path.Combine(AppDataPath, RECENT_PROJECTS_FILE);
            string json = JsonConvert.SerializeObject(_recentProjects);
            File.WriteAllText(filePath, json);
        }
        
        public void AddToRecentProjects(string projectPath)
        {
            // Remove it if exists (to move to top)
            _recentProjects.Remove(projectPath);
            
            // Add to beginning
            _recentProjects.Insert(0, projectPath);
            
            // Trim to max size
            if (_recentProjects.Count > MAX_RECENT_PROJECTS)
                _recentProjects = _recentProjects.Take(MAX_RECENT_PROJECTS).ToList();
                
            SaveRecentProjects();
        }
        
        public List<string> GetRecentProjects()
        {
            // Filter to only existing projects
            return _recentProjects.Where(File.Exists).ToList();
        }
        
        public MinecraftPack CreateNewProject(string name, string description, string version, 
                                            int resolution, string projectPath, string iconPath = null)
        {
            var pack = new MinecraftPack
            {
                Name = name,
                Description = description,
                Version = version,
                Resolution = resolution,
                ProjectPath = projectPath,
                IconPath = iconPath
            };
            
            pack.UpdatePackFormat();
            
            // Create directory structure
            Directory.CreateDirectory(pack.GetTexturesPath());
            
            // Create standard folders
            foreach (string folder in new[] { "block", "item", "entity", "gui", "model", "font", "environment" })
            {
                Directory.CreateDirectory(Path.Combine(pack.GetTexturesPath(), folder));
            }
            
            // Generate pack.mcmeta
            pack.GeneratePackMcmeta();
            
            // Copy icon if provided
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {
                File.Copy(iconPath, pack.GetPackIconPath(), true);
            }
            
            // Save project file
            SaveProject(pack);
            
            // Add to recent projects
            string projectFilePath = GetProjectFilePath(projectPath);
            AddToRecentProjects(projectFilePath);
            
            CurrentPack = pack;
            OnProjectLoaded(pack);
            
            // Setup auto-save timer
            UpdateAutoSaveTimer(pack);
            
            return pack;
        }
        
        public void SaveProject(MinecraftPack pack)
        {
            string projectFilePath = GetProjectFilePath(pack.ProjectPath);
            string json = JsonConvert.SerializeObject(pack, Formatting.Indented);
            File.WriteAllText(projectFilePath, json);
            
            pack.ModifiedDate = DateTime.Now;
            pack.IsDirty = false;
            
            OnProjectSaved(pack);
            AddToRecentProjects(projectFilePath);
        }
        
        public void SaveProjectAs(MinecraftPack pack, string newPath)
        {
            // Create the new directory if it doesn't exist
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            
            // Copy all files from old location to new location
            foreach (string dirPath in Directory.GetDirectories(pack.ProjectPath, "*", 
                                                              SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(pack.ProjectPath, newPath));
            }

            foreach (string filePath in Directory.GetFiles(pack.ProjectPath, "*.*", 
                                                          SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(pack.ProjectPath, newPath), true);
            }
            
            // Update project path
            pack.ProjectPath = newPath;
            
            // Save project file
            SaveProject(pack);
        }
        
        public MinecraftPack LoadProject(string path)
        {
            string projectFilePath;
            
            // Determine if we're opening a .mcpackproj file or a directory
            if (Directory.Exists(path))
            {
                projectFilePath = GetProjectFilePath(path);
                
                // Check if there's a pack.mcmeta but no .mcpackproj
                if (!File.Exists(projectFilePath) && File.Exists(Path.Combine(path, "pack.mcmeta")))
                {
                    // Create a new project file for this existing resource pack
                    var packName = new DirectoryInfo(path).Name;
                    
                    var newPack = new MinecraftPack
                    {
                        Name = packName,
                        ProjectPath = path,
                        IconPath = File.Exists(Path.Combine(path, "pack.png")) ? 
                                  Path.Combine(path, "pack.png") : null
                    };
                    
                    // Try to read description and pack_format from pack.mcmeta
                    try
                    {
                        string mcmetaJson = File.ReadAllText(Path.Combine(path, "pack.mcmeta"));
                        dynamic mcmeta = JsonConvert.DeserializeObject(mcmetaJson);
                        
                        if (mcmeta?.pack?.description != null)
                        {
                            newPack.Description = mcmeta.pack.description.ToString();
                        }
                        
                        if (mcmeta?.pack?.pack_format != null)
                        {
                            int packFormat = (int)mcmeta.pack.pack_format;
                            newPack.PackFormat = packFormat;
                            
                            // Set version based on pack_format
                            if (packFormat >= 18) newPack.Version = "1.20.2";
                            else if (packFormat >= 15) newPack.Version = "1.20.1";
                            else if (packFormat >= 13) newPack.Version = "1.19.4";
                            else if (packFormat >= 9) newPack.Version = "1.19.3";
                            else if (packFormat >= 8) newPack.Version = "1.18.2";
                            else if (packFormat >= 7) newPack.Version = "1.17.1";
                            else if (packFormat >= 6) newPack.Version = "1.16.5";
                            else if (packFormat >= 5) newPack.Version = "1.15.2";
                            else if (packFormat >= 4) newPack.Version = "1.14.4";
                            else if (packFormat >= 3) newPack.Version = "1.12.2";
                            else if (packFormat >= 2) newPack.Version = "1.10.2";
                            else newPack.Version = "1.8.9";
                        }
                    }
                    catch
                    {
                        // Use defaults if we can't read the pack.mcmeta
                    }
                    
                    // Save the new project file
                    SaveProject(newPack);
                    return newPack;
                }
            }
            else if (File.Exists(path) && Path.GetExtension(path).ToLower() == PROJECT_FILE_EXTENSION)
            {
                projectFilePath = path;
            }
            else
            {
                throw new FileNotFoundException("Invalid project path. Must be a .mcpackproj file or directory.");
            }
            
            if (!File.Exists(projectFilePath))
            {
                throw new FileNotFoundException("Project file not found", projectFilePath);
            }
            
            string json = File.ReadAllText(projectFilePath);
            var pack = JsonConvert.DeserializeObject<MinecraftPack>(json);
            
            // Add to recent projects
            AddToRecentProjects(projectFilePath);
            
            CurrentPack = pack;
            OnProjectLoaded(pack);
            
            // Setup auto-save timer
            UpdateAutoSaveTimer(pack);
            
            return pack;
        }
        
        public void ExportResourcePack(MinecraftPack packToExport, string exportPath)
        {
            // Make sure everything is up to date
            packToExport.GeneratePackMcmeta();
            
            // Create a temp directory for the export
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                // Copy all files to the temp directory
                foreach (string dirPath in Directory.GetDirectories(packToExport.ProjectPath, "*", 
                                                                  SearchOption.AllDirectories))
                {
                    // Skip the .mcpackproj file
                    if (dirPath.EndsWith(".mcpackproj", StringComparison.OrdinalIgnoreCase))
                        continue;
                        
                    Directory.CreateDirectory(dirPath.Replace(packToExport.ProjectPath, tempDir));
                }

                foreach (string filePath in Directory.GetFiles(packToExport.ProjectPath, "*.*", 
                                                              SearchOption.AllDirectories))
                {
                    // Skip the .mcpackproj file
                    if (Path.GetExtension(filePath).Equals(PROJECT_FILE_EXTENSION, 
                                                          StringComparison.OrdinalIgnoreCase))
                        continue;
                        
                    File.Copy(filePath, filePath.Replace(packToExport.ProjectPath, tempDir), true);
                }
                
                // Create a zip file
                if (File.Exists(exportPath))
                    File.Delete(exportPath);
                    
                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, exportPath);
            }
            finally
            {
                // Cleanup
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        
        public bool ValidateResourcePack(MinecraftPack packToValidate, out List<string> issues)
        {
            issues = new List<string>();
            
            // Check if pack.mcmeta exists
            if (!File.Exists(packToValidate.GetPackMetaPath()))
            {
                issues.Add("Missing pack.mcmeta file");
            }
            
            // Check for non-power-of-two textures
            var texturePath = packToValidate.GetTexturesPath();
            if (Directory.Exists(texturePath))
            {
                foreach (string pngFile in Directory.GetFiles(texturePath, "*.png", SearchOption.AllDirectories))
                {
                    try
                    {
                        using (var img = Image.FromFile(pngFile))
                        {
                            bool isPowerOfTwo = IsPowerOfTwo(img.Width) && IsPowerOfTwo(img.Height);
                            if (!isPowerOfTwo)
                            {
                                issues.Add($"Non-power-of-two texture: {GetRelativePath(packToValidate.ProjectPath, pngFile)}");
                            }
                        }
                    }
                    catch
                    {
                        issues.Add($"Invalid texture file: {GetRelativePath(packToValidate.ProjectPath, pngFile)}");
                    }
                }
            }
            
            return issues.Count == 0;
        }
        
        private bool IsPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        
        private string GetRelativePath(string basePath, string fullPath)
        {
            // Ensure both paths end with directory separator
            basePath = basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            
            // Convert paths to URIs for comparison
            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);
            
            // Get the relative path
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            
            // Convert URI to string path
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            
            // Replace forward slashes with backslashes
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            
            return relativePath;
        }
        
        private string GetProjectFilePath(string projectDirectory)
        {
            return Path.Combine(projectDirectory, Path.GetFileName(projectDirectory) + PROJECT_FILE_EXTENSION);
        }
        
        private void UpdateAutoSaveTimer(MinecraftPack pack)
        {
            _autoSaveTimer.Stop();
            
            if (pack.AutoSave)
            {
                _autoSaveTimer.Interval = pack.AutoSaveInterval * 60 * 1000; // Convert minutes to milliseconds
                _autoSaveTimer.Start();
            }
        }
        
        protected virtual void OnProjectLoaded(MinecraftPack pack)
        {
            ProjectLoaded?.Invoke(this, pack);
        }
        
        protected virtual void OnProjectSaved(MinecraftPack pack)
        {
            ProjectSaved?.Invoke(this, pack);
        }
        
        protected virtual void OnErrorOccurred(string message)
        {
            ErrorOccurred?.Invoke(this, message);
        }
    }
}
