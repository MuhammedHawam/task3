using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Opportunities.Commands
{
    public class SetOpportunityVisibilityCommandHandler
          : IRequestHandler<SetOpportunityVisibilityCommand, Result>
    {
        private readonly IOpportunityRepository _opportunityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public SetOpportunityVisibilityCommandHandler(
            IOpportunityRepository opportunityRepository,
            IUnitOfWork unitOfWork,
            IUserService userService)
        {
            _opportunityRepository = opportunityRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<Result> Handle(
            SetOpportunityVisibilityCommand request,
            CancellationToken cancellationToken)
        {



            var opportunity =
                await _opportunityRepository.GetByIdAsync(request.OpportunityId);

            if (opportunity == null)
                return Result.Failure("Partnership doesn't exist");

            if (opportunity.Status != OpportunityStatus.Published)
                return Result.Failure("Only published Partnership can be hidden.");

            Result result = opportunity.SetVisibility(request.Hide , _userService.CurrentUserId);
                

            if (result.IsFailure)
                return Result.Failure(result.Error);

            _opportunityRepository.Update(opportunity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);


            return Result.Success(request.Hide == true ? "Partnership has been successfully hidden." : "Partnership has been successfully unhidden.");

            

        }
    }
}
