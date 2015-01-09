using System;
using System.Collections;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Xml;

namespace MyPinger
{
	/// <summary>
	/// Summary description for MyPingerCommon.
	/// </summary>
	/// 

	public class HexEncoding
	{
		public HexEncoding()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public static int GetByteCount(string hexString)
		{
			int numHexChars = 0;
			char c;
			// remove all none A-F, 0-9, characters
			for (int i=0; i<hexString.Length; i++)
			{
				c = hexString[i];
				if (IsHexDigit(c))
					numHexChars++;
			}
			// if odd number of characters, discard last character
			if (numHexChars % 2 != 0)
			{
				numHexChars--;
			}
			return numHexChars / 2; // 2 characters per byte
		}
		/// <summary>
		/// Creates a byte array from the hexadecimal string. Each two characters are combined
		/// to create one byte. First two hexadecimal characters become first byte in returned array.
		/// Non-hexadecimal characters are ignored. 
		/// </summary>
		/// <param name="hexString">string to convert to byte array</param>
		/// <param name="discarded">number of characters in string ignored</param>
		/// <returns>byte array, in the same left-to-right order as the hexString</returns>
		public static byte[] GetBytes(string hexString, out int discarded)
		{
			discarded = 0;
			string newString = "";
			char c;
			// remove all none A-F, 0-9, characters
			for (int i=0; i<hexString.Length; i++)
			{
				c = hexString[i];
				if (IsHexDigit(c))
					newString += c;
				else
					discarded++;
			}
			// if odd number of characters, discard last character
			if (newString.Length % 2 != 0)
			{
				discarded++;
				newString = newString.Substring(0, newString.Length-1);
			}

			int byteLength = newString.Length / 2;
			byte[] bytes = new byte[byteLength];
			string hex;
			int j = 0;
			for (int i=0; i<bytes.Length; i++)
			{
				hex = new String(new Char[] {newString[j], newString[j+1]});
				bytes[i] = HexToByte(hex);
				j = j+2;
			}
			return bytes;
		}

		//		public static string ToString(byte[] bytes)
		//		{
		//			string hexString = "";
		//			for (int i=0; i<bytes.Length; i++)
		//			{
		//				hexString += bytes[i].ToString("X2");
		//			}
		//			return hexString;
		//		}

		
		public static string ToString(byte[] bytes)
		{
			string hexString = "";
			for (int i=0; i<bytes.Length; i++)
			{
				hexString += " " + i.ToString() + ":" + bytes[i].ToString("X2");
			}
			return hexString;
		}
		/// <summary>
		/// Determines if given string is in proper hexadecimal string format
		/// </summary>
		/// <param name="hexString"></param>
		/// <returns></returns>
		public static bool InHexFormat(string hexString)
		{
			bool hexFormat = true;

			foreach (char digit in hexString)
			{
				if (!IsHexDigit(digit))
				{
					hexFormat = false;
					break;
				}
			}
			return hexFormat;
		}

		/// <summary>
		/// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
		/// </summary>
		/// <param name="c">Character to test</param>
		/// <returns>true if hex digit, false if not</returns>
		public static bool IsHexDigit(Char c)
		{
			int numChar;
			int numA = Convert.ToInt32('A');
			int num1 = Convert.ToInt32('0');
			c = Char.ToUpper(c);
			numChar = Convert.ToInt32(c);
			if (numChar >= numA && numChar < (numA + 6))
				return true;
			if (numChar >= num1 && numChar < (num1 + 10))
				return true;
			return false;
		}
		/// <summary>
		/// Converts 1 or 2 character string into equivalant byte value
		/// </summary>
		/// <param name="hex">1 or 2 character string</param>
		/// <returns>byte</returns>
		private static byte HexToByte(string hex)
		{
			if (hex.Length > 2 || hex.Length <= 0)
				throw new ArgumentException("hex must be 1 or 2 characters in length");
			byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
			return newByte;
		}
	}

