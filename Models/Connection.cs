using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class Connection
{
    public int ConnectId { get; set; }

    public int? UserId { get; set; }

    public int? RqstCount { get; set; }

    public virtual UserRegistration? User { get; set; }
}
