using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tmp.Link.Upload
{
    [Serializable]
    public class SettingsModel
    {
        public string Token { get; set; } = string.Empty;
        public bool AutoStart { get; set; }
        public int Expires { get; set; }
        public bool AutoUpload { get; set; }

        public int ExpiresToUploadModel()
        {
            switch (Expires)
            {
                // Forever
                case 3: return 99;
            }

            return Expires;
        }
    }
}