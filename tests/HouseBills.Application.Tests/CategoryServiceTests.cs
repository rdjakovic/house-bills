using HouseBills.Application.Categories;
using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HouseBills.Application.Tests;

public sealed class CategoryServiceTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repository);
    }

    [Fact]
    public async Task SaveAsync_BlankName_ReturnsValidationErrorWithoutSaving()
    {
        var result = await _service.SaveAsync(new SaveCategoryRequest(null, "  ", null), TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Validation);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SaveAsync_DuplicateName_ReturnsValidationError()
    {
        _repository.NameExistsAsync("Utilities", null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.SaveAsync(new SaveCategoryRequest(null, " Utilities ", null), TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Validation);
        result.Error.Message.ShouldContain("already exists");
    }

    [Fact]
    public async Task SaveAsync_NewCategory_AddsTrimmedName()
    {
        var result = await _service.SaveAsync(new SaveCategoryRequest(null, " Garden ", null), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Is<Category>(c => c.Name == "Garden"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_ConcurrentUpdate_ReturnsConflict()
    {
        _repository.GetAsync(5, Arg.Any<CancellationToken>()).Returns(TestData.Persisted(new Category("Old"), 5));
        _repository.UpdateAsync(Arg.Any<Category>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException());

        var result = await _service.SaveAsync(new SaveCategoryRequest(5, "New", TestData.RowVersion), TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Conflict);
    }

    [Fact]
    public async Task SaveAsync_DeletedMeanwhile_ReturnsNotFound()
    {
        _repository.GetAsync(5, Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _service.SaveAsync(new SaveCategoryRequest(5, "New", TestData.RowVersion), TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_CategoryInUse_ReturnsValidationErrorWithoutDeleting()
    {
        _repository.IsInUseAsync(3, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.DeleteAsync(3, TestData.RowVersion, TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Validation);
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default, default!, TestContext.Current.CancellationToken);
    }
}