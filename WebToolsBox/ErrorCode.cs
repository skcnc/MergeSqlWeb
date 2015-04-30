using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebToolsBox
{
    public class ErrorCode : Dictionary<int, string>
    {
        private ErrorCode()
        {
            this.Add(0000, "%"); //无需处理或已成功执行,返回值可能是后续程序需要的内容。
            this.Add(9000, "查询字符串为空");
            this.Add(9001, "测试DB未能检索到该交易");
            this.Add(9002, "所选交易代码：%后1000个代码中无可用值");
            this.Add(9003, "测试DB未能检索到Function_Info:%");
        
        }
        private static readonly ErrorCode instance = new ErrorCode();
        public static ErrorCode Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 返回错误描述
        /// </summary>
        /// <param name="key">返回码</param>
        /// <param name="paras">内容参数，替换内容中的$字符</param>
        /// <returns>说明</returns>
        public KeyValuePair<int,string> GetStatement(int key,List<string> paras)
        {
            var state = (from item in this where item.Key == key select item.Value);
            if(state.Count() == 0)
            {
                return new KeyValuePair<int, string>(key, "未知错误，请联系开发人员");
            }

            string statement = state.ToList()[0];

            if(paras != null)
            {
                foreach(string para in paras)
                {
                    int i = statement.IndexOf('%');
                    if(i >=0)
                    {
                        statement = statement.Remove(i,1).Insert(i,para);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return new KeyValuePair<int, string>(key, statement);
        }
    }
}