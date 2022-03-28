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
using eShop.Services;

namespace eShop.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ItemService _its;
        public ItemsController(DataContext context, ItemService its)
        {
            _context = context;
            _its = its;
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

            List<Item> items = await _its.PaginatedSortItemsAsync(start, len, sortBy, x => true);

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
            var items = await _its.PaginatedSortItemsAsync(start, len, sortBy, x => x.Name.Contains(name));

            return items;
        }

        [AllowAnonymous]
        [HttpGet("categorysearch/{categoryId}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetSearchByCategory(int categoryId, int start, int len, [FromQuery] SortBy sortBy)
        {
            var items = await _its.PaginatedSortItemsAsync(start, len, sortBy, x => x.CategoryId == categoryId);

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

            await _its.SaveAsync();

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
            await _its.SaveAsync();

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
            await _its.SaveAsync();
            
            //TODO: check created at action
            return CreatedAtAction(nameof(GetItem), new { id = item.ItemId }, item);
        }
    }
}
