using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mango.Services.OrderAPI
{
    public class CartDto
    {
 
        public CartHeaderDto CartHeader { get; set; }

        public IEnumerable<CartDetailsDto>? CartDetails { get; set; }
    }
}
