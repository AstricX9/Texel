using System;
using System.IO;
using System.Windows.Forms;
using Texel.Models;

namespace Texel.Dialogs
{
    public partial class TexturePathDialog : Form
    {
        private readonly MinecraftPack _pack;
        
        public string SelectedPath { get; private set; }
        
        public TexturePathDialog(MinecraftPack pack)
        {
            InitializeComponent();
            _pack = pack;
            
            LoadCategories();
        }
        
        private void LoadCategories()
        {
            // Add standard texture categories
            cboCategory.Items.Add("block");
            cboCategory.Items.Add("item");
            cboCategory.Items.Add("entity");
            cboCategory.Items.Add("gui");
            cboCategory.Items.Add("font");
            cboCategory.Items.Add("environment");
            cboCategory.Items.Add("models");
            cboCategory.Items.Add("particle");
            
            // Add custom categories from the pack
            string texturesPath = _pack.GetTexturesPath();
            if (Directory.Exists(texturesPath))
            {
                foreach (string dir in Directory.GetDirectories(texturesPath))
                {
                    string categoryName = Path.GetFileName(dir);
                    if (!cboCategory.Items.Contains(categoryName))
                    {
                        cboCategory.Items.Add(categoryName);
                    }
                }
            }
            
            // Set default
            if (cboCategory.Items.Count > 0)
            {
                cboCategory.SelectedIndex = 0;
            }
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cboCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.", "Missing Information", 
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            SelectedPath = cboCategory.SelectedItem.ToString();
            
            // Add subdirectory if specified
            if (!string.IsNullOrWhiteSpace(txtSubdirectory.Text))
            {
                SelectedPath = Path.Combine(SelectedPath, txtSubdirectory.Text);
            }
            
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void btnNewCategory_Click(object sender, EventArgs e)
        {
            using (var dialog = new TextInputDialog("New Category", "Enter the new category name:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    string newCategory = dialog.InputText.Trim();
                    
                    // Validate - no spaces or special chars
                    if (newCategory.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        MessageBox.Show("Category name contains invalid characters.", "Invalid Name", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Add to list if not already there
                    if (!cboCategory.Items.Contains(newCategory))
                    {
                        cboCategory.Items.Add(newCategory);
                    }
                    
                    cboCategory.SelectedItem = newCategory;
                }
            }
        }
    }
}
