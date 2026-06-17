# Resource Booking Application

A .NET 10 web application for managing resource bookings across employees, featuring real-time conflict detection and a Blazor-based web UI.

## How to Run the Project

### Prerequisites
- .NET 10 SDK (specified in `global.json`)
- SQLite (auto-created on first run)

### Running the Application

1. **Restore dependencies and build:**
   ```bash
   dotnet build
   ```

2. **Run the API server** (from the `ResourceBooking.Api` directory):
   ```bash
   dotnet run
   ```
   - API runs on: `https://localhost:7000`
   - OpenAPI docs available at: `https://localhost:7000/openapi/v1.json`

3. **In a separate terminal, run the Blazor frontend** (from the `ResourceBooking.Blazor` directory):
   ```bash
   dotnet run
   ```
   - Frontend runs on: `https://localhost:7002`

4. **Access the application:**
   - Open `https://localhost:7002` in your browser
   - Navigate to the bookings, resources, and employees pages

### Database
- SQLite database (`resourcebooking.db`) is automatically created and migrated on first API startup
- Located at the root of the project

---

## Key Design Decisions

### 1. **Layered Architecture**
The solution follows a clean, layered approach:
- **ResourceBooking.Core**: Domain models and DTOs (shared business logic)
- **ResourceBooking.Api**: ASP.NET Core API with controllers, services, and repositories
- **ResourceBooking.Blazor**: WebAssembly UI for user interaction
- **ResourceBooking.Tests**: Unit tests for core business logic

**Rationale**: Separates concerns, enables code reuse between API and frontend, and makes testing easier.

### 2. **Repository Pattern with Entity Framework Core**
- Generic `IRepository<T>` interface for CRUD operations
- Specialized repositories (`IBookingRepository`, `IResourceRepository`, `IEmployeeRepository`) for complex queries
- EF Core with SQLite for data persistence

**Rationale**: Abstracts database access, making it easy to swap implementations or add caching later.

### 3. **Service Layer with Business Logic**
- `IBookingService`, `IResourceService`, `IEmployeeService` encapsulate business rules
- Services validate input and coordinate repository operations
- Conflict detection logic centralized in `BookingService`

**Rationale**: Keeps validation and business rules in one place, reusable across API endpoints and frontend operations.

### 4. **DTO (Data Transfer Object) Pattern**
- `BookingDto`, `ResourceDto`, `EmployeeDto` for API responses
- `CreateBookingDto` for inbound requests
- `BookingConflictResponse` for conflict details

**Rationale**: Decouples API contracts from internal models, allowing flexible model evolution without breaking clients.

### 5. **CORS Configuration**
- AllowAll policy for frontend-API communication
- Simplified for development; would be restricted in production

**Rationale**: Enables the Blazor frontend to call the API across different ports during development.

### 6. **Automatic Database Migration**
- Migrations run automatically at API startup
- No manual `dotnet ef database update` needed

**Rationale**: Simplifies deployment and ensures schema is always in sync.

---

## How Booking Conflicts Are Detected

### Conflict Detection Algorithm

Conflicts are detected at two levels:

#### 1. **Database-Level Query** (in `BookingRepository.GetOverlappingBookingsAsync`)
```csharp
// A booking overlaps if:
// - It's for the same resource
// - Its start time is before the new booking's end time
// - Its end time is after the new booking's start time
.Where(b => b.ResourceId == resourceId &&
            b.StartTime < endTime &&
            b.EndTime > startTime)
```

**Mathematical explanation**: Two time intervals `[start1, end1]` and `[start2, end2]` overlap if:
- `start1 < end2` AND `end1 > start2`

This handles all overlap scenarios:
- Complete containment: `[1:00, 5:00]` vs `[2:00, 4:00]` ✓
- Partial overlap: `[1:00, 3:00]` vs `[2:00, 4:00]` ✓
- Adjacent bookings: `[1:00, 2:00]` vs `[2:00, 3:00]` ✗ (no overlap)

#### 2. **Service-Level Validation** (in `BookingService.CheckConflictAsync`)
- Validates time range: `EndTime > StartTime`
- Verifies resource exists and is active
- Calls repository to find overlapping bookings
- Returns detailed `BookingConflictResponse` with:
  - Boolean flag (`IsConflict`)
  - Error message
  - List of conflicting bookings (for UI display)
