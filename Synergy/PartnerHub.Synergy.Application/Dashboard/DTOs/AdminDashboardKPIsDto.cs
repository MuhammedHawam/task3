using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Dashboard.DTOs
{
    public class AdminDashboardKPIsDto
    {
        public int TotalActiveCompaniesCount { get; set; }
        public int TotalSuccessStoriesCount { get; set; }
        public int PublishedSuccessStoriesCount { get; set; }
        public int TotalPendingApprovalSuccessStories { get; set; }
        public int TotalOpportunitiesCount { get; set; }
        public int PublishedOpportunitiesCount { get; set; }
        public int TotalPendingReviewOpportunitiesCount { get; set; }
        public List<SectorKPI> SectorsKPIs { get; set; }
        public List<CollaborationTypeKPI> CollaborationTypeKPIs { get; set; }
        public List<EngagementTrend> EngagementTrends { get; set; }
        public List<CompanyKPI> TopPerformingCompanies { get; set; }
        
    }
    public class SectorKPI
    {
        public string SectorName { get; set; }
        public Guid SectorId { get; set; }
        public int TotalOpportunitiesCount { get; set; }
        public int TotalSuccessStoriesCount { get; set; }
    }
    public class CollaborationTypeKPI
    {
        public string CollaborationTypeName { get; set; }
        public int CollaborationTypeId { get; set; }
        public int PublishedOpportunitiesCount { get; set; }
    }
    public class EngagementTrend
    {
        public int TotalSuccessStoriesCount { get; set; }
        public int TotalOpportunitiesCount { get; set; }
        public string Month { get; set; } 
    }
    public class CompanyKPI
    {
        public int TotalOpportunitiesCount { get; set; }
        public int TotalSuccessStoriesCount { get; set; }
        public string CompanyName { get; set; }
    }

}
