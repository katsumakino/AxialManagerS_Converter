using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxialManagerS_Converter {
  public class ConverterGlobal {

    // AXM1のフォルダパス
    public static readonly string Axm1DBFolderPath = "C:\\TomeyApp\\AxialManager\\AxialManagerDB\\_file";
    public static readonly string Axm1CommentFolderPath = "C:\\TomeyApp\\AxialManager\\AxialManagerDB\\_comment_log";
    public static readonly string Axm1SettingFolderPath = "C:\\TomeyApp\\AxialManager\\AxialManager\\_file";

    // AXM1のファイル名
    public static readonly string Axm1DBFileName = "database.sqlite";
    public static readonly string Axm1CommentFileName = "_CommentLog.json";
    public static readonly string Axm1SettingFileName = "setup_client.json";

  }
}
