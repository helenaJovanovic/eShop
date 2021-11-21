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

        //GET: /Items/pag/start/len with pagination
        [AllowAnonymous]
        [HttpGet("pag/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsPag(int start, int len)
        {
            return await _context.Items.Skip(start).Take(len).ToListAsync();
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
        
        [AllowAnonymous]
        [HttpGet("namesearch/{name}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetSearchByName(string name, int start, int len)
        {
            var items = await _context.Items.Where(x => x.Name.ToLower().Contains(name)).Skip(start).Take(len).ToListAsync();

            if(items == null)
            {
                return NotFound();
            }

            return items;
        }

        [AllowAnonymous]
        [HttpGet("categorysearch/{categoryId}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetSearchByCategory(int categoryId, int start, int len)
        {
            var items = await _context.Items.Where(x => x.CategoryId == categoryId).Skip(start).Take(len).ToListAsync();

            if(items == null)
            {
                return NotFound();
            }

            return items;
        }

        [Authorize(Role.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, Item item)
        {
            if(id != item.ItemId)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!ItemExists(item.ItemId))
                {
                    return NotFound();
                }
                else
                {
                    throw new AppException("Concurrency exception with updating item");
                }
            }

            return NoContent();
        }

        [Authorize(Role.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if(item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
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
