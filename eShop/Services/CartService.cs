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
    public class CartService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private User user;
        public CartService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            user = getUser();
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
        public async Task<Cart> GetUserCartAsync()
        {
            Cart cart = await _context.Carts.Where(c => c.UserId == user.UserId).FirstOrDefaultAsync();
            await _context.Entry(cart).Collection(s => s.CartItems).LoadAsync();
            return cart;
        }

        //check if item is in cart
        //must have references to CartItems loaded
        //(CartItem contains a reference to ItemId)
        public bool IsItemIdInCart(int ItemId, Cart cart)
        {
            foreach(var ci in cart.CartItems)
            {
                if(ci.ItemId == ItemId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCartItemInUserCart(int CartItemId, Cart cart)
        {
            foreach(var ci in cart.CartItems)
            {
                if(ci.CartItemId == CartItemId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<CartItem> createCartItemAsync(int CartId, AddItemRequest req)
        {
            CartItem cartItem = new CartItem { CartId = CartId, ItemId = req.ItemId, Quantity = req.Quantity };
            await _context.CartItems.AddAsync(cartItem);
            return cartItem;
        }

        public Cart changeTotal(Cart cart, float value)
        {
            cart.Total += value;
            return cart;
        }

        public async Task<Cart> addItemToCartAsync(AddItemRequest req)
        {
            Cart cart = await GetUserCartAsync();
            Item item = await _context.Items.FindAsync(req.ItemId);
            if (item == null)
            {
                throw new KeyNotFoundException("No such item id");
            }

            CartItem cartItem = await createCartItemAsync(cart.CartId, req);
            cart = changeTotal(cart, cartItem.Quantity * item.Price);

            _context.Entry(cart).State = EntityState.Modified;
            await SaveAsync();

            return cart;
        }
        public async Task changeItemAsync(Cart cart, CartItem cartItem)
        {
            _context.Entry(cartItem).State = EntityState.Modified;

            var cartItemOld = _context.Entry(cartItem).GetDatabaseValues();
            int oldQuantity = cartItemOld.GetValue<int>("Quantity");

            Item item = await _context.Items.FindAsync(cartItem.ItemId);
            cart = changeTotal(cart, item.Price * (cartItem.Quantity - oldQuantity));

            _context.Entry(cart).State = EntityState.Modified;
            await SaveAsync();

        }
        public async Task removeCartItemAsync(Cart cart, CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);

            Item item = await _context.Items.FindAsync(cartItem.ItemId);
            cart = changeTotal(cart, (-1) * item.Price * cartItem.Quantity);

            await SaveAsync();
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
