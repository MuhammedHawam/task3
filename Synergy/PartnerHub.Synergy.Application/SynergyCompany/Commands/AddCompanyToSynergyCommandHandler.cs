using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.ValueObjects;

namespace PartnersHub.Synergy.Application.SynergyCompany.Commands;

/// <summary>
/// Handler for adding a company to Synergy from PIF middleware
/// </summary>
public class AddCompanyToSynergyCommandHandler : IRequestHandler<AddCompanyToSynergyCommand, Result<Guid>>
{
    private readonly ICompanyIntegrationService _integrationService;
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public AddCompanyToSynergyCommandHandler(
        ICompanyIntegrationService integrationService,
        ISynergyCompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _integrationService = integrationService;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<Result<Guid>> Handle(AddCompanyToSynergyCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch company from PIF middleware service
        var externalCompany = await _integrationService.GetCompanyByIdAsync(request.CompanyId);
        
        if (externalCompany == null)
        {
            return Result<Guid>.Failure($"Company not found in PIF system");
        }

        // 2. Check if company already exists in Synergy
        var existingCompany = await _companyRepository.GetByIdAsync(request.CompanyId, asNoTracking: true);
        if (existingCompany != null)
        {
            return Result<Guid>.Failure($"Company already exists in Synergy");
        }

        // 3. Prepare representative information
        // Prefer mobile over phone if both available
        string repName = externalCompany.Representative?.Name ?? "Not Provided";
        string repEmail = externalCompany.Representative?.Email ?? "not.provided@email.com";
        string repPhone = externalCompany.Representative?.Mobile 
            ?? externalCompany.Representative?.Phone 
            ?? "000-000-0000";
        string repJobTitle = externalCompany.Representative?.Position ?? "Not Provided";

        // 4. Prepare location information
        string country = externalCompany.Country ?? "Saudi Arabia";
        string city = externalCompany.City ?? "Riyadh";

        // 5. Create Synergy company entity using the static Create method
        var companyResult = Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany.Create(
            companyId: externalCompany.Id,
            name: externalCompany.Name,
            headquarterCountry: country,
            headquarterCity: city,
            description: externalCompany.Description ?? "No description provided",
            repName: repName,
            repPosition: repJobTitle,
            repEmail: repEmail,
            repPhone: repPhone,
            createdBy: _userService.CurrentUserId,
            logo: externalCompany.Logo);

        if (companyResult.IsFailure)
        {
            return Result<Guid>.Failure(companyResult.Error!);
        }

        var company = companyResult.Value!;

        // 6. Add sector if available
        if (externalCompany.SectorId.HasValue && !string.IsNullOrEmpty(externalCompany.SectorName))
        {
            var addSectorResult = company.AddSector(
                externalCompany.SectorId.Value, 
                externalCompany.SectorName);
            
            if (addSectorResult.IsFailure)
            {
                return Result<Guid>.Failure($"Failed to add sector: {addSectorResult.Error}");
            }
        }

        // 7. Save the company to database
        try
        {
            await _companyRepository.AddAsync(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(company.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to save company: {ex.Message}");
        }
    }
}
