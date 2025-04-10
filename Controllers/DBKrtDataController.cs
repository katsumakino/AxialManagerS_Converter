using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;

namespace AxialManagerS_Converter.Controllers {

  public class DBKrtDataController {

    // ケラト測定値書込み
    public void SetKrt(KrtList conditions, GeneralSetting? setting) {
      try {
        if (conditions == null) return;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return;
        if (setting == null) return;

        bool result = false;
        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          // todo: 設定取得
          int selectId = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)setting.DisplaySetting.KrtSelectType]) - 1;

          // クエリコマンド実行
          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.PatientID);
          if (uuid == string.Empty) {
            // AXMからの測定データ登録時は、必ず患者データが存在する
            return;
          } else {
            // EXAM_LISTに保存(右眼測定値)
            var exam_id_r = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.KRT],
                DBConst.eEyeType.RIGHT,
                conditions.ExamDateTime,
                conditions.IsRManualInput,
                sqlConnection);
            // EXAM_KRTに保存(右眼測定値)
            var rec_krt_r = MakeKrtRec(exam_id_r,
                DBConst.strEyeType[DBConst.eEyeType.RIGHT],
                setting.DisplaySetting,
                sqlConnection);
            rec_krt_r.k1_mm[selectId] = conditions.RK1_mm;
            rec_krt_r.k1_d[selectId] = conditions.RK1_d;
            rec_krt_r.k2_mm[selectId] = conditions.RK2_mm;
            rec_krt_r.k2_d[selectId] = conditions.RK2_d;
            rec_krt_r.avek_mm[selectId] = (conditions.RK1_mm + conditions.RK2_mm) / 2;
            rec_krt_r.avek_d[selectId] = (conditions.RK1_d + conditions.RK2_d) / 2;
            rec_krt_r.cyl_d[selectId] = conditions.RCyl_d;
            rec_krt_r.is_exam_data = conditions.RK1_mm != null && conditions.RK2_mm != null && conditions.RCyl_d != null
              || conditions.RK1_d != null && conditions.RK2_d != null && conditions.RCyl_d != null;
            rec_krt_r.measured_at = conditions.ExamDateTime;

            // DB登録
            result = Insert(rec_krt_r, sqlConnection);

            // EXAM_LISTに保存(左眼測定値)
            var exam_id_l = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.KRT],
                DBConst.eEyeType.LEFT,
                conditions.ExamDateTime,
                conditions.IsLManualInput,
                sqlConnection);
            // EXAM_KRTに保存(左眼測定値)
            var rec_krt_l = MakeKrtRec(exam_id_l,
                DBConst.strEyeType[DBConst.eEyeType.LEFT],
                setting.DisplaySetting,
                sqlConnection);
            rec_krt_l.k1_mm[selectId] = conditions.LK1_mm;
            rec_krt_l.k1_d[selectId] = conditions.LK1_d;
            rec_krt_l.k2_mm[selectId] = conditions.LK2_mm;
            rec_krt_l.k2_d[selectId] = conditions.LK2_d;
            rec_krt_l.avek_mm[selectId] = (conditions.LK1_mm + conditions.LK2_mm) / 2;
            rec_krt_l.avek_d[selectId] = (conditions.LK1_d + conditions.LK2_d) / 2;
            rec_krt_l.cyl_d[selectId] = conditions.LCyl_d;
            rec_krt_l.is_exam_data = conditions.LK1_mm != null && conditions.LK2_mm != null && conditions.LCyl_d != null
              || conditions.LK1_d != null && conditions.LK2_d != null && conditions.LCyl_d != null;
            rec_krt_l.measured_at = conditions.ExamDateTime;

            // DB登録
            result &= Insert(rec_krt_l, sqlConnection);
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
          }

          // PostgreSQL Server 通信切断
          dbAccess.CloseSqlConnection();
        }
      } catch {
      }

      return;
    }

    public static ExamKrtRec MakeKrtRec(int examId, string posEye, DisplaySettingClass displaySetting, NpgsqlConnection sqlConnection) {

      var recKrt = new ExamKrtRec();
      try {
        recKrt.exam_id = examId;
        recKrt.examtype_id = Select_Examtype_ID(sqlConnection, DBConst.strMstDataType[DBConst.eMSTDATATYPE.KRT]);
        recKrt.eye_id = Select_Eye_ID(sqlConnection, posEye);
        recKrt.device_id = Select_Device_ID(sqlConnection, DBConst.AxmDeviceType);

        recKrt.is_exam_data = false;
        recKrt.comment = ""; // タグが無いので空文字
        recKrt.select_id = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)displaySetting.KrtSelectType]) - 1;

        recKrt.phi_id = Select_PhiId_By_PhiType(sqlConnection, DBConst.PHI_TYPE[(int)displaySetting.KrtPhiType]);
        recKrt.is_meas_auto = false; // false固定でよい

        recKrt.k1_mm = new List<double?>() { 0, 0, 0 };
        recKrt.k1_d = new List<double?>() { 0, 0, 0 };
        recKrt.k1_axis_deg = new List<int?>() { 0, 0, 0 };
        recKrt.k2_mm = new List<double?>() { 0, 0, 0 };
        recKrt.k2_d = new List<double?>() { 0, 0, 0 };
        recKrt.k2_axis_deg = new List<int?>() { 0, 0, 0 };
        recKrt.avek_mm = new List<double?>() { 0, 0, 0 };
        recKrt.avek_d = new List<double?>() { 0, 0, 0 };
        recKrt.k_index = 0; // 0固定でよい
        recKrt.cyl_d = new List<double?>() { 0, 0, 0 };
        recKrt.axis_deg = new List<int?>() { 0, 0, 0 };
        recKrt.is_reliabiliy = false;
        recKrt.reliability = new List<string?>() { "", "", "" };
        recKrt.data_path = ""; // データパスが無いので空文字

        recKrt.measured_at = null;

        // 更新日、作成日は揃える
        var dateNow = DateTime.Now;
        recKrt.updated_at = dateNow;
        recKrt.created_at = dateNow;
      } catch {
      } finally {
      }
      return recKrt;
    }

    public static bool Insert(ExamKrtRec aExamKeratoRec, NpgsqlConnection sqlConnection) {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.EXAM_KRT]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_ExamKrtList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_ExamKrtList[i]);
        text2 += _bind(COLNAME_ExamKrtList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_exam_krt"));
      stringBuilder.Append(_doupdateexam(COLNAME_ExamKrtList[(int)eExamKrt.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamKrtList[(int)eExamKrt.select_id], aExamKeratoRec.select_id.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamKrtList[(int)eExamKrt.phi_id], aExamKeratoRec.phi_id.ToString()));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.k1_mm], aExamKeratoRec.k1_mm));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.k1_d], aExamKeratoRec.k1_d));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.k2_mm], aExamKeratoRec.k2_mm));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.k2_d], aExamKeratoRec.k2_d));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.avek_mm], aExamKeratoRec.avek_mm));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.avek_d], aExamKeratoRec.avek_d));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamKrtList[(int)eExamKrt.cyl_d], aExamKeratoRec.cyl_d));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamKrtList[(int)eExamKrt.is_exam_data], aExamKeratoRec.is_exam_data.ToString()));
      stringBuilder.Append(";");
      int num = 0;
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.exam_id], aExamKeratoRec.exam_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.examtype_id], aExamKeratoRec.examtype_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.eye_id], aExamKeratoRec.eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.device_id], aExamKeratoRec.device_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.is_exam_data], aExamKeratoRec.is_exam_data);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.comment], aExamKeratoRec.comment);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.select_id], aExamKeratoRec.select_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.phi_id], aExamKeratoRec.phi_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.is_meas_auto], aExamKeratoRec.is_meas_auto);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k1_mm], aExamKeratoRec.k1_mm);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k1_d], aExamKeratoRec.k1_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k1_axis_deg], aExamKeratoRec.k1_axis_deg);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k2_mm], aExamKeratoRec.k2_mm);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k2_d], aExamKeratoRec.k2_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k2_axis_deg], aExamKeratoRec.k2_axis_deg);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.avek_mm], aExamKeratoRec.avek_mm);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.avek_d], aExamKeratoRec.avek_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.k_index], aExamKeratoRec.k_index);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.cyl_d], aExamKeratoRec.cyl_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.axis_deg], aExamKeratoRec.axis_deg);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.is_reliability], aExamKeratoRec.is_reliabiliy);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.reliability], aExamKeratoRec.reliability);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.data_path], aExamKeratoRec.data_path);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.measured_at], _DateTimeToObject(aExamKeratoRec.measured_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.updated_at], _DateTimeToObject(aExamKeratoRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamKrtList[(int)eExamKrt.created_at], _DateTimeToObject(aExamKeratoRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    // todo: 誤字修正
    public static string[] COLNAME_ExamKrtList = new string[(int)eExamKrt.MAX]
    {
      "exam_id", "examtype_id", "eye_id", "device_id", "is_exam_data", "comment", "select_id", "phi_id", "is_meas_auto", "k1_mm"
      , "k1_d", "k1_axis_deg", "k2_mm", "k2_d", "k2_axis_deg", "avek_mm", "avek_d", "k_index", "cyl_d", "axis_deg", "is_reliabillty"
      , "reliabillty", "data_path","measured_at", "updated_at", "created_at"
    };

    public enum eExamKrt {
      exam_id = 0,
      examtype_id,
      eye_id,
      device_id,
      is_exam_data,
      comment,
      select_id,
      phi_id,
      is_meas_auto,
      k1_mm,
      k1_d,
      k1_axis_deg,
      k2_mm,
      k2_d,
      k2_axis_deg,
      avek_mm,
      avek_d,
      k_index,
      cyl_d,
      axis_deg,
      is_reliability,
      reliability,
      data_path,
      measured_at,
      updated_at,
      created_at,
      MAX
    }
  }
}

