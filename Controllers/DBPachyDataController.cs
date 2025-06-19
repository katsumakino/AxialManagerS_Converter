using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;

namespace AxialManagerS_Converter.Controllers {

  public class DBPachyDataController {

    // 角膜厚測定値書込み
    public int SetPachy(PachyList conditions) {
      bool result = true;

      try {
        if (conditions == null) return 1;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return 1;

        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          int selectId = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)DBConst.SelectType.average]) - 1;
          int deviceId = Select_Device_ID(sqlConnection, DBConst.AxmDeviceType);

          // クエリコマンド実行
          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.PatientID);
          if (uuid == string.Empty) {
            // AXMからの測定データ登録時は、必ず患者データが存在する
            return 1;
          } else {
            // EXAM_LISTに保存(右眼測定値)
            var exam_id_r = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.PACHY_CCT],
                DBConst.eEyeType.RIGHT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Pachyに保存(右眼測定値)
            var rec_Pachy_r = MakePachyRec(exam_id_r,
                DBConst.strEyeType[DBConst.eEyeType.RIGHT],
                deviceId,
                sqlConnection);
            rec_Pachy_r.pachy_um[selectId] = conditions.RPachy;
            rec_Pachy_r.is_exam_data = conditions.RPachy != null;
            rec_Pachy_r.measured_at = conditions.ExamDateTime;

            // DB登録
            if(rec_Pachy_r.is_exam_data == true) {
              result = Insert(rec_Pachy_r, sqlConnection);
            }           

            // EXAM_LISTに保存(左眼測定値)
            var exam_id_l = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.PACHY_CCT],
                DBConst.eEyeType.LEFT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Pachyに保存(左眼測定値)
            var rec_Pachy_l = MakePachyRec(exam_id_l,
                DBConst.strEyeType[DBConst.eEyeType.LEFT],
                deviceId,
                sqlConnection);
            rec_Pachy_l.pachy_um[selectId] = conditions.LPachy;
            rec_Pachy_l.is_exam_data = conditions.LPachy != null;
            rec_Pachy_l.measured_at = conditions.ExamDateTime;

            // DB登録
            if(rec_Pachy_l.is_exam_data == true) {
              result &= Insert(rec_Pachy_l, sqlConnection);
            }
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
            string filePath = "C:\\TomeyApp\\AxialManager2\\output.txt";
            string content = "PACHY:" + conditions.PatientID;

            // ファイルの末尾に書き込む
            System.IO.File.AppendAllText(filePath, content + Environment.NewLine);
          }

          // PostgreSQL Server 通信切断
          dbAccess.CloseSqlConnection();
        }
      } catch {
      }

      return (result)? 0 : 1;
    }

    public static ExamPachyRec MakePachyRec(int examId, string posEye, int deviceId, NpgsqlConnection sqlConnection) {

      var recPachy = new ExamPachyRec();
      try {

        // todo: 設定ファイルから情報取得
        // todo: Target_EYESとFITTINGSのDBテーブルを入替
        int fittingId = Select_FittingId_By_FittingType(sqlConnection, DBConst.FITTINGS_TYPE[(int)DBConst.FittingsType.none]);
        int targetEyeId = Select_TargetEyeId_By_TargetEyeType(sqlConnection, DBConst.TARGET_EYE_TYPE[(int)DBConst.TargetEyeType.none]);

        recPachy.exam_id = examId;
        recPachy.examtype_id = Select_Examtype_ID(sqlConnection, DBConst.strMstDataType[DBConst.eMSTDATATYPE.PACHY_CCT]);
        recPachy.eye_id = Select_Eye_ID(sqlConnection, posEye);
        recPachy.device_id = deviceId;

        recPachy.is_exam_data = false;
        recPachy.comment = ""; // タグが無いので空文字
        recPachy.select_id = 0; // 0固定でよい

        recPachy.target_eye_id = targetEyeId;
        recPachy.fitting_id = fittingId;

        recPachy.is_meas_auto = false; // false固定でよい
        recPachy.pachy_um = new List<double?>() { 0, 0, 0 };

        recPachy.oct_sd = 0;
        recPachy.oct_snr = new List<int?>() { 0, 0, 0 };

        recPachy.is_oct_average_ref_ind = false;
        recPachy.oct_pachy_ref_ind = 0;

        recPachy.is_us_velocity = false;
        recPachy.us_velocity_mpers = 0;

        recPachy.is_us_bias_offset_um = false;
        recPachy.us_bias_offset_um = 0;
        recPachy.us_bias_offset_per = 0;

        recPachy.is_em_us_correction = false;
        recPachy.em_us_correction_um = 0;

        recPachy.is_reliabiliy = false;
        recPachy.reliability = new List<string?>() { "", "", "" };
        recPachy.data_path = ""; // データパスが無いので空文字

        recPachy.measured_at = null;

        // 更新日、作成日は揃える
        var dateNow = DateTime.Now;
        recPachy.updated_at = dateNow;
        recPachy.created_at = dateNow;
      } catch {
      } finally {
      }
      return recPachy;
    }

    public static bool Insert(ExamPachyRec aExamPachyRec, NpgsqlConnection sqlConnection) {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.EXAM_PACHY_CCT]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_ExamPachyList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_ExamPachyList[i]);
        text2 += _bind(COLNAME_ExamPachyList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_exam_pachy_cct"));
      stringBuilder.Append(_doupdateexam(COLNAME_ExamPachyList[(int)eExamPachy.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamPachyList[(int)eExamPachy.pachy_um], aExamPachyRec.pachy_um));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamPachyList[(int)eExamPachy.is_exam_data], aExamPachyRec.is_exam_data.ToString()));
      stringBuilder.Append(";");
      int num = 0;
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.exam_id], aExamPachyRec.exam_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.examtype_id], aExamPachyRec.examtype_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.eye_id], aExamPachyRec.eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.device_id], aExamPachyRec.device_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_exam_data], aExamPachyRec.is_exam_data);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.comment], aExamPachyRec.comment);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.select_id], aExamPachyRec.select_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.target_eye_id], aExamPachyRec.target_eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.fitting_id], aExamPachyRec.fitting_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_meas_auto], aExamPachyRec.is_meas_auto);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.pachy_um], aExamPachyRec.pachy_um);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.oct_sd], aExamPachyRec.oct_sd);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.oct_snr], aExamPachyRec.oct_snr);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_oct_average_ref_ind], aExamPachyRec.is_oct_average_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.oct_pachy_ref_ind], aExamPachyRec.oct_pachy_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_us_velocity], aExamPachyRec.is_us_velocity);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.us_velocity_mpers], aExamPachyRec.us_velocity_mpers);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_us_bias_offset_um], aExamPachyRec.is_us_bias_offset_um);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.us_bias_offset_um], aExamPachyRec.us_bias_offset_um);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.us_bias_offset_per], aExamPachyRec.us_bias_offset_per);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_em_us_correction], aExamPachyRec.is_em_us_correction);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.em_us_correction_um], aExamPachyRec.em_us_correction_um);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.is_reliability], aExamPachyRec.is_reliabiliy);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.reliability], aExamPachyRec.reliability);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.data_path], aExamPachyRec.data_path);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.measured_at], _DateTimeToObject(aExamPachyRec.measured_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.updated_at], _DateTimeToObject(aExamPachyRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamPachyList[(int)eExamPachy.created_at], _DateTimeToObject(aExamPachyRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    // todo: 誤字修正
    public static string[] COLNAME_ExamPachyList = new string[(int)eExamPachy.MAX]
    {
      "exam_id", "examtype_id", "eye_id", "device_id", "is_exam_data", "comment", "select_id", "target_eye_id", "fitting_id", "is_meas_auto",
      "pachy_um", "oct_sd", "oct_snr", "is_oct_average_ref_ind", "oct_pachy_ref_ind", "is_us_velocity", "us_velocity_mpers", "is_us_bias_offset_um",
      "us_bias_offset_um", "us_bias_offset_per", "is_em_us_correction", "em_us_correction_um", "is_reliability", "reliability", "data_path",
      "measured_at", "updated_at", "created_at"
    };

    public enum eExamPachy {
      exam_id = 0,
      examtype_id,
      eye_id,
      device_id,
      is_exam_data,
      comment,
      select_id,
      target_eye_id,
      fitting_id,
      is_meas_auto,
      pachy_um,
      oct_sd,
      oct_snr,
      is_oct_average_ref_ind,
      oct_pachy_ref_ind,
      is_us_velocity,
      us_velocity_mpers,
      is_us_bias_offset_um,
      us_bias_offset_um,
      us_bias_offset_per,
      is_em_us_correction,
      em_us_correction_um,
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

public class ExamPachyRec {
  public int? exam_id { get; set; }
  public int? examtype_id { get; set; }
  public int? eye_id { get; set; }
  public int? device_id { get; set; }
  public bool? is_exam_data { get; set; }
  public string? comment { get; set; }
  public int? select_id { get; set; }
  public int? target_eye_id { get; set; }
  public int? fitting_id { get; set; }
  public bool? is_meas_auto { get; set; }
  public List<double?> pachy_um { get; set; } = new List<double?>();
  public double? oct_sd { get; set; }
  public List<int?> oct_snr { get; set; } = new List<int?>();
  public bool? is_oct_average_ref_ind { get; set; }
  public double? oct_pachy_ref_ind { get; set; }
  public bool? is_us_velocity { get; set; }
  public int? us_velocity_mpers { get; set; }
  public bool? is_us_bias_offset_um { get; set; }
  public int? us_bias_offset_um { get; set; }
  public int? us_bias_offset_per { get; set; }
  public bool? is_em_us_correction { get; set; }
  public int? em_us_correction_um { get; set; }
  public bool? is_reliabiliy { get; set; }
  public List<string?> reliability { get; set; } = new List<string?>();
  public string? data_path { get; set; }
  public DateTime? measured_at { get; set; }
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}
