using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Forms;

namespace STROOP.Tabs.GhostTab
{
    partial class GhostTab
    {
        static XElement configNode;
        static HashSet<string> fileWatcherPaths = new HashSet<string>();

        [Utilities.InitializeConfigParser]
        static void InitConfigParser()
        {
            Utilities.XmlConfigParser.AddConfigParser("GhostFileWatchers", ParseGhostConfig);
        }

        static void ParseGhostConfig(XElement node)
        {
            configNode = node;
            foreach (var watcherNode in node.Elements().Where(_ => _.Name == "FileWatcher"))
            {
                var attr = watcherNode.Attribute(XName.Get("path"));
                if (attr != null)
                    fileWatcherPaths.Add(attr.Value);
            }
        }

        static void SaveConfig()
        {
            if (configNode == null)
                Program.config.Root.Add(configNode = new XElement(XName.Get("GhostFileWatchers")));
            configNode.RemoveAll();
            foreach (var path in fileWatcherPaths)
            {
                var n = new XElement(XName.Get("FileWatcher"));
                n.SetAttributeValue(XName.Get("path"), path);
                configNode.Add(n);
            }

            configNode.Document.Save(Program.CONFIG_FILE_NAME);
        }

        Dictionary<string, FileSystemWatcher> activeFileWatchers = new Dictionary<string, FileSystemWatcher>();

        void UpdateFileWatchers()
        {
            foreach (var entry in activeFileWatchers)
                entry.Value.Dispose();
            activeFileWatchers.Clear();
            foreach (var path in fileWatcherPaths)
                AddFileWatcher(path);
        }

        void AddFileWatcher(string path)
        {
            DateTime oldFileChangedDate = DateTime.Now;
            string file = Path.GetFullPath(path);
            int i = 0;
            try
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    var folder = Path.GetDirectoryName(Path.GetFullPath(path));
                    FileSystemWatcher watcher = new FileSystemWatcher(folder);
                    file = folder + "\\tmp.ghost";
                    activeFileWatchers[path] = watcher;
                    var ghostName = "auto-rec";
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.EnableRaisingEvents = true;
                    watcher.Changed += (a, aa) =>
                    {
                        if (aa.FullPath == file)
                        {
                            var newFileChangedDate = File.GetLastWriteTime(file);
                            if (newFileChangedDate - oldFileChangedDate > new TimeSpan(0, 0, 1))
                            {
                                BinaryReader rd = null;
                                try
                                {
                                    rd = new BinaryReader(new FileStream(file, FileMode.Open));
                                    var newGhost = Ghost.FromFile(rd);
                                    groupBoxGhosts.Invoke((Action)(() => AddGhost($"{ghostName} {i++}", newGhost)));
                                    oldFileChangedDate = newFileChangedDate;
                                }
                                catch
                                {
                                }
                                finally
                                {
                                    rd?.Close();
                                }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to watch Directory:{path}\n\n{ex.ToString()}");
            }
        }

        private void buttonWatchGhostFile_Click(object sender, EventArgs e)
        {
            var frm = new Form();
            frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            frm.Width = 750;
            var lst = new ListBox();
            lst.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            lst.Size = frm.ClientRectangle.Size;
            lst.Location = new System.Drawing.Point(frm.ClientRectangle.Left + 5, frm.ClientRectangle.Top + 5);
            lst.Height -= 50;
            lst.Width -= 10;
            foreach (var path in fileWatcherPaths)
                lst.Items.Add(path);
            frm.Controls.Add(lst);

            var btnAdd = new Button();
            btnAdd.Text = "Add...";
            btnAdd.Location = new System.Drawing.Point(lst.Left, lst.Bottom + 5);
            btnAdd.Width = lst.Width / 2 - 2;
            btnAdd.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            frm.Controls.Add(btnAdd);

            var btnRemove = new Button();
            btnRemove.Text = "Remove";
            btnRemove.Width = lst.Width / 2 - 2;
            btnRemove.Location = new System.Drawing.Point(btnAdd.Right + 5, lst.Bottom + 5);
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            frm.Controls.Add(btnRemove);

            btnAdd.Click += (_, __) =>
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "recordghost file (*.lua)|*.lua";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileWatcherPaths.Add(dlg.FileName);
                    lst.Items.Clear();
                    foreach (var path in fileWatcherPaths)
                        lst.Items.Add(path);
                }
            };

            btnRemove.Click += (_, __) =>
            {
                if (lst.SelectedItem != null)
                    fileWatcherPaths.Remove((string)lst.SelectedItem);
                lst.Items.Clear();
                foreach (var path in fileWatcherPaths)
                    lst.Items.Add(path);
            };

            frm.ShowDialog();
            UpdateFileWatchers();
            SaveConfig();
        }
    }
}
