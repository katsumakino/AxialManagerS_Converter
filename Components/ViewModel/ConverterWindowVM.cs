using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Common.ViewModelBase;

namespace AxialManagerS_Converter.Components.ViewModel {
  internal class ConverterWindowVM : ViewModelBase {

    #region ■フィールド/プロパティ

    public int ExamYearRangeMin {
      get { return _model.examYearRangeMin; }
    }

    public int ExamYearRangeMax {
      get { return _model.examYearRangeMax; }
    }

    public int ExamYearMin {
      get { return _model.examYearMin; }
      set {
        if (_model.examYearMin != value) {
          _model.examYearMin = value;
          RaisePropertyChanged(nameof(ExamYearMin));
        }
      }
    }

    public int ExamYearMax {
      get { return _model.examYearMax; }
      set {
        if (_model.examYearMax != value) {
          _model.examYearMax = value;
          RaisePropertyChanged(nameof(ExamYearMax));
        }
      }
    }

    public int AgeRangeMin {
      get { return _model.ageRangeMin; }
    }

    public int AgeRangeMax {
      get { return _model.ageRangeMax; }
    }

    public int AgeMin {
      get { return _model.ageMin; }
      set {
        if (_model.ageMin != value) {
          _model.ageMin = value;
          RaisePropertyChanged(nameof(AgeMin));
        }
      }
    }

    public int AgeMax {
      get { return _model.ageMax; }
      set {
        if (_model.ageMax != value) {
          _model.ageMax = value;
          RaisePropertyChanged(nameof(AgeMax));
        }
      }
    }

    public bool SetExamYearRange {
      get { return _model.setExamYearRange; }
      set {
        if (_model.setExamYearRange != value) {
          _model.setExamYearRange = value;
          RaisePropertyChanged(nameof(SetExamYearRange));
        }
      }
    }

    public bool SetAgeRange {
      get { return _model.setAgeRange; }
      set {
        if (_model.setAgeRange != value) {
          _model.setAgeRange = value;
          RaisePropertyChanged(nameof(SetAgeRange));
        }
      }
    }

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

    public int ExecErrorCount {
      get { return _model.execErrorCount; }
      set {
        if (_model.execErrorCount != value) {
          _model.execErrorCount = value;
          RaisePropertyChanged(nameof(ExecErrorCount));
        }
      }
    }

    #endregion

    #region ■コンストラクタ

    public ConverterWindowVM() {
      _model = new();
    }

    #endregion

    #region ■メソッド

    public void DoConvert() {
      try {
        ExecErrorCount = 0;

        string settingFilePath = System.IO.Path.Combine(_srcFolder, ConverterGlobal.Axm1SettingFileName);
        // 設定ファイルを読み込み→変換
        _model.ConvertSettingFile(settingFilePath);

        // DBからデータを読み込み→変換
        _model.ConvertDB(_srcFolder);

        // コメントファイルを読み込み→変換
        _model.ConvertCommentFile(_srcFolder);
      } catch {
      } finally { }

    }

    public void GetConvertPatientCount() {
      _model.GetConvertPatientCount(_srcFolder);
    }

    public int GetConvertProgress() {
      if(_model.patientCount == 0) {
        return 100;
      }
      return _model.progressValue * 100 / _model.patientCount;
    }

    #endregion
  }
}
