using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace KvHashHandler
{
    class Program
    {
        /*Notes  ***UseFul SQl queries***    
            Resets Auto_INCREMENT back to the lowest number. 
            ALTER TABLE kvs AUTO_INCREMENT = 1;

            Checks for duplicate hashes
            SELECT * FROM kvs WHERE hash IN ( SELECT hash FROM kvs GROUP BY hash HAVING count(*) > 1) ORDER BY hash;

            Deletes Duplicate hashes
            DELETE S1 FROM kvs AS S1 JOIN kvs AS S2 WHERE S1.id > S2.id AND S1.hash = S2.hash;
        */
        static void Main(string[] args)
        {

            string filePath = Application.StartupPath + "\\MySQLconfig.ini";

            if (!File.Exists(filePath))
            {
                throw new Exception("Settings file not found");
            }

            Utils.LoadedIni = new IniParsing(filePath);

            Global.host = Utils.GetSqlHostName();
            Global.Username = Utils.GetSqlUserName();
            Global.password = Utils.GetSqlPassword();
            Global.Database = Utils.GetSqlDatabase();

            Global.NumberOfHashes = Utils.GetNumberOfHashes();

            Utils.AppendText(string.Concat(new object[] {
            "<*> ", "User", " -> ", "mysql host: ", Global.host, "\n",
            "<*> ", "User", " -> ", "mysql UserName: ", Global.Username, "\n",
            "<*> ", "User", " -> ", "mysql password: ", Global.password, "\n",
            "<*> ", "User", " -> ", "mysql Database: ", Global.Database, "\n",
            "<*> ", "User", " -> ", "NumberOfHashes: ", Global.NumberOfHashes, "\n",
            }), ConsoleColor.DarkGray);
            Utils.AppendText(string.Concat(new object[] { "<!> ", " -> ", "KvHashHandler[ConfigInfo]", "\n" }), ConsoleColor.Green);
            //byte[] LastHash = new byte[0x4];
            for (int i = 0; i < Global.NumberOfHashes; i++)
            {

                byte[] hashA = new byte[0x4];
                byte[] KVfile = new byte[0];
                
                int fileSize = 0;
                //DummyKv.GenerateDummyKv(i);         

                if (File.Exists("bin/kv/" + i.ToString() + "/kv.bin"))
                {

                    KVfile = File.ReadAllBytes("bin/kv/" + i.ToString() + "/kv.bin");
                    fileSize = KVfile.Length;
                    Buffer.BlockCopy(KVfile.Skip(0x4).Take(0x4).ToArray(), 0x0, hashA, 0x0, 0x4);
                    //if (Utils.BytesToString(LastHash) == Utils.BytesToString(hashA))
                    //{
                        /*Needs work*/
                        //Console.Write("Duplicate hash found! Exiting...{0} in Folder {1}\n", Utils.BytesToString(hashA), i.ToString());
                        /*Error Check for duplicates*/
                        //break;
                    //};
                    Directory.CreateDirectory(string.Concat(new object[] { "../", Utils.BytesToString(hashA) }));
                    File.WriteAllBytes(string.Concat(new object[] { "../", Utils.BytesToString(hashA), "/kv.bin" }), KVfile);
                    Utils.AppendText(string.Concat(new object[] {
                        "<*> ", "User", " -> ", "Writing hash: ", Utils.BytesToString(hashA), "\n",
                        "<*> ", "User", " -> ", "Folder Count: ", i.ToString(), "\n",
                        "<*> ", "User", " -> ", "Sending hash to mysql: ", Utils.BytesToString(hashA), "\n",
                        }), ConsoleColor.DarkGray);
                    Utils.AppendText(string.Concat(new object[] { "<!> ", " -> ", "KvHashHandler[HashInfo]", "\n" }), ConsoleColor.Green);
                    //Buffer.BlockCopy(KVfile.Skip(0x4).Take(0x4).ToArray(), 0x0, LastHash, 0x0, 0x4);/*Copy Lasthash for Error Handling*/
                    MySQL.AddKVHash(Utils.BytesToString(hashA));

                }
                else {

                    Console.WriteLine("No file found in folder {0} :( \n", i.ToString());
                }
            }
            Console.WriteLine("Running SQL Hash Duplicate Check... \n");
            MySQL.HashDuplicateCheck();
            Console.WriteLine("Done! \n");
            Console.ReadKey();
        }
    }
}
