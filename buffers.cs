using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Fundamental
{
    public class Frame<T> where T : unmanaged
    {
        internal readonly byte[] buffer;
        public unsafe Frame() {buffer = new byte[sizeof(T)]; }
        public Frame(int size_bytes) {buffer = new byte[size_bytes];}
        public unsafe Frame(T t) {buffer = new byte[sizeof(T)]; Copy(t);}
        public Frame(byte[] array, int size_bytes, uint offset_bytes = 0)
        {
            buffer = new byte[size_bytes];
            System.Buffer.BlockCopy(array, (int)offset_bytes, buffer, 0, size_bytes);
        }
        public unsafe void Copy(T any) {fixed (byte* ptr = &buffer[0]) *(T*)ptr = any;}
        public byte[] Buffer() { return buffer; }
        public int Length() {return buffer.Length; }
        public unsafe T GetStructure()
        {
            fixed (byte* ptr = &buffer[0]) return *(T*)ptr;
        }

       // public int Read(FileStream fstream) { if (fstream == null) return -1; return fstream.Read(buffer, 0, buffer.Length); }
       // public int Write(FileStream fstream) { if (fstream == null) return -1; fstream.Write(buffer, 0, buffer.Length); return 0; }
       // public int Send(NetworkStream nstream) { if (nstream == null) return -1; nstream.Write(buffer, 0, buffer.Length); return 0; }
    }
    
    public static class Inline
    {
        public unsafe static byte[] Serialize<T>(T any, uint offset = 0) where T : unmanaged
        {
            byte[] buffer = new byte[sizeof(T) + offset];
            fixed (byte* ptr = &buffer[offset]) { *(T*)ptr = any; }
            return buffer;
        }

        public unsafe static T Deserialize<T>(byte[] buffer, uint offset = 0) where T : unmanaged
        {
            if (buffer.Length + offset < sizeof(T)) return default;//throw new Exception("Deserializatoin failed!");
            fixed (byte* ptr = &buffer[offset]) { return *(T*)ptr; }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public unsafe struct Struct
    {
        [MarshalAs(UnmanagedType.I4)]
        public int in1;
        [MarshalAs(UnmanagedType.I4)]
        public int in2;
        [MarshalAs(UnmanagedType.I4)]
        public int out1;
        [MarshalAs(UnmanagedType.I4)]
        public int out2;

        public Struct(int _in1,int _in2){in1=_in1;in2=_in2;out1=0;out2=0;}
        public Struct(byte[] B, uint offset_bytes = 0) { fixed (byte* ptr = &B[offset_bytes]) this = *(Struct*)ptr; }
        public Frame<Struct> frame() { return new Frame<Struct>(this); }
    }

    public class Class : Extentions.Frame<Struct>
    {
        public Class() { }
        public Class(byte[] B, int size_bytes, uint offset_bytes = 0) : base(B, size_bytes, offset_bytes) { }
        public unsafe Class(Struct s) : base() { fixed (byte* ptr = &buffer[0]) *((Struct*)ptr) = s; }
        public unsafe Class(int _in1, int _in2, int _out1, int _out2) : base()
        {
            fixed (byte* ptr = &buffer[0])
            {
                Struct* x = (Struct*)ptr;
                x->in1=_in1;
                    x->in2=_in2;
            x->out1=_out1;
            x->out2=_out2;
            }
        }

        public unsafe int GetIn1() { fixed (byte* ptr = &buffer[0]) return ((Struct*)ptr)->in1; }
        public unsafe int GetIn2() { fixed (byte* ptr = &buffer[0]) return ((Struct*)ptr)->in2; }
        public unsafe void SetOut1(int out1) { fixed (byte* ptr = &buffer[0]) ((Struct*)ptr)->out1 = out1; }
        public unsafe void SetOut2(int out2) { fixed (byte* ptr = &buffer[0]) ((Struct*)ptr)->out2 = out2; }
    }

    public class buffers
    {
        public byte[] buffer_in = new byte[1000];
        public byte[] buffer_out;
        public  buffers(){for (int n=0;n<1000;n++) buffer_in[n]=(byte)n;}

        public unsafe void Test(int iterations){
            Stopwatch sw = new Stopwatch();
            Struct s = new Struct(100, 200);
            Class c;
            System.Threading.Thread.Sleep(3000);
            sw.Start();
    
            
            for (int n = 0; n < iterations; n++){
                s.out1=s.in1 + s.in2;
                s.out2=s.in2 - s.in1;
            }

            sw.Stop();
            Console.WriteLine("simple struct operation " + sw.ElapsedTicks.ToString());
            sw.Reset();
            System.Threading.Thread.Sleep(3000);
            sw.Start();

            //buffer get/set
            for (int n = 0; n < iterations; n++)
            {
                c=new Class(buffer_in, sizeof(Struct), 0);
                int t1 = c.GetIn1();
                int t2 = c.GetIn2();
                c.SetOut1(t1+t2);
                c.SetOut2(t2-t1);
                buffer_out=c.buffer;
            }

            sw.Stop();
            Console.WriteLine("buffer get/set " + sw.ElapsedTicks.ToString());
            sw.Reset();
            System.Threading.Thread.Sleep(3000);
            sw.Start();

            for (int n = 0; n < iterations; n++)
            {
                c = new Class(buffer_in, sizeof(Struct), 0);
                Struct t=c.GetStructure();
                t.out1= t.in1 + t.in2;
                t.out2= t.in1 - t.in2;
                c=new Class(t);
                buffer_out = c.buffer;
            }

            sw.Stop();
            Console.WriteLine("conversion to struct " + sw.ElapsedTicks.ToString());
            sw.Reset();
            System.Threading.Thread.Sleep(3000);
            sw.Start();

            for (int n = 0; n < iterations; n++)
            {
                s = new Struct();
                s.in1 = BitConverter.ToInt32(buffer_in, 0);
                s.in2 = BitConverter.ToInt32(buffer_in, 4);
                s.out1 = s.in1 + s.in2;
                s.out2 = s.in1 - s.in2;
                buffer_out=new byte[16];
                Buffer.BlockCopy(BitConverter.GetBytes(s.in1), 0, buffer_out, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(s.in2), 0, buffer_out, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(s.out1), 0, buffer_out, 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(s.out2), 0, buffer_out, 12, 4);
            }
            sw.Stop();
            Console.WriteLine("classic safe bitconverter " + sw.ElapsedTicks.ToString());
            sw.Reset();
            System.Threading.Thread.Sleep(3000);
            sw.Start();


            for (int n = 0; n < iterations; n++)
            {
                s = new Struct(buffer_in);
                s.out1 = s.in1 + s.in2;
                s.out2 = s.in1 - s.in2;
                buffer_out= s.frame().buffer;
            }
            sw.Stop();
            Console.WriteLine("template " + sw.ElapsedTicks.ToString());
            sw.Reset();
            System.Threading.Thread.Sleep(3000);
            sw.Start();


            for (int n = 0; n < iterations; n++)
            {
                s = Inline.Deserialize<Struct>(buffer_in);
                s.out1 = s.in1 + s.in2;
                s.out2 = s.in1 - s.in2;
                buffer_out = Inline.Serialize<Struct>(s);
            }
            sw.Stop();
            Console.WriteLine("inline " + sw.ElapsedTicks.ToString());
        }
    }
}
