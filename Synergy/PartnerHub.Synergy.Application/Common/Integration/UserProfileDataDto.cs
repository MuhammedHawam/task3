public class UserProfileDataDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public PositionDto Position { get; set; }
    public SectorDto Sector { get; set; }

}
public class PositionDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}
public class SectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } 
}

public class DataWrapper<T>
{
    public T Data { get; set; }
}
