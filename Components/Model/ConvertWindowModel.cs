using AxialManagerS_Converter.Common;
using AxialManagerS_Converter.Controllers;
using AxialManagerS_Converter.Converter;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace AxialManagerS_Converter.Components.Model {
  public class ConvertWindowModel {

    // DB検索条件のプロパティ設定
    public readonly int examYearRangeMin = 2015;
    public readonly int examYearRangeMax = DateTime.Now.Year;
    public int examYearMin = 2015;
    public int examYearMax = DateTime.Now.Year;

    public readonly int ageRangeMin = 0;
    public readonly int ageRangeMax = 120;
    public int ageMin = 6;
    public int ageMax = 22;

    public bool setExamYearRange = false;
    public bool setAgeRange = false;

    public int patientCount = 0;
    public int progressValue = 0;

    public int execErrorCount = 0; // エラー件数

    // todo: 設定ファイルのパスを確認(wwwroot?)
    private string settingDirTopPath = @"C:/TomeyApp/AxialManager2/Setting/";
    private readonly string settingFileName = "GeneralSetting.json";
    private GeneralSetting? axm2Setting = new();

    // 旧AXM治療IDとAXM2治療IDの対比表
    class TreatmentIdTable {
      public string Axm1TreatmentId { get; set; } = string.Empty;
      public int Axm2TreatmentId { get; set; } = 0;
    }
    private List<TreatmentIdTable> treatmentIdList = new();

    static int? GetAxm2IdByAxm1Id(List<TreatmentIdTable> list, string axm1Id) {
      var id = list.FirstOrDefault(p => p.Axm1TreatmentId == axm1Id);
      return id?.Axm2TreatmentId;
    }

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

        axm2Setting = new();

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
        // エラー処理
        execErrorCount++;
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
                    execErrorCount += dBAxmComment.SetAxmComment(request);
                  } else {
                    if (values[1] != string.Empty) {
                      // 測定日コメントをDBに書込
                      execErrorCount += dBAxmComment.SetAxmComment(request);
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

          ConvertMedicalSetupTable(connection);
          ConvertPatientInfoTable(connection);
        }
      } catch {
      } finally { }

      return true;
    }

    public void GetConvertPatientCount(string srcPath) {
      int count = 0;

      try {
        // SQLiteデータベースに接続
        string dbFilePath = System.IO.Path.Combine(srcPath, ConverterGlobal.Axm1DBFileName);
        using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;")) {
          connection.Open();

          string sql = "SELECT COUNT(*) FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)Axm1PatientClass.eAxm1DbTable.patientInfoTable];
          if(setAgeRange) {
            DateOnly birthMin = DBCommonController.CalculateBirthDateFromAge(ageMin);
            DateOnly birthMax = DBCommonController.CalculateBirthDateFromAge(ageMax, true);
            sql += " WHERE Birth BETWEEN";
            sql += (" '" + birthMax.ToString("yyyy/MM/dd") + "' AND '" + birthMin.ToString("yyyy/MM/dd") + "'");
          }

          using (var command = new SQLiteCommand(sql, connection)) {
            using (var reader = command.ExecuteReader()) {
              while (reader.Read()) {
                count = Convert.ToInt32(reader[0]);
              }
            }
          }
        }
      } catch {
      } finally { }

      patientCount = count;
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

      DateOnly birthMin = DBCommonController.CalculateBirthDateFromAge(ageMin);
      DateOnly birthMax = DBCommonController.CalculateBirthDateFromAge(ageMax, true);
      string yearMin = examYearMin.ToString() + "/01/01";
      string yearMax = examYearMax.ToString() + "/12/31";

      progressValue = 0;

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)Axm1PatientClass.eAxm1DbTable.patientInfoTable];
        if (setAgeRange) {
          sql += "WHERE Birth BETWEEN";
          sql += (" '" + birthMax.ToString("yyyy/MM/dd") + "' AND '" + birthMin.ToString("yyyy/MM/dd") + "'");
        }
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

              // DBに書込
              execErrorCount += dbPatientInfo.SetPatientInfo(patientInfo);
          
              ConvertAxialTable(connection, patientInfo.ID, yearMin, yearMax);
              ConvertRefTable(connection, patientInfo.ID, yearMin, yearMax, true);
              ConvertRefTable(connection, patientInfo.ID, yearMin, yearMax, false);
              ConvertKrtTable(connection, patientInfo.ID, yearMin, yearMax);
              ConvertPachyTable(connection, patientInfo.ID, yearMin, yearMax);
              ConvertMedicalTreatmentTable(connection, patientInfo.ID);

              progressValue++;
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
    private bool ConvertAxialTable(SQLiteConnection connection, string pt_id, string min, string max) {

      if (axm2Setting == null) {
        return false;
      }

      Axm1PatientClass.eAxm1DbTable table;

      switch (axm2Setting.DisplaySetting.AxialFittingsType) {
        case FittingsType.Contact:
          table = Axm1PatientClass.eAxm1DbTable.axialContactTable;
          break;
        case FittingsType.Contact2:
          table = Axm1PatientClass.eAxm1DbTable.axialContact2Table;
          break;
        case FittingsType.OptLength:
          table = Axm1PatientClass.eAxm1DbTable.axialOptLengthTable;
          break;
        case FittingsType.Immersion:
          table = Axm1PatientClass.eAxm1DbTable.axialTable;
          break;
        default:
          return false;
      }

      string st_dt;
      string? axialR;
      string? axialL;
      bool manual;

      DBAxialDataController dBAxialData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += (" WHERE ID = '" + pt_id + "'");
        if (setExamYearRange) {
          sql += " AND ST_DT BETWEEN";
          sql += (" '" + max + "' AND '" + min + "'");
        }
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

              // DBに書込
              execErrorCount += dBAxialData.SetOptAxial(axialList, axm2Setting);
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
    private bool ConvertRefTable(SQLiteConnection connection, string pt_id, string min, string max, bool isObj) {

      if (axm2Setting == null) {
        return false;
      }

      Axm1PatientClass.eAxm1DbTable table;
      if (isObj) {
        switch (axm2Setting.DisplaySetting.RefSelectType) {
          case SelectType.Average:
            table = Axm1PatientClass.eAxm1DbTable.refObjTable;
            break;
          case SelectType.Median:
            table = Axm1PatientClass.eAxm1DbTable.refObjTypTable;
            break;
          default:
            return false;
        }
      } else {
        table = Axm1PatientClass.eAxm1DbTable.refTable;
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
        sql += (" WHERE ID = '" + pt_id + "'");
        if (setExamYearRange) {
          sql += " AND ST_DT BETWEEN";
          sql += (" '" + max + "' AND '" + min + "'");
        }
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

              if (table == Axm1PatientClass.eAxm1DbTable.refTable) {
                // DBに書込(自覚値)
                execErrorCount += dBSciRefData.SetSciRef(refList);
              } else {
                // DBに書込(他覚値)
                execErrorCount += dBRefData.SetRef(refList, axm2Setting);
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
    private bool ConvertKrtTable(SQLiteConnection connection, string pt_id, string min, string max) {

      if (axm2Setting == null) {
        return false;
      }

      Axm1PatientClass.eAxm1DbTable table;

      switch (axm2Setting.DisplaySetting.KrtSelectType) {
        case SelectType.Average:
          switch (axm2Setting.DisplaySetting.KrtDeviceType) {
            case KrtDeviceType.OA2000:
              switch (axm2Setting.DisplaySetting.KrtPhiType) {
                case PhiType.Phi2_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOA20Table;
                  break;
                case PhiType.Phi2_5:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOA25Table;
                  break;
                case PhiType.Phi3_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOATable;
                  break;
                default:
                  return false;
              }
              break;
            case KrtDeviceType.MR:
              switch (axm2Setting.DisplaySetting.KrtPhiType) {
                case PhiType.Phi2_0:
                  table = Axm1PatientClass.eAxm1DbTable.kerato20Table;
                  break;
                case PhiType.Phi2_5:
                  table = Axm1PatientClass.eAxm1DbTable.kerato25Table;
                  break;
                case PhiType.Phi3_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoTable;
                  break;
                default:
                  return false;
              }
              break;
            default:
              return false;
          }
          break;
        case SelectType.Median:
          switch (axm2Setting.DisplaySetting.KrtDeviceType) {
            case KrtDeviceType.OA2000:
              switch (axm2Setting.DisplaySetting.KrtPhiType) {
                case PhiType.Phi2_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOA20TypTable;
                  break;
                case PhiType.Phi2_5:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOA25TypTable;
                  break;
                case PhiType.Phi3_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoOATypTable;
                  break;
                default:
                  return false;
              }
              break;
            case KrtDeviceType.MR:
              switch (axm2Setting.DisplaySetting.KrtPhiType) {
                case PhiType.Phi2_0:
                  table = Axm1PatientClass.eAxm1DbTable.kerato20TypTable;
                  break;
                case PhiType.Phi2_5:
                  table = Axm1PatientClass.eAxm1DbTable.kerato25TypTable;
                  break;
                case PhiType.Phi3_0:
                  table = Axm1PatientClass.eAxm1DbTable.keratoTypTable;
                  break;
                default:
                  return false;
              }
              break;
            default:
              return false;
          }
          break;
        default:
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
        sql += (" WHERE ID = '" + pt_id + "'");
        if (setExamYearRange) {
          sql += " AND ST_DT BETWEEN";
          sql += (" '" + max + "' AND '" + min + "'");
        }
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

              // DBに書込
              execErrorCount += dBKrtData.SetKrt(krtList, axm2Setting);
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
    private bool ConvertPachyTable(SQLiteConnection connection, string pt_id, string min, string max) {

      if (axm2Setting == null) {
        return false;
      }

      Axm1PatientClass.eAxm1DbTable table;

      switch (axm2Setting.DisplaySetting.PachyDeviceType) {
        case PachyDeviceType.OA2000:
          table = Axm1PatientClass.eAxm1DbTable.pachyTable;
          break;
        case PachyDeviceType.MR:
          table = Axm1PatientClass.eAxm1DbTable.pachyMRTable;
          break;
        default:
          return false;
      }

      string st_dt;
      string? pachyR;
      string? pachyL;
      bool manual;

      DBPachyDataController dBPachyData = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += (" WHERE ID = '" + pt_id + "'");
        if (setExamYearRange) {
          sql += " AND ST_DT BETWEEN";
          sql += (" '" + max + "' AND '" + min + "'");
        }
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

              // DBに書込
              execErrorCount += dBPachyData.SetPachy(pachyList);
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
    private bool ConvertMedicalSetupTable(SQLiteConnection connection) {

      Axm1PatientClass.eAxm1DbTable table = Axm1PatientClass.eAxm1DbTable.medicalSetupTable;

      string? id;
      string? colorR;
      string? colorG;
      string? colorB;

      DBTreatmentController dBTreatment = new();

      // 治療方法IDの対比表を初期化
      treatmentIdList.Clear();

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

              // IDを新規取得する
              treatmentList.ID = dBTreatment.GetTreatmentId(treatmentList.TreatName);

              // DBに書込
              execErrorCount += dBTreatment.SetTreatmentMethod(treatmentList);

              // 治療IDの対比表更新
              treatmentIdList.Add(new TreatmentIdTable() {
                Axm1TreatmentId = id,
                Axm2TreatmentId = treatmentList.ID
              });
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
    private bool ConvertMedicalTreatmentTable(SQLiteConnection connection, string pt_id) {

      Axm1PatientClass.eAxm1DbTable table = Axm1PatientClass.eAxm1DbTable.medicalTreatmentTable;

      if (treatmentIdList.Count == 0) {
        // 治療方法テーブルの変換が行われていない
        return false;
      }

      int? treatId;
      string? medical;
      string? id;
      string? start;
      string? end;

      DBTreatmentController dBTreatment = new();

      try {
        string sql = "SELECT * FROM " + Axm1PatientClass.Axm1DB_TableNames[(int)table];
        sql += (" WHERE ID = '" + pt_id + "'");
        using (var command = new SQLiteCommand(sql, connection)) {
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              TreatmentData treatmentList = new();
              TreatmentDataRequest treatmentDataRequest = new();

              // データを読み取り、変換処理を行う
              medical = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.Medical]].ToString() ?? string.Empty;
              id = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.id]].ToString() ?? string.Empty;
              start = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.startDate]].ToString();
              // DateTime型に変換
              treatmentList.StartDateTime = DateTime.TryParse(start, out DateTime stDt) ? stDt : null;
              end = reader[Axm1PatientClass.COLNAME_Axm1MedicalTreatmentList
                [(int)Axm1PatientClass.eAxm1MedicalTreatmentTable.endDate]].ToString();
              // DateTime型に変換
              treatmentList.EndDateTime = DateTime.TryParse(end, out DateTime edDt) ? edDt : null;

              // ID変換(string -> int)
              treatId = GetAxm2IdByAxm1Id(treatmentIdList, medical);
              if (treatId != null) {
                treatmentList.TreatID = (int)treatId;
              } else {
                continue;
              }

              // 被検者IDを取得
              if (id == null || id == string.Empty) {
                continue;
              }
              treatmentDataRequest.PatientID = id;
              treatmentDataRequest.TreatmentData = treatmentList;

              // DBに書込
              execErrorCount += dBTreatment.SetTreatment(treatmentDataRequest);
            }
          }
        }

      } catch {
      } finally { }

      return true;
    }

  }
}
