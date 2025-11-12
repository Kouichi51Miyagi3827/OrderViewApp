using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderViewApp.Models
{
    public class Order
    {
        // XAMLのBinding名と一致させるのが重要
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public string? Detail { get; set; }
        public DateTime DueDate { get; set; }
        public string? ExpiryDate { get; set; } // 期限基本（表示用・ソート用）
        public string? ExpiryDate2 { get; set; } // 期限2（表示用）
        public string? Requirements { get; set; }
        public string? CustomerName { get; set; }
        public int? ReceptionNumber { get; set; }
        public int? GroupColorIndex { get; set; } // グループ背景色インデックス（0=水色、1=緑）
        public string? StockOK { get; set; }
        public string? StockCleaning { get; set; }
        public short? Preparer { get; set; }
        public bool? Departure { get; set; }
    }
}
