using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eShop.Helpers;
using eShop.Models;
using eShop.Authorization;
using eShop.Entities;

namespace eShop.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly DataContext _context;

        public ItemsController(DataContext context)
        {
            _context = context;
        }

        // GET: /Items
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            return await _context.Items.ToListAsync();
        }

        // GET: /Items/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                throw new AppException("Item doesn't exist");
            }

            return item;
        }
        
     

        [Authorize(Role.Admin)]
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {            
            //default behaviour if category is not specified
            //is to set categoryid to 1 which represents no category
            if(item.CategoryId == 0)
            {
                item.CategoryId = 1;
            }
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            

            return CreatedAtAction("GetItem", new { id = item.ItemId }, item);
        }


        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }
    }
}
