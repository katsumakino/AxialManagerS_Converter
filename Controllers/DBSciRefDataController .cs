using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;

namespace AxialManagerS_Converter.Controllers {

  public class DBSciRefDataController {

    // レフ(自覚)測定値書込み
    public void SetSciRef(RefList conditions) {
      try {
        if (conditions == null) return;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return;

        bool result = false;
        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();
          int deviceId = Select_Device_ID(sqlConnection, DBConst.AxmDeviceType);

          // クエリコマンド実行
          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.PatientID);
          if (uuid == string.Empty) {
            // AXMからの測定データ登録時は、必ず患者データが存在する
            return;
          } else {
            // EXAM_LISTに保存(右眼測定値)
            var exam_id_r = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.SCI_REF],
                DBConst.eEyeType.RIGHT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Refに保存(右眼測定値)
            var rec_Ref_r = MakeRefRec(exam_id_r,
                DBConst.strEyeType[DBConst.eEyeType.RIGHT],
                deviceId,
                sqlConnection);
            rec_Ref_r.s_d = conditions.RS_d;
            rec_Ref_r.c_d = conditions.RC_d;
            rec_Ref_r.a_deg = conditions.RA_deg;
            rec_Ref_r.se_d = conditions.RS_d + conditions.RC_d / 2;
            rec_Ref_r.is_exam_data = conditions.RS_d != null && conditions.RC_d != null && conditions.RA_deg != null;
            rec_Ref_r.measured_at = conditions.ExamDateTime;

            // DB登録
            result = Insert(rec_Ref_r, sqlConnection);

            // EXAM_LISTに保存(左眼測定値)
            var exam_id_l = RegisterExamList(uuid,
                DBConst.strMstDataType[DBConst.eMSTDATATYPE.SCI_REF],
                DBConst.eEyeType.LEFT,
                conditions.ExamDateTime,
                deviceId,
                sqlConnection);
            // EXAM_Refに保存(左眼測定値)
            var rec_Ref_l = MakeRefRec(exam_id_l,
                DBConst.strEyeType[DBConst.eEyeType.LEFT],
                deviceId,
                sqlConnection);
            rec_Ref_l.s_d = conditions.LS_d;
            rec_Ref_l.c_d = conditions.LC_d;
            rec_Ref_l.a_deg = conditions.LA_deg;
            rec_Ref_l.se_d = conditions.LS_d + conditions.LC_d / 2;
            rec_Ref_l.is_exam_data = conditions.LS_d != null && conditions.LC_d != null && conditions.LA_deg != null;
            rec_Ref_l.measured_at = conditions.ExamDateTime;

            // DB登録
            result &= Insert(rec_Ref_l, sqlConnection);
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
            int aaa = 0;
          }

          // PostgreSQL Server 通信切断
          dbAccess.CloseSqlConnection();
        }
      } catch {
      }

      return;
    }

    public static ExamSciRefRec MakeRefRec(int examId, string posEye, int deviceId, NpgsqlConnection sqlConnection) {

      var recRef = new ExamSciRefRec();
      try {
        recRef.exam_id = examId;
        recRef.examtype_id = Select_Examtype_ID(sqlConnection, DBConst.strMstDataType[DBConst.eMSTDATATYPE.SCI_REF]);
        recRef.eye_id = Select_Eye_ID(sqlConnection, posEye);
        recRef.device_id = deviceId;

        recRef.is_exam_data = true;
        recRef.comment = "";

        recRef.s_d = 0;
        recRef.c_d = 0;
        recRef.a_deg = 0;
        recRef.se_d = 0;

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

    public static bool Insert(ExamSciRefRec aExamSciRefRec, NpgsqlConnection sqlConnection) {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.EXAM_SCI_REF]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_ExamSciRefList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_ExamSciRefList[i]);
        text2 += _bind(COLNAME_ExamSciRefList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_exam_sciref"));
      stringBuilder.Append(_doupdateexam(COLNAME_ExamSciRefList[(int)eExamSciRef.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamSciRefList[(int)eExamSciRef.s_d], aExamSciRefRec.s_d.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamSciRefList[(int)eExamSciRef.c_d], aExamSciRefRec.c_d.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamSciRefList[(int)eExamSciRef.a_deg], aExamSciRefRec.a_deg.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamSciRefList[(int)eExamSciRef.se_d], aExamSciRefRec.se_d.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_ExamSciRefList[(int)eExamSciRef.is_exam_data], aExamSciRefRec.is_exam_data.ToString()));
      stringBuilder.Append(";");
      int num = 0;
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.exam_id], aExamSciRefRec.exam_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.examtype_id], aExamSciRefRec.examtype_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.eye_id], aExamSciRefRec.eye_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.device_id], aExamSciRefRec.device_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.is_exam_data], aExamSciRefRec.is_exam_data);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.comment], aExamSciRefRec.comment);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.s_d], aExamSciRefRec.s_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.c_d], aExamSciRefRec.c_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.a_deg], aExamSciRefRec.a_deg);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.se_d], aExamSciRefRec.se_d);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.measured_at], _DateTimeToObject(aExamSciRefRec.measured_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.updated_at], _DateTimeToObject(aExamSciRefRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_ExamSciRefList[(int)eExamSciRef.created_at], _DateTimeToObject(aExamSciRefRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    // todo: 誤字修正
    public static string[] COLNAME_ExamSciRefList = new string[(int)eExamSciRef.MAX]
    {
      "exam_id", "examtype_id", "eye_id", "device_id", "is_exam_data", "comment", "s_d", "c_d", "a_deg", "se_d", "measured_at", "updated_at", "created_at"
    };

    public enum eExamSciRef {
      exam_id = 0,
      examtype_id,
      eye_id,
      device_id,
      is_exam_data,
      comment,
      s_d,
      c_d,
      a_deg,
      se_d,
      measured_at,
      updated_at,
      created_at,
      MAX
    }
  }
}

public class ExamSciRefRec {
  public int? exam_id { get; set; }
  public int? examtype_id { get; set; }
  public int? eye_id { get; set; }
  public int? device_id { get; set; }
  public bool? is_exam_data { get; set; }
  public string? comment { get; set; }
  public double? s_d { get; set; }
  public double? c_d { get; set; }
  public int? a_deg { get; set; }
  public double? se_d { get; set; }
  public DateTime? measured_at { get; set; }
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}
