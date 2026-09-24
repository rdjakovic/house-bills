using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Categories;
using HouseBills.Domain;
using HouseBills.Presentation.Resources;

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

    public override string Title => Id is null ? Strings.Categories_EditorNewTitle : Strings.Categories_EditorEditTitle;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_NameRequired))]
    [MaxLength(Category.NameMaxLength, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_MaxLength))]
    public partial string Name { get; set; }

    public SaveCategoryRequest ToRequest() => new(Id, Name, RowVersion);
}