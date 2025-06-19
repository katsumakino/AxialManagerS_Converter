using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;

namespace AxialManagerS_Converter.Controllers {


  public class DBAxialDataController {

    // 眼軸長測定値書込み
    public int SetOptAxial(AxialList conditions, GeneralSetting? setting) {
      bool result = true;

      try {
        if (conditions == null) return 1;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return 1;
        if (setting == null) return 1;

        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          int selectId = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)DBConst.SelectType.average]) - 1;
          int deviceId;
          if (conditions.IsRManualInput || conditions.IsLManualInput) {
            deviceId = Select_Device_ID(sqlConnection, DBConst.AxmDeviceType);
          } else {
            deviceId = Select_Device_ID(sqlConnection, DBConst.AxmOldDeviceType);
          }
          
          // クエリコマンド実行
          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.PatientID);
          if (uuid == string.Empty) {
            // AXMからの測定データ登録時は、必ず患者データが存在する
            return 1;
          } else {
            // EXAM_LISTに保存(右眼測定値)
            var exam_id_r = RegisterExamList(uuid,
            DBConst.strMstDataType[DBConst.eMSTDATATYPE.OPTAXIAL],
                DBConst.eEyeType.RIGHT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_OPTAXIALに保存(右眼測定値)
            var rec_optaxial_r = MakeOptaxialRec(exam_id_r,
              DBConst.strEyeType[DBConst.eEyeType.RIGHT],
              setting.DisplaySetting.AxialFittingsType,
              deviceId,
              sqlConnection);
            rec_optaxial_r.axial_mm[selectId] = conditions.RAxial;
            rec_optaxial_r.is_exam_data = conditions.RAxial != null;
            rec_optaxial_r.measured_at = conditions.ExamDateTime;

            // DB登録
            if (rec_optaxial_r.is_exam_data == true) {
              result = Insert(rec_optaxial_r, sqlConnection);
            }

            // EXAM_LISTに保存(左眼測定値)
            var exam_id_l = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.OPTAXIAL],
                DBConst.eEyeType.LEFT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_OPTAXIALに保存(左眼測定値)
            var rec_optaxial_l = MakeOptaxialRec(exam_id_l,
              DBConst.strEyeType[DBConst.eEyeType.LEFT],
              setting.DisplaySetting.AxialFittingsType,
              deviceId,
              sqlConnection);
            rec_optaxial_l.axial_mm[selectId] = conditions.LAxial;
            rec_optaxial_l.is_exam_data = conditions.LAxial != null;
            rec_optaxial_l.measured_at = conditions.ExamDateTime;

            // DB登録
            if(rec_optaxial_l.is_exam_data == true) {
              result &= Insert(rec_optaxial_l, sqlConnection);
            }            
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
            string filePath = "C:\\TomeyApp\\AxialManager2\\output.txt";
            string content = "AXIAL:" + conditions.PatientID;

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

    public static ExamOptaxialRec MakeOptaxialRec(int examId, string posEye, FittingsType fittingsType, int deviceId, NpgsqlConnection sqlConnection) {

      int selectId = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)DBConst.SelectType.none]);
      int fittingId = Select_FittingId_By_FittingType(sqlConnection, DBConst.FITTINGS_TYPE[(int)fittingsType]);
      int targetEyeId = Select_TargetEyeId_By_TargetEyeType(sqlConnection, DBConst.TARGET_EYE_TYPE[(int)DBConst.TargetEyeType.none]);

      var recOpax = new ExamOptaxialRec();
      try {
        recOpax.exam_id = examId;
        recOpax.examtype_id = Select_Examtype_ID(sqlConnection, DBConst.strMstDataType[DBConst.eMSTDATATYPE.OPTAXIAL]);
        recOpax.eye_id = Select_Eye_ID(sqlConnection, posEye);
        recOpax.device_id = deviceId;
        recOpax.is_exam_data = true;
        recOpax.comment = string.Empty;
        recOpax.select_id = selectId;
        recOpax.target_eye_id = targetEyeId;
        recOpax.fitting_id = fittingId;
        recOpax.iol_eye_id = Select_IolEyeId_By_IolEyeType(sqlConnection, "none");

        recOpax.is_meas_auto = false;
        recOpax.axial_mm.AddRange(new List<double?>() { 0, 0, 0 });
        recOpax.sd = 0;
        recOpax.snr.AddRange(new List<int?>() { 0, 0, 0 });
        recOpax.is_average_ref_ind = false;
        recOpax.axial_ref_ind = 0;
        recOpax.pachy_ref_ind = 0;
        recOpax.acd_ref_ind = 0;
        recOpax.lens_ref_ind = 0;
        recOpax.iol_ref_ind = 0;
        recOpax.vitreous_ref_ind = 0;

        recOpax.is_caliper = false;
        recOpax.is_reliability = false;
        recOpax.reliability.AddRange(new List<string?>() { string.Empty, string.Empty, string.Empty });
        recOpax.data_path = string.Empty;
        recOpax.measured_at = null;

        // 更新日、作成日は揃える
        var dateNow = DateTime.Now;
        recOpax.updated_at = dateNow;
        recOpax.created_at = dateNow;
      } catch {
      } finally {
      }
      return recOpax;
    }

    public static bool Insert(ExamOptaxialRec aExamOptaxialRec, NpgsqlConnection sqlConnection) {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.EXAM_OPTAXIAL]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_ExamOptaxialList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_ExamOptaxialList[i]);
        text2 += _bind(COLNAME_ExamOptaxialList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_exam_optaxial"));
      stringBuilder.Append(_doupdateexam(COLNAME_ExamOptaxialList[(int)eExamOptAxial.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamOptaxialList[(int)eExamOptAxial.axial_mm], aExamOptaxialRec.axial_mm));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.fitting_id], aExamOptaxialRec.fitting_id.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_exam_data], aExamOptaxialRec.is_exam_data.ToString()));
      stringBuilder.Append(";");
      int num = 0;
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.exam_id], aExamOptaxialRec.exam_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.examtype_id], aExamOptaxialRec.examtype_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.eye_id], aExamOptaxialRec.eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.device_id], aExamOptaxialRec.device_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_exam_data], aExamOptaxialRec.is_exam_data);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.comment], aExamOptaxialRec.comment);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.select_id], aExamOptaxialRec.select_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.target_eye_id], aExamOptaxialRec.target_eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.fitting_id], aExamOptaxialRec.fitting_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.iol_eye_id], aExamOptaxialRec.iol_eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_meas_auto], aExamOptaxialRec.is_meas_auto);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.axial_mm], aExamOptaxialRec.axial_mm);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.sd], aExamOptaxialRec.sd);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.snr], aExamOptaxialRec.snr);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_average_ref_ind], aExamOptaxialRec.is_average_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.axial_ref_ind], aExamOptaxialRec.axial_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.pachy_ref_ind], aExamOptaxialRec.pachy_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.acd_ref_ind], aExamOptaxialRec.acd_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.lens_ref_ind], aExamOptaxialRec.lens_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.iol_ref_ind], aExamOptaxialRec.iol_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.vitreous_ref_ind], aExamOptaxialRec.vitreous_ref_ind);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_caliper], aExamOptaxialRec.is_caliper);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.is_reliability], aExamOptaxialRec.is_reliability);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.reliability], aExamOptaxialRec.reliability);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.data_path], aExamOptaxialRec.data_path);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.measured_at], _DateTimeToObject(aExamOptaxialRec.measured_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.updated_at], _DateTimeToObject(aExamOptaxialRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamOptaxialList[(int)eExamOptAxial.created_at], _DateTimeToObject(aExamOptaxialRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    public static string[] COLNAME_ExamOptaxialList = new string[(int)eExamOptAxial.MAX]
    {
      "exam_id", "examtype_id", "eye_id", "device_id", "is_exam_data", "comment", "select_id", "target_eye_id", "fitting_id", "iol_eye_id", "is_meas_auto",
      "axial_mm", "sd", "snr", "is_average_ref_ind", "axial_ref_ind", "pachy_ref_ind", "acd_ref_ind", "lens_ref_ind", "iol_ref_ind", "vitreous_ref_ind",
      "is_caliper", "is_reliability", "reliability", "data_path", "measured_at", "updated_at", "created_at"
    };

    public enum eExamOptAxial {
      exam_id = 0,
      examtype_id,
      eye_id,
      device_id,
      is_exam_data,
      comment,
      select_id,
      target_eye_id,
      fitting_id,
      iol_eye_id,
      is_meas_auto,
      axial_mm,
      sd,
      snr,
      is_average_ref_ind,
      axial_ref_ind,
      pachy_ref_ind,
      acd_ref_ind,
      lens_ref_ind,
      iol_ref_ind,
      vitreous_ref_ind,
      is_caliper,
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

public class ExamOptaxialRec {
  public int? exam_id { get; set; }
  public int? examtype_id { get; set; }
  public int? eye_id { get; set; }
  public int? device_id { get; set; }
  public bool? is_exam_data { get; set; }
  public string? comment { get; set; } = string.Empty;
  public int? select_id { get; set; }
  public int? target_eye_id { get; set; }
  public int? fitting_id { get; set; }
  public int? iol_eye_id { get; set; }
  public bool? is_meas_auto { get; set; }
  public List<double?> axial_mm { get; set; } = new List<double?>();
  public double? sd { get; set; }
  public List<int?> snr { get; set; } = new List<int?>();
  public bool? is_average_ref_ind { get; set; }
  public double? axial_ref_ind { get; set; }
  public double? pachy_ref_ind { get; set; }
  public double? acd_ref_ind { get; set; }
  public double? lens_ref_ind { get; set; }
  public double? iol_ref_ind { get; set; }
  public double? vitreous_ref_ind { get; set; }
  public bool? is_caliper { get; set; }
  public bool? is_reliability { get; set; }
  public List<string?> reliability { get; set; } = new List<string?>();
  public string? data_path { get; set; } = string.Empty;
  public DateTime? measured_at { get; set; }
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}
