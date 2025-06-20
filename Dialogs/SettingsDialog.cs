using System;
using System.Windows.Forms;
using Texel.Classes;
using Texel.Services;

namespace Texel.Dialogs
{
    public partial class SettingsDialog : Form
    {
        private readonly ProjectService _projectService;
        private readonly MinecraftAssetsService _assetsService;
        private readonly AppSettings _settings;
        
        public AppSettings UpdatedSettings { get; private set; }
        
        public SettingsDialog(ProjectService projectService, MinecraftAssetsService assetsService, AppSettings settings)
        {
            InitializeComponent();
            _projectService = projectService;
            _assetsService = assetsService;
            _settings = settings;
            UpdatedSettings = new AppSettings();
            
            // Initialize settings fields
            LoadSettings();
        }
        
        private async void LoadSettings()
        {
            // Load default Minecraft version
            cboDefaultVersion.Enabled = false;
            cboDefaultVersion.Items.Add("Loading versions...");
            cboDefaultVersion.SelectedIndex = 0;
            
            try
            {
                var versions = await _assetsService.GetAvailableVersionsAsync();
                
                cboDefaultVersion.Items.Clear();
                foreach (var version in versions)
                {
                    cboDefaultVersion.Items.Add(version);
                }
                
                // Set current value
                if (!string.IsNullOrEmpty(_settings.DefaultMinecraftVersion) && 
                    cboDefaultVersion.Items.Contains(_settings.DefaultMinecraftVersion))
                {
                    cboDefaultVersion.SelectedItem = _settings.DefaultMinecraftVersion;
                }
                else if (cboDefaultVersion.Items.Count > 0)
                {
                    cboDefaultVersion.SelectedIndex = 0;
                }
            }
            catch
            {
                cboDefaultVersion.Items.Clear();
                cboDefaultVersion.Items.AddRange(new object[] { "1.20.1", "1.19.4", "1.18.2", "1.17.1", "1.16.5" });
                cboDefaultVersion.SelectedIndex = 0;
            }
            finally
            {
                cboDefaultVersion.Enabled = true;
            }
            
            // Load default resolution
            cboDefaultResolution.Items.AddRange(new object[] { 16, 32, 64, 128, 256, 512 });
            cboDefaultResolution.SelectedItem = _settings.DefaultResolution;
            if (cboDefaultResolution.SelectedIndex == -1 && cboDefaultResolution.Items.Count > 0)
            {
                cboDefaultResolution.SelectedIndex = 0;
            }
            
            // Auto save settings
            chkAutoSave.Checked = _settings.AutoSaveEnabled;
            numAutoSaveInterval.Value = _settings.AutoSaveIntervalMinutes;
            numAutoSaveInterval.Enabled = chkAutoSave.Checked;
            
            // UI settings
            chkShowGrid.Checked = _settings.ShowGrid;
            chkDarkTheme.Checked = _settings.UseDarkTheme;
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Save updated settings
            UpdatedSettings.DefaultMinecraftVersion = cboDefaultVersion.SelectedItem?.ToString();
            UpdatedSettings.DefaultResolution = (int)cboDefaultResolution.SelectedItem;
            UpdatedSettings.AutoSaveEnabled = chkAutoSave.Checked;
            UpdatedSettings.AutoSaveIntervalMinutes = (int)numAutoSaveInterval.Value;
            UpdatedSettings.ShowGrid = chkShowGrid.Checked;
            UpdatedSettings.UseDarkTheme = chkDarkTheme.Checked;
            
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            numAutoSaveInterval.Enabled = chkAutoSave.Checked;
        }
    }
}
