using Receiver.Data;
using Receiver.Entities;

namespace Receiver.Repo;

public class ServiceBusRepo(DataContext context) : BaseRepo<SubscribeEntity>(context)
{
    private readonly DataContext _context = context;
}
