using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Texel.Models;
using Texel.Services;

namespace Texel.Dialogs
{
    public partial class NewPackDialog : Form
    {
        private readonly MinecraftAssetsService _assetsService;
        private readonly ProjectService _projectService;
        private string _iconPath;

        public MinecraftPack CreatedPack { get; private set; }

        public NewPackDialog(MinecraftAssetsService assetsService, ProjectService projectService)
        {
            InitializeComponent();
            _assetsService = assetsService;
            _projectService = projectService;

            // Populate resolution dropdown
            cboResolution.Items.AddRange(new object[] { 16, 32, 64, 128, 256, 512 });
            cboResolution.SelectedIndex = 0;

            // Set default location
            txtLocation.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            LoadVersionsAsync();
        }

        private async void LoadVersionsAsync()
        {
            try
            {
                cboVersion.Enabled = false;
                cboVersion.Items.Clear();
                cboVersion.Items.Add("Loading versions...");
                cboVersion.SelectedIndex = 0;

                var versions = await _assetsService.GetAvailableVersionsAsync();

                cboVersion.Items.Clear();
                foreach (var version in versions)
                {
                    cboVersion.Items.Add(version);
                }

                if (cboVersion.Items.Count > 0)
                {
                    cboVersion.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Minecraft versions: {ex.Message}\nUsing default versions instead.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                cboVersion.Items.Clear();
                cboVersion.Items.AddRange(new object[] { "1.20.1", "1.19.4", "1.18.2", "1.17.1", "1.16.5" });
                cboVersion.SelectedIndex = 0;
            }
            finally
            {
                cboVersion.Enabled = true;
            }
        }

        private void btnBrowseLocation_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select location to save your resource pack";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtLocation.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnSelectIcon_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                dialog.Title = "Select Pack Icon";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var img = Image.FromFile(dialog.FileName))
                        {
                            // Dispose old image if any
                            picIcon.Image?.Dispose();

                            picIcon.Image = new Bitmap(img);
                            _iconPath = dialog.FileName;

                            if (img.Width != img.Height)
                            {
                                MessageBox.Show("Warning: Pack icons should be square. The image will be resized.",
                                                "Non-square Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a pack name.", "Missing Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please select a location to save your pack.", "Missing Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string packFolderPath = Path.Combine(txtLocation.Text, SanitizeFileName(txtName.Text));

            if (Directory.Exists(packFolderPath))
            {
                var result = MessageBox.Show($"A folder named '{txtName.Text}' already exists at this location. " +
                                             "Do you want to use it anyway? Existing files might be overwritten.",
                                             "Folder Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            try
            {
                CreatedPack = _projectService.CreateNewProject(
                    txtName.Text,
                    txtDescription.Text,
                    cboVersion.SelectedItem.ToString(),
                    int.Parse(cboResolution.SelectedItem.ToString()),
                    packFolderPath,
                    _iconPath
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating resource pack: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = name;
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }
            return sanitized;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            picIcon.Image?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
