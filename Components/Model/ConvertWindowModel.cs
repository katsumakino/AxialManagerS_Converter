using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Controllers;
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
      if (axm1Setting == null) return; // 変換元無し
      if (axm2Setting == null) axm2Setting = new();
      if (axm2Setting.DisplaySetting == null) axm2Setting.DisplaySetting = new();
      if (axm2Setting.OutPutSetting == null) axm2Setting.OutPutSetting = new();  // todo: 出力設定

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
      switch (axm1Setting.Pachy_Device_Type) {
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

      DBAxmCommentController dBAxmComment = new();

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
                  AxmCommentRequest request = new AxmCommentRequest {
                    PatientID = pt_id,
                    AxmComment = new AxmComment() {
                      ID = null,
                      CommentType = property.Name == "PatientData" ? AxmCommentType.Patient : AxmCommentType.ExamDate,
                      Description = values[1],
                      ExamDateTime = DateTime.TryParse(values[0], out DateTime examDate) ? examDate : DateTime.Today
                    }
                  };

                  if (property.Name == "PatientData") {
                    // 被検者コメントをDBに書込
                    dBAxmComment.SetAxmComment(request);
                  } else {
                    if (values[1] != string.Empty) {
                      // 測定日コメントをDBに書込
                      dBAxmComment.SetAxmComment(request);
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
          ConvertAxialTable(connection, Axm1PatientClass.eAxm1DbTable.axialTable);
          ConvertRefTable(connection, Axm1PatientClass.eAxm1DbTable.refObjTable);
          ConvertKrtTable(connection, Axm1PatientClass.eAxm1DbTable.keratoTable);
          ConvertPachyTable(connection, Axm1PatientClass.eAxm1DbTable.pachyTable);
          ConvertMedicalSetupTable(connection, Axm1PatientClass.eAxm1DbTable.medicalSetupTable);
          ConvertMedicalTreatmentTable(connection, Axm1PatientClass.eAxm1DbTable.medicalTreatmentTable);

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

      string birth;
      string gender;

      DBPatientInfoController dbPatientInfo = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)Axm1PatientClass.eAxm1DbTable.patientInfoTable];
        sql += " WHERE ID = '123456789'"; // todo: test確認用
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
              // DateTime型に変換
              patientInfo.BirthDate = DateTime.TryParse(birth, out DateTime birthDate) ? birthDate : null;
              gender = reader[Axm1PatientClass.COLNAME_Axm1PatientInfoList
                [(int)Axm1PatientClass.eAxm1PatientInfoTable.sex]].ToString() ?? string.Empty;
              // Enum値に変換
              if (gender == "M") {
                patientInfo.Gender = Gender.male;
              } else if (gender == "F") {
                patientInfo.Gender = Gender.female;
              } else {
                patientInfo.Gender = Gender.none;
              }

              // todo: SD Converterを見て、高速化手法確認

              // todo: DBに書込

              dbPatientInfo.SetPatientInfo(patientInfo);
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

      DBAxialDataController dBAxialData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE ID = '123456789'"; // todo: test確認用
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              AxialList axialList = new();

              // データを読み取り、変換処理を行う
              axialList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.stDt]].ToString() ?? string.Empty;
              // DateTime型に変換
              axialList.ExamDateTime = DateTime.TryParse(st_dt, out DateTime stDt) ? stDt : null;
              axialR = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.axialOd]].ToString();
              axialList.RAxial = (double.TryParse(axialR, out double dAxialR)) ? dAxialR : null;
              axialL = reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.axialOs]].ToString();
              axialList.LAxial = (double.TryParse(axialL, out double dAxialL)) ? dAxialL : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1AxialList
                [(int)Axm1PatientClass.eAxm1AxialTable.manual]].ToString() == "y");
              axialList.IsRManualInput = manual;
              axialList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す

              dBAxialData.SetOptAxial(axialList);
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

      DBRefDataController dBRefData = new();
      DBSciRefDataController dBSciRefData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE ID = '123456789'"; // todo: test確認用
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              RefList refList = new();

              // データを読み取り、変換処理を行う
              refList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.stDt]].ToString() ?? string.Empty;
              // DateTime型に変換
              refList.ExamDateTime = DateTime.TryParse(st_dt, out DateTime stDt) ? stDt : null;
              sR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.sphOd]].ToString();
              refList.RS_d = (double.TryParse(sR, out double dSR)) ? dSR : null;
              cR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.cylOd]].ToString();
              refList.RC_d = (double.TryParse(cR, out double dCR)) ? dCR : null;
              aR = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.axisOd]].ToString();
              refList.RA_deg = (int.TryParse(aR, out int iAR)) ? iAR : null;
              sL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                 [(int)Axm1PatientClass.eAxm1RefTable.sphOs]].ToString();
              refList.LS_d = (double.TryParse(sL, out double dSL)) ? dSL : null;
              cL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.cylOs]].ToString();
              refList.LC_d = (double.TryParse(cL, out double dCL)) ? dCL : null;
              aL = reader[Axm1PatientClass.COLNAME_Axm1RefList
                [(int)Axm1PatientClass.eAxm1RefTable.axisOs]].ToString();
              refList.LA_deg = (int.TryParse(aL, out int iAL)) ? iAL : null;

              if (table == Axm1PatientClass.eAxm1DbTable.refTable) {
                manual = true;
              } else {
                manual = (reader[Axm1PatientClass.COLNAME_Axm1RefObjList
                  [(int)Axm1PatientClass.eAxm1RefObjTable.manual]].ToString() == "y");
              }
              refList.IsRManualInput = manual;
              refList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
              if (table == Axm1PatientClass.eAxm1DbTable.refTable) {
                dBSciRefData.SetSciRef(refList);
              } else {
                dBRefData.SetRef(refList);
              }
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

      DBKrtDataController dBKrtData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE ID = '123456789'"; // todo: test確認用
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              KrtList krtList = new();

              // データを読み取り、変換処理を行う
              krtList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.stDt]].ToString() ?? string.Empty;
              // DateTime型に変換
              krtList.ExamDateTime = DateTime.TryParse(st_dt, out DateTime stDt) ? stDt : null;
              k1R = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k1Od]].ToString();
              krtList.RK1_mm = (double.TryParse(k1R, out double dK1R)) ? dK1R : null;
              k2R = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k2Od]].ToString();
              krtList.RK2_mm = (double.TryParse(k2R, out double dK2R)) ? dK2R : null;
              cylR = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.cylOd]].ToString();
              krtList.RCyl_d = (double.TryParse(cylR, out double dCylR)) ? dCylR : null;
              k1L = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k1Os]].ToString();
              krtList.LK1_mm = (double.TryParse(k1L, out double dK1L)) ? dK1L : null;
              k2L = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.k2Os]].ToString();
              krtList.LK2_mm = (double.TryParse(k2L, out double dK2L)) ? dK2L : null;
              cylL = reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.cylOs]].ToString();
              krtList.LCyl_d = (double.TryParse(cylL, out double dCylL)) ? dCylL : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.manual]].ToString() == "y");
              krtList.IsRManualInput = manual;
              krtList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
              dBKrtData.SetKrt(krtList);
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

      DBPachyDataController dBPachyData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE ID = '123456789'"; // todo: test確認用
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              PachyList pachyList = new();

              // データを読み取り、変換処理を行う
              pachyList.PatientID = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.id]].ToString() ?? string.Empty;
              st_dt = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.stDt]].ToString() ?? string.Empty;
              // DateTime型に変換
              pachyList.ExamDateTime = DateTime.TryParse(st_dt, out DateTime stDt) ? stDt : null;
              pachyR = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.pachyOd]].ToString();
              pachyList.RPachy = (double.TryParse(pachyR, out double dPachyR)) ? dPachyR : null;
              pachyL = reader[Axm1PatientClass.COLNAME_Axm1PachyList
                [(int)Axm1PatientClass.eAxm1PachyTable.pachyOs]].ToString();
              pachyList.LPachy = (double.TryParse(pachyL, out double dPachyL)) ? dPachyL : null;
              manual = (reader[Axm1PatientClass.COLNAME_Axm1KeratoList
                [(int)Axm1PatientClass.eAxm1KeratoTable.manual]].ToString() == "y");
              pachyList.IsRManualInput = manual;
              pachyList.IsLManualInput = manual;

              // todo: DBに書込
              // todo: Table情報も渡す
              dBPachyData.SetPachy(pachyList);
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

      DBTreatmentController dBTreatment = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE MEDICAL = 'Atropine'"; // todo: test確認用
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
              treatmentList.RGBAColor.R = (int.TryParse(colorR, out int iColorR)) ? iColorR : 0;
              colorG = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.colorG]].ToString();
              treatmentList.RGBAColor.G = (int.TryParse(colorG, out int iColorG)) ? iColorG : 0;
              colorB = reader[Axm1PatientClass.COLNAME_Axm1MedicalSetupList
                [(int)Axm1PatientClass.eAxm1MedicalSetupTable.colorB]].ToString();
              treatmentList.RGBAColor.B = (int.TryParse(colorB, out int iColorB)) ? iColorB : 0;

              // todo: DBに書込
              // todo: IDを新規取得する
              //dBTreatment.SetTreatmentMethod(treatmentList);
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

      DBTreatmentController dBTreatment = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += " WHERE MEDICAL = 'Atropine'"; // todo: test確認用
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              TreatmentData treatmentList = new();
              TreatmentDataRequest treatmentDataRequest = new();

              // データを読み取り、変換処理を行う
              medical = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.id]].ToString() ?? string.Empty;
              id = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.id]].ToString() ?? string.Empty;
              treatmentList.ID = (int.TryParse(id, out int iId)) ? iId : 0;
              start = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.startDate]].ToString();
              // DateTime型に変換
              treatmentList.StartDateTime = DateTime.TryParse(start, out DateTime stDt) ? stDt : null;
              end = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.endDate]].ToString();
              // DateTime型に変換
              treatmentList.EndDateTime = DateTime.TryParse(end, out DateTime edDt) ? edDt : null;

              // todo: DBに書込
              // todo: ID変換(string -> int)
              treatmentDataRequest.PatientID = string.Empty;  // todo:
              treatmentDataRequest.TreatmentData = treatmentList;
              //dBTreatment.SetTreatment(treatmentDataRequest);
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

  }
}
