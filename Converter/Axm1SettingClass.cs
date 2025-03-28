namespace AxialManagerS_Converter.Converter {
  public class Axm1SettingClass {

    public static string[] COLNAME_Axm1SettingLIST = {
      "Axial_Convert_Type",
      "Ref_Value_Type",
      "Krt_Device_Type",
      "Krt_Value_Type",
      "Krt_Phi_Type",
      "Pachy_Device_Type",
      "Ref_Trend_Type",
      "Medical_Count",
      "Medical_Setting_1",
      "Medical_Setting_2",
      "Medical_Setting_3",
      "Medical_Setting_4",
      "Medical_Setting_5",
      "Medical_Setting_6",
      "Medical_Setting_7",
      "Medical_Setting_8",
      "Medical_Setting_9",
      "Medical_Setting_10",
      "Medical_Setting_11",
      "Medical_Setting_12",
      "Medical_Setting_13",
      "Medical_Setting_14",
      "Medical_Setting_15",
      "Export_Directory",
      "File_Item_Type_1",
      "File_Item_Type_2",
      "File_Item_Type_3",
      "File_Item_Type_4",
      "File_Item_Type_5",
      "File_Item_Type_6",
      "File_Item_Type_7",
      "File_Item_Type_8",
      "File_Item_Type_9",
      "File_Item_Type_10",
      "File_Separate_Type_1",
      "File_Separate_Type_2",
      "File_Separate_Type_3",
      "File_Separate_Type_4",
      "File_Separate_Type_5",
      "File_Separate_Type_6",
      "File_Separate_Type_7",
      "File_Separate_Type_8",
      "File_Separate_Type_9"
    };

    public enum eAxm1Setting {
      kTagSetupAxialConvertType = 0,
      kTagSetupRefValueType,
      kTagSetupKrtDeviceType,
      kTagSetupKrtValueType,
      kTagSetupKrtPhiType,
      kTagSetupPachyDeviceType,
      kTagSetupRefTrendType,
      kTagSetupMedicalCount,
      kTagSetupMedicalSetting1,
      kTagSetupMedicalSetting2,
      kTagSetupMedicalSetting3,
      kTagSetupMedicalSetting4,
      kTagSetupMedicalSetting5,
      kTagSetupMedicalSetting6,
      kTagSetupMedicalSetting7,
      kTagSetupMedicalSetting8,
      kTagSetupMedicalSetting9,
      kTagSetupMedicalSetting10,
      kTagSetupMedicalSetting11,
      kTagSetupMedicalSetting12,
      kTagSetupMedicalSetting13,
      kTagSetupMedicalSetting14,
      kTagSetupMedicalSetting15,
      kTagSetupExportDirectory,
      kTagSetupExportFileItemType1,
      kTagSetupExportFileItemType2,
      kTagSetupExportFileItemType3,
      kTagSetupExportFileItemType4,
      kTagSetupExportFileItemType5,
      kTagSetupExportFileItemType6,
      kTagSetupExportFileItemType7,
      kTagSetupExportFileItemType8,
      kTagSetupExportFileItemType9,
      kTagSetupExportFileItemType10,
      kTagSetupExportFileSeparateType1,
      kTagSetupExportFileSeparateType2,
      kTagSetupExportFileSeparateType3,
      kTagSetupExportFileSeparateType4,
      kTagSetupExportFileSeparateType5,
      kTagSetupExportFileSeparateType6,
      kTagSetupExportFileSeparateType7,
      kTagSetupExportFileSeparateType8,
      kTagSetupExportFileSeparateType9,
      MAX
    }

    // AXIAL変換方式(AXM1での定義)
    public enum SelectConvertType {
      kConvertTypeContact = 0,
      kConvertTypeContact2,
      kConvertTypeImmersion,
      kConvertTypeOptLength,
      MAX
    }

    // 選択値の取得方法(AXM1での定義)
    public enum SelectValueType {
      kValueTypeTypical = 0,
      kValueTypeAverage,
      MAX
    }

    // 装置情報(AXM1での定義)
    public enum SelectDeviceType {
      kDeviceTypeOA = 0,
      kDeviceTypeMR,
      MAX
    }

    // 測定位置情報(AXM1での定義)
    public enum SelectPhiType {
      kPhiType3 = 0,
      kPhiType2_5,
      kPhiType2,
      MAX
    }

    public enum SelectRefTrendType {
      kRefTrendTypeRef = 0,
      kRefTrendTypeRefObj,
      MAX
    }

    public enum ExportFileItemType {
      eExportFileItem_NONE = 0,
      eExportFileItem_LASTNAME, //1
      eExportFileItem_FIRSTNAME, //2
      eExportFileItem_PT_ID, //3
      eExportFileItem_EXAM_DATE, //4
      eExportFileItem_EXAM_TIME, //5
      eExportFileItem_EYE_RL, //6
      eExportFileItem_EYE_ODOS, //7
      eExportFileItem_MODEL_NAME_OA, //8
      eExportFileItem_MODEL_NAME_ALMANAGER, //9
      eExportFileItem_EXTENSION_STRING, //10
      eExportFileItem_EXTENSION_NUMBER, //11
      MAX
    }

    public static string[] D_EXPORT_FILE_ITEM_LIST = {
      "(None)",
      "Last Name",
      "First Name",
      "Patient ID",
      "Date",
      "Time",
      "Eye(L/R/B)",
      "Eye(OS/OD/OU)",
      "Model Name(OA-2000)",
      "Model Name(ALManager)",
      "Extension String",
      "Extension Number"
    };

    public enum ExportFileSeparateType {
      eExportFileSeparate_ALLNONE = 0,
      eExportFileSeparate_NONE,
      eExportFileSeparate_SPACE,
      eExportFileSeparate_3,
      eExportFileSeparate_4,
      eExportFileSeparate_5,
      eExportFileSeparate_6,
      eExportFileSeparate_7,
      eExportFileSeparate_8,
      eExportFileSeparate_9,
      eExportFileSeparate_10,
      eExportFileSeparate_11,
      eExportFileSeparate_12,
      eExportFileSeparate_13,
      eExportFileSeparate_14,
      eExportFileSeparate_15,
      eExportFileSeparate_16,
      eExportFileSeparate_17,
      eExportFileSeparate_18,
      eExportFileSeparate_19,
      MAX
    }

    public static string[] D_EXPORT_FILE_SEPARATE_LIST = {
      "(All None)",
      "(none)",
      "(space)",
      "_", //3
      "-", //4
      "+", //5
      ",", //6
      "(", //7
      ")", //8
      "[", //9
      "]", //10
      "!", //11
      "#", //12
      "$", //13
      "%", //14
      "&", //15
      "'", //16
      "=", //17
      "~", //18
      "@", //19
    };

    public class RGBColor {
      int R { get; set; } = 255;
      int G { get; set; } = 255;
      int B { get; set; } = 255;
    }

    public class MedicalSetting {
      string MedicalID { get; set; } = string.Empty;
      string MedicalName { get; set; } = string.Empty;
      RGBColor MedicalColor { get; set; } = new RGBColor();
    }

    public class JsonSettingData {
      public SelectConvertType Axial_Converter_Type { get; set; } = default;
      public SelectValueType Ref_Value_Type { get; set; } = default;
      public SelectDeviceType Krt_Device_Type { get; set; } = default;
      public SelectValueType Krt_Value_Type { get; set; } = default;
      public SelectPhiType Krt_Phi_Type { get; set; } = default;
      public SelectDeviceType Pachy_Device_Type { get; set; } = default;
      public SelectRefTrendType Ref_Trend_Type { get; set; } = default;
      public string Export_Directory { get; set; } = string.Empty;
      // 治療方法は、設定ファイルではなく、DBから取得するので不要
      //public int Medical_Count { get; set; } = 0;
      //public MedicalSetting Medical_Setting_1 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_2 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_3 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_4 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_5 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_6 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_7 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_8 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_9 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_10 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_11 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_12 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_13 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_14 { get; set; } = new MedicalSetting();
      //public MedicalSetting Medical_Setting_15 { get; set; } = new MedicalSetting();
      public ExportFileItemType File_Item_Type_1 { get; set; } = default;
      public ExportFileItemType File_Item_Type_2 { get; set; } = default;
      public ExportFileItemType File_Item_Type_3 { get; set; } = default;
      public ExportFileItemType File_Item_Type_4 { get; set; } = default;
      public ExportFileItemType File_Item_Type_5 { get; set; } = default;
      public ExportFileItemType File_Item_Type_6 { get; set; } = default;
      public ExportFileItemType File_Item_Type_7 { get; set; } = default;
      public ExportFileItemType File_Item_Type_8 { get; set; } = default;
      public ExportFileItemType File_Item_Type_9 { get; set; } = default;
      public ExportFileItemType File_Item_Type_10 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_1 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_2 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_3 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_4 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_5 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_6 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_7 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_8 { get; set; } = default;
      public ExportFileSeparateType File_Separate_Type_9 { get; set; } = default;
    }

  }
}
