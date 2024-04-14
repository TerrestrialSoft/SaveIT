namespace SaveIt.App.Domain.Models;
public class LockFileModel
{
    public required LockFileStatus Status { get; set; }
    public LockDetailsModel? LockDetails { get; set; }
}

public class LockDetailsModel
{
    public required string LockedBy { get; set; }
    public required DateTime LockedAt { get; set; }
}
