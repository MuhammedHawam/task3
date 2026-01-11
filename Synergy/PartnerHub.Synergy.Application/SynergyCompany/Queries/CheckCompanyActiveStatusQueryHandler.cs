using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public class CheckCompanyActiveStatusQueryHandler
    : IRequestHandler<CheckCompanyActiveStatusQuery, Result<bool>>
{
    private readonly ISynergyCompanyRepository _companyRepository;

    public CheckCompanyActiveStatusQueryHandler(ISynergyCompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<bool>> Handle(
        CheckCompanyActiveStatusQuery request,
        CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(
            request.CompanyId,
            asNoTracking: true
        );

        if (company == null)
            return Result<bool>.Failure("Company does not exist.");

        return Result<bool>.Success(company.IsActive);
    }
}

