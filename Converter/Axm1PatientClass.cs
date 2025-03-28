namespace AxialManagerS_Converter {
  public class Axm1PatientClass {

    public static string[] COLNAME_Axm1PatientInfoList = {
      "ID", "FirstName", "LastName", "Birth", "Sex", "Update_Date"
    };

    public enum eAxm1PatientInfoTable {
      id = 0,
      firstName,
      lastName,
      birth,
      sex,
      updateDate,
      MAX
    }

    public static string[] COLNAME_Axm1AxialList = {
      "ID", "ST_DT", "Axial_OD", "Axial_OS", "Manual"
    };

    public enum eAxm1AxialTable {
      id = 0,
      stDt,
      axialOd,
      axialOs,
      manual,
      MAX
    }

    public static string[] COLNAME_Axm1RefList = {
      "ID", "ST_DT", "Sph_OD", "Cyl_OD", "Axis_OD", "Sph_OS", "Cyl_OS", "Axis_OS"
    };

    public enum eAxm1RefTable {
      id = 0,
      stDt,
      sphOd,
      cylOd,
      axisOd,
      sphOs,
      cylOs,
      axisOs,
      MAX
    }

    public static string[] COLNAME_Axm1RefObjList = {
      "ID", "ST_DT", "Sph_OD", "Cyl_OD", "Axis_OD", "Sph_OS", "Cyl_OS", "Axis_OS", "Manual"
    };

    public enum eAxm1RefObjTable {
      id = 0,
      stDt,
      sphOd,
      cylOd,
      axisOd,
      sphOs,
      cylOs,
      axisOs,
      manual,
      MAX
    }

    public static string[] COLNAME_Axm1KeratoList = {
      "ID", "ST_DT", "K1_OD", "K2_OD", "Cyl_OD", "K1_OS", "K2_OS", "Cyl_OS", "Manual"
    };

    public enum eAxm1KeratoTable {
      id = 0,
      stDt,
      k1Od,
      k2Od,
      cylOd,
      k1Os,
      k2Os,
      cylOs,
      manual,
      MAX
    }

    public static string[] COLNAME_Axm1PachyList = {
      "ID", "ST_DT", "Pachy_OD", "Pachy_OS", "Manual"
    };

    public enum eAxm1PachyTable {
      id = 0,
      stDt,
      pachyOd,
      pachyOs,
      manual,
      MAX
    }

    public static string[] COLNAME_Axm1MedicalTreatmentList = {
      "ID", "MEDICAL", "START_DATE", "END_DATE"
    };

    public enum eAxm1MedicalTreatmentTable {
      id = 0,
      Medical,
      startDate,
      endDate,
      MAX
    }

    public static string[] COLNAME_Axm1MedicalSetupList = {
      "MEDICAL", "NAME", "COLOR_R", "COLOR_G", "COLOR_B"
    };

    public enum eAxm1MedicalSetupTable {
      medical = 0,
      name,
      colorR,
      colorG,
      colorB,
      MAX
    }

    public static string[] Axm1DB_TableNames = {
        "", "PatientInfoTable", "AxialTable", "AxialContactTable", "AxialContact2Table", "AxialOptLengthTable"
        , "KeratoTable", "KeratoOATable", "KeratoTypTable", "KeratoOATypTable"
        , "Kerato25Table", "KeratoOA25Table", "Kerato25TypTable", "KeratoOA25TypTable"
        , "Kerato20Table", "KeratoOA20Table", "Kerato20TypTable", "KeratoOA20TypTable"
        , "RefObjTable", "RefObjTypTable", "RefTable", "PachyTable", "PachyMRTable"
        , "MedicalSetupTable", "MedicalTreatmentTable"
    };

    public enum eAxm1DbTable {
      none = 0,
      patientInfoTable,
      axialTable,
      axialContactTable,
      axialContact2Table,
      axialOptLengthTable,
      keratoTable,
      keratoOATable,
      keratoTypTable,
      keratoOATypTable,
      kerato25Table,
      keratoOA25Table,
      kerato25TypTable,
      keratoOA25TypTable,
      kerato20Table,
      keratoOA20Table,
      kerato20TypTable,
      keratoOA20TypTable,
      refObjTable,
      refObjTypTable,
      refTable,
      pachyTable,
      pachyMRTable,
      medicalSetupTable,
      medicalTreatmentTable,
      MAX
    }

  }
}