	class MyIcmpPacketManipulator
	{
		static UInt16 m_NextSequence = 0;
		
		public const int PING_ICMP_HEADER_SIZE = 8;
		public const int PING_ICMP_DATA_SIZE = 32;
		public const int PING_ICMP_PACKET_SIZE = PING_ICMP_HEADER_SIZE + PING_ICMP_DATA_SIZE;
		public const int PING_ICMP_ECHO_REQUEST = 8;
		public const int PING_ICMP_ECHO_REPLY = 0;
			
		private Byte m_Type = PING_ICMP_ECHO_REQUEST;
		private Byte m_Code = 0;
		private UInt16 m_CheckSum;
		private UInt16 m_Identifier;
		private UInt16 m_Sequence;
		private byte[] m_Data;

		//private byte[] m_Packet;
			
		public MyIcmpPacketManipulator(UInt16 aIdentifier)
		{
			//m_Packet = new byte[PING_ICMP_PACKET_SIZE];
			this.m_Type = PING_ICMP_ECHO_REQUEST;
			this.m_Code = 0;
			this.m_Identifier = aIdentifier;
			this.m_Sequence = m_NextSequence;
			m_NextSequence ++;
			this.m_CheckSum = 0;
			m_Data = new byte[PING_ICMP_DATA_SIZE];
			for( int i = 0; i < PING_ICMP_DATA_SIZE; i++) m_Data[i] = (byte)'!';
		}

		public byte[] BuildPacket()
		{
			return BuildPacket( 0xffff);
		}
		
		public byte[] BuildPacket( UInt16 aSequence)
		{
			byte[] result = new byte[PING_ICMP_PACKET_SIZE];
			byte[] temp;
				
			if (aSequence == 0xffff) 
			{
				this.m_Sequence = m_NextSequence;
				m_NextSequence++;
			} 
			else this.m_Sequence = aSequence;
			
			//Form the ICMP Echo-Request packet header
			result[0] = this.m_Type;
			result[1] = this.m_Code;
			// Set checksum field to ZERO to calculate it
			result[2] = 0x00;
			result[3] = 0x00;
			temp = BitConverter.GetBytes( this.m_Identifier);
			result[5] = temp[0];
			result[4] = temp[1];
			temp = BitConverter.GetBytes( this.m_Sequence);
			result[7] = temp[0];
			result[6] = temp[1];
				
			// Append the ICMP packet paload after the header
			Array.Copy( m_Data, 0, result, PING_ICMP_HEADER_SIZE, PING_ICMP_DATA_SIZE);

			int sum = 0;

			for ( int i = 0; i < PING_ICMP_PACKET_SIZE ; i += 2) 
				sum += Convert.ToInt32( BitConverter.ToUInt16( result, i));

			sum = (sum >> 16) + (sum & 0xffff);
			sum += (sum >> 16);
			this.m_CheckSum = (UInt16)(~sum);

			// Finalise froging the packet haeader by giving the CHECKSUM field its value
			temp = BitConverter.GetBytes( this.m_CheckSum);
			result[2] = temp[0];
			result[3] = temp[1];

			return result;
		}

		public static Byte ExtractType( Byte[] aIcmpPacket, int aOffset)
		{
			return (aIcmpPacket[aOffset + 0]);
		}

		public static UInt16 ExtractIdentifier( Byte[] aIcmpPacket, int aOffset)
		{
			//return (BitConverter.ToUInt16( aIcmpPacket, aOffset + 4));
			return (UInt16)((((UInt16)(aIcmpPacket[aOffset + 4])) << 8) | ((UInt16)(aIcmpPacket[aOffset + 5])));
		}

		public static UInt16 ExtractSequence( Byte[] aIcmpPacket, int aOffset)
		{
			//return (BitConverter.ToUInt16( aIcmpPacket, aOffset + 6));
			return (UInt16)((((UInt16)(aIcmpPacket[aOffset + 6])) << 8) | ((UInt16)(aIcmpPacket[aOffset + 7])));
		}

