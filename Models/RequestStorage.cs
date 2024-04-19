using System;
using System.Collections.Generic;

namespace Cybage_Connect.Models;

public partial class RequestStorage
{
    public int RequestId { get; set; }

    public int? RequestSenderId { get; set; }

    public int? RequestReceiverId { get; set; }

    public string? ConnectionStatus { get; set; }

    public virtual UserRegistration? RequestReceiver { get; set; }

    public virtual UserRegistration? RequestSender { get; set; }
}
