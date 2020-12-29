using System;
using System.ServiceModel;

namespace Server
{
   class Program
   {
      static void Main(string[] args)
      {
         using(ServiceHost host = new ServiceHost(typeof(letsTalk.ChatService)))
         {
            host.Open();
            Console.WriteLine("Host is activated");
            Console.ReadLine();
         }
      }
   }
}
