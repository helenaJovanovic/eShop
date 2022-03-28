using eShop.Helpers;
using eShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eShop.Services
{
    public class ItemService: AbstractDb
    {
        public ItemService(DataContext dataContext): base(dataContext)
        {
        }
        /*
         * Items sorted acording to sortBy enum and then paginated
         * 0 PriceAscending
         * 1 PriceDescending
         * 2 NameAZ
         */
        public async Task<List<Item>> PaginatedSortItemsAsync(int start, int len, SortBy sortBy, Expression<Func<Item, bool>> whereExpr)
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

        public bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }
    }
}
