namespace AxialManagerS_Converter.Common {
  public class GeneralSetting {

    public GeneralSetting() {
    }

    // Jsonファイル読込の場合、配列のデフォルト値は、上書きされずに残ってしまうため、ココで追加する
    public void SetDefaultArray() {
      // ログインユーザ設定
      LoginSetting = [
        new LoginSetting() {
              IsAdmin = true,
              ID = "admin",
              Password = "il8ORXBSLaPqcx2ROgN1Cg=="
            }
      ];

      // 伸長評価用閾値
      DisplaySetting.TreatmentAlertAgeThreshold = [
       // todo: 正常値のデフォルト値設定
       new TreatmentAlertAgeThreshold(){
             Age = 7,
             //StandardValue = 0.0,
             ChangeAmount = 0.179,
             AbnormalIncrementAmount = 0.507
           },
       new TreatmentAlertAgeThreshold(){
             Age = 8,
             //StandardValue = 0.0,
             ChangeAmount = 0.14,
             AbnormalIncrementAmount = 0.389
           },
       new TreatmentAlertAgeThreshold(){
             Age = 9,
             //StandardValue = 0.0,
             ChangeAmount = 0.112,
             AbnormalIncrementAmount = 0.306
           },
       new TreatmentAlertAgeThreshold(){
             Age = 10,
             //StandardValue = 0.0,
             ChangeAmount = 0.091,
             AbnormalIncrementAmount = 0.246
           },
       new TreatmentAlertAgeThreshold() {
             Age = 11,
             //StandardValue = 0.0,
             ChangeAmount = 0.076,
             AbnormalIncrementAmount = 0.20
           },
       new TreatmentAlertAgeThreshold(){
             Age = 12,
             //StandardValue = 0.0,
             ChangeAmount = 0.064,
             AbnormalIncrementAmount = 0.166
           },
       new TreatmentAlertAgeThreshold(){
             Age = 13,
             //StandardValue = 0.0,
             ChangeAmount = 0.055,
             AbnormalIncrementAmount = 0.139
           },
       new TreatmentAlertAgeThreshold(){
             Age = 14,
             //StandardValue = 0.0,
             ChangeAmount = 0.048,
             AbnormalIncrementAmount = 0.117
           },
       new TreatmentAlertAgeThreshold(){
             Age = 15,
             //StandardValue = 0.0,
             ChangeAmount = 0.042,
             AbnormalIncrementAmount = 0.099
           },
       new TreatmentAlertAgeThreshold(){
             Age = 16,
             //StandardValue = 0.0,
             ChangeAmount = 0.037,
             AbnormalIncrementAmount = 0.085
           },
       new TreatmentAlertAgeThreshold(){
             Age = 17,
             //StandardValue = 0.0,
             ChangeAmount = 0.033,
             AbnormalIncrementAmount = 0.072
           },
       new TreatmentAlertAgeThreshold(){
             Age = 18,
             //StandardValue = 0.0,
             ChangeAmount = 0.029,
             AbnormalIncrementAmount = 0.062
           },
       new TreatmentAlertAgeThreshold(){
             Age = 19,
             //StandardValue = 0.0,
             ChangeAmount = 0.026,
             AbnormalIncrementAmount = 0.053
           },
       new TreatmentAlertAgeThreshold(){
             Age = 20,
             //StandardValue = 0.0,
             ChangeAmount = 0.024,
             AbnormalIncrementAmount = 0.046
           },
       new TreatmentAlertAgeThreshold(){
             Age = 21,
             //StandardValue = 0.0,
             ChangeAmount = 0.022,
             AbnormalIncrementAmount = 0.04
           },
      ];

      // 出力ファイル名
      OutPutSetting.ExportFileItem = [
        ExportFileItemType.eExportFileItem_PT_ID,
            ExportFileItemType.eExportFileItem_EXAM_DATE,
            ExportFileItemType.eExportFileItem_EXAM_TIME,
            ExportFileItemType.eExportFileItem_MODEL_NAME_ALMANAGER,
            ExportFileItemType.eExportFileItem_EYE_RL,
            ExportFileItemType.eExportFileItem_EXTENSION_STRING,
            ExportFileItemType.eExportFileItem_NONE,
            ExportFileItemType.eExportFileItem_NONE,
            ExportFileItemType.eExportFileItem_NONE,
            ExportFileItemType.eExportFileItem_NONE
      ];
      OutPutSetting.ExportFileSeparate = [
        ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_3,
            ExportFileSeparateType.eExportFileSeparate_NONE,
            ExportFileSeparateType.eExportFileSeparate_NONE,
            ExportFileSeparateType.eExportFileSeparate_NONE
      ];
    }

