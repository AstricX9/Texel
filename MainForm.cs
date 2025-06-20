using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Texel.Classes.Input;
using Texel.Dialogs;
using Texel.Models;
using Texel.Services;

namespace Texel
{
    public partial class MainForm : Form
    {
        private InputHandler _inputHandler;
        private ProjectService _projectService;
        private MinecraftAssetsService _assetsService;
        private List<ToolStripMenuItem> _recentProjectMenuItems = new List<ToolStripMenuItem>();
        
        public MainForm()
        {
            InitializeComponent();
            
            // Initialize services
            _projectService = new ProjectService();
            _assetsService = new MinecraftAssetsService();
            
            // Initialize input handler
            _inputHandler = new InputHandler(pixelCanvasControl1);
            
            // Set up events
            _projectService.ProjectLoaded += OnProjectLoaded;
            _projectService.ProjectSaved += OnProjectSaved;
            _projectService.ErrorOccurred += OnErrorOccurred;
            this.toggleGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleGridToolStripMenuItem.Name = "toggleGridToolStripMenuItem";
            this.toggleGridToolStripMenuItem.Text = "Toggle Grid";
            this.toggleGridToolStripMenuItem.CheckOnClick = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            toolWindowControl1.ToolChanged += tool =>
            {
                pixelCanvasControl1.CurrentTool = tool;
            };
            
            // Update status bar
            UpdateStatusBar();
            
            // Populate recent projects menu
            UpdateRecentProjectsMenu();
        }
        
        private void OnProjectLoaded(object sender, MinecraftPack pack)
        {
            // Update UI based on loaded project
            Text = $"Texel - {pack.Name}";
            
            // If opening an empty project, set grid size to match resolution
            if (pixelCanvasControl1.GridWidth != pack.Resolution || 
                pixelCanvasControl1.GridHeight != pack.Resolution)
            {
                pixelCanvasControl1.SetGridSize(pack.Resolution, pack.Resolution);
            }
            
            // Refresh UI
            UpdateStatusBar();
            UpdateRecentProjectsMenu();
        }
        
        private void OnProjectSaved(object sender, MinecraftPack pack)
        {
            // Update UI
            Text = $"Texel - {pack.Name}";
            
            // Refresh recent projects
            UpdateRecentProjectsMenu();
            
            // Show success message
            statusLabel.Text = $"Project saved to {pack.ProjectPath}";
        }
        
        private void OnErrorOccurred(object sender, string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private void UpdateStatusBar()
        {
            coordsLabel.Text = $"Grid: {pixelCanvasControl1.GridWidth}x{pixelCanvasControl1.GridHeight}";
            zoomLabel.Text = $"Zoom: {pixelCanvasControl1.CellSize}x";
            toolLabel.Text = $"Tool: {pixelCanvasControl1.CurrentTool}";
        }
        
        private void UpdateRecentProjectsMenu()
        {
            // Clear existing items
            foreach (var item in _recentProjectMenuItems)
            {
                recentProjectsToolStripMenuItem.DropDownItems.Remove(item);
            }
            _recentProjectMenuItems.Clear();
            
            // Hide menu if no recent projects
            var recentProjects = _projectService.GetRecentProjects();
            recentProjectsToolStripMenuItem.Visible = recentProjects.Count > 0;
            
            if (recentProjects.Count == 0)
                return;
                
            // Add new items
            foreach (var projectPath in recentProjects)
            {
                var item = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(projectPath));
                item.Tag = projectPath;
                item.Click += RecentProject_Click;
                
                recentProjectsToolStripMenuItem.DropDownItems.Add(item);
                _recentProjectMenuItems.Add(item);
            }
        }
        
