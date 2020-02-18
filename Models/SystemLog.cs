using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models
{
    public class SystemLog
    {
        public int SystemLogID { get; set; }

        public string Message { get; set; }

        public DateTime LogTime { get; set; }
    }
}
