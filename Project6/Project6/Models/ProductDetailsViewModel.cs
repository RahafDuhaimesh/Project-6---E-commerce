using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project6.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public Review Review { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
    }
}