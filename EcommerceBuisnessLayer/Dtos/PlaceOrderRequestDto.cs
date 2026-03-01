using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBuisnessLayer.Dtos
{
    public class PlaceOrderRequestDto
    {
        public int Userid { get; set; }
        public List<OrderItemRequestDto> Items { get; set; } = new();
    }

}