		public Byte Type
		{
			get 
			{
				return m_Type;
			}

			set 
			{
				m_Type = value;
			}
		}
		
		public Byte Code
		{
			get 
			{
				return m_Code;
			}

			set 
			{
				m_Code = value;
			}
		}

		public UInt16 CheckSum
		{
			get 
			{
				return m_CheckSum;
			}

			set 
			{
				m_CheckSum = value;
			}
		}

		public UInt16 Identifier
		{
			get 
			{
				return m_Identifier;
			}

			set 
			{
				m_Identifier = value;
			}
		}

		public UInt16 Sequence
		{
			get 
			{
				return m_Sequence;
			}

			set 
			{
				m_Sequence = value;
			}
		}

		public byte[] Data
		{
			get 
			{
				return m_Data;
			}

			set 
			{
				m_Data = value;
			}
		}
	}

	public class MyPingResult
	{
		private string m_Client;
		private string m_Server;
		private int m_TimeTick;
		private System.DateTime m_Time;
		private int m_Delay;
		private bool m_Timeout;
		private byte[] m_SentData;
		private byte[] m_ReceivedData;

		public MyPingResult()
		{
			m_Timeout = false;
		}

		public override string ToString()
		{ 
			string delay;

			if (m_Timeout) delay = "timeout";
			else delay = m_Delay.ToString();

			return "\"" + this.m_Time + "\"; \"" + this.m_TimeTick + "\"; \"" + delay + "\"; \"" + this.m_Client + "\"; \"" + this.m_Server + "\"";
		}

		public int TimeTick
		{
			set 
			{
				m_TimeTick = value;
			}

			get 
			{
				return m_TimeTick;
			}
		}

		public System.DateTime Time
		{
			set 
			{
				m_Time = value;
			}

			get 
			{
				return m_Time;
			}
		}

		public string Client 
		{
			set 
			{
				m_Client = value;
			}

			get 
			{
				return m_Client;
			}
		}

		public string Server
		{
			set 
			{
				m_Server = value;
			}

			get 
			{
				return m_Server;
			}
		}

		public int Delay
		{
			set 
			{
				m_Delay = value;
			}

			get 
			{
				return m_Delay;
			}
		}

		public bool Timeout
		{
			set 
			{
				m_Timeout = value;
			}

			get 
			{
				return m_Timeout;
			}
		}

		public byte[] sentData
		{
			set
			{
				m_SentData = value;
			}

			get 
			{
				return m_SentData;
			}
		}

		public byte[] receivedData
		{
			set 
			{
				m_ReceivedData = value;
			}

			get 
			{
				return m_ReceivedData;
			}
		}
	}

	public delegate void OnMyPingEvent( object aSender, MyPingResult aEventArgs);

	public class MyPinger
	{
		private System.Net.IPEndPoint m_Client;
		private System.Net.IPEndPoint m_Server;
		
		private int m_Timeout;
		private int m_RepeatInterval;
		private bool m_Cancel;

		public OnMyPingEvent OnPingStart;
		public OnMyPingEvent OnPingResponse;
		public OnMyPingEvent OnPingTimeout;

		public MyPinger()
		{
			m_Client = null;
			m_Server = null;

			m_Timeout = 3000;
			m_RepeatInterval = 5000;

			m_Cancel = false;

			this.ClientIP = "127.0.0.1";
			this.ServerIP = "127.0.0.1";
		}

		public string ClientIP
		{
			set 
			{
				m_Client = new System.Net.IPEndPoint( IPAddress.Parse(value),0);
			}

			get 
			{
				if (m_Client != null) return m_Client.Address.ToString();
				else return "0.0.0.0";
			}
		}

		public string ServerIP
		{
			set 
			{
				m_Server = new System.Net.IPEndPoint( IPAddress.Parse(value),0);
			}

			get 
			{
				if (m_Server != null) return (m_Server.Address.ToString());
				else return "0.0.0.0";
			}
		}

