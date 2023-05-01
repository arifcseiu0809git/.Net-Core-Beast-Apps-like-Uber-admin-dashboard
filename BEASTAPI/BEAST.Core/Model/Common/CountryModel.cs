﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Common
{
    public class CountryModel : AuditModel
    {
        public string CountryName { get; set; }
        public string ShortName { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public int MobileNumberDigitCount { get; set; }
        public string MobileNumberCode { get; set; }
        public int SortingPriority { get; set; }
    }
}
