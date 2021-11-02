using System.Threading.Tasks;

namespace BonusCalcListener.Infrastructure
{
    public class DbSaver : IDbSaver
    {
        private readonly BonusCalcContext _context;
        public DbSaver(BonusCalcContext context)
        {
            _context = context;
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
