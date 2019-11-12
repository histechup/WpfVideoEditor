using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WpfVideoEditor.Models
{
    public class ClipsCollection : ObservableCollection<Clip>
    {
        private static readonly string MarkingsFilenamePostfix = ".markings.json";
        private static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private static FileInfo GetMarkingsFilepathFor(FileInfo videofile) => new FileInfo(videofile.FullName + MarkingsFilenamePostfix);

        internal static ClipsCollection LoadFor(FileInfo videofile)
        {
            var markingsFile = GetMarkingsFilepathFor(videofile);
            if (!markingsFile.Exists)
            {
                return new ClipsCollection();
            }

            var list = new ClipsCollection();
            var json = File.ReadAllText(markingsFile.FullName, Encoding);
            var d = JsonDocument.Parse(json);
            var it = d.RootElement.EnumerateArray();
            while (it.MoveNext())
            {
                list.Add(new Clip { StartMs = it.Current.GetProperty("start").GetInt32(), EndMs = it.Current.GetProperty("end").GetInt32(), });
            }
            return list;

            //return JsonSerializer.Parse<Markings>(json);
        }

        internal void Save(FileInfo videofile)
        {
            var markingsFile = GetMarkingsFilepathFor(videofile);
            if (Count == 0)
            {
                if (markingsFile.Exists)
                {
                    markingsFile.Delete();
                }
                return;
            }

            var tmp = new FileInfo(markingsFile.FullName + ".new");
            using (var stream = tmp.OpenWrite())
            {
                JsonSerializer.SerializeAsync(stream, this).Wait();
            }
            if (markingsFile.Exists)
            {
                tmp.Replace(markingsFile.FullName, destinationBackupFileName: null);
            }
            else
            {
                tmp.MoveTo(markingsFile.FullName);
            }
        }
    }
}