        private void RecentProject_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag is string projectPath)
            {
                try
                {
                    _projectService.LoadProject(projectPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading project: {ex.Message}", "Error", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new NewPackDialog(_assetsService, _projectService))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Project created and loaded by the service
                }
            }
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Texture Pack Project|*.mcpackproj|Minecraft Resource Pack|pack.mcmeta";
                dialog.Title = "Open Texture Pack";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string path = dialog.FileName;
                        
                        // If they selected pack.mcmeta, use its directory
                        if (Path.GetFileName(path).Equals("pack.mcmeta", StringComparison.OrdinalIgnoreCase))
                        {
                            path = Path.GetDirectoryName(path);
                        }
                        
                        _projectService.LoadProject(path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening project: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                saveProjectAsToolStripMenuItem_Click(sender, e);
                return;
            }
            
            try
            {
                _projectService.SaveProject(_projectService.CurrentPack);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", "Error", 
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                newProjectToolStripMenuItem_Click(sender, e);
                return;
            }
            
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select location to save your resource pack";
                dialog.ShowNewFolderButton = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _projectService.SaveProjectAs(_projectService.CurrentPack, dialog.SelectedPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving project: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void exportZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Zip File|*.zip";
                dialog.Title = "Export Resource Pack";
                dialog.FileName = $"{_projectService.CurrentPack.Name}.zip";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _projectService.ExportResourcePack(_projectService.CurrentPack, dialog.FileName);
                        MessageBox.Show($"Resource pack exported to {dialog.FileName}", "Export Complete", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting resource pack: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void validatePackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            List<string> issues;
            bool isValid = _projectService.ValidateResourcePack(_projectService.CurrentPack, out issues);
            
            if (isValid)
            {
                MessageBox.Show("Resource pack is valid!", "Validation", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Resource pack has issues:\n\n{string.Join("\n", issues)}", 
                               "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void openInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            System.Diagnostics.Process.Start("explorer.exe", _projectService.CurrentPack.ProjectPath);
        }

        private void downloadAssetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            string version = _projectService.CurrentPack.Version;
            
            var progress = new Progress<int>(percent => 
            {
                toolStripProgressBar.Value = percent;
                statusLabel.Text = $"Downloading assets: {percent}%";
            });
            
            toolStripProgressBar.Visible = true;
            
            Task.Run(async () => 
            {
                try
                {
                    await _assetsService.DownloadTexturesAsync(version, progress);
                    
                    Invoke(new Action(() => 
                    {
                        toolStripProgressBar.Visible = false;
                        statusLabel.Text = "Download complete!";
                        MessageBox.Show($"Minecraft textures for version {version} downloaded successfully.",
                                      "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => 
                    {
                        toolStripProgressBar.Visible = false;
                        statusLabel.Text = "Download failed.";
                        MessageBox.Show($"Error downloading assets: {ex.Message}", "Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        private void importTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_projectService.CurrentPack == null)
            {
                MessageBox.Show("Please open or create a project first.", "No Project", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "PNG Image|*.png";
                dialog.Title = "Import Texture";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var img = Image.FromFile(dialog.FileName))
                        {
                            // Create a dialog to select texture category/path
                            using (var pathDialog = new TexturePathDialog(_projectService.CurrentPack))
                            {
                                if (pathDialog.ShowDialog() == DialogResult.OK)
                                {
                                    string targetPath = Path.Combine(
                                        _projectService.CurrentPack.GetTexturesPath(),
                                        pathDialog.SelectedPath,
                                        Path.GetFileName(dialog.FileName));
                                        
                                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                                    File.Copy(dialog.FileName, targetPath, true);
                                    
                                    MessageBox.Show($"Texture imported to {targetPath}", "Import Complete", 
                                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing texture: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void exportCurrentTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG Image|*.png";
                dialog.Title = "Export Current Texture";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Create a bitmap from the pixel data
                        int width = pixelCanvasControl1.GridWidth;
                        int height = pixelCanvasControl1.GridHeight;
                        
                        using (var bmp = new Bitmap(width, height))
                        {
                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    bmp.SetPixel(x, y, pixelCanvasControl1.Pixels[x, y]);
                                }
                            }
                            
                            bmp.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        
                        MessageBox.Show($"Texture exported to {dialog.FileName}", "Export Complete", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting texture: {ex.Message}", "Error", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pixelCanvasControl1.CanUndo)
            {
                pixelCanvasControl1.Undo();
                UpdateUndoRedoState();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pixelCanvasControl1.CanRedo)
            {
                pixelCanvasControl1.Redo();
                UpdateUndoRedoState();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pixelCanvasControl1.CutSelection();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pixelCanvasControl1.CopySelection();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pixelCanvasControl1.PasteFromClipboard();
        }

        private void UpdateUndoRedoState()
        {
            undoToolStripMenuItem.Enabled = pixelCanvasControl1.CanUndo;
            redoToolStripMenuItem.Enabled = pixelCanvasControl1.CanRedo;
        }

        private void toggleGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pixelCanvasControl1.ShowGrid = !pixelCanvasControl1.ShowGrid;
            toggleGridToolStripMenuItem.Checked = pixelCanvasControl1.ShowGrid;
            pixelCanvasControl1.Invalidate();
        }

        private void resizeCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new ResizeCanvasDialog(pixelCanvasControl1.GridWidth, pixelCanvasControl1.GridHeight))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pixelCanvasControl1.SetGridSize(dialog.NewWidth, dialog.NewHeight);
                    UpdateStatusBar();
                }
            }
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = Classes.AppSettings.Load();
            
            using (var dialog = new SettingsDialog(_projectService, _assetsService, settings))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the updated settings
                    Classes.AppSettings.Save(dialog.UpdatedSettings);
                    
                    // Apply any immediate UI changes
                    pixelCanvasControl1.ShowGrid = dialog.UpdatedSettings.ShowGrid;
                    toggleGridToolStripMenuItem.Checked = dialog.UpdatedSettings.ShowGrid;
                    pixelCanvasControl1.Invalidate();
                    
                    // Update auto-save settings if there's a current project
                    if (_projectService.CurrentPack != null)
                    {
                        _projectService.CurrentPack.AutoSave = dialog.UpdatedSettings.AutoSaveEnabled;
                        _projectService.CurrentPack.AutoSaveInterval = dialog.UpdatedSettings.AutoSaveIntervalMinutes;
                    }
                }
            }
        }
    }
}
