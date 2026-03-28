using IMDB.Core.Interfaces;
using IMDB.Data;
using IMDB.Data.Base;
using IMDB.Data.Models;

namespace IMDB.Core.Services
{
    public class ProducersService : EntityBaseRepositry<Producer>,IProducersService
    {
       
        public ProducersService(AppDbContext context) : base(context)
        {
            
        }
       
    }
    
}
