using MediatR;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.SuccessStories.Commands;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;

public class CreateSuccessStoryCommandHandler : IRequestHandler<CreateSuccessStoryCommand, Result<Guid>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly RequestCodeSettings _requestCodeSettings;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public CreateSuccessStoryCommandHandler(IOpportunityRepository opportunityRepository, 
        ISynergyCompanyRepository synergyCompanyRepository,
        ISuccessStoryRepository successStoryRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IOptions<RequestCodeSettings> requestCodeSettings
)
    {

        _synergyCompanyRepository = synergyCompanyRepository;
        _opportunityRepository = opportunityRepository;
        _successStoryRepository = successStoryRepository;
        _unitOfWork = unitOfWork;
        _userService = userService;
        _requestCodeSettings = requestCodeSettings.Value;
    }

    public async Task<Result<Guid>> Handle(CreateSuccessStoryCommand request, CancellationToken cancellationToken)
    {

      var HasNoCollaboratedProfile = request.CollaboratedProfiles == null || request.CollaboratedProfiles.Count == 0;
        List<SynergyCompany> synergyCompanies = await _synergyCompanyRepository.GetByIdsAsync(request.CollaboratedProfiles);
        if (!HasNoCollaboratedProfile)
        {
            if (synergyCompanies == null || synergyCompanies.Count <= default(int) || request.CollaboratedProfiles.Count > synergyCompanies.Count)
                return Result<Guid>.Failure("Synergy company doesn't exist");
        }


        await _lock.WaitAsync(cancellationToken);
        string nextCode;

        var isTitleExist = await _successStoryRepository.CheckTitleUniqueness(request.Title);
        if (isTitleExist)
            return Result<Guid>.Failure("Title already exist");
        try
        {
            int nextRequestId = await _successStoryRepository.GetNextRequestIdAsync();

            nextCode = _requestCodeSettings.GenerateCode(nextRequestId);
        }
        catch (OperationCanceledException)
        {
            return Result<Guid>.Failure("Operation was cancelled");
        }
        finally
        {
            _lock.Release();
        }
        var successStoryResult = SuccessStory.Create(( request.IsAdmin == true ? request.UserCompanyId.Value : _userService.CompanyId),
            request.Title, request.Description,
            request.SuccessStoryTypeId,
            nextCode,
            request.StartDate,
            request.EndDate,
            request.SuccessStoryCollaborationStatusId,
            Guid.NewGuid(),
            _userService.CurrentUserId,
            request.SectorId,
            request.SectorName,
            request.UserEmail,request.IsAdmin);
        if (successStoryResult.IsFailure)
            return Result<Guid>.Failure(successStoryResult.Error);

        var result = successStoryResult.Value.AddCollaboratedCompanies(request.CollaboratedProfiles);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        result = successStoryResult.Value.AddAssociatedOpportunities(request.AssociatedOpportunities != null ? (new List<Guid> { request.AssociatedOpportunities.Value  }) : null);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        successStoryResult.Value.Submit(_userService.CurrentUserId, request.Title,request.CompanyName);

        if(request.IsAdmin.HasValue && request.IsAdmin.Value) successStoryResult.Value.Publish(_userService.CurrentUserId, successStoryResult.Value.Title.Value, successStoryResult.Value.CompanyName, successStoryResult.Value.UserEmail, request.IsAdmin.Value);

        await _successStoryRepository.AddAsync(successStoryResult.Value);
        await _unitOfWork.SaveChangesAsync();



        return Result<Guid>.Success(successStoryResult.Value.Id);

    }
}