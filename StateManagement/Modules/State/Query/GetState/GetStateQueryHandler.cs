using MediatR;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Query.GetState;

public class GetStateQueryHandler : IRequestHandler<GetStateQuery, StateFullModelOut?>
{
    private readonly StateManagementDbContext _db;

    public GetStateQueryHandler(StateManagementDbContext db)
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
