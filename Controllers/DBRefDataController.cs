using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;

namespace AxialManagerS_Converter.Controllers {

  public class DBRefDataController {

    // レフ(他覚)測定値書込み
    public int SetRef(RefList conditions, GeneralSetting? setting) {
      bool result = true;

      try {
        if (conditions == null) return 1;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return 1;
        if (setting == null) return 1;

        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          int selectId = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)setting.DisplaySetting.RefSelectType]) - 1;
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
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.REF],
                DBConst.eEyeType.RIGHT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Refに保存(右眼測定値)
            var rec_Ref_r = MakeRefRec(exam_id_r,
                DBConst.strEyeType[DBConst.eEyeType.RIGHT],
                setting.DisplaySetting.RefSelectType,
                deviceId,
                sqlConnection);
            rec_Ref_r.s_d[selectId] = conditions.RS_d;
            rec_Ref_r.c_d[selectId] = conditions.RC_d;
            rec_Ref_r.a_deg[selectId] = conditions.RA_deg;
            rec_Ref_r.se_d[selectId] = conditions.RS_d + conditions.RC_d / 2;
            rec_Ref_r.is_exam_data = conditions.RS_d != null && conditions.RC_d != null && conditions.RA_deg != null;
            rec_Ref_r.measured_at = conditions.ExamDateTime;

            // DB登録
            if(rec_Ref_r.is_exam_data == true) {
              result = Insert(rec_Ref_r, sqlConnection);
            }            

            // EXAM_LISTに保存(左眼測定値)
            var exam_id_l = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.REF],
                DBConst.eEyeType.LEFT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Refに保存(左眼測定値)
            var rec_Ref_l = MakeRefRec(exam_id_l,
                DBConst.strEyeType[DBConst.eEyeType.LEFT],
                setting.DisplaySetting.RefSelectType,
                deviceId,
                sqlConnection);
            rec_Ref_l.s_d[selectId] = conditions.LS_d;
            rec_Ref_l.c_d[selectId] = conditions.LC_d;
            rec_Ref_l.a_deg[selectId] = conditions.LA_deg;
            rec_Ref_l.se_d[selectId] = conditions.LS_d + conditions.LC_d / 2;
            rec_Ref_l.is_exam_data = conditions.LS_d != null && conditions.LC_d != null && conditions.LA_deg != null;
            rec_Ref_l.measured_at = conditions.ExamDateTime;

            // DB登録
            if(rec_Ref_l.is_exam_data == true) {
              result &= Insert(rec_Ref_l, sqlConnection);
            }            
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
            string filePath = "C:\\TomeyApp\\AxialManager2\\output.txt";
            string content = "REF:" + conditions.PatientID;

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

    public static ExamRefRec MakeRefRec(int examId, string posEye, SelectType selectType, int deviceId, NpgsqlConnection sqlConnection) {

      var recRef = new ExamRefRec();
      try {
        recRef.exam_id = examId;
        recRef.examtype_id = Select_Examtype_ID(sqlConnection, DBConst.strMstDataType[DBConst.eMSTDATATYPE.REF]);
        recRef.eye_id = Select_Eye_ID(sqlConnection, posEye);
        recRef.device_id = deviceId;

        recRef.is_exam_data = false;
        recRef.comment = ""; // タグが無いので空文字
        recRef.select_id = Select_SelectTypeID(sqlConnection, DBConst.SELECT_TYPE[(int)selectType]) - 1;

        recRef.is_meas_auto = false; // false固定でよい

        recRef.is_human_eye_correction = false;
        recRef.s_d = new List<double?>() { 0, 0, 0 };
        recRef.c_d = new List<double?>() { 0, 0, 0 };
        recRef.a_deg = new List<int?>() { 0, 0, 0 };
        recRef.se_d = new List<double?>() { 0, 0, 0 };
        recRef.vd_mm = 0;

        recRef.is_reliabiliy = false;
        recRef.reliability = new List<string?>() { "", "", "" };
        recRef.data_path = ""; // データパスが無いので空文字

        recRef.measured_at = null;

        // 更新日、作成日は揃える
        var dateNow = DateTime.Now;
        recRef.updated_at = dateNow;
        recRef.created_at = dateNow;
      } catch {
      } finally {
      }
      return recRef;
    }

    public static bool Insert(ExamRefRec aExamRefRec, NpgsqlConnection sqlConnection) {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.EXAM_REF]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_ExamRefList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_ExamRefList[i]);
        text2 += _bind(COLNAME_ExamRefList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_exam_ref"));
      stringBuilder.Append(_doupdateexam(COLNAME_ExamRefList[(int)eExamRef.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamRefList[(int)eExamRef.select_id], aExamRefRec.select_id.ToString()));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamRefList[(int)eExamRef.s_d], aExamRefRec.s_d));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamRefList[(int)eExamRef.c_d], aExamRefRec.c_d));
      stringBuilder.Append(_doupdateintlist(COLNAME_ExamRefList[(int)eExamRef.a_deg], aExamRefRec.a_deg));
      stringBuilder.Append(_doupdatedoublelist(COLNAME_ExamRefList[(int)eExamRef.se_d], aExamRefRec.se_d));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamRefList[(int)eExamRef.is_exam_data], aExamRefRec.is_exam_data.ToString()));
      stringBuilder.Append(";");
      int num = 0;
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.exam_id], aExamRefRec.exam_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.examtype_id], aExamRefRec.examtype_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.eye_id], aExamRefRec.eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.device_id], aExamRefRec.device_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.is_exam_data], aExamRefRec.is_exam_data);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.comment], aExamRefRec.comment);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.select_id], aExamRefRec.select_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.is_meas_auto], aExamRefRec.is_meas_auto);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.is_human_eye_correction], aExamRefRec.is_human_eye_correction);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.s_d], aExamRefRec.s_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.c_d], aExamRefRec.c_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.a_deg], aExamRefRec.a_deg);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.se_d], aExamRefRec.se_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.vd_mm], aExamRefRec.vd_mm);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.is_reliability], aExamRefRec.is_reliabiliy);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.reliability], aExamRefRec.reliability);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.data_path], aExamRefRec.data_path);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.measured_at], _DateTimeToObject(aExamRefRec.measured_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.updated_at], _DateTimeToObject(aExamRefRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamRefList[(int)eExamRef.created_at], _DateTimeToObject(aExamRefRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }
    
    // todo: 誤字修正
    public static string[] COLNAME_ExamRefList = new string[(int)eExamRef.MAX]
    {
      "exam_id", "examtype_id", "eye_id", "device_id", "is_exam_data", "comment", "select_id", "is_meas_auto"
      ,"is_human_eye_correction", "s_d", "c_d", "a_deg", "se_d", "vd_mm", "is_reliabillty"
      , "reliabillty", "data_path","measured_at", "updated_at", "created_at"
    };

    public enum eExamRef {
      exam_id = 0,
      examtype_id,
      eye_id,
      device_id,
      is_exam_data,
      comment,
      select_id,
      is_meas_auto,
      is_human_eye_correction,
      s_d,
      c_d,
      a_deg,
      se_d,
      vd_mm,
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

public class ExamRefRec {
  public int? exam_id { get; set; }
  public int? examtype_id { get; set; }
  public int? eye_id { get; set; }
  public int? device_id { get; set; }
  public bool? is_exam_data { get; set; }
  public string? comment { get; set; }
  public int? select_id { get; set; }
  public bool? is_meas_auto { get; set; }
  public bool? is_human_eye_correction { get; set; }
  public List<double?> s_d { get; set; } = new List<double?>();
  public List<double?> c_d { get; set; } = new List<double?>();
  public List<int?> a_deg { get; set; } = new List<int?>();
  public List<double?> se_d { get; set; } = new List<double?>();
  public double? vd_mm { get; set; }
  public bool? is_reliabiliy { get; set; }
  public List<string?> reliability { get; set; } = new List<string?>();
  public string? data_path { get; set; }
  public DateTime? measured_at { get; set; }
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}
