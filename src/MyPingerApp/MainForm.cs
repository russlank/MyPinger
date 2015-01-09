using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace MyPinger
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ListBox listOutput;
		private System.Windows.Forms.Timer timerRefresh;
		private System.Windows.Forms.Button btnReadCFG;

		//ArrayList loggers;

		MyPingersBundle m_PingersBundle;
		
		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnReadCFG = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.listOutput = new System.Windows.Forms.ListBox();
			this.timerRefresh = new System.Windows.Forms.Timer(this.components);
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnReadCFG);
			this.panel1.Controls.Add(this.btnStop);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(560, 40);
			this.panel1.TabIndex = 2;
			this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
			// 
			// btnReadCFG
			// 
			this.btnReadCFG.Location = new System.Drawing.Point(8, 8);
			this.btnReadCFG.Name = "btnReadCFG";
			this.btnReadCFG.Size = new System.Drawing.Size(72, 24);
			this.btnReadCFG.TabIndex = 12;
			this.btnReadCFG.Text = "Start";
			this.btnReadCFG.Click += new System.EventHandler(this.btnReadCFG_Click);
			// 
			// btnStop
			// 
			this.btnStop.Location = new System.Drawing.Point(80, 8);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(72, 24);
			this.btnStop.TabIndex = 11;
			this.btnStop.Text = "Stop";
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.listOutput);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 40);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(560, 389);
			this.panel2.TabIndex = 3;
			// 
			// listOutput
			// 
			this.listOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listOutput.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(177)));
			this.listOutput.HorizontalScrollbar = true;
			this.listOutput.ItemHeight = 18;
			this.listOutput.Location = new System.Drawing.Point(0, 0);
			this.listOutput.Name = "listOutput";
			this.listOutput.ScrollAlwaysVisible = true;
			this.listOutput.Size = new System.Drawing.Size(560, 382);
			this.listOutput.TabIndex = 2;
			// 
			// timerRefresh
			// 
			this.timerRefresh.Enabled = true;
			this.timerRefresh.Interval = 1000;
			this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(560, 429);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "MainForm";
			this.Text = "MyPinger";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

//		public void OnPingComplete( object aSender, PingResult aResult)
//		{
//			listOutput.Items.Add( aResult.ToString());
//		}

		public void OnPingStart( object aSender, MyPingResult aEventArgs)
		{
			listOutput.Items.Add( "PING");
			listOutput.Update();
		}

		public void OnPingResponse( object aSender, MyPingResult aEventArgs)
		{
			listOutput.Items.Add( "RESPONSE");
			listOutput.Items.Add( aEventArgs.ToString());
			listOutput.Items.Add( HexEncoding.ToString(aEventArgs.sentData));
			listOutput.Items.Add( HexEncoding.ToString(aEventArgs.receivedData));
			listOutput.Update();
		}

		public void OnPingTimeout( object aSender, MyPingResult aEventArgs)
		{
			listOutput.Items.Add( "TIMEOUT");
			listOutput.Items.Add( aEventArgs.ToString());
			listOutput.Update();
		}

//		private void btnPing_Click(object sender, System.EventArgs e)
//		{
//			if ((m_PingerThread.ThreadState & (ThreadState.Unstarted | ThreadState.Stopped)) != 0)
//			{
//				m_Pinger.SetPingLoopParameters( new System.Net.IPEndPoint( IPAddress.Parse(textDistanation.Text),0), 1000, 10);
//				listOutput.Items.Add( "Start pinging: " + textDistanation.Text);
//				m_PingerThread = new Thread( new ThreadStart(m_Pinger.PingLoop));
//				m_PingerThread.Start();
//			}
//		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			MyPinger mypinger = new MyPinger();
			mypinger.ClientIP = "127.0.0.1";
			mypinger.RepeatInterval = 10000;
			mypinger.Timeout = 3000;

			m_PingersBundle = new MyPingersBundle();
			
//			m_Pinger =  new Pinger( new System.Net.IPEndPoint( IPAddress.Parse("127.0.0.1"), 0), 1000, 1500);
//			m_Pinger.OnPingComplete = new PingCompleteEvent( this.OnPingComplete);
//			m_PingerThread = new Thread( new ThreadStart(m_Pinger.PingLoop));
		}

		private void btnMyPing_Click(object sender, System.EventArgs e)
		{
		}

		private void btnPingEx_Click(object sender, System.EventArgs e)
		{
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			this.m_PingersBundle.StopAllPingLoggers();
		}

		private void timerRefresh_Tick(object sender, System.EventArgs e)
		{
//			listOutput.Items.Clear();
//			
//			foreach( MyPingLogger pl in loggers)
//			{
//				listOutput.Items.Add( pl.Server);
//			}

		}

		private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
		
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.m_PingersBundle.StopAllPingLoggers();
		}

		private void btnReadCFG_Click(object sender, System.EventArgs e)
		{
			m_PingersBundle.ReadCFGFile( "C:\\MyPinger\\MyPingerCFG.xml");
		}
	}
}