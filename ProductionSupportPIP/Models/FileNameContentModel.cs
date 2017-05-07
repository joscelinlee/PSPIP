using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProductionSupportPIP.Models
{
    public class FileNameContentModel
    {
        public List<String> FileNameArray { get; set; }
        public List<String> FileContentArray { get; set; }
        public int ArrayCount { get; set; }
        public String RetrieveCSIID { get; set; }
        public String RetrieveArticleName { get; set; }
        public String RetrieveFileContent { get; set; }
        public String SuccessMessage { get; set; }
        public bool UpdateBool { get; set; }
        public int CurrentUpdateID { get; set; }
    }
}
