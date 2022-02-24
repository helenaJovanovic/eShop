using eShop.Authorization;
using eShop.Entities;
using eShop.Models;
using eShop.Models.DTOs;
using eShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {

        private readonly OrderService _ors;
        public OrdersController(OrderService orderService)
        {
            _ors = orderService;
        }

        [Authorize(Role.User)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            return await _ors.GetUserOrdersAsync();
        }

        [Authorize(Role.Admin)]
        [HttpGet("all/")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            return await _ors.GetAllOrdersAsync();
        }

        [Authorize(Role.User)]
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderRequest orderRequest)
        {
            Order order = await _ors.createOrderAsync(orderRequest);
            return CreatedAtAction(nameof(GetUserOrders), new { id = order.OrderId }, order);
        }

        [Authorize(Role.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteItem(int id)
        {
            var order = await _ors.GetOrderByIdAsync(id);

            if (order == null)
            {
                throw new KeyNotFoundException("Item doesn't exist");
            }

            await _ors.RemoveOrderAsync(order);

            return NoContent();
        }
    }
}
