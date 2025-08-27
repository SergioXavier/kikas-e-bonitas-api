using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KikasBonitas.Api.Data;
using KikasBonitas.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KikasBonitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
            => await _context.OrderItems
                             .Include(oi => oi.Product)
                             .Include(oi => oi.Order)
                             .ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> GetOrderItem(int id)
        {
            var orderItem = await _context.OrderItems
                                          .Include(oi => oi.Product)
                                          .Include(oi => oi.Order)
                                          .FirstOrDefaultAsync(oi => oi.OrderId == id);
            if (orderItem == null) return NotFound();
            return orderItem;
        }

        [HttpPost]
        public async Task<ActionResult<OrderItem>> CreateOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.OrderItemId }, orderItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, OrderItem orderItem)
        {
            if (id != orderItem.OrderItemId) return BadRequest();
            _context.Entry(orderItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
