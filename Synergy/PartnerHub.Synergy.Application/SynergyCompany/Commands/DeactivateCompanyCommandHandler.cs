using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Commands;

public class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, Result>
{
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public DeactivateCompanyCommandHandler(
        ISynergyCompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<Result> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);
        if (company == null)
            return Result.Failure("Company not found");

        var result = company.Deactivate(_userService.CurrentUserId);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _companyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
