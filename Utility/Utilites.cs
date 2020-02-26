using Endevrian.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Utility
{
    public static class Utilites
    {
        public static AdventureLog NewCreateDateFormatted(AdventureLog adventureLog)
        {
            DateTime createTime = DateTime.Now;
            string stringVersion = createTime.ToString("M/d/yyyy");
            createTime = DateTime.Parse(stringVersion);

            adventureLog.DisplayLogDate = stringVersion;
            adventureLog.LogDate = createTime;

            return adventureLog;
        }

        public static DateTime NewCreateDateFormatted()
        {
            DateTime createTime = DateTime.Now;
            string stringVersion = createTime.ToString("M/d/yyyy");
            createTime = DateTime.Parse(stringVersion);

            return createTime;
        }
    }
}
