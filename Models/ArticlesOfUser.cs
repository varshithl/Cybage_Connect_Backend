using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class ArticlesOfUser
{
    public int ArticleId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string ArticleTitle { get; set; } = null!;

    public DateTime PublishedDate { get; set; }

    public int? Likes { get; set; }

    public int? Comments { get; set; }

    public string? Content { get; set; }
}
