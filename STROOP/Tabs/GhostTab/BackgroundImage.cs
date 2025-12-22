using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace STROOP.Tabs.MapTab
{
    public class BackgroundImage : IComparable
    {
        static class ImageQueryManager
        {
            public static object FileMoveLock = new object();

            static volatile Task currentlyDownloading;

            public static void Download(string sourcePath, Action<Lazy<Image>> assignment)
            {
                if (currentlyDownloading == null)
                {
                    (currentlyDownloading = new Task(() =>
                    {
                        System.Threading.Thread.Sleep(2000); // We don't want to spam github with image fetches
                        var path = sourcePath.Replace('\\', '/').Replace(" ", "%20");
                        var source = $"https://raw.githubusercontent.com/SM64-TAS-ABC/STROOP/dev/STROOP/{path}";
                        try
                        {
                            using (WebClient client = new WebClient())
                                client.DownloadFile(new Uri(source), sourcePath + "_tmp");
                            lock (FileMoveLock)
                            {
                                File.Move(sourcePath + "_tmp", sourcePath);
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to download \"{source}\"");
                            assignment(MapTab.MapAssociations.BrokenBackgroundImage);
                            return;
                        }

                        assignment(new Lazy<Image>(() => Image.FromFile(sourcePath)));
                        currentlyDownloading = null;
                    })).Start();
                }
            }
        }


        public readonly string Name;
        readonly string sourcePath;
        Lazy<Image> loadedImage;

        public Lazy<Image> GetImage()
        {
            if (loadedImage != null)
                return loadedImage;

            lock (ImageQueryManager.FileMoveLock)
            {
                if (File.Exists(sourcePath))
                    return loadedImage = new Lazy<Image>(() => Image.FromFile(sourcePath));
                else
                    ImageQueryManager.Download(sourcePath, _ => loadedImage = _);
            }

            return MapTab.MapAssociations.DownloadingBackgroundImage;
        }

        public BackgroundImage(string name, string sourcePath)
        {
            this.Name = name;
            this.sourcePath = sourcePath;
        }

        public static bool operator ==(BackgroundImage a, BackgroundImage b)
        {
            if ((object)a == null ^ (object)b == null)
                return false;
            if (a == null)
                return true;
            return (a.Name == b.Name && a.sourcePath == b.sourcePath);
        }

        public static bool operator !=(BackgroundImage a, BackgroundImage b)
        {
            return !(a == b);
        }

        public override bool Equals(object other)
        {
            if (!(other is BackgroundImage))
                return false;

            return (this == (BackgroundImage)other);
        }

        public override int GetHashCode() => Name.GetHashCode() * 127;

        public override string ToString() => Name;

        public int CompareTo(object obj)
        {
            if (!(obj is BackgroundImage)) return -1;
            BackgroundImage other = (BackgroundImage)obj;
            return Name.CompareTo(other.Name);
        }
    }
}
