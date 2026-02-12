using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;
using PartnersHub.ConfigurationHub.Application.Middleware.Interfaces;
using PartnersHub.ConfigurationHub.Application.Middleware.Queries;

namespace PartnersHub.ConfigurationHub.Application.Middleware.QueryHandlers;

public class GetMiddlewareCompaniesQueryHandler 
    : IRequestHandler<GetMiddlewareCompaniesQuery, Result<PaginatedList<MiddlewareCompanyDto>>>
{
    private readonly IMiddlewareCompanyService _middlewareService;
    private readonly ILogger<GetMiddlewareCompaniesQueryHandler> _logger;

    public GetMiddlewareCompaniesQueryHandler(
        IMiddlewareCompanyService middlewareService,
        ILogger<GetMiddlewareCompaniesQueryHandler> logger)
    {
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<MiddlewareCompanyDto>>> Handle(
        GetMiddlewareCompaniesQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var requestDto = new MiddlewareCompanyRequestDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SearchText = request.SearchText,
                SectorIds = request.SectorIds,
                CityIds = request.CityIds
            };

            var result = await _middlewareService.GetCompaniesAsync(requestDto);
            return Result<PaginatedList<MiddlewareCompanyDto>>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching companies from middleware");
            return Result<PaginatedList<MiddlewareCompanyDto>>.Failure($"Failed to fetch companies from middleware: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching companies from middleware");
            return Result<PaginatedList<MiddlewareCompanyDto>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}

public class GetAllMiddlewareCompaniesQueryHandler
    : IRequestHandler<GetAllMiddlewareCompaniesQuery, Result<List<MiddlewareCompanyDto>>>
{
    private readonly IMiddlewareCompanyService _middlewareService;
    private readonly ILogger<GetAllMiddlewareCompaniesQueryHandler> _logger;

    public GetAllMiddlewareCompaniesQueryHandler(
        IMiddlewareCompanyService middlewareService,
        ILogger<GetAllMiddlewareCompaniesQueryHandler> logger)
    {
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Result<List<MiddlewareCompanyDto>>> Handle(
        GetAllMiddlewareCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _middlewareService.GetAllCompaniesAsync();
            return Result<List<MiddlewareCompanyDto>>.Success(companies);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching all companies from middleware");
            return Result<List<MiddlewareCompanyDto>>.Failure($"Failed to fetch companies from middleware: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all companies from middleware");
            return Result<List<MiddlewareCompanyDto>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
public class GetMiddlewareCompanyByIdQueryHandler 
    : IRequestHandler<GetMiddlewareCompanyByIdQuery, Result<MiddlewareCompanyDto>>
{
    private readonly IMiddlewareCompanyService _middlewareService;
    private readonly ILogger<GetMiddlewareCompanyByIdQueryHandler> _logger;

    public GetMiddlewareCompanyByIdQueryHandler(
        IMiddlewareCompanyService middlewareService,
        ILogger<GetMiddlewareCompanyByIdQueryHandler> logger)
    {
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Result<MiddlewareCompanyDto>> Handle(
        GetMiddlewareCompanyByIdQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await _middlewareService.GetCompanyByIdAsync(request.CompanyId);
            
            if (company == null)
            {
                return Result<MiddlewareCompanyDto>.Failure($"Company with ID {request.CompanyId} not found");
            }

            return Result<MiddlewareCompanyDto>.Success(company);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching company {CompanyId} from middleware", request.CompanyId);
            return Result<MiddlewareCompanyDto>.Failure($"Failed to fetch company from middleware: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching company {CompanyId} from middleware", request.CompanyId);
            return Result<MiddlewareCompanyDto>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}

public class GetMiddlewareCompanyBySectorIdQueryHandler
    : IRequestHandler<GetMiddlewareCompanyBySectorIdQuery, Result<List<MiddlewareCompanyDto>>>
{
    private readonly IMiddlewareCompanyService _middlewareService;
    private readonly ILogger<GetMiddlewareCompanyBySectorIdQueryHandler> _logger;

    public GetMiddlewareCompanyBySectorIdQueryHandler(
        IMiddlewareCompanyService middlewareService,
        ILogger<GetMiddlewareCompanyBySectorIdQueryHandler> logger)
    {
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Result<List<MiddlewareCompanyDto>>> Handle(
        GetMiddlewareCompanyBySectorIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await _middlewareService.GetCompanyBySectorIdAsync(request.SectorId);

            if (company == null)
            {
                return Result<List<MiddlewareCompanyDto>>.Failure($"Company with Sector ID {request.SectorId} not found");
            }

            return Result<List<MiddlewareCompanyDto>>.Success(company);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching company with sector {SectorId} from middleware", request.SectorId);
            return Result<List<MiddlewareCompanyDto>>.Failure($"Failed to fetch company from middleware: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching company with sector {SectorId} from middleware", request.SectorId);
            return Result<List<MiddlewareCompanyDto>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
public class GetMiddlewareSectorsQueryHandler 
    : IRequestHandler<GetMiddlewareSectorsQuery, Result<List<MiddlewareSectorDto>>>
{
    private readonly IMiddlewareCompanyService _middlewareService;
    private readonly ILogger<GetMiddlewareSectorsQueryHandler> _logger;

    public GetMiddlewareSectorsQueryHandler(
        IMiddlewareCompanyService middlewareService,
        ILogger<GetMiddlewareSectorsQueryHandler> logger)
    {
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Result<List<MiddlewareSectorDto>>> Handle(
        GetMiddlewareSectorsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var sectors = await _middlewareService.GetSectorsAsync();
            return Result<List<MiddlewareSectorDto>>.Success(sectors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching sectors from middleware");
            return Result<List<MiddlewareSectorDto>>.Failure($"Failed to fetch sectors from middleware: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching sectors from middleware");
            return Result<List<MiddlewareSectorDto>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
