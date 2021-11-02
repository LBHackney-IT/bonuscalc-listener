using System.Threading.Tasks;

namespace BonusCalcListener.Infrastructure
{
    public interface IDbSaver
    {
        public Task SaveChangesAsync();
    }
}
