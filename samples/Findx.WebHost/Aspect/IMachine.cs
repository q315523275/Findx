namespace Findx.WebHost.Aspect;

public interface IMachine
{
    bool Buy(double price);

    void Purchase(int count);
}