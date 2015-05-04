using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebToolsBox;
using WebToolsBox.EntityFramework;
using System.Data.Objects;

namespace WebToolsBox
{
    /// <summary>
    /// 针对Q0001问题的临时方案的数据结构
    /// </summary>
    class commandItem
    {
        public string FLAG { get; set; }
        public int STEP { get; set; }        
    }

    /// <summary>
    /// 针对Q0001问题的临时方案的数据结构
    /// </summary>
    class printmodulemember
    {
        public int GroupId { get; set; }
        public int Module { get; set; }
        public string ReturnCode { get; set; }
    }

    /// <summary>
    /// 系统级参数定义
    /// </summary>
    public static class ParameterClass
    {
        /// <summary>
        /// print_module_group 表生成最小值索引，用来避免与分公司添加模版冲突。
        /// </summary>
        public static int PRINTGROUPMIN = 1500;

        /// <summary>
        /// tbl_print_module 表生成最小值索引，用来避免与分公司添加模版冲突。
        /// </summary>
        public static int PRINTMODULEMIN = 1500;
    }

    public class CalCode
    {
        KeyValuePair<int, string> parseFunctionInfo(int func_index, ref QueryEntity qEntity)
        {
            //值为0，不处理
            if (func_index == 0)
            {
                return ErrorCode.Instance.GetStatement(0000, new List<string> { "0" });
            }

            //if ((from item in session.Instance.TPOSDB.FUNCTION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.FUNC_INDEX == func_index select item).Count() == 0)
            if ((from item in session.Instance.LocalDataHandle.FUNCTION_INFO where item.DELETE_FLAG == "0" && (int)item.FUNC_INDEX == func_index select item).Count() == 0)
            {
                //DB上不存在该数据，直接不处理
                return ErrorCode.Instance.GetStatement(9003, new List<string> { func_index.ToString() });
            }

            //PRDTPOSDB.FUNCTION_INFORow fiRow = (from item in session.Instance.TPOSDB.FUNCTION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.FUNC_INDEX == func_index select item).ToList()[0];
            EntityFramework.DataLocalEntities.FUNCTION_INFO fiRow = (from item in session.Instance.LocalDataHandle.FUNCTION_INFO where item.DELETE_FLAG == "0" && (int)item.FUNC_INDEX == func_index select item).ToList()[0];

            //if ((from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.FUNC_INDEX == fiRow.FUNC_INDEX && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.FUNCTION_INFO where item.DELETE_FLAG == "0" && item.FUNC_INDEX == fiRow.FUNC_INDEX && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select item).Count() > 0)
            {
                //联机数据库中已经包含该条目，不处理
                return ErrorCode.Instance.GetStatement(0000, new List<string> { fiRow.FUNC_INDEX.ToString() });
            }

            //if ((from item in qEntity.FunctionInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == func_index.ToString() select item).Count() > 0)
            if ((from item in qEntity.FunctionInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == func_index.ToString() select item).Count() > 0)
            {
                //条目已经处理，不再重复处理

                return ErrorCode.Instance.GetStatement(0000, new List<string> { (from item in qEntity.FunctionInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == func_index.ToString() select item).ToList()[0].Split('F')[0].Substring(1) });
            }

            //if ((from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.FUNCTION_INFO where item.DELETE_FLAG == "0" && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select item).Count() > 0)
            {
                //生产数据库中已经包含该条目，只是索引不同
                //int new_fi = (from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select (int)item.FUNC_INDEX).ToList()[0];
                int new_fi = (from item in session.Instance.OnlineDataHandle.FUNCTION_INFO where item.DELETE_FLAG == "0" && item.INFO1 == fiRow.INFO1 && item.INFO2 == fiRow.INFO2 && item.INFO3 == fiRow.INFO3 select (int)item.FUNC_INDEX).ToList()[0];

                qEntity.FunctionInfoAddRecord.Add("Z" + new_fi.ToString() + "F" + func_index.ToString());
                return ErrorCode.Instance.GetStatement(0000, new List<string> { new_fi.ToString() });
            }

            //需要添加新的function_info
            //List<int> unavailiable_fi = (from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() select (int)item.FUNC_INDEX).Distinct().ToList();
            List<int> unavailiable_fi = (from item in session.Instance.OnlineDataHandle.FUNCTION_INFO select (int)item.FUNC_INDEX).Distinct().ToList();

            foreach (string s in qEntity.FunctionInfoAddRecord)
            {
                unavailiable_fi.Add(int.Parse(s.Split('F')[0].Substring(1)));
            }

            unavailiable_fi.Sort();

            for (int k = 0; k < unavailiable_fi.Count - 1; k++)
            {
                if (Math.Abs(unavailiable_fi[k + 1] - unavailiable_fi[k]) > 1)
                {
                    int new_fi = unavailiable_fi[k] + 1;

                    qEntity.FunctionInfoAddRecord.Add("Z" + new_fi + "F" + func_index.ToString());
                    qEntity.FunctionInfoQuery.Add(TransQuery.FunctionInfoQuery(new_fi.ToString(), fiRow.OP_FLAG, fiRow.MODULE_NUM.ToString(), fiRow.INFO1_FORMAT, fiRow.INFO1, fiRow.INFO2_FORMAT, fiRow.INFO2, fiRow.INFO3_FORMAT, fiRow.INFO3));

                    return ErrorCode.Instance.GetStatement(0000, new List<string> { new_fi.ToString() });
                }
            }

            return ErrorCode.Instance.GetStatement(0000, new List<string> { "0" });


            #region 备份代码
            //string new_function_info = transDefRow.FUNCTION_INDEX.ToString();
            //PRDTPOSDB.FUNCTION_INFORow function_info = (from item in session.Instance.TPOSDB.FUNCTION_INFOTableAdapter.GetData() where item.FUNC_INDEX == transDefRow.FUNCTION_INDEX select item).ToList()[0];

            //string function_info_string = function_info.INFO1;
            //bool isAdd = false;

            //var exist_function_info = (from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() where item.INFO1 == function_info_string && (item.DELETE_FLAG == "0") select item);
            //if (exist_function_info.Count() != 0)
            //{
            //    new_function_info = exist_function_info.ToList()[0].FUNC_INDEX.ToString();
            //    qEntity.FunctionInfoAddRecord.Add("I" + new_function_info);
            //}
            //else
            //{
            //    new_function_info = ((from item in session.Instance.ONLINEDB.FUNCTION_INFOTableAdapter.GetData() select item.FUNC_INDEX).Max() + 1).ToString();
            //    isAdd = true;
            //    qEntity.FunctionInfoAddRecord.Add("A" + new_function_info);
            //}

            //if (isAdd)
            //{
            //    //添加function_info语句
            //    qEntity.FunctionInfoQuery.Add(TransQuery.FunctionInfoQuery(new_function_info, function_info.OP_FLAG, function_info.MODULE_NUM.ToString(), function_info.INFO1_FORMAT, function_info.INFO1, function_info.INFO2_FORMAT, function_info.INFO2, function_info.INFO3_FORMAT, function_info.INFO3));
            //}
            #endregion
        }

        void parseTransDef(string Origin_trans_type, string New_trans_type, ref QueryEntity qEntity)
        {

        }

