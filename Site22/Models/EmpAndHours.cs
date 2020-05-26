using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site22.Models
{
    public class EmpAndHours
    {
        public String subject { get; set; }
        public String EmpName { get; set; }
        public int hoursForLect { get; set; } = 0;
        public int hoursForPractice { get; set; } = 0;
        public int hoursForLab { get; set; } = 0;
    }
}