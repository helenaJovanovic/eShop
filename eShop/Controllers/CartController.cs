using eShop.Authorization;
using eShop.Entities;
using eShop.Helpers;
using eShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Controllers
{ 
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CartController: ControllerBase
    {
        private readonly DataContext _context;
        private IHttpContextAccessor _httpContextAccessor;

        public CartController(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        User getUser()
        {
            User user = (User)_httpContextAccessor.HttpContext.Items["User"];

            if (user == null)
            {
                throw new KeyNotFoundException("Unknown user");
            }

            return user;
        }

        [Authorize(Role.User)]
        [HttpGet]
        public async Task<ActionResult<Cart>> GetCart()
        {
            User user = getUser();

            var userCart = await _context.Carts.Where(c => c.UserId == user.UserId).FirstOrDefaultAsync();

            await _context.Entry(userCart).Collection(s => s.CartItems).LoadAsync();

            return userCart;
        }

        [Authorize(Role.User)]
        [HttpPost]
        public async Task<ActionResult<CartItem>> addItemToCart(AddItemRequest req)
        {
            //get user and user cart
            User user = getUser();
            Cart cart = await _context.Carts.Where(c => c.UserId == user.UserId).FirstOrDefaultAsync();
            await _context.Entry(cart).Collection(s => s.CartItems).LoadAsync();

            //check if item is already in the cart
            //in that case quantity should be modified
            foreach(var ci in cart.CartItems)
            {
                if(ci.ItemId == req.ItemId)
                {
                    throw new AppException("Cannot add an item that is already in the cart");
                }
            }
            //find if item with id exists
            Item item = await _context.Items.FindAsync(req.ItemId);
            if (item == null)
            {
                throw new KeyNotFoundException("No such item id");
            }

            //make a cart item for a cart based on itemId
            CartItem cartItem = new CartItem { CartId = cart.CartId, ItemId = req.ItemId, Quantity = req.Quantity };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            //update the total price of items in the cart
            float Price = item.Price;
            cart.Total += cartItem.Quantity*Price;

            //update the total price of items in the cart
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCart), new { id = cart.CartId }, cart);

        }

        /*
            Find item delete it from the cart if the current user is the cart owner.
            Then substract the item from the total price
         */
        [Authorize(Role.User)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if(cartItem == null)
            {
                throw new KeyNotFoundException("Cart Item doesn't exist");
            }

            User user = getUser();
            Cart cart = await _context.Carts.FindAsync(cartItem.CartId);
            if(cart.UserId != user.UserId)
            {
                return Unauthorized();
            }

            _context.CartItems.Remove(cartItem);

            Item item = await _context.Items.FindAsync(cartItem.ItemId);
            cart.Total -= item.Price * cartItem.Quantity;

            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
