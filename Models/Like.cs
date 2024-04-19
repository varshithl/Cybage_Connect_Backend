using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class Like
{
    public int Id { get; set; }

    public int? ArticleId { get; set; }

    public int? BlogId { get; set; }

    public int? ProjectInsightId { get; set; }

    public int? LikedById { get; set; }
}
