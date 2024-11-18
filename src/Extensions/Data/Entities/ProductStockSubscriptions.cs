using Insite.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extensions.Data.Entities
{
    [Table("ProductStockSubscriptions", Schema = "Extensions")]
    public class ProductStockSubscriptions : EntityBase
    {
        [Required(AllowEmptyStrings = false)]
        public string ProductErpNumber { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string WarehouseName { get; set; }
    }
}