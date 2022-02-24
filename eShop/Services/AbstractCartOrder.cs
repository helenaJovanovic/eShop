using eShop.Entities;
using eShop.Helpers;
using eShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Services
{
    //TODO: Rename acording to function
    public class AbstractCartOrder
    {
        protected readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AbstractCartOrder(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public User getUser()
        {
            User user = (User)_httpContextAccessor.HttpContext.Items["User"];

            if (user == null)
            {
                throw new KeyNotFoundException("Unknown user");
            }

            return user;
        }

        //get users cart
        public async Task<Cart> GetUserCartAsync(int UserId)
        {
            Cart cart = await _context.Carts.Where(c => c.UserId == UserId).FirstOrDefaultAsync();
            await _context.Entry(cart).Collection(s => s.CartItems).LoadAsync();
            return cart;
        }

        public async Task<Cart> GetUserCartAsync()
        {
            User user = getUser();
            return await GetUserCartAsync(user.UserId);
        }

        public Cart changeTotal(Cart cart, float value)
        {
            cart.Total += value;
            return cart;
        }

        public async Task removeCartItemAsync(Cart cart, CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);

            Item item = await _context.Items.FindAsync(cartItem.ItemId);
            cart = changeTotal(cart, (-1) * item.Price * cartItem.Quantity);

            await SaveAsync();
        }

        public async Task<CartItem> createCartItemAsync(int CartId, AddItemRequest req)
        {
            CartItem cartItem = new CartItem { CartId = CartId, ItemId = req.ItemId, Quantity = req.Quantity };
            await _context.CartItems.AddAsync(cartItem);
            return cartItem;
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Concurrency exception while updating cart and cart items");
            }
        }
    }
}
