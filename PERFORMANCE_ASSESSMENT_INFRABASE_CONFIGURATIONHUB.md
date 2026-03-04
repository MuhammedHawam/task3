# Performance Review Notes  
## InfraBase + ConfigurationHub

Prepared for: Team Lead  
Prepared by: Engineering Review  

---

## 1) Quick Summary

I reviewed both InfraBase and ConfigurationHub with one goal: improve performance and scalability **without changing business logic**.

The biggest issues were:
- repeated lookup and mapping logic in multiple handlers
- N+1 database calls (especially around permissions)
- unnecessary memory usage in file upload flow
- some read queries doing extra work that can be avoided

I already implemented a first wave of refactors that are safe, targeted, and high impact.

---

## 2) What Was Improved (Already Implemented)

### A) InfraBase

#### A1. Unified asset list mapping in one shared service (High Impact)
**What I noticed:**  
The same heavy mapping logic was repeated in list and dashboard handlers.

**What I changed:**  
Created one shared projection service and reused it across handlers.

**Result:**  
Less duplicated code, fewer repeated calls, easier maintenance, and cleaner future optimization.

---

#### A2. Reduced lookup overhead for sector/subsector/asset type/UOM names (High Impact)
**What I noticed:**  
Lookup resolution was doing more network/search work than needed.

**What I changed:**  
Optimized lookup service to use cached in-memory maps built from lookup lists.

**Result:**  
Fewer repeated lookups, lower latency, and less pressure on upstream services.

---

#### A3. Optimized read paths in asset repository (Medium Impact)
**What I noticed:**  
Some read queries were tracking entities when they did not need to, and some include patterns were expensive.

**What I changed:**  
- Added `AsNoTracking()` on read-only query paths  
- Added `AsSplitQuery()` for multi-collection includes  
- Added a focused history loader (`GetByIdWithHistoryAsync`) so history endpoint does not load full details unnecessarily

**Result:**  
Lower read overhead and better behavior under load.

---

#### A4. Removed unnecessary memory buffering in file uploads (High Impact)
**What I noticed:**  
Files were copied into `MemoryStream` before upload.

**What I changed:**  
Stream files directly in multipart request content.

**Result:**  
Lower memory footprint and better scalability for large/multiple uploads.

---

### B) ConfigurationHub

#### B1. Removed N+1 permission queries (High Impact)
**What I noticed:**  
Permission checks were querying per role in loops.

**What I changed:**  
Added a set-based permission fetch (`GetPermissionNamesByUserIdAsync`) and used it in role service.

**Result:**  
Fewer DB round-trips and faster authorization checks.

---

#### B2. Optimized role-permission bulk assignment flow (High Impact)
**What I noticed:**  
Each permission assignment did separate existence checks.

**What I changed:**  
- deduplicate input list  
- one existence check  
- one batch insert

**Result:**  
Much more efficient permission assignment.

---

#### B3. Improved registered company list/add/delete query path (Medium Impact)
**What I noticed:**  
There were avoidable read costs (tracked reads, sync count) and duplicate-check inefficiencies.

**What I changed:**  
- `AsNoTracking()` for read lists  
- `CountAsync()` instead of sync count  
- improved duplicate check flow  
- null-safe delete behavior

**Result:**  
Cleaner behavior and better query efficiency.

---

#### B4. Cleaned middleware response handling (Medium Impact)
**What I noticed:**  
Response content was read/parsing path more than once in one flow.

**What I changed:**  
Single parse path with JSON exception handling.

**Result:**  
Less allocation and cleaner error handling.

---

#### B5. Fixed EF configuration override correctness (Medium Impact)
**What I noticed:**  
Some configuration classes were not overriding base config method correctly.

**What I changed:**  
Converted to proper `override` and called `base.Configure(...)`.

**Result:**  
More reliable EF model configuration behavior.

---

#### B6. Applied API-level pagination on LDAP search response (Medium Impact)
**What I noticed:**  
LDAP search returned full list and then wrapped it as paged response without slicing.

**What I changed:**  
Applied page validation with `Skip/Take` before returning.

**Result:**  
Smaller payloads and faster API response for big result sets.

> Note: True LDAP **server-side paging** is still recommended as next step.

---

## 3) Priority To-Do List (Recommended Next Sprint)

### P0 — Must Do
1. Add hard safety limits/pagination strategy for very large asset list paths (especially current `int.MaxValue` usage).
2. Add missing SQL indexes based on real filter/sort patterns.  
   Suggested start:
   - InfraBase: `(CompanyId, Status, CreatedAt)`, `(CreatedBy, Status, CreatedAt)`, `AssetCode`
3. Implement true LDAP server-side paging (not only in-memory page slicing).

### P1 — High Value
4. Move asset code generation to DB-backed strategy (avoid large in-memory scan/parse as data grows).
5. Add caching policy for stable lookup data (bounded memory cache / Redis if needed).

### P2 — Hardening
6. Add performance regression test suite for key list/dashboard/permission endpoints.
7. Add query and external-call telemetry (SQL duration/count + external API call timings).

---

## 4) Risk and Validation Notes

- I kept logic/functionality intact and focused only on performance-safe refactoring.
- Build command could not run in this environment because `.NET SDK` is not installed (`dotnet` not found).
- Changes were validated through code-level dependency and flow checks; full CI build/test should still run in pipeline.

---

## 5) Final Comment for Leadership

Current refactor wave already removes several immediate bottlenecks and code redundancies, and it puts both modules in a better position for scaling.  
If we complete the P0 items next (indexes, hard pagination controls, and true LDAP paging), we should see a clear improvement in response times and system stability under higher load.
