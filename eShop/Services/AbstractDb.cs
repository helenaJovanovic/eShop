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
    //TODO: Rename acording to function
    public class AbstractDb
    {
        protected readonly DataContext _context;
        public AbstractDb(DataContext context)
        {
            _context = context;
        }

        //get users cart
        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Concurrency exception while updating cart and cart items");
            }
        }
    }
}
