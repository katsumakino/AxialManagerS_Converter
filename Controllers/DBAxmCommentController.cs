using Npgsql;
using System.Data;
using System.Text;
using static AxialManagerS_Converter.Controllers.DBCommonController;
using static AxialManagerS_Converter.Controllers.DBTreatmentController;
using AxialManagerS_Converter.Common;

namespace AxialManagerS_Converter.Controllers {

  public class DBAxmCommentController {

    // コメント登録
    public int SetAxmComment(AxmCommentRequest conditions) {
      bool result = false;

      try {
        if (conditions == null) return 1;
        if (conditions.PatientID == null || conditions.PatientID == string.Empty) return 1;
        if (conditions.AxmComment.Description == null) return 1;

        DBAccess dbAccess = DBAccess.GetInstance();

        try {
          // PostgreSQL Server 通信接続
          NpgsqlConnection sqlConnection = dbAccess.GetSqlConnection();

          // クエリコマンド実行

          // UUIDの有無を確認(true:update / false:insert)
          var uuid = Select_PTUUID_by_PTID(sqlConnection, conditions.PatientID);
          if (uuid == string.Empty) {
            // コメント登録時は、必ず患者データが存在する
            return 1;
          }

          // コメントデータID取得
          var commenttype_id = Select_AxmCommentTypeId(sqlConnection, AXM_COMMENT_TYPE[(int)conditions.AxmComment.CommentType]);
          var comment_id = Select_AxmCommentID_by_PK(sqlConnection, uuid, (DateTime)conditions.AxmComment.ExamDateTime
            , conditions.AxmComment.CommentType, commenttype_id);
          if (comment_id == -1) {
            comment_id = SelectMaxCommentId(sqlConnection);
          }

          // 更新日、作成日は揃える
          var dateNow = DateTime.Now;

          // DB登録
          result = InsertAxmComment(new AxmCommentRec {
            comment_id = comment_id,
            commenttype_id = commenttype_id,
            pt_uuid = uuid,
            description = conditions.AxmComment.Description ?? string.Empty,
            measured_at = conditions.AxmComment.CommentType == AxmCommentType.ExamDate ? conditions.AxmComment.ExamDateTime : null,
            created_at = dateNow,
            updated_at = dateNow
          }, sqlConnection);

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

      return (result)? 0 : 1;
    }

    public static bool InsertAxmComment(AxmCommentRec rec, NpgsqlConnection sqlConnection) {
      // SQLコマンド
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("insert into ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_COMMENT]));
      string text = " (";
      string text2 = " (";
      for (int i = 0; i < COLNAME_AxmCommentList.Count(); i++) {
        if (i != 0) {
          text += ",";
          text2 += ",";
        }

        text += _col(COLNAME_AxmCommentList[i]);
        text2 += _bind(COLNAME_AxmCommentList[i]);
      }

      text += ")";
      text2 += ")";
      stringBuilder.Append(text);
      stringBuilder.Append(" values ");
      stringBuilder.Append(text2);
      stringBuilder.Append(_onconflict("pk_axm_comment"));
      stringBuilder.Append(_doupdateexam(COLNAME_AxmCommentList[(int)eAxmComment.updated_at], DateTime.Now));
      stringBuilder.Append(_doupdatevalue(COLNAME_AxmCommentList[(int)eAxmComment.description], rec.description));
      stringBuilder.Append(";");
      int num = 0;
      // SQLコマンド実行
      using (NpgsqlCommand sqlCommand = new(stringBuilder.ToString(), sqlConnection)) {
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.comment_id], rec.comment_id);
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.commenttype_id], rec.commenttype_id);
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.pt_uuid], Guid.Parse(rec.pt_uuid));
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.description], rec.description);
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.measured_at], _DateTimeToObject(rec.measured_at));
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.created_at], _DateTimeToObject(rec.created_at));
        sqlCommand.Parameters.AddWithValue(COLNAME_AxmCommentList[(int)eAxmComment.updated_at], _DateTimeToObject(rec.updated_at));
        num = sqlCommand.ExecuteNonQuery();
      }

      return num != 0;
    }

    public static int Select_AxmCommentID_by_PK(NpgsqlConnection sqlConnection, string pt_uuid, DateTime measured_at, AxmCommentType type, int commenttype_id) {
      int result = -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("select ");
      stringBuilder.Append(_col(COLNAME_AxmCommentList[(int)eAxmComment.comment_id]));
      stringBuilder.Append("from ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_COMMENT]));
      stringBuilder.Append("where ");
      stringBuilder.Append(_col(COLNAME_AxmCommentList[(int)eAxmComment.pt_uuid]));
      stringBuilder.Append("= ");
      stringBuilder.Append(_val(pt_uuid));
      stringBuilder.Append(" and ");
      stringBuilder.Append(_col(COLNAME_AxmCommentList[(int)eAxmComment.commenttype_id]));
      stringBuilder.Append("= ");
      stringBuilder.Append(_val(commenttype_id.ToString()));
      if (type == AxmCommentType.ExamDate) {
        stringBuilder.Append(" and ");
        stringBuilder.Append(_col(COLNAME_AxmCommentList[(int)eAxmComment.measured_at]));
        stringBuilder.Append("= ");
        stringBuilder.Append(_val(measured_at.ToString("yyyy-MM-dd HH:mm:ss")));
      }
      stringBuilder.Append(";");
      using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection);
      using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
      while (npgsqlDataReader.Read()) {
        result = _objectToInt(npgsqlDataReader[0]);
      }

      return result;
    }

    // comment_idの最大値取得
    public static int SelectMaxCommentId(NpgsqlConnection sqlConnection) {
      int result = -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("select ");
      stringBuilder.Append(_maxcol(COLNAME_AxmCommentList[(int)eAxmComment.comment_id]));
      stringBuilder.Append("from ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.AXM_COMMENT]));
      stringBuilder.Append(";");
      using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection);
      using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
      while (npgsqlDataReader.Read()) {
        result = _objectToInt(npgsqlDataReader[0]);
      }

      return result != 0 ? result + 1 : 1;
    }

    public static int Select_AxmCommentTypeId(NpgsqlConnection sqlConnection, string commenttype) {
      int result = -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("select ");
      stringBuilder.Append(_col(COLNAME_MstAxmCommentTypesList[0]));
      stringBuilder.Append("from ");
      stringBuilder.Append(_table(DB_TableNames[(int)eDbTable.MST_AXMCOMMENTTYPES]));
      stringBuilder.Append("where ");
      stringBuilder.Append(_col(COLNAME_MstAxmCommentTypesList[1]));
      stringBuilder.Append("= ");
      stringBuilder.Append(_val(commenttype));
      stringBuilder.Append(";");

      using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(stringBuilder.ToString(), sqlConnection);
      using NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
      while (npgsqlDataReader.Read()) {
        result = _objectToInt(npgsqlDataReader[0]);
      }

      return result;
    }
        
    public static string[] COLNAME_AxmCommentList = new string[(int)eAxmComment.MAX] {
            "comment_id", "commenttype_id", "pt_uuid", "description", "measured_at", "updated_at", "created_at"
        };

    public static string[] AXM_COMMENT_TYPE = ["none", "Patient", "ExamDate"];

    public enum eAxmCommentType {
      none = 0,
      Patient,
      ExamDate
    }

    public enum eAxmComment {
      comment_id = 0,
      commenttype_id,
      pt_uuid,
      description,
      measured_at,
      updated_at,
      created_at,
      MAX
    }

  }

  public class AxmCommentRec {
    public int comment_id { get; set; }
    public int commenttype_id { get; set; }
    public string pt_uuid { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public DateTime? measured_at { get; set; }
    public DateTime? updated_at { get; set; }

    public DateTime? created_at { get; set; }
  }
}