        void parseFieldMessage(int Origin_trans_type, int New_trans_type, ref QueryEntity qEntity)
        {
            //PRDTPOSDB.FIELD_MESSAGERow fieldMessageRow = (from item in session.Instance.TPOSDB.FIELD_MESSAGETableAdapter.GetData() where (item.TPOS_CODE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0") select item).ToList()[0];
            EntityFramework.DataLocalEntities.FIELD_MESSAGE fieldMessageRow = (from item in session.Instance.LocalDataHandle.FIELD_MESSAGE where (item.TPOS_CODE == Origin_trans_type && item.DELETE_FLAG == "0") select item).ToList()[0];

            //qEntity.FieldMessageQuery.Add(TransQuery.FieldMessageQuery(New_trans_type, fieldMessageRow.FBSKD, fieldMessageRow.FIELD1, fieldMessageRow.FIELD3, fieldMessageRow.FIELD22, fieldMessageRow.FIELD25, fieldMessageRow.FIELD54, fieldMessageRow.FIELD48_REQ, fieldMessageRow.FIELD48_RESP, fieldMessageRow.FLAG.ToString(), fieldMessageRow.TRN_TYPE, fieldMessageRow.TRN_NAME, fieldMessageRow.MSG_TYPE.ToString(), fieldMessageRow.MSG48_TYPE, fieldMessageRow.APP_ID.ToString(), fieldMessageRow.RESV3, fieldMessageRow.SUBFIELD, fieldMessageRow.FIELD4, fieldMessageRow.FIELD60, fieldMessageRow.DISPINFO, fieldMessageRow.ZFINFO, fieldMessageRow.FILL_FBSKD, fieldMessageRow.MSG_SPEC_VER.ToString(), fieldMessageRow.FIELD48_REQ_PREFIX, fieldMessageRow.FIELD48_REQ_SUFFIX, fieldMessageRow.FIELD48_RESP_PREFIX, fieldMessageRow.FIELD48_RESP_SUFFIX, fieldMessageRow.FIELD63_REQ, fieldMessageRow.FIELD63_RESP, fieldMessageRow.FIELD57_REQ, fieldMessageRow.FIELD57_RESP, fieldMessageRow.FIELD62_REQ, fieldMessageRow.FIELD62_RESP, fieldMessageRow.FIELD2, fieldMessageRow.REQ_MIN_BITMAP, fieldMessageRow.REQ_MAX_BITMAP, fieldMessageRow.RES_MIN_BITMAP, fieldMessageRow.RES_MAX_BITMAP, fieldMessageRow.PRONAME, fieldMessageRow.PRODETAIL, fieldMessageRow.BACK, fieldMessageRow.NEED_DIGITAL_SIGN, fieldMessageRow.TIDCNV_FLAG, fieldMessageRow.CNV_NO.ToString()));
            qEntity.FieldMessageQuery.Add(TransQuery.FieldMessageQuery(New_trans_type.ToString(), fieldMessageRow.FBSKD, fieldMessageRow.FIELD1, fieldMessageRow.FIELD3, fieldMessageRow.FIELD22, fieldMessageRow.FIELD25, fieldMessageRow.FIELD54, fieldMessageRow.FIELD48_REQ, fieldMessageRow.FIELD48_RESP, fieldMessageRow.FLAG.ToString(), fieldMessageRow.TRN_TYPE, fieldMessageRow.TRN_NAME, fieldMessageRow.MSG_TYPE.ToString(), fieldMessageRow.MSG48_TYPE, fieldMessageRow.APP_ID.ToString(), fieldMessageRow.RESV3, fieldMessageRow.SUBFIELD, fieldMessageRow.FIELD4, fieldMessageRow.FIELD60, fieldMessageRow.DISPINFO, fieldMessageRow.ZFINFO, fieldMessageRow.FILL_FBSKD, fieldMessageRow.MSG_SPEC_VER.ToString(), fieldMessageRow.FIELD48_REQ_PREFIX, fieldMessageRow.FIELD48_REQ_SUFFIX, fieldMessageRow.FIELD48_RESP_PREFIX, fieldMessageRow.FIELD48_RESP_SUFFIX, fieldMessageRow.FIELD63_REQ, fieldMessageRow.FIELD63_RESP, fieldMessageRow.FIELD57_REQ, fieldMessageRow.FIELD57_RESP, fieldMessageRow.FIELD62_REQ, fieldMessageRow.FIELD62_RESP, fieldMessageRow.FIELD2, fieldMessageRow.REQ_MIN_BITMAP, fieldMessageRow.REQ_MAX_BITMAP, fieldMessageRow.RES_MIN_BITMAP, fieldMessageRow.RES_MAX_BITMAP, fieldMessageRow.PRONAME, fieldMessageRow.PRODETAIL, fieldMessageRow.BACK, fieldMessageRow.NEED_DIGITAL_SIGN, fieldMessageRow.TIDCNV_FLAG, fieldMessageRow.CNV_NO.ToString()));
            qEntity.FieldMessageAddRecord.Add("Z" + New_trans_type + "F" + Origin_trans_type);
        }

