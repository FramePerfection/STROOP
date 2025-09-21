using System.Diagnostics;
using System.IO.Compression;

namespace STROOP.Core.GameMemoryAccess
{
    public class StFileIO : BaseProcessIO
    {
        public override bool IsSuspended => false;

        public override event EventHandler OnClose;

        private string _path;
        public string Path => _path;

        protected override UIntPtr BaseOffset => new UIntPtr(0x1B0);
        protected override EndiannessType Endianness => EndiannessType.Little;

        public override string Name => System.IO.Path.GetFileName(_path);
        public override Process Process => null;

        private byte[] _data;

        public StFileIO(string path) : base()
        {
            _path = path;
            LoadMemory();
        }

        private void LoadMemory()
        {
            using var fileStream = new FileStream(_path, FileMode.Open);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using MemoryStream unzip = new MemoryStream();
            gzipStream.CopyTo(unzip);
            _data = unzip.ToArray();
        }

        public void SaveMemory(string path)
        {
            using var fileStream = new FileStream(path, FileMode.Create);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
            gzipStream.Write(_data, 0, _data.Length);
        }

        public override bool Resume()
        {
            return true;
        }

        public override bool Suspend()
        {
            return true;
        }

        protected override bool WriteFunc(UIntPtr address, byte[] buffer)
        {
            if ((uint)address + buffer.Length > _data.Length)
                return false;

            Array.Copy(buffer, 0, _data, (uint)address, buffer.Length);
            return true;
        }

        protected override bool ReadFunc(UIntPtr address, byte[] buffer)
        {
            if ((uint)address + buffer.Length > _data.Length)
                return false;

            Array.Copy(_data, (uint)address, buffer, 0, buffer.Length);
            return true;
        }

        public override byte[] ReadAllMemory()
        {
            byte[] output = new byte[_data.Length];
            Array.Copy(_data, output, _data.Length);
            return output;
        }
    }
}