- Excludes the current booking when updating (via `excludeBookingId` parameter)

### Response on Conflict
```json
{
  "isConflict": true,
  "message": "Resource is already booked during this time period. 2 conflicting booking(s) found.",
  "conflictingBookings": [
    { "id": 1, "resourceName": "Meeting Room A", "employeeName": "John Doe", ... },
    { "id": 2, "resourceName": "Meeting Room A", "employeeName": "Jane Smith", ... }
  ]
}
```

---

## Trade-Offs Due to 4-Hour Time Limit

Given the time constraint, the following decisions prioritized core functionality:

### 1. **Authentication & Authorization**
- **Trade-off**: No user authentication or role-based access control (RBAC)
- **Rationale**: Implemented generic booking functionality instead
- **Impact**: In production, would add auth middleware and claim-based authorization

### 2. **Minimal Frontend Styling**
- **Trade-off**: Bootstrap basic styling only; no custom design system
- **Rationale**: Focused on functionality over visual polish
- **Impact**: UI is functional but not visually polished

### 3. **No Update/Edit Booking Functionality**
- **Trade-off**: Bookings can only be created or deleted, not modified
- **Rationale**: Conflict detection works for creation; edit logic adds complexity
- **Impact**: Users cannot reschedule bookings—must delete and recreate

### 4. **Limited Error Handling**
- **Trade-off**: No retry logic, no graceful degradation for network failures
- **Rationale**: Frontend catches exceptions and displays them; no timeout recovery
- **Impact**: Transient failures aren't retried automatically

### 5. **No Pagination**
- **Trade-off**: All bookings loaded at once (no limit on list size)
- **Rationale**: Simpler API design; assumes reasonable dataset size
- **Impact**: Performance degrades if bookings table grows beyond ~10,000 records

### 6. **No Concurrent Conflict Prevention**
- **Trade-off**: No database-level locking or optimistic concurrency control
- **Rationale**: Relies on application logic; race conditions possible under high load
- **Impact**: Two simultaneous requests could both pass conflict check but create overlapping bookings

### 7. **Minimal Logging**
- **Trade-off**: Only basic controller-level logging; no structured logging
- **Rationale**: Focused on happy-path functionality
- **Impact**: Debugging production issues more difficult

### 8. **No Soft Deletes**
- **Trade-off**: Hard deletes only; audit trail not preserved
- **Rationale**: Simpler schema and logic
- **Impact**: Cannot recover deleted bookings or track history

---

## Future Improvements (With More Time)

### High Priority

1. **User Authentication & Authorization**
   - Implement OAuth2 / OpenID Connect
   - Add role-based access control (admin, manager, employee)
   - Ensure users can only see/modify their own bookings

2. **Booking Management Features**
   - Implement edit/update endpoints with conflict re-checking
   - Add soft deletes with audit timestamps (`DeletedAt`)
   - Implement booking status (pending, confirmed, cancelled)

3. **Conflict Prevention**
   - Add database-level unique constraints
   - Implement optimistic concurrency with `RowVersion`
   - Add pessimistic locking for simultaneous bookings

4. **API Improvements**
   - Add pagination with skip/take parameters
   - Implement filtering (by resource, employee, date range)
   - Add sorting options
   - Support bulk operations

### Medium Priority

5. **Enhanced Frontend**
   - Add a calendar view for visual booking management
   - Implement date/time pickers
   - Add real-time availability checking
   - Show conflict warnings before submission

6. **Notifications**
   - Email notifications for booking confirmations
   - Conflict alerts to resource managers
   - Reminder emails before bookings

7. **Reporting & Analytics**
   - Resource utilization reports
   - Employee booking history
   - Conflict frequency analytics
   - Peak booking times

8. **Advanced Features**
   - Recurring bookings
   - Booking approval workflows
   - Resource capacity management
   - Multi-resource bookings

### Low Priority

9. **DevOps & Performance**
   - Docker containerization
   - API caching (Redis)
   - Database indexing optimization
   - Load testing and performance profiling

10. **Testing & Quality**
    - Comprehensive integration tests
    - E2E tests with Selenium/Playwright
    - Performance benchmarks
    - API contract testing

---