        void parseTransSplit(int Origin_trans_type, int New_trans_type, List<int> Origin_trans_type_list, List<int> New_trans_type_list, ref QueryEntity qEntity)
        {
            //if ((from item in session.Instance.TPOSDB.TRANS_SPLITTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() != 0)
            if ((from item in session.Instance.LocalDataHandle.TRANS_SPLIT where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() != 0)
            {
                //PRDTPOSDB.TRANS_SPLITRow transSplitRow = (from item in session.Instance.TPOSDB.TRANS_SPLITTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];

                EntityFramework.DataLocalEntities.TRANS_SPLIT transSplitRow = (from item in session.Instance.LocalDataHandle.TRANS_SPLIT where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];

                if (transSplitRow.COND_NEXT_TRANS_TYPES != null && transSplitRow.COND_NEXT_TRANS_TYPES != string.Empty)
                {
                    for (int j = 0; j < Origin_trans_type_list.Count(); j++)
                    {
                        if (transSplitRow.COND_NEXT_TRANS_TYPES.Contains(Origin_trans_type_list[j].ToString()))
                        {
                            transSplitRow.COND_NEXT_TRANS_TYPES = transSplitRow.COND_NEXT_TRANS_TYPES.Replace(Origin_trans_type_list[j].ToString(), New_trans_type_list[j].ToString());
                        }
                    }
                }

                qEntity.TransSplitQuery.Add(TransQuery.TransSplitQuery(New_trans_type, transSplitRow.COND_NEXT_TRANS_TYPES, transSplitRow.FLAG, "null"));

                qEntity.TransSplitAddRecord.Add("Z" + New_trans_type + "F" + Origin_trans_type);
            }
        }

        //void parseTransCommands(string new_trans_type, List<string> AddRecord, PRDTPOSDB.TRANS_COMMANDSRow commandsRow)
        void parseTransCommands(int new_trans_type, List<string> AddRecord, EntityFramework.DataLocalEntities.TRANS_COMMANDS commandsRow, ref QueryEntity qEntity)
        {
            if (AddRecord == null) AddRecord = new List<string>();

            string func_index = commandsRow.FUNC_INDEX.ToString();
            string data_index = commandsRow.DATA_INDEX.ToString();
            string new_func_index = string.Empty;
            string new_data_index = string.Empty;

            if (func_index == "0" || func_index == string.Empty)
            {
                new_func_index = "0";
            }
            //判断operation_info 
            else if ((from item in qEntity.OperationInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == func_index select item).Count() > 0)
            {
                new_func_index = (from item in qEntity.OperationInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == func_index select item).ToList()[0].Split('F')[0].Substring(1);
            }
            else
            {
                new_func_index = func_index;
            }

            if (data_index == "0" || data_index == string.Empty)
            {
                new_data_index = "0";
            }
            else if ((from item in AddRecord where item.Substring(item.LastIndexOf('F') + 1) == data_index select item).Count() > 0)
            {
                new_data_index = (from item in AddRecord where item.Substring(item.LastIndexOf('F') + 1) == data_index select item).ToList()[0].Split('F')[0].Substring(1);
            }
            else
            {
                new_data_index = data_index;
            }

            qEntity.TransCommandsQuery.Add(TransQuery.TransCommandsQuery(new_trans_type, commandsRow.STEP.ToString(), commandsRow.FLAG, commandsRow.COMMAND, new_func_index.ToString(), commandsRow.ALOG, commandsRow.COMMAND_NAME, commandsRow.ORGCOMMAND, new_data_index.ToString()));

        }

        void parseTblDispContent(int Data_index, ref QueryEntity qEntity)
        {
            //值为0，不处理
            if (Data_index == 0)
            {
                return;
            }

            //if ((from item in session.Instance.TPOSDB.TBL_DISP_CONTENTTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.DATA_INDEX == Data_index select item).Count() == 0)
            if ((from item in session.Instance.LocalDataHandle.TBL_DISP_CONTENT where item.DELETE_FLAG == "0" && (int)item.DATA_INDEX == Data_index select item).Count() == 0)
            {
                //DB上不存在该数据，直接不处理
                return;
            }

            //PRDTPOSDB.TBL_DISP_CONTENTRow tdcRow = (from item in session.Instance.TPOSDB.TBL_DISP_CONTENTTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.DATA_INDEX == Data_index select item).ToList()[0];
            EntityFramework.DataLocalEntities.TBL_DISP_CONTENT tdcRow = (from item in session.Instance.LocalDataHandle.TBL_DISP_CONTENT where item.DELETE_FLAG == "0" && (int)item.DATA_INDEX == Data_index select item).ToList()[0];

            //if ((from item in session.Instance.ONLINEDB.TBL_DISP_CONTENTTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.DATA_INDEX == tdcRow.DATA_INDEX && item.LINES == tdcRow.LINES select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.TBL_DISP_CONTENT where item.DELETE_FLAG == "0" && item.DATA_INDEX == tdcRow.DATA_INDEX && item.LINES == tdcRow.LINES select item).Count() > 0)
            {
                //联机数据库中已经包含该条目，不处理
                return;
            }

            if ((from item in qEntity.TblDispContentAddRecord where item.Substring(item.LastIndexOf('F') + 1) == Data_index.ToString() select item).Count() > 0)
            {
                //条目已经处理，不再重复处理
                return;
            }

            //if ((from item in session.Instance.ONLINEDB.TBL_DISP_CONTENTTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.LINES == tdcRow.LINES select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.TBL_DISP_CONTENT where item.DELETE_FLAG == "0" && item.LINES == tdcRow.LINES select item).Count() > 0)
            {
                //生产数据库中已经包含该条目，只是索引不同
                //int new_tdc = (from item in session.Instance.ONLINEDB.TBL_DISP_CONTENTTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.LINES == tdcRow.LINES select (int)item.DATA_INDEX).ToList()[0];
                int new_tdc = (from item in session.Instance.OnlineDataHandle.TBL_DISP_CONTENT where item.DELETE_FLAG == "0" && item.LINES == tdcRow.LINES select (int)item.DATA_INDEX).ToList()[0];

                qEntity.TblDispContentAddRecord.Add("Z" + new_tdc + "F" + tdcRow.DATA_INDEX.ToString());
                return;
            }

            //需要添加新的屏显
            //List<int> unavailiable_tdc = (from item in session.Instance.ONLINEDB.TBL_DISP_CONTENTTableAdapter.GetData() select (int)item.DATA_INDEX).Distinct().ToList();
            List<int> unavailiable_tdc = (from item in session.Instance.OnlineDataHandle.TBL_DISP_CONTENT select (int)item.DATA_INDEX).Distinct().ToList();

            foreach (string s in qEntity.TblDispContentAddRecord)
            {
                if (!unavailiable_tdc.Contains(int.Parse(s.Split('F')[0].Substring(1))))
                {
                    unavailiable_tdc.Add(int.Parse(s.Split('F')[0].Substring(1)));
                }
            }

            unavailiable_tdc.Sort();

            for (int k = 0; k < unavailiable_tdc.Count - 1; k++)
            {
                if (Math.Abs(unavailiable_tdc[k + 1] - unavailiable_tdc[k]) > 1)
                {
                    int new_tdc = unavailiable_tdc[k] + 1;

                    qEntity.TblDispContentAddRecord.Add("Z" + new_tdc + "F" + tdcRow.DATA_INDEX.ToString());
                    qEntity.DynamicValidQuery.Add(TransQuery.TblDispContentQuery(new_tdc.ToString(), tdcRow.LINES, tdcRow.TIMEOUT.ToString(), tdcRow.CTRL_BITMAP.ToString(), tdcRow.REFRESH_MODE));

                    return;
                }
            }

        }

        void parseDynamicValid(int Data_index, ref QueryEntity qEntity)
        {
            //值为0，不处理
            if (Data_index == 0)
                return;

            //if ((from item in session.Instance.TPOSDB.DYNAMIC_VALIDTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.REC_NO == Data_index select item).Count() == 0)
            if ((from item in session.Instance.LocalDataHandle.DYNAMIC_VALID where item.DELETE_FLAG == "0" && (int)item.REC_NO == Data_index select item).Count() == 0)
            {
                //DB上不存在该数据，直接不处理
                return;
            }

            //PRDTPOSDB.DYNAMIC_VALIDRow dvRow = (from item in session.Instance.TPOSDB.DYNAMIC_VALIDTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)item.REC_NO == Data_index select item).ToList()[0];
            EntityFramework.DataLocalEntities.DYNAMIC_VALID dvRow = (from item in session.Instance.LocalDataHandle.DYNAMIC_VALID where item.DELETE_FLAG == "0" && (int)item.REC_NO == Data_index select item).ToList()[0];

            //if ((from item in session.Instance.ONLINEDB.DYNAMIC_VALIDTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.REC_NO == dvRow.REC_NO && item.INPUT_NAME == dvRow.INPUT_NAME select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.DYNAMIC_VALID where item.DELETE_FLAG == "0" && item.REC_NO == dvRow.REC_NO && item.INPUT_NAME == dvRow.INPUT_NAME select item).Count() > 0)
            {
                //联机数据库中已经包含该条目，不处理
                return;
            }

            if ((from item in qEntity.OperationInfoAddRecord where item.Substring(item.LastIndexOf('F') + 1) == Data_index.ToString() select item).Count() > 0)
            {
                //条目已经处理，不再重复处理
                return;
            }

            //if ((from item in session.Instance.ONLINEDB.DYNAMIC_VALIDTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.INPUT_NAME == dvRow.INPUT_NAME && item.INPUT_TYPE == dvRow.INPUT_TYPE && item.INPUT_FLAG == dvRow.INPUT_FLAG && item.MAXLEN == dvRow.MAXLEN && item.MINLEN == dvRow.MINLEN select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.DYNAMIC_VALID where item.DELETE_FLAG == "0" && item.INPUT_NAME == dvRow.INPUT_NAME && item.INPUT_TYPE == dvRow.INPUT_TYPE && item.INPUT_FLAG == dvRow.INPUT_FLAG && item.MAXLEN == dvRow.MAXLEN && item.MINLEN == dvRow.MINLEN select item).Count() > 0)
            {
                //生产数据库中已经包含该条目，只是索引不同
                //int new_dv = (from item in session.Instance.ONLINEDB.DYNAMIC_VALIDTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.INPUT_NAME == dvRow.INPUT_NAME && item.INPUT_TYPE == dvRow.INPUT_TYPE && item.INPUT_FLAG == dvRow.INPUT_FLAG && item.MAXLEN == dvRow.MAXLEN && item.MINLEN == dvRow.MINLEN select (int)item.REC_NO).ToList()[0];

                int new_dv = (from item in session.Instance.OnlineDataHandle.DYNAMIC_VALID where item.DELETE_FLAG == "0" && item.INPUT_NAME == dvRow.INPUT_NAME && item.INPUT_TYPE == dvRow.INPUT_TYPE && item.INPUT_FLAG == dvRow.INPUT_FLAG && item.MAXLEN == dvRow.MAXLEN && item.MINLEN == dvRow.MINLEN select (int)item.REC_NO).ToList()[0];

                qEntity.DynamicValidAddRecord.Add("Z" + new_dv + "F" + dvRow.REC_NO.ToString());
                return;
            }

            //需要添加新的动态验证
            //List<int> unavailiable_dv = (from item in session.Instance.ONLINEDB.DYNAMIC_VALIDTableAdapter.GetData() select (int)item.REC_NO).Distinct().ToList();

            List<int> unavailiable_dv = (from item in session.Instance.OnlineDataHandle.DYNAMIC_VALID select (int)item.REC_NO).Distinct().ToList();

            unavailiable_dv.Sort();

            for (int k = 0; k < unavailiable_dv.Count - 1; k++)
            {
                if (Math.Abs(unavailiable_dv[k + 1] - unavailiable_dv[k]) > 1)
                {
                    int new_dv = unavailiable_dv[k] + 1;

                    qEntity.DynamicValidAddRecord.Add("Z" + new_dv + "F" + dvRow.REC_NO.ToString());
                    qEntity.DynamicValidQuery.Add(TransQuery.DynamicValidQuery(new_dv.ToString(), dvRow.MINLEN.ToString(), dvRow.MAXLEN.ToString(), dvRow.MINVAL.ToString(), dvRow.MAXVAL.ToString(), dvRow.INPUT_FLAG, dvRow.INPUT_TYPE, dvRow.RSV, dvRow.OPTNUM1.ToString(), dvRow.OPTNUM2.ToString(), dvRow.OPTCODE, dvRow.INPUT_NAME));

                    return;
                }
            }

        }

        void parseTblExchange(int Origin_trans_type, int New_trans_type, ref QueryEntity qEntity)
        {
            //if ((from item in session.Instance.TPOSDB.TBL_EXCHANGETableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() != 0)
            if ((from item in session.Instance.LocalDataHandle.TBL_EXCHANGE where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() != 0)
            {
                //PRDTPOSDB.TBL_EXCHANGERow exchangeRow = (from item in session.Instance.TPOSDB.TBL_EXCHANGETableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];
                EntityFramework.DataLocalEntities.TBL_EXCHANGE exchangeRow = (from item in session.Instance.LocalDataHandle.TBL_EXCHANGE where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];

                qEntity.TblExchangeQuery.Add(TransQuery.TblExchangeQuery(New_trans_type, exchangeRow.BEFORE, exchangeRow.AFTER));

                qEntity.TblExchangeAddRecord.Add("A" + New_trans_type);
            }
        }

        void parseAutoVoid(int Origin_trans_type, int New_trans_type, List<int> Origin_trans_type_list, List<int> New_trans_type_list, ref QueryEntity qEntity)
        {
            //if ((from item in session.Instance.TPOSDB.TRANS_AUTOTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() > 0)
            if ((from item in session.Instance.LocalDataHandle.TRANS_AUTO where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() > 0)
            {
                //PRDTPOSDB.TRANS_AUTORow transAutoRow = (from item in session.Instance.TPOSDB.TRANS_AUTOTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];
                EntityFramework.DataLocalEntities.TRANS_AUTO transAutoRow = (from item in session.Instance.LocalDataHandle.TRANS_AUTO where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];

                for (int j = 0; j < Origin_trans_type_list.Count(); j++)
                {
                    if (transAutoRow.NEXT_TRANS_TYPE == Origin_trans_type)
                    {
                        transAutoRow.NEXT_TRANS_TYPE = New_trans_type_list[j];
                    }
                }

                qEntity.TransAutoQuery.Add(TransQuery.TransAutoQuery(New_trans_type, (int)transAutoRow.NEXT_TRANS_TYPE));
                qEntity.TransAutoAddRecord.Add("A" + New_trans_type);
            }
        }

        void parseVoidConfig(int Origin_trans_type, int New_trans_type, ref QueryEntity qEntity)
        {
            //if ((from item in session.Instance.TPOSDB.VOID_CONFIGTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() > 0)
            if ((from item in session.Instance.LocalDataHandle.VOID_CONFIG where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).Count() > 0)
            {
                //PRDTPOSDB.VOID_CONFIGRow voidConfigRow = (from item in session.Instance.TPOSDB.VOID_CONFIGTableAdapter.GetData() where item.TRANS_TYPE.ToString() == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];
                EntityFramework.DataLocalEntities.VOID_CONFIG voidConfigRow = (from item in session.Instance.LocalDataHandle.VOID_CONFIG where item.TRANS_TYPE == Origin_trans_type && item.DELETE_FLAG == "0" select item).ToList()[0];

                qEntity.VoidConfigQuery.Add(TransQuery.VoidConfigQuery(New_trans_type));
                qEntity.VoidConfigAddRecord.Add("A" + New_trans_type);
            }
        }
        /// <summary>
        /// 不处理情况：
        /// 1. 以上线打印模版组中，新增返回码对应打印模版
        /// </summary>
        /// <param name="Data_index"></param>
        void parsePrint(int Data_index, ref QueryEntity qEntity)
        {

            List<int> origin_tbl_print_module = new List<int>();
            int origin_print_module_group = Data_index;

            List<int> new_tbl_print_module = new List<int>();
            int new_print_module_group = 0;

            if (origin_print_module_group == 0)
            {
                return;
            }

            //if ((from item in session.Instance.TPOSDB.PRINT_MODULE_GROUPTableAdapter.GetData() where (int)item.GROUP_ID == origin_print_module_group select item).Count() == 0)
            if ((from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP where (int)item.GROUP_ID == origin_print_module_group select item).Count() == 0)
            {
                //说明测试环境都不存在该模块号，直接不处理
                return;
            }

            //PRDTPOSDB.PRINT_MODULE_GROUPRow pmgRow = (from item in session.Instance.TPOSDB.PRINT_MODULE_GROUPTableAdapter.GetData() where (int)item.GROUP_ID == origin_print_module_group select item).ToList()[0];

            EntityFramework.DataLocalEntities.PRINT_MODULE_GROUP pmgRow = (from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP where (int)item.GROUP_ID == origin_print_module_group select item).ToList()[0];

            //if ((from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where (int)item.GROUP_ID == origin_print_module_group && item.GROUP_NAME == pmgRow.GROUP_NAME select item).Count() > 0)
            if ((from item in session.Instance.OnlineDataHandle.PRINT_MODULE_GROUP where (int)item.GROUP_ID == origin_print_module_group && item.GROUP_NAME == pmgRow.GROUP_NAME select item).Count() > 0)
            {
                //说明测试环境和生产环境打印模版相同，直接不处理
                return;
            }
            //else if ((from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.GROUP_NAME == pmgRow.GROUP_NAME select item).Count() > 0)
            else if ((from item in session.Instance.OnlineDataHandle.PRINT_MODULE_GROUP where item.GROUP_NAME == pmgRow.GROUP_NAME select item).Count() > 0)
            {
                //生产环境中已存在该模版组，但是编号不同
               // new_print_module_group = (from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.GROUP_NAME == pmgRow.GROUP_NAME select (int)item.GROUP_ID).ToList()[0];

                new_print_module_group = (from item in session.Instance.OnlineDataHandle.PRINT_MODULE_GROUP where item.GROUP_NAME == pmgRow.GROUP_NAME select (int)item.GROUP_ID).ToList()[0];

                qEntity.PrintModuleGroupAddRecord.Add("Z" + new_print_module_group + "F" + origin_print_module_group);
                return;
            }
            else if ((from item in qEntity.PrintModuleGroupAddRecord where int.Parse(item.Split('F')[1]) == origin_print_module_group select item).Count() > 0)
            {
                //打印模版在前面的交易中已经添加
                return;
            }
            else
            {

                // group id 从 ParameterClass.PRINTGROUPMIN 开始添加
                // tbl_print_module 从 ParameterClass.PRINTMODULEMIN 开始
                //List<int> unavailiable_pmg = (from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where (int)item.GROUP_ID > ParameterClass.PRINTGROUPMIN select (int)item.GROUP_ID).ToList();
                List<int> unavailiable_pmg = (from item in session.Instance.OnlineDataHandle.PRINT_MODULE_GROUP where (int)item.GROUP_ID > ParameterClass.PRINTGROUPMIN select (int)item.GROUP_ID).ToList();

                foreach (string s in qEntity.PrintModuleGroupAddRecord)
                {
                    if (!unavailiable_pmg.Contains(int.Parse(s.Split('F')[0].Substring(1))))
                    {
                        unavailiable_pmg.Add(int.Parse(s.Split('F')[0].Substring(1)));
                    }
                }

                unavailiable_pmg.Sort();

                for (int j = 0; j < unavailiable_pmg.Count - 1; j++)
                {
                    if (Math.Abs(unavailiable_pmg[j + 1] - unavailiable_pmg[j]) > 1)
                    {
                        new_print_module_group = unavailiable_pmg[j] + 1;

                        qEntity.PrintModuleGroupAddRecord.Add("Z" + new_print_module_group + "F" + origin_print_module_group);
                        qEntity.PrintModuleGroupQuery.Add(TransQuery.PrintModuleGroupQuery(new_print_module_group.ToString(), pmgRow.GROUP_NAME, pmgRow.DEPT_NO));

                        break;

                    }
                }


            }


            //需要新增打印模版组情况
            //
            //
            //if ((from item in session.Instance.TPOSDB.PRINT_MODULE_GROUP_MEMBERTableAdapter.GetData() where (int)item.GROUP_ID == origin_print_module_group select item).Count() == 0)
            if ((from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP_MEMBER where (int)item.GROUP_ID == origin_print_module_group select item).Count() == 0)
            {
                //机构打印模版未配置，采用默认模版2
                origin_tbl_print_module.Add(2);
            }
            else
            {
                //origin_tbl_print_module = (from item in session.Instance.TPOSDB.PRINT_MODULE_GROUP_MEMBERTableAdapter.GetData() where (int)item.GROUP_ID == origin_print_module_group select (int)item.MODULE).ToList();

                origin_tbl_print_module = (from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP_MEMBER where (int)item.GROUP_ID == origin_print_module_group select (int)item.MODULE).ToList();

            }

            foreach (int s in origin_tbl_print_module)
            {
                new_tbl_print_module.Add(s);
            }


            for (int k = 0; k < origin_tbl_print_module.Count; k++)
            {
                //PRDTPOSDB.TBL_PRINT_MODULERow tpmRow = (from item in session.Instance.TPOSDB.TBL_PRINT_MODULETableAdapter.GetData() where (int)item.MODULE == origin_tbl_print_module[k] select item).ToList()[0];
                EntityFramework.DataLocalEntities.TBL_PRINT_MODULE tpmRow = (from item in session.Instance.LocalDataHandle.TBL_PRINT_MODULE where (int)item.MODULE == origin_tbl_print_module[k] select item).ToList()[0];

                //if ((from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where item.MODULE == tpmRow.MODULE && item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select item).Count() > 0)
                if ((from item in session.Instance.OnlineDataHandle.TBL_PRINT_MODULE where item.MODULE == tpmRow.MODULE && item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select item).Count() > 0)
                {
                    //说明机构打印模版，DB和生产环境是相同的，直接使用生产环境的数据
                    new_tbl_print_module[k] = origin_tbl_print_module[k];
                    continue;
                }
                //else if ((from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select item).Count() > 0)
                else if ((from item in session.Instance.OnlineDataHandle.TBL_PRINT_MODULE where item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select item).Count() > 0)
                {
                    //说明生产存在DB相同的模版，但是编号不同
                    //new_tbl_print_module[k] = (from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select (int)item.MODULE).ToList()[0];

                    new_tbl_print_module[k] = (from item in session.Instance.OnlineDataHandle.TBL_PRINT_MODULE where item.DESCRIBE == tpmRow.DESCRIBE && item.DELETE_FLAG == "0" select (int)item.MODULE).ToList()[0];

                    continue;
                }
                else if ((from item in qEntity.TBLPrintModuleAddRecord where item.Split('F')[1] == origin_tbl_print_module[k].ToString() select item).Count() > 0)
                {
                    //说明之前的指令中，已经对该模版处理
                    new_tbl_print_module[k] = int.Parse((from item in qEntity.TBLPrintModuleAddRecord where item.Split('F')[1] == origin_tbl_print_module[k].ToString() select item).ToList()[0].Split('F')[0].Substring(1));
                    continue;
                }
                else
                {
                    //说明该模版暂时不存在，需要进一步添加
                    //List<int> unavailiable_tpm = (from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where (int)item.MODULE > ParameterClass.PRINTMODULEMIN select (int)item.MODULE).ToList();

                    List<int> unavailiable_tpm = (from item in session.Instance.OnlineDataHandle.TBL_PRINT_MODULE where (int)item.MODULE > ParameterClass.PRINTMODULEMIN select (int)item.MODULE).ToList();

                    foreach (string s in qEntity.TBLPrintModuleAddRecord)
                    {
                        unavailiable_tpm.Add(int.Parse(s.Split('F')[0].Substring(1)));
                    }

                    unavailiable_tpm.Sort();

                    for (int j = 0; j < unavailiable_tpm.Count - 1; j++)
                    {
                        if (Math.Abs(unavailiable_tpm[j + 1] - unavailiable_tpm[j]) > 1)
                        {
                            new_tbl_print_module[k] = unavailiable_tpm[j] + 1;

                            qEntity.TBLPrintModuleAddRecord.Add("Z" + new_tbl_print_module[k].ToString() + "F" + origin_tbl_print_module[k]);
                            qEntity.TBLPrintModuleQuery.Add(TransQuery.TblPrintModuleQuery(new_tbl_print_module[k].ToString(), tpmRow.DESCRIBE, tpmRow.PRINT_NUM.ToString(), tpmRow.TITLE1.ToString(), tpmRow.TITLE2.ToString(), tpmRow.TITLE3.ToString(), tpmRow.SIGN1.ToString(), tpmRow.SIGN2.ToString(), tpmRow.SIGN3.ToString(), tpmRow.REC_NUM.ToString(), tpmRow.REC_NO, tpmRow.LOGO_FLAG, tpmRow.DEPT_NO));

                            break;
                        }
                    }

                }


            }

            //至此打印组所使用的打印模版，均一一对应的包含在origin_tbl_print_module 和 new_tbl_print_module
            //List<PRDTPOSDB.PRINT_MODULE_GROUP_MEMBERRow> pmgmRow = (from item in session.Instance.TPOSDB.PRINT_MODULE_GROUP_MEMBERTableAdapter.GetData() where item.GROUP_ID == pmgRow.GROUP_ID && item.DELETE_FLAG == "0" select item).ToList();

            //List<EntityFramework.DataLocalEntities.PRINT_MODULE_GROUP_MEMBER> pmgmRow = (from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP_MEMBER where item.GROUP_ID == pmgRow.GROUP_ID && item.DELETE_FLAG == "0" select item).ToList();

            List<printmodulemember> pmms =
                (from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP_MEMBER
                 where item.GROUP_ID == pmgRow.GROUP_ID && item.DELETE_FLAG == "0"
                 select new printmodulemember
                 {
                     GroupId = (int)item.GROUP_ID,
                     Module = (int)item.MODULE,
                     ReturnCode = item.RETURN_CODE
                 }).ToList();

            //foreach (PRDTPOSDB.PRINT_MODULE_GROUP_MEMBERRow row in pmgmRow)
            foreach (printmodulemember pmm in pmms)
            {
                EntityFramework.DataLocalEntities.PRINT_MODULE_GROUP_MEMBER row =
                    (from item in session.Instance.LocalDataHandle.PRINT_MODULE_GROUP_MEMBER
                     where (int)item.GROUP_ID == pmm.GroupId && (int)item.MODULE == pmm.Module && item.RETURN_CODE == pmm.ReturnCode
                     select item).First() as EntityFramework.DataLocalEntities.PRINT_MODULE_GROUP_MEMBER;

                for (int j = 0; j < new_tbl_print_module.Count; j++)
                {
                    if ((int)row.MODULE == origin_tbl_print_module[j])
                    {
                        qEntity.PrintModuleGroupMemberAddRecord.Add("A" + new_print_module_group + "R" + row.RETURN_CODE + "W" + new_tbl_print_module[j]);
                        qEntity.PrintModuleGroupMemberQuery.Add(TransQuery.PrintModuleGroupMemberQuery(new_print_module_group.ToString(), new_tbl_print_module[j].ToString(), row.RETURN_CODE, row.STYLUS_PRINTER_FLAG, row.ID, row.DEPT_NO, row.TMIS_PRINTER_FLAG));
                        break;
                    }
                }
            }







            //if (Data_index == 0)
            //    return;

            //if ((from item in session.Instance.TPOSDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)(item.GROUP_ID) == Data_index select item).Count() > 0)
            //{
            //    PRDTPOSDB.PRINT_MODULE_GROUPRow OriginPrintModuleGroup = (from item in session.Instance.TPOSDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)(item.GROUP_ID) == Data_index select item).ToList()[0];

            //    if ((from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.GROUP_NAME == OriginPrintModuleGroup.GROUP_NAME && item.GROUP_ID == OriginPrintModuleGroup.GROUP_ID select item).Count() > 0)
            //    {
            //        //使用现有group或之前刚加入的group
            //        return;
            //    }
            //    else
            //    {
            //        foreach (string s in UpLoadSQLMerger.qEntity.PrintModuleGroupQuery)
            //        {
            //            if (s.Contains(OriginPrintModuleGroup.GROUP_NAME))
            //                return;
            //        }
            //    }
            //    List<PRDTPOSDB.PRINT_MODULE_GROUP_MEMBERRow> OriginPrintModuleGroupMembers = (from item in session.Instance.TPOSDB.PRINT_MODULE_GROUP_MEMBERTableAdapter.GetData() where item.DELETE_FLAG == "0" && (int)(item.GROUP_ID) == Data_index select item).ToList();

            //    if (OriginPrintModuleGroupMembers.Count > 0)
            //    {
            //        List<int> OriginMemberModules = (from item in OriginPrintModuleGroupMembers select (int)(item.MODULE)).ToList();
            //        //去重
            //        OriginMemberModules = OriginMemberModules.Distinct().ToList();

            //        if ((from item in session.Instance.TPOSDB.TBL_PRINT_MODULETableAdapter.GetData() where item.DELETE_FLAG == "0" && OriginMemberModules.Contains((int)item.MODULE) select item).Count() > 0)
            //        {
            //            List<PRDTPOSDB.TBL_PRINT_MODULERow> TblPrintModules = (from item in session.Instance.TPOSDB.TBL_PRINT_MODULETableAdapter.GetData() where item.DELETE_FLAG == "0" && OriginMemberModules.Contains((int)item.MODULE) select item).ToList();
            //            List<int> Origin_module = new List<int>();
            //            List<int> New_module = new List<int>();
            //            //生产比对数据库中选取相同数量的空白模版号
            //            foreach (PRDTPOSDB.TBL_PRINT_MODULERow rowM in TblPrintModules)
            //            {


            //                //如果db和生产备份编号和名称相同则使用已有版本
            //                if ((from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where item.DELETE_FLAG == "0" && item.DESCRIBE == rowM.DESCRIBE select item).Count() > 0)
            //                {
            //                    continue;
            //                }



            //                Origin_module.Add((int)rowM.MODULE);

            //                List<int> unavailiable_module = (from item in session.Instance.ONLINEDB.TBL_PRINT_MODULETableAdapter.GetData() where item.MODULE > rowM.MODULE && item.MODULE < rowM.MODULE + 1000 && item.DELETE_FLAG == "0" select (int)item.MODULE).ToList();

            //                //读入已添加模版
            //                foreach (string s in UpLoadSQLMerger.qEntity.TBLPrintModuleAddRecord)
            //                {
            //                    string ss = s.Split('F')[0].Substring(1);
            //                    if (!(unavailiable_module.Contains(int.Parse(ss))))
            //                    {
            //                        unavailiable_module.Add(int.Parse(ss));
            //                    }

            //                }

            //                unavailiable_module.Sort();

            //                for (int k = 0; k < unavailiable_module.Count - 1; k++)
            //                {
            //                    if ((Math.Abs(unavailiable_module[k + 1] - unavailiable_module[k]) > 1) && !New_module.Contains(unavailiable_module[k] + 1))
            //                    {
            //                        New_module.Add(unavailiable_module[k] + 1);
            //                        UpLoadSQLMerger.qEntity.TBLPrintModuleQuery.Add(TransQuery.TblPrintModuleQuery((unavailiable_module[k] + 1).ToString(), rowM.DESCRIBE, rowM.PRINT_NUM.ToString(), rowM.TITLE1.ToString(), rowM.TITLE2.ToString(), rowM.TITLE3.ToString(), rowM.SIGN1.ToString(), rowM.SIGN2.ToString(), rowM.SIGN3.ToString(), rowM.REC_NUM.ToString(), rowM.REC_NO, rowM.LOGO_FLAG, rowM.DEPT_NO));

            //                        qEntity.TBLPrintModuleAddRecord.Add("Z" + (unavailiable_module[k] + 1).ToString() + "F" + rowM.MODULE.ToString());
            //                        break;
            //                    }
            //                }

            //            }
            //        }

            //    }

            //    //选取新的group_id 
            //    List<int> unavailiable_print_group = (from item in session.Instance.ONLINEDB.PRINT_MODULE_GROUPTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.GROUP_ID > OriginPrintModuleGroup.GROUP_ID && item.GROUP_ID < OriginPrintModuleGroup.GROUP_ID + 1000 select (int)item.GROUP_ID).ToList();

            //    unavailiable_print_group.Sort();

            //    int newid = 0;
            //    for (int k = 0; k < unavailiable_print_group.Count - 1; k++)
            //    {
            //        if (Math.Abs(unavailiable_print_group[k + 1] - unavailiable_print_group[k]) > 1)
            //        {

            //            qEntity.PrintModuleGroupQuery.Add(TransQuery.PrintModuleGroupQuery((unavailiable_print_group[k] + 1).ToString(), OriginPrintModuleGroup.GROUP_NAME, OriginPrintModuleGroup.DEPT_NO));

            //            qEntity.PrintModuleGroupAddRecord.Add("Z" + (unavailiable_print_group[k] + 1).ToString() + "F" + OriginPrintModuleGroup.GROUP_ID.ToString());

            //            newid = (unavailiable_print_group[k] + 1);
            //            break;
            //        }
            //    }
            //    foreach (PRDTPOSDB.PRINT_MODULE_GROUP_MEMBERRow pmgmRow in OriginPrintModuleGroupMembers)
            //    {
            //        string module = string.Empty;
            //        foreach (string s in qEntity.TBLPrintModuleAddRecord)
            //        {
            //            if (s.Contains(pmgmRow.MODULE.ToString()))
            //            {
            //                module = s.Split('F')[0].Substring(1);
            //                qEntity.PrintModuleGroupMemberQuery.Add(TransQuery.PrintModuleGroupMemberQuery(newid.ToString(), s, pmgmRow.RETURN_CODE, pmgmRow.STYLUS_PRINTER_FLAG, pmgmRow.ID, pmgmRow.DEPT_NO, pmgmRow.TMIS_PRINTER_FLAG));

            //                qEntity.PrintModuleGroupMemberAddRecord.Add("A" + newid.ToString() + "W" + module);
            //                break;
            //            }
            //        }
            //    }

            //}
        }

        void parseDynamic(int Data_index, ref QueryEntity qEntity)
        {
            //至少DB数据要存在
            //if ((from item in session.Instance.TPOSDB.DYNAMIC_ITEMTableAdapter.GetData() where (int)item.RECNO == Data_index select item).Count() > 0)
            if ((from item in session.Instance.LocalDataHandle.DYNAMIC_ITEM where (int)item.RECNO == Data_index select item).Count() > 0)
            {
                //PRDTPOSDB.DYNAMIC_ITEMRow dynamicItemRow = (from item in session.Instance.TPOSDB.DYNAMIC_ITEMTableAdapter.GetData() where (int)item.RECNO == Data_index select item).ToList()[0];

                EntityFramework.DataLocalEntities.DYNAMIC_ITEM dynamicItemRow = (from item in session.Instance.LocalDataHandle.DYNAMIC_ITEM where (int)item.RECNO == Data_index select item).ToList()[0];

                //查看编号相同，描述相同的情况
                //if ((from item in session.Instance.ONLINEDB.DYNAMIC_ITEMTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.TITLE == dynamicItemRow.TITLE && item.RECNO == dynamicItemRow.RECNO select item).Count() > 0)
                if ((from item in session.Instance.OnlineDataHandle.DYNAMIC_ITEM where item.DELETE_FLAG == "0" && item.TITLE == dynamicItemRow.TITLE && item.RECNO == dynamicItemRow.RECNO select item).Count() > 0)
                {
                    return;
                }

                //查看已经添加的条目
                int OriginRecno = (int)dynamicItemRow.RECNO;
                int NewRecno = 0;

                foreach (string s in qEntity.DynamicItemAddRecord)
                {
                    if (s.Contains(OriginRecno.ToString()))
                    {
                        NewRecno = int.Parse(s.Split('F')[0].Substring(1));
                        break;
                    }
                }

                if (NewRecno == 0)
                {
                    //说明是新创建的编号
                   // List<int> unavailiableRecno = (from item in session.Instance.ONLINEDB.DYNAMIC_ITEMTableAdapter.GetData() select (int)item.RECNO).ToList();

                    List<int> unavailiableRecno = (from item in session.Instance.OnlineDataHandle.DYNAMIC_ITEM select (int)item.RECNO).ToList();

                    unavailiableRecno.Sort();

                    for (int k = 0; k < unavailiableRecno.Count - 1; k++)
                    {
                        if (Math.Abs(unavailiableRecno[k + 1] - unavailiableRecno[k]) > 1)
                        {
                            NewRecno = unavailiableRecno[k] + 1;
                            qEntity.DynamicItemAddRecord.Add("Z" + NewRecno.ToString() + "F" + OriginRecno.ToString());
                            qEntity.DynamicItemQuery.Add(TransQuery.DynamicItemQuery(NewRecno.ToString(), dynamicItemRow.TITLE, dynamicItemRow.DESCRIBE, dynamicItemRow.ITEM_NUM.ToString(), dynamicItemRow.DATA_RULE, dynamicItemRow.MARKS, dynamicItemRow.DATA_SCR_TYPE.ToString(), dynamicItemRow.VARIABLE));
                            break;
                        }
                    }
                }
            }
        }

        //void parseOperationInfo(List<PRDTPOSDB.TRANS_COMMANDSRow> transCommandRows)
        void parseOperationInfo(List<EntityFramework.DataLocalEntities.TRANS_COMMANDS> transCommandRows, ref QueryEntity qEntity)
        {
            List<int> OperInfo = (from item in transCommandRows where item.FUNC_INDEX != 0 select (int)item.FUNC_INDEX).Distinct().ToList();
            OperInfo.Sort();

            //List<PRDTPOSDB.OPERATION_INFORow> operationInfoRow = (from item in session.Instance.TPOSDB.OPERATION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && OperInfo.Contains((int)item.OPER_INDEX) && (item.OPER_INDEX != 0) select item).ToList();

            //List<EntityFramework.DataLocalEntities.OPERATION_INFO> operationInfoRow = (from item in session.Instance.LocalDataHandle.OPERATION_INFO where item.DELETE_FLAG == "0" && OperInfo.Contains((int)item.OPER_INDEX) && (item.OPER_INDEX != 0) select item).ToList();

            List<int> operationInfoRow = (from item in session.Instance.LocalDataHandle.OPERATION_INFO where item.DELETE_FLAG == "0" && OperInfo.Contains((int)item.OPER_INDEX) && (item.OPER_INDEX != 0) select (int)item.OPER_INDEX).ToList();

            //索引为0，不处理
            foreach (int oiRowi in operationInfoRow)
            {
                EntityFramework.DataLocalEntities.OPERATION_INFO oiRow = null;

                oiRow = (from item in session.Instance.LocalDataHandle.OPERATION_INFO
                     where (int)item.OPER_INDEX == oiRowi
                     select item).First() as EntityFramework.DataLocalEntities.OPERATION_INFO;

                //if ((from item in session.Instance.ONLINEDB.OPERATION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.INFO1 == oiRow.INFO1 && item.OPER_INDEX == oiRow.OPER_INDEX select item).Count() > 0)
                if ((from item in session.Instance.OnlineDataHandle.OPERATION_INFO where item.DELETE_FLAG == "0" && item.INFO1 == oiRow.INFO1 && item.OPER_INDEX == oiRow.OPER_INDEX select item).Count() > 0)
                {
                    //内容相同，索引相同，不处理
                    continue;
                }

                if ((from item in qEntity.OperationInfoAddRecord where item.Contains(oiRow.OPER_INDEX.ToString()) select item).Count() > 0)
                {
                    //提示已经添加过，不再重复处理
                    continue;
                }

                //if ((from item in session.Instance.ONLINEDB.OPERATION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.MODULE_NUM == oiRow.MODULE_NUM && item.INFO1 == oiRow.INFO1 && item.INFO2 == oiRow.INFO2 && item.INFO3 == oiRow.INFO3 select item).Count() > 0)
                if ((from item in session.Instance.OnlineDataHandle.OPERATION_INFO where item.DELETE_FLAG == "0" && item.MODULE_NUM == oiRow.MODULE_NUM && item.INFO1 == oiRow.INFO1 && item.INFO2 == oiRow.INFO2 && item.INFO3 == oiRow.INFO3 select item).Count() > 0)
                {

                    //内容相同，索引不同
                    //int new_oi = (from item in session.Instance.ONLINEDB.OPERATION_INFOTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.MODULE_NUM == oiRow.MODULE_NUM && item.INFO1 == oiRow.INFO1 && item.INFO2 == oiRow.INFO2 && item.INFO3 == oiRow.INFO3 select (int)item.OPER_INDEX).ToList()[0];

                    int new_oi = (from item in session.Instance.OnlineDataHandle.OPERATION_INFO where item.DELETE_FLAG == "0" && item.MODULE_NUM == oiRow.MODULE_NUM && item.INFO1 == oiRow.INFO1 && item.INFO2 == oiRow.INFO2 && item.INFO3 == oiRow.INFO3 select (int)item.OPER_INDEX).ToList()[0];

                    qEntity.OperationInfoAddRecord.Add("Z" + new_oi.ToString() + "F" + oiRow.OPER_INDEX.ToString());
                    continue;
                }

                //搜索新的可用Oper_index
                //List<int> unavailiabe_oper_index = (from item in session.Instance.ONLINEDB.OPERATION_INFOTableAdapter.GetData() where item.OPER_INDEX >= oiRow.OPER_INDEX select (int)item.OPER_INDEX).Distinct().ToList();

                List<int> unavailiabe_oper_index = (from item in session.Instance.OnlineDataHandle.OPERATION_INFO where item.OPER_INDEX >= oiRow.OPER_INDEX select (int)item.OPER_INDEX).Distinct().ToList();

                foreach (string s in qEntity.OperationInfoAddRecord)
                {
                    unavailiabe_oper_index.Add(int.Parse(s.Split('F')[0].Substring(1)));
                }

                unavailiabe_oper_index.Sort();



                for (int k = 0; k < unavailiabe_oper_index.Count - 1; k++)
                {
                    if (Math.Abs(unavailiabe_oper_index[k + 1] - unavailiabe_oper_index[k]) > 1)
                    {
                        int new_oi = (int)(unavailiabe_oper_index[k] + 1);

                        qEntity.OperationInfoAddRecord.Add("Z" + new_oi.ToString() + "F" + oiRow.OPER_INDEX.ToString());

                        qEntity.OperationInfoQuery.Add(TransQuery.OperationInfoQuery(new_oi.ToString(), oiRow.OP_FLAG, oiRow.MODULE_NUM.ToString(), oiRow.INFO1_FORMAT, oiRow.INFO1, oiRow.INFO2_FORMAT, oiRow.INFO2, oiRow.INFO3_FORMAT, oiRow.INFO3));
                        break;
                    }
                }


            }
        }

        KeyValuePair<int, string> MergeSQLFunc(string ori_string, ref QueryEntity qEntity)
        {
            qEntity.CleanQuerys();

            if (ori_string == string.Empty)
            {
                return ErrorCode.Instance.GetStatement(9000,null);
            }

            string[] i_trans_code_string = ori_string.Split('|');
            string[] i_trans_code_real_string = new string[i_trans_code_string.Count()];

            List<int> i_trans_code = new List<int>();
            List<int> i_trans_code_real = new List<int>();

            foreach (string i in i_trans_code_string)
            {
                i_trans_code.Add(Convert.ToInt32(i.Trim()));
            }

            for (int i = 0; i < i_trans_code.Count(); i++)
            {
                i_trans_code_real.Add(i_trans_code[i]);
            }

            int t = 0;

            try
            {
                //t = (from item in session.Instance.TPOSDB.TRANS_DEFTableAdapter.GetData() where i_trans_code.Contains(item.TRANS_TYPE.ToString()) select item).Count();
                
                t = (from item in session.Instance.LocalDataHandle.TRANS_DEF where i_trans_code.Contains((Int32)(item.TRANS_TYPE)) select item).Count();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return ErrorCode.Instance.GetStatement(9001,null);
            }

            if (t != i_trans_code.Count())
            {
                //MessageBox.Show("包含DB所不存在的交易代码");
                return ErrorCode.Instance.GetStatement(9001, null);
            }

            //遍历所有代码
            //计算出trans_code DB和上线对应关系
            for (int i = 0; i < i_trans_code.Count(); i++)
            {
                int code = i_trans_code[i];
                //if ((from item in session.Instance.ONLINEDB.TRANS_DEFTableAdapter.GetData() where item.TRANS_TYPE.ToString() == code && (item.DELETE_FLAG == "0") select item).Count() != 0)
                if ((from item in session.Instance.OnlineDataHandle.TRANS_DEF where item.TRANS_TYPE == code && (item.DELETE_FLAG == "0") select item).Count() != 0)
                {

                    //出现交易代码冲突
                    List<int> unavailiable_code = (from item in session.Instance.OnlineDataHandle.TRANS_DEF where (item.TRANS_TYPE > code && (item.TRANS_TYPE < code + 1000) && (item.DELETE_FLAG == "0")) select (int)item.TRANS_TYPE).ToList();

                    foreach (int s in i_trans_code_real)
                    {
                        if (!unavailiable_code.Contains(s))
                        {
                            unavailiable_code.Add(s);
                        }
                    }

                    unavailiable_code.Sort();

                    int temp = unavailiable_code[0];

                    foreach (int co in unavailiable_code)
                    {
                        if (temp - co != 0)
                        {
                            if (!(i_trans_code_real.Contains(temp)))
                            {
                                i_trans_code_real[i] = temp;
                                break;
                            }
                        }
                        temp++;
                    }

                    if (i_trans_code_real[i] != temp)
                    {
                        return ErrorCode.Instance.GetStatement(9002, new List<string> { i_trans_code_real[i].ToString() });
                    }
                }
            }
            //TPOSDB上的交易代码保存在i_trans_code 
            //上线后的交易代码 保存在 i_trans_code_real

            for (int i = 0; i < i_trans_code.Count(); i++)
            {
                //PRDTPOSDB.TRANS_DEFRow transDefRow = (from item in session.Instance.TPOSDB.TRANS_DEFTableAdapter.GetData() where item.TRANS_TYPE.ToString() == i_trans_code[i] && (item.DELETE_FLAG == "0") select item).ToList()[0];
                int trans_type_tmp = i_trans_code[i];
                WebToolsBox.EntityFramework.DataLocalEntities.TRANS_DEF transDefRow = (from item in session.Instance.LocalDataHandle.TRANS_DEF where item.TRANS_TYPE == trans_type_tmp && (item.DELETE_FLAG == "0") select item).ToList()[0];
                //获取后续交易
                string next_trans = transDefRow.NEXT_TRANS_CODE;
                if (transDefRow.NEXT_TRANS_CODE != null && transDefRow.NEXT_TRANS_CODE.Trim() != string.Empty && transDefRow.NEXT_TRANS_CODE.Trim() != "null")
                {
                    for (int j = 0; j < i_trans_code.Count(); j++)
                    {
                        if (i_trans_code[j] == int.Parse(transDefRow.NEXT_TRANS_CODE))
                        {
                            next_trans = i_trans_code_real[j].ToString().PadLeft(8, '0');
                            break;
                        }
                    }
                }

                //function_info
                string new_function_info = parseFunctionInfo((int)transDefRow.FUNCTION_INDEX, ref qEntity).Value;


                //添加trans_def语句
                qEntity.TransDefQuerys.Add(TransQuery.TransDefQuery(i_trans_code_real[i].ToString(), i_trans_code_real[i].ToString().PadLeft(8, '0'), next_trans, transDefRow.AUTOVOID, transDefRow.PIN_BLOCK, new_function_info, transDefRow.TRANS_NAME, transDefRow.TELEPHONE_NO.ToString(), transDefRow.DISP_TYPE, transDefRow.SERVER_CODE, transDefRow.DEPTNO, transDefRow.REMARK, transDefRow.CASHIERBILLAD_RECNO.ToString(), transDefRow.NII));

                //添加field_message语句
                parseFieldMessage(i_trans_code[i], i_trans_code_real[i], ref qEntity);

                //添加trans_split
                parseTransSplit(i_trans_code[i], i_trans_code_real[i], i_trans_code, i_trans_code_real, ref qEntity);

                //添加tbl_exchange
                parseTblExchange(i_trans_code[i], i_trans_code_real[i], ref qEntity);

                //添加auto_void
                parseAutoVoid(i_trans_code[i], i_trans_code_real[i], i_trans_code, i_trans_code_real, ref qEntity);

                //添加void_config
                parseVoidConfig(i_trans_code[i], i_trans_code_real[i], ref qEntity);

                //List<PRDTPOSDB.TRANS_COMMANDSRow> transCommandRows = (from item in session.Instance.TPOSDB.TRANS_COMMANDSTableAdapter.GetData() where item.DELETE_FLAG == "0" && item.TRANS_TYPE.ToString() == i_trans_code[i] select item).ToList();

                trans_type_tmp = i_trans_code[i];

                //未解决问题: 编号Q0001
                //问题描述  ：查询返回故障，当查询同个表的全列多条数据时，返回内容数量相同，但是内容重复成首条内容
                //暂时方案  ：
                //              1. 先获取对应交易代码下,删除标记为0数据的 step 和 flag，该数据具备唯一性。
                //              2. 按照flag，step 的顺序再读取trans_commands 数据
                //              3. 将获取到的数据重新整合成为list。
                //List<EntityFramework.DataLocalEntities.TRANS_COMMANDS> transCommandRows = (from item in session.Instance.LocalDataHandle.TRANS_COMMANDS where item.DELETE_FLAG == "0" && item.TRANS_TYPE == trans_type_tmp select item).ToList();




                //step 1:
                List<commandItem> transItems =
                    (from item in session.Instance.LocalDataHandle.TRANS_COMMANDS
                     where item.DELETE_FLAG == "0" && (int)(item.TRANS_TYPE) == trans_type_tmp
                     select
                     new commandItem()
                     {
                         FLAG = item.FLAG,
                         STEP = (int)item.STEP
                     }).ToList();
           
                     
               //step 2 & 3:
                List<EntityFramework.DataLocalEntities.TRANS_COMMANDS> transCommandRows = new List<EntityFramework.DataLocalEntities.TRANS_COMMANDS>();

                foreach (commandItem comm in transItems)
                {
                    EntityFramework.DataLocalEntities.TRANS_COMMANDS command =
                        (from item in session.Instance.LocalDataHandle.TRANS_COMMANDS
                         where item.DELETE_FLAG == "0" && (int)(item.TRANS_TYPE) == trans_type_tmp && item.FLAG == comm.FLAG && item.STEP == comm.STEP
                         select item).First() as EntityFramework.DataLocalEntities.TRANS_COMMANDS;


                    transCommandRows.Add(command);
                }


                //开始遍历指令之前，先处理提示信息
                parseOperationInfo(transCommandRows, ref qEntity);

                //foreach (PRDTPOSDB.TRANS_COMMANDSRow row in transCommandRows)
                foreach (EntityFramework.DataLocalEntities.TRANS_COMMANDS row in transCommandRows)
                {
                    //对单交易的trans_commands遍历

                    switch (row.ORGCOMMAND.Trim())
                    {
                        case "21":
                            {
                                int Data_index21 = (int)row.DATA_INDEX;
                                parsePrint(Data_index21, ref qEntity);
                                parseTransCommands(i_trans_code_real[i], qEntity.PrintModuleGroupAddRecord, row, ref qEntity);
                                break;
                            }
                        case "69":
                            {
                                int Data_index69 = (int)row.DATA_INDEX;
                                parsePrint(Data_index69, ref qEntity);
                                parseTransCommands(i_trans_code_real[i], qEntity.PrintModuleGroupAddRecord, row, ref qEntity);
                                break;
                            }
                        case "41":
                            {
                                int Data_index41 = (int)row.DATA_INDEX;
                                parseDynamic(Data_index41, ref  qEntity);
                                parseTransCommands(i_trans_code_real[i], qEntity.DynamicItemQuery, row, ref  qEntity);
                                break;
                            }
                        case "40":
                            {
                                int Data_index40 = (int)row.DATA_INDEX;
                                parseDynamicValid(Data_index40, ref qEntity);
                                parseTransCommands(i_trans_code_real[i], qEntity.DynamicValidQuery, row, ref qEntity);
                                break;
                            }
                        case "22":
                            {
                                int Data_index22 = (int)row.DATA_INDEX;
                                parseTblDispContent(Data_index22, ref qEntity);
                                parseTransCommands(i_trans_code_real[i], qEntity.TblDispContentAddRecord, row, ref qEntity);
                                break;
                            }
                        default:
                            {
                                parseTransCommands(i_trans_code_real[i], null, row, ref qEntity);
                                break;
                            }

                    }
                }
            }

            //parse完毕，开始存储
            return ErrorCode.Instance.GetStatement(0000, new List<string> { "成功" });
        }

        /// <summary>
        /// 向外提供接口，供查询sql语句
        /// </summary>
        /// <param name="ori_string">源交易列表</param>
        /// <returns>执行结果</returns>
        public KeyValuePair<int, string> GetSQL(string ori_string, ref QueryEntity qEntity)
        {
            return MergeSQLFunc(ori_string,ref qEntity);
        }
    }
}