    public LoginSetting[]? LoginSetting { get; set; } // ログイン設定
    public DisplaySettingClass DisplaySetting { get; set; } = new DisplaySettingClass();//表示設定
                                                                                        //public TreatmentMethodSetting[] TreatmentMethodSetting { get; set; }//治療方法設定 //変更 TreatmentMethodSetting→TreatmentMethodSetting[]
    public LicenseManage LicenseManage { get; set; } = new LicenseManage();//ライセンス管理
    public OutPutSetting OutPutSetting { get; set; } = new OutPutSetting();//出力設定
                                                                           //public DatabaseSetting DatabaseSetting { get; set; }//データベース設定
                                                                           //public NumberLayoutSetting NumberLayoutSetting { get; set; }
  }

  //ログインユーザ設定
  public class LoginSetting {
    public bool IsAdmin { get; set; }//管理者権限
    public string ID { get; set; }
    public string? Password { get; set; }
  }

  public class DisplaySettingClass {

    //グラフ・測定結果表示の切り替え設定
    public bool IsRefSubVisible { get; set; } = true;//Ref自覚値表示
    public bool IsRefObjVisible { get; set; } = true;//Ref他覚値表示
    public bool IsRefCalcVisible { get; set; } = true;//Ref算出値表示
    public bool IsKrtVisible { get; set; } = true;//角膜曲率表示
    public bool IsPachyVisible { get; set; } = true;//角膜厚表示
    public bool IsDiaVisible { get; set; } = true;//瞳孔径表示
    public bool IsSightVisible { get; set; } = true;//視力表示

    public FittingsType AxialFittingsType { get; set; } = FittingsType.Immersion; //眼軸長変換方式の選択　

    public AxialDeviceType AxialDeviceType { get; set; } = AxialDeviceType.OA2000; //Axial計測装置

    public SelectType RefSelectType { get; set; } = SelectType.Average; //Ref値形式

    public RefDeviceType RefDeviceType { get; set; } = RefDeviceType.MR; //Ref計測装置

    public SelectType KrtSelectType { get; set; } = SelectType.Average;//Krt値形式

    public PhiType KrtPhiType { get; set; } = PhiType.Phi3_0;//Krt測定位置

    public KrtDeviceType KrtDeviceType { get; set; } = KrtDeviceType.OA2000;//Krt計測装置　角膜曲率
    public KrtUnitType KrtUnitType { get; set; } = KrtUnitType.Mm;//Krt単位 mm

    public PachyDeviceType PachyDeviceType { get; set; } = PachyDeviceType.OA2000; //Pachy計測装置　角膜厚

    public DiaDeviceType DiaDeviceType { get; set; } = DiaDeviceType.OA2000;//DIA計測装置 瞳孔径

    public bool IsPtIdAdjustLength { get; set; } = false;//被検者IDの桁揃え有無
    public int PtIdAdjustLength { get; set; } = -1;//被検者IDの桁揃え桁数

    //トレンドグラフ縦軸
    //縦軸範囲の固定・可変
    public VerticalAxisSetting VerticalAxisSetting { get; set; } = VerticalAxisSetting.Fixed;

