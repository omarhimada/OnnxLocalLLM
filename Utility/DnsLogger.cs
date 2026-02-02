using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UI.Utility {

	/// <summary>
	/// Provides functionality to listen for and log incoming DNS requests over TCP.
	/// </summary>
	/// <remarks>DnsLogger is intended for diagnostic or monitoring scenarios where capturing DNS request data is
	/// required. The class operates as a TCP-based DNS listener and logs received requests. It is designed for use in
	/// environments where monitoring DNS traffic is necessary, such as debugging or auditing network activity. This class
	/// is not intended for use as a production DNS server.</remarks>
	public class DnsLogger {
		private const int DNS_PORT = 53;
		private static readonly Lock _lockObject = new();
		private static TcpListener? _listener;
		private static bool _isRunning;

		public static void StartListening(int port) {
			if (_isRunning)
				throw new InvalidOperationException("DNS listener is already running.");

			_isRunning = true;
			_listener = new TcpListener(IPAddress.Any, port);
			_listener.Start();

			Console.WriteLine("DNS Listener started on port " + port);

			while (_isRunning) {
				TcpClient client = _listener.AcceptTcpClient();
				HandleDnsRequest(client);
			}
		}

		private static void HandleDnsRequest(TcpClient client) {
			NetworkStream stream = client.GetStream();

			lock (_lockObject) {
				byte[] buffer = new byte[4096];
				int bytesRead = stream.Read(buffer, 0, buffer.Length);

				if (bytesRead > 0) {
					string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
					LogDnsRequest(request);
				}
			}

			stream.Close();
		}

		private static void LogDnsRequest(string request) {
			// Implement your logging mechanism here, e.g., writing to a file or sending to a remote service.
			Console.WriteLine("Received DNS request:\n" + request);
		}
	}
}
