using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Onyeyiri;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Etier.IconHelper;
using System.Runtime.InteropServices;

namespace Launchy
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string path, string filePath);

        #region Privates
        private HotKeyManager hkm;
        private static int count = 0;
        private String[] arrShortCuts;
        private static String strConfig = Application.StartupPath + "\\conf.ini";
        
        #endregion
        public Form1()
        {
            InitializeComponent();

            hkm = new HotKeyManager();
            hkm.Register(Keys.Space, HotKeyModifier.Shift, 100, new HotKeyEventHandler(Handler));

            arrShortCuts = new String[10];
            InitConfig();
            InitDisplay();
        }

        private void InitDisplay()
        {
            ContextMenuStrip menu = new System.Windows.Forms.ContextMenuStrip();
            smallIcon.ContextMenuStrip = menu;

            ToolStripMenuItem showItem = new ToolStripMenuItem();
            showItem.Click += new EventHandler(Button_Show);
            showItem.Text = "Show LightDesktop";
            menu.Items.Add(showItem);

            ToolStripMenuItem saveItem = new ToolStripMenuItem();
            saveItem.Click += new EventHandler(Button_Save);
            saveItem.Text = "Save       Ctrl+S";
            menu.Items.Add(saveItem);

            ToolStripSeparator spilt = new ToolStripSeparator();
            menu.Items.Add(spilt);

            ToolStripMenuItem closeItem = new ToolStripMenuItem();
            closeItem.Click += new EventHandler(Button_Close);
            closeItem.Text = "Exit";
            menu.Items.Add(closeItem);
        }

        void Button_Save(object sender, EventArgs e)
        {
            Save();
        }

        void Save()
        {
            StringBuilder temp = new StringBuilder(256);
            for (int i = 0; i < count; ++i)
            {
                WritePrivateProfileString("ShortCuts", "Item"+i, arrShortCuts[i], strConfig);
            }
        }

        void Button_Show(object sender, EventArgs e)
        {
            Show();
        }

        void Show()
        {
            this.Visible = true;
            this.Activate();
        }

        void Button_Close(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        public void DoClick(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                Show();
            }
            
        }

        public void Handler(int id)
        {
            this.Visible = !this.Visible;
            if (this.Visible)
            {
                this.Activate();
            }
        }
        
        private void Key_Down(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyValue == 'S' || e.KeyValue == 's')
                    Save();
            }
            else if (e.Alt)
            {
                if (e.KeyCode == System.Windows.Forms.Keys.F4)
                {
                    this.Close();
                    Application.Exit();
                }
            }
            else
            {
                String execPath = "";
                String arguments = "";
                int index = e.KeyValue - '1';
                if (index >= 0 && index < 10)
                {
                    execPath = arrShortCuts[index];
                    if (execPath != null && execPath.Length > 0)
                    {
                        Launchy(execPath, arguments);
                    }
                }
                this.Hide();
            }
        }

        private void Launchy( String execPath, String arguments="" )
        {
            try
            {
                Process exec = new Process();
                exec.StartInfo.FileName = execPath;
                exec.StartInfo.Arguments = arguments;
                exec.StartInfo.UseShellExecute = true;

                exec.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

       

        private void Form_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Reset the label text.
           
            String str = (String)e.Data.GetData(typeof(System.String));
            e.Effect = DragDropEffects.Link;
        }

        private void Form_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (count < 10)
            {
                Array data = ((IDataObject)e.Data).GetData("FileNameW") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        String filename = ((string[])data)[0];

                        int x = 75;
                        int y = 94;
                        this.arrShortCuts[count] = filename;
                        if (count * x + x > this.Size.Width)
                        {
                            this.Size = new System.Drawing.Size(this.Size.Width + x, this.Size.Height);
                            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                        }

                        this.Add_ShortCut(new System.Drawing.Point(count * x, 2), filename);

                        ++count;
                        return;
                    }
                }
            }
        }

        private void Form_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Reset the label text.
            Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
            if (data != null)
            {
                if ((data.Length == 1) && (data.GetValue(0) is String))
                {
                    String filename = ((string[])data)[0];
                 }
            }
        }

        private void Form_DragLeave(object sender, System.EventArgs e)
        {
            // Reset the label text.
        }

        private void Add_ShortCut(System.Drawing.Point p, String filename)
        {
           
            int iconx = 45;
            int icony = 45;

            Icon iconForFile = SystemIcons.WinLogo;
            try
            {
                iconForFile = IconReader.GetFileIcon(filename, IconReader.IconSize.Large, false);
            }
            catch (Exception e)
            {
                iconForFile = IconReader.GetFolderIcon(IconReader.IconSize.Large, IconReader.FolderType.Closed);
                
            }
            

            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();
            pictureBox.BackgroundImage = iconForFile.ToBitmap();
            pictureBox.Location = new System.Drawing.Point(p.X + 15, p.Y + 2);
            pictureBox.Size = new System.Drawing.Size(iconx,icony);
            pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            pictureBox.AllowDrop = true;
            pictureBox.Click += async (sender, e) =>
                {
                    Launchy(filename);
                    this.Hide();
                };
            pictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Controls.Add(pictureBox);

            System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
            label2.Text = System.IO.Path.GetFileName(filename);
            label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            label2.Location = new System.Drawing.Point(p.X, p.Y + 55);
            label2.Size = new System.Drawing.Size(75,94-55);
            this.Controls.Add(label2);

        }

        void Form_LostFocus(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void InitConfig()
        {
            int x = 75;
            int y = 94;
            StringBuilder temp = new StringBuilder(256);
            int tempcount = 0;
            for (int i = 0; ; ++i)
            {
                if (tempcount > 10)
                    break;
                GetPrivateProfileString("ShortCuts", "Item"+i, "", temp, 256, strConfig);
                if (string.IsNullOrEmpty(temp.ToString()))
                {
                    break;
                }
                else
                {
                    arrShortCuts[tempcount++] = temp.ToString();
                }
            }

            foreach (String filename in this.arrShortCuts)
            {
                if (filename != null)
                {
                    if (count * x + x > this.Size.Width)
                    {
                        this.Size = new System.Drawing.Size(this.Size.Width + x, this.Size.Height);
                        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                    }

                    this.Add_ShortCut(new System.Drawing.Point(count * x, 2), filename);

                    //Launchy(filename);
                    //this.Hide();
                    ++count;
                }
            }
        }
        /// <summary>
        /// Setup the background image, enable transparency, and move the window position
        /// to that saved in the registry if available
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                Stream imgStream = null;
                Bitmap bmp = null;
                Assembly a = Assembly.GetExecutingAssembly();

                // get a list of resource names from the manifest
                string[] resNames = a.GetManifestResourceNames();

                foreach (string s in resNames)
                {
                    if (s.EndsWith("bkgd2.bmp"))
                    {
                        // attach to stream to the resource in the manifest
                        imgStream = a.GetManifestResourceStream(s);
                        if (!(null == imgStream))
                        {
                            // create a new bitmap from this stream and 
                            // add it to the arraylist
                            bmp = Bitmap.FromStream(imgStream) as Bitmap;
                            //	imgStream.Close();
                            //	imgStream = null;
                        }
                    }
                }

                if (bmp != null)
                {
                    bmp.MakeTransparent(bmp.GetPixel(0, 0));
                    this.BackgroundImage = bmp;
                    //this.TransparencyKey = bmp.GetPixel(0,0);
                }
                if (imgStream != null) { imgStream.Close(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            // Set the window position
            //Location = new Point(options.posX, options.posY);
        }
    }
}