    public double VerticalAxisVariableUpperLimit { get; set; } = 40.0;//トレンドグラフ縦軸可変上限値
    public double VerticalAxisVariableLowerLimit { get; set; } = 20.0;//トレンドグラフ可変下限値
    public double VerticalAxisFixedUpperLimit { get; set; } = 28.0;//トレンドグラフ縦軸固定上限値
    public double VerticalAxisFixedLowerLimit { get; set; } = 20.0;//トレンドグラフ固定下限値

    public bool BarcodeSearchSetting { get; set; } = true;//バーコード検索設定有効
    public int BarcodeIDStartIndex { get; set; } = 1;//バーコードのID読み込み開始位置
    public int BarcodeIDStringNum { get; set; } = 64;//バーコード　ID文字数

    public bool VerticalAxisPlotValue { get; set; } = true;//トレンドグラフプロット値表示

    public bool MyopiaTendencyInitDisp { get; set; } = true;//近視眼の進行傾向初期表示
    public bool ScormStudyInitDisp { get; set; } = false;//SCORM study初期表示
    public bool PercentileLinearInitDisp { get; set; } = false;//小中学生の眼軸長パーセンタイル(直線)初期表示
    public bool PercentileCurveInitDisp { get; set; } = false;//小中学生の眼軸長パーセンタイル(曲線)初期表示

    public bool VerticalAxisSampleValue { get; set; } = true;//トレンドグラフサンプル値表示
    public bool TreatInterventionAlert { get; set; } = true;//治療介入アラート表示(オプション機能？)
    public bool ComparisionDisplay { get; set; } = true;//比較表示
    public bool SuppressionRate { get; set; } = true;//抑制率表示
    public bool Evalucation { get; set; } = true;//評価表示

    //伸長評価用年代別閾値
    public TreatmentAlertAgeThreshold[]? TreatmentAlertAgeThreshold { get; set; }

    // 検索条件表示
    public bool IsAgeCheck { get; set; } = true; // 年齢チェック
  }

  //眼軸長変換方式の選択　測定した値の内どれで表示を行うか
  public enum FittingsType {
    None,//未設定
    Contact,//コンタクト
    Immersion,//イマージョン
    Contact2,//コンタクト2
    OptLength//光路長
  }

  //Axial計測装置
  public enum AxialDeviceType {
    None,//未設定
    OA1,
    OA2000,
    AXM2//手入力
  }

  //表示方式
  public enum SelectType {
    None,//未設定
    Average,//平均値
    Median,//代表値
  }

  //Ref計測装置
  public enum RefDeviceType {
    None,//未設定
    MR,//MR-6000
    AXM2//手入力
  }

  //Krt測定位置
  public enum PhiType {
    Phi3_0,//φ3,
    Phi2_5,//φ2.5,
    Phi2_0//φ2
  }

  //Krt計測装置　角膜曲率
  public enum KrtDeviceType {
    None,//未設定
    OA2000,//OA-2000
    OA1,//OA-1
    MR,//MR-6000
    AXM2//手入力
  }

  public enum KrtUnitType {
    None,//未設定
    Mm,//mm
    D//Diopter
  }

  //Pachy計測装置　角膜厚
  public enum PachyDeviceType {
    None,//未設定
    OA2000,//OA-2000
    MR,//MR-6000
    AXM2//手入力
  }

  //DIA計測装置 瞳孔径
  public enum DiaDeviceType {
    None,//未設定
    OA2000,//OA-2000
    OA1,//OA-1
    MR,//MR-6000
    AXM2//手入力
  }

  // AXMの設定には存在しない
  public enum TargetEyeType {
    None//未設定
  }

  //縦軸範囲の固定・可変
  public enum VerticalAxisSetting {
    Fixed,//固定
    Variable//可変
  }

  //治療介入アラート年代別閾値
  public class TreatmentAlertAgeThreshold {
    public int Age { get; set; }//年齢
    //public double StandardValue { get; set; }//基準値
    public double ChangeAmount { get; set; }//変化量
    public double AbnormalIncrementAmount { get; set; }//注意変化量
  }