		public int RepeatInterval
		{
			set 
			{
				if (value > 100) m_RepeatInterval = value;
				else m_RepeatInterval = 100;

				if (m_Timeout >= m_RepeatInterval) m_Timeout = m_RepeatInterval - 1;
			}

			get 
			{
				return m_RepeatInterval;
			}
		}

		public int Timeout
		{
			set 
			{
				m_Timeout = value;
				if (m_Timeout >= m_RepeatInterval) m_RepeatInterval = m_Timeout + 1;
			}

			get 
			{
				return m_Timeout;
			}
		}

		public void Cancel()
		{
			lock(this) m_Cancel = true;
		}

		public void PingLoop( int aCount)
		{
			lock(this) m_Cancel = false;
			//IcmpPacket icmpPacket = new IcmpPacket();
			MyIcmpPacketManipulator icmpPacketBuilder = new MyIcmpPacketManipulator( 10);

			Socket socket = new Socket( System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Raw, System.Net.Sockets.ProtocolType.Icmp);
			socket.SetSocketOption( System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.SendTimeout, m_Timeout);
			//socket.SetSocketOption( System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReceiveTimeout, m_Timeout);
			socket.SetSocketOption( System.Net.Sockets.SocketOptionLevel.IP, System.Net.Sockets.SocketOptionName.IpTimeToLive, 128);

			//byte[] sendBuffer = icmpPacket.ToByteArray();

			for (int i = 0;;)
			{
				int startTick = 0;
				int stopTick = 0;
				int latency;

				MyPingResult result = new MyPingResult();

				result.Server = this.ServerIP;
				result.Client = this.ClientIP;

				System.Net.EndPoint endPoint = m_Client;

				try 
				{
					byte[] receiveBuffer = new Byte[256];

					byte[] sendBuffer = icmpPacketBuilder.BuildPacket();
					result.sentData = sendBuffer;

					if (OnPingStart != null) OnPingStart( this, result);
					socket.SetSocketOption( System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReceiveTimeout, m_Timeout);
					
					result.Time = System.DateTime.Now;
					
					startTick = Environment.TickCount;
					int timeoutAfter = startTick + m_Timeout;
					socket.SendTo( sendBuffer, MyIcmpPacketManipulator.PING_ICMP_PACKET_SIZE, System.Net.Sockets.SocketFlags.None, m_Server);
					for(;;)
					{
						int byteCount = socket.ReceiveFrom( receiveBuffer, 256, System.Net.Sockets.SocketFlags.None, ref endPoint);
						stopTick = Environment.TickCount;
						int remainedTimout = (timeoutAfter - stopTick);
						
						if ( MyIcmpPacketManipulator.ExtractType( receiveBuffer, 20) == MyIcmpPacketManipulator.PING_ICMP_ECHO_REPLY)
						{
							UInt16 identifier = MyIcmpPacketManipulator.ExtractIdentifier( receiveBuffer, 20);
							UInt16 sequence = MyIcmpPacketManipulator.ExtractSequence( receiveBuffer, 20);

							if (( icmpPacketBuilder.Identifier == identifier) && (icmpPacketBuilder.Sequence == sequence))
								break;
						}

						if (remainedTimout > 0)
							socket.SetSocketOption( System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReceiveTimeout, remainedTimout);
					}

					result.receivedData = receiveBuffer;
			
					latency = (stopTick - startTick);
			
					result.TimeTick = startTick;
					result.Delay = latency;
					result.Timeout = false;

					if (OnPingResponse != null) OnPingResponse( this, result);
				} 
				catch (Exception ex)
				{
					stopTick = Environment.TickCount;

					latency = (stopTick - startTick);

					result.TimeTick = startTick;
					result.Delay = latency;
					result.Timeout = true;

					if (OnPingTimeout != null) OnPingTimeout( this, result);
				}

				i++;

				bool doCancel;

				lock(this) doCancel = m_Cancel;
		
				if (((aCount == 0) || (i < aCount)) && (!doCancel))
				{
					if (latency < m_RepeatInterval) Thread.Sleep( m_RepeatInterval - latency);
				} 
				else break;
			}

			socket.Close();
		}

