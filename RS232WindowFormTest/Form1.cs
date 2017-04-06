using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//引入串口类
using System.IO.Ports;


namespace RS232WindowFormTest
{
    public partial class Form1 : Form
    {
        //声明一个串口类
        SerialPort sp = null;
        //打开串口标志位
        bool isOpen = false;
        //属性设置标志位
        bool isSetProperty = false;
        //十六进制显示标志位
        bool isHex = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;//设置窗体的最大尺寸
            this.MaximizeBox = false;//设置窗体的最大尺寸按钮不可用

            string[] str = SerialPort.GetPortNames();
            
            //设置串口选择框的数据
            if (str.Length==0)
            {
                cbxCOMPort.Items.Add("未检测到可用串口");
            }
            else
            {
                foreach(string element in str)
                {
                    cbxCOMPort.Items.Add(element);
                }
            }
            //设置默认串口位COM1
            cbxCOMPort.SelectedIndex = 0;

            //列出常用的波特率
            cbxBaudRate.Items.Add("1200");//0
            cbxBaudRate.Items.Add("2400");//1
            cbxBaudRate.Items.Add("4800");//2
            cbxBaudRate.Items.Add("9600");//3
            cbxBaudRate.Items.Add("19200");//4
            cbxBaudRate.Items.Add("38400");//5
            cbxBaudRate.Items.Add("43000");//6
            cbxBaudRate.Items.Add("56000");//7
            cbxBaudRate.Items.Add("57600");//8
            cbxBaudRate.Items.Add("115200");//9
            cbxBaudRate.SelectedIndex = 3;

            //列出常用停止位
            cbxStopBits.Items.Add("0");//0
            cbxStopBits.Items.Add("1");//1
            cbxStopBits.Items.Add("1.5");//1.5
            cbxStopBits.Items.Add("2");//2
            cbxStopBits.SelectedIndex = 1;

            //列出常用数据位
            cbxDataBits.Items.Add("8");//0
            cbxDataBits.Items.Add("7");//1
            cbxDataBits.Items.Add("6");//2
            cbxDataBits.Items.Add("5");//3
            cbxDataBits.SelectedIndex = 0;

            //列出奇偶校验位
            cbxParity.Items.Add("无");//0
            cbxParity.Items.Add("奇校验");//1
            cbxParity.Items.Add("偶校验");//2
            cbxParity.SelectedIndex = 0;

            //默认为Char显示
            rbnChar.Checked = true;




        }

        /// <summary>
        /// 检测哪些串口可以使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;//有可用的串口
            cbxCOMPort.Items.Clear(); //清空当前串口号中所有的串口名称

            string[] str = SerialPort.GetPortNames();

            if (str.Length==0)
            {
                cbxCOMPort.Items.Add("未检测到可用的串口");
                comExistence = false;
            }
            else
            {
                foreach(string element in str)
                {
                    cbxCOMPort.Items.Add(element);
                }
                comExistence = true;
            }

            if(comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;//默认添加第一个索引
            }
            else
            {
                MessageBox.Show("没有可以用的串口!", "错误提示");
            }
        }


        /// <summary>
        /// 检查串口设置是否正确
        /// </summary>
        /// <returns>true or false</returns>
        private bool checkPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "")
                return false;
            if (cbxBaudRate.Text.Trim() == "")
                return false;
            if (cbxDataBits.Text.Trim() == "")
                return false;
            if (cbxParity.Text.Trim() == "")
                return false;
            if (cbxStopBits.Text.Trim() == "")
                return false;

            return true;
        }

        /// <summary>
        /// 检查发送的数据是否为空
        /// </summary>
        /// <returns>true or false</returns>
        private bool checkSendData()
        {
            if (tbxSendData.Text.Trim() == "")
                return false;
            return true;
        }

        private void setPortProperty()
        {
            sp = new SerialPort();

            sp.PortName = cbxCOMPort.Text.Trim();//设置串口名;

            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());//设置串口的波特率;

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());//设置停止位

            //设置停止位
            if(f==0)
            {
                sp.StopBits = StopBits.None;
            }
            else if(f==1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if(f==1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            //设置数据位
            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());

            //设置奇偶校验位
            string s = cbxParity.Text.Trim();

            if(s.CompareTo("无")==0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None; 
            }

            //设置超时读取时间
            sp.ReadTimeout = -1;

            sp.RtsEnable = true;

            //接收缓冲区当有一个字节的话就触发接收函数，如果不加上这句话的话，那就有时候触发接收有时候都发了好多次了也没有触发接收，有时候延时现象特别严重；
            sp.ReceivedBytesThreshold = 1;

            //定时DataReceived事件，当串口收到数据后出发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceiver);

            if(rbnHex.Checked)
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }

        }


        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenCom_Click(object sender, EventArgs e)
        {
           if(isOpen==false)
            {
                if(!checkPortSetting())
                {
                    MessageBox.Show("串口未设置！", "错误提示");
                    return;
                }

                if(!isSetProperty)//串口未设置，设置串口
                {
                    setPortProperty();
                    isSetProperty = true;
                }

                //打开串口
                try
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";

                    //串口打开以后设置串口相关设置按钮不可用
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxStopBits.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                }
                catch(Exception)
                {
                    //打开失败，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或者已被占用！", "错误提示");
                    
                }
            }
           else//关闭串口
            {
                try
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";
                    cbxCOMPort.Enabled = true;
                    cbxBaudRate.Enabled = true;
                    cbxStopBits.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;
                }
                catch(Exception)
                {
                    MessageBox.Show("关闭串口时发生错误！", "错误提示");
                }
            }
        }

        /// <summary>
        /// 串口收到消息的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_DataReceiver(object sender, SerialDataReceivedEventArgs e)
        {
            //System.Threading.Thread.Sleep(100);//延时100毫秒，等待数据接收完毕

            //使用this.Invoke跨线程访问UI
            this.Invoke((EventHandler)(delegate
            {
                if(isHex==false)
                {
                    tbxRecvData.Text += sp.ReadLine();
                }
                else
                {
                    Byte[] receivedData = new Byte[sp.BytesToRead];
                    sp.Read(receivedData, 0, receivedData.Length);
                    string recvDataText = null;
                    for(int i=0;i<receivedData.Length-1;i++)
                    {
                        recvDataText += "0x" + receivedData[i].ToString("X2") + " ";
                    }
                    tbxRecvData.Text = recvDataText;
                }

                //丢弃缓存区数据
                sp.DiscardInBuffer();
            }));
        }

        /// <summary>
        /// 发送串口数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (isOpen)//写串口数据
            {
                if (!checkSendData())
                {
                    MessageBox.Show("请输入要发送的数据！", "错误提示");
                    return;
                }

                try
                {
                    sp.WriteLine(tbxSendData.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("发送串口数据失败！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开！", "错误提示");
                return;
            }



        }

        /// <summary>
        /// 清空数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
            tbxSendData.Text = "";
        }
    }
}
