# Quick Reference: Unit Testing Setup Guide

## ✅ All Tests Passing: 80/80 Tests

## Created Test Files

| File | Tests | Coverage |
|------|-------|----------|
| **ProductServiceTests.cs** | 14 | CRUD operations, image validation, localization |
| **CartServiceTests.cs** | 17 | Cart management, stock validation, quantity checks |
| **ProductControllerTests.cs** | 15 | REST endpoints, authorization, HTTP responses |
| **CartControllerTests.cs** | 15 | Cart endpoints, error handling, status codes |
| **CategoryServiceTests.cs** | 11 | Category CRUD, product lists, localization |
| **FavouriteServiceTests.cs** | 8 | Favourite management, duplicate checks |

## How to Run Tests

### Run All Tests
```bash
cd /Users/rania/Desktop/Furniture/Furniture/backend
dotnet test Furniture.Tests/Furniture.Tests.csproj
```

### Run Specific Test File
```bash
dotnet test Furniture.Tests/Furniture.Tests.csproj --filter "ClassName=ProductServiceTests"
```

### Run Single Test
```bash
dotnet test Furniture.Tests/Furniture.Tests.csproj --filter "FullyQualifiedName~GetByIdAsync_WithValidId_ReturnsProductDetailsDto"
```

### Generate Coverage Report
```bash
dotnet test Furniture.Tests/Furniture.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Structure Example

Each test follows the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Setup mocks and test data
    var userId = "user-1";
    var mockService = new Mock<IProductService>();
    mockService.Setup(s => s.GetByIdAsync(1))
        .ReturnsAsync(new ProductDetailsDto { Id = 1 });

    // Act - Call the method being tested
    var result = await service.GetByIdAsync(1);

    // Assert - Verify the result
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
}
```

## Mocking Guidelines Used

### Service Mocking
```csharp
var mockUnitOfWork = new Mock<IUnitOfWork>();
var mockRepository = new Mock<IGenaricRepository<Product, int>>();

mockUnitOfWork
    .Setup(u => u.GetRepository<Product, int>())
    .Returns(mockRepository.Object);
```

### Specification Mocking
```csharp
mockRepository
    .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Product, int>>()))
    .ReturnsAsync(products);
```

### AutoMapper Mocking
```csharp
var mockMapper = new Mock<IMapper>();
mockMapper
    .Setup(m => m.Map<ProductDetailsDto>(product))
    .Returns(expectedDto);
```

### Controller Context Setup
```csharp
private void SetupControllerContext(string userId = "test-user", string role = "buyer")
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role)
    };

    var identity = new ClaimsIdentity(claims, "TestScheme");
    var principal = new ClaimsPrincipal(identity);
    
    var httpContext = new DefaultHttpContext { User = principal };
    _controller.ControllerContext = new ControllerContext
    {
        HttpContext = httpContext
    };
}
```

## Key Testing Patterns

### Testing Exceptions
```csharp
await Assert.ThrowsAsync<KeyNotFoundException>(
    () => service.DeleteAsync(invalidId)
);
```

### Testing with SetupSequence (Multiple Calls)
```csharp
mockRepository
    .SetupSequence(r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()))
    .ReturnsAsync(Enumerable.Empty<Favourite>())  // First call
    .ReturnsAsync(new List<Favourite> { favourite }); // Second call
```

### Verifying Mock Calls
```csharp
mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
```

## Test Categories

### ✅ Service Tests (50 tests)
- ProductService (14 tests)
- CartService (17 tests)
- CategoryService (11 tests)
- FavouriteService (8 tests)

### ✅ Controller Tests (30 tests)
- ProductController (15 tests)
- CartController (15 tests)

## Features Tested

### Data Validation
- ✅ Image count limits (max 5)
- ✅ Quantity validation (> 0)
- ✅ Stock availability checks
- ✅ Duplicate favorite prevention

### Localization
- ✅ English language support
- ✅ Arabic language support
- ✅ Fallback to English

### Authentication
- ✅ User ID from ClaimsPrincipal
- ✅ Role-based authorization
- ✅ Unauthorized access handling

### Error Handling
- ✅ Product not found exceptions
- ✅ Invalid quantity errors
- ✅ Stock insufficient errors
- ✅ Duplicate entry prevention

### CRUD Operations
- ✅ Create with validation
- ✅ Read by ID
- ✅ Update with verification
- ✅ Delete with cleanup

## Dependencies

All tests use:
- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library
- **ASP.NET Core Test Utilities** - Controller testing

## Notes

- No production code was modified
- No fake methods added to production classes
- All tests are compile-ready
- Compatible with .NET 9
- No external API calls made
- All mocking follows best practices
- Tests are independent and isolated

## Troubleshooting

### Tests not running?
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

### Need to debug a test?
Add breakpoint and run:
```bash
dotnet test --no-build
```

### Specific test failing?
Get more details with:
```bash
dotnet test --verbosity detailed
```

## Next Steps

1. Run the tests regularly in CI/CD pipeline
2. Add more edge case tests as needed
3. Monitor code coverage metrics
4. Keep mocks synchronized with actual implementations
5. Review and update tests when production code changes

