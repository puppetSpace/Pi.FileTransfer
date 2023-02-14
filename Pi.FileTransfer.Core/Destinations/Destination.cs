namespace Pi.FileTransfer.Core.Destinations;
public class Destination
{
    public Destination(Guid id,string name, string address)
    {
        Id = id;
        Name = name;
        Address = address;
    }

    public Guid Id { get; }
    public string Name { get; }

    public string Address { get; }


}
