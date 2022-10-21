using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TFUserver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DB 연결 테스트 : " + DBManager.Instance.ConnectionTest());

            CommandManager commandManager = new CommandManager();
            commandManager.startCommandListener();
            
            Console.WriteLine("===TripforU Server===");
            Console.WriteLine("start: 서버 실행\nstop: 서버 종료\nexit: 프로그램 종료\nsyncWP: 관광지 정보 삽입/갱신");
            Console.WriteLine("=====================");
        }
    }
}