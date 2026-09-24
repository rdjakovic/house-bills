using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Categories;
using HouseBills.Domain;

namespace HouseBills.Wpf.ViewModels.Categories;

public sealed partial class CategoryEditorViewModel : EditorViewModel
{
    public CategoryEditorViewModel(CategoryDto? category)
    {
        Id = category?.Id;
        RowVersion = category?.RowVersion;
        Name = category?.Name ?? string.Empty;
        ResetValidation();
    }

    public int? Id { get; }

    public byte[]? RowVersion { get; }

    public override string Title => Id is null ? "New category" : "Edit category";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(Category.NameMaxLength)]
    public partial string Name { get; set; }

    public SaveCategoryRequest ToRequest() => new(Id, Name, RowVersion);
}