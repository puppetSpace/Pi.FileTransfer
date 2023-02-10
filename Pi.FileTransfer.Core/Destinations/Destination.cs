namespace Pi.FileTransfer.Core.Destinations;
public class Destination
{
    public Destination(string name, string address)
    {
        Name = name;
        Address = address;
    }
    public string Name { get; private set; }

    public string Address { get; private set; }


}
