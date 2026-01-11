using MediatR;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using System.Globalization;

namespace PartnersHub.Communities.Application.Communities.Commands;

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, Result<Guid>>
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOpportunityTypeRepository _opportunityTypeRepository;
    private readonly IThematicAreaRepository _thematicAreaRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUserService _userService;
    private readonly RequestCodeSettings _requestCodeSettings;
    private readonly IUserProfileDataIntegrationService _userProfileDataIntegrationService;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    public CreateOpportunityCommandHandler(IOpportunityRepository repository, IUnitOfWork unitOfWork,
        ISynergyCompanyRepository synergyCompanyRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository,
        IOpportunityRepository opportunityRepository,
        IOpportunityTypeRepository opportunityTypeRepository,
        ICollaborationRequirementRepository collaborationRequirementRepository,
        IThematicAreaRepository thematicAreaRepository,
        IUserService userService,
        IOptions<RequestCodeSettings> requestCodeSettings
        , IUserProfileDataIntegrationService userProfileDataIntegrationService



        )
    {
        _unitOfWork = unitOfWork;
        _synergyCompanyRepository = synergyCompanyRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
        _opportunityTypeRepository = opportunityTypeRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _thematicAreaRepository = thematicAreaRepository;
        _opportunityRepository = opportunityRepository;
        _userService = userService;
        _requestCodeSettings = requestCodeSettings.Value;
        _userProfileDataIntegrationService = userProfileDataIntegrationService;
    }

    public async Task<Result<Guid>> Handle(CreateOpportunityCommand command, CancellationToken cancellationToken)
    {
        var result = await ValidateCommand(command);
        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }
        var HasNoCollaboratedProfile = command.CollaboratedProfiles == null || command.CollaboratedProfiles.Count == 0;
        List<SynergyCompany> synergyCompanies = await _synergyCompanyRepository.GetByIdsAsync(command.CollaboratedProfiles);
        if (!HasNoCollaboratedProfile)
        {
            if (synergyCompanies == null || synergyCompanies.Count <= default(int) || command.CollaboratedProfiles.Count > synergyCompanies.Count)
                return Result<Guid>.Failure("Compaines not found");
        }

        List<ExpectedOutcome> expectedOutcomes = await _expectedOutcomesRepository.GetByIdsAsync(command.ExpectedOutcomes);
        var HasNoExpectedOutcomes = command.ExpectedOutcomes == null || command.ExpectedOutcomes.Count == 0;
        var HasExpectedOutcomesOther = !string.IsNullOrWhiteSpace(command.ExpectedOutcomeOther);
        if(HasNoExpectedOutcomes && HasExpectedOutcomesOther)
            return Result<Guid>.Failure("expectedoutcomes not found");
        if (!HasNoExpectedOutcomes)
        {
            if ((expectedOutcomes == null || expectedOutcomes.Count == 0)
                || (command.ExpectedOutcomes.Count > expectedOutcomes.Count))
                return Result<Guid>.Failure("expectedoutcomes not found");

        }

        List<CollaborationRequirement> collaborationRequirements = await _collaborationRequirementRepository.GetByIdsAsync(command.CollaborationRequirements);
        var HasNoCollaborationRequirments = command.CollaborationRequirements == null || command.CollaborationRequirements.Count == 0;
        var HasCollaborationRequirmentother = !string.IsNullOrWhiteSpace(command.CollaborationRequirementOther);
        if (HasNoCollaborationRequirments && HasCollaborationRequirmentother)
            return Result<Guid>.Failure("collaboration Requirements not found");
        if (!HasNoCollaborationRequirments)
        {
            if ((collaborationRequirements == null || collaborationRequirements.Count == 0)
                || (command.CollaborationRequirements.Count > collaborationRequirements.Count))
                return Result<Guid>.Failure("collaboration Requirements not found");

        }
        var userEmail = string.Empty;
        var userFullName = string.Empty;    
        var userMobile = string.Empty;
        var userPosition = string.Empty;
        if (command.IsAdmin != true)
        {
            UserProfileDataDto userProfileDataDto = await _userProfileDataIntegrationService.GetUserProfileData();
            if (userProfileDataDto == null)
            {
                return Result<Guid>.Failure("User data isn't found");
            }

             userEmail = userProfileDataDto?.Email;
             userFullName = FormFullName(userProfileDataDto.FirstName, userProfileDataDto.LastName);
             userMobile = userProfileDataDto?.Mobile;
             userPosition = userProfileDataDto?.Position?.Name;
        }
        else
        {
            userEmail = command.UserEmail;
            userFullName = command.UserFullName;
            userMobile = command.UserMobile;
        }
            ThematicArea thematicArea = await _thematicAreaRepository.GetById(command.ThematicAreaId);


        OpportunityType opportunityType = await _opportunityTypeRepository.GetById(command.TypeId);
        //SynergyCompany synergyCompany = await _synergyCompanyRepository.GetByIdAsync(_userService.CompanyId, includes: c => c.Sectors);
        //if (synergyCompany == null)
        //    return Result<Guid>.Failure("Company doesn't exist");



        await _lock.WaitAsync(cancellationToken);
        string nextCode;
        try
        {
            int nextRequestId = await _opportunityRepository.GetNextRequestIdAsync();

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


        var opportunity = Opportunity.Create(
            command.IsAdmin == true ? command.CompanyId : _userService.CompanyId,
            command.Title,
            command.Description,
            command.SectorName,
            command.SectorId,
            command.TypeId,
            command.ThematicAreaId,
            userEmail,
            userFullName,
            userMobile,
            userPosition,
            command.CollaborationRationale,
            nextCode,
            command.StartDate,
            command.EndDate,
            Guid.NewGuid(),
            _userService.CurrentUserId,
            command.IsAdmin,
            command.ContactName,
            command.ContactAddress,
            command.ContactMobile,
            command.CompanyName, userEmail);
        if (opportunity.IsFailure)
            return Result<Guid>.Failure(opportunity.Error);




        result = opportunity.Value.SetOpportunityType(opportunityType);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        result = opportunity.Value.AddCollaboratedCompanies(command.CollaboratedProfiles);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        result = opportunity.Value.AddCollaborationRequirement(collaborationRequirements, command.CollaborationRequirementOther);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        result = opportunity.Value.SetThematicArea(thematicArea);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        result = opportunity.Value.AddExpectedOutcome(expectedOutcomes, command.ExpectedOutcomeOther);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);


        result = opportunity.Value.Submit(_userService.CurrentUserId, command.Title, command.CompanyName);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        if (command.IsAdmin.HasValue && command.IsAdmin.Value) opportunity.Value.Publish(_userService.CurrentUserId, opportunity.Value.Title.Value, opportunity.Value.CompanyName, opportunity.Value.UserEmail, command.IsAdmin.Value);


        await _opportunityRepository.AddAsync(opportunity.Value);
        await _unitOfWork.SaveChangesAsync();

        return Result<Guid>.Success(opportunity.Value.Id);
    }
    private async Task<Result> ValidateCommand(CreateOpportunityCommand command)
    {

        if (!await _opportunityTypeRepository.ExistsAsync(command.TypeId))
        {
            return Result.Failure("Opportunity Type does not exist");
        }

        if (!await _thematicAreaRepository.ExistsAsync(command.ThematicAreaId))
        {
            return Result.Failure("Thematic Area does not exist");
        }
        if (await _opportunityRepository.IsOpportunityWithTitleAndCompanyExistsAsync(command.Title,( command.IsAdmin == true ? command.UserCompanyId.Value : _userService.CompanyId)))
        {
            return Result.Failure("Opportunity with the same title exists");

        }

        return Result.Success();
    }
    private string FormFullName(string firstName , string lastName)
    {
        return string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) ? "" : firstName == null ? lastName : lastName == null ? firstName : firstName + " " + lastName;
    }
}