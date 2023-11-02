using MediatR;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data;
using StateManagement.Data.Entities;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Command.PostState;

public class PostStateCommandHandler : IRequestHandler<PostStateCommand, StateFullModelOut>
{
    private readonly StateManagementDbContext _db;

    public PostStateCommandHandler(StateManagementDbContext db)
    {
        _db = db;
    }

    public async Task<StateFullModelOut> Handle(PostStateCommand request, CancellationToken cancellationToken)
    {
        var state = await _db.States.SingleOrDefaultAsync(
            e => e.Key == request.Key,
            cancellationToken: cancellationToken
        );

        if (state is null)
        {
            state = new StateDto
            {
                Key = request.Key,
                ReferenceVersion = request.ReferenceVersion,
                UpdatedAt = request.UpdatedAt,
                Payload = Convert.FromBase64String(request.Payload64)
            };

            _db.States.Add(state);
        }
        else
        {
            state.ReferenceVersion = request.ReferenceVersion;
            state.UpdatedAt = request.UpdatedAt;
            state.Payload = Convert.FromBase64String(request.Payload64);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new StateFullModelOut
        {
            Key = request.Key,
            ReferenceVersion = request.ReferenceVersion,
            UpdatedAt = request.UpdatedAt,
            Payload64 = request.Payload64
        };
    }
}
