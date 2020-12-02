using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.Healthchecks.Models
{
    public class PropertyDataDetails
    {
        public int id { get; set; }
        public string text { get; set; }
        public string alias { get; set; }
        public bool published { get; set; }
        public DateTime updateDate { get; set; }
        public string dataNvarchar { get; set; }
        public string dataNText { get; set; }

    }
}
