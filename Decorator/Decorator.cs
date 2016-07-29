using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Authentication;

namespace A 
{
	public class ShowMyDecorator
	{
		public static void RunSnippet()
		{
			var ps = new ProtectedStream(new MemoryStream(new byte [] {(byte)'a', (byte)'b', (byte)'c' }), "123");
			
			Console.WriteLine("IsAuthenticated = " + ps.IsAuthenticated);
			ps.Authenticate("123");
			Console.WriteLine("IsAuthenticated = " + ps.IsAuthenticated);
			
			var buffer = new byte[3];
			for(int i = 0; i < ps.Read(buffer, 0, 3); i++)
			{
				Console.Write("#" + i + "=" + buffer[i]);
			}
		}
		
		#region Helper methods
		
		public static void Main()
		{
			try
			{
				RunSnippet();
			}
			catch (Exception e)
			{
				string error = string.Format("---\nThe following error occurred while executing the snippet:\n{0}\n---", e.ToString());
				Console.WriteLine(error);
			}
			finally
			{
				Console.Write("Press any key to continue...");
				Console.ReadKey();
			}
		}
	
		private static void WL(object text, params object[] args)
		{
			Console.WriteLine(text.ToString(), args);	
		}
		
		private static void RL()
		{
			Console.ReadLine();	
		}
		
		private static void Break() 
		{
			System.Diagnostics.Debugger.Break();
		}
	
		#endregion
	}
	
	sealed class ProtectedStream : Stream
	{
		Stream target;
		bool isAuthenticated;
		string password;
		
		public ProtectedStream(Stream target, string password)
		{
			if(target == null)
			{
				throw new ArgumentNullException("Target is null");
			}
			
			if(password == null)
			{
				throw new ArgumentNullException("Password is not assigned");
			}
			
			this.target = target;
			this.password = password;
			this.isAuthenticated = false;
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			if(!this.IsAuthenticated)
			{
				throw new AuthenticationException("No permissions");
			}
			
			return target.Read(buffer, offset, count);
		}
		
		public override void Flush()
		{
			target.Flush();
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			return target.Seek(offset, origin);
		}
		
		public override void SetLength(long value)
		{
			target.SetLength(value);
		}
		
		public override void Write(byte[] buffer, int offset, int count)
		{
			target.Write(buffer, offset, count);
		}
		
		public void Authenticate(string password)
		{
			if(!this.password.Equals(password))
			{
				throw new ArgumentException("Wrong password");
			}
			
			this.isAuthenticated = true;
		}	
		
		public override bool CanRead { get { return target.CanRead; } }
		public override bool CanWrite { get { return target.CanWrite; } }
		public override bool CanSeek { get { return target.CanSeek; } }
		public override long Length { get { return target.Length; } }
		
		public override long Position 
		{ 
			get { return target.Position; } 
			set { target.Position = value; } 
		}
		
		public bool IsAuthenticated { get { return this.isAuthenticated; } }
	}
}