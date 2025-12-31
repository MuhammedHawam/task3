using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;


namespace PartnersHub.Synergy.Application.Opportunities.Commands
{
    public class ApproveOpportunityCommandHandler : IRequestHandler<ApproveOpportunityCommand, Result>
    {
        private readonly IOpportunityRepository _opportunityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public ApproveOpportunityCommandHandler(IOpportunityRepository opportunityRepository,
            IUnitOfWork unitOfWork, IUserService userService)
        {
            _userService = userService;
            _opportunityRepository = opportunityRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ApproveOpportunityCommand request, CancellationToken cancellationToken)
        {
            var opportunity = await _opportunityRepository.GetByIdAsync(request.OpportunityId);
            if (opportunity == null)
                return Result.Failure("Opportunity doesn't exist");

            var result = opportunity.ApproveByAssetManager(_userService.CurrentUserId, opportunity.Title.Value, opportunity.CompanyName, opportunity.UserEmail);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            _opportunityRepository.Update(opportunity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
