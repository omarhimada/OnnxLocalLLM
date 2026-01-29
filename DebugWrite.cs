using System.Diagnostics;
using static UI.Constants;

namespace UI {
	internal static class DebugWrite {
		private const string _distinguishingLine =
			@"°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°";

		internal static void Line(string category, Exception exception) =>
			Debug.WriteLine($"{_distinguishingLine}{_twoNewLinesVerbatimNoReturn}{exception.Message}{_twoNewLinesVerbatimNoReturn}{exception.StackTrace}{_distinguishingLine}");
	}
}