using System.ComponentModel.DataAnnotations;

namespace story_web.Models;

public class CategoryLinkViewModel
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class StoryCardViewModel
{
    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? AuthorName { get; set; }

    public string? CategoryName { get; set; }

    public string? PostStatus { get; set; }

    public int ChapterCount { get; set; }

    public int Views { get; set; }

    public double? Rating { get; set; }

    public bool IsFavourite { get; set; }

    public int? LastReadChapterId { get; set; }
}
public class RankingItemViewModel
{
    public int StoryId { get; set; }
    public string StoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Views { get; set; }
    public int FavouriteCount { get; set; }
}
public class HomeIndexViewModel
{
    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    public string Title { get; set; } = "Latest stories";

    public IReadOnlyList<CategoryLinkViewModel> Categories { get; set; } = Array.Empty<CategoryLinkViewModel>();

    public IReadOnlyList<StoryCardViewModel> Stories { get; set; } = Array.Empty<StoryCardViewModel>();
    public IReadOnlyList<RankingItemViewModel> TopViews { get; set; } = Array.Empty<RankingItemViewModel>();

    public IReadOnlyList<RankingItemViewModel> TopFavourites { get; set; } = Array.Empty<RankingItemViewModel>();
    public IReadOnlyList<StoryCardViewModel> LatestUpdatedStories { get; set; } = new List<StoryCardViewModel>();
}
public class CommentItemViewModel
{
    public int CommentId { get; set; }
    public string UserName { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime? PostedAt { get; set; }
}
public class StoryChapterListItemViewModel
{
    public int ChapterId { get; set; }

    public decimal? ChapterNumber { get; set; }

    public string ChapterName { get; set; } = string.Empty;

    public DateTime? PostedAt { get; set; }
}

public class StoryDetailsViewModel
{
    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? AuthorName { get; set; }

    public string? CategoryName { get; set; }

    public string? PostStatus { get; set; }

    public DateTime? PostedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int Views { get; set; }

    public int FavouriteCount { get; set; }

    public bool IsFavourite { get; set; }

    public bool CanManage { get; set; }

    public int? ContinueReadingChapterId { get; set; }

    public IReadOnlyList<StoryChapterListItemViewModel> Chapters { get; set; } = Array.Empty<StoryChapterListItemViewModel>();
    public IReadOnlyList<CommentItemViewModel> Comments { get; set; } = new List<CommentItemViewModel>();
    public IReadOnlyList<StoryCardViewModel> LatestUpdatedStories { get; set; } = new List<StoryCardViewModel>();
}

public class ChapterReaderViewModel
{
    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? AuthorName { get; set; }

    public int ChapterId { get; set; }

    public decimal? ChapterNumber { get; set; }

    public string ChapterName { get; set; } = string.Empty;

    public string? AISummary { get; set; }

    public string? AudioUrl { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? PreviousChapterId { get; set; }

    public int? NextChapterId { get; set; }

    public IReadOnlyList<StoryChapterListItemViewModel> Chapters { get; set; } = Array.Empty<StoryChapterListItemViewModel>();
}

public class LibraryPageViewModel
{
    public string Title { get; set; } = string.Empty;

    public string EmptyMessage { get; set; } = string.Empty;

    public IReadOnlyList<ProfileStoryItemViewModel> Items { get; set; } = Array.Empty<ProfileStoryItemViewModel>();
}

public class NotificationsPageViewModel
{
    public IReadOnlyList<NotificationItemViewModel> Items { get; set; } = Array.Empty<NotificationItemViewModel>();
}

public class AuthorRegistrationViewModel
{
    [Required]
    [Display(Name = "Bút danh")]
    [StringLength(100, MinimumLength = 2)]
    public string PenName { get; set; } = string.Empty;

    [Display(Name = "Tiểu sử")]
    [StringLength(1000)]
    public string? Bio { get; set; }

    [Display(Name = "URL Đại diện")]
    [StringLength(255)]
    [Url]
    public string? Avatar { get; set; }
}

public class StoryFormViewModel
{
    public int? StoryId { get; set; }

    [Required]
    [Display(Name = "Tên truyện")]
    [StringLength(200, MinimumLength = 2)]
    public string StoryName { get; set; } = string.Empty;

    [Display(Name = "Thể loại")]
    [Required]
    public List<int> SelectedCategories { get; set; } = new();

    public string? StoryStatus { get; set; }
    [Display(Name = "Bộ chữ truyện")]
    [DataType(DataType.Upload)]
    public IFormFile? ImageFile { get; set; }

    public string? CurrentImage { get; set; }

    [Display(Name = "Tóm tắt truyện (hiển thị trên trang chủ)")]
    [Required]
    [StringLength(4000, MinimumLength = 20)]
    public string? Description { get; set; }
}

public class WriterStoryListItemViewModel
{
    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? CategoryName { get; set; }

    public string? PostStatus { get; set; }

    public int ChapterCount { get; set; }

    public DateTime? ModifiedAt { get; set; }
}

public class WriterStoryEditorViewModel
{
    public string Title { get; set; } = string.Empty;

    public StoryFormViewModel Form { get; set; } = new();

    public IReadOnlyList<CategoryLinkViewModel> Categories { get; set; } = Array.Empty<CategoryLinkViewModel>();
}

public class ChapterFormViewModel
{
    public int? ChapterId { get; set; }

    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Số chương")]
    public decimal? ChapterNumber { get; set; }

    [Required]
    [Display(Name = "Tiêu đề chương")]
    [StringLength(250)]
    public string ChapterName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;
}

public class WriterStoryManageViewModel
{
    public int StoryId { get; set; }

    public string StoryName { get; set; } = string.Empty;

    public string? PostStatus { get; set; }

    public string? RejectReason { get; set; }

    public IReadOnlyList<StoryChapterListItemViewModel> Chapters { get; set; } = Array.Empty<StoryChapterListItemViewModel>();
    public List<int> SelectedCategories { get; set; } = new();
}
