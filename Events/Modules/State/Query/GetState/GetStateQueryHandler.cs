using Events.Data;
using Events.Modules.State.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.State.Query.GetState;

public class GetStateQueryHandler : IRequestHandler<GetStateQuery, StateFullModelOut?>
{
    private readonly EventDbContext _db;

    public GetStateQueryHandler(EventDbContext db)
    {
        _db = db;
    }

    public async Task<StateFullModelOut?> Handle(GetStateQuery request, CancellationToken cancellationToken)
    {
        var state = await _db.States.SingleOrDefaultAsync(
            e => e.Key == request.Key,
            cancellationToken: cancellationToken
        );

        if (state is null)
        {
            return null;
        }

        return new StateFullModelOut
        {
            Key = state.Key,
            ReferenceVersion = state.ReferenceVersion,
            UpdatedAt = state.UpdatedAt,
            Payload64 = Convert.ToBase64String(state.Payload)
        };
    }
}
