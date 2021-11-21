﻿using eShop.Authorization;
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
            return await _context.Categories.Skip(start).Take(len).ToListAsync();
        }


        //Get one
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new AppException("Category doesn't exist");
            }

            return category;
        }

        //Get categories that match given name
        [AllowAnonymous]
        [HttpGet("namesearch/{name}/{start}/{len}")]
        public async Task<ActionResult<IEnumerable<Category>>> GetSearchByName(string name, int start, int len)
        {
            var categories = await _context.Categories.Where(x => x.Name.ToLower().Contains(name)).Skip(start).Take(len).ToListAsync();

            if (categories == null)
            {
                return NotFound();
            }

            return categories;
        }

        [Authorize(Role.Admin)]
        [HttpPost]
        public async Task<ActionResult<Category>> PostItem(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();


            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }

        [Authorize(Role.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if(category == null)
            {
                return NotFound();
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
                return BadRequest();
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
                    return NotFound();
                }
                else
                {
                    throw new AppException("Concurreny exception with updating category");
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