  //ライセンス管理
  public class LicenseManage {
    public string SerialNumber { get; set; } = "000A000000"!;//シリアルナンバー
    public string LicenseKey { get; set; } = "000A"!;//ライセンスキー
    public string LicenseName { get; set; } = "LicenseName"!;//ライセンス名
  }

  //出力設定
  public class OutPutSetting {
    public OutPutFileFormat OutPutFileFormat { get; set; } = OutPutFileFormat.CSV;//出力ファイル形式
    public bool PatientInfo { get; set; } = true;//被検者情報
    public bool PatientComment { get; set; } = true;//被検者コメント
    public bool MeasureDeviceName { get; set; } = true;//測定機器名
    public bool MeasureMethod { get; set; } = true;//測定方法
    public ExportFileItemType[]? ExportFileItem { get; set; }
    public ExportFileSeparateType[]? ExportFileSeparate { get; set; }
  }

  public enum OutPutFileFormat {
    CSV,
    XML
  }

  public enum ExportFileItemType {
    eExportFileItem_NONE = 0,
    eExportFileItem_LASTNAME,
    eExportFileItem_FIRSTNAME,
    eExportFileItem_PT_ID,
    eExportFileItem_EXAM_DATE,
    eExportFileItem_EXAM_TIME,
    eExportFileItem_EYE_RL,
    eExportFileItem_EYE_ODOS,
    eExportFileItem_MODEL_NAME_DEVICE,
    eExportFileItem_MODEL_NAME_ALMANAGER,
    eExportFileItem_EXTENSION_STRING,
    eExportFileItem_EXTENSION_NUMBER,
    MAX
  }

  public enum ExportFileSeparateType {
    eExportFileSeparate_ALLNONE = 0,
    eExportFileSeparate_NONE,
    eExportFileSeparate_SPACE,
    eExportFileSeparate_3,
    eExportFileSeparate_4,
    eExportFileSeparate_5,
    eExportFileSeparate_6,
    eExportFileSeparate_7,
    eExportFileSeparate_8,
    eExportFileSeparate_9,
    eExportFileSeparate_10,
    eExportFileSeparate_11,
    eExportFileSeparate_12,
    eExportFileSeparate_13,
    eExportFileSeparate_14,
    eExportFileSeparate_15,
    eExportFileSeparate_16,
    eExportFileSeparate_17,
    eExportFileSeparate_18,
    eExportFileSeparate_19,
    MAX
  }

  public static class ExportFileDictionary {
    // Enum と String のマッピング
    public static Dictionary<string, ExportFileItemType> FileItemMapping = new() {
    { "(None)",                ExportFileItemType.eExportFileItem_NONE                 },
    { "Last Name",             ExportFileItemType.eExportFileItem_LASTNAME             },
    { "First Name",            ExportFileItemType.eExportFileItem_FIRSTNAME            },
    { "Patient ID",            ExportFileItemType.eExportFileItem_PT_ID                },
    { "Date",                  ExportFileItemType.eExportFileItem_EXAM_DATE            },
    { "Time",                  ExportFileItemType.eExportFileItem_EXAM_TIME            },
    { "Eye(L/R/B)",            ExportFileItemType.eExportFileItem_EYE_RL               },
    { "Eye(OS/OD/OU)",         ExportFileItemType.eExportFileItem_EYE_ODOS             },
    { "Model Name(Device)",    ExportFileItemType.eExportFileItem_MODEL_NAME_DEVICE    },
    { "Model Name(ALManager)", ExportFileItemType.eExportFileItem_MODEL_NAME_ALMANAGER },
    { "Extension String",      ExportFileItemType.eExportFileItem_EXTENSION_STRING     },
    { "Extension Number",      ExportFileItemType.eExportFileItem_EXTENSION_NUMBER     }
  };

