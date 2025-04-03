using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Converter;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace AxialManagerS_Converter.Components.Model {
  public class ConvertWindowModel {

    // todo: 設定ファイルのパスを確認(wwwroot?)
    private string settingDirTopPath = @"C:/TomeyApp/AxialManager2/Setting/";
    private readonly string settingFileName = "GeneralSetting.json";

    /// <summary>
    /// 設定ファイル変換
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="destPath"></param>
    /// <returns></returns>
    public bool ConvertSettingFile(string srcPath) {
      FIleUtilities utilities = new();

      try {
        if (!utilities.FileExists(srcPath)) {
          return false;
        }

        // Jsonファイルから読出
        string json = File.ReadAllText(srcPath);

        // Jsonファイルからデシリアライズ
        var setting = JsonSerializer.Deserialize<Axm1SettingClass.JsonSettingData>(json);

        string destPath = Path.Combine(settingDirTopPath, settingFileName);

        GeneralSetting? axm2Setting = new();

        if (utilities.FileExists(destPath)) {
          // AXM2の設定ファイル読出し
          string json2 = File.ReadAllText(destPath);
          axm2Setting = JsonSerializer.Deserialize<GeneralSetting>(json2);
        }

        // AXM2の設定ファイルに変換
        JsonSettingDataToGeneralSetting(in setting, ref axm2Setting);

        // フォルダの作成
        utilities.CreateDir(settingDirTopPath);

        // JSONシリアライズオプションを設定
        var options = new JsonSerializerOptions {
          WriteIndented = true
        };

        // AXM2の設定ファイルに書込
        string writeJson = JsonSerializer.Serialize(axm2Setting, options);
        File.WriteAllText(destPath, writeJson);

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// 旧AXMの設定をAXM2の設定に変換
    /// </summary>
    /// <param name="axm1Setting"></param>
    /// <param name="axm2Setting"></param>
    private void JsonSettingDataToGeneralSetting(in Axm1SettingClass.JsonSettingData? axm1Setting, ref GeneralSetting? axm2Setting) {
      if(axm1Setting == null) return; // 変換元無し
      if(axm2Setting == null) axm2Setting = new();
      if(axm2Setting.DisplaySetting == null) axm2Setting.DisplaySetting = new();
      if(axm2Setting.OutPutSetting == null) axm2Setting.OutPutSetting = new();  // todo: 出力設定

      // AXIAL 変換方式
      switch (axm1Setting.Axial_Converter_Type) {
        case Axm1SettingClass.SelectConvertType.kConvertTypeContact:
          axm2Setting.DisplaySetting.AxialFittingsType = FittingsType.Contact;
          break;
        case Axm1SettingClass.SelectConvertType.kConvertTypeContact2:
          axm2Setting.DisplaySetting.AxialFittingsType = FittingsType.Contact2;
          break;
        case Axm1SettingClass.SelectConvertType.kConvertTypeOptLength:
          axm2Setting.DisplaySetting.AxialFittingsType = FittingsType.OptLength;
          break;
        case Axm1SettingClass.SelectConvertType.kConvertTypeImmersion:
        default:
          axm2Setting.DisplaySetting.AxialFittingsType = FittingsType.Immersion;
          break;
      }

      // REF データタイプ
      switch (axm1Setting.Ref_Value_Type) {
        case Axm1SettingClass.SelectValueType.kValueTypeTypical:
          axm2Setting.DisplaySetting.RefSelectType = SelectType.Median;
          break;
        case Axm1SettingClass.SelectValueType.kValueTypeAverage:
        default:
          axm2Setting.DisplaySetting.RefSelectType = SelectType.Average;
          break;
      }

      // KRT データタイプ
      switch (axm1Setting.Krt_Value_Type) {
        case Axm1SettingClass.SelectValueType.kValueTypeTypical:
          axm2Setting.DisplaySetting.KrtSelectType = SelectType.Median;
          break;
        case Axm1SettingClass.SelectValueType.kValueTypeAverage:
        default:
          axm2Setting.DisplaySetting.KrtSelectType = SelectType.Average;
          break;
      }

      // KRT 装置種別
      switch (axm1Setting.Krt_Device_Type) {
        case Axm1SettingClass.SelectDeviceType.kDeviceTypeMR:
          axm2Setting.DisplaySetting.KrtDeviceType = KrtDeviceType.MR;
          break;
        case Axm1SettingClass.SelectDeviceType.kDeviceTypeOA:
        default:
          axm2Setting.DisplaySetting.KrtDeviceType = KrtDeviceType.OA2000;
          break;
      }

      // KRT 測定位置
      switch (axm1Setting.Krt_Phi_Type) {
        case Axm1SettingClass.SelectPhiType.kPhiType2:
          axm2Setting.DisplaySetting.KrtPhiType = PhiType.Phi2_0;
          break;
        case Axm1SettingClass.SelectPhiType.kPhiType2_5:
          axm2Setting.DisplaySetting.KrtPhiType = PhiType.Phi2_5;
          break;
        case Axm1SettingClass.SelectPhiType.kPhiType3:
        default:
          axm2Setting.DisplaySetting.KrtPhiType = PhiType.Phi3_0;
          break;
      }

      // PACHY 装置種別
      switch(axm1Setting.Pachy_Device_Type) {
        case Axm1SettingClass.SelectDeviceType.kDeviceTypeMR:
          axm2Setting.DisplaySetting.PachyDeviceType = PachyDeviceType.MR;
          break;
        case Axm1SettingClass.SelectDeviceType.kDeviceTypeOA:
        default:
          axm2Setting.DisplaySetting.PachyDeviceType = PachyDeviceType.OA2000;
          break;
      }
    }

    /// <summary>
    /// コメントファイル変換
    /// </summary>
    /// <param name="srcPath"></param>
    /// <returns></returns>
    public bool ConvertCommentFile(string srcPath) {
      FIleUtilities utilities = new();

      // srcPathからファイル名にCommentLog.jsonを含むファイルを探索
      List<string> commentFileList = new();

      try {
        utilities.SearchFilesContainingString(srcPath, ConverterGlobal.Axm1CommentFileName, ref commentFileList);

        foreach (string commentFile in commentFileList) {
          string pt_id = utilities.ExtractFileName(commentFile).Replace(ConverterGlobal.Axm1CommentFileName, string.Empty);
          string json = File.ReadAllText(commentFile);

          using (JsonDocument doc = JsonDocument.Parse(json)) {
            JsonElement root = doc.RootElement;
            foreach (JsonProperty property in root.EnumerateObject()) {
              if (property.Value.ValueKind == JsonValueKind.Array) {
                List<string> values = new List<string>();
                foreach (JsonElement element in property.Value.EnumerateArray()) {
                  values.Add(element.GetString());
                }

                if (values.Count == 2) {
                  if (property.Name == "PatientData") {
                    // todo: 被検者コメントをDBに書込

                  } else {
                    if (values[1] != string.Empty) {
                      // todo: 測定日コメントをDBに書込

                    }
                  }
                }
              }
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    public bool ConvertDB(string srcPath) {

      try {
        // SQLiteデータベースに接続
        string dbFilePath = System.IO.Path.Combine(srcPath, ConverterGlobal.Axm1DBFileName);
        using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;")) {
          connection.Open();

          ConvertPatientInfoTable(connection);
        }
      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// 被検者情報テーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    private bool ConvertPatientInfoTable(SQLiteConnection connection) {

      string birth = string.Empty;
      string gender = string.Empty;
      string updateDate = string.Empty;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)Axm1PatientClass.eAxm1DbTable.patientInfoTable];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              PatientInfo patientInfo = new();

              // データを読み取り、変換処理を行う
              patientInfo.ID = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.id]].ToString() ?? string.Empty;
              patientInfo.FirstName = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.firstName]].ToString() ?? string.Empty;
              patientInfo.FamilyName = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.lastName]].ToString() ?? string.Empty;
              birth = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.birth]].ToString() ?? string.Empty;
              // todo: DateTime型に変換
              gender = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.sex]].ToString() ?? string.Empty;
              // todo: Enum値に変換
              updateDate = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.updateDate]].ToString() ?? string.Empty;
              // todo: DateTime型に変換

              // todo: SD Converterを見て、高速化手法確認

              // todo: DBに書込
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// Axialデータテーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertAxialTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table < Axm1PatientClass.eAxm1DbTable.axialTable
        || table > Axm1PatientClass.eAxm1DbTable.axialOptLengthTable) {
        return false;
      }

      string st_dt;
      string? axialR;
      string? axialL;
      bool manual;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              AxialList axialList = new();

              // データを読み取り、変換処理を行う
              axialList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.stDt]].ToString() ?? string.Empty;
              // todo: DateTime型に変換
              axialR = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.axialOd]].ToString();
              axialList.RAxial = (axialR != null) ? Convert.ToDouble(axialR) : null;
              axialL = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.axialOs]].ToString();
              axialList.LAxial = (axialL != null) ? Convert.ToDouble(axialL) : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.manual]].ToString() == "y");
              axialList.IsRManualInput = manual;
              axialList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// Refデータテーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertRefTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table < Axm1PatientClass.eAxm1DbTable.refObjTable
        || table > Axm1PatientClass.eAxm1DbTable.refTable) {
        return false;
      }

      string st_dt;
      string? sR;
      string? cR;
      string? aR;
      string? sL;
      string? cL;
      string? aL;
      bool manual;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              RefList axialList = new();

              // データを読み取り、変換処理を行う
              axialList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.stDt]].ToString() ?? string.Empty;
              // todo: DateTime型に変換
              sR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.sphOd]].ToString();
              axialList.RS_d = (sR != null) ? Convert.ToDouble(sR) : null;
              cR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.cylOd]].ToString();
              axialList.RC_d = (cR != null) ? Convert.ToDouble(cR) : null;
              aR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.axisOd]].ToString();
              axialList.RA_deg = (aR != null) ? Convert.ToInt32(aR) : null;
              sL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                 [(int)Axm1PatientClass.eAxm1RefTable.sphOs]].ToString();
              axialList.LS_d = (sR != null) ? Convert.ToDouble(sL) : null;
              cL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.cylOs]].ToString();
              axialList.LC_d = (cL != null) ? Convert.ToDouble(cL) : null;
              aL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.axisOs]].ToString();
              axialList.LA_deg = (aL != null) ? Convert.ToInt32(aL) : null;

              if (table == Axm1PatientClass.eAxm1DbTable.refTable) {
                manual = true;
              } else {
                manual = (reader[Axm1PatientClass.COLNAME_Axm1RefList
                  [(int)Axm1PatientClass.eAxm1RefObjTable.manual]].ToString() == "y");
              }
              axialList.IsRManualInput = manual;
              axialList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// KRTデータテーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertKrtTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table < Axm1PatientClass.eAxm1DbTable.keratoTable
        || table > Axm1PatientClass.eAxm1DbTable.keratoOA20TypTable) {
        return false;
      }

      string st_dt;
      string? k1R;
      string? k2R;
      string? cylR;
      string? k1L;
      string? k2L;
      string? cylL;
      bool manual;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              KrtList krtList = new();

              // データを読み取り、変換処理を行う
              krtList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.stDt]].ToString() ?? string.Empty;
              // todo: DateTime型に変換
              k1R = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k1Od]].ToString();
              krtList.RK1_mm = (k1R != null) ? Convert.ToDouble(k1R) : null;
              k2R = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k2Od]].ToString();
              krtList.RK2_mm = (k2R != null) ? Convert.ToDouble(k2R) : null;
              cylR = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.cylOd]].ToString();
              krtList.RCyl_d = (cylR != null) ? Convert.ToDouble(cylR) : null;
              k1L = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k1Os]].ToString();
              krtList.LK1_mm = (k1L != null) ? Convert.ToDouble(k1L) : null;
              k2L = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k2Os]].ToString();
              krtList.LK2_mm = (k2L != null) ? Convert.ToDouble(k2L) : null;
              cylL = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.cylOs]].ToString();
              krtList.LCyl_d = (cylL != null) ? Convert.ToDouble(cylL) : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.manual]].ToString() == "y");
              krtList.IsRManualInput = manual;
              krtList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// Pachyデータテーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertPachyTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table < Axm1PatientClass.eAxm1DbTable.pachyTable
        || table > Axm1PatientClass.eAxm1DbTable.pachyMRTable) {
        return false;
      }

      string st_dt;
      string? pachyR;
      string? pachyL;
      bool manual;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              PachyList pachyList = new();

              // データを読み取り、変換処理を行う
              pachyList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.stDt]].ToString() ?? string.Empty;
              // todo: DateTime型に変換
              pachyR = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.pachyOd]].ToString();
              pachyList.RPachy = (pachyR != null) ? Convert.ToDouble(pachyR) : null;
              pachyL = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.pachyOs]].ToString();
              pachyList.LPachy = (pachyL != null) ? Convert.ToDouble(pachyL) : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.manual]].ToString() == "y");
              pachyList.IsRManualInput = manual;
              pachyList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// 治療方法テーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertMedicalSetupTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table != Axm1PatientClass.eAxm1DbTable.medicalSetupTable) {
        return false;
      }

      string? id;
      string? colorR;
      string? colorG;
      string? colorB;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              TreatmentMethodSetting treatmentList = new();
              treatmentList.RGBAColor = new();

              // データを読み取り、変換処理を行う
              id = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.medical]].ToString() ?? string.Empty;
              treatmentList.TreatName = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.name]].ToString() ?? string.Empty;
              colorR = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.colorR]].ToString();
              treatmentList.RGBAColor.R = (colorR != null) ? Convert.ToInt32(colorR) : 0;
              colorG = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.colorG]].ToString();
              treatmentList.RGBAColor.G = (colorG != null) ? Convert.ToInt32(colorG) : 0;
              colorB = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.colorB]].ToString();
              treatmentList.RGBAColor.B = (colorB != null) ? Convert.ToInt32(colorB) : 0;

              // todo: DBに書込
              // todo: IDを新規取得する
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

    /// <summary>
    /// 治療状況テーブルの変換
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private bool ConvertMedicalTreatmentTable(SQLiteConnection connection, Axm1PatientClass.eAxm1DbTable table) {

      if (table != Axm1PatientClass.eAxm1DbTable.medicalTreatmentTable) {
        return false;
      }

      string? medical;
      string? id;
      string? start;
      string? end;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              TreatmentData treatmentList = new();

              // データを読み取り、変換処理を行う
              medical = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.id]].ToString() ?? string.Empty;
              id = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.id]].ToString() ?? string.Empty;
              treatmentList.ID = (id != null) ? Convert.ToInt32(id) : 0;
              start = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.startDate]].ToString();
              // todo: DateTime型に変換
              end = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.endDate]].ToString();
              // todo: DateTime型に変換

              // todo: DBに書込
              // todo: ID変換(string -> int)
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

  }
}
