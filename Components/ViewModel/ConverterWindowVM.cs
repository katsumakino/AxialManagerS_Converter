using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Common.ViewModelBase;

namespace AxialManagerS_Converter.Components.ViewModel {
  internal class ConverterWindowVM : ViewModelBase {

    private Model.ConvertWindowModel _model;
    
    private string _srcFolder = string.Empty;
    public string SrcFolder {
      get { return _srcFolder; }
      set {
        if (_srcFolder != value) {
          _srcFolder = value;
          RaisePropertyChanged(nameof(SrcFolder));
        }
      }
    }

    public ConverterWindowVM() {
      _model = new();
    }

    public void DoConvert() {
      try {
        string settingFilePath = System.IO.Path.Combine(_srcFolder, ConverterGlobal.Axm1SettingFileName);
        // 設定ファイルを読み込み→変換
        // todo: 変換後のファイル名設定
        //_model.ConvertSettingFile(settingFilePath);
        // todo: 合否確認

        // DBからデータを読み込み→変換
        _model.ConvertDB(_srcFolder);
        // todo: 合否確認

        // コメントファイルを読み込み→変換
        //_model.ConvertCommentFile(_srcFolder);

      } catch {
      } finally { }

    }
  }
}
