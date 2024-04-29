namespace SaveIt.App.Domain.Models;
public class LockFileModel
{
    public required LockFileStatus Status { get; set; }
    public LockDetailsModel? LockDetails { get; set; }
}

public class LockDetailsModel
{
    public required Guid LockedByUserId { get; set; }
    public required string LockedByUsername { get; set; }
    public required DateTime LockedAt { get; set; }
}