		public void PingEndlessLoop()
		{
			PingLoop(0);
		}
	}

	
	public class MyLoggingPingerEvent
	{
		public const int NONE = 0;
		public const int LINK_DWON = 1;
		public const int LINK_RESTORED = 2;
		public const int LINK_UNSTABLE = 3;
		
		private System.DateTime m_Time;
		private int m_Code;
		private string m_Description;

		public MyLoggingPingerEvent()
		{
		}

		public override string ToString()
		{ 
			return "\"" + this.m_Time + "\"; \"" + this.m_Code + "\"; \"" + this.m_Description + "\"";
		}

		public System.DateTime Time
		{
			set 
			{
				this.m_Time = value;
			}

			get 
			{
				return (this.m_Time);
			}
		}

		public int Code
		{
			set 
			{
				this.m_Code = value;
			}

			get 
			{
				return (this.m_Code);
			}
		}

		public string Description
		{
			set 
			{
				this.m_Description = value;
			}

			get 
			{
				return (this.m_Description);
			}
		}
	}
	
	public delegate void OnMyPingLoggerEvent( object aSender);
	
	public class MyLoggingPinger
	{
		public OnMyPingLoggerEvent OnPingLoopStart;
		public OnMyPingLoggerEvent OnPingLoopEnd;

		private MyPinger m_Pinger;
		private Thread m_PingerThread;
		private ArrayList m_PingLog;
		private ArrayList m_EventLog;
		private string m_PingLogFilePath;
		private string m_EventsFilePath;
		//private string m_EventLogFilePath;
		private int m_LogFlushInterval;
		private int m_NextLogFlushTimetick;
		private string m_Server;

		private int m_Timeouts;
		private bool m_TimeoutEventCreated;
		private int m_TimeoutsForEvent;

		private int m_Responses;
		private bool m_ResponseEventCreated;
		private int m_ResponsesForEvent;

		public MyLoggingPinger( string aClient, string aServer, string aLogFilePath, string aEventsFilePath)
		{
			m_Server = aServer;
			m_PingLog = new ArrayList();
			m_EventLog = new ArrayList();
			m_Pinger = new MyPinger();
			m_Pinger.ClientIP = aClient;
			m_Pinger.ServerIP = aServer;
			m_PingLogFilePath = aLogFilePath;
			m_EventsFilePath = aEventsFilePath;
			m_LogFlushInterval = 30000;
			m_NextLogFlushTimetick = Environment.TickCount + m_LogFlushInterval;
			m_Pinger.OnPingResponse = new OnMyPingEvent( this.OnPingResponse);
			m_Pinger.OnPingStart = new OnMyPingEvent( this.OnPingStart);
			m_Pinger.OnPingTimeout = new OnMyPingEvent( this.OnPingTimeout);

			OnPingLoopStart = null;
			OnPingLoopEnd = null;
		
			this.m_TimeoutEventCreated = true;
			this.m_Timeouts = 0;
			this.m_TimeoutsForEvent = 10;

			this.m_ResponseEventCreated = true;
			this.m_Responses = 0;
			this.m_ResponsesForEvent = 10;

			this.m_PingerThread = new Thread( new ThreadStart(this.ExecutePingerLoop));
		}

		public void Run()
		{
			m_PingerThread.Start();
		}

		public void StopRequest()
		{
			m_Pinger.Cancel();
		}

		public void Stop()
		{
			m_Pinger.Cancel();
			m_PingerThread.Join( m_Pinger.Timeout);
			FlushLog();
		}

		public int LogFlushInterval 
		{
			set 
			{
				if (value > 10000) m_LogFlushInterval = value;
				else m_LogFlushInterval = 10000;
			}

			get 
			{
				return m_LogFlushInterval;
			}
		}

