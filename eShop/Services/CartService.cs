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
    public class CartService : AbstractCartOrder
    {

        private User user;
        public CartService(DataContext context, IHttpContextAccessor httpContextAccessor)
            :base(context, httpContextAccessor)
        { 
            user = getUser();
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

        public async Task<Cart> addItemToCartAsync(AddToCartRequest req)
        {
            Cart cart = await GetUserCartAsync(user.UserId);
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

    }
}
