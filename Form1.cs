using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace USBCloner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //get drive list
            LoadDriveList();
            
            
            textBox1.Text = USBCloner.Properties.Settings.Default.LastUsedDir;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //browse for a folder
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                USBCloner.Properties.Settings.Default.LastUsedDir = textBox1.Text;
                USBCloner.Properties.Settings.Default.Save();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //pictureBox1.Visible = true;
            //start clone to all drives
            //try to create a thread for each copy process            
            foreach (string s in checkedListBox1.CheckedItems) 
            {
                Process proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "xcopy.exe");
                proc.StartInfo.Arguments = "\""+textBox1.Text +"\" \""+ s +"\\\" /E /I";
                proc.Start();
            }
            if (DialogResult.OK == MessageBox.Show("Complete!"))
            {
                Application.Exit();
            }

        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        private void LoadDriveList()
        {
            try
            {
                ManagementObjectSearcher letterSearcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_LogicalDisk");

                foreach (ManagementObject letterQueryObj in letterSearcher.Get())
                {
                    if (letterQueryObj["Name"].ToString() != "C:")
                    {
                        checkedListBox1.Items.Add(letterQueryObj["Name"].ToString(),true);
                    }
                }

            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
            
        }

        private void but_FormatDrives_Click(object sender, EventArgs e)
        {
            foreach (string s in checkedListBox1.CheckedItems) 
            {
                ClearDrive(s); 
            }
        }
        private void ClearDrive(string s) 
        {
            string[] files = Directory.GetFiles(s+"\\");
            foreach (string file in files) 
            {
                File.Delete(file);
            }
            string[] dirs = Directory.GetDirectories(s + "\\");
            foreach (string dir in dirs)
            {
                Directory.Delete(dir,true);
            }
        }
    }
}
