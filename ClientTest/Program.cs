﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
       
        static void Main(string[] args)
        {
           
            CEnvir.StartClientServer();
            Console.ReadLine();
        }
    }
}
