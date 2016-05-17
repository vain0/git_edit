using System;
using System.IO;

namespace GitEdit.View
{
    public static class FileInfoExtensions
    {
        public static void DeleteIfExists(this FileInfo self)
        {
            if (self.Exists)
            {
                self.Delete();
            }
        }

        public static void OverwriteSafely(this FileInfo self, Action<FileInfo> f)
        {
            var tempFile = new FileInfo(Path.GetTempFileName());
            try
            {
                f(tempFile);
                self.DeleteIfExists();
                tempFile.MoveTo(self.FullName);
            }
            catch (Exception e)
            {
                tempFile.DeleteIfExists();
                throw e;
            }
        }
    }
}
