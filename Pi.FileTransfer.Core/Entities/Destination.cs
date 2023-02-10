namespace Pi.FileTransfer.Core.Entities;
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
