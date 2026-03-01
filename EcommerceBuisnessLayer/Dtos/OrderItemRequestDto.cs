using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBuisnessLayer.Dtos
{
   
        public class OrderItemRequestDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }



}
