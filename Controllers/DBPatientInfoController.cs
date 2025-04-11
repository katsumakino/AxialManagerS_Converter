using AxialManagerS_Converter.Common;
using Npgsql;
using System.Data;
using System.Text;
using System.Text.Json;
using static AxialManagerS_Converter.Controllers.DBAxmCommentController;
using static AxialManagerS_Converter.Controllers.DBCommonController;
using static AxialManagerS_Converter.Controllers.DBTreatmentController;

namespace AxialManagerS_Converter.Controllers {

  public class DBPatientInfoController {

    // 患者情報書込み
    public void SetPatientInfo(PatientInfo conditions) {
      try {
        if (conditions == null) return;
        if (conditions.ID == null || conditions.ID == string.Empty) return;

        bool result = false;
        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.ID);
          if (uuid == string.Empty) {
            // Insert
            DateTime dateTime = DateTime.Now;
            PatientRec patientRec = new() {
              pt_id = conditions.ID,
              pt_lastname = conditions.FamilyName ?? string.Empty,
              pt_firstname = conditions.FirstName ?? string.Empty,
              gender_id = Select_GenderId(sqlConnection, GENDER_TYPE[(int)conditions.Gender]),
              pt_dob = conditions.BirthDate,
              pt_description = string.Empty,
              pt_updated_at = dateTime,
              pt_created_at = dateTime
            };

            result = Insert(sqlConnection, patientRec);

            // AXM用患者情報テーブルにも登録
            uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.ID);
            if (uuid != string.Empty) {
              AxmPatientRec axmPatientRec = new() {
                pt_uuid = uuid,
                axm_pt_id = SelectMaxAxmPatientId(sqlConnection),
                axm_flag = conditions.Mark,
                is_axm_same_pt_id = (conditions.SameID != null && conditions.SameID != string.Empty),
                axm_same_pt_id = conditions.SameID ?? string.Empty,
                updated_at = dateTime,
                created_at = dateTime
              };

              var axm_pt_id_ = Select_AxmPatientID_by_PK(sqlConnection, uuid);
              if (axm_pt_id_ != -1) axmPatientRec.axm_pt_id = axm_pt_id_;

              result &= InsertAxmPatient(sqlConnection, axmPatientRec);
            }
          } else {
            // Update
            // 装置出力データ取込時は、入力あり→なしにはしない(アプリ上での編集時は可能)
            DateTime dateTime = DateTime.Now;
            PatientRec patientRec = new() {
              pt_uuid = uuid,
              pt_id = conditions.ID,
              pt_lastname = conditions.FamilyName ?? string.Empty,
              pt_firstname = conditions.FirstName ?? string.Empty,
              gender_id = Select_GenderId(sqlConnection, GENDER_TYPE[(int)conditions.Gender]),
              pt_dob = conditions.BirthDate,
              pt_updated_at = dateTime
            };

            result = Update(sqlConnection, patientRec);

            // AXM用患者情報テーブルにも更新
            AxmPatientRec axmPatientRec = new() {
              pt_uuid = uuid,
              axm_pt_id = SelectMaxAxmPatientId(sqlConnection),
              axm_flag = conditions.Mark,
              is_axm_same_pt_id = (conditions.SameID != null && conditions.SameID != string.Empty),
              axm_same_pt_id = conditions.SameID ?? string.Empty,
              updated_at = dateTime,
              created_at = dateTime
            };

            var axm_pt_id_ = Select_AxmPatientID_by_PK(sqlConnection, uuid);
            if (axm_pt_id_ != -1) axmPatientRec.axm_pt_id = axm_pt_id_;

            result &= InsertAxmPatient(sqlConnection, axmPatientRec);
          }
        } catch {
        } finally {
          if (!result) {
            // todo: Error通知
            string filePath = "C:\\TomeyApp\\AxialManager2\\output.txt";
            string content = "PATIENT:" + conditions.ID;

            // ファイルの末尾に書き込む
            System.IO.File.AppendAllText(filePath, content + Environment.NewLine);
          }

          // PostgreSQL Server 通信切断
          dbAccess.CloseSqlConnection();
        }

      } catch {
      }

      return;
    }

    // 主キー重複時Update
    private bool Insert(NpgsqlConnection sqlConnection, PatientRec aPatientRec) {
      int num = 0;

      StringBuilder stringBuilder = new();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.PATIENT_LIST]));
      string text = " (";
      string text2 = " (";
      for (int i = 1; i < COLNAME_PatientList.Count(); i++) {
        if (i != 1) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_PatientList[i]);
        text2 += _bind(COLNAME_PatientList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(";");

      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_id], aPatientRec.pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_lastname], aPatientRec.pt_lastname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_firstname], aPatientRec.pt_firstname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.gender_id], aPatientRec.gender_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_dob], _DateTimeToObject(aPatientRec.pt_dob));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_description], aPatientRec.pt_description);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.updated_at], _DateTimeToObject(aPatientRec.pt_updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.created_at], _DateTimeToObject(aPatientRec.pt_created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    // 測定データ時の被検者ID新規登録
    public static bool InsertPatientId(NpgsqlConnection sqlConnection, string pt_id) {
      int num = 0;

      DateTime dateTime = DateTime.Now;
      PatientRec aPatientRec = new() {
        pt_id = pt_id,
        pt_lastname = string.Empty,
        pt_firstname = string.Empty,
        gender_id = Select_GenderId(sqlConnection, GENDER_TYPE[(int)GenderType.other]),
        pt_dob = null,
        pt_description = string.Empty,
        pt_updated_at = dateTime,
        pt_created_at = dateTime
      };

      StringBuilder stringBuilder = new();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.PATIENT_LIST]));
      string text = " (";
      string text2 = " (";
      for (int i = 1; i < COLNAME_PatientList.Count(); i++) {
        if (i != 1) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_PatientList[i]);
        text2 += _bind(COLNAME_PatientList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(";");

      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_id], aPatientRec.pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_lastname], aPatientRec.pt_lastname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_firstname], aPatientRec.pt_firstname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.gender_id], aPatientRec.gender_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_dob], _DateTimeToObject(aPatientRec.pt_dob));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_description], aPatientRec.pt_description);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.updated_at], _DateTimeToObject(aPatientRec.pt_updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.created_at], _DateTimeToObject(aPatientRec.pt_created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    private bool Update(NpgsqlConnection sqlConnection, PatientRec aPatientRec) {
      int num = 0;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("update ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.PATIENT_LIST]));
      stringBuilder.Append("set ");
      string text = "";
      for (int i = 1; i < (int)ePatientList.MAX; i++) {
        // コメントおよび作成日時は、アプリ上から更新されない
        if (i != (int)ePatientList.pt_description && i != (int)ePatientList.created_at) {
          text = text + _col(COLNAME_PatientList[i]) + "= " + _bind(COLNAME_PatientList[i]);
          if (i != (int)ePatientList.updated_at) {
            text += ",";
          }
        }
      }

      stringBuilder.Append(text);
      stringBuilder.Append(" where ");
      stringBuilder.Append(_col(COLNAME_PatientList[(int)ePatientList.pt_uuid]));
      stringBuilder.Append("= ");
      stringBuilder.Append(_val(aPatientRec.pt_uuid));
      stringBuilder.Append(";");
      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_id], aPatientRec.pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_lastname], aPatientRec.pt_lastname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_firstname], aPatientRec.pt_firstname);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.gender_id], aPatientRec.gender_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.pt_dob], _DateTimeToObject(aPatientRec.pt_dob));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_PatientList[(int)ePatientList.updated_at], _DateTimeToObject(DateTime.Now));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    private static bool InsertAxmPatient(NpgsqlConnection sqlConnection, AxmPatientRec aPatientRec) {
      int num = 0;

      StringBuilder stringBuilder = new();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_PATIENT_LIST]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_AxmPatientList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_AxmPatientList[i]);
        text2 += _bind(COLNAME_AxmPatientList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_axm_patient_list"));
      stringBuilder.Append(_doupdateexam(COLNAME_AxmPatientList[(int)eAxmPatientList.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatevalue(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_flag], aPatientRec.axm_flag.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_AxmPatientList[(int)eAxmPatientList.is_axm_same_pt_id], aPatientRec.is_axm_same_pt_id.ToString()));
      stringBuilder.Append(_doupdatevalue(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_same_pt_id], aPatientRec.axm_same_pt_id));
      stringBuilder.Append(";");

      using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection)) {
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.pt_uuid], Guid.Parse(aPatientRec.pt_uuid));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_pt_id], aPatientRec.axm_pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_flag], aPatientRec.axm_flag);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.is_axm_same_pt_id], aPatientRec.is_axm_same_pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_same_pt_id], aPatientRec.axm_same_pt_id);
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.updated_at], _DateTimeToObject(aPatientRec.updated_at));
        npgsqlCommand.Parameters.AddWithValue(COLNAME_AxmPatientList[(int)eAxmPatientList.created_at], _DateTimeToObject(aPatientRec.created_at));
        num = npgsqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    // axm_pt_idの最大値取得
    public static int SelectMaxAxmPatientId(NpgsqlConnection sqlConnection) {
      int result = -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("select ");
      stringBuilder.Append(_maxcol(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_pt_id]));
      stringBuilder.Append("from ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_PATIENT_LIST]));
      stringBuilder.Append(";");
      using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection);
      using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
      while (npgsqlDataReader.Read()) {
        result = _objectToInt(npgsqlDataReader[0]);
      }

      return result != 0 ? result + 1 : 1;
    }

    public static int Select_AxmPatientID_by_PK(NpgsqlConnection sqlConnection, string pt_uuid) {
      int result = -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("select ");
      stringBuilder.Append(_col(COLNAME_AxmPatientList[(int)eAxmPatientList.axm_pt_id]));
      stringBuilder.Append("from ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_PATIENT_LIST]));
      stringBuilder.Append("where ");
      stringBuilder.Append(_col(COLNAME_AxmPatientList[(int)eAxmPatientList.pt_uuid]));
      stringBuilder.Append("= ");
      stringBuilder.Append(_val(pt_uuid));
      stringBuilder.Append(";");
      using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection);
      using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
      while (npgsqlDataReader.Read()) {
        result = _objectToInt(npgsqlDataReader[0]);
      }

      return result;
    }

    public static string[] GENDER_TYPE = ["none", "male", "female", "other"];

    public enum GenderType {
      none = 0,
      male,
      female,
      other,
    }

    public static string[] COLNAME_SearchPatientList = {
      COLNAME_PatientList[(int)ePatientList.pt_uuid],
      COLNAME_PatientList[(int)ePatientList.pt_id],
      COLNAME_PatientList[(int)ePatientList.pt_lastname],
      COLNAME_PatientList[(int)ePatientList.pt_firstname],
      COLNAME_PatientList[(int)ePatientList.gender_id],
      COLNAME_PatientList[(int)ePatientList.pt_dob],
      COLNAME_AxmPatientList[(int)eAxmPatientList.axm_flag],
      COLNAME_AxmPatientList[(int)eAxmPatientList.axm_same_pt_id],
      COLNAME_ExamList[(int)eExamList.measured_at],
      COLNAME_AxmCommentList[(int)eAxmComment.description],
      COLNAME_ExamList[(int)eExamList.examtype_id]
    };
  }
}

public class PatientRec {
  public string pt_uuid { get; set; } = "";
  public string pt_id { get; set; } = "";
  public string pt_lastname { get; set; } = "";
  public string pt_firstname { get; set; } = "";
  public int gender_id { get; set; }
  public DateTime? pt_dob { get; set; }
  public string pt_description { get; set; } = "";
  public DateTime? pt_updated_at { get; set; }
  public DateTime? pt_created_at { get; set; }
}

public class AxmPatientRec {
  public string pt_uuid { get; set; } = "";
  public int axm_pt_id { get; set; }
  public bool axm_flag { get; set; }
  public bool is_axm_same_pt_id { get; set; }
  public string axm_same_pt_id { get; set; } = "";
  public DateTime? updated_at { get; set; }
  public DateTime? created_at { get; set; }
}

public enum eSearchPatientList {
  pt_uuid = 0,
  pt_id,
  pt_lastname,
  pt_firstname,
  gender_id,
  pt_dob,
  axm_flag,
  axm_same_pt_id,
  exam_datetime,
  description,
  examtype_id,
  MAX
}
