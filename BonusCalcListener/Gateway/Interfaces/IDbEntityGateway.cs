using BonusCalcListener.Domain;
using System;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway.Interfaces
{
    public interface IDbEntityGateway
    {
        Task<DomainEntity> GetEntityAsync(Guid id);
        Task SaveEntityAsync(DomainEntity entity);
    }
}
