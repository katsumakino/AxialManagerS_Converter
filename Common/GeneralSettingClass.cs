using System.ComponentModel;

namespace AxialManagerS_Converter.Common {
  public class GeneralSetting {
    public LoginSetting[] LoginSetting { get; set; }//ログイン設定 //変更 LoginSetting→LoginSetting[]
    public DisplaySettingClass DisplaySetting { get; set; }//表示設定
    //public TreatmentMethodSetting[] TreatmentMethodSetting { get; set; }//治療方法設定 //変更 TreatmentMethodSetting→TreatmentMethodSetting[]
    public LicenseManage LicenseManage { get; set; }//ライセンス管理
    public OutPutSetting OutPutSetting { get; set; }//出力設定
    public DatabaseSetting DatabaseSetting { get; set; }//データベース設定
    public NumberLayoutSetting NumberLayoutSetting { get; set; }
  }

  //ログインユーザ設定
  public class LoginSetting {
    public bool IsAdmin { get; set; }//管理者権限
    public string ID { get; set; }
    public string? Password { get; set; }
  }

  public class DisplaySettingClass {

    //グラフ・測定結果表示の切り替え設定
    public bool IsAxialVisible { get; set; }//眼軸長表示
    public bool IsRefSubVisible { get; set; }//Ref自覚値表示
    public bool IsRefObjVisible { get; set; }//Ref他覚値表示
    public bool IsRefCalcVisible { get; set; }//Ref算出値表示
    public bool IsKrtVisible { get; set; }//角膜曲率表示
    public bool IsPachyVisible { get; set; }//角膜厚表示
    public bool IsDiaVisible { get; set; }//瞳孔径表示
    public bool IsSightVisible { get; set; }//視力表示

    public FittingsType AxialFittingsType { get; set; } //眼軸長変換方式の選択　

    public AxialDeviceType AxialDeviceType { get; set; } //Axial計測装置

    public SelectType RefSelectType { get; set; } //Ref値形式

    public RefDeviceType RefDeviceType { get; set; } //Ref計測装置

    public SelectType KrtSelectType { get; set; }//Krt値形式

    public PhiType KrtPhiType { get; set; }//Krt測定位置

    public KrtDeviceType KrtDeviceType { get; set; }//Krt計測装置　角膜曲率

    public PachyDeviceType PachyDeviceType { get; set; } //Pachy計測装置　角膜厚

    public DiaDeviceType DiaDeviceType { get; set; }//DIA計測装置 瞳孔径

    public TargetEyeType TargetEyeType { get; set; }// todo: AXMの設定には存在しない

    //トレンドグラフ縦軸
    //縦軸範囲の固定・可変
    public VerticalAxisSetting VerticalAxisSetting { get; set; }

    public double VerticalAxisVariableUpperLimit { get; set; }//トレンドグラフ縦軸固定上限値
    public double VerticalAxisVariableLowerLimit { get; set; }//トレンドグラフ固定下限値
    public double VerticalAxisFixedUpperLimit { get; set; }//トレンドグラフ縦軸可変上限値
    public double VerticalAxisFixedLowerLimit { get; set; }//トレンドグラフ可変下限値
    public bool VerticalAxisPlotValue { get; set; }//トレンドグラフプロット値表示
    public bool VerticalAxisSampleValue { get; set; }//トレンドグラフサンプル値表示
    public bool TreatInterventionAlert { get; set; }//治療介入アラート表示
    public bool ComparisionDisplay { get; set; }//比較表示
    public bool SuppressionRate { get; set; }//抑制率表示
    public bool Evalucation { get; set; }//評価表示

    //治療介入アラート年代別閾値
    public TreatmentAlertAgeThreshold[] TreatmentAlertAgeThreshold { get; set; }

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
    public double StandardValue { get; set; }//基準値
    public double ChangeAmount { get; set; }//変化量
    public double AbnormalIncrementAmount { get; set; }//異常増加量
  }

  //治療方法設定
  //public class TreatmentMethodSetting {
  //  public int ID { get; set; }
  //  public string TreatName { get; set; }//治療方法の名前
  //  public RGBAColor RGBAColor { get; set; }//治療方法に割り当てられた色
  //  public int SuppresionRate { get; set; }//抑制率
  //}

  //ライセンス管理
  public class LicenseManage {
    public string SerialNumber { get; set; }//シリアルナンバー
    public string LicenseKey { get; set; }//ライセンスキー
    public string LicenseName { get; set; }//ライセンス名
  }

  //出力設定
  public class OutPutSetting {
    OutPutFileFormat OutPutFileFormat { get; set; }//出力ファイル形式
    public bool PatientInfo { get; set; }//患者情報
    public bool PatientComment { get; set; }//患者コメント
    public bool MeasureDeviceName { get; set; }//測定機器名
    public bool MeasureMethod { get; set; }//測定方法
  }

  public enum OutPutFileFormat {
    CSV,
    XML
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
