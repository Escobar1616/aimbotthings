using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;
using System.Threading;
using System.Runtime.InteropServices;

namespace aimbotthings
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]

        static extern short GetAsyncKeyState(Keys vKey);


        #region Offsets

        string PlayerBase = "ac_client.exe+0x109B74";
        string EntityList = "ac_client.exe+0x110D90";
        string Health = ",0xF8";
        string x = ",0x4";
        string y = ",0x8";
        string z = ",0xc";
        string ViewY = ",0x44";
        string ViewX = ",0x40";


        #endregion






        Mem m = new Mem();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            int PID = m.GetProcIdFromName("ac_client");
            if (PID > 0)
            { 
                m.OpenProcess(PID);

                Thread AB = new Thread(Aimbot) { IsBackground = true };
                AB.Start();




            }
        }


        void Aimbot()
        { 
            while (true) 
            {
                if (GetAsyncKeyState(Keys.XButton2) < 0)
                {
                    var LocalPlayer = GetLocal();
                    var Players = GetPlayers(LocalPlayer);

                    Players = Players.OrderBy(o => o.Magnitude).ToList();

                    if (Players.Count != 0)
                    {
                        Aim(LocalPlayer, Players[0]);
                    }

                }
                





                Thread.Sleep(2);
            }
        }

        Player GetLocal()
        {
            var Player = new Player
            {
                x = m.ReadFloat(PlayerBase + x),
                y = m.ReadFloat(PlayerBase + y),
                z = m.ReadFloat(PlayerBase + z)



            };


            return Player;
        }


        float GetMag(Player player, Player entity)
        {
            float mag;


            mag = (float)Math.Sqrt(Math.Pow(entity.x - player.x, 2) + Math.Pow(entity.y - player.y, 2) + Math.Pow(entity.x - player.z, 2));



            return mag;
        
        }


        void Aim(Player Player, Player Enemy)
        { 
            float deltax = Enemy.x -Player.x;

            float deltay = Enemy.y -Player.y;

            float viewx = (float)(Math.Atan2(deltay, deltax) * 180 / Math.PI) + 90;

            float deltaz = Enemy.z -Player.z;

            double distance = Math.Sqrt(deltax * deltax + deltay * deltay);

            float viewy = (float)(Math.Atan2(deltaz, distance) * 180 / Math.PI);




            m.WriteMemory(PlayerBase + ViewX, "float", viewx.ToString());

            m.WriteMemory(PlayerBase + ViewY, "float", viewy.ToString());

        }


        List<Player> GetPlayers(Player local)
        { 
        
            var players = new List<Player>();

            for (int i = 0; i < 20; i++)
            {
                var CurrentStr = EntityList + ",0x" + (i * 0x4).ToString("x");


                var Player = new Player
                {
                    x = m.ReadFloat(CurrentStr + x),
                    y = m.ReadFloat(CurrentStr + y),
                    z = m.ReadFloat(CurrentStr + z),
                    Health = m.ReadInt(CurrentStr + Health),
                };

                Player.Magnitude = GetMag(local, Player);




                if (Player.Health > 0 && Player.Health < 102)
                    players.Add(Player);

            }





            return players;
        
        }
    }
}
