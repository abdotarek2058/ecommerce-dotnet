using IMDB.Core.Interfaces;
using IMDB.Data;
using IMDB.Data.Base;
using IMDB.Data.Models;

namespace IMDB.Core.Services
{
    public class CinemasService :  EntityBaseRepositry<Cinema>,ICinemasService
    {
        
        public CinemasService(AppDbContext context) : base(context)
        {
            
        }
        
    }
}
