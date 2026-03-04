# InfraBase + ConfigurationHub Performance Assessment

## Scope
- `InfraBase/*`
- `ConfigurationHub/*`

## Executive Summary
- Main scalability risks were caused by repeated lookup/network calls, N+1 permission queries, and duplicated mapping logic across handlers.
- High-impact refactors were implemented to reduce redundant work and improve read-path efficiency without changing business behavior.
- Additional improvements are still recommended (indexing and endpoint-level pagination safeguards) for large-scale growth.

---

## Findings and Actions

### 1) InfraBase: Repeated mapping + lookup work across handlers (HIGH)
**Problem**
- Dashboard/list handlers duplicated expensive mapping and lookup resolution logic.
- This increased maintenance cost and made performance fixes inconsistent.

**Action implemented**
- Introduced shared service:
  - `InfraBase/PartnersHub.InfraBase.Application/Common/Interfaces/IAssetListProjectionService.cs`
  - `InfraBase/PartnersHub.InfraBase.Infrastructure/Services/AssetListProjectionService.cs`
- Refactored handlers to reuse shared projection:
  - `GetAssetListQueryHandler`
  - `GetAssetsByStatusQueryHandler`
  - `DashboardQueryHandlers` (all dashboard variants)
- Registered service in DI:
  - `InfraBase/PartnersHub.InfraBase.Apis/Program.cs`

**Expected impact**
- Lower repeated CPU/network work per request.
- Stronger consistency and easier future optimization.

---

### 2) InfraBase: Lookup inefficiency for sector names (HIGH)
**Problem**
- Sector name resolution used by-id HTTP requests repeatedly.

**Action implemented**
- `ConfigurationLookupService` now resolves sector/subsector/asset-type/UOM names through cached in-memory lookup maps built from list endpoints.
- File:
  - `InfraBase/PartnersHub.InfraBase.Infrastructure/Services/ConfigurationLookupService.cs`

**Expected impact**
- Fewer external calls, lower latency, reduced load on ConfigurationHub lookup endpoints.

---

### 3) InfraBase: Read query overhead in repository methods (MEDIUM)
**Problem**
- Read-heavy methods used tracking where not needed.
- Multi-collection includes could produce expensive query shapes.
- History endpoint loaded full asset graph unnecessarily.

**Action implemented**
- Added `AsNoTracking()` to read-only list/count paths.
- Added `AsSplitQuery()` where multi-collection includes are needed.
- Added focused history loader and updated history handler:
  - `GetByIdWithHistoryAsync` in repository/interface
  - `GetAssetHistoryQueryHandler` uses focused loader
- Files:
  - `IAssetRepository.cs`
  - `AssetRepository.cs`
  - `GetAssetHistoryQueryHandler.cs`

**Expected impact**
- Lower change-tracker overhead and better query execution behavior under load.

---

### 4) InfraBase: Upload path memory pressure (HIGH)
**Problem**
- File uploads copied each file into `MemoryStream` before request.

**Action implemented**
- Stream files directly into multipart content.
- File:
  - `InfraBase/PartnersHub.InfraBase.Infrastructure/Services/MiddlewareIntegrationService.cs`

**Expected impact**
- Lower memory usage, reduced allocation pressure, better scalability for larger/multiple uploads.

---

### 5) ConfigurationHub: N+1 permission queries per user (HIGH)
**Problem**
- Permission checks iterated roles and queried permissions repeatedly (N+1).

**Action implemented**
- Added set-based repository method:
  - `GetPermissionNamesByUserIdAsync`
- `RoleService.UserHasPermissionAsync` and `GetUserPermissionsAsync` now use one set-based fetch.
- Files:
  - `IRolePermissionRepository.cs`
  - `RolePermissionRepository.cs`
  - `RoleService.cs`

**Expected impact**
- Fewer DB round-trips and lower authorization latency.

---

### 6) ConfigurationHub: Role-permission insert path was chatty (HIGH)
**Problem**
- Role-permission assignment did per-item existence checks.

**Action implemented**
- `AddAsync` in `RolePermissionRepository` now:
  - De-duplicates input IDs
  - Performs one existence check
  - Performs one batch insert
- Also switched read paths to `AsNoTracking()` where appropriate.

**Expected impact**
- Significant reduction in DB calls during bulk permission assignment.

---

### 7) ConfigurationHub: Registered company query inefficiencies (MEDIUM)
**Problem**
- Tracked reads, sync count, and heavier-than-needed query shape.

**Action implemented**
- Added `AsNoTracking()`
- Replaced sync count with `CountAsync()`
- Improved search filter and duplicate-check workflow
- Added null-safe delete behavior
- File:
  - `ConfigurationHub/ConfigurationHub.Infrastructure/Persistence/Repositories/RegisteredCompanyRepository.cs`

**Expected impact**
- Improved list endpoint performance and more predictable behavior under load.

---

### 8) ConfigurationHub: Middleware response double read (MEDIUM)
**Problem**
- Sector-based company fetch read response body twice.

**Action implemented**
- Parse JSON once with error handling (`JsonException`).
- File:
  - `ConfigurationHub/ConfigurationHub.Infrastructure/Services/MiddlewareCompanyService.cs`

**Expected impact**
- Lower allocations and cleaner response handling.

---

### 9) ConfigurationHub: EF config override risk for Permission/Module (MEDIUM)
**Problem**
- Configuration methods were not overriding base configuration method.

**Action implemented**
- Converted to `override` and invoked `base.Configure(builder)`.
- Files:
  - `PermissionConfiguration.cs`
  - `ProductConfiguration.cs` (ModuleConfiguration)

**Expected impact**
- More reliable EF model configuration application and schema consistency.

---

### 10) ConfigurationHub: LDAP response pagination in API output (MEDIUM)
**Problem**
- LDAP search returned full result list without applying requested page.

**Action implemented**
- Applied page validation and `Skip/Take` before building response.
- File:
  - `ConfigurationHub/ConfigurationHub.Infrastructure/Services/LdapUserService.cs`

**Expected impact**
- Smaller API payloads and better response time for large directories.

> Note: true LDAP server-side paging remains recommended as a follow-up task.

---

## Prioritized Team To-Do (Next Sprint)

### P0 (must do)
1. **Add safe caps/pagination strategy for `GetAssetsByStatus` and in-memory sort flows**
   - Current path can still fetch very large result sets (`int.MaxValue` usage in handlers).
2. **Add SQL indexes aligned with query patterns**
   - InfraBase likely candidates: `(CompanyId, Status, CreatedAt)`, `(CreatedBy, Status, CreatedAt)`, `AssetCode`.
   - ConfigurationHub likely candidates: whitelist and role/user query-specific indexes.
3. **Implement true LDAP server-side paging**
   - Replace full-search then in-memory page with LDAP paging controls.

### P1 (high value)
4. **Move asset-code generation to DB-backed strategy**
   - Current approach scans and parses many asset codes in memory.
5. **Introduce response caching for stable lookup data**
   - Add bounded IMemoryCache/Redis policy for lookup dictionaries across requests.

### P2 (quality/scalability hardening)
6. **Add performance regression tests**
   - Load tests for dashboard/list endpoints.
   - Benchmarks for permission checks and role-permission assignments.
7. **Add query telemetry**
   - Track SQL count/time and external call count per request (OpenTelemetry/App Insights).

---

## Verification Notes
- Build tooling (`dotnet`) is unavailable in the current execution environment, so compile/test commands could not be executed here.
- All changes were validated through code-level consistency checks and cross-file dependency review.
