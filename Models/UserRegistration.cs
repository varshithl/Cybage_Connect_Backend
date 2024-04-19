using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class UserRegistration
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string? Email { get; set; }

    public long? MobileNumber { get; set; }

    public string? Designation { get; set; }

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual ICollection<FriendRequest> FriendRequestReceivers { get; set; } = new List<FriendRequest>();

    public virtual ICollection<FriendRequest> FriendRequestSenders { get; set; } = new List<FriendRequest>();

    public virtual ICollection<RequestStorage> RequestStorageRequestReceivers { get; set; } = new List<RequestStorage>();

    public virtual ICollection<RequestStorage> RequestStorageRequestSenders { get; set; } = new List<RequestStorage>();
}
