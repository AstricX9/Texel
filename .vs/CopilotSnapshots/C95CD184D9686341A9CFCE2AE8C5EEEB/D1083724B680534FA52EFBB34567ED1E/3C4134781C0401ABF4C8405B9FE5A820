using System;
using System.IO;
using System.Windows.Forms;
using Texel.Services;
using System.Drawing;

namespace Texel.Dialogs
{
    public partial class TextureBrowserForm : Form
    {
        private readonly MinecraftAssetsService _assetsService;
        private readonly PixelCanvasControl _canvasControl;
        private bool _isLoading = false;

        public string SelectedTexturePath { get; private set; }
        public string SelectedVersion { get; private set; }

        // Event that fires when a texture is opened
        public event EventHandler<string> TextureOpened;

        public TextureBrowserForm(MinecraftAssetsService assetsService, PixelCanvasControl canvasControl)
        {
            InitializeComponent();
            _assetsService = assetsService;
            _canvasControl = canvasControl;
            
            // Set form properties
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = false;
            
            // Handle form closing - hide instead of close
            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
            
            LoadVersionsAsync();
        }

        public void RefreshBrowser(string preferredVersion = null)
        {
            if (!string.IsNullOrEmpty(preferredVersion) && cboVersion.Items.Contains(preferredVersion))
            {
                cboVersion.SelectedItem = preferredVersion;
            }
            else if (cboVersion.Items.Count > 0 && cboVersion.SelectedIndex == -1)
            {
                cboVersion.SelectedIndex = 0;
            }
            else if (cboVersion.SelectedIndex >= 0)
            {
                // Refresh current selection
                LoadTexturesTree(cboVersion.SelectedItem.ToString());
            }
        }

        private async void LoadVersionsAsync()
        {
            _isLoading = true;
            try
            {
                cboVersion.Items.Clear();
                cboVersion.Items.Add("Loading...");
                cboVersion.SelectedIndex = 0;
                cboVersion.Enabled = false;
                
                var versions = await _assetsService.GetAvailableVersionsAsync();
                
                cboVersion.Items.Clear();
                foreach (var v in versions)
                {
                    cboVersion.Items.Add(v);
                }
                
                if (cboVersion.Items.Count > 0)
                {
                    cboVersion.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Minecraft versions: {ex.Message}", 
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cboVersion.Enabled = true;
                _isLoading = false;
            }
        }

        private void cboVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || cboVersion.SelectedItem == null || cboVersion.SelectedItem.ToString() == "Loading...")
                return;
                
            SelectedVersion = cboVersion.SelectedItem.ToString();
            LoadTexturesTree(SelectedVersion);
        }

        private async void LoadTexturesTree(string version)
        {
            treeTextures.Nodes.Clear();
            pictureBox.Image = null;
            this.Cursor = Cursors.WaitCursor;
            
            try
            {
                // Show loading message
                var loadingNode = new TreeNode("Loading textures...");
                treeTextures.Nodes.Add(loadingNode);
                
                // Ensure textures are downloaded/cached
                var progress = new Progress<int>(percent => {
                    loadingNode.Text = $"Downloading textures: {percent}%";
                    treeTextures.Refresh();
                });
                
                await _assetsService.DownloadTexturesAsync(version, progress);
                treeTextures.Nodes.Clear();
                
                string rootPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Texel Editor", "Cache", version);

                if (Directory.Exists(rootPath))
                {
                    var rootNode = new TreeNode(version) { Tag = rootPath };
                    AddDirectoryNodes(rootNode, rootPath);
                    treeTextures.Nodes.Add(rootNode);
                    rootNode.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading textures: {ex.Message}", 
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void AddDirectoryNodes(TreeNode parent, string path)
        {
            try
            {
                // Add directories first
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var dirName = Path.GetFileName(dir);
                    var dirNode = new TreeNode(dirName) { Tag = dir };
                    AddDirectoryNodes(dirNode, dir);
                    
                    // Only add non-empty directories
                    if (dirNode.Nodes.Count > 0)
                        parent.Nodes.Add(dirNode);
                }
                
                // Then add files
                foreach (var file in Directory.GetFiles(path, "*.png"))
                {
                    var fileName = Path.GetFileName(file);
                    var fileNode = new TreeNode(fileName) { 
                        Tag = file,
                        ImageIndex = 1  // Set image index for file icon if you add an ImageList
                    };
                    parent.Nodes.Add(fileNode);
                }
            }
            catch (Exception ex)
            {
                parent.Nodes.Add(new TreeNode($"Error: {ex.Message}"));
            }
        }

        private void treeTextures_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = e.Node.Tag as string;
            SelectedTexturePath = null;
            
            if (File.Exists(path) && Path.GetExtension(path).ToLowerInvariant() == ".png")
            {
                try
                {
                    // Dispose previous image
                    if (pictureBox.Image != null)
                    {
                        var oldImage = pictureBox.Image;
                        pictureBox.Image = null;
                        oldImage.Dispose();
                    }
                    
                    // Load new image
                    pictureBox.Image = Image.FromFile(path);
                    SelectedTexturePath = path;
                    btnOpen.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading texture: {ex.Message}", 
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnOpen.Enabled = false;
                }
            }
            else
            {
                // Clear the preview for directories
                if (pictureBox.Image != null)
                {
                    var oldImage = pictureBox.Image;
                    pictureBox.Image = null;
                    oldImage.Dispose();
                }
                btnOpen.Enabled = false;
            }
        }

        // Double-click to open the texture
        private void treeTextures_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedTexturePath))
            {
                OpenSelectedTexture();
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenSelectedTexture();
        }

        private void OpenSelectedTexture()
        {
            if (string.IsNullOrEmpty(SelectedTexturePath))
                return;
                
            try
            {
                TextureOpened?.Invoke(this, SelectedTexturePath);
                
                // Load the image into the canvas
                if (_canvasControl != null)
                {
                    using (var img = Image.FromFile(SelectedTexturePath))
                    {
                        _canvasControl.SetGridSize(img.Width, img.Height);
                        
                        // Copy pixels from the image to the canvas
                        var bitmap = new Bitmap(img);
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                _canvasControl.Pixels[x, y] = bitmap.GetPixel(x, y);
                            }
                        }
                        
                        _canvasControl.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening texture: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
