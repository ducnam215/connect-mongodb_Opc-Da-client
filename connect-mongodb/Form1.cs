using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Bson.Serialization.Attributes;
using OPCAutomation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace connect_mongodb
{
    public partial class Form1 : Form
    {
       
      
        public static IMongoClient client = new MongoClient("mongodb://127.0.0.1");

        public static IMongoDatabase db = client.GetDatabase("mydatabase5");

        public static IMongoCollection<Resident> collection = db.GetCollection<Resident>("mycol55");


        public OPCServer MyOPCServer = new OPCServer();
        public OPCGroup My_OPCGroup = null;
        OPCItem MyOPCItem;
        int[] MyServerHandles = new int[2];
        object[] MyValues = new object[2];
        public Array MyErrors;

        int ItemCount;
        Array OPCItemIDs = Array.CreateInstance(typeof(string), 10);
        Array ItemServerHandles = Array.CreateInstance(typeof(Int32), 10);
        Array ItemServerErrors = Array.CreateInstance(typeof(Int32), 10);
        Array ClientHandles = Array.CreateInstance(typeof(Int32), 10);
        Array RequestedDataTypes = Array.CreateInstance(typeof(Int16), 10);
        Array AccessPaths = Array.CreateInstance(typeof(string), 10);
        Array WriteItems = Array.CreateInstance(typeof(string), 10);


        public Form1()
        {
            InitializeComponent();
            
        }

        public void readData()
        {
            List<Resident> list = collection.AsQueryable().ToList();

            dataGridView1.DataSource = list;

        }


        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readData();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {

                MyOPCServer.Connect(txtServerName.Text, txtServer.Text);
                My_OPCGroup = MyOPCServer.OPCGroups.Add("gp1");
                My_OPCGroup.IsActive = true;
                My_OPCGroup.IsSubscribed = true;
                My_OPCGroup.UpdateRate = 1000;    //1 second

                My_OPCGroup.DataChange += new DIOPCGroupEvent_DataChangeEventHandler(My_OPCGroup_DataChange);
                


                ItemCount = 2;
                OPCItemIDs.SetValue("Channel6.Device1.Simulation.Counter", 1);
                ClientHandles.SetValue(1, 1);


                OPCItemIDs.SetValue("Channel6.Device1.Simulation.Random", 2);
                ClientHandles.SetValue(2, 2);

                My_OPCGroup.OPCItems.DefaultIsActive = true;

                My_OPCGroup.OPCItems.AddItems(ItemCount, ref OPCItemIDs, ref ClientHandles, out ItemServerHandles, out ItemServerErrors, RequestedDataTypes, AccessPaths);

                timer1.Enabled = !timer1.Enabled;
                timer2.Enabled = !timer2.Enabled;
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Connect! \r\n" + ex.Message);
            }
        }

        private void My_OPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                for (int i = 1; i <= NumItems; i++)
                {
                    if ((Convert.ToInt32(ClientHandles.GetValue(i)) == 1))
                    {
                        txtReadVal.Text = ItemValues.GetValue(i).ToString();
                    }
                    if ((Convert.ToInt32(ClientHandles.GetValue(i)) == 2))
                    {
                        txtWriteVal.Text = ItemValues.GetValue(i).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            My_OPCGroup.IsSubscribed = false;
            My_OPCGroup.IsActive = false;
            MyOPCServer.OPCGroups.RemoveAll();
            btnDisconnect.Enabled = false;
            btnConnect.Enabled = true;
            timer1.Enabled = !timer1.Enabled;
            timer2.Enabled = !timer2.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("dd/MM/yyyy hh:mm:ss tt");
            Resident resident = new Resident(formattedDateTime, "Int1", txtReadVal.Text);

            collection.InsertOne(resident);
            readData();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("dd/MM/yyyy hh:mm:ss tt");
            Resident resident = new Resident(formattedDateTime, "Int2", txtWriteVal.Text);

            collection.InsertOne(resident);
            readData();
        }
    }

    public class Resident
    {
        [BsonId]
        public ObjectId Id { get; set; }



        [BsonElement("Log")]
        public string Log { get; set; }

        [BsonElement("Tag")]
        public string Tag { get; set; }

        [BsonElement("Values")]
        public string Values { get; set; }

        



        public Resident ( string log, string tag, string value)
        {
            Log = log;
            Tag = tag;
            Values = value;

        }
        
    }
}
