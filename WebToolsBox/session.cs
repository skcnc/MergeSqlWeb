using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebToolsBox.EntityFramework;

namespace WebToolsBox
{
    public class session
    {
        DataLocalEntities DataLocalObject;
        DataOnlineEntities DataOnlineObject;

        private session() { }
        private static readonly session instance = new session();
        public static session Instance
        {
            get 
            {
                //数据库对象实例化
                return instance; 
            }
        }

        /// <summary>
        /// 测试数据库的实例
        /// </summary>
        public DataLocalEntities LocalDataHandle
        {
            get
            {
                if (DataLocalObject == null)
                {
                    DataLocalObject = new DataLocalEntities();
                }
                return DataLocalObject;
            }
        }

        /// <summary>
        /// 准生产数据库的实例
        /// </summary>
        public DataOnlineEntities OnlineDataHandle
        {
            get
            {
                if (DataOnlineObject == null)
                {
                    DataOnlineObject = new DataOnlineEntities();
                }
                return DataOnlineObject;
            }
        }
        
    }
}