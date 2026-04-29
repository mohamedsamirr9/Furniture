using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models.Enum
{
    public enum OrderStatus
    {
        
        Pending = 0,     
        Accepted = 1,   
        Paid = 2,        
        Processing = 3,   
        Shipped = 4,      
        Delivered = 5,   
        Completed = 6,    
        Cancelled = 7,    
        Declined = 8   
    }
}
