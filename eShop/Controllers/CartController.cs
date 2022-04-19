using eShop.Authorization;
using eShop.Entities;
using eShop.Helpers;
using eShop.Models;
using eShop.Services;
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
        private readonly CartService _cartService;
        public CartController(DataContext context,
            CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        [Authorize(Role.User)]
        [HttpGet]
        public async Task<ActionResult<Cart>> GetCart()
        {
            User user = _cartService.getUser();

            var userCart = await _context.Carts.Where(c => c.UserId == user.UserId).FirstOrDefaultAsync();

            await _context.Entry(userCart).Collection(s => s.CartItems).LoadAsync();

            return userCart;
        }

        [Authorize(Role.User)]
        [HttpPost]
        public async Task<ActionResult<CartItem>> addItemToCart(AddToCartRequest req)
        {
            Cart cart = await _cartService.addItemToCartAsync(req);

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

            Cart cart = await _cartService.GetUserCartAsync();
            //is id in usercart
            if (!_cartService.IsCartItemInUserCart(id, cart))
            {
                return Unauthorized();
            }

            await _cartService.removeCartItemAsync(cart, cartItem);

            return NoContent();
        }

        [Authorize(Role.User)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, CartItem cartItem)
        {
            if (id != cartItem.CartItemId)
            {
                throw new AppException("Parameter id doesn't match body id");
            }

            bool exists = _context.CartItems.Any(ci => ci.CartItemId == id);
            if (exists == false)
            {
                throw new KeyNotFoundException("Cart Item doesn't exist");
            }

            Cart cart = await _cartService.GetUserCartAsync();
            //is id in usercart
            if (!_cartService.IsCartItemInUserCart(id, cart))
            {
                return Unauthorized();
            }

            await _cartService.changeItemAsync(cart, cartItem);

            return NoContent();

        }
    }

    
}
