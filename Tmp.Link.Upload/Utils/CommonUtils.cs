using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tmp.Link.Upload.Utils
{
    public static class CommonUtils
    {
        public static string ByteStrToSizeStr(string byteStr)
        {
            if (long.TryParse(byteStr, out var bytes))
            {
                return ByteStrToSizeStr(bytes);
            }

            return "0";
        }


        public static string ByteStrToSizeStr(long bytes)
        {
            var kb = bytes / 1024.0;
            if (kb > 1024)
            {
                var mb = kb / 1024;
                if (mb > 1024)
                {
                    var gb = mb / 1024;
                    return $"{gb:F} gb";
                }

                return $"{mb:F} mb";
            }

            return $"{kb:F} kb";
        }
    }
}