﻿namespace Centeva.ObjectStorage;

public class UpdateStorageEntryRequest
{
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
