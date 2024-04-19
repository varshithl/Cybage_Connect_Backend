using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class BlogsOfUser
{
    public int BlogId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string BlogTitle { get; set; } = null!;

    public DateTime PublishedDateOfBlog { get; set; }

    public int? Likes { get; set; }

    public int? Comments { get; set; }

    public string? Content { get; set; }
}
