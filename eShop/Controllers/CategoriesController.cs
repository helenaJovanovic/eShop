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
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoriesController(DataContext context)
        {
            _context = context;
        }

        //Get all
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        //Get all paginated
        [AllowAnonymous]
        [HttpGet("pag/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int start, int len)
        {
            return await _context.Categories.OrderBy(c => c.Name).Skip(start).Take(len).ToListAsync();
        }


        //Get one
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new KeyNotFoundException("Category doesn't exist");
            }

            await _context.Entry(category).Collection(s => s.Items).LoadAsync();

            return category;
        }

        //Get categories that match given name
        [AllowAnonymous]
        [HttpGet("namesearch/{name}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Category>>> GetSearchByName(string name, int start, int len)
        {
            //Values of sortBy: 0, 1, 2
            List<Category> categories = await _context.Categories.Where(x => x.Name.ToLower().Contains(name))
                .OrderBy(c => c.Name).Skip(start).Take(len).ToListAsync();

            if (categories == null)
            {
                throw new KeyNotFoundException("No results");
            }

            return categories;
        }

        [Authorize(Role.Admin)]
        [HttpPost]
        public async Task<ActionResult<Category>> PostItem(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, category);
        }

        [Authorize(Role.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if(category == null)
            {
                throw new KeyNotFoundException("Category doesn't exist");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Role.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if(id != category.CategoryId)
            {
                throw new AppException("Parameter id doesn't match body id");
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!CategoryExists(category.CategoryId))
                {
                    throw new KeyNotFoundException("Category doesn't exist");
                }
                else
                {
                    throw new Exception("Concurrency exception with updating category");
                }
            }

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Items.Any(e => e.CategoryId == id);
        }

    }
}
