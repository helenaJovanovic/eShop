using eShop.Entities;
using eShop.Helpers;
using eShop.Models;
using eShop.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Services
{
    public class OrderService: AbstractCartOrder
    {
        private User user;
        public OrderService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
            user = getUser();
        }

        public async Task createOrderItemAsync(Cart cart, CartItem cartItem, int OrderId)
        {

            OrderItem oi = new OrderItem { ItemId = cartItem.ItemId, OrderId = OrderId, Quantity = cartItem.Quantity};
            await _context.OrderItems.AddAsync(oi);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            List<Order> orders = await _context.Orders.ToListAsync();

            foreach (Order or in orders)
            {
                await _context.Entry(or).Collection(order => order.OrderItems).LoadAsync();
                foreach(OrderItem oi in or.OrderItems)
                {
                    await _context.Entry(oi).Reference(orderItem => orderItem.Item).LoadAsync();
                }
            }

            return orders;
        }
        public async Task<List<Order>> GetUserOrdersAsync()
        {
            List<Order> orders = await _context.Orders.Where(o => o.UserId == user.UserId).ToListAsync();

            foreach(Order or in orders)
            {
                await _context.Entry(or).Collection(order => order.OrderItems).LoadAsync();
                foreach (OrderItem oi in or.OrderItems)
                {
                    await _context.Entry(oi).Reference(orderItem => orderItem.Item).LoadAsync();
                }
            }

            return orders;
        }

        public async Task<Order> createOrderAsync(OrderRequest or)
        {
            /*Payment Id placeholder*/
            Random random = new Random();
            string paymentId = random.Next(1000000, 2000000).ToString();
            /*Payment Id placeholder*/

            Cart cart = await GetUserCartAsync(user.UserId);
            Order order = new Order { UserId = user.UserId, Address = or.Address, PhoneNumber = or.PhoneNumber, Total = cart.Total, PaymentId = paymentId };
            await _context.Orders.AddAsync(order);

            await SaveAsync();
            //cart has cartItems
            List<CartItem> cartItems = cart.CartItems.ToList();
            foreach(CartItem cartItem in cartItems)
            {
                await createOrderItemAsync(cart, cartItem, order.OrderId);
                await removeCartItemAsync(cart, cartItem);
            }

            await SaveAsync();

            await _context.Entry(order).Collection(o => o.OrderItems).LoadAsync();
            return order;
        }

        public async Task RemoveOrderAsync(Order order)
        {
            _context.Orders.Remove(order);
            await SaveAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

    }
}
