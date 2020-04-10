using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IMTO.Common
{
    public static class Helper
    {
        public static bool IsItemExistInList(string[] list, string item)
        {

            if ((list == null | string.IsNullOrEmpty(item)))
                return false;

            string foundItem = (from l in list where l.ToLower() == item.ToLower() select l).FirstOrDefault();

            return !string.IsNullOrEmpty(foundItem);

        }

        public static bool AreItemsInList(string[] list, string[] items)
        {

            if ((list == null || items == null))
                return false;

            var count = list.Intersect(items).ToList().Count;

            return count == items.Length;
        }

        public static int CommmonItemsInList(string[] list, string[] items)
        {

            if ((list == null || items == null))
                return 0;

            var count = list.Intersect(items).ToList().Count;

            return count;
        }

        public static bool IsAnyItemInList(string[] list, string[] items)
        {

            if ((list == null || items == null))
                return false;
            bool hasMatch = list.Select(x => x)
                          .Intersect(items)
                          .Any();

            return hasMatch;
        }
        public static string GetInfo<T>(T obj)
        {
            var builder = new StringBuilder();
            foreach (PropertyInfo p in obj.GetType().GetProperties())
            {
                var s = p.GetValue(obj, null);
                builder.AppendLine(string.Format("{0}: {1}", p.Name, s?.ToString()));
            }
            return builder.ToString();
        }
        public static string GetNextGuid()
        {
            byte[] b = Guid.NewGuid().ToByteArray();
            DateTime dateTime = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
            TimeSpan timeOfDay = now.TimeOfDay;
            byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
            byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
            Array.Reverse(bytes1);
            Array.Reverse(bytes2);
            Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
            Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
            return new Guid(b).ToString().Replace("-", "");
        }
    }
}
