using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        /*
         * Items sorted acording to sortBy enum and then paginated
         * 0 PriceAscending
         * 1 PriceDescending
         * 2 NameAZ
         */
        async Task<List<Item>> PaginatedSortItemsAsync(int start, int len, SortBy sortBy, Expression<Func<Item, bool>> whereExpr)
        {
            List<Item> items = new List<Item>();

            if (sortBy == SortBy.PriceAscending)
            {
                items = await _context.Items.Where(whereExpr).OrderBy(i => i.Price).Skip(start).Take(len).ToListAsync();
            }
            else if (sortBy == SortBy.PriceDescending)
            {
                items = await _context.Items.Where(whereExpr).OrderByDescending(i => i.Price).Skip(start).Take(len).ToListAsync();
            }
            else if (sortBy == SortBy.NameAZ)
            {
                items = await _context.Items.Where(whereExpr).OrderBy(i => i.Name).Skip(start).Take(len).ToListAsync();
            }

            return items;
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
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsPag(int start, int len, [FromQuery]SortBy sortBy)
        {

            List<Item> items = await PaginatedSortItemsAsync(start, len, sortBy, x => true);

            return items;
        }

        // GET: /Items/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                throw new KeyNotFoundException("Item doesn't exist");
            }

            return item;
        }
        
        [AllowAnonymous]
        [HttpGet("namesearch/{name}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetSearchByName(string name, int start, int len, [FromQuery] SortBy sortBy)
        {
            var items = await PaginatedSortItemsAsync(start, len, sortBy, x => x.Name.Contains(name));

            return items;
        }

        [AllowAnonymous]
        [HttpGet("categorysearch/{categoryId}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetSearchByCategory(int categoryId, int start, int len, [FromQuery] SortBy sortBy)
        {
            var items = await PaginatedSortItemsAsync(start, len, sortBy, x => x.CategoryId == categoryId);

            return items;
        }

        [Authorize(Role.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, Item item)
        {
            if(id != item.ItemId)
            {
                throw new AppException("Parameter id doesn't match body id");
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
                    throw new KeyNotFoundException("Item doesn't exist");
                }
                else
                {
                    throw new Exception("Concurrency exception with updating item");
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
                throw new KeyNotFoundException("Item doesn't exist");
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
            if(item.CategoryId == 0 || item.CategoryId == null)
            {
                item.CategoryId = 1;
            }
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            
            //TODO: check created at action
            return CreatedAtAction(nameof(GetItem), new { id = item.ItemId }, item);
        }



        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }
    }
}