		public MyPinger Pinger
		{
			get 
			{
				return m_Pinger;
			}
		}

		public void OnPingStart( object aSender, MyPingResult aEventArgs)
		{
		}

		public void OnPingResponse( object aSender, MyPingResult aEventArgs)
		{
			lock(this) 
			{
				m_PingLog.Add( aEventArgs);

				m_Timeouts = 0;

				if (m_Responses < m_ResponsesForEvent) m_Responses++;
				else
				{
					// Generate an event for connectrion restore
					if (this.m_ResponseEventCreated == false)
					{
						this.m_ResponseEventCreated = true;
						MyLoggingPingerEvent loggingPingerEvent = new MyLoggingPingerEvent();
						loggingPingerEvent.Code = MyLoggingPingerEvent.LINK_RESTORED;
						loggingPingerEvent.Time = System.DateTime.Now;
						loggingPingerEvent.Description = "Connectivity with " + this.Server.ToString() + " was restored";
						this.m_EventLog.Add( loggingPingerEvent);
					}

					this.m_TimeoutEventCreated = false;
				} 
			}

			FlushPeriodic();
		}

		public void OnPingTimeout( object aSender, MyPingResult aEventArgs)
		{
			lock(this) 
			{			
				m_PingLog.Add( aEventArgs);

				m_Responses = 0;

				if (m_Timeouts < m_TimeoutsForEvent) m_Timeouts++;
				else 
				{
					// Generate an event for connectrion down
					if (this.m_TimeoutEventCreated == false)
					{
						this.m_TimeoutEventCreated = true;
						MyLoggingPingerEvent loggingPingerEvent = new MyLoggingPingerEvent();
						loggingPingerEvent.Code = MyLoggingPingerEvent.LINK_DWON;
						loggingPingerEvent.Time = System.DateTime.Now;
						loggingPingerEvent.Description = "Connectivity with " + this.Server.ToString() + " came down on";
						this.m_EventLog.Add( loggingPingerEvent);
					}

					this.m_ResponseEventCreated = false;
				} 
				
			}
			
			FlushPeriodic();
		}

		private void FlushPeriodic()
		{
			int nextLogFlushTimetick;

			lock(this) nextLogFlushTimetick = m_NextLogFlushTimetick;

			if ( nextLogFlushTimetick < Environment.TickCount) 
			{
				m_NextLogFlushTimetick = Environment.TickCount + m_LogFlushInterval;
				FlushLog();
			}
		}

		public void FlushLog()
		{
			bool doFlushLog = false;
			bool doFlushEvents = false;

			lock(this) 
			{
				doFlushLog = (this.m_PingLog.Count > 0);
				doFlushEvents = (this.m_EventLog.Count > 0);
			}

			if (doFlushLog || doFlushEvents)
			{
				DateTime now = System.DateTime.Now;
				
				string day = now.Day.ToString();
				string month = now.Month.ToString();
				string year = now.Year.ToString();

				while (day.Length < 2) day = "0" + day;
				while (month.Length < 2) month = "0" + month;
				while (year.Length < 4) year = "0" + year;

				string date = year + "-" + month + "-" + day;

				if ( doFlushLog )
				{
					ArrayList oldLog;

					lock(this)
					{
						oldLog = this.m_PingLog;
						this.m_PingLog = new ArrayList();
					}

					string fp = this.m_PingLogFilePath.Replace("{HOST}", this.Server.ToString());
					fp = fp.Replace("{DATE}", date);
					fp = fp.Replace("{DAY}", day);
					fp = fp.Replace("{MONTH}", month);
					fp = fp.Replace("{YEAR}", year);

					TextWriter writer = File.AppendText( fp);

					foreach ( object item in oldLog)
					{
						writer.WriteLine(item);
					}

					oldLog.Clear();

					writer.Close();
				}

				if ( doFlushEvents )
				{
					ArrayList oldEvents;

					lock(this)
					{
						oldEvents = this.m_EventLog;
						this.m_EventLog = new ArrayList();

					}

					string fp = this.m_EventsFilePath.Replace("{HOST}", this.Server.ToString());
					fp = fp.Replace("{DATE}", date);
					fp = fp.Replace("{DAY}", day);
					fp = fp.Replace("{MONTH}", month);
					fp = fp.Replace("{YEAR}", year);

					TextWriter writer = File.AppendText( fp);

					foreach ( object item in oldEvents)
					{
						writer.WriteLine(item);
					}

					oldEvents.Clear();

					writer.Close();
				}
			}
		}

