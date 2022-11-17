using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    //it is in model folder because it will be in db (not in viewmodel)
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; } //navigation property
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        //number of books user wants in shopping cart
        public int Count { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } //navigation property  
        //we don't need this property in the db - NotMapped
        [NotMapped]
        public double Price { get; set; } //final price of product based on quantity
    }
}
