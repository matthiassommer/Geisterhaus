using System;
using System.Windows.Forms;

namespace Geisterhaus
{
    public class BTC
    {
        Timer timer1; 
        Geisterhaus g;
        
        public BTC(Geisterhaus g)
        {
            BTConnect.Init();
            this.g = g;

            timer1 = new Timer();
            timer1.Interval = 50;
            timer1.Enabled = true;
            timer1.Tick += new System.EventHandler(this.timer1_Tick);
            timer1.Start();
        }

        int dev_id = 0;

        public void connect()
        {
            int count = BTConnect.UpdateDevices();
            if (count <= 0) return;

            BTDeviceInfo[] infos = new BTDeviceInfo[count];
            BTConnect.GetDeviceList(count, infos);

            // Try establishing / negotiating with the device
            for (int j = 0; j < count; j++)
            {
                if (BTConnect.SendHelloToDevice(j) != 1)
                {
                    BTConnect.DisconnectDevice(j);
                    dev_id = j;
                    continue;
                }
            }
        }

        public void msg1(int i)
        {       
            if (BTConnect.SendCaptureBitToDevice(dev_id, i) == 0)
                BTConnect.DisconnectDevice(dev_id);
        }

        public void disconnect()
        {
            BTConnect.DisconnectDevice(dev_id);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            char msg = BTConnect.GetReceivedMsg();
      
            switch (msg)
            {
                case 'S': g.gameScene.handleWiiInput(0); break;
                case 'D': g.gameScene.handleWiiInput(2); break;
                case 'K': g.gameScene.handleWiiInput(1); break;
                case 'V': g.gameScene.handleWiiInput(3); break;
                default: break;
            }
        }
    }
}
