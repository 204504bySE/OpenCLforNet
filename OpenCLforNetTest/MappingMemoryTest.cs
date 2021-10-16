using NUnit.Framework;
using System.Linq;

namespace OpenCLforNetTest
{
    public unsafe class MappingMemoryTest : TestBase
    {
        [Test]
        public void CreateTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                Assert.NotNull(mem.Context);
                Assert.AreEqual(100, mem.Size);
            }
        }

        [Test]
        public void ReadWriteTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                byte[] dat = Enumerable.Range(0, 100).Select(n => (byte)n).ToArray();
                byte[] ans = new byte[100];

                fixed (void* datPtr = dat)
                fixed (void* ansPtr = ans)
                {
                    mem.WriteUnsafe(CommandQueue, true, 0, dat.Length, datPtr).Dispose();

                    mem.ReadUnsafe(CommandQueue, true, 0, ans.Length, ansPtr).Dispose();
                    Assert.AreEqual(dat, ans);

                    mem.ReadUnsafe(CommandQueue, true, ansPtr).Dispose();
                    Assert.AreEqual(dat, ans);

                    mem.WriteUnsafe(CommandQueue, true, datPtr).Dispose();
                    Assert.AreEqual(dat, ans);
                }
            }
        }

        [Test]
        public void ReadPartTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                byte[] dat = Enumerable.Range(0, 100).Select(n => (byte)n).ToArray();
                byte[] ans = new byte[25];

                fixed (void* datPtr = dat)
                fixed (void* ansPtr = ans)
                {
                    mem.WriteUnsafe(CommandQueue, true, 0, dat.Length, datPtr).Dispose();

                    mem.ReadUnsafe(CommandQueue, true, 0, 25, ansPtr).Dispose();
                    Assert.AreEqual(dat.Skip(0).Take(25), ans);

                    mem.ReadUnsafe(CommandQueue, true, 25, 25, ansPtr).Dispose();
                    Assert.AreEqual(dat.Skip(25).Take(25), ans);

                    mem.ReadUnsafe(CommandQueue, true, 50, 25, ansPtr).Dispose();
                    Assert.AreEqual(dat.Skip(50).Take(25), ans);

                    mem.ReadUnsafe(CommandQueue, true, 75, 25, ansPtr).Dispose();
                    Assert.AreEqual(dat.Skip(75).Take(25), ans);
                }
            }
        }

        [Test]
        public void WritePartTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                byte[] dat = Enumerable.Range(0, 100).Select(n => (byte)n).ToArray();
                byte[] ans = new byte[100];

                fixed (byte* datPtr = dat)
                fixed (byte* ansPtr = ans)
                {
                    // 0-clear
                    mem.WriteUnsafe(CommandQueue, true, 0, ans.Length, ansPtr).Dispose();


                    mem.WriteUnsafe(CommandQueue, true, 0, 25, datPtr).Dispose();
                    mem.ReadUnsafe(CommandQueue, true, 0, 25, ansPtr).Dispose();
                    Assert.AreEqual(dat.Take(25), ans.Take(25));

                    mem.WriteUnsafe(CommandQueue, true, 25, 25, datPtr + 25).Dispose();
                    mem.WriteUnsafe(CommandQueue, true, 50, 25, datPtr + 50).Dispose();
                    mem.WriteUnsafe(CommandQueue, true, 75, 25, datPtr + 75).Dispose();
                    mem.ReadUnsafe(CommandQueue, true, ansPtr).Dispose();
                    Assert.AreEqual(dat, ans);
                }
            }
        }

        [Test]
        public void MappingTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                void* dtPtr;
                mem.MappingUnsafe(CommandQueue, true, 0, 100, out dtPtr);

                for (int i = 0; i < 100; ++i)
                {
                    ((byte*)dtPtr)[i] = (byte)i;
                }

                mem.UnMappingUnsafe(CommandQueue, dtPtr);

                byte[] ans = new byte[100];
                fixed (void* ansPtr = ans)
                {
                    mem.ReadUnsafe(CommandQueue, true, ansPtr).Dispose();
                    Assert.AreEqual(Enumerable.Range(0, 100).Select(n => (byte)n), ans);
                }
            }
        }

        [Test]
        public void MappingPartTest()
        {
            using (var mem = Context.CreateMappingMemory(100))
            {
                byte[] expected = Enumerable.Range(0, 100).Select(n => (byte)n).ToArray();

                void* dtPtr;
                mem.MappingUnsafe(CommandQueue, true, 0, 50, out dtPtr);
                for (int i = 0; i < 50; ++i)
                    ((byte*)dtPtr)[i] = (byte)i;
                mem.UnMappingUnsafe(CommandQueue, dtPtr);

                byte[] ans = new byte[100];
                fixed (void* ansPtr = ans)
                {
                    mem.ReadUnsafe(CommandQueue, true, ansPtr).Dispose();
                    Assert.AreEqual(expected.Take(50), ans.Take(50));
                }

                mem.MappingUnsafe(CommandQueue, true, 50, 50, out dtPtr);
                for (int i = 0; i < 50; ++i)
                    ((byte*)dtPtr)[i] = (byte)(i + 50);
                mem.UnMappingUnsafe(CommandQueue, dtPtr);

                fixed (void* ansPtr = ans)
                {
                    mem.ReadUnsafe(CommandQueue, true, ansPtr).Dispose();
                    Assert.AreEqual(expected, ans);
                }
            }
        }
    }
}
