using MediatR;

namespace Application.Application.Handlers.Profile.Queries.ListAllProfiles;

public class ListAllProfilesQueryRequest : IRequest<List<Domain.Entities.Profile>?>
{
    
}