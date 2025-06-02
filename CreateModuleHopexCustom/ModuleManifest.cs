using Mega.Has.Commons;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateModuleHopexCustom
{


    internal class ModuleManifestc
    {
        private void AddManifestToPackage(string packagePath)
        {
            string tmp = IoHelper.GetTempFolder();
            try
            {
                string tmpManifestFileName = Path.Combine(tmp, "has-manifest.json");
                //this.Manifest.PackagePatterns = null;
                //this.Manifest.AdditionalPatterns = null;
                //this.Manifest.Save(tmp);
                //if (!this._dryRun)
                //{
                //    using (FileStream writer = new FileStream(packagePath, FileMode.Open, FileAccess.ReadWrite))
                //    {
                //        using (ZipArchive zip = new ZipArchive(writer, ZipArchiveMode.Update, false))
                //        {
                //            zip.CreateEntryFromFile(tmpManifestFileName, "has-manifest.json");
                //        }
                //    }
                //}
            }
            finally
            {
                Directory.Delete(tmp, true);
            }
        }
    }
}