		private void ExecutePingerLoop()
		{
			if (this.OnPingLoopStart != null)
				lock(this) this.OnPingLoopStart(this);
			
			this.m_Pinger.PingEndlessLoop();

			if (this.OnPingLoopEnd != null)
				lock(this) this.OnPingLoopEnd(this);
		}

		public string Server 
		{
			get 
			{
				return this.m_Server;
			}
		}
	}

	public class MyPingersBundle
	{
		private ArrayList m_PingerLoggers;
		
		public MyPingersBundle()
		{
			m_PingerLoggers = new ArrayList();
		}

		public MyLoggingPinger InvokePingerLoggerIdle( string aClient, string aServer, string aLogFilePath, string aEventsFilePath)
		{
			MyLoggingPinger pingLogger = new MyLoggingPinger( aClient, aServer, aLogFilePath, aEventsFilePath);

			pingLogger.OnPingLoopStart = new OnMyPingLoggerEvent( this.OnPingLoopStart);
			pingLogger.OnPingLoopEnd = new OnMyPingLoggerEvent( this.OnPingLoopEnd);

			m_PingerLoggers.Add( pingLogger);

			return pingLogger;
		}

		public MyLoggingPinger InvokePingerLogger( string aClient, string aServer, string aLogFilePath, string aEventsFilePath)
		{
			MyLoggingPinger pingLogger = InvokePingerLoggerIdle( aClient, aServer, aLogFilePath, aEventsFilePath);
			pingLogger.Run();
			return pingLogger;
		}



		private void OnPingLoopStart( object aSender)
		{
		}

		private void OnPingLoopEnd( object aSender)
		{
			if ( aSender != null)
				lock(m_PingerLoggers) m_PingerLoggers.Remove( aSender);
		}

		public void StopAllPingLoggers()
		{
			lock(this.m_PingerLoggers)
			{
				foreach( MyLoggingPinger pingLogger in this.m_PingerLoggers)
					pingLogger.StopRequest();
				this.m_PingerLoggers.Clear();
			}
			
		}

		public void ReadCFGFile( string aFilePath)
		{
			//lb.Items.Clear();
			
			XmlTextReader xr = new XmlTextReader( aFilePath);
			while (xr.Read())
			{
				if (xr.NodeType == XmlNodeType.Element)
				{
					switch (xr.Name)
					{
						case "PingedHost":
							string ip = xr.GetAttribute("IP");
							string logfile = xr.GetAttribute("LogFile");
							string eventsfile = xr.GetAttribute("EventsFile");
							string interval = xr.GetAttribute("Interval");
							string timeout = xr.GetAttribute("Timeout");

							if ((ip != null) && (logfile != null))
							{
								MyLoggingPinger pl = this.InvokePingerLoggerIdle( "127.0.0.1", ip, logfile, eventsfile);

								if ( interval != null) 
									pl.Pinger.RepeatInterval = System.Convert.ToInt32( interval);

								if ( timeout != null)
									pl.Pinger.Timeout = System.Convert.ToInt32( timeout);

								pl.Run();

								//lb.Items.Add( "HOST = " + ip + " - LOGFILE = " + logfile);
							}
							break;
					}
				}
			}

			xr.Close();
		}


	}
}
