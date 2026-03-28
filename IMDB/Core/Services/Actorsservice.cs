using IMDB.Core.Interfaces;
using IMDB.Data;
using IMDB.Data.Base;
using IMDB.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Core.Services
{
    public class Actorsservice : EntityBaseRepositry<Actor>,IActorsService
    {
       
        public Actorsservice(AppDbContext context) : base(context)
        {
            
        }
       
    }
}
