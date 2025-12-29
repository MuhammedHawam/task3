using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Interfaces.Repository.Dapper;
using PartnersHub.Synergy.Application.Models;

using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;

using PartnersHub.Synergy.Domain.Common;
using OpportunityEntity = PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate.Opportunity;
using SynergyCompanyEntity = PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany;
namespace PartnersHub.Synergy.Application.SuccessStories.Queries
{

    public class SearchSuccessStoriesQueryHandler : IRequestHandler<SearchSuccessStoriesQuery, Result<PaginatedList<SuccessStoryResponseDto>>>
    {

        private readonly IDapperRepository _dapperSuccessStoryReadOnlyRepository;
        public SearchSuccessStoriesQueryHandler(IDapperRepository dapperSuccessStoryReadOnlyRepository)
        {

            _dapperSuccessStoryReadOnlyRepository = dapperSuccessStoryReadOnlyRepository;
        }

        public async Task<Result<PaginatedList<SuccessStoryResponseDto>>> Handle(SearchSuccessStoriesQuery request, CancellationToken cancellationToken)
        {
            // Parse status string to enum
            SuccessStoryStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (Enum.TryParse<SuccessStoryStatus>(request.Status, true, out var parsedStatus))
                {
                    statusEnum = parsedStatus;
                }
            }

            (List<SuccessStoryResponseDto> successStories, int count) = await _dapperSuccessStoryReadOnlyRepository.Search(
                pageNumber: request.PageNumber, 
                pageSize: request.PageSize,
                partnerCompanyName: request.PartnerCompanyName, 
                sectorIds: request.SectorIds, 
                startDate: request.StartDate, 
                endDate: request.EndDate, 
                status: statusEnum,
                searchTerm: request.SearchTerm,
                companyIds: request.CompanyIds, 
                collaborationTypeIds: request.CollaborationTypes, 
                sortBy: request.SortBy, 
                sortDescending: request.SortDescending);

            return Result<PaginatedList<SuccessStoryResponseDto>>.Success(new PaginatedList<SuccessStoryResponseDto>(successStories, count, request.PageNumber, request.PageSize));
        }


    }
    public class GetSuccessStoryByIdQueryHandler : IRequestHandler<GetSuccessStoryByIdQuery, Result<SuccessStoryResponseDto>>
    {
        private readonly IDapperRepository _dapperSuccessStoryReadOnlyRepository;

        public GetSuccessStoryByIdQueryHandler(IDapperRepository dapperSuccessStoryReadOnlyRepository)
        {
            _dapperSuccessStoryReadOnlyRepository = dapperSuccessStoryReadOnlyRepository;

        }

        public async Task<Result<SuccessStoryResponseDto>> Handle(GetSuccessStoryByIdQuery request, CancellationToken cancellationToken)
        {
            var successStoryResponseDto = await _dapperSuccessStoryReadOnlyRepository.GetByIdAsync(request.Id);



            if (successStoryResponseDto == null)
            {
                return Result<SuccessStoryResponseDto>.Failure("success story doesn't exist");
            }
            return Result<SuccessStoryResponseDto>.Success(successStoryResponseDto);
        }


    }
    public class GetSuccessStoriesByStatusQueryHandler : IRequestHandler<GetSuccessStoriesByStatusQuery, Result<List<SuccessStoryResponseDto>>>
    {
        private readonly IDapperRepository _dapperSuccessStoryReadOnlyRepository;

        public GetSuccessStoriesByStatusQueryHandler(IDapperRepository dapperSuccessStoryReadOnlyRepository)
        {
            _dapperSuccessStoryReadOnlyRepository = dapperSuccessStoryReadOnlyRepository;

        }

        public async Task<Result<List<SuccessStoryResponseDto>>> Handle(GetSuccessStoriesByStatusQuery request, CancellationToken cancellationToken)
        {
            var successStoryResponses = await _dapperSuccessStoryReadOnlyRepository.GetByStatusAsync(request.Status);



            if (successStoryResponses == null || successStoryResponses.Count <= default(int))
            {
                return Result<List<SuccessStoryResponseDto>>.Failure("success stories don't exist");
            }
            return Result<List<SuccessStoryResponseDto>>.Success(successStoryResponses);
        }


    }

}
