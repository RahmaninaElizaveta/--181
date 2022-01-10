using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using static System.Threading.Semaphore;

namespace WindowsFormsApp2
{

    public partial class Form1 : Form
    {
        private Machine _machines;
        private Miller[] _millers;
        private Loader _loader;
        private ManualResetEvent ItemNew1;
        private ManualResetEvent ItemNew2;
        private ManualResetEvent ItemNew3;
        private ManualResetEvent ItemNew4;
        private ManualResetEvent ItemReady;
        private ManualResetEvent evToLoaderItem;
        private ManualResetEvent MachineWork;

        private static Semaphore _pool;

        public Form1()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            ItemNew1 = new ManualResetEvent(false);
            ItemNew2 = new ManualResetEvent(false);
            ItemNew3 = new ManualResetEvent(false);
            ItemNew4 = new ManualResetEvent(false);
            ItemReady = new ManualResetEvent(false);
            MachineWork = new ManualResetEvent(false);
            evToLoaderItem = new ManualResetEvent(false);
            _pool = new Semaphore(1, 1);

            _millers = new Miller[4];
            _millers[0] = new Miller(ref people1, _pool, ItemNew1, ItemReady, MachineWork, evToLoaderItem);
            _millers[1] = new Miller(ref people2, _pool, ItemNew2, ItemReady, MachineWork, evToLoaderItem);
            _millers[2] = new Miller(ref people3, _pool, ItemNew3, ItemReady, MachineWork, evToLoaderItem);
            _millers[3] = new Miller(ref people4, _pool, ItemNew4, ItemReady, MachineWork, evToLoaderItem);

            _machines = new Machine(ref statusMachine, ref item, MachineWork, ItemReady);
            _loader = new Loader(ref loader, evToLoaderItem);
        }

        private void newItem(object sender, EventArgs e)
        {
            Button PushButtom = sender as Button;
            switch (PushButtom.Text)
            {
                case "Новая деталь 1":
                    ItemNew1.Set();
                    break;
                case "Новая деталь 2":
                    ItemNew2.Set();
                    break;
                case "Новая деталь 3":
                    ItemNew3.Set();
                    break;
                case "Новая деталь 4":
                    ItemNew4.Set();
                    break;
                default:
                    return;
            }
        }
    }
    interface ILoader
    {
        // погружчик
        void receivedItem();
        void loadedItem();
    }
    
    class Miller
    {
        public Thread Thrd;
        ManualResetEvent ItemNew;
        ManualResetEvent ItemReady;
        ManualResetEvent MachineWork;
        ManualResetEvent ItemToLoader;
        
        private Semaphore MachineIsBusy;
        private Label text;

        public Miller(  ref Label field, 
                        Semaphore MachineIsBusy, 
                        ManualResetEvent ItemNew, 
                        ManualResetEvent ItemReady, 
                        ManualResetEvent MachineWork, 
                        ManualResetEvent ItemToLoader  )
        {
            text = field;
            
            this.MachineIsBusy = MachineIsBusy;
            this.ItemNew = ItemNew;
            this.ItemReady = ItemReady;
            this.MachineWork = MachineWork;
            this.ItemToLoader = ItemToLoader;
            
            Thrd = new Thread(Run);
            Thrd.Start();
        }

        void Run()
        {
            while (true)
            {
                text.Text = "Ожидаю";
                
                ItemNew.WaitOne();                  // ждем нового события - новая деталь
                MachineIsBusy.WaitOne();            // ждем освобождения станка
                
                text.Text = "Деталь принял, загрузил в станок, работаю";
                MachineWork.Set();
                
                ItemReady.WaitOne();
                text.Text = "Деталь готова, отдаю погрузчику";
                ItemNew.Reset();
                ItemReady.Reset();
                MachineIsBusy.Release();
                
                ItemToLoader.Set();
            }
        }
    }

    class Loader : ILoader
    {
        public Thread Thrd;
        public ManualResetEvent evToLoader;
        public Label text;
        
        public Loader(ref Label field, ManualResetEvent evToLoader)
        {
            Thrd = new Thread(this.Run);
            this.evToLoader = evToLoader;
            text = field;
            Thrd.Start();
        }
        
        public void Run()
        {
            while (true)
            {
                evToLoader.WaitOne();
                // обработчик
                receivedItem();
                Thread.Sleep(1000);
                loadedItem();
                evToLoader.Reset();
            }
        }
        
        public void receivedItem()
        {
            text.Text = "Деталь принял, начинаю погрузку";
        }

        public void loadedItem()
        {
            text.Text = "Деталь отгружена";
        }
    }
    class Machine 
    {
        public Thread Thrd;
        public ManualResetEvent MachineIsBusy;
        public ManualResetEvent ItemReady;
        public Label text;
        public Button Thing;

        public Machine(ref Label field, ref Button Thing, ManualResetEvent MachineIsBusy, ManualResetEvent ItemReady)
        {
            Thrd = new Thread(Run);
            this.MachineIsBusy = MachineIsBusy;
            this.ItemReady = ItemReady;
            this.Thing = Thing;
            text = field;
            Thrd.Start();
        }

        public void Run()
        {
            while (true)
            {
                
                MachineIsBusy.WaitOne();
                int start = 126;
                int end = 265;
                int Y = 16;
                int X = start;
                int step = 1;
                Thing.Location = new Point(126, 16);
                // обработчик
                text.Text = "Станок изготавливает деталь";
                Thread.Sleep(5000);
                text.Text = "Деталь готова";
               
                while (X < end) 
                {
                    Thing.Location = new Point(X, Y);
                    X += step;
                    Thread.Sleep(5);
                }
               
                MachineIsBusy.Reset();
                ItemReady.Set();
            }
        }

      
    }
  
}