    // Enum と String のマッピング
    public static Dictionary<string, ExportFileSeparateType> FileSeparateMapping_first = new() {
    { "(All None)", ExportFileSeparateType.eExportFileSeparate_ALLNONE },
    { "(none)",     ExportFileSeparateType.eExportFileSeparate_NONE    },
    { "(space)",    ExportFileSeparateType.eExportFileSeparate_SPACE   },
    { "_",          ExportFileSeparateType.eExportFileSeparate_3       },
    { "-",          ExportFileSeparateType.eExportFileSeparate_4       },
    { "+",          ExportFileSeparateType.eExportFileSeparate_5       },
    { ",",          ExportFileSeparateType.eExportFileSeparate_6       },
    { "(",          ExportFileSeparateType.eExportFileSeparate_7       },
    { ")",          ExportFileSeparateType.eExportFileSeparate_8       },
    { "[",          ExportFileSeparateType.eExportFileSeparate_9       },
    { "]",          ExportFileSeparateType.eExportFileSeparate_10      },
    { "!",          ExportFileSeparateType.eExportFileSeparate_11      },
    { "#",          ExportFileSeparateType.eExportFileSeparate_12      },
    { "$",          ExportFileSeparateType.eExportFileSeparate_13      },
    { "%",          ExportFileSeparateType.eExportFileSeparate_14      },
    { "&",          ExportFileSeparateType.eExportFileSeparate_15      },
    { "'",          ExportFileSeparateType.eExportFileSeparate_16      },
    { "=",          ExportFileSeparateType.eExportFileSeparate_17      },
    { "~",          ExportFileSeparateType.eExportFileSeparate_18      },
    { "@",          ExportFileSeparateType.eExportFileSeparate_19      }
  };

    public static Dictionary<string, ExportFileSeparateType> FileSeparateMapping = new() {
    { "(none)",     ExportFileSeparateType.eExportFileSeparate_NONE    },
    { "(space)",    ExportFileSeparateType.eExportFileSeparate_SPACE   },
    { "_",          ExportFileSeparateType.eExportFileSeparate_3       },
    { "-",          ExportFileSeparateType.eExportFileSeparate_4       },
    { "+",          ExportFileSeparateType.eExportFileSeparate_5       },
    { ",",          ExportFileSeparateType.eExportFileSeparate_6       },
    { "(",          ExportFileSeparateType.eExportFileSeparate_7       },
    { ")",          ExportFileSeparateType.eExportFileSeparate_8       },
    { "[",          ExportFileSeparateType.eExportFileSeparate_9       },
    { "]",          ExportFileSeparateType.eExportFileSeparate_10      },
    { "!",          ExportFileSeparateType.eExportFileSeparate_11      },
    { "#",          ExportFileSeparateType.eExportFileSeparate_12      },
    { "$",          ExportFileSeparateType.eExportFileSeparate_13      },
    { "%",          ExportFileSeparateType.eExportFileSeparate_14      },
    { "&",          ExportFileSeparateType.eExportFileSeparate_15      },
    { "'",          ExportFileSeparateType.eExportFileSeparate_16      },
    { "=",          ExportFileSeparateType.eExportFileSeparate_17      },
    { "~",          ExportFileSeparateType.eExportFileSeparate_18      },
    { "@",          ExportFileSeparateType.eExportFileSeparate_19      }
  };
  }

  public class DatabaseSetting {
    public int ID { get; set; }//ユーザID
    public string Password { get; set; }//パスワード
    public string IPAdress { get; set; }//IPアドレス //変更 int→string
    public int PortNumber { get; set; }//ポート番号
  }

  //ID桁/取り込み年齢設定
  public class NumberLayoutSetting {
    public bool DigitLimit { get; set; }//ID桁制限切替
    public bool AgeLimit { get; set; }//取り込み年齢設定切替
    public int IDDigits { get; set; }//ID桁ぞろえ制限
    public int AgeLimitUpper { get; set; }//年齢制限上限
    public int AgeLimitLower { get; set; }//年齢制限下限
  }

}
