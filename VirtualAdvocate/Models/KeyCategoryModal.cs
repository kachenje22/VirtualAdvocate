using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class KeyCategoryModal
    {
        public int KeyCateogryId { get; set; }
        public string KeyCategoryName { get; set; }
        public int AssetKeyId { get; set; }
        public string AssetName { get; set; }
        public string CustomerName { get; set; }
        public string AssetInsured { get; set; }
        public string Currency { get; set; }
    }
}