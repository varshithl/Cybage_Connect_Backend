using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class ProjectInsightsOfUser
{
    public int ProjectInsightId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string ProjectTitle { get; set; } = null!;

    public DateTime PublishedDateOfProjectInsight { get; set; }

    public int? Likes { get; set; }

    public int? Comments { get; set; }

    public string? Tools { get; set; }

    public string? ProjectDescription { get; set; }

    public string? Duration { get; set; }
}
