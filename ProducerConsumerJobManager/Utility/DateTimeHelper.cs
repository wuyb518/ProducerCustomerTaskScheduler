using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerJobManager.Utility
{
    public static class DateTimeHelper
    {

        /// <summary>
        /// DateTime时间格式转换为13位带毫秒的Unix时间戳
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static long Convert2LongTimestamp(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalMilliseconds;
        }

        //=========================================
        /// <summary>
        /// DateTime时间格式转换为10位不带毫秒的Unix时间戳
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static int Convert2ShortTimestamp(DateTime time)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        //========================================
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }


        public static long GetCurrentLongTimestamp()
        {
            return Convert2LongTimestamp(DateTime.Now);
        }

        public static int GetCurrentShortTimestamp()
        {
            return Convert2ShortTimestamp(DateTime.Now);
        }

    }
}
