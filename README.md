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
            fixed (byte* ptr = &B[offset]) this = *(NameOfYourStructure*)ptr;
        }
public int Send(NetworkStream nstream) { return new Frame<NameOfYourStructure>(this).Send(nstream); }
```
And now you can send it to any stream by calling. (to the network or to a file)
NameOfYourStructure.Send(NameOfYourStream);

Any connection has a buffer where a frame or a packet is. So you can get the structure as well.
NameOfYourStructure name=new NameOfYourStructure(B);

! Be careful whith it's length. Always check it! This method works when the length of the structure is known!
