using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebToolsBox.EntityFramework;

namespace WebToolsBox
{
    public class session
    {

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
        /// 测试DB数据库的实例
        /// </summary>
        public EntityFramework.DataLocalEntities.DataLocalEntities LocalDataHandle
        {
            get
            {
                return new EntityFramework.DataLocalEntities.DataLocalEntities(); 
            }
        }

        /// <summary>
        /// 测试联机数据库的实例
        /// </summary>
        public EntityFramework.DataLocalEntities.DataUmsEntity UmsDataHandle
        {
            get
            {
                return new EntityFramework.DataLocalEntities.DataUmsEntity();
            }
        }

        /// <summary>
        /// 准生产数据库的实例
        /// </summary>
        public EntityFramework.DataLocalEntities.DataOnlineEntity2 OnlineDataHandle
        {
            get
            {
                //if (DataOnlineObject == null)
                //{
                //    DataOnlineObject = new EntityFramework.DataLocalEntities.DataOnlineEntity2();
                //}
                //return DataOnlineObject;
                return new EntityFramework.DataLocalEntities.DataOnlineEntity2();
            }
        }


        
    }
}