# Frame
Similar to serialization.
You can recieve or send a structure with a stream. But you'l have a set of bytes. These methods convert it to usable structure. The fastest one is #5, the safest is #4 and the others can be used with dynamic-length structures whose require to be parced from the buffer. #1 is for understanding the computer possibilities.

Usage of the example:
1. Create a c# NET project.
2. Add:
```
using Fundamental;
static unsafe void Main(string[] args)
{
   new buffers().Test(1000000);
}
```
The usage of #5 is the most interesting.
Add to any structure the constructor and the function:
```
public NameOfYourStructure(byte[] B, uint offset = 0)
        {
            fixed (byte* ptr = &B[offset]) this = *(NameOfYourStructure*)ptr; //copy
        }
public int Send(NetworkStream nstream) { return new Frame<NameOfYourStructure>(this).Send(nstream); }
```
And now you can send it to any stream by calling
```
NameOfYourStructure.Send(NameOfYourStream);
```
Or you can get the structure from any buffer.
```
NameOfYourStructure name=new NameOfYourStructure(B);
```
<H1>! Be careful whith it's length. Always check it! This method works when the length of the structure is known!</H1>

You can also add an interface with a contract to your structure
```
public interface IHaveABuffer
    {
        byte[] ToBuffer();
    }
    
public struct YourStructureName: IHaveABuffer
    {
        public byte[] ToBuffer() { return new Frame<YourStructureName>(this).Buffer(); }   
    }
    
public void SendMessage<S>(ref IPEndPoint remote, S structure) where S:IHaveABuffer
    {
        byte[] data=structure.ToBuffer();
        try { point.Send(data, data.Length, remote); }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
```
<H1> Inline </H1>
The same as Frame but uses static functions. Works slower in Debug mode and saster in Release.

```
public struct YourStructureName: IHaveABuffer
    {
        public byte[] ToBuffer() { return Inline.Serialize<YourStructureName>(this); }   
    }
```