public class ExamKrtRec {
  public int? exam_id { get; set; }
  public int? examtype_id { get; set; }
  public int? eye_id { get; set; }
  public int? device_id { get; set; }
  public bool? is_exam_data { get; set; }
  public string? comment { get; set; }
  public int? select_id { get; set; }
  public int? phi_id { get; set; }
  public bool? is_meas_auto { get; set; }
  public List<double?> k1_mm { get; set; } = new List<double?>();
  public List<double?> k1_d { get; set; } = new List<double?>();
  public List<int?> k1_axis_deg { get; set; } = new List<int?>();
  public List<double?> k2_mm { get; set; } = new List<double?>();
  public List<double?> k2_d { get; set; } = new List<double?>();
  public List<int?> k2_axis_deg { get; set; } = new List<int?>();
  public List<double?> avek_mm { get; set; } = new List<double?>();
  public List<double?> avek_d { get; set; } = new List<double?>();
  public double? k_index { get; set; }
  public List<double?> cyl_d { get; set; } = new List<double?>();
  public List<int?> axis_deg { get; set; } = new List<int?>();
  public bool? is_reliabiliy { get; set; }
  public List<string?> reliability { get; set; } = new List<string?>();
  public string? data_path { get; set; }
  public DateTime? measured_at { get; set; }
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}
