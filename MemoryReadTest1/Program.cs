using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class MemoryRead
{
    const int readFlag = 0x0010;                    // 0x0010 for read access
    const string processName = "Diablo";
    const int charNameAddress = 0x00686588;         // Adress for current character name, 15 char long. Mirrored by 0x00646CE4
    const int currentLifeAddress = 0x006865D8;      // 0x006865D8 and E0 both hold the value of current life bitshifted to 4480
                                                    // 0x006865E8 - original funky memory address that is slightly off

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    public static void Main()
    {

        Process process = Process.GetProcessesByName(processName)[0];
        IntPtr processHandle = OpenProcess(readFlag, false, process.Id);

        int bytesRead = 0;
        byte[] bufferName = new byte[16]; // String, 15 letters long + \0.
        byte[] bufferLife = new byte[4];  // Stored as int, 4 bytes.
        
        StringBuilder testName = new StringBuilder();
        
        bool x = true;
        int i = 0;
         
        // Reads out name of active character
        ReadProcessMemory((int)processHandle, charNameAddress, bufferName, bufferName.Length, ref bytesRead);
        
        // Debug stuff
        //Console.WriteLine(Encoding.Latin1.GetString(bufferName)); // Latin1 instead of ASCII for correct encoding for now
        //Console.WriteLine("======================================");
        
        // Extracts everything before \0 
        while (x && i < bufferName.Length)
        {
            if ((char)bufferName[i] != 0)
            {
                testName.Append((char)bufferName[i]); // adding (char) also fixes the special character problem
                i++;
            }
            else
            {
                x = false;
            }
        }
        
        Console.WriteLine(testName.ToString());
        Console.WriteLine("============================");

        int life;
        int deaths = 0;
        do
        {
            ReadProcessMemory((int)processHandle, currentLifeAddress, bufferLife, bufferLife.Length, ref bytesRead);
            life = BitConverter.ToInt32(bufferLife); //Convert the byte array to int.

            
            Thread.Sleep(800);

            if (life == 0)
            {
                deaths++;
                Console.WriteLine($"You died... Number of deaths: {deaths}");
                while (life == 0)
                {
                    ReadProcessMemory((int)processHandle, currentLifeAddress, bufferLife, bufferLife.Length, ref bytesRead);
                    life = BitConverter.ToInt32(bufferLife);
                    Thread.Sleep(800);

                    if (life != 0)
                    {
                        break;
                    }
                }
            }

        } while (true);       
    }
